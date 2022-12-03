namespace Tvmaid
{
    partial class SetupForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupForm));
            this.acceptButton = new System.Windows.Forms.Button();
            this.tvtestDialog = new System.Windows.Forms.SaveFileDialog();
            this.recDirDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.driverDialog = new System.Windows.Forms.SaveFileDialog();
            this.postProcessDialog = new System.Windows.Forms.SaveFileDialog();
            this.cancelButton = new System.Windows.Forms.Button();
            this.otherTabPage = new System.Windows.Forms.TabPage();
            this.epgHourBox = new System.Windows.Forms.TextBox();
            this.postProcessBox = new System.Windows.Forms.TextBox();
            this.recFileBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.autoSleepCheck = new System.Windows.Forms.CheckBox();
            this.postProcessRefButton = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tunerTabPage = new System.Windows.Forms.TabPage();
            this.tunerPanel = new System.Windows.Forms.Panel();
            this.tunerBox = new System.Windows.Forms.TreeView();
            this.tunerNameBox = new System.Windows.Forms.TextBox();
            this.upButton = new System.Windows.Forms.Button();
            this.downButton = new System.Windows.Forms.Button();
            this.driverRefButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.removeButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.driverBox = new System.Windows.Forms.TextBox();
            this.tunerAddButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tunerUpdateCheck = new System.Windows.Forms.CheckBox();
            this.baseicTabPage = new System.Windows.Forms.TabPage();
            this.unregStartupButton = new System.Windows.Forms.Button();
            this.regStartupButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.recDirRefButton = new System.Windows.Forms.Button();
            this.tvtestRefButton = new System.Windows.Forms.Button();
            this.recDirBox = new System.Windows.Forms.TextBox();
            this.tvtestBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.chatTabPage = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.niconicoPasswordBox = new System.Windows.Forms.TextBox();
            this.niconicoMailBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.otherTabPage.SuspendLayout();
            this.tunerTabPage.SuspendLayout();
            this.tunerPanel.SuspendLayout();
            this.baseicTabPage.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.chatTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // acceptButton
            // 
            this.acceptButton.Location = new System.Drawing.Point(439, 491);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(114, 30);
            this.acceptButton.TabIndex = 1;
            this.acceptButton.Text = "OK";
            this.acceptButton.Click += new System.EventHandler(this.endButton_Click);
            // 
            // tvtestDialog
            // 
            this.tvtestDialog.CheckFileExists = true;
            this.tvtestDialog.Filter = "TVTest|TVTest.exe";
            this.tvtestDialog.OverwritePrompt = false;
            this.tvtestDialog.Title = "TVTestの場所";
            // 
            // recDirDialog
            // 
            this.recDirDialog.Description = "録画フォルダを選択してください。";
            this.recDirDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // driverDialog
            // 
            this.driverDialog.CheckFileExists = true;
            this.driverDialog.Filter = "BonDriver|Bondriver*.dll";
            this.driverDialog.OverwritePrompt = false;
            this.driverDialog.Title = "Bonドライバ";
            // 
            // postProcessDialog
            // 
            this.postProcessDialog.CheckFileExists = true;
            this.postProcessDialog.Filter = "実行ファイル (*.exe *.bat)|*.exe;*.bat";
            this.postProcessDialog.OverwritePrompt = false;
            this.postProcessDialog.Title = "録画後プロセス";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(559, 491);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(114, 30);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // otherTabPage
            // 
            this.otherTabPage.Controls.Add(this.epgHourBox);
            this.otherTabPage.Controls.Add(this.postProcessBox);
            this.otherTabPage.Controls.Add(this.recFileBox);
            this.otherTabPage.Controls.Add(this.label8);
            this.otherTabPage.Controls.Add(this.label17);
            this.otherTabPage.Controls.Add(this.label14);
            this.otherTabPage.Controls.Add(this.autoSleepCheck);
            this.otherTabPage.Controls.Add(this.postProcessRefButton);
            this.otherTabPage.Controls.Add(this.label13);
            this.otherTabPage.Controls.Add(this.label12);
            this.otherTabPage.Location = new System.Drawing.Point(4, 28);
            this.otherTabPage.Name = "otherTabPage";
            this.otherTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.otherTabPage.Size = new System.Drawing.Size(653, 441);
            this.otherTabPage.TabIndex = 3;
            this.otherTabPage.Text = "その他";
            this.otherTabPage.UseVisualStyleBackColor = true;
            // 
            // epgHourBox
            // 
            this.epgHourBox.Location = new System.Drawing.Point(161, 151);
            this.epgHourBox.Name = "epgHourBox";
            this.epgHourBox.Size = new System.Drawing.Size(461, 27);
            this.epgHourBox.TabIndex = 10;
            // 
            // postProcessBox
            // 
            this.postProcessBox.Location = new System.Drawing.Point(161, 85);
            this.postProcessBox.Name = "postProcessBox";
            this.postProcessBox.Size = new System.Drawing.Size(341, 27);
            this.postProcessBox.TabIndex = 3;
            // 
            // recFileBox
            // 
            this.recFileBox.Location = new System.Drawing.Point(161, 36);
            this.recFileBox.Name = "recFileBox";
            this.recFileBox.Size = new System.Drawing.Size(461, 27);
            this.recFileBox.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(45, 265);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(577, 80);
            this.label8.TabIndex = 13;
            this.label8.Text = "スリープからの自動復帰時、録画終了後にスリープ状態に戻す機能です。\r\n（録画後プロセスが指定されているときは無視され、OFFになります）\r\n";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(157, 191);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(321, 19);
            this.label17.TabIndex = 11;
            this.label17.Text = "カンマで区切って、複数指定できます。(例) 9,15,21";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(30, 154);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(114, 19);
            this.label14.TabIndex = 9;
            this.label14.Text = "番組表更新時刻";
            // 
            // autoSleepCheck
            // 
            this.autoSleepCheck.AutoSize = true;
            this.autoSleepCheck.Location = new System.Drawing.Point(49, 239);
            this.autoSleepCheck.Name = "autoSleepCheck";
            this.autoSleepCheck.Size = new System.Drawing.Size(140, 23);
            this.autoSleepCheck.TabIndex = 12;
            this.autoSleepCheck.Text = "自動スリープを行う\r\n";
            this.autoSleepCheck.UseVisualStyleBackColor = true;
            // 
            // postProcessRefButton
            // 
            this.postProcessRefButton.Location = new System.Drawing.Point(508, 85);
            this.postProcessRefButton.Name = "postProcessRefButton";
            this.postProcessRefButton.Size = new System.Drawing.Size(114, 30);
            this.postProcessRefButton.TabIndex = 4;
            this.postProcessRefButton.Text = "参照...";
            this.postProcessRefButton.UseVisualStyleBackColor = true;
            this.postProcessRefButton.Click += new System.EventHandler(this.postProcessRefButton_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(45, 91);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(99, 19);
            this.label13.TabIndex = 2;
            this.label13.Text = "録画後プロセス";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(48, 39);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(96, 19);
            this.label12.TabIndex = 0;
            this.label12.Text = "録画ファイル名";
            // 
            // tunerTabPage
            // 
            this.tunerTabPage.Controls.Add(this.tunerPanel);
            this.tunerTabPage.Controls.Add(this.tunerUpdateCheck);
            this.tunerTabPage.Location = new System.Drawing.Point(4, 28);
            this.tunerTabPage.Name = "tunerTabPage";
            this.tunerTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.tunerTabPage.Size = new System.Drawing.Size(653, 441);
            this.tunerTabPage.TabIndex = 1;
            this.tunerTabPage.Text = "チューナ";
            this.tunerTabPage.UseVisualStyleBackColor = true;
            // 
            // tunerPanel
            // 
            this.tunerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tunerPanel.Controls.Add(this.tunerBox);
            this.tunerPanel.Controls.Add(this.tunerNameBox);
            this.tunerPanel.Controls.Add(this.upButton);
            this.tunerPanel.Controls.Add(this.downButton);
            this.tunerPanel.Controls.Add(this.driverRefButton);
            this.tunerPanel.Controls.Add(this.label7);
            this.tunerPanel.Controls.Add(this.removeButton);
            this.tunerPanel.Controls.Add(this.label6);
            this.tunerPanel.Controls.Add(this.label5);
            this.tunerPanel.Controls.Add(this.driverBox);
            this.tunerPanel.Controls.Add(this.tunerAddButton);
            this.tunerPanel.Controls.Add(this.label2);
            this.tunerPanel.Enabled = false;
            this.tunerPanel.Location = new System.Drawing.Point(6, 48);
            this.tunerPanel.Name = "tunerPanel";
            this.tunerPanel.Size = new System.Drawing.Size(641, 383);
            this.tunerPanel.TabIndex = 1;
            // 
            // tunerBox
            // 
            this.tunerBox.FullRowSelect = true;
            this.tunerBox.HideSelection = false;
            this.tunerBox.Indent = 5;
            this.tunerBox.Location = new System.Drawing.Point(120, 154);
            this.tunerBox.Name = "tunerBox";
            this.tunerBox.ShowLines = false;
            this.tunerBox.ShowNodeToolTips = true;
            this.tunerBox.ShowPlusMinus = false;
            this.tunerBox.ShowRootLines = false;
            this.tunerBox.Size = new System.Drawing.Size(380, 201);
            this.tunerBox.TabIndex = 8;
            // 
            // tunerNameBox
            // 
            this.tunerNameBox.Location = new System.Drawing.Point(120, 106);
            this.tunerNameBox.Name = "tunerNameBox";
            this.tunerNameBox.Size = new System.Drawing.Size(380, 27);
            this.tunerNameBox.TabIndex = 5;
            // 
            // upButton
            // 
            this.upButton.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.upButton.Location = new System.Drawing.Point(506, 154);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(114, 30);
            this.upButton.TabIndex = 9;
            this.upButton.Text = "上へ";
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.Click += new System.EventHandler(this.upButton_Click);
            // 
            // downButton
            // 
            this.downButton.Location = new System.Drawing.Point(506, 200);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(114, 30);
            this.downButton.TabIndex = 10;
            this.downButton.Text = "下へ";
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.Click += new System.EventHandler(this.downButton_Click);
            // 
            // driverRefButton
            // 
            this.driverRefButton.Location = new System.Drawing.Point(506, 60);
            this.driverRefButton.Name = "driverRefButton";
            this.driverRefButton.Size = new System.Drawing.Size(114, 30);
            this.driverRefButton.TabIndex = 3;
            this.driverRefButton.Text = "参照...";
            this.driverRefButton.UseVisualStyleBackColor = true;
            this.driverRefButton.Click += new System.EventHandler(this.driverRefButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(44, 154);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 19);
            this.label7.TabIndex = 7;
            this.label7.Text = "チューナ";
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(506, 248);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(114, 30);
            this.removeButton.TabIndex = 11;
            this.removeButton.Text = "削除";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(391, 19);
            this.label6.TabIndex = 0;
            this.label6.Text = "Bonドライバ、チューナ名を指定して、追加ボタンを押してください。";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 64);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 19);
            this.label5.TabIndex = 1;
            this.label5.Text = "Bonドライバ";
            // 
            // driverBox
            // 
            this.driverBox.Location = new System.Drawing.Point(120, 61);
            this.driverBox.Name = "driverBox";
            this.driverBox.Size = new System.Drawing.Size(380, 27);
            this.driverBox.TabIndex = 2;
            // 
            // tunerAddButton
            // 
            this.tunerAddButton.Location = new System.Drawing.Point(506, 105);
            this.tunerAddButton.Name = "tunerAddButton";
            this.tunerAddButton.Size = new System.Drawing.Size(114, 30);
            this.tunerAddButton.TabIndex = 6;
            this.tunerAddButton.Text = "追加";
            this.tunerAddButton.UseVisualStyleBackColor = true;
            this.tunerAddButton.Click += new System.EventHandler(this.tunerAddButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "チューナ名";
            // 
            // tunerUpdateCheck
            // 
            this.tunerUpdateCheck.AutoSize = true;
            this.tunerUpdateCheck.Location = new System.Drawing.Point(26, 15);
            this.tunerUpdateCheck.Name = "tunerUpdateCheck";
            this.tunerUpdateCheck.Size = new System.Drawing.Size(143, 23);
            this.tunerUpdateCheck.TabIndex = 0;
            this.tunerUpdateCheck.Text = "チューナ設定を行う";
            this.tunerUpdateCheck.UseVisualStyleBackColor = true;
            this.tunerUpdateCheck.CheckedChanged += new System.EventHandler(this.tunerUpdateCheck_CheckedChanged);
            // 
            // baseicTabPage
            // 
            this.baseicTabPage.Controls.Add(this.unregStartupButton);
            this.baseicTabPage.Controls.Add(this.regStartupButton);
            this.baseicTabPage.Controls.Add(this.label4);
            this.baseicTabPage.Controls.Add(this.recDirRefButton);
            this.baseicTabPage.Controls.Add(this.tvtestRefButton);
            this.baseicTabPage.Controls.Add(this.recDirBox);
            this.baseicTabPage.Controls.Add(this.tvtestBox);
            this.baseicTabPage.Controls.Add(this.label1);
            this.baseicTabPage.Controls.Add(this.label3);
            this.baseicTabPage.Location = new System.Drawing.Point(4, 28);
            this.baseicTabPage.Name = "baseicTabPage";
            this.baseicTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.baseicTabPage.Size = new System.Drawing.Size(653, 441);
            this.baseicTabPage.TabIndex = 0;
            this.baseicTabPage.Text = "基本";
            this.baseicTabPage.UseVisualStyleBackColor = true;
            // 
            // unregStartupButton
            // 
            this.unregStartupButton.Location = new System.Drawing.Point(330, 162);
            this.unregStartupButton.Name = "unregStartupButton";
            this.unregStartupButton.Size = new System.Drawing.Size(175, 30);
            this.unregStartupButton.TabIndex = 7;
            this.unregStartupButton.Text = "登録解除...";
            this.unregStartupButton.UseVisualStyleBackColor = true;
            this.unregStartupButton.Click += new System.EventHandler(this.unregStartupButton_Click);
            // 
            // regStartupButton
            // 
            this.regStartupButton.Location = new System.Drawing.Point(146, 162);
            this.regStartupButton.Name = "regStartupButton";
            this.regStartupButton.Size = new System.Drawing.Size(175, 30);
            this.regStartupButton.TabIndex = 6;
            this.regStartupButton.Text = "スタートアップ登録...";
            this.regStartupButton.UseVisualStyleBackColor = true;
            this.regStartupButton.Click += new System.EventHandler(this.regStartupButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(142, 281);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(348, 38);
            this.label4.TabIndex = 8;
            this.label4.Text = "初めてのセットアップ時は、チューナの設定を行ってください。\r\n上部にタブがあります。";
            // 
            // recDirRefButton
            // 
            this.recDirRefButton.Location = new System.Drawing.Point(511, 86);
            this.recDirRefButton.Name = "recDirRefButton";
            this.recDirRefButton.Size = new System.Drawing.Size(114, 30);
            this.recDirRefButton.TabIndex = 5;
            this.recDirRefButton.Text = "参照...";
            this.recDirRefButton.UseVisualStyleBackColor = true;
            this.recDirRefButton.Click += new System.EventHandler(this.recDirRefButton_Click);
            // 
            // tvtestRefButton
            // 
            this.tvtestRefButton.Location = new System.Drawing.Point(511, 40);
            this.tvtestRefButton.Name = "tvtestRefButton";
            this.tvtestRefButton.Size = new System.Drawing.Size(114, 30);
            this.tvtestRefButton.TabIndex = 2;
            this.tvtestRefButton.Text = "参照...";
            this.tvtestRefButton.UseVisualStyleBackColor = true;
            this.tvtestRefButton.Click += new System.EventHandler(this.tvtestRefButton_Click);
            // 
            // recDirBox
            // 
            this.recDirBox.Location = new System.Drawing.Point(146, 86);
            this.recDirBox.Name = "recDirBox";
            this.recDirBox.Size = new System.Drawing.Size(359, 27);
            this.recDirBox.TabIndex = 4;
            // 
            // tvtestBox
            // 
            this.tvtestBox.Location = new System.Drawing.Point(146, 40);
            this.tvtestBox.Name = "tvtestBox";
            this.tvtestBox.Size = new System.Drawing.Size(359, 27);
            this.tvtestBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "TVTestの場所";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(55, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 19);
            this.label3.TabIndex = 3;
            this.label3.Text = "録画の場所";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.baseicTabPage);
            this.tabControl1.Controls.Add(this.tunerTabPage);
            this.tabControl1.Controls.Add(this.otherTabPage);
            this.tabControl1.Controls.Add(this.chatTabPage);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(661, 473);
            this.tabControl1.TabIndex = 0;
            // 
            // chatTabPage
            // 
            this.chatTabPage.Controls.Add(this.label11);
            this.chatTabPage.Controls.Add(this.niconicoPasswordBox);
            this.chatTabPage.Controls.Add(this.niconicoMailBox);
            this.chatTabPage.Controls.Add(this.label9);
            this.chatTabPage.Controls.Add(this.label10);
            this.chatTabPage.Location = new System.Drawing.Point(4, 28);
            this.chatTabPage.Name = "chatTabPage";
            this.chatTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.chatTabPage.Size = new System.Drawing.Size(653, 441);
            this.chatTabPage.TabIndex = 4;
            this.chatTabPage.Text = "実況コメント";
            this.chatTabPage.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(67, 153);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(532, 122);
            this.label11.TabIndex = 14;
            this.label11.Text = "録画の再生時に実況コメントを表示するには、ニコニコ動画のアカウントが必要です。\r\nメールアドレスとパスワードを指定してください。\r\n";
            // 
            // niconicoPasswordBox
            // 
            this.niconicoPasswordBox.Location = new System.Drawing.Point(178, 90);
            this.niconicoPasswordBox.Name = "niconicoPasswordBox";
            this.niconicoPasswordBox.PasswordChar = '*';
            this.niconicoPasswordBox.Size = new System.Drawing.Size(359, 27);
            this.niconicoPasswordBox.TabIndex = 8;
            // 
            // niconicoMailBox
            // 
            this.niconicoMailBox.Location = new System.Drawing.Point(178, 44);
            this.niconicoMailBox.Name = "niconicoMailBox";
            this.niconicoMailBox.Size = new System.Drawing.Size(359, 27);
            this.niconicoMailBox.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(67, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(87, 19);
            this.label9.TabIndex = 5;
            this.label9.Text = "メールアドレス";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(87, 93);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 19);
            this.label10.TabIndex = 7;
            this.label10.Text = "パスワード";
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(685, 533);
            this.Controls.Add(this.acceptButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tvmaid セットアップ";
            this.otherTabPage.ResumeLayout(false);
            this.otherTabPage.PerformLayout();
            this.tunerTabPage.ResumeLayout(false);
            this.tunerTabPage.PerformLayout();
            this.tunerPanel.ResumeLayout(false);
            this.tunerPanel.PerformLayout();
            this.baseicTabPage.ResumeLayout(false);
            this.baseicTabPage.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.chatTabPage.ResumeLayout(false);
            this.chatTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.SaveFileDialog tvtestDialog;
        private System.Windows.Forms.FolderBrowserDialog recDirDialog;
        private System.Windows.Forms.SaveFileDialog driverDialog;
        private System.Windows.Forms.SaveFileDialog postProcessDialog;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabPage otherTabPage;
        private System.Windows.Forms.TextBox epgHourBox;
        private System.Windows.Forms.TextBox postProcessBox;
        private System.Windows.Forms.TextBox recFileBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox autoSleepCheck;
        private System.Windows.Forms.Button postProcessRefButton;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TabPage tunerTabPage;
        private System.Windows.Forms.Panel tunerPanel;
        private System.Windows.Forms.TreeView tunerBox;
        private System.Windows.Forms.TextBox tunerNameBox;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.Button driverRefButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox driverBox;
        private System.Windows.Forms.Button tunerAddButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox tunerUpdateCheck;
        private System.Windows.Forms.TabPage baseicTabPage;
        private System.Windows.Forms.Button unregStartupButton;
        private System.Windows.Forms.Button regStartupButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button recDirRefButton;
        private System.Windows.Forms.Button tvtestRefButton;
        private System.Windows.Forms.TextBox recDirBox;
        private System.Windows.Forms.TextBox tvtestBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage chatTabPage;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox niconicoPasswordBox;
        private System.Windows.Forms.TextBox niconicoMailBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
    }
}

