using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tvmaid.Chat;

namespace Tvmaid
{
    //スクリプトから呼ばれるメソッドはpublicにすること
    class WebApi : WebApiBase
    {
        public WebApi(HttpListenerContext con) : base(con) { }

        public void GetTable()
        {
            using (var tvdb = new Tvdb(true))
            {
                tvdb.Sql = GetQuery("sql");
                ret.data1 = tvdb.GetList();
            }
        }

        //予約削除
        public void RemoveReserve()
        {
            var id = GetQuery("id");

            using (var tvdb = new Tvdb(true))
            {
                var res = new Reserve(tvdb, id.ToInt());

                //自動予約(または終わっている番組)の場合は、削除せずに無効にする
                if (res.Auto == -1 || res.EndTime < DateTime.Now)
                    res.Remove(tvdb);
                else
                    res.SetEnable(tvdb, false);
            }
        }

        //予約追加、更新
        //statusは、有効、追従のビット以外は無視される
        public void AddReserve()
        {
            var res = SetReserveQuery(); //送信された値を入力

            using (var tvdb = new Tvdb(true))
            {
                //追従モードなら番組情報を入力
                if ((res.Status & (int)Reserve.StatusCode.EventMode) > 0)
                {
                    var ev = new Event(tvdb, res.Fsid, res.Eid);
                    res.Fsid = ev.Fsid;
                    res.Eid = ev.Eid;
                    res.StartTime = ev.Start;
                    res.Duration = ev.Duration;
                    res.Title = GetQuery("title", ev.Title);
                }
                res.Add(tvdb);
            }
            ret.data1 = res.Id;
        }

        Reserve SetReserveQuery()
        {
            Reserve res;
            var id = GetQuery("id", -1);

            if (id == -1)
                res = new Reserve();
            else
            {
                using (var tvdb = new Tvdb(true))
                    res = new Reserve(tvdb, id);

                if ((res.Status & (int)Reserve.StatusCode.Recoding) > 0)
                    throw new Exception("録画中のため、予約を変更できません。");
            }

            res.Fsid = GetQuery("fsid", res.Fsid);
            res.Eid = GetQuery("eid", res.Eid);

            res.StartTime = new DateTime(GetQuery("start", res.StartTime.Ticks));
            res.Duration = GetQuery("duration", res.Duration);

            res.Title = GetQuery("title", res.Title);
            res.TunerName = GetQuery("tuner", res.TunerName);

            var status = GetQuery("status", -1);
            if (status != -1)
            {
                status &= 7;    //有効、追従、チューナ固定のビット以外を削除
                res.Status &= 255 - 7;  //有効、追従、チューナ固定のビットを削除
                res.Status |= status;   //合成する
            }

            return res;
        }

        //予約チューナ再割り当て
        public void ResetReserveTuner()
        {
            using (var tvdb = new Tvdb(true))
                Reserve.ResetTuner(tvdb);
        }

        //自動予約削除
        public void RemoveAutoReserve()
        {
            var id = GetQuery("id");

            using (var tvdb = new Tvdb(true))
            {
                var auto = new AutoReserve(tvdb, id.ToInt());
                auto.Remove(tvdb);
            }
        }

        //自動予約追加、更新
        public void AddAutoReserve()
        {
            var auto = SetAutoReserveQuery();

            using (var tvdb = new Tvdb(true))
                auto.Add(tvdb);

            ret.data1 = auto.Id;
        }

        AutoReserve SetAutoReserveQuery()
        {
            AutoReserve auto;
            var id = GetQuery("id", -1);

            if (id == -1)
                auto = new AutoReserve();
            else
            {
                using (var tvdb = new Tvdb(true))
                    auto = new AutoReserve(tvdb, id);
            }

            auto.Name = GetQuery("name", auto.Name);
            auto.Folder = GetQuery("folder", auto.Folder);

            auto.Query = GetQuery("query", auto.Query);
            auto.Option = GetQuery("option", auto.Option);
            auto.Status = GetQuery("status", auto.Status);

            return auto;
        }

