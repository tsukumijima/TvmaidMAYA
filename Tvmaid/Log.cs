using System;
using System.Collections.Generic;

namespace Tvmaid
{
    static class Log
    {
        static Ldb ldb;

        public static void Open()
        {
            ldb = new Ldb(true);
        }

        public static void Close()
        {
            ldb.Dispose();
        }

        public static void EpgUpdate(string text)
        {
            Write(text, 4);
        }

        public static void Error(string text)
        {
            Write(text, 3);
        }

        public static void Alert(string text)
        {
            Write(text, 2);
        }

        public static void Info(string text)
        {
            Write(text, 1);
        }

        public static void Debug(string text)
        {
            Write(text, 0);
        }

        static void Write(string text, int type)
        {
            lock (ldb)
            {
                ldb.Sql = "insert into log values ({0}, {1}, '{2}')".Formatex(DateTime.Now.Ticks, type, Ldb.SqlEncode(text));
                ldb.Execute();
            }
        }
    }

    public class Ldb : Database
    {
        protected override string Path
        {
            get { return "log.db"; }
        }

        public Ldb() { }

        public Ldb(bool open) : base(open) { }
    }
}
