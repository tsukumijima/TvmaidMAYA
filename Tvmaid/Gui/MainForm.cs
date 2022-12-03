using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tvmaid
{
    public partial class MainForm : Form
    {
        SleepMan sleepMan = new SleepMan();
        Task backTask;

        public MainForm()
        {
            InitializeComponent();

            backTask = Task.Factory.StartNew(() =>
            {
                RecTimer.Run();
                WebServer.Run();
                HlsStream.ClearSegment();
            });
        }
                
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_POWERBROADCAST = 0x0218;   //電源に関するメッセージ
            const int PBT_APMSUSPEND = 0x0004;      //スリープに入る
            const int PBT_APMRESUMEAUTOMATIC = 0x0012;  //復帰した

            if (WM_POWERBROADCAST == m.Msg)
            {
                switch (m.WParam.ToInt32())
                {
                    case PBT_APMRESUMEAUTOMATIC: sleepMan.OnResume(); break;
                    case PBT_APMSUSPEND: sleepMan.OnSuspend(); break;
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            sleepMan.Dispose();

            WebServer.Stop();
            RecTimer.Stop();
            HlsStream.StopAll();

            //スレッド終了待ち
            var timeout = 30;

            var form = new ExitForm(timeout);

            Task.Factory.StartNew(() =>
            {
                if (backTask != null)
                    backTask.Wait(timeout * 1000);

                System.Diagnostics.Debug.WriteLine("backTask Exit: ");
            })
            .ContinueWith(_ =>
            {
                form.Close();

                Log.Info("{0} {1} を終了します。".Formatex(Program.Name, Program.Version));

                System.Diagnostics.Debug.WriteLine("MainForm Exit: ");

            }, TaskScheduler.FromCurrentSynchronizationContext());

            form.ShowDialog();
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            var ret = MessageBox.Show("終了していいですか？", Program.Name, MessageBoxButtons.OKCancel);

            if (ret == DialogResult.OK)
                Close();
        }

        private void updateTunerMenuItem_Click(object sender, EventArgs e)
        {
            var msg = "チューナ更新していいですか？\n続行すると、更新処理をしてTvmaidを再起動します。";
            var res = MessageBox.Show(msg, Program.Name, MessageBoxButtons.OKCancel);

            if (res == DialogResult.OK)
            {
                Program.IsTunerUpdate = true;
                Program.IsReboot = true;
                Close();
            }
        }

        private void stopEpgMenuItem_Click(object sender, EventArgs e)
        {
            RecTimer.CancelUpdateEpg();
        }

        private void startEpgMenuItem_Click(object sender, EventArgs e)
        {
            RecTimer.UpdateEpg();
        }

        private void sleepMenuItem_Click(object sender, EventArgs e)
        {
            sleepMan.SetSleep();
        }

        private void openEpgMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDefine.Main.Data["epgurl"]);
            }
            catch (Exception ex)
            {
                Log.Error("番組表を開けませんでした。[詳細] " + ex.Message);
                Log.Debug(ex.StackTrace);
            }
        }

        private void setupMenuItem_Click(object sender, EventArgs e)
        {
            var setup = new SetupForm();
            var ret = setup.ShowDialog(this);

            if (ret == DialogResult.Yes)
                Close();
        }
    }
}
