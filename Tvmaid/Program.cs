using System;
using System.Windows.Forms;

namespace Tvmaid
{
    static class Program
    {
        public static string Name = "Tvmaid";
        public static string Version = "MAYA リリース 27";

        public static bool IsTunerUpdate = false;
        public static bool IsReboot = false;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            System.Threading.ThreadPool.SetMinThreads(64, 64);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(Init());

            PostProcess();
        }

        static void PostProcess()
        {
            try
            {
                if (IsTunerUpdate)
                {
                    AppDefine.Load();
                    Util.CopyPlugin();
                    TunerUpdater.Update();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("チューナの更新に失敗しました。[詳細]" + ex.Message, Program.Name);
            }

            Log.Close();

            try
            {
                if (IsReboot)
                {
                    var p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = Application.ExecutablePath;
                    p.Start();
                    p.WaitForInputIdle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("再起動に失敗しました。[詳細]" + ex.Message, Program.Name);
            }
        }

        static Form Init()
        {
            try
            {
                Util.CopyUserFile();
                Log.Open();
                Log.Info("{0} {1} を開始します。".Formatex(Program.Name, Program.Version));
            }
            catch (Exception ex)
            {
                MessageBox.Show("初期化に失敗しました。" + ex.Message);
                throw;
            }

            try
            {
                AppDefine.Load();
                AppDefine.Main.Check();
                Util.CopyPlugin();
                return new MainForm();
            }
            catch(Exception ex)
            {
                MessageBox.Show("起動に失敗しました。" + ex.Message, "Tvmaid");
                return new SetupForm();
            }
        }
    }
}
           
