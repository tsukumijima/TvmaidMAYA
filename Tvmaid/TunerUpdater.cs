using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Tvmaid
{
    static class TunerUpdater
    {
        public static void Update()
        {
            using (var tvdb = new Tvdb(true))
            {
                Log.Info("チューナを更新しています...");
                UpdateTuner(tvdb);    //チューナ更新(DB更新)

                Log.Info("サービスを更新しています...");
                bool overlap = UpdateService(tvdb);

                Log.Info("余分なデータを削除しています...");
                Cleanup(tvdb);

                if (overlap)
                    MessageBox.Show("サービスが重複しています。\nこのままでも使用できますが、TVTestのチャンネルスキャンで同じ放送局を1つを残して他は無効(チェックを外す)にすることをおすすめします。", Program.Name);
            }

            Log.Info("チューナ更新が完了しました。");
        }

        static void Cleanup(Tvdb tvdb)
        {
            tvdb.BeginTrans();

            try
            {
                //以前から残っている番組で、新しくなったサービスにないものは削除する
                tvdb.Sql = "delete from event where fsid not in (select fsid from service group by fsid)";
                tvdb.Execute();

                //同ユーザ番組表
                tvdb.Sql = "delete from user_epg where fsid not in (select fsid from service group by fsid)";
                tvdb.Execute();

                //同予約
                tvdb.Sql = "delete from reserve where fsid not in (select fsid from service group by fsid)";
                tvdb.Execute();

                //予約でチューナがないものは削除
                tvdb.Sql = "delete from reserve where tuner not in (select name from tuner)";
                tvdb.Execute();
            }
            finally
            {
                tvdb.Commit();
            }
        }

        //チューナ更新
        static void UpdateTuner(Tvdb tvdb)
        {
            var def = new PairList(Util.GetUserPath("tuner.def"));
            def.Load();

            tvdb.Sql = "delete from tuner";
            tvdb.Execute();

            foreach (var pair in def)
            {
                var tuner = new Tuner(pair.Key, pair.Value);
                tuner.Add(tvdb);
            }
        }

        //サービス更新
        //戻り値: サービスの重複があるかどうか
        static bool UpdateService(Tvdb tvdb)
        {
            tvdb.Sql = "delete from service";
            tvdb.Execute();

            var tuners = new List<Tuner>();
            tvdb.Sql = "select * from tuner group by driver ";

            using (var table = tvdb.GetTable())
                while (table.Read())
                    tuners.Add(new Tuner(table));

            bool overlap = false;

            foreach (Tuner tuner in tuners)
            {
                TvServer server = null;

                try
                {
                    server = new TvServer(tuner);
                    server.Open();
                    overlap = GetServices(server, tvdb); //サービスをTVTestから読み込み
                }
                finally
                {
                    if (server != null)
                        server.Close();
                }
            }

            return overlap;
        }

        //サービスリスト更新
        static bool GetServices(TvServer server, Tvdb tvdb)
        {
            var list = server.GetServices();
            bool overlap = false;

            foreach (var service in list)
            {
                //fsidが0の場合は不正とみなして無視する
                //CATVがないのにBonDriverのChSet.txtでCATVが有効になっていると、このデータが返ってくる
                if (service.Fsid == 0) continue;

                tvdb.Sql = "select id from service where driver = '{0}' and fsid = {1}".Formatex(Tvdb.SqlEncode(service.Driver), service.Fsid);
                var id = tvdb.GetData();
                if (id == null)
                    service.Add(tvdb);
                else
                    overlap = true;
            }

            return overlap;
        }
    }
}
