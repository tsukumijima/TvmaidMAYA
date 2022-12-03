using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tvmaid
{
    //スリープマネージャ

    //OSからスリープ予告が来た場合 → スリープ準備 → OSがスリープ実行
    //Tvmaidメニューのスリープが選択された場合 → スリープ待ちタイマーセット → スリープできるなら実行
    //自動復帰後、自動スリープの場合 → スリープ待ちタイマーセット → スリープできるなら実行

    public class SleepMan: IDisposable
    {
        WakeTimer wake = new WakeTimer();   //復帰タイマー
        Timer waitTimer = new Timer();      //スリープ待ちタイマー

        public SleepMan()
        {
            waitTimer.Interval = 1000;
            waitTimer.Tick += new EventHandler(Sleep);
            Sleep(null, null);  //1回目をすぐに呼ぶ
        }

        //Tvmaidメニューのスリープが選択された
        public void SetSleep()
        {
            Log.Info("ユーザの手動操作で、スリープモードにしました。");
            waitTimer.Start();
        }

        //OSからスリープ予告が来た
        public void OnSuspend()
        {
            PrepareSleep();
        }

        //スリープできる状態まで待つ
        //Tvmaidメニューのスリープが選択されたときか、自動復帰後の自動スリープのとき
        void Sleep(object sender, EventArgs e)
        {
            if (SleepState.IsStop())
                return;

            if (Recorder.Running)
                return;

            if (EpgUpdater.Running)
                return;

            //次の予約がまで10分以上ならスリープ
            var time = GetNextTime();
            var span = time - DateTime.Now;

            if (span > new TimeSpan(0, 10, 0))
            {
                waitTimer.Stop();

                var countdown = new SleepCountdown(GetNextTime());
                var res = countdown.ShowDialog();

                if (res == DialogResult.OK)
                {
                    OnSuspend();
                    Application.SetSuspendState(PowerState.Suspend, false, false);
                }
                else
                    Log.Info("スリープをキャンセルしました。");
            }
        }

        //スリープの準備
        public void PrepareSleep()
        {
            Log.Info("スリープ状態に入ります。");

            var time = GetNextTime();
            time -= new TimeSpan(0, 2, 0);    //2分前に復帰させる

            //2分以内に次の予約がある
            if (time < DateTime.Now)
                time = DateTime.Now + new TimeSpan(0, 0, 30);   //すぐ復帰させる(30秒後)

            wake.SetTimer(time);

            Log.Info("復帰タイマーを次の時間にセットしました。" + time.ToString("MM/dd HH:mm:ss"));
        }

        //復帰
        public void OnResume()
        {
            Log.Info("スリープから復帰しました。");

            wake.Cancel();

            if (AppDefine.Main.Data["autosleep"] != "on")
                return;

            //現在の時間が次の予約の3分前以内なら、自動復帰したと判断し、再スリープするよう準備
            var time = GetNextTime();
            var span = time - DateTime.Now;

            if (span < new TimeSpan(0, 3, 0))
            {
                waitTimer.Start();
                Log.Info("スリープモードで自動復帰したため、録画後再スリープします。");
            }
            else
                Log.Info("自動復帰でないため、再スリープしません。");
        }

        public void Dispose()
        {
            wake.Cancel();  //復帰タイマーキャンセル
        }

        //一番早い有効な予約、または番組表更新の時間を取得
        DateTime GetNextTime()
        {
            var epg = RecTimer.NextEpgUpdate;   //次の番組表更新
            var res = new DateTime(GetNextReserveTime());   //次の予約

            //早い方を返す
            return res < epg ? res : epg;
        }

        //次の予約の時間を取得
        long GetNextReserveTime()
        {
            using (var tvdb = new Tvdb(true))
            {
                tvdb.Sql = "select start from reserve where status & {0} and start > {1} order by start".Formatex((int)Reserve.StatusCode.Enable, DateTime.Now.Ticks);
                using (var t = tvdb.GetTable())
                    return t.Read() ? t.GetLong(0) : DateTime.MaxValue.Ticks;
            }
        }
    }

    //復帰タイマー
    class WakeTimer
    {
        IntPtr handle = IntPtr.Zero;

        public void SetTimer(DateTime wake)
        {
            Cancel();

            handle = CreateWaitableTimer(IntPtr.Zero, true, "WaitableTimer");

            if (handle.ToInt32() == 0)
                throw new Exception("復帰タイマーの設定に失敗しました。エラーコード = " + Marshal.GetLastWin32Error().ToString());

            long interval = (wake - DateTime.Now).Ticks * -1;
            var ret = SetWaitableTimer(handle, ref interval, 0, IntPtr.Zero, IntPtr.Zero, true);

            if (ret == false)
                throw new Exception("復帰タイマーの設定に失敗しました。エラーコード = " + Marshal.GetLastWin32Error().ToString());
        }

        public void Cancel()
        {
            if (handle != IntPtr.Zero)
            {
                CancelWaitableTimer(handle);
                CloseHandle(handle);
                handle = IntPtr.Zero;
            }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32.dll")]
        static extern bool CancelWaitableTimer(IntPtr hTimer);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
    }
    
    //スリープ状態
    class SleepState
    {
        static int count = 0;  //スリープ抑止カウンタ
        static object lockObj = new object();

        public static bool IsStop()
        {
            lock (lockObj)
                return count > 0;
        }

        public static void Stop(bool flug)
        {
            lock (lockObj)
            {
                if (flug)
                {
                    count++;
                    if (count == 1) SetState(true);
                }
                else
                {
                    count--;
                    if (count == 0) SetState(false);
                }
            }
        }

        //trueでスリープ抑止
        static void SetState(bool stop)
        {
            if (stop)
                SetThreadExecutionState(ExecutionState.SystemRequired | ExecutionState.Continuous);
            else
                SetThreadExecutionState(ExecutionState.Continuous);
        }

        [DllImport("kernel32.dll")]
        extern static ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        [FlagsAttribute]
        enum ExecutionState : uint
        {
            SystemRequired = 1,     // スタンバイを抑止
            DisplayRequired = 2,    // 画面OFFを抑止
            Continuous = 0x80000000 // 効果を永続
        }
    }
}
