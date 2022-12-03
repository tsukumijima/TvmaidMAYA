namespace Tvmaid
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.serviceView = new System.Windows.Forms.ListView();
            this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.eventHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.serviceMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showTvMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeTvMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.borderMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.recordAddMenuIte = new System.Windows.Forms.ToolStripMenuItem();
            this.borderMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.fontChangeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitter = new System.Windows.Forms.SplitContainer();
            this.tunerView = new System.Windows.Forms.TreeView();
            this.statusbar = new System.Windows.Forms.StatusStrip();
            this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.serviceMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
            this.splitter.Panel1.SuspendLayout();
            this.splitter.Panel2.SuspendLayout();
            this.splitter.SuspendLayout();
            this.statusbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // serviceView
            // 
            this.serviceView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.serviceView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHeader,
            this.timeHeader,
            this.eventHeader});
            this.serviceView.ContextMenuStrip = this.serviceMenu;
            this.serviceView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serviceView.FullRowSelect = true;
            this.serviceView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.serviceView.HideSelection = false;
            this.serviceView.Location = new System.Drawing.Point(0, 0);
            this.serviceView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.serviceView.MultiSelect = false;
            this.serviceView.Name = "serviceView";
            this.serviceView.ShowItemToolTips = true;
            this.serviceView.Size = new System.Drawing.Size(525, 170);
            this.serviceView.TabIndex = 0;
            this.serviceView.UseCompatibleStateImageBehavior = false;
            this.serviceView.View = System.Windows.Forms.View.Details;
            this.serviceView.VirtualMode = true;
            this.serviceView.ItemActivate += new System.EventHandler(this.serviceView_ItemActivate);
            this.serviceView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.serviceView_RetrieveVirtualItem);
            // 
            // nameHeader
            // 
            this.nameHeader.Text = "サービス";
            this.nameHeader.Width = 179;
            // 
            // timeHeader
            // 
            this.timeHeader.Text = "時間";
            this.timeHeader.Width = 133;
            // 
            // eventHeader
            // 
            this.eventHeader.Text = "番組";
            this.eventHeader.Width = 195;
            // 
            // serviceMenu
            // 
            this.serviceMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.serviceMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showTvMenuItem,
            this.closeTvMenuItem,
            this.borderMenuItem1,
            this.recordAddMenuIte,
            this.borderMenuItem2,
            this.fontChangeMenuItem});
            this.serviceMenu.Name = "contextMenu";
            this.serviceMenu.Size = new System.Drawing.Size(221, 112);
            // 
            // showTvMenuItem
            // 
            this.showTvMenuItem.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Bold);
            this.showTvMenuItem.Name = "showTvMenuItem";
            this.showTvMenuItem.Size = new System.Drawing.Size(220, 24);
            this.showTvMenuItem.Text = "TVTestで表示(&V)";
            this.showTvMenuItem.Click += new System.EventHandler(this.showTvMenuItem_Click);
            // 
            // closeTvMenuItem
            // 
            this.closeTvMenuItem.Name = "closeTvMenuItem";
            this.closeTvMenuItem.Size = new System.Drawing.Size(220, 24);
            this.closeTvMenuItem.Text = "TVTestを閉じる(&C)";
            this.closeTvMenuItem.Click += new System.EventHandler(this.closeTvMenuItem_Click);
            // 
            // borderMenuItem1
            // 
            this.borderMenuItem1.Name = "borderMenuItem1";
            this.borderMenuItem1.Size = new System.Drawing.Size(217, 6);
            // 
            // recordAddMenuIte
            // 
            this.recordAddMenuIte.Name = "recordAddMenuIte";
            this.recordAddMenuIte.Size = new System.Drawing.Size(220, 24);
            this.recordAddMenuIte.Text = "選択した番組を予約(&R)";
            this.recordAddMenuIte.Click += new System.EventHandler(this.recordAddMenuItem_Click);
            // 
            // borderMenuItem2
            // 
            this.borderMenuItem2.Name = "borderMenuItem2";
            this.borderMenuItem2.Size = new System.Drawing.Size(217, 6);
            // 
            // fontChangeMenuItem
            // 
            this.fontChangeMenuItem.Name = "fontChangeMenuItem";
            this.fontChangeMenuItem.Size = new System.Drawing.Size(220, 24);
            this.fontChangeMenuItem.Text = "フォントの変更(&F)...";
            this.fontChangeMenuItem.Click += new System.EventHandler(this.fontChangeMenuItem_Click);
            // 
            // splitter
            // 
            this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitter.Location = new System.Drawing.Point(0, 0);
            this.splitter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            this.splitter.Panel1.Controls.Add(this.tunerView);
            // 
            // splitter.Panel2
            // 
            this.splitter.Panel2.Controls.Add(this.serviceView);
            this.splitter.Size = new System.Drawing.Size(709, 170);
            this.splitter.SplitterDistance = 179;
            this.splitter.SplitterWidth = 5;
            this.splitter.TabIndex = 9;
            // 
            // tunerView
            // 
            this.tunerView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tunerView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tunerView.FullRowSelect = true;
            this.tunerView.HideSelection = false;
            this.tunerView.Location = new System.Drawing.Point(0, 0);
            this.tunerView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tunerView.Name = "tunerView";
            this.tunerView.ShowLines = false;
            this.tunerView.ShowPlusMinus = false;
            this.tunerView.ShowRootLines = false;
            this.tunerView.Size = new System.Drawing.Size(179, 170);
            this.tunerView.TabIndex = 0;
            this.tunerView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tunerView_AfterSelect);
            // 
            // statusbar
            // 
            this.statusbar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusText});
            this.statusbar.Location = new System.Drawing.Point(0, 170);
            this.statusbar.Name = "statusbar";
            this.statusbar.Size = new System.Drawing.Size(709, 25);
            this.statusbar.TabIndex = 10;
            // 
            // statusText
            // 
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(46, 20);
            this.statusText.Text = "ready";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 195);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.statusbar);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "メイドバー";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.serviceMenu.ResumeLayout(false);
            this.splitter.Panel1.ResumeLayout(false);
            this.splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).EndInit();
            this.splitter.ResumeLayout(false);
            this.statusbar.ResumeLayout(false);
            this.statusbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView serviceView;
        private System.Windows.Forms.SplitContainer splitter;
        private System.Windows.Forms.ColumnHeader nameHeader;
        private System.Windows.Forms.ColumnHeader timeHeader;
        private System.Windows.Forms.ColumnHeader eventHeader;
        private System.Windows.Forms.TreeView tunerView;
        private System.Windows.Forms.ContextMenuStrip serviceMenu;
        private System.Windows.Forms.ToolStripMenuItem showTvMenuItem;
        private System.Windows.Forms.ToolStripSeparator borderMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem closeTvMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fontChangeMenuItem;
        private System.Windows.Forms.StatusStrip statusbar;
        private System.Windows.Forms.ToolStripStatusLabel statusText;
        private System.Windows.Forms.ToolStripMenuItem recordAddMenuIte;
        private System.Windows.Forms.ToolStripSeparator borderMenuItem2;
    }
}