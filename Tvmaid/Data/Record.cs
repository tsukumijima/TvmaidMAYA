using System;

namespace Tvmaid
{
    //録画
    class Record
    {
        public int Id = -1;             //-1: 新規
        public string Title;
        public string ServiceName;
        public string File;

        public DateTime Start;
        public DateTime End;
        public DateTime ReserveStart;
        public DateTime ReserveEnd;

        public int Code = 0;
        public int Error = 0;
        public int Drop = 0;
        public int Scramble = 0;
        public string Message = "";

        public string Desc = "";
        public string LongDesc = "";
        public string GenreText = "";

        public int Status = 0;
        public string Auto = "";

        public enum StatusCode
        {
            Delete = 1
        };

        public Record() { }

        public Record(Tvdb tvdb, int id)
        {
            tvdb.Sql = "select * from record where id = " + id;
            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("録画が見つかりません。");
            }
        }

        void Init(DbTable t)
        {
            Id = t.GetInt("id");

            Start = new DateTime(t.GetLong("start"));
            End = new DateTime(t.GetLong("end"));

            Title = t.GetStr("title");
            File = t.GetStr("file");
        }

        //削除
        public void Remove(Tvdb tvdb)
        {
            tvdb.Sql = "delete from record where id = " + Id;
            tvdb.Execute();
        }

        //追加
        public void Add(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                if (Id == -1)
                    Id = tvdb.GetNextId("record");

                tvdb.Sql = @"insert into record values(
                            {0}, '{1}', '{2}', '{3}',
                            {4}, {5}, {6}, {7},
                            {8}, {9}, {10}, {11}, '{12}',
                            '{13}', '{14}', '{15}',
                            {16}, '{17}');".Formatex(
                                Id,
                                Tvdb.SqlEncode(Title),
                                Tvdb.SqlEncode(ServiceName),
                                Tvdb.SqlEncode(File),

                                Start.Ticks,
                                End.Ticks,
                                ReserveStart.Ticks,
                                ReserveEnd.Ticks,

                                Code,
                                Error,
                                Drop,
                                Scramble,
                                Tvdb.SqlEncode(Message),

                                Tvdb.SqlEncode(Desc),
                                Tvdb.SqlEncode(LongDesc),
                                Tvdb.SqlEncode(GenreText),

                                Status,
                                Tvdb.SqlEncode(Auto));
                tvdb.Execute();
            }
            finally
            {
                tvdb.Commit();
            }
        }
    }
}
