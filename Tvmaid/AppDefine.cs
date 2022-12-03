using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tvmaid
{
    class AppDefine
    {
        public static MainDefine Main;
        public static EpgWaitDefine EpgWait;
        public static TextConverter TextConverter;
        public static GenreConverter GenreConverter;

        public static void Load()
        {
            Main = new MainDefine();
            EpgWait = new EpgWaitDefine();
            TextConverter = new TextConverter();
            GenreConverter = new GenreConverter();

            Main.Load();
            EpgWait.Load();
            TextConverter.Load();
            GenreConverter.Load();
        }
    }

    //メイン設定
    public class MainDefine
    {
        PairList list;

        public PairList Data
        {
            get
            {
                return list;
            }
        }

        public void Load()
        {
            list = new PairList(Util.GetUserPath("main.def"));
            list.Load();
        }

        public void Save()
        {
            Check();
            list.Save();
        }

        public void Check()
        {
            Check1();
            Check2();
            Check3();
            Check4();
            Check5();
        }

        //定義されていないか「""」なら、デフォルト値をセット
        void SetDefault(string key, string defaultVal)
        {
            if (list.IsDefined(key) == false || list[key] == "")
                list[key] = defaultVal;
        }

        void Check5()
        {
            //epg.basic
            SetDefault("epg.basic", "");

            var val = list["epg.basic"];
            if (val != "")
            {
                var nidList = val.Split(new char[] { ',' });

                foreach (var nid in nidList)
                {
                    int num;
                    if (int.TryParse(nid, out num) == false)
                        throw new Exception("番組表 基本情報取得NIDが不正な値です。" + nid);
                }
            }
            Log.Info("番組表 基本情報取得NID: " + (val == "" ? "なし" : val));
        }

        void Check4()
        {
            //postprocess
            SetDefault("postprocess", "");

            var val = list["postprocess"];
            if (val != "" && File.Exists(val) == false)
                    throw new Exception("録画後プロセスが見つかりません。設定を確認してください。");

            Log.Info("録画後プロセス: " + (list["postprocess"] == "" ? "なし" : list["postprocess"]));

            //autosleep
            SetDefault("autosleep", "on");

            //録画後プロセスがあるときは強制的にOFF
            if (list["postprocess"] != "") list["autosleep"] = "off";

            Log.Info("自動スリープ: " + (list["autosleep"] == "on" ? "on" : "off"));

            //epgurl
            SetDefault("epgurl", "http://localhost:20001/maid/epg.html");

            Log.Info("番組表URL: " + list["epgurl"]);

            //url
            if (list.IsDefined("url") == false) list["url"] = "http://+:20000/";

        }

        void Check3()
        {
            //epg.hour
            SetDefault("epg.hour", "9");

            var data = list["epg.hour"];
            var hours = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var hour in hours)
            {
                int val;
                if (int.TryParse(hour, out val) == false)
                    throw new Exception("番組表更新時刻が不正な値です。" + hour);

                val = hour.ToInt();
                if (val < 0 || val > 23)
                    throw new Exception("番組表更新時刻が不正な値です。" + val);
            }

            Log.Info("番組表更新時刻: " + list["epg.hour"] + " 時");
        }

        void Check2()
        {
            //record.file
            SetDefault("record.file", "{title}-{start-yy}{start-MM}{start-dd}-{start-hh}{start-mm}.ts");
            Log.Info("録画ファイル: " + list["record.file"]);

            //record.margin.start
            SetDefault("record.margin.start", "10");
            list.GetInt("record.margin.start");
            Log.Info("開始マージン: " + list["record.margin.start"] + "秒");

            //record.margin.end
            SetDefault("record.margin.end", "10");
            list.GetInt("record.margin.end");
            Log.Info("終了マージン: " + list["record.margin.end"] + "秒");
        }

        void Check1()
        {
            //tvtest
            if (list.IsDefined("tvtest"))
            {
                var val = list["tvtest"];
                Log.Info("TVTest: " + val);

                if (File.Exists(val) == false)
                    throw new Exception("TVTestが見つかりません。設定を確認してください。");
            }
            else
                throw new Exception("TVTestのパスを設定してください。");

            //record.folder
            if (list.IsDefined("record.folder"))
            {
                var val = list["record.folder"];
                Log.Info("録画フォルダ: " + val);

                if (Directory.Exists(val) == false)
                    throw new Exception("録画フォルダが見つかりません。");
            }
            else
                throw new Exception("録画フォルダのパスを設定してください。");
        }
    }

    //番組表更新待ち時間設定
    class EpgWaitDefine
    {
        PairList list;

        public void Load()
        {
            list = new PairList(Util.GetUserPath("epgwait.def"));
            list.Load();
            Check();
        }

        void Check()
        {
            foreach (var pare in list)
            {
                int data;

                if (int.TryParse(pare.Value, out data) == false)
                    throw new Exception("番組表更新待ち時間の設定値が不正です。" + pare.Value);
            }
        }

        public int GetWait(int nid)
        {
            var wait = list[nid.ToString()];
            if (wait != null)
                return wait.ToInt();
            else
            {
                wait = list["default"];
                return wait == null ? 60 : wait.ToInt();
            }
        }
    }

    //テキストコンバータ
    //全角数字、全角アルファベット、半角カタカナ等を変換
    class TextConverter
    {
        PairList list;

        public void Load()
        {
            list = new PairList(Util.GetUserPath("convert.def"));
            list.Load();
        }

        public string Convert(string src)
        {
            var sb = new StringBuilder(src);

            foreach (var pair in list)
                sb = sb.Replace(pair.Key, pair.Value);

            return sb.ToString();
        }
    }

    //ジャンルID→テキストコンバータ
    class GenreConverter
    {
        Dictionary<int, string> genres = new Dictionary<int, string>();
        
        public void Load()
        {
            LoadFile(genres, "genre.def");
        }

        public void LoadFile(Dictionary<int, string> dic, string file)
        {
            var list = new PairList(Util.GetUserPath(file));
            list.Load();

            foreach (var pair in list)
            {
                int code = Convert.ToInt32(pair.Key, 16);
                dic[code] = pair.Value;
            }
        }

        public string GetText(long data)
        {
            var text = "";

            for (var i = 0; i < 6; i++)
            {
                var code = (int)((data >> (i * 8)) & 0xff);

                if (genres.ContainsKey(code))
                {
                    if (genres.ContainsKey(code))
                        text += genres[code] + "\n";
                }
            }

            return text;
        }
    }
}
