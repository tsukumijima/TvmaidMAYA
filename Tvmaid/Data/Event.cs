using System;
using System.Collections.Generic;

namespace Tvmaid
{
    //番組
    class Event
    {
        public int Id = -1;
        public long Fsid;
        public int Eid;
        public DateTime Start;
        public int Duration;
        public string Title;
        public string Desc;
        public string LongDesc;
        public long Genre;
        public bool Pay;

        public DateTime End
        {
            get
            {
                return new DateTime(Start.Ticks + ((long)Duration * 10 * 1000 * 1000));
            }
        }

        public string GenreText
        {
            get
            {
                return AppDefine.GenreConverter.GetText(Genre);
            }
        }

        public int Week
        {
            get { return (int)Start.DayOfWeek; }
        }

        public Event() { }

        public Event(DbTable t)
        {
            Init(t);
        }

        void Init(DbTable t)
        {
            Id = t.GetInt("id");
            Fsid = t.GetLong("fsid");
            Eid = t.GetInt("eid");
            Start = new DateTime(t.GetLong("start"));
            Duration = t.GetInt("duration");
            Title = t.GetStr("title");
            Desc = t.GetStr("desc");
            LongDesc = t.GetStr("longdesc");
            Genre = t.GetLong("genre");
        }

        public Event(Tvdb tvdb, long fsid, int eid)
        {
            tvdb.Sql = "select * from event where fsid = {0} and eid = {1}".Formatex(fsid, eid);
            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("番組が見つかりません。");
            }
        }

        public Event(Tvdb tvdb, int id)
        {
            tvdb.Sql = "select * from event where id = " + id;
            using (var t = tvdb.GetTable())
            {
                if (t.Read())
                    Init(t);
                else
                    throw new Exception("番組が見つかりません。");
            }
        }

        void Remove(Tvdb tvdb)
        {
            RemoveById(tvdb, Id);
        }

        static void RemoveById(Tvdb tvdb, int id)
        {
            tvdb.Sql = "delete from event where id = " + id;
            tvdb.Execute();
        }

        public void Add(Tvdb tvdb)
        {
            try
            {
                tvdb.BeginTrans();

                if (Id == -1)
                    Id = tvdb.GetNextId("event");   //IDは常に増加させること
                else
                    Remove(tvdb);
                
                var conv = AppDefine.TextConverter;

                tvdb.Sql = @"insert into event values(
                        {0}, {1}, {2}, 
                        {3}, {4}, {5},
                        '{6}', '{7}', '{8}', {9}, {10},
                        {11}, '{12}');".Formatex(
                                Id,
                                Fsid,
                                Eid,

                                Start.Ticks,
                                End.Ticks,
                                Duration,

                                Tvdb.SqlEncode(conv.Convert(Title)),
                                Tvdb.SqlEncode(conv.Convert(Desc)),
                                Tvdb.SqlEncode(conv.Convert(LongDesc)),
                                Genre,
                                Pay ? 1: 0,

                                Week,
                                GenreText
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
