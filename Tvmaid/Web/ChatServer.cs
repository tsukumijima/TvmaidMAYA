using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Tvmaid.Chat
{
    //コメント
    //DynamicJsonでシリアライズできるように、メンバーはプロパティにする
    class Chat
    {
        public int time { get; set; }
        public string text { get; set; }
    }

    //ChatServer基本クラス
    abstract class ChatServer : IDisposable
    {
        public abstract void Open(long fsid);
        public abstract List<Chat> GetChat(int time, int max);  //前回取得時からtime(unix時間)までを取得(最大max個まで)
        public abstract void Dispose();
        
        public Stopwatch Active = new Stopwatch();
    }

    //cookieを使えるようにしたWebClient
    class WebClientEx : WebClient
    {
        CookieContainer cookie = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);

            if (req is HttpWebRequest)
                (req as HttpWebRequest).CookieContainer = cookie;

            return req;
        }
    }

    //ニコニコ実況基本クラス
    abstract class NiconicoServer : ChatServer
    {
        protected static WebClientEx client = new WebClientEx() { Encoding = Encoding.UTF8 };
        protected string server;
        protected string port;
        protected string thread;
        protected int lastno = -1;  //取得した最後の番号

        public static int ToUnixTime(DateTime time)
        {
            return (int)new DateTimeOffset(time).ToUnixTimeSeconds();
        }

        public static DateTime ToDateTime(int time)
        {
            return DateTimeOffset.FromUnixTimeSeconds(time).ToLocalTime().DateTime;
        }

        public override void Dispose() { }

        //ニコニコ実況のチャンネルを取得
        protected int GetChannel(long fsid)
        {
            string text;

            using (var sr = new StreamReader(Util.GetUserPath("niconico-ch.def")))
                text = sr.ReadToEnd();

            var nid = fsid >> 32 & 0xffff;
            var sid = fsid & 0xffff;
            nid = (nid == 4 || nid == 6 || nid == 7) ? nid : 15;

            foreach (var line in text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var ch = line.Split(new char[] { '\t' });

                if (ch[1].ToInt() == nid && ch[2].ToInt() == sid)
                    return ch[0].ToInt();
            }

            return -1;
        }

        protected string Match(string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        //コメントを取得するURLを派生クラスで取得
        protected abstract string GetDownloadUrl(int time, int count);

        public override List<Chat> GetChat(int time, int max)
        {
            if (server == null || port == null || thread == null)
                throw new Exception("初期化エラーです。");

            var res = client.DownloadString(GetDownloadUrl(time, 0));
            var data = DynamicJson.Parse(res);

            if (data[0].IsDefined("thread") == false)
                throw new Exception("スレッド情報が取得できませんでした。");

            if (data[0].thread.IsDefined("last_res") == false)
                throw new Exception("最終レス番号が取得できませんでした。");

            if (lastno == -1)
            {
                lastno = (int)data[0].thread.last_res;
                return new List<Chat>();
            }

            var count = (int)data[0].thread.last_res - lastno;
            count = count < max ? count : max;
            count = count > 0 ? count : 0;

            res = client.DownloadString(GetDownloadUrl(time, -count));
            data = DynamicJson.Parse(res);

            var list = new List<Chat>();

            foreach (var line in data)
            {
                if (line.IsDefined("thread"))
                {
                    lastno = (int)line.thread.last_res;
                    continue;
                }

                if (line.IsDefined("chat"))
                {
                    var chat = new Chat()
                    {
                        text = line.chat.content,
                        time = (int)line.chat.date
                    };
                    list.Add(chat);
                }
            }

            return list;
        }
    }

    //ニコニコ実況ライブ
    class NiconicoLiveServer : NiconicoServer
    {
        public override void Open(long fsid)
        {
            var ch = GetChannel(fsid);

            if (ch == -1)
                throw new Exception("対応するニコニコ実況チャンネルがありません。");

            var url = "http://jk.nicovideo.jp/api/v2/getflv?v=jk{0}".Formatex(ch);
            var res = client.DownloadString(url);

            server = Match(res, "&ms=(.+?)&");
            port = Match(res, "&http_port=(.+?)&");
            thread = Match(res, "&thread_id=(.+?)&");

            if (server == null || port == null || thread == null)
                throw new Exception("ニコニコ実況サーバURLが取得できませんでした。");
        }

        protected override string GetDownloadUrl(int time, int count)
        {
            var baseUrl = "http://{0}:{1}/api.json/thread?thread={2}&version=20061206&res_from={3}";
            return baseUrl.Formatex(server, port, thread, count);
        }
    }

    //ニコニコ実況過去ログ
    class NiconicoLogServer : NiconicoServer
    {
        static bool login = false;

        string mail;
        string password;
        int starttime;
        int endtime;

        string waybackkey;
        string user;

        public NiconicoLogServer(string mail, string password, int start, int end)
        {
            if (mail == "" || password == "")
                throw new Exception("メールアドレスまたはパスワードが設定されていません。");

            this.mail = mail;
            this.password = password;
            starttime = start;
            endtime = end;
        }

        void Login()
        {
            client.UploadValues("https://secure.nicovideo.jp/secure/login?site=niconico", new NameValueCollection
            {
                {"next_url", ""},
                {"mail", mail },
                {"password", password }
            });

            login = true;
        }

        public override void Open(long fsid)
        {
            lock (client)
                if (login == false)
                    Login();

            var ch = GetChannel(fsid);

            if (ch == -1)
                throw new Exception("対応するニコニコ実況チャンネルがありません。");

            var url = "http://jk.nicovideo.jp/api/v2/getflv?v=jk{0}&start_time={1}&end_time={2}"
                .Formatex(ch, starttime, endtime);

            var res = client.DownloadString(url);

            user = Match(res, "&user_id=(.+?)&");

            if (user == null)
            {
                login = false;
                throw new Exception("ニコニコ動画にログインできませんでした。");
            }

            server = Match(res, "&ms=(.+?)&");
            port = Match(res, "&http_port=(.+?)&");
            thread = Match(res, "&thread_id=(.+?)&");

            url = "http://jk.nicovideo.jp/api/v2/getwaybackkey?thread={0}".Formatex(thread);
            res = client.DownloadString(url);
            waybackkey = Match(res, "waybackkey=(.+?)$");

            if (server == null || port == null || thread == null || waybackkey == null)
                throw new Exception("ニコニコ実況サーバURLが取得できませんでした。");
        }

        void Logout()
        {
            client.DownloadString("https://secure.nicovideo.jp/secure/logout");
        }

        protected override string GetDownloadUrl(int time, int count)
        {
            var baseUrl = "http://{0}:{1}/api.json/thread?thread={2}&version=20061206&when={3}&waybackkey={4}&user_id={5}&res_from={6}";
            return baseUrl.Formatex(server, port, thread, time, waybackkey, user, count);
        }
    }
}
