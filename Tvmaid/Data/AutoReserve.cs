using System;
using System.Collections.Generic;

namespace Tvmaid
{
    //自動録画予約
    class AutoReserve
    {
        public int Id = -1;                     //-1: 新規
        public string Name = "未定";
        public string Folder = "";

        public string Query = "";
        public string Option = "";
        public int Status = (int)Reserve.StatusCode.Overlay | (int)Reserve.StatusCode.EventMode;

        public enum AutoRecStatus
        {
            Enable = 1
        };

        public AutoReserve() { }

        public AutoReserve(DbTable t)
        {
            Init(t);
        }

        void Init(DbTable t)
        {
            Id = t.GetInt("id");
            Name = t.GetStr("name");
            Folder = t.GetStr("folder");

            Query = t.GetStr("query");
            Option = t.GetStr("option");
            Status = t.GetInt("status");
        }

        public AutoReserve(Tvdb tvdb, int id)
        {
            tvdb.Sql = "select * from auto_reserve where id = " + id;
            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("自動予約が見つかりません。");
            }
        }

        //削除
        public void Remove(Tvdb tvdb)
        {
            //自動で行われた予約を削除
            tvdb.Sql = "delete from reserve where auto = " + Id;
            tvdb.Execute();

            tvdb.Sql = "delete from auto_reserve where id = " + Id;
            tvdb.Execute();

            Reserve.SetOverlay(tvdb);
        }

        //追加/編集
        public void Add(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                if (this.Name == "")
                    throw new Exception("名前を入力してください。");

                if (Id == -1)
                {
                    tvdb.Sql = "select id from auto_reserve where name = '{0}'".Formatex(Name);

                    if (tvdb.GetData() != null)
                        throw new Exception("同じ名前の自動予約があります。登録できませんでした。 - " + Name);

                    Id = tvdb.GetNextId("auto_reserve");
                }
                else
                {
                    tvdb.Sql = "select id from auto_reserve where name = '{0}' and not id = {1}".Formatex(Name, Id);

                    if (tvdb.GetData() != null)
                        throw new Exception("同じ名前の自動予約があります。登録できませんでした。 - " + Name);

                    Remove(tvdb);
                }

                if (Folder == "")
                    throw new Exception("フォルダ名がありません。登録できませんでした。 - " + Name);

                tvdb.Sql = @"insert into auto_reserve values(
                        {0}, '{1}', '{2}', '{3}', '{4}', {5});".Formatex(
                                Id,
                                Tvdb.SqlEncode(Name),
                                Tvdb.SqlEncode(Folder),
                                Tvdb.SqlEncode(Query),
                                Tvdb.SqlEncode(Option),
                                Status
                                );
                tvdb.Execute();
            }
            finally
            {
                tvdb.Commit();
            }

            if (Status == 1)
                AddReserve(tvdb);
        }
        
        //検索条件に合致して、まだ予約されていない番組を予約
        public void AddReserve(Tvdb tvdb)
        {
            //現時刻+マージン(マージンを足しておかないと、終了した番組を予約してしまうため)
            var margin = AppDefine.Main.Data.GetInt("record.margin.end");
            var now = DateTime.Now + new TimeSpan(0, 0, margin);

            tvdb.Sql = @"select * from event
                            left join reserve on event.fsid = reserve.fsid and event.eid = reserve.eid
                            where
                            event.end > {0} and reserve.id is null and event.id in ({1})".Formatex(now.Ticks, Query);

            var events = new List<Event>();

            using (var t = tvdb.GetTable())
            {
                while (t.Read())
                    events.Add(new Event(t));
            }

            //missAutoReserveCount以上ヒットした場合、その自動予約を無効にする(間違った自動予約と判定する)
            const int missAutoReserveCount = 50;

            if (events.Count > missAutoReserveCount)
            {
                this.Status = 0;
                tvdb.Sql = "update auto_reserve set status = 0 where id = " + this.Id;
                tvdb.Execute();

                Log.Alert("自動予約 '{0}' を無効にしました。{1} 件以上ヒットします。条件を見なおしてください。".Formatex(Name, missAutoReserveCount));
            }
            else
            {
                foreach (var ev in events)
                {
                    var res = new Reserve();
                    res.Fsid = ev.Fsid;
                    res.Eid = ev.Eid;
                    res.StartTime = ev.Start;
                    res.Duration = ev.Duration;
                    res.Title = ev.Title;
                    res.Auto = Id;
                    res.Add(tvdb);
                }
            }
        }

        public static void AddReserveAll(Tvdb tvdb)
        {
            var autoList = new List<AutoReserve>();
            tvdb.Sql = "select * from auto_reserve where status = 1 and query <> ''";

            using (var t = tvdb.GetTable())
            {
                while (t.Read())
                    autoList.Add(new AutoReserve(t));
            }

            foreach (var auto in autoList)
                auto.AddReserve(tvdb);
        }
    }
}
