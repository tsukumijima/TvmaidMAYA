using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Tvmaid
{
    //番組情報更新
    class EpgUpdater
    {
        TvServer server;
        Tuner tuner;
        Tvdb tvdb;

        static bool stop = false;
        static int instance = 0; //インスタンスのカウンタ。実行中かどうか調べるのに使用

        public static bool Running
        {
            get { return instance > 0 || RecTimer.Queue.Count > 0; }
        }

        public static void Stop()
        {
            stop = true;
        }

        public static void Resume()
        {
            stop = false;
        }

        public EpgUpdater(Tuner tuner)
        {
            this.tuner = tuner;
        }

        public void Start()
        {
            try
            {
                Interlocked.Increment(ref instance);

                SleepState.Stop(true);

                tvdb = new Tvdb(true);
                server = new TvServer(tuner);
                server.Open();
                server.AddRef();

                while (true)
                {
                    var service = RecTimer.Queue.Find(tvdb, tuner);

                    if (service == null)
                        break;

                    service = RecTimer.Queue.Dequeue(tvdb, tuner);

                    var epgWait = AppDefine.EpgWait.GetWait(service.Nid);

                    var msg = "{0}: 番組表を取得しています... {1}".Formatex(
                        tuner.Name,
                        service.Name);

                    Log.EpgUpdate(msg);

                    server.SetService(service);

                    var sw = new Stopwatch();
                    sw.Start();

                    while (sw.ElapsedMilliseconds < epgWait * 1000)
                    {
                        var res = Reserve.GetActiveReserve(tuner, tvdb);

                        if (res != null)
                            throw new Exception("予約時間のため中断しました。");
                        else if (stop)
                            throw new Exception("中断しました。");
                        else
                            Thread.Sleep(500);
                    }

                    UpdateEvent(service);
                }
            }
            catch (Exception ex)
            {
                Log.Error(tuner.Name + ": 番組表更新に失敗しました。" + ex.Message);
                Log.Debug(ex.StackTrace);
            }
            finally
            {
                try
                {
                    server.RemoveRef();
                    server.Close();
                }
                catch { }

                SleepState.Stop(false);
                Interlocked.Decrement(ref instance);

                if (Running == false)
                {
                    try
                    {
                        Log.Info("予約を更新しています...");

                        Reserve.Cleanup(tvdb);
                        Event.Cleanup(tvdb);
                        Reserve.UpdateReserveTime(tvdb);
                        AutoReserve.AddReserveAll(tvdb);
                        tvdb.Dispose();

                        Log.Info("番組表更新が完了しました。");
                    }
                    catch (Exception ex)
                    {
                        Log.Error("番組表更新に失敗しました。" + ex.Message);
                        Log.Debug(ex.StackTrace);
                    }
                }
            }
        }
                
        //番組表更新
        //取得したサービスだけでなく、同じnid, tsidのサービスも更新する
        void UpdateEvent(Service service)
        {
            var list = new List<Service>();

            if (service.EpgBasic)
                tvdb.Sql = "select * from service where driver = '{0}' and (fsid >> 32) = {1}".Formatex(service.Driver, service.Nid);
            else
                tvdb.Sql = "select * from service where driver = '{0}' and ((fsid >> 16) & 0xffff) = {1}".Formatex(service.Driver, service.Tsid);

            using (var t = tvdb.GetTable())
                while (t.Read())
                    list.Add(new Service(t));

            foreach (var s in list)
            {
                GetEvents(server, tvdb, s);
                GetLogo(s);
            }
        }

        //番組表更新
        public static void GetEvents(TvServer server, Tvdb tvdb, Service service)
        {
            List<Event> list = null;
            try
            {
                list = server.GetEvents(service);
            }
            catch (TvServerExceotion e)
            {
                Log.Error(e.Message);
                return;
            }

            foreach (var ev in list)
            {
                //同じeidの番組を探す
                tvdb.Sql = "select id from event where eid = {0} and fsid = {1}".Formatex(ev.Eid, ev.Fsid);
                var id = tvdb.GetData();

                //存在する場合、削除して同じidで登録する
                ev.Id = id == null ? -1 : (int)(long)id;
                ev.Add(tvdb);
            }
        }

        void GetLogo(Service s)
        {
            try
            {
                var dir = Util.GetUserPath("logo");

                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);

                var logo = Path.Combine(dir, s.Fsid + ".bmp");

                if (File.Exists(logo) == false)
                    server.GetLogo(s, logo);
                else
                {
                    var update = DateTime.Now + TimeSpan.FromDays(30);
                    if (File.GetLastWriteTime(logo) > update)
                        server.GetLogo(s, logo);
                }
            }
            catch (Exception ex)
            {
                Log.Error("ロゴの取得に失敗しました。[詳細] " + ex.Message);
                Log.Debug(ex.StackTrace);
            }
        }
    }
}
