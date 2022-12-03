using System;
using System.Net;
using System.Threading.Tasks;

namespace Tvmaid
{
    class WebServer
    {
        HttpListener listener = new HttpListener();
        bool stop = false;

        static WebServer server;

        public static void Run()
        {
            Task.Factory.StartNew(() =>
            {
                server = new WebServer();
                server.Start();

                System.Diagnostics.Debug.WriteLine("WebServer Exit");

            }, TaskCreationOptions.AttachedToParent);
        }

        public static void Stop()
        {
            if (server != null) server.Dispose();
        }

        WebServer() {}

        public void Start()
        {
            var prefix = AppDefine.Main.Data["url"];
            if (prefix == null) prefix = "http://+:20001/";

            Log.Info("URL: " + prefix);

            try
            {
                listener.Prefixes.Add(prefix);
                listener.Start();
            }
            catch (Exception ex)
            {
                var msg = "Webサーバの初期化に失敗しました。この状態では、ブラウザからアクセスできません。アプリケーションを終了してください。[詳細] " + ex.Message;
                Log.Error(msg);
                Log.Debug(ex.StackTrace);

                System.Windows.Forms.MessageBox.Show(msg, Program.Name);
                return;
            }

            while (stop == false)
            {
                try
                {
                    var context = listener.GetContext();
                    Task.Factory.StartNew(() =>
                    {
                        OnRequest(context);
                    }, TaskCreationOptions.AttachedToParent);
                }
                catch (HttpListenerException he)
                {
                    //終了時に出るエラーを回避
                    if (he.ErrorCode == 995) return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Debug(ex.StackTrace);
                }
            }
        }

        public void Dispose()
        {
            listener.Close();
            stop = true;
        }

        void OnRequest(HttpListenerContext con)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("web request... " + con.Request.Url.AbsolutePath);

                if (con.Request.HttpMethod != "GET" && con.Request.HttpMethod != "HEAD")
                {
                    con.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    con.Response.ContentLength64 = 0;
                    con.Response.OutputStream.Close();
                    return;
                }

                var uri = con.Request.Url.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                WebTask task;

                if (uri.Length == 0)
                    uri = new string[] { "" };

                switch (uri[0])
                {
                    case "webapi":
                        task = new WebApi(con);
                        break;
                    case "live-stream":
                        task = new WebPdLiveStream(con);
                        break;
                    case "record-stream":
                        task = new WebPdRecordStream(con);
                        break;
                    case "hls-playlist":
                        task = new WebHlsPlaylist(con);
                        break;
                    case "hls":
                        task = new WebHlsSegment(con);
                        break;
                    case "logo":
                        task = new WebLogo(con);
                        break;
                    default:
                        task = new WebFile(con);
                        break;
                }

                task.Run();
            }
            catch (Exception ex)
            {
                try
                {
                    con.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    con.Response.ContentLength64 = 0;
                    con.Response.OutputStream.Close();
                }
                catch { }

                Log.Error(ex.Message);
                Log.Debug(ex.StackTrace);
            }
        }
    }
}
     