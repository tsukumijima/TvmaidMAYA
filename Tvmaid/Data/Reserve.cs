using System;
using System.Collections.Generic;

namespace Tvmaid
{
    //録画予約
    class Reserve
    {
        public int Id = -1;             //-1: 新規
        public long Fsid = 0;
        public int Eid = -1;            //-1: 番組情報なし

        public DateTime StartTime = DateTime.Now;
        public int Duration = 0;

        public int Auto = -1;           //-1: 手動予約
        public int Status = (int)StatusCode.Enable | (int)StatusCode.EventMode;
        public string Title = "未定";
        public string TunerName = "";   //"": チューナ自動選択

        public DateTime EndTime
        {
            get
            {
                long d = Duration;
                return new DateTime(StartTime.Ticks + (d * 10 * 1000 * 1000));
            }
        }

        public enum StatusCode
        {
            Enable = 1,
            EventMode = 2,
            TunerLock = 4,
            Overlay = 32,
            Recoding = 64,
            Complete = 128
        };

        public Reserve() { }

        public Reserve(DbTable t)
        {
            Init(t);
        }

        void Init(DbTable t)
        {
            Id = t.GetInt("id");

            Fsid = t.GetLong("fsid");
            Eid = t.GetInt("eid");
            StartTime = new DateTime(t.GetLong("start"));
            Duration = t.GetInt("duration");

            Auto = t.GetInt("auto");
            Status = t.GetInt("status");
            Title = t.GetStr("title");
            TunerName = t.GetStr("tuner");
        }

        public Reserve(Tvdb tvdb, int id)
        {
            tvdb.Sql = "select * from reserve where id = " + id;

            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("予約が見つかりません。");
            }
        }

        //予約追加
        public void Add(Tvdb tvdb)
        {
            if(this.EndTime < DateTime.Now)
                throw new Exception("過去の番組は予約できません。");

            //チューナが指定されているか？
            if (TunerName != "")
            {
                //チューナが不正でないか確認
                tvdb.Sql =
                        @"select name from tuner
                        where
                        name = '{0}'
                        and
                        driver in (select driver from service where fsid = {1})"
                        .Formatex(Tvdb.SqlEncode(TunerName), Fsid);

                var name = (string)tvdb.GetData();

                if (name == null)
                    throw new Exception("指定されたチューナ間違っています。チューナを確認してください。");
            }
            else
                GetFreeTuner(tvdb);  //空きを探す

            bool newId = this.Id == -1;
            AddReserve(tvdb);

            if (newId)
                Log.Info("予約しました。" + this.Title);
            else
                Log.Info("予約を変更しました。" + this.Title);
        }

