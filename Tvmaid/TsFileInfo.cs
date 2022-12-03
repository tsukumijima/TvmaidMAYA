using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Diagnostics;

namespace Tvmaid
{
    //TSファイルの情報を取得
    class TsFileInfo
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public DateTime ReserveStart;
        public DateTime ReserveEnd;

        public string Title = "";
        public string Service = "";
        public string Desc = "";
        public string LongDesc = "";
        public string Genre = "";
        public string File;

        public TsFileInfo(string path)
        {
            File = Path.GetFileName(path);
            Title = Path.GetFileNameWithoutExtension(path);

            using (var fs = new FileStream(path, FileMode.Open))
            {
                if (fs.Length < 188 * 10)
                    throw new Exception("ファイルサイズが小さすぎます。");

                StartTime = GetTsTime(fs, false);

                fs.Seek(-188, SeekOrigin.End);
                EndTime = GetTsTime(fs, true);
            }

            if (EndTime - StartTime > TimeSpan.FromHours(24))
                throw new Exception("不正な時刻のファイルです。");

            ReserveStart = StartTime;
            ReserveEnd = EndTime;

            var csv = GetTsInfo(path);

            if (csv != null)
                ParseCsv(csv);
        }

        string GetTsInfo(string path)
        {
            var p = new Process();
            p.StartInfo.FileName = Util.GetBasePath("rplsinfo.exe");
            p.StartInfo.Arguments = string.Format("\"{0}\" -C -dtpcbieg -l 10", path);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            var data = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return p.ExitCode == 0 ? data : null;
        }

        void ParseCsv(string csv)
        {
            var parser = new TextFieldParser(new StringReader(csv));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TrimWhiteSpace = false;
            var row = parser.ReadFields();

            var start = row[0] + " " + row[1];
            var duration = row[2];

            DateTime time;

            if (DateTime.TryParse(start, out time))
                ReserveStart = time;

            TimeSpan span;
            if (TimeSpan.TryParse(duration, out span))
                ReserveEnd = ReserveStart.Add(span);

            var conv = AppDefine.TextConverter;

            Service = row[3];
            Title = conv.Convert(row[4]);
            Desc = conv.Convert(row[5]);
            LongDesc = conv.Convert(row[6]);
            Genre = row[7].Replace(" 〔", "/").Replace("〕　", "\r\n").Replace("〕", "\r\n");
        }

        DateTime GetTsTime(FileStream fs, bool back)
        {
            var packet = new byte[188];
            long start = 0; //PCR最初
            long end = 0;   //PCR最後

            while (true)
            {
                var count = fs.Read(packet, 0, packet.Length);

                if (count != packet.Length)
                    throw new Exception("時刻の情報が見つかりません。");

                if (packet[0] != 0x47)
                    throw new Exception("不正なデータがあり、時間を取得できませんでした。");

                //PCR
                //アダプテーションあり+PCRあり 
                if ((packet[3] & 0x20) > 0 && (packet[5] & 0x10) > 0)
                {
                    //33bitのPCRを取得
                    long pcr = ((long)packet[6] << 25) + ((long)packet[7] << 17) + ((long)packet[8] << 9) + ((long)packet[9] << 1) + ((packet[10] >> 7) & 0x1);

                    if (start == 0 && end == 0)
                        start = pcr;

                    end = pcr;
                }

                //TOT
                var pid = ((packet[1] << 8) + packet[2]) & 0x1fff;

                if (pid == 0x14)
                {
                    var diff = (start - end) / 90000;           //90kHzで割って秒単位にする
                    var time = GetMjdTime(packet, 4 + 1 + 3);   //4+1+3「TSヘッダ+ペイロード区切り+ペイロードの時刻情報」までのバイト数

                    if (Math.Abs(diff) < 11)
                        return time.AddSeconds(diff);   //TOTは5～10秒ごとなので、PCRで補正する
                    else
                        return time;    //PCRが11秒以上なら無視(不正と判断)してTOTの時刻を返す
                }

                if (back)
                    fs.Seek(-188 * 2, SeekOrigin.Current);
            }
        }

        DateTime GetMjdTime(byte[] packet, int start)
        {
            var mjd = (packet[start + 0] << 8) | packet[start + 1];

            var y = (int)((mjd - 15078.2) / 365.25);
            var m = (int)((mjd - 14956.1 - (int)(y * 365.25)) / 30.6001);
            var k = (14 == m || m == 15) ? 1 : 0;

            var day = mjd - 14956 - (int)(y * 365.25) - (int)(m * 30.6001);
            var year = 1900 + y + k;
            var month = m - 1 - k * 12;

            var hour = ((packet[start + 2] & 0xf0) >> 4) * 10 + (packet[start + 2] & 0x0f);
            var minute = ((packet[start + 3] & 0xf0) >> 4) * 10 + (packet[start + 3] & 0x0f);
            var second = ((packet[start + 4] & 0xf0) >> 4) * 10 + (packet[start + 4] & 0x0f);

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
