using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tvmaid
{
    //エンコーダ基本クラス
    abstract class Encoder : IDisposable
    {
        Process process = new Process();
        BinaryReader reader;
        BinaryWriter writer;
        int ready = 0;
        Stopwatch readerWatch = new Stopwatch();
        Stopwatch writerWatch = new Stopwatch();

        protected void Open(string encoder, string option, bool window, string workDir)
        {
            var p = new Process();
            p.StartInfo.WorkingDirectory = workDir;
            p.StartInfo.FileName = encoder;
            p.StartInfo.Arguments = option;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = window;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.Start();
            process = p;

            reader = new BinaryReader(p.StandardOutput.BaseStream);
            writer = new BinaryWriter(p.StandardInput.BaseStream);
            Interlocked.Increment(ref ready);

            //エンコーダの変換が止まってないか監視するスレッド
            Task.Factory.StartNew(() =>
            {
                const int timecout = 30 * 1000;

                readerWatch.Start();
                writerWatch.Start();

                while (ready > 0)
                {
                    if ((writer != null && writerWatch.ElapsedMilliseconds > timecout) || readerWatch.ElapsedMilliseconds > timecout)
                    {
                        Log.Debug("エンコーダの応答がないため停止します。");
                        Dispose();
                        break;
                    }
                    Thread.Sleep(500);
                }
            }, TaskCreationOptions.AttachedToParent);
        }

        void ErrorCheck()
        {
            if (Ready == false)
                throw new Exception("エンコーダの準備ができていません。");
        }

        public void Write(byte[] buf, int count)
        {
            ErrorCheck();
            writerWatch.Restart();
            writer.Write(buf, 0, count);
        }

        public void Read()
        {
            ErrorCheck();
            readerWatch.Restart();
        }

        public byte[] Read(int size)
        {
            ErrorCheck();
            readerWatch.Restart();
            return reader.ReadBytes(size);
        }

        public string ReadLine()
        {
            ErrorCheck();
            readerWatch.Restart();
            return process.StandardOutput.ReadLine();
        }

        //書き込み終了
        public void EndWrite()
        {
            if (writer != null) writer.Close();
            writer = null;
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref ready);

            try
            {
                if (reader != null) reader.Close();
                if (writer != null) writer.Close();
                if (process != null) process.Dispose();
            }
            catch { }
        }

        public bool Ready { get { return ready > 0; } }
    }

    class PdEncoder : Encoder
    {
        public string MediaType;

        public void Open(string mode)
        {
            var define = new PairList(Util.GetUserPath("pd.def"));
            define.Load();

            var encoder = Util.GetBasePath(define["encoder"]);

            if (File.Exists(encoder) == false)
                throw new Exception("エンコーダがありません。" + encoder);

            if (define.IsDefined(mode) == false)
                throw new Exception("指定された変換モードはありません。" + mode);

            MediaType = define["type"];

            Open(encoder, define[mode], define["window"] == "hide", null);
        }
    }

    class HlsEncoder : Encoder
    {
        static int unique = 0;  //セグメントファイル名をユニークにするための番号

        public void Open(string mode)
        {
            var define = new PairList(Util.GetUserPath("hls.def"));
            define.Load();

            var encoder = Util.GetBasePath(define["encoder"]);

            if (File.Exists(encoder) == false)
                throw new Exception("エンコーダがありません。" + encoder);

            if (define.IsDefined(mode) == false)
                throw new Exception("指定された変換モードはありません。" + mode);

            var option = define[mode].Replace("{segment-id}", unique.ToString());
            Interlocked.Increment(ref unique);

            var workDir = Util.GetTempPath();

            if (Directory.Exists(workDir) == false)
                Directory.CreateDirectory(workDir);

            Open(encoder, option, define["window"] == "hide", workDir);
        }
    }
}