        //録画フォルダ空き取得
        public void GetRecordFolderFree()
        {
            var dir = AppDefine.Main.Data["record.folder"];
            ulong free, total, totalFree;

            if (GetDiskFreeSpaceEx(dir, out free, out total, out totalFree))
                ret.data1 = free;
            else
                throw new Exception("録画フォルダの空き容量が取得できませんでした。");
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(
            string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);

        //ユーザ番組表設定
        public void SetUserEpg()
        {
            var id = GetQuery("id").ToInt();
            var list = GetQuery("fsid").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            using (var tvdb = new Tvdb(true))
            {
                try
                {
                    tvdb.BeginTrans();

                    tvdb.Sql = "delete from user_epg where id = " + id;
                    tvdb.Execute();

                    for (var i = 0; i < list.Length; i++)
                    {
                        var fsid = list[i].ToLong();
                        tvdb.Sql = @"insert into user_epg values({0}, {1}, {2});".Formatex(id, fsid, i);
                        tvdb.Execute();
                    }
                    tvdb.Commit();
                }
                catch
                {
                    tvdb.Rollback();
                    throw;
                }
            }
        }

        public void GetLog()
        {
            using (var ldb = new Ldb(true))
            {
                ldb.Sql = GetQuery("sql");
                ret.data1 = ldb.GetList();
            }
        }

        public void ClearLog()
        {
            using (var ldb = new Ldb(true))
            {
                ldb.Sql = "delete from log";
                ldb.Execute();
            }

            Log.Info("ログをクリアしました。");
        }

        public void ClearRecord()
        {
            using (var tvdb = new Tvdb(true))
            {
                tvdb.Sql = "delete from record";
                tvdb.Execute();
            }

            Log.Info("録画をクリアしました。");
        }

        public void CancelUpdateEpg()
        {
            RecTimer.CancelUpdateEpg();
        }

        public void UpdateEpg()
        {
            RecTimer.UpdateEpg();
        }

        int GetTunerState(string tunerName, Tvdb tvdb)
        {
            Tuner tuner;

            tuner = new Tuner(tvdb, tunerName);

            var server = new TvServer(tuner);
            var state = server.GetState();

            //番組表更新中は5
            if (state == TvServerApi.State.View)
                return EpgUpdater.Running ? 5 : 0;
            else
                return (int)state;
        }

        public void GetTunersState()
        {
            var list = new List<object[]>();

            using (var tvdb = new Tvdb(true))
            {
                tvdb.Sql = "select name from tuner order by id";

                using (var t = tvdb.GetTable())
                {
                    while (t.Read())
                    {
                        var val = new object[2];
                        val[0] = t.GetStr(0);
                        list.Add(val);
                    }
                }

                foreach (var val in list)
                    val[1] = GetTunerState((string)val[0], tvdb);
            }

            ret.data1 = list;
        }

        public void ShowServer()
        {
            var tunerName = GetQuery("tuner");
            var fsid = GetQuery("fsid").ToLong();
            Tuner tuner;
            Service service;

            using (var tvdb = new Tvdb(true))
            {
                tuner = new Tuner(tvdb, tunerName);
                service = new Service(tvdb, fsid);
            }

            var server = new TvServer(tuner);
            server.Open(true);
            server.SetService(service);
        }

        public void CloseServer()
        {
            var tunerName = GetQuery("tuner");
            Tuner tuner;

            using (var tvdb = new Tvdb(true))
                tuner = new Tuner(tvdb, tunerName);

            var server = new TvServer(tuner);
            server.Close();
        }

        public void RemoveFile()
        {
            var id = GetQuery("id").ToInt();
            Record record;

            using (var tvdb = new Tvdb(true))
                record = new Record(tvdb, id);

            var file = Path.Combine(AppDefine.Main.Data["record.folder"], record.File);

            if (File.Exists(file))
                ShellFile.Delete(file);

            UpdateRecordStatus();
        }

        public void UpdateRecordStatus()
        {
            var recFolder = AppDefine.Main.Data["record.folder"];
            var files = Directory.GetFiles(recFolder);

            using (var tvdb = new Tvdb(true))
            {
                tvdb.Sql = "update record set status = status | {0}".Formatex((int)Record.StatusCode.Delete);
                tvdb.Execute();

                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);

                    tvdb.Sql = "update record set status = status - {0} where file = '{1}'".Formatex((int)Record.StatusCode.Delete, Tvdb.SqlEncode(name));
                    tvdb.Execute();
                }
            }
        }

