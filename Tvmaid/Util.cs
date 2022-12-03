using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Tvmaid
{
    //拡張メソッド
    static class StringExtension
    {
        public static string Formatex(this string format, params object[] values)
        {
            return string.Format(format, values);
        }

        public static bool CompareNC(this string str1, string str2)
        {
            return string.Compare(str1, str2, true) == 0;
        }

        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }

        public static double ToDouble(this string s)
        {
            return double.Parse(s);
        }

        public static long ToLong(this string s)
        {
            return long.Parse(s);
        }

        public static DateTime ToDateTime(this string s)
        {
            return DateTime.Parse(s);
        }
    }

    //関数
    public static class Util
    {
        //実行ファイルのフォルダ
        public static string GetBasePath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static string GetBasePath(string file)
        {
            return Path.Combine(GetBasePath(), file);
        }

        //Webサーバルートフォルダ
        public static string GetWwwRootPath()
        {
            return Path.Combine(GetBasePath(), "wwwroot");
        }

        //ユーザ設定フォルダ
        public static string GetUserPath()
        {
            return Path.Combine(GetBasePath(), "user");
        }

        public static string GetUserPath(string file)
        {
            return Path.Combine(GetUserPath(), file);
        }

        public static string GetTempPath()
        {
            return Path.Combine(Path.GetTempPath(), "tvmaid");
        }

        //ユーザ設定ファイルがなければコピー
        public static void CopyUserFile()
        {
            var user = Util.GetUserPath();
            if (Directory.Exists(user) == false)
                Directory.CreateDirectory(user);

            var dir = new DirectoryInfo(Path.Combine(Util.GetBasePath(), "default"));
            var files = dir.GetFiles();

            foreach (var file in files)
            {
                var path = Path.Combine(user, file.Name);

                if (File.Exists(path) == false)
                    file.CopyTo(path);
            }
        }

        public static void CopyPlugin()
        {
            try
            {
                var src = Util.GetBasePath("TvmaidPlugin.tvtp");
                var tvtest = AppDefine.Main.Data["tvtest"];
                var dir = Path.Combine(Path.GetDirectoryName(tvtest), "Plugins");
                var dest = Path.Combine(dir, "TvmaidPlugin.tvtp");

                if (File.Exists(dest))
                {
                    if (File.GetLastWriteTime(src) == File.GetLastWriteTime(dest))
                    {
                        Log.Info("Tvmaidプラグイン OK");
                        return;
                    }
                }

                File.Copy(src, dest, true);
                Log.Info("Tvmaidプラグインを更新しました。");
            }
            catch (Exception ex)
            {
                throw new Exception("TVTestのプラグインフォルダに、Tvmaidプラグインをコピーできませんでした。[詳細]" + ex.Message);
            }
        }
    }

    //mutexを使いやすくしたクラス
    class MutexEx : IDisposable
    {
        Mutex mutex;

        public MutexEx(string name, int timeout)
        {
            try
            {
                mutex = new Mutex(false, name);

                if (mutex.WaitOne(timeout) == false)
                    throw new Exception("時間内に処理できませんでした。");
            }
            //放棄された場合(AbandonedMutexException)も正常終了とする
            catch (AbandonedMutexException) { }
        }

        public void Dispose()
        {
            try
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
            catch { }
        }
    }

    //キー=値のファイル
    public class PairList : List<KeyValuePair<string, string>>
    {
        string path;

        public PairList(string path)
        {
            this.path = path;
        }

        public int GetInt(string key)
        {
            return this[key].ToInt();
        }

        public float GetFloat(string key)
        {
            return (float)Convert.ToDouble(this[key]);
        }

        public string this[string key]
        {
            set
            {
                for (var i = 0; i < this.Count; i++)
                {
                    if (this[i].Key == key)
                    {
                        this[i] = new KeyValuePair<string, string>(key, value);
                        return;
                    }
                }
                this.Add(new KeyValuePair<string, string>(key, value));
            }
            get
            {
                foreach (var pair in this)
                    if (pair.Key == key) return pair.Value;

                return null;
            }
        }

        public void Save()
        {
            using (var sw = new StreamWriter(path, false, Encoding.GetEncoding("utf-8")))
            {
                foreach (var pair in this)
                    sw.WriteLine(pair.Key + "=" + pair.Value);
            }
        }

        public void Load()
        {
            using (var sr = new StreamReader(path, Encoding.GetEncoding("utf-8")))
            {
                var text = sr.ReadToEnd();
                string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    //コメント行
                    if (line.Length >= 2 && line.StartsWith("//")) { continue; }

                    int sepa = line.IndexOf('=');
                    if (sepa == -1) continue; 

                    var key = line.Substring(0, sepa);
                    var val = "";

                    if (sepa + 1 < line.Length)
                        val = line.Substring(sepa + 1);

                    this[key] = val;
                }
            }
        }

        public bool IsDefined(string key)
        {
            foreach (var pair in this)
                if (pair.Key == key) return true;

            return false;
        }
    }
}
