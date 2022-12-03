using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Tvmaid
{
    //ライブと録画の読み込みを共通化する
    abstract class VideoStreamReader : IDisposable
    {
        protected int ready = 0;

        public abstract int Read(byte[] buf);
        public abstract void Dispose();
        public bool Ready { get { return ready > 0; } } 
    }

    class LiveStreamReader : VideoStreamReader
    {
        TvServer server;
        LiveStream stream;

        public LiveStreamReader(string tunerName, long fsid)
        {
            Tuner tuner;
            Service service;

            try
            {
                using (var tvdb = new Tvdb(true))
                {
                    tuner = new Tuner(tvdb, tunerName);
                    service = new Service(tvdb, fsid);
                }

                server = new TvServer(tuner);
                server.Open(false);
                server.AddRef();
                server.SetService(service);

                stream = new LiveStream("/tvmaid/shared/stream/" + tuner.DriverId, fsid);

                Interlocked.Increment(ref ready);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public override int Read(byte[] buf)
        {
            if (server.IsOpen() == false)
                throw new Exception("TVTestが終了しました。");

            var watch = new Stopwatch();
            watch.Start();
            const int timeout = 20 * 1000;   //最大待ち時間

            while (true)
            {
                var count = stream.Read(buf);
                if (count > 0)
                    return count;
                else if (watch.ElapsedMilliseconds > timeout)
                    return 0;
                else
                    Thread.Sleep(50);
            }
        }

        public override void Dispose()
        {
            Interlocked.Decrement(ref ready);

            try
            {
                if (stream != null)
                    stream.Dispose();

                Thread.Sleep(2 * 1000);    //終了を遅らせる(すぐに次のリクエストがあるとTVTextが終了中になるため)

                if (server != null)
                {
                    server.RemoveRef();
                    server.Close();
                }
            }
            catch { }
        }
    }

    class RecordStreamReader : VideoStreamReader
    {
        FileStream stream;
        double duration;

        public RecordStreamReader(int id, int start)
        {
            Record rec;

            using (var tvdb = new Tvdb(true))
                rec = new Record(tvdb, id);

            var path = Path.Combine(AppDefine.Main.Data["record.folder"], rec.File);

            if (File.Exists(path) == false)
                throw new Exception("指定されたIDの録画ファイルはありません。");

            duration = (rec.End - rec.Start).TotalSeconds;

            if (start < 0 || start > duration)
                throw new Exception("record streamで無効な開始時間が指定されました。");

            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var pos = (long)Math.Floor(stream.Length / duration) * start;
            pos -= (pos % 188);

            stream.Seek(pos, SeekOrigin.Begin);

            Interlocked.Increment(ref ready);
        }

        public override int Read(byte[] buf)
        {
            return stream.Read(buf, 0, buf.Length);
        }

        public override void Dispose()
        {
            Interlocked.Decrement(ref ready);

            try
            {
                stream.Dispose();
            }
            catch { }
        }
    }
}