        public void StartHls()
        {
            var streamId = GetQuery("stream");
            var ready = GetQuery("ready", 1);

            if (GetQuery("type") == "live")
            {
                var tuner = GetQuery("tuner");
                var fsid = GetQuery("fsid").ToLong();
                var mode = GetQuery("mode");

                HlsStream.Start(streamId, tuner, fsid, mode);
            }
            else
            {
                var id = GetQuery("id").ToInt();
                var start = GetQuery("start").ToInt();
                var mode = GetQuery("mode");

                HlsStream.Start(streamId, id, start, mode);
            }

            var stream = HlsStream.GetStream(streamId);

            while (stream.GetPlaylistCount < ready)
            {
                if (stream.IsStop)
                    throw new Exception("ストリームの開始に失敗しました。");

                System.Threading.Thread.Sleep(100);
            }
        }

        public void StopHls()
        {
            var stream = GetQuery("stream");
            HlsStream.Stop(stream);
        }

        static bool tsFileInfoRunning = false;

        public void AddTsFileInfo()
        {
            if (tsFileInfoRunning)
                throw new Exception("TSファイル登録は、すでに実行中です。");

            tsFileInfoRunning = true;
            Log.Info("TSファイル登録を開始します。");

            Task.Factory.StartNew(()=>
            {
                var files = Directory.GetFiles(AppDefine.Main.Data["record.folder"], "*.ts");

                using (var tvdb = new Tvdb(true))
                {
                    foreach (var file in files)
                    {
                        tvdb.Sql = "select id from record where file = '{0}'".Formatex(Tvdb.SqlEncode(Path.GetFileName(file)));
                        using (var t = tvdb.GetTable())
                            if (t.Read())
                                continue;   //すでに登録済み

                        try
                        {
                            var info = new TsFileInfo(file);
                            var rec = new Record();
                            rec.Title = info.Title;
                            rec.ServiceName = info.Service;
                            rec.Desc = info.Desc;
                            rec.LongDesc = info.LongDesc;
                            rec.GenreText = info.Genre;
                            rec.File = info.File;
                            rec.Start = info.StartTime;
                            rec.End = info.EndTime;
                            rec.ReserveStart = info.ReserveStart;
                            rec.ReserveEnd = info.ReserveEnd;
                            rec.Message = "[TSファイル登録]";
                            rec.Add(tvdb);

                            Log.Info("登録: " + Path.GetFileName(file));
                        }
                        catch (Exception ex)
                        {
                            Log.Error("{0} を登録できませんでした。{1}".Formatex(Path.GetFileName(file), ex.Message));
                        }
                    }
                }
                Log.Info("TSファイル登録が完了しました。");

            }, TaskCreationOptions.AttachedToParent)
            .ContinueWith(_ =>
            {
                tsFileInfoRunning = false;
            });
        }

        static Dictionary<string, ChatServer> ChatServers = new Dictionary<string, ChatServer>(); //接続中サーバリスト

