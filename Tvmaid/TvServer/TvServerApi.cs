using System;
using System.Collections.Generic;

namespace Tvmaid
{
    abstract class TvServerApi : TvServerBase
    {
        public enum State
        {
            View,       //視聴
            Recoding,   //録画中
            RecPaused,  //録画一時停止中
            Stoped,     //停止
            Unknown     //不明
        }

        public TvServerApi(Tuner tuner) : base(tuner) { }

        public void Close()
        {
            Call(Api.Close);
        }

        public bool IsEpgComplete(int nid, int tsid)
        {
            var arg = "{0}\x1{1}\x0".Formatex(nid, tsid);
            return Call(Api.IsEpgComplete, arg) == "1";
        }

        public int GetState()
        {
            return Call(Api.GetState).ToInt();
        }

        //サービスリストを取得
        public List<Service> GetServices()
        {
            var ret = Call(Api.GetServices);

            var lines = ret.Split(new char[] { '\x1' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<Service>();

            foreach (var line in lines)
            {
                try
                {
                    //データの一部が、""になっている場合があるので、
                    //StringSplitOptions.RemoveEmptyEntriesをつけてはいけない
                    var data = line.Split(new char[] { '\x2' });

                    var s = new Service();
                    s.Driver = System.IO.Path.GetFileName(tuner.DriverPath);
                    s.Nid = Convert.ToInt32(data[0]);
                    s.Tsid = Convert.ToInt32(data[1]);
                    s.Sid = Convert.ToInt32(data[2]);
                    s.Name = data[3];
                    list.Add(s);
                }
                catch (Exception ex)
                {
                    throw new Exception("サービス情報が不正です。[追加情報] " + ex.Message);
                }

            }
            return list;
        }

        //サービス切り替え
        public void SetService(Service service)
        {
            var arg = string.Format("{0}\x1{1}\x1{2}\x0", service.Nid, service.Tsid, service.Sid);
            Call(Api.SetService, arg);
        }

        //指定イベントの時間を取得
        //録画時の追従用
        //EventのStartとDurationのみセットする
        public Event GetEventTime(Service service, int eid)
        {
            var arg = "{0}\x1{1}\x1{2}\x1{3}\x0".Formatex(service.Nid, service.Tsid, service.Sid, eid);
            var ret = Call(Api.GetEventTime, arg);

            try
            {
                var data = ret.Split(new char[] { '\x1' }, StringSplitOptions.RemoveEmptyEntries);
                var ev = new Event();
                ev.Start = Convert.ToDateTime(data[0]);
                ev.Duration = Convert.ToInt32(data[1]);

                return ev;
            }
            catch (Exception ex)
            {
                throw new Exception("番組情報が不正です。[追加情報] " + ex.Message);
            }
        }

        //TSの状態を取得
        public TsStatus GetTsStatus()
        {
            var ret = Call(Api.GetTsStatus);

            try
            {
                var data = ret.Split(new char[] { '\x1' }, StringSplitOptions.RemoveEmptyEntries);
                var error = Convert.ToUInt64(data[0]);
                var drop = Convert.ToUInt64(data[1]);
                var scramble = Convert.ToUInt64(data[2]);

                var ts = new TsStatus();
                ts.Error = error < int.MaxValue ? (int)error : int.MaxValue;
                ts.Drop = drop < int.MaxValue ? (int)drop : int.MaxValue;
                ts.Scramble = scramble < int.MaxValue ? (int)scramble : int.MaxValue;

                return ts;
            }
            catch (Exception ex)
            {
                throw new Exception("TSエラー情報が不正です。[追加情報] " + ex.Message);
            }
        }

        //番組表更新
        public List<Event> GetEvents(Service service)
        {
            var arg = string.Format("{0}\x1{1}\x1{2}\x0", service.Nid, service.Tsid, service.Sid);
            var ret = Call(Api.GetEvents, arg);
            var list = new List<Event>();

            var lines = ret.Split(new char[] { '\x1' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    //データの一部が、""になっている場合があるので、
                    //StringSplitOptions.RemoveEmptyEntriesをつけてはいけない
                    var data = line.Split(new char[] { '\x2' });
                    var ev = new Event();

                    ev.Eid = data[0].ToInt();
                    ev.Start = data[1].ToDateTime();
                    ev.Duration = data[2].ToInt();
                    ev.Title = data[3];
                    ev.Desc = data[4];
                    ev.LongDesc = data[5];
                    ev.Genre = data[6].ToLong();
                    ev.Pay = data[7] == "1";
                    ev.Fsid = service.Fsid;
                    list.Add(ev);
                }
                catch (Exception ex)
                {
                    throw new Exception("番組情報が不正です。[追加情報] " + ex.Message);
                }
            }
            return list;
        }

        public void StartRec(string file)
        {
            Call(Api.StartRec, string.Format("{0}\0", file));
        }

        public void GetLogo(Service service, string path)
        {
            var req = string.Format("{0}\x1{1}\x1{2}\x1{3}\x0", service.Nid, service.Tsid, service.Sid, path);
            Call(Api.GetLogo, req);
        }

        public void StopRec()
        {
            Call(Api.StopRec);
        }
    }
}
