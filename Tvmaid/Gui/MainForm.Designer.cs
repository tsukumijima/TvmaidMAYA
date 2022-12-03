namespace Tvmaid
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.updateTunerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.stopEpgMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startEpgMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.sleepMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openEpgMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitMenuItem,
            this.toolStripMenuItem1,
            this.updateTunerMenuItem,
            this.setupMenuItem,
            this.toolStripMenuItem3,
            this.stopEpgMenuItem,
            this.startEpgMenuItem,
            this.toolStripSeparator1,
            this.sleepMenuItem,
            this.toolStripSeparator2,
            this.openEpgMenuItem});
            this.trayMenu.Name = "contextMenuStrip";
            this.trayMenu.Size = new System.Drawing.Size(221, 210);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(220, 26);
            this.exitMenuItem.Text = "終了(&X)";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(217, 6);
            // 
            // updateTunerMenuItem
            // 
            this.updateTunerMenuItem.Name = "updateTunerMenuItem";
            this.updateTunerMenuItem.Size = new System.Drawing.Size(220, 26);
            this.updateTunerMenuItem.Text = "チューナ更新(&T)";
            this.updateTunerMenuItem.Click += new System.EventHandler(this.updateTunerMenuItem_Click);
            // 
            // setupMenuItem
            // 
            this.setupMenuItem.Name = "setupMenuItem";
            this.setupMenuItem.Size = new System.Drawing.Size(220, 26);
            this.setupMenuItem.Text = "設定(&S)...";
            this.setupMenuItem.Click += new System.EventHandler(this.setupMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(217, 6);
            // 
            // stopEpgMenuItem
            // 
            this.stopEpgMenuItem.Name = "stopEpgMenuItem";
            this.stopEpgMenuItem.Size = new System.Drawing.Size(220, 26);
            this.stopEpgMenuItem.Text = "番組表更新を中止(&A)";
            this.stopEpgMenuItem.Click += new System.EventHandler(this.stopEpgMenuItem_Click);
            // 
            // startEpgMenuItem
            // 
            this.startEpgMenuItem.Name = "startEpgMenuItem";
            this.startEpgMenuItem.Size = new System.Drawing.Size(220, 26);
            this.startEpgMenuItem.Text = "番組表更新(&E)";
            this.startEpgMenuItem.Click += new System.EventHandler(this.startEpgMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(217, 6);
            // 
            // sleepMenuItem
            // 
            this.sleepMenuItem.Name = "sleepMenuItem";
            this.sleepMenuItem.Size = new System.Drawing.Size(220, 26);
            this.sleepMenuItem.Text = "スリープ(&S)";
            this.sleepMenuItem.Click += new System.EventHandler(this.sleepMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(217, 6);
            // 
            // openEpgMenuItem
            // 
            this.openEpgMenuItem.Name = "openEpgMenuItem";
            this.openEpgMenuItem.Size = new System.Drawing.Size(220, 26);
            this.openEpgMenuItem.Text = "番組表を開く(&O)";
            this.openEpgMenuItem.Click += new System.EventHandler(this.openEpgMenuItem_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Tvmaid";
            this.notifyIcon.Visible = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(245, 97);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(-100, -100);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Tvmaid MAYA";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateTunerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setupMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem stopEpgMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startEpgMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem sleepMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem openEpgMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    }
}