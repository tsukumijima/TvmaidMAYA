using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace Tvmaid
{
    //TSの状態
    class TsStatus
    {
        public int Error = 0;       //エラー数
        public int Drop = 0;        //ドロップ数
        public int Scramble = 0;    //復号化エラー数
    }

    //TVTestプラグインとの通信を行う
    abstract class TvServerBase
    {
        protected Tuner tuner;
        const int timeout = 30 * 1000;
        
        //Api(ウインドウメッセージ番号)
        protected enum Api
        {
            Close = 0xb000,
            GetState,
            GetServices,
            GetEvents,
            SetService,
            StartRec,
            StopRec,
            GetEventTime,
            GetTsStatus,
            GetLogo
        }

        //TVTestプラグインエラーコード
        public enum ErrorCode
        {
            NoError = 0,
            CreateShared,
            CreateWindow,
            CreateMutex,
            StartRec,
            StopRec,
            SetService,
            GetEvents,
            GetState,
            GetEnv,
            GetEventTime,
            GetTsStatus,
            OutOfShared
        }

        public TvServerBase(Tuner tuner)
        {
            this.tuner = tuner;
        }

        public void Open(bool show)
        {
            string param = show ? "" : " /nodshow /min /silent";

            var p = new Process();
            p.StartInfo.FileName = AppDefine.Main.Data["tvtest"];
            p.StartInfo.Arguments = string.Format("/d \"{0}\"" + param, tuner.DriverPath);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.EnvironmentVariables.Add("DriverId", tuner.DriverId);
            p.Start();
            p.WaitForInputIdle();
        }

        //指定チューナを使用中のレコーダが存在するかどうか
        public bool IsOpen()
        {
            var handle = FindWindow("/tvmaid/window", tuner.DriverId);
            return handle != IntPtr.Zero;
        }

        //TVTest呼び出し
        protected string Call(Api api, string arg = "")
        {
            SharedText sharedIn = null;
            SharedText sharedOut = null;
            var mutex = new MutexEx("/tvmaid/mutex/tvserver/" + tuner.DriverId, timeout);

            try
            {
                var handle = FindWindow("/tvmaid/window", tuner.DriverId);

                if (handle == IntPtr.Zero)
                    throw new Exception("TVTest呼び出しに失敗しました(TVTestが終了されました)。");

                sharedIn = new SharedText("/tvmaid/shared/in/" + tuner.DriverId);
                sharedOut = new SharedText("/tvmaid/shared/out/" + tuner.DriverId);

                sharedIn.Write(arg);

                UIntPtr result;
                IntPtr ret = SendMessageTimeout(handle, (uint)api, UIntPtr.Zero, IntPtr.Zero, (uint)SendMessageTimeoutFlags.SMTO_NORMAL, timeout, out result);

                if (ret.ToInt32() == 0)
                    throw new Exception("TVTest呼び出しに失敗しました(タイムアウト)。");

                var code = result.ToUInt32();

                if (code != 0)
                    throw new TvServerExceotion(code);

                return sharedOut.Read();
            }
            finally
            {
                if (sharedIn != null) sharedIn.Dispose();
                if (sharedOut != null) sharedOut.Dispose();
                if (mutex != null) mutex.Dispose();
            }
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [Flags]
        enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult
        );
    }
    
    class TvServerExceotion : Exception
    {
        public uint Code;

        string[] messages =
        {
            "",
            "Tvmaid、TVTest間の共有メモリ作成に失敗しました。",
            "Tvmaid、TVTest間の通信ウインドウの作成に失敗しました。",
            "排他制御の作成に失敗しました。",
            "録画開始に失敗しました。",
            "録画停止に失敗しました。",
            "サービス切り替えに失敗しました。",
            "番組情報の取得に失敗しました(複数の番組)。",
            "録画状態の取得に失敗しました。",
            "初期化に失敗しました(環境変数の取得失敗)。",
            "録画中の番組時間の取得に失敗しました。",
            "エラーパケット数の取得に失敗しました。",
            "共有メモリが足りません。"
        };

        public TvServerExceotion(uint code) : base()
        {
            Code = code;
        }

        public override string Message
        {
            get
            {
                return "TVTestでエラーが発生しました。" + messages[Code];
            }
        }
    }

    //共有メモリ
    abstract class SharedMemory : IDisposable
    {
        MemoryMappedFile map = null;
        protected MemoryMappedViewAccessor view = null;

        public SharedMemory(string name)
        {
            map = MemoryMappedFile.OpenExisting(name);
            view = map.CreateViewAccessor();
        }

        public void Dispose()
        {
            try
            {
                if (view != null) view.Dispose();
                if (map != null) map.Dispose();
            }
            catch { }
        }
    }

    class SharedText : SharedMemory
    {
        public SharedText(string name) : base(name) { }

        public void Write(string str)
        {
            var arr = Encoding.Unicode.GetBytes(str);
            view.WriteArray(0, arr, 0, arr.Length);
        }

        public string Read()
        {
            long position = 0;
            var str = new StringBuilder();

            while (true)
            {
                var c = view.ReadChar(position);
                if (c == '\x0')
                    return str.ToString();

                str.Append(c);
                position += sizeof(char);
            }
        }
    }
}
