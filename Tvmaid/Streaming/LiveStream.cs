using System.Collections.Generic;

namespace Tvmaid
{
    //TVTestからTSパケットを読み込んで、必要なパケットを取り出す
    class LiveStream : SharedMemory
    {
        List<int> epidList = new List<int>(); //残すエレメンタリPID(動画や音声のストリーム)のリスト
        int currentPmt = -1;    //現在のサービスのSIDに対応するPMTのPID
        long readPos;           //読み込み位置
        long fsid;

        public LiveStream(string name, long fsid) : base(name)
        {
            this.fsid = fsid;

            var length = view.ReadInt32(8);

            readPos = view.ReadInt64(0) - 30000;   //30000パケット戻ったところから開始
            if (readPos < 0) readPos = 0;
        }

        //バッファへパケットを読み込み
        //バッファへ書き込んだバイト数を返す
        public int Read(byte[] buf)
        {
            var wp = view.ReadInt64(0);
            var length = view.ReadInt32(8);

            var sid = (int)(fsid & 0xffff);

            int count = 0;

            //書き込み完了位置までか、バッファいっぱいまで読み込み
            while (wp > readPos && buf.Length > count * 188)
            {
                var packet = new byte[188];
                var pos = 16 + readPos % length * 188;

                view.ReadArray(pos, packet, 0, 188);

                if (fsid != 0)
                {
                    if (AnalPacket(packet, sid))
                    {
                        packet.CopyTo(buf, count * 188);
                        count++;
                    }
                }

                readPos++;
            }

            return count * 188;
        }

        //パケットの分析
        //PATの編集、EPIDリスト作成、残すパケットかどうかの判別を行う
        //戻り値 ture: 残す、false: 破棄
        bool AnalPacket(byte[] packet, int sid)
        {
            var pid = ((packet[1] << 8) + packet[2]) & 0x1fff;

            //PAT
            if (pid == 0)
            {
                //現在有効でなければ無視
                var isCurrent = (packet[10] & 0x1) > 0;

                if (isCurrent == false)
                    return true;

                var pmt = GetPmtPid(packet, sid);

                if (pmt == -1) return false; //該当のSIDがないので破棄する

                currentPmt = pmt;
                EditPat(packet, currentPmt, sid);
                return true;
            }
            //現在のサービスのPMT
            else if (pid == currentPmt)
            {
                GetEpidList(packet);
                return true;
            }
            else
                return epidList.IndexOf(pid) != -1;
        }

        void GetEpidList(byte[] packet)
        {
            //後続パケットなら無視
            var isStartPacket = (packet[1] & 0x40) > 0;

            if (isStartPacket == false)
                return;

            //現在有効でなければ無視
            var isCurrent = (packet[10] & 0x1) > 0;

            if (isCurrent == false)
                return;

            epidList.Clear();

            var length = ((packet[6] << 8) + packet[7]) & 0xfff;    //セクション長
            var end = 8 + length - 4;   //セクション長まで + セクション長 - CRC

            if (end > 188) end = 188;

            //PCRのPIDを追加
            var pcr = ((packet[13] << 8) + packet[14]) & 0x1fff;
            epidList.Add(pcr);

            //可変長の情報の長さ
            var infoLength = ((packet[15] << 8) + packet[16]) & 0xfff;
            var i = 17 + infoLength;    //ストリームのリストの先頭位置

            //packet[i + 4]の位置が範囲外になるまでループ
            while (i < end - 4)
            {
                if (packet[i] != 0xd)   //ストリーム形式0xdは無視する
                {
                    //エレメンタリPIDをリストに追加
                    int epid = ((packet[i + 1] << 8) + packet[i + 2]) & 0x1fff;
                    epidList.Add(epid);
                }

                infoLength = ((packet[i + 3] << 8) + packet[i + 4]) & 0xfff;
                i = i + 5 + infoLength; 
            }
        }

        //指定SIDのPMTを取得
        int GetPmtPid(byte[] packet, int sid)
        {
            var length = ((packet[6] << 8) + packet[7]) & 0xfff;    //セクション長
            var end = 8 + length - 4;   //セクション長まで + セクション長 - CRC

            if (end > 188) end = 188;

            for (var i = 17; i < end; i += 4)
            {
                int num = (packet[i] << 8) + packet[i + 1];

                if (num == sid)
                    return ((packet[i + 2] << 8) + packet[i + 3]) & 0x1fff;
            }

            return -1;  //指定SIDが見つからない
        }

        //PATパケット編集
        void EditPat(byte[] packet, int pmt, int sid)
        {
            //セクション長書き換え
            packet[7] = 0x11;

            //PMT情報書き換え
            for (var i = 25; i < 188; i++)
                packet[i] = 0xff;

            pmt = pmt | 0xe000; //111 を追加

            packet[17] = (byte)(sid >> 8);
            packet[18] = (byte)(sid & 0xff);
            packet[19] = (byte)(pmt >> 8);
            packet[20] = (byte)(pmt & 0xff);

            //CRC書き換え
            var hash = CRC32.Calc(packet, 5, 20);

            packet[21] = (byte)(hash >> 24);
            packet[22] = (byte)(hash >> 16 & 0xff);
            packet[23] = (byte)(hash >> 8 & 0xff);
            packet[24] = (byte)(hash & 0xff);
        }
    }

    static class CRC32
    {
        static ulong[] crcTable = null;

        static void CreateTable()
        {
            crcTable = new ulong[256];

            for (ulong i = 0; i < 256; i++)
            {
                ulong crc = i << 24;

                for (int j = 0; j < 8; j++)
                {
                    var x = (crc & 0x80000000) != 0 ? (ulong)0x04c11db7 : 0;
                    crc = (crc << 1) ^ x;
                }
                crcTable[i] = crc;
            }
        }

        static public ulong Calc(byte[] buf, int start, int end)
        {
            if (crcTable == null)
                CreateTable();

            ulong crc = 0xffffffff;

            for (int i = start; i <= end; i++)
                crc = (crc << 8) ^ crcTable[((crc >> 24) ^ buf[i]) & 0xff];

            return crc & 0xffffffff;
        }
    }
}
