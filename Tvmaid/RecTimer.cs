using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tvmaid
{
    //録画、番組情報更新タイマー
    class RecTimer
    {
        Tvdb tvdb;
        Tuner tuner;

        public static DateTime NextEpgUpdate { get; set; }   //次回番組表更新時間
        public static ServiceQueue Queue = new ServiceQueue();

        static bool stop = false;

        //チューナ毎のスレッドを起動
        public static void Run()
        {
            SetNextEpgUpdate();

            using (var tvdb = new Tvdb(true))
            {
                tvdb.Sql = @"select * from tuner";

                using (var t = tvdb.GetTable())
                {
                    while (t.Read())
                    {
                        var tuner = new Tuner(t);

                        Task.Factory.StartNew(() =>
                        {
                            var timer = new RecTimer(tuner);
                            timer.Start();

                            System.Diagnostics.Debug.WriteLine("RecTimer Exit: " + tuner.Name);
                        }, TaskCreationOptions.AttachedToParent);
                    }
                }
            }
        }

        public static void Stop()
        {
            stop = true;
            Recorder.Stop();
            EpgUpdater.Stop();
        }
                
        RecTimer(Tuner tuner)
        {
            this.tuner = tuner;
        }

        //メインループ
        //録画、番組情報更新、番組情報更新キュー作成
        void Start()
        {
            using (tvdb = new Tvdb(true))
            {
                while (stop == false)
                {
                    var res = Reserve.GetActiveReserve(tuner, tvdb);

                    if (res != null)
                    {
                        if (res.EndTime < DateTime.Now)
                        {
                            res.SetEnable(tvdb, false);
                            Log.Error("この予約は実行されません。終了時刻が過ぎています。- " + res.Title);
                        }
                        else
                        {
                            var server = new TvServer(tuner);
                            if (server.IsRecording)
                                server.StopRec();

                            var recorder = new Recorder(tuner, res);
                            recorder.Start();
                        }
                    }
                    else if (Queue.Find(tvdb, tuner) != null)
                    {
                        var updater = new EpgUpdater(tuner);
                        updater.Start();
                    }
                    else if (NextEpgUpdate < DateTime.Now)
                    {
                        SetNextEpgUpdate();
                        Queue.Init();
                        EpgUpdater.Resume();
                    }
                    else
                        Thread.Sleep(500);
                }
            }
        }

        //次回、番組表更新日時
        static void SetNextEpgUpdate()
        {
            var now = DateTime.Now.Hour;
            var data = AppDefine.Main.Data["epg.hour"];
            var hours = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<int>();

            foreach (var hour in hours)
                list.Add(hour.ToInt());

            list.Sort();
            list.Add(list[0] + 24);

            var span = 0;
            foreach (var hour in list)
            {
                if (now < hour)
                {
                    span = hour;
                    break;
                }
            }

            NextEpgUpdate = DateTime.Now.Date.AddHours(span);

            Log.Info("次回番組表更新: " + NextEpgUpdate.ToString("M/d HH:mm"));
        }

        public static void UpdateEpg()
        {
            if (Queue.Count == 0)
            {
                Queue.Init();
                EpgUpdater.Resume();
                Log.Info("番組表更新を開始します。");
            }
            else
                Log.Alert("番組表更新は、すでに開始しています。");
        }
        /*
        public static void UpdateEvent(Service service)
        {
            Queue.Enqueue(service);
        }
        */
        public static void CancelUpdateEpg()
        {
            Queue.Clear();
            EpgUpdater.Stop();
            Log.Info("番組表更新を中止します。");
        }
    }

    class ServiceQueue
    {
        List<Service> list = new List<Service>();

        public void Init()
        {
            lock (list)
            {
                list.Clear();

                using (var tvdb = new Tvdb(true))
                {
                    var nid = AppDefine.Main.Data["epg.basic"];

                    //空の時はあり得ない数値を入れておく(該当しないようにする)
                    if (nid == "") nid = "-1";

                    //epg.basicで指定されたnidのサービスは、nidでまとめる
                    //(基本情報のみの場合、同じnidのどれか一つでよい)
                    tvdb.Sql = "select *, (fsid >> 32) as nid from service"
                                + " where fsid >> 32 in ({0})"
                                + " group by nid"
                                + " order by id";
                    tvdb.Sql = tvdb.Sql.Formatex(nid);

                    Add(tvdb, true);

                    //上記以外のサービスは、tsidでまとめる
                    //(詳細情報も取得する場合、同じtsidのどれか一つでよい)
                    tvdb.Sql = "select *, ((fsid >> 16) & 0xffff) as tsid from service"
                                + " where fsid >> 32 not in ({0})"
                                + " group by tsid"
                                + " order by id";
                    tvdb.Sql = tvdb.Sql.Formatex(nid);

                    Add(tvdb, false);
                }
            }
        }

        void Add(Tvdb tvdb, bool basic)
        {
            using (var t = tvdb.GetTable())
            {
                while (t.Read())
                {
                    var service = new Service(t);
                    service.EpgBasic = basic;
                    list.Add(service);
                }
            }
        }
        /*
        public void Enqueue(Service service)
        {
            lock (list)
                list.Add(service);
        }
        */
        public Service Dequeue(Tvdb tvdb, Tuner tuner)
        {
            lock (list)
            {
                var service = Find(tvdb, tuner);

                if (service != null)
                    list.Remove(service);

                return service;
            }
        }

        public Service Find(Tvdb tvdb, Tuner tuner)
        {
            lock (list)
            {
                if (list.Count == 0)
                    return null;

                return list.Find(service =>
                {
                    tvdb.Sql = "select id from service where fsid = {0} and driver = '{1}'".Formatex(service.Fsid, tuner.Driver);

                    using (var t = tvdb.GetTable())
                        return t.Read();
                });
            }
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public void Clear()
        {
            lock (list)
                list.Clear();
        }
    }
}
