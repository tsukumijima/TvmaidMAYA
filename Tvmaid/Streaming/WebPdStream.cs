using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Tvmaid
{
    //PDストリーミング基本クラス
    abstract class WebPdStream : WebTask
    {
        protected VideoStreamReader reader;
        PdEncoder encoder;

        public WebPdStream(HttpListenerContext con) : base(con) { }

        public override void Run() { }

        protected void Encode(string mode)
        {
            try
            {
                encoder = new PdEncoder();
                encoder.Open(mode);
                con.Response.ContentType = encoder.MediaType;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("エンコーダが起動できません... " + ex.Message);
                Log.Error(ex.Message);
                Dispose();
                return;
            }

            var stop = false;
            var tasks = new Task[2];

            //エンコーダへデータを送信
            tasks[0] = Task.Factory.StartNew(() =>
            {
                try
                {
                    var buf = new byte[188 * 1024];

                    while (stop == false && encoder.Ready)
                    {
                        var count = reader.Read(buf);

                        if (count > 0)
                            encoder.Write(buf, count);
                        else
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ライブ→エンコーダでエラーが発生... " + ex.Message);
                }

                encoder.EndWrite();

            }, TaskCreationOptions.AttachedToParent);

            //エンコーダからキューへ格納
            const int queueSize = 32;
            var queue = new Queue<byte[]>(queueSize);   //エンコードデータを溜めるキュー
            bool finish = false;    //エンコード完了フラグ

            tasks[1] = Task.Factory.StartNew(() =>
            {
                try
                {
                    while (stop == false && encoder.Ready)
                    {
                        if (queue.Count < queueSize)
                        {
                            var bufsize = 64 * 1024;
                            var data =  encoder.Read(bufsize);

                            if (data.Length > 0)
                                queue.Enqueue(data);
                            else
                                break;
                        }
                        else
                        {
                            encoder.Read(); //キューがいっぱいのときに、エンコーダが終了しないようにする
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    stop = true;
                    Debug.WriteLine("エンコーダ→キューでエラーが発生... " + ex.Message);
                }

                finish = true;

            }, TaskCreationOptions.AttachedToParent);

            //キューからブラウザへ送信
            try
            {
                con.Response.SendChunked = true;

                while (stop == false && encoder.Ready)
                {
                    if (queue.Count > 0)
                    {
                        var data = queue.Dequeue();
                        con.Response.OutputStream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        if (finish)
                            break;
                        else
                            Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                stop = true;
                Debug.WriteLine("キュー→レスポンスでエラーが発生... " + ex.Message);
            }

            Task.WaitAll(tasks, 3000);
            Dispose();
        }

        void Dispose()
        {
            if (encoder != null)
                encoder.Dispose();

            if (reader != null)
                reader.Dispose();
        }
    }

    class WebPdLiveStream : WebPdStream
    {
        public WebPdLiveStream(HttpListenerContext con) : base(con) { }

        public override void Run()
        {
            Debug.WriteLine("live stream request.");

            var tunerName = GetQuery("tuner");
            var fsid = GetQuery("fsid").ToLong();
            var mode = GetQuery("mode");

            reader = new LiveStreamReader(tunerName, fsid);
            Encode(mode);
        }
    }

    class WebPdRecordStream : WebPdStream
    {
        public WebPdRecordStream(HttpListenerContext con) : base(con) { }

        public override void Run()
        {
            Debug.WriteLine("record stream request.");

            var id = GetQuery("id").ToInt();
            var start = GetQuery("start").ToInt();
            var mode = GetQuery("mode");

            reader = new RecordStreamReader(id, start);
            Encode(mode);
        }
    }
}
