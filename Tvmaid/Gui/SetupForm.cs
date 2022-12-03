using System;
using System.IO;
using System.Windows.Forms;

namespace Tvmaid
{
    public partial class SetupForm : Form
    {
        MainDefine mainDefine;
        PairList tunerDefine;

        public SetupForm()
        {
            InitializeComponent();

            LoadMainDef();
            LoadTunerDef();
        }

        void LoadMainDef()
        {
            mainDefine = new MainDefine();
            mainDefine.Load();

            tvtestBox.Text = mainDefine.Data["tvtest"];
            recDirBox.Text = mainDefine.Data["record.folder"];
            recFileBox.Text = mainDefine.Data["record.file"];
            epgHourBox.Text = mainDefine.Data["epg.hour"];
            autoSleepCheck.Checked = mainDefine.Data["autosleep"] == "on";
            postProcessBox.Text = mainDefine.Data["postprocess"];
            niconicoMailBox.Text = mainDefine.Data["chat.niconico.mail"];
            niconicoPasswordBox.Text = mainDefine.Data["chat.niconico.password"];
        }

        void LoadTunerDef()
        {
            tunerDefine = new PairList(Util.GetUserPath("tuner.def"));
            tunerDefine.Load();

            foreach (var pair in tunerDefine)
                tunerBox.Nodes.Add(pair.Key + "=" + pair.Value);
        }


        //設定完了
        private void endButton_Click(object sender, EventArgs arg)
        {
            try
            {
                SaveMainDefine();

                if (tunerUpdateCheck.Checked)
                {
                    SaveTunerDef();
                    Program.IsTunerUpdate = true;
                }

                var res = MessageBox.Show("Tvmaidを再起動して、設定を反映しますか？", Program.Name, MessageBoxButtons.OKCancel);

                if (res == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.Yes;
                    Program.IsReboot = true;
                }

                Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Program.Name);
            }
        }

        private void SaveMainDefine()
        {
            mainDefine.Data["tvtest"] = tvtestBox.Text;
            mainDefine.Data["record.folder"] = recDirBox.Text;
            mainDefine.Data["record.file"] = recFileBox.Text;
            mainDefine.Data["epg.hour"] = epgHourBox.Text;
            mainDefine.Data["autosleep"] = autoSleepCheck.Checked ? "on" : "off";
            mainDefine.Data["postprocess"] = postProcessBox.Text;
            mainDefine.Data["chat.niconico.mail"] = niconicoMailBox.Text;
            mainDefine.Data["chat.niconico.password"] = niconicoPasswordBox.Text;

            mainDefine.Data.Save();
        }

        private void SaveTunerDef()
        {
            tunerDefine.Clear();

            foreach (TreeNode item in tunerBox.Nodes)
            {
                var data = item.Text.Split(new char[] { '=' });
                tunerDefine[data[0]] = data[1];
            }

            tunerDefine.Save();
        }

        //参照
        private void tvtestRefButton_Click(object sender, EventArgs e)
        {
            var res = this.tvtestDialog.ShowDialog();

            if (res == DialogResult.OK)
                tvtestBox.Text = tvtestDialog.FileName;
        }

        //参照
        private void recDirRefButton_Click(object sender, EventArgs e)
        {
            var res = recDirDialog.ShowDialog();

            if (res == DialogResult.OK)
                recDirBox.Text = recDirDialog.SelectedPath;
        }

        //参照
        private void driverRefButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(tvtestBox.Text))
                driverDialog.InitialDirectory = Path.GetDirectoryName(tvtestBox.Text);

            var res = driverDialog.ShowDialog();
            if (res == DialogResult.OK)
                driverBox.Text = driverDialog.FileName;
        }

        //参照
        private void postProcessRefButton_Click(object sender, EventArgs e)
        {
            var res = this.postProcessDialog.ShowDialog();

            if (res == DialogResult.OK)
                postProcessBox.Text = postProcessDialog.FileName;
        }

        //追加
        private void tunerAddButton_Click(object sender, EventArgs arg)
        {
            try
            {
                if (tunerNameBox.Text == "")
                    throw new Exception("チューナ名を入力してください。");
                if (tunerNameBox.Text.IndexOf('=') != -1)
                    throw new Exception("チューナ名に「=」は使えません。");
                if (File.Exists(driverBox.Text) == false)
                    throw new Exception("Bonドライバのパスが間違っています。");

                foreach (TreeNode item in tunerBox.Nodes)
                {
                    var tuner = item.Text.Split(new char[] { '=' });

                    if (tunerNameBox.Text == tuner[0])
                        throw new Exception("同じ名前のチューナを複数指定できません。");
                }

                tunerBox.Nodes.Add(tunerNameBox.Text + "=" + driverBox.Text);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Program.Name);
            }
        }

        //上へ
        private void upButton_Click(object sender, EventArgs e)
        {
            if (tunerBox.SelectedNode != null)
                ReplaceNode(tunerBox.SelectedNode, tunerBox.SelectedNode.PrevNode);
        }

        //下へ
        private void downButton_Click(object sender, EventArgs e)
        {
            if (tunerBox.SelectedNode != null)
                ReplaceNode(tunerBox.SelectedNode, tunerBox.SelectedNode.NextNode);
        }

        void ReplaceNode(TreeNode node1, TreeNode node2)
        {
            if (node1 != null && node2 != null)
            {
                var text = node1.Text;
                node1.Text = node2.Text;
                node2.Text = text;
                tunerBox.SelectedNode = node2;
            }
        }

        //削除
        private void removeButton_Click(object sender, EventArgs e)
        {
            if (tunerBox.SelectedNode != null)
            {
                tunerBox.Nodes.Remove(tunerBox.SelectedNode);
            }            
        }
               
        private void tunerUpdateCheck_CheckedChanged(object sender, EventArgs e)
        {
            tunerPanel.Enabled = tunerUpdateCheck.Checked;
        }

        private void regStartupButton_Click(object sender, EventArgs e)
        {
            var msg =
                "スタートアップに登録していいですか？\n"
                + "注意！ レジストリのスタートアップに登録します。\n"
                + "この機能は使用せず、自分でショーカットをスタートメニューに置いてもかまいません。";

            if (MessageBox.Show(msg, Program.Name, MessageBoxButtons.OKCancel) != DialogResult.OK) return;

            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            key.SetValue("TvmaidMAYA", Util.GetBasePath("Tvmaid.exe"));
            key.Close();

            MessageBox.Show("登録しました。", Program.Name);
        }

        private void unregStartupButton_Click(object sender, EventArgs e)
        {
            var msg = "レジストリのスタートアップ設定を削除していいですか？";

            if (MessageBox.Show(msg, Program.Name, MessageBoxButtons.OKCancel) != DialogResult.OK) return;

            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            key.DeleteValue("TvmaidMAYA", false);
            key.Close();

            MessageBox.Show("削除しました。", Program.Name);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