        public void OpenChatServer()
        {
            var type = GetQuery("type").ToInt();
            var id = GetQuery("id");
            var fsid = GetQuery("fsid").ToLong();

            lock (ChatServers)
            {
                ChatServer server = null;

                if (ChatServers.ContainsKey(id))
                    ChatServers.Remove(id);

                switch (type)
                {
                    case 1:
                        server = new NiconicoLiveServer();
                        break;
                    case 2:
                        var mail = AppDefine.Main.Data["chat.niconico.mail"];
                        var password = AppDefine.Main.Data["chat.niconico.password"];

                        if (mail == null || mail == "" || password == null || password == "")
                            throw new Exception("メールアドレスまたはパスワードが設定されていないため、ニコニコ実況過去ログは使用できません。");
                        else
                            server = new NiconicoLogServer(mail, password, GetQuery("start").ToInt(), GetQuery("end").ToInt());
                        break;
                    default:
                        throw new Exception("定義されていないChatServerです。");
                }

                server.Open(fsid);
                ChatServers.Add(id, server);
                server.Active.Start();

                RemoveChatServer();
            }
        }

        void RemoveChatServer()
        {
            var list = new List<string>();

            foreach (var server in ChatServers)
                if (server.Value.Active.ElapsedMilliseconds > 60 * 1000)
                    list.Add(server.Key);

            foreach (var key in list)
                ChatServers.Remove(key);
        }

        public void GetChat()
        {
            var id = GetQuery("id");
            var time = GetQuery("time").ToInt();
            var max = GetQuery("max").ToInt();

            lock (ChatServers)
            {
                if (ChatServers.ContainsKey(id))
                    ret.data1 = ChatServers[id].GetChat(time, max);
                else
                    throw new Exception("指定されたIDのChatServerがありません。");

                ChatServers[id].Active.Restart();
            }
        }
    }

    public static class ShellFile
    {
        public static void Delete(string path)
        {
            var sh = new SHFILEOPSTRUCT();
            sh.hwnd = IntPtr.Zero;
            sh.wFunc = FOFunc.FO_DELETE;
            sh.pFrom = path + "\0\0";
            sh.pTo = null;
            sh.fFlags = FOFlags.FOF_SILENT | FOFlags.FOF_NOERRORUI | FOFlags.FOF_ALLOWUNDO | FOFlags.FOF_NOCONFIRMATION;
            sh.fAnyOperationsAborted = false;
            sh.hNameMappings = IntPtr.Zero;
            sh.lpszProgressTitle = "";

            var ret = SHFileOperation(ref sh);

            if (ret != 0)
                throw new Exception("ファイルの削除に失敗しました。");
        }

        enum FOFunc : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004
        }

        enum FOFlags : ushort
        {
            FOF_MULTIDESTFILES = 0x0001,
            FOF_CONFIRMMOUSE = 0x0002,
            FOF_SILENT = 0x0004,            // don't create progress/report
            FOF_RENAMEONCOLLISION = 0x0008,
            FOF_NOCONFIRMATION = 0x0010,    // Don't prompt the user.
            FOF_WANTMAPPINGHANDLE = 0x0020, // Fill in SHFILEOPSTRUCT.hNameMappings
                                            // Must be freed using SHFreeNameMappings
            FOF_ALLOWUNDO = 0x0040,
            FOF_FILESONLY = 0x0080,         // on *.*, do only files
            FOF_SIMPLEPROGRESS = 0x0100,    // means don't show names of files
            FOF_NOCONFIRMMKDIR = 0x0200,    // don't confirm making any needed dirs
            FOF_NOERRORUI = 0x0400,         // don't put up error UI
            FOF_NOCOPYSECURITYATTRIBS = 0x0800,  // dont copy NT file Security Attributes
            FOF_NORECURSION = 0x1000,       // don't recurse into directories.
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,  // don't operate on connected elements.
            FOF_WANTNUKEWARNING = 0x4000,   // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
            FOF_NORECURSEREPARSE = 0x8000   // treat reparse points as objects, not containers
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FOFunc wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public FOFlags fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);
    }
}