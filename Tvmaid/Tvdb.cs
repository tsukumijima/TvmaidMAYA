using System;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tvmaid
{
    [SQLiteFunction(Name = "regexp", Arguments = 3, FuncType = FunctionType.Scalar)]
    class SqlRegex : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            try
            {
                return Regex.IsMatch((string)args[0], (string)args[1], (RegexOptions)Convert.ToInt32(args[2]));
            }
            catch { return false; }
        }
    }

    public class Tvdb : Database
    {
        protected override string Path
        {
            get { return "tvmaid-5.db"; }
        }

        public Tvdb() { }
        public Tvdb(bool open) : base(open) { }
    }

    public abstract class Database : IDisposable
    {
        IDbCommand command = null;

        int transCount = 0;     //トランザクションカウンタ
        bool rollback = false;  //ロールバックフラグ(途中でRollBackがあった場合、その後CommitしてもRollBackする)

        protected abstract string Path { get; }

        public Database() { }

        public Database(bool open)
        {
            if (open) Open();
        }

        public void Open()
        {
            var builder = new SQLiteConnectionStringBuilder()
            {
                DataSource = System.IO.Path.Combine(Util.GetUserPath(), Path),
                Version = 3,
                LegacyFormat = false,
                SyncMode = SynchronizationModes.Normal,
                JournalMode = SQLiteJournalModeEnum.Wal,
                BusyTimeout = 60000
            };

            command = new SQLiteCommand();
            command.Connection = new SQLiteConnection(builder.ToString());
            command.Connection.Open();
        }

        public void Dispose()
        {
            command.Connection.Dispose();
            command.Dispose();
        }

        public string Sql
        {
            get { return command.CommandText; }
            set { command.CommandText = value; }
        }

        public void Execute()
        {
            command.ExecuteNonQuery();
        }

        public object GetData()
        {
            return command.ExecuteScalar();
        }

        public DbTable GetTable()
        {
            return new DbTable(command.ExecuteReader());
        }

        public void BeginTrans()
        {
            if (transCount == 0)
                command.Transaction = command.Connection.BeginTransaction();

            transCount++;
        }

        public void Rollback()
        {
            transCount--;

            if (transCount == 0)
                command.Transaction.Rollback();
            else
                rollback = true;
        }

        public void Commit()
        {
            transCount--;

            if (transCount == 0)
            {
                if (rollback)
                    command.Transaction.Rollback();
                else
                    command.Transaction.Commit();

                rollback = false;
            }
        }

        public static string SqlEncode(string text)
        {
            return text.Replace("'", "''");
        }
        
        public static string SqlLikeEncode(string text)
        {
            text = SqlEncode(text);

            text = text.Replace("^", "^^");
            text = text.Replace("_", "^_");
            text = text.Replace("%", "^%");

            return text;
        }
        
        public int GetNextId(string table)
        {
            Sql = "select max(id) as maxid from " + table;
            var id = GetData();
            return DBNull.Value.Equals(id) ? 0 : ((int)(long)id) + 1;
        }

        public List<object[]> GetList()
        {
            using (var table = GetTable())
            {
                var list = new List<object[]>();
                while (table.Read())
                {
                    var values = new object[table.FieldCount];
                    table.GetValues(values);
                    list.Add(values);
                }
                return list;
            }
        }
    }

    //結果テーブル
    public class DbTable : IDisposable
    {
        protected IDataReader reader = null;

        public DbTable(IDataReader dr)
        {
            reader = dr;
        }

        public string GetStr(int i)
        {
            return (string)reader[i];
        }

        public int GetInt(int i)
        {
            return (int)(long)reader[i];
        }

        public long GetLong(int i)
        {
            return (long)reader[i];
        }

        public string GetStr(string name)
        {
            return (string)reader[name];
        }

        public int GetInt(string name)
        {
            return (int)(long)reader[name];
        }

        public long GetLong(string name)
        {
            return (long)reader[name];
        }

        public bool Read()
        {
            return reader.Read();
        }

        public void Dispose()
        {
            reader.Dispose();
        }
        
        public bool IsNull(string name)
        {
            return reader.IsDBNull(reader.GetOrdinal(name));
        }
        
        public bool IsNull(int i)
        {
            return reader.IsDBNull(i);
        }

        public int FieldCount
        {
            get { return reader.FieldCount; }
        }

        internal void GetValues(object[] values)
        {
            reader.GetValues(values);
        }
    }
}
