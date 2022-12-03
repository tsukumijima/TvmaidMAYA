using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Tvmaid
{
    abstract class WebTask
    {
        protected HttpListenerContext con;
        protected NameValueCollection query = new NameValueCollection();

        public abstract void Run();

        public WebTask(HttpListenerContext con)
        {
            this.con = con;
            GetQueryList();
        }

        protected virtual string GetContentType(string path)
        {
            var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(Path.GetExtension(path));
            object val = null;
            if (key != null) val = key.GetValue("Content Type");

            return (key == null || val == null) ? System.Net.Mime.MediaTypeNames.Application.Octet : val.ToString();
        }

        protected void Close(HttpStatusCode code)
        {
            con.Response.StatusCode = (int)code;
            con.Response.ContentLength64 = 0;
            con.Response.OutputStream.Close();
        }

        void GetQueryList()
        {
            var i = con.Request.RawUrl.IndexOf('?');
            if (i != -1)
            {
                var q = con.Request.RawUrl.Substring(i);
                query = System.Web.HttpUtility.ParseQueryString(q);
            }
        }

        protected string GetQuery(string name)
        {
            if (query[name] == null)
                throw new Exception("必須のパラメータがありません。 - " + name);

            return query[name];
        }

        protected string GetQuery(string name, string defaultVal)
        {
            return query[name] == null ? defaultVal : query[name];
        }

        protected int GetQuery(string name, int defaultVal)
        {
            return query[name] == null ? defaultVal : query[name].ToInt();
        }

        protected long GetQuery(string name, long defaultVal)
        {
            return query[name] == null ? defaultVal : query[name].ToLong();
        }
    }

    class WebFile : WebTask
    {
        public WebFile(HttpListenerContext con) : base(con) { }

        public override void Run()
        {
            var url = Uri.UnescapeDataString(con.Request.Url.AbsolutePath);
            var path = Util.GetWwwRootPath() + url;
            SendFile(path.Replace('/', '\\'));
        }

        protected void SendFile(string path)
        {
            try
            {
                Send(path);
            }
            catch (WebException wex)
            {
                Close(wex.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error("ファイルの送信に失敗しました。" + ex.Message);

                Close(HttpStatusCode.InternalServerError);
            }
        }

        void Send(string path)
        {
            con.Response.SendChunked = false;

            if (File.Exists(path) == false)
                throw new WebException(HttpStatusCode.NotFound);

            con.Response.ContentType = GetContentType(path);
            con.Response.Headers["Accept-Ranges"] = "bytes";

            var writeTime = File.GetLastWriteTimeUtc(path).ToString("r");
            con.Response.Headers["Last-Modified"] = writeTime;

            var etag = GetMD5(path + writeTime);
            con.Response.Headers["ETag"] = etag;

            //範囲
            long start, end;
            GetSendRange(path, etag, out start, out end);

            con.Response.ContentLength64 = end - start + 1; 

            //ファイル変更なし
            var ifNoneMatch = con.Request.Headers["If-None-Match"];
            if (ifNoneMatch != null && ifNoneMatch == etag)
            {
                Close(HttpStatusCode.NotModified);
                return;
            }

            //ファイル未更新
            var ifModified = con.Request.Headers["If-Modified-Since"];
            if (ifModified != null)
            {
                DateTime time;
                if (DateTime.TryParse(ifModified, out time))
                {
                    if (time.Ticks / 10000000 == File.GetLastWriteTime(path).Ticks / 10000000)
                    {
                        Close(HttpStatusCode.NotModified);
                        return;
                    }
                }
            }

            //ヘッダのみ送信
            //HEADのときは、ContentLength64を付けていても問題ない
            if (con.Request.HttpMethod == "HEAD")
                Close(HttpStatusCode.OK);
            else
                Write(path, start, end);
        }

        protected string GetMD5(string src)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(src);

            using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash);
            }
        }

        void GetSendRange(string path, string etag, out long start, out long end)
        {
            var info = new FileInfo(path);
            start = 0;
            end = info.Length - 1;
            var range = con.Request.Headers["Range"];

            //ファイルに変更があれば全体、なければ範囲
            var ifRange = con.Request.Headers["If-Range"];
            if (ifRange != null && ifRange != etag)
                return; //ETagが違うので全体

            if (range == null)
                return;

            var regex = new Regex(@"bytes=(?<start>\d*)-(?<end>\d*)");
            var match = regex.Matches(range);

            start = long.TryParse(match[0].Groups["start"].Value, out start) ? start : 0;
            end = long.TryParse(match[0].Groups["end"].Value, out end) ? end : info.Length - 1;

            //要求が大きすぎる場合
            if ((end - start) > info.Length)
                throw new WebException(HttpStatusCode.RequestedRangeNotSatisfiable);

            con.Response.Headers["Content-Range"] = "bytes {0}-{1}/{2}".Formatex(start, end, info.Length);
            con.Response.StatusCode = (int)HttpStatusCode.PartialContent;
        }

        void Write(string path, long start, long end)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var remain = end - start + 1;

                fs.Seek(start, SeekOrigin.Begin);

                const int bufsize = 64 * 1024;
                var buf = new byte[bufsize];

                while (true)
                {
                    var size = buf.Length > remain ? (int)remain : buf.Length;
                    size = fs.Read(buf, 0, size);
                    remain -= size;

                    try
                    {
                        con.Response.OutputStream.Write(buf, 0, size);
                    }
                    catch 
                    {
                        return;
                    }

                    if (remain <= 0) break;
                    if (size == 0) throw new Exception("ファイル読み込みが中断しました。");
                }
            }
        }
    }

    class WebException : Exception
    {
        public WebException(HttpStatusCode code)
        {
            StatusCode = code;
        }

        public HttpStatusCode StatusCode { get; set; }
    }

    class WebLogo : WebFile
    {
        public WebLogo(HttpListenerContext con) : base(con) { }

        public override void Run()
        {
            var url = con.Request.Url.AbsolutePath;
            var path = Util.GetUserPath() + url;
            SendFile(path);
        }
    }
}