        //チューナをすべてリセット
        public static void ResetTuner(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                //固定でなく、録画中でなく、有効な予約のチューナを削除
                tvdb.Sql = "update reserve set tuner = '' where status & {0} = 0 and status & {1} > 0".Formatex((int)StatusCode.TunerLock + (int)StatusCode.Recoding, (int)StatusCode.Enable);
                tvdb.Execute();

                //再度割り当てる
                var list = new List<Reserve>();
                tvdb.Sql = "select * from reserve where tuner = '' order by start";

                using (var t = tvdb.GetTable())
                {
                    while (t.Read())
                        list.Add(new Reserve(t));
                }

                foreach (var res in list)
                {
                    res.GetFreeTuner(tvdb);
                    tvdb.Sql = "update reserve set tuner = '{0}' where id = {1}".Formatex(Tvdb.SqlEncode(res.TunerName), res.Id);
                    tvdb.Execute();
                }

                tvdb.Commit();
                SetOverlay(tvdb);
            }
            catch
            {
                tvdb.Rollback();
                throw;
            }
        }

        void GetFreeTuner(Tvdb tvdb)
        {
            //指定サービスを持っていて、予約の入っていないチューナを検索
            tvdb.Sql =
                @"select name from tuner
                    where
                    driver in (select driver from service where fsid = {0}) 
                    and
                    name not in (select tuner from reserve where {1} < end and {2} > start and status & {3})
                    order by id"
                    .Formatex(Fsid, StartTime.Ticks, EndTime.Ticks, (int)Reserve.StatusCode.Enable);

            var name = tvdb.GetData();

            if (name != null)
                TunerName = (string)name;
            else
            {
                //空きが無いので、指定サービスを持つ1番目のチューナを取得
                tvdb.Sql =
                    @"select name from tuner
                        where
                        driver in (select driver from service where fsid = {0}) 
                        order by id"
                    .Formatex(Fsid);

                TunerName = (string)tvdb.GetData();
            }
        }

        //予約削除
        public void Remove(Tvdb tvdb, bool removeOnly = false)
        {
            tvdb.Sql = "delete from reserve where id = " + Id;
            tvdb.Execute();

            if (removeOnly == false)
            {
                SetOverlay(tvdb);
                Log.Info("予約を取り消しました。" + this.Title);
            }
        }

        //開始時間が過ぎていて、有効な予約を取得
        public static Reserve GetActiveReserve(Tuner tuner, Tvdb tvdb)
        {
            var margin = AppDefine.Main.Data.GetInt("record.margin.start");
            var now = DateTime.Now + new TimeSpan(0, 0, margin);

            tvdb.Sql = @"select * from reserve where tuner = '{0}' and status & {2} and start <= {1} order by start limit 1"
                .Formatex(tuner.Name, now.Ticks, (int)StatusCode.Enable);

            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    return new Reserve(t);
            }

            return null;
        }

        public static void UpdateReserveTime(Tvdb tvdb)
        {
            //予約のstart,end,durationをeventテーブルの値で更新
            //条件: 追従モードの予約
            tvdb.Sql = @"update reserve set
                        start = (select start from event where reserve.fsid = event.fsid and reserve.eid = event.eid),
                        end = (select end from event where reserve.fsid = event.fsid and reserve.eid = event.eid),
                        duration = (select duration from event where reserve.fsid = event.fsid and reserve.eid = event.eid)
                        where 
                        status & {0} > 0"
                        .Formatex((int)StatusCode.EventMode);
            tvdb.Execute();

            SetOverlay(tvdb);
        }

        //ステータスフラグをセット
        void SetStatus(Tvdb tvdb, StatusCode status, bool flag)
        {
            if (flag)
                tvdb.Sql = "update reserve set status = status | {0} where id = {1}".Formatex((int)status, Id);
            else
                tvdb.Sql = "update reserve set status = status & ~{0} where id = {1}".Formatex((int)status, Id);

            tvdb.Execute();
        }

        //有効/無効にセット
        public void SetEnable(Tvdb tvdb, bool flag)
        {
            SetStatus(tvdb, StatusCode.Enable, flag);
            SetOverlay(tvdb);
        }

        //待機中/録画中にセット
        public void SetRecoding(Tvdb tvdb, bool flag)
        {
            SetStatus(tvdb, StatusCode.Recoding, flag);
        }

        //完了にセット
        public void SetComplete(Tvdb tvdb)
        {
            SetEnable(tvdb, false);
            SetRecoding(tvdb, false);
            SetStatus(tvdb, StatusCode.Complete, true);
        }

        //重複判定をセット
        public static void SetOverlay(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                //重複フラグをクリアする
                tvdb.Sql = @"update reserve set status = status & ~{0}".Formatex((int)StatusCode.Overlay);
                tvdb.Execute();

                //自己結合して、チューナと時間の重なっている予約を取り出し重複フラグをセット
                tvdb.Sql = @"update reserve set status = status | {0}
                        where id in (
                        select r1.id from reserve r1
                        join reserve r2
                        on r1.id <> r2.id
                        where
                        r1.tuner = r2.tuner
                        and r1.start < r2.end
                        and r1.end > r2.start
                        and r1.status & {1}
                        and r2.status & {1})"
                            .Formatex((int)StatusCode.Overlay, (int)StatusCode.Enable);

                tvdb.Execute();
                tvdb.Commit();
            }
            catch 
            {
                tvdb.Rollback();
                throw;
            }
        }

        public void AddReserve(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                if (Id == -1)
                    Id = tvdb.GetNextId("reserve");
                else
                    Remove(tvdb, true);

                tvdb.Sql = @"insert into reserve values(
                        {0}, {1}, {2},
                        {3}, {4}, {5},
                        {6}, {7}, '{8}', '{9}');".Formatex(
                                Id,
                                Fsid,
                                Eid,

                                StartTime.Ticks,
                                EndTime.Ticks,
                                Duration,

                                Auto,
                                Status,
                                Tvdb.SqlEncode(Title),
                                Tvdb.SqlEncode(TunerName)
                                );
                tvdb.Execute();
            }
            finally
            {
                tvdb.Commit();
            }

            SetOverlay(tvdb);
        }
    }
}
