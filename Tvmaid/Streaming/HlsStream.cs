using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Tvmaid
{
    //HLSストリーミング基本クラス
    abstract class HlsStream
    {
        //実行中ストリームのリスト
        protected static Dictionary<string, HlsStream> streams = new Dictionary<string, HlsStream>();

        protected HlsPlaylist playlist;
        protected bool stop = false;    //停止フラグ

        Stopwatch active = new Stopwatch(); //使用中かどうかのチェック

        //ストリームを取得
        public static HlsStream GetStream(string id)
        {
            lock (streams)
            {
                if (streams.ContainsKey(id))
                {
                    streams[id].active.Restart();   //アクセスされたので更新
                    return streams[id];
                }

                return null;
            }
        }

        //ライブストリームスタート
        public static void Start(string streamId, string tuner, long fsid, string mode)
        {
            Start(streamId, new HlsLiveStream(tuner, fsid, mode));
        }

        //録画ストリームスタート
        public static void Start(string streamId, int id, int start, string mode)
        {
            if (streams.ContainsKey(streamId) && streams[streamId].Seekable(start))
                streams[streamId].Seek(start);
            else
                Start(streamId, new HlsRecordStream(id, start, mode));
        }

        static void Start(string streamId, HlsStream stream)
        {
            lock (streams)
            {
                if (streams.ContainsKey(streamId))
                {
                    streams[streamId].Stop();
                    streams[streamId] = stream;
                }
                else
                    streams.Add(streamId, stream);
            }

            Task.Factory.StartNew(stream.Run, TaskCreationOptions.AttachedToParent);
            Task.Factory.StartNew(stream.CheckActive, TaskCreationOptions.AttachedToParent);
        }

        public static void Stop(string id)
        {
            lock (streams)
            {
                if (streams.ContainsKey(id))
                {
                    streams[id].Stop();
                    streams.Remove(id);
                }
            }
        }

        public static void StopAll()
        {
            foreach (var stream in streams)
                stream.Value.Stop();

            streams.Clear();
        }

        //セグメントファイルをすべて削除
        public static void ClearSegment()
        {
            try
            {
                if (Directory.Exists(Util.GetTempPath()))
                    foreach (var file in Directory.GetFiles(Util.GetTempPath()))
                        File.Delete(file);
            }
            catch { }
        }

        protected abstract void Run();

        //使用されているかチェック
        void CheckActive()
        {
            active.Start();

            while (stop == false)
            {
                Thread.Sleep(1000);

                if (active.ElapsedMilliseconds > 30 * 1000)
                {
                    Debug.WriteLine("一定時間ストリームが使用されていないので終了しました。");
                    Stop();
                }
            }
        }

        public int GetPlaylistCount { get { return playlist.Count; } }
        public bool IsStop { get { return stop; } }

        public string GetPlayList()
        {
            return playlist.GetList();
        }

        void Stop()
        {
            stop = true;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5 * 1000);    //しばらく待ってからセグメントを削除
                RemoveSegment();

            }, TaskCreationOptions.AttachedToParent);
        }

        //ストリームのセグメントファイルを削除
        void RemoveSegment()
        {
            playlist.GetSegmentNames(name =>
            {
                var path = Path.Combine(Util.GetTempPath(), name);
                try
                {
                    File.Delete(path);
                }
                catch { }
            });
        }

        protected virtual bool Seekable(int pos)
        {
            return false;
        }

        protected virtual void Seek(int pos) { }
    }

    abstract class HlsStreamEncode : HlsStream
    {
        protected VideoStreamReader reader;
        HlsEncoder encoder;

        protected override void Run() { }

        protected void Encode(string mode)
        {
            try
            {
                encoder = new HlsEncoder();
                encoder.Open(mode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("エンコーダが起動できません... " + ex.Message);
                Log.Error(ex.Message);
                Dispose();
                return;
            }

            //エンコーダへデータを送信
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    var buf = new byte[188 * 1024];

                    while (stop == false)
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
                    stop = true;
                    Debug.WriteLine("ライブ→エンコーダでエラーが発生... " + ex.Message);
                }

                encoder.EndWrite();

            }, TaskCreationOptions.AttachedToParent);

            //エンコーダの出力をプレイリストに書き込み
            try
            {
                while (true)
                {
                    var line = encoder.ReadLine();

                    if (line != null)
                        playlist.Load(line);
                    else
                        break;
                }
            }
            catch (Exception ex)
            {
                stop = true;
                Debug.WriteLine("エンコーダ→レスポンスでエラーが発生... " + ex.Message);
            }

            task.Wait(3000);
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

    class HlsLiveStream : HlsStreamEncode
    {
        string tunerName;
        long fsid;
        string mode;

        public HlsLiveStream(string tuner, long fsid, string mode)
        {
            this.tunerName = tuner;
            this.fsid = fsid;
            this.mode = mode;
            playlist = new HlsLivePlaylist();
        }

        //このメソッドはスレッドで起動されるので、例外をすべてcatchすること
        protected override void Run()
        {
            Debug.WriteLine("hls live request.");

            try
            {
                reader = new LiveStreamReader(tunerName, fsid);
                Encode(mode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("エンコードに失敗しました... " + ex.Message);
                Log.Error(ex.Message);
            }
        }
    }

    class HlsRecordStream : HlsStreamEncode
    {
        int id;
        int start;
        string mode;

        public HlsRecordStream(int id, int start, string mode)
        {
            this.id = id;
            this.start = start;
            this.mode = mode;
            playlist = new HlsRecordPlaylist();
        }

        protected override bool Seekable(int pos)
        {
            var list = (HlsRecordPlaylist)playlist;
            return pos - start >= 0 && list.Seekable(pos - start);
        }

        protected override void Seek(int pos)
        {
            var list = (HlsRecordPlaylist)playlist;
            list.Seek(pos - start);
        }

        //このメソッドはスレッドで起動されるので、例外をすべてcatchすること
        protected override void Run()
        {
            Debug.WriteLine("hls record request.");

            try
            {
                reader = new RecordStreamReader(id, start);
                Encode(mode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("エンコードに失敗しました... " + ex.Message);
                Log.Error(ex.Message);
            }
        }
    }

    //プレイリストを送信
    class WebHlsPlaylist : WebTask
    {
        public WebHlsPlaylist(HttpListenerContext con) : base(con) { }

        public override void Run()
        {
            var stream = HlsStream.GetStream(GetQuery("stream"));

            if (stream == null)
                throw new WebException(HttpStatusCode.NotFound);

            var data = System.Text.Encoding.UTF8.GetBytes(stream.GetPlayList());

            con.Response.SendChunked = false;
            con.Response.ContentLength64 = data.Length;
            con.Response.ContentType = "application/x-mpegURL";
            con.Response.StatusCode = (int)HttpStatusCode.OK;
            con.Response.OutputStream.Write(data, 0, data.Length);
        }
    }

    //セグメントを送信
    class WebHlsSegment : WebFile
    {
        public WebHlsSegment(HttpListenerContext con) : base(con) { }

        public override void Run()
        {
            var path = con.Request.Url.AbsolutePath.Replace("/hls/", Util.GetTempPath() + '\\');
            SendFile(path);
        }

        protected override string GetContentType(string path)
        {
            return "video/MP2T";
        }
    }
}
