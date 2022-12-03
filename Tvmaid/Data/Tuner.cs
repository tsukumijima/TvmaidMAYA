using System;

namespace Tvmaid
{
    //チューナ
    class Tuner
    {
        public int Id;
        public string Name;
        public string DriverPath;
        public int DriverIndex;

        public string Driver
        {
            get
            {
                return System.IO.Path.GetFileName(DriverPath);
            }
        }

        public string DriverId
        {
            get
            {
                return "{0}/{1}".Formatex(Driver, DriverIndex);
            }
        }

        public Tuner(string name, string driverPath) 
        {
            Id = -1;
            Name = name;
            DriverPath = driverPath;
            DriverIndex = 0;
        }

        public Tuner(DbTable t)
        {
            Init(t);
        }

        void Init(DbTable t)
        {
            Id = t.GetInt("id");
            Name = t.GetStr("name");
            DriverPath = t.GetStr("driver_path");
            DriverIndex = t.GetInt("driver_index");
        }

        public Tuner(Tvdb tvdb, string name)
        {
            tvdb.Sql = @"select * from tuner where name = '{0}'".Formatex(Tvdb.SqlEncode(name));
            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("チューナがありません。" + name);
            }
        }

        public void Add(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                if (Id == -1)
                    Id = tvdb.GetNextId("tuner");

                tvdb.Sql = "select count(id) from tuner where driver = '" + Tvdb.SqlEncode(Driver) + "'";
                var index = tvdb.GetData();
                DriverIndex = (int)(long)index;

                tvdb.Sql = "insert into tuner values({0}, '{1}', '{2}', '{3}', {4});".Formatex(
                            Id,
                            Tvdb.SqlEncode(Name),
                            Tvdb.SqlEncode(DriverPath),
                            Tvdb.SqlEncode(Driver),
                            DriverIndex
                            );

                tvdb.Execute();
            }
            finally
            {
                tvdb.Commit();
            }
        }
    }
}
