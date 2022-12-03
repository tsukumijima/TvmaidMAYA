
using System;

namespace Tvmaid
{
    //サービス
    class Service
    {
        public int Id = -1;
        public string Driver;
        public int Nid;
        public int Tsid;
        public int Sid;
        public string Name;

        public bool EpgBasic = false; //番組表更新時にnid単位で取得するかどうかのフラグ

        public Service() { }

        public Service(DbTable t)
        {
            Init(t);
        }

        void Init(DbTable t)
        {
            Id = t.GetInt("id");
            Driver = t.GetStr("driver");
            Fsid = t.GetLong("fsid");
            Name = t.GetStr("name");
        }

        public long Fsid
        {
            get
            {
                return ((long)Nid << 32) + (Tsid << 16) + Sid;
            }
            set
            {
                Nid = (int)(value >> 32 & 0xffff);
                Tsid = (int)(value >> 16 & 0xffff);
                Sid = (int)(value & 0xffff);
            }
        }

        public Service(Tvdb tvdb, long fsid)
        {
            tvdb.Sql = @"select * from service where fsid = {0}".Formatex(fsid);
            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("サービスが見つかりません。" + fsid);
            }
        }

        public Service(Tvdb tvdb, int id)
        {
            tvdb.Sql = "select * from service where id = " + id;
            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("サービスが見つかりません。");
            }
        }

        public void Add(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                if (Id == -1)
                    Id = tvdb.GetNextId("service");

                tvdb.Sql = @"insert into service values({0}, '{1}', {2}, '{3}');".Formatex(
                            Id,
                            Tvdb.SqlEncode(Driver),
                            Fsid,
                            Tvdb.SqlEncode(Name)
                            );

                tvdb.Execute();
            }
            finally
            {
                tvdb.Commit();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
