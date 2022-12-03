using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Tvmaid
{
    //録画
    class Recorder
    {
        Tvdb tvdb;
        TvServer server;
        Tuner tuner;
        Service service;
        Reserve reserve;
        Record record;
        TsStatus tsStatus;
        string recPath;

        bool eventTimeError = false;    //録画中の番組情報取得エラーフラグ
        bool recContinue = false;       //録画継続フラグ(録画時間変更に使用)

        static bool stop = false;
        static int instance = 0; //インスタンスのカウンタ。実行中かどうか調べるのに使用

        public static bool Running
        {
            get { return instance > 0; }
        }        

        public static void Stop()
        {
            stop = true;
        }

        public Recorder(Tuner tuner, Reserve reserve)
        {
            this.tuner = tuner;
            this.reserve = reserve;
        }

        //次の予約の時間を取得
        long GetNextReserveTime()
        {
            //チューナが同一で、録画中の番組の終了時間より後に開始する有効な番組を検索
            tvdb.Sql = "select start from reserve where tuner = '{0}' and status & {1} and start >= {2} order by start".Formatex(reserve.TunerName, (int)Reserve.StatusCode.Enable, reserve.EndTime.Ticks);
            using (var t = tvdb.GetTable())
                return t.Read() ? t.GetLong(0) : DateTime.MaxValue.Ticks;
        }

        public void Start()
        {
            try
            {
                Interlocked.Increment(ref instance);

                Log.Info(tuner.Name + ": 録画を開始します。" + reserve.Title);
                StartRec();

                if (reserve.EndTime < DateTime.Now)
                    throw new Exception("終了時刻が過ぎています。");

                var marginDef = AppDefine.Main.Data.GetInt("record.margin.end");

                while (true)
                {
                    var margin = new TimeSpan(0, 0, marginDef);

                    //次の予約と現在の予約の間がマージン以上空いていたらマージンを0にする
                    if (GetNextReserveTime() - reserve.EndTime.Ticks > margin.Ticks)
                        margin = new TimeSpan(0);

                    if (reserve.EndTime - margin < DateTime.Now)
                        break;

                    if (stop)
                        throw new Exception("アプリケーション終了のため、録画を中断します。");

                    CheckCancel();   //録画キャンセル確認
                    CheckEvent();    //番組情報確認

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                var msg = "{0}: 録画に失敗しました。{1}".Formatex(tuner.Name, ex.Message);
                Log.Error(msg + " - " + reserve.Title);
                Log.Debug(ex.StackTrace);

                if (record != null)
                {
                    record.Code = 1;
                    record.Message = msg;
                }
            }
            finally
            {
                try
                {
                    StopRec();
                }
                catch (Exception ex)
                {
                    Log.Error("{0}: 録画終了処理に失敗しました。{1}".Formatex(tuner.Name, ex.Message));
                    Log.Debug(ex.StackTrace);
                }

                Log.Info("{0}: 録画終了しました。 - {1}".Formatex(tuner.Name, reserve.Title));

                Interlocked.Decrement(ref instance);

                PostProcess();
            }
        }

        private void PostProcess()
        {
            try
            {
                var process = AppDefine.Main.Data["postprocess"];
                if (record.Code == 0 && process != "")
                {
                    Log.Info("録画後プロセス実行.");
                    Process.Start(process, "\"" + recPath + "\"");
                }
            }
            catch (Exception ex)
            {
                Log.Error("録画後プロセスの実行に失敗しました。[詳細] " + ex.Message);
                Log.Debug(ex.StackTrace);
            }
        }

        Event GetEventTime()
        {
            try
            {
                var ev = server.GetEventTime(service, reserve.Eid);

                if (eventTimeError)
                {
                    Log.Info(tuner.Name + ": 番組時間の取得成功。" + reserve.Title);
                    eventTimeError = false; //エラーフラグをリセット
                }
                return ev;

            }
            catch (TvServerExceotion ex)
            {
                if (ex.Code == (uint)TvServer.ErrorCode.GetEventTime)
                {
                    if (eventTimeError == false)    //何度もエラーが表示されないようにする
                    {
                        Log.Alert(tuner.Name + ": 番組時間の取得に失敗しました。番組がなくなった可能性があります。録画は続行します。" + reserve.Title);
                        eventTimeError = true;
                    }

                    return null;
                }

                throw;
            }
        }

        //番組の時間に変更がないか確認
        void CheckEvent()
        {
            if (reserve == null) return;

            //追従モードでない
            if ((reserve.Status & (int)Reserve.StatusCode.EventMode) == 0) return;

            var ev = GetEventTime();
            if (ev == null) return;

            if (ev.Start != reserve.StartTime || ev.Duration != reserve.Duration)
            {
                Log.Alert(tuner.Name + ": 番組時間が変更されました。" + reserve.Title);

                //予約を変更
                reserve.StartTime = ev.Start;
                reserve.Duration = ev.Duration;
                EpgUpdater.GetEvents(server, tvdb, service);//番組表を更新する
                Reserve.UpdateReserveTime(tvdb);  //予約を更新
                Reserve.ResetTuner(tvdb);        //予約のチューナをリセット(番組の時間が変更したことで重複の可能性があるため)

                Log.Alert("チューナの再割り当てを行いました。");

                //番組開始が、現時刻より1分以上後に変更された場合、一旦録画を終了する
                if (reserve.StartTime - DateTime.Now > TimeSpan.FromMinutes(1))
                {
                    recContinue = true;
                    throw new Exception("番組の開始時間が遅れているため、録画を中断します。");
                }
            }
        }

        //現在の予約が有効かどうか確認
        void CheckCancel()
        {
            if (reserve == null) return;

            tsStatus = server.GetTsStatus();

            tvdb.Sql = "select status from reserve where id = " + reserve.Id;
            var status = tvdb.GetData();

            if (status == null)  //予約が取り消された
                throw new Exception("予約が取り消されたため、録画を中断します。");
            else if (((int)(long)status & (int)Reserve.StatusCode.Enable) == 0)  //予約が無効にされた
                throw new Exception("予約が無効にされたため、録画を中断します。");

            if (server.GetState() != TvServer.State.Recoding)
                throw new Exception(" 録画が中断しました。");
        }

        void StartRec()
        {
            SleepState.Stop(true);

            tvdb = new Tvdb(true);
            service = new Service(tvdb, reserve.Fsid);
            recContinue = false;
            eventTimeError = false;

            var path = GetRecPath();
            this.recPath = path;

            InitRecord(path);
            tsStatus = new TsStatus();

            reserve.SetRecoding(tvdb, true);

            server = new TvServer(tuner);
            server.Open();
            server.AddRef();
            server.SetService(service);
            server.StartRec(path);

            Thread.Sleep(1000); //録画開始後ウエイトを入れる。すぐにTVTestにアクセスするとエラーになるため
        }

        void InitRecord(string path)
        {
            record = new Record();
            record.Title = reserve.Title;
            record.ServiceName = service.Name;
            record.File = Path.GetFileName(path);
            record.ReserveStart = reserve.StartTime;
            record.ReserveEnd = reserve.EndTime;
            record.Start = DateTime.Now;

            if (reserve.Auto != -1)
            { 
                tvdb.Sql = "select name from auto_reserve where id = " + reserve.Auto;
                var auto = (string)tvdb.GetData();

                if (auto != null)
                    record.Auto = auto;
            }
        }

        string GetRecPath()
        {
            var dir = AppDefine.Main.Data["record.folder"];
            var file = ConvertFileMacro(AppDefine.Main.Data["record.file"]);
            var path = Path.Combine(dir, file);
            return CheckFilePath(path);
        }

        void StopRec()
        {
            if (recContinue)
                reserve.SetRecoding(tvdb, false);    //延期する
            else
                reserve.SetComplete(tvdb);

            try
            {
                var state = server.GetState();
                if (state == TvServer.State.Recoding || state == TvServer.State.RecPaused)
                    server.StopRec();
            }
            catch { }

            try
            {
                server.RemoveRef();
                server.Close();
            }
            catch { }

            try
            {
                SetRecord();
            }
            catch (Exception ex)
            {
                Log.Error("録画情報の保存に失敗しました。" + ex.Message);
            }

            tvdb.Dispose();

            SleepState.Stop(false);
        }

        void SetRecord()
        {
            record.Error = tsStatus.Error;
            record.Drop = tsStatus.Drop;
            record.Scramble = tsStatus.Scramble;
            record.End = DateTime.Now;

            if (reserve.Eid != -1)
            {
                var ev = new Event(tvdb, reserve.Fsid, reserve.Eid);
                record.Desc = ev.Desc;
                record.LongDesc = ev.LongDesc;
                record.GenreText = ev.GenreText;
            }

            record.Add(tvdb);
        }

        //ファイル名に使えない文字を変換
        string ConvertFileName(string name)
        {
            var list = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
            foreach (var c in list)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        //ファイル名マクロ変換
        public string ConvertFileMacro(string name)
        {
            var dic = new Dictionary<string, string>();

            dic.Add("{title}", reserve.Title);
            dic.Add("{service}", service.Name);

            dic.Add("{nid}", service.Nid.ToString("x"));
            dic.Add("{tsid}", service.Tsid.ToString("x"));
            dic.Add("{sid}", service.Sid.ToString("x"));
            dic.Add("{eid}", reserve.Eid.ToString("x"));

            dic.Add("{start-yyyy}", reserve.StartTime.ToString("yyyy"));
            dic.Add("{start-yy}", reserve.StartTime.ToString("yy"));
            dic.Add("{start-MM}", reserve.StartTime.ToString("MM"));
            dic.Add("{start-M}", reserve.StartTime.ToString("%M"));
            dic.Add("{start-dd}", reserve.StartTime.ToString("dd"));
            dic.Add("{start-d}", reserve.StartTime.ToString("%d"));
            dic.Add("{start-week}", reserve.StartTime.ToString("ddd"));

            dic.Add("{start-hh}", reserve.StartTime.ToString("HH"));
            dic.Add("{start-h}", reserve.StartTime.ToString("%H"));
            dic.Add("{start-mm}", reserve.StartTime.ToString("mm"));
            dic.Add("{start-m}", reserve.StartTime.ToString("%m"));

            dic.Add("{end-yyyy}", reserve.EndTime.ToString("yyyy"));
            dic.Add("{end-yy}", reserve.EndTime.ToString("yy"));
            dic.Add("{end-MM}", reserve.EndTime.ToString("MM"));
            dic.Add("{end-M}", reserve.EndTime.ToString("%M"));
            dic.Add("{end-dd}", reserve.EndTime.ToString("dd"));
            dic.Add("{end-d}", reserve.EndTime.ToString("%d"));
            dic.Add("{end-week}", reserve.EndTime.ToString("ddd"));

            dic.Add("{end-hh}", reserve.EndTime.ToString("HH"));
            dic.Add("{end-h}", reserve.EndTime.ToString("%H"));
            dic.Add("{end-mm}", reserve.EndTime.ToString("mm"));
            dic.Add("{end-m}", reserve.EndTime.ToString("%m"));

            var duration = new TimeSpan(0, 0, reserve.Duration);
            dic.Add("{duration-hh}", duration.ToString("hh"));
            dic.Add("{duration-h}", duration.ToString("%h"));
            dic.Add("{duration-mm}", duration.ToString("mm"));
            dic.Add("{duration-m}", duration.ToString("%m"));

            foreach (var macro in dic)
            {
                name = name.Replace(macro.Key, macro.Value);
            }
            return ConvertFileName(name);
        }

        //ファイルがすでに存在した場合は「(2)」をつける。
        string CheckFilePath(string src)
        {
            string dest = src;
            int i = 2;
            while (File.Exists(dest))
            {
                dest = "{0}({1}){2}".Formatex(Path.Combine(Path.GetDirectoryName(src), Path.GetFileNameWithoutExtension(src)), i, Path.GetExtension(src));
                i++;
            }
            return dest;
        }
    }
}
