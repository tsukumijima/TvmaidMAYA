namespace Tvmaid
{
    partial class SleepCountdown
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.countLabel = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.sleepButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.wakeTimeLable = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(429, 92);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(99, 31);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // countLabel
            // 
            this.countLabel.AutoSize = true;
            this.countLabel.Location = new System.Drawing.Point(27, 23);
            this.countLabel.Name = "countLabel";
            this.countLabel.Size = new System.Drawing.Size(86, 19);
            this.countLabel.TabIndex = 0;
            this.countLabel.Text = "カウントダウン";
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // sleepButton
            // 
            this.sleepButton.Location = new System.Drawing.Point(324, 92);
            this.sleepButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sleepButton.Name = "sleepButton";
            this.sleepButton.Size = new System.Drawing.Size(99, 31);
            this.sleepButton.TabIndex = 3;
            this.sleepButton.Text = "スリープ";
            this.sleepButton.UseVisualStyleBackColor = true;
            this.sleepButton.Click += new System.EventHandler(this.sleepButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(31, 64);
            this.progressBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressBar.Maximum = 60;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(497, 10);
            this.progressBar.TabIndex = 1;
            // 
            // wakeTimeLable
            // 
            this.wakeTimeLable.AutoSize = true;
            this.wakeTimeLable.Location = new System.Drawing.Point(27, 98);
            this.wakeTimeLable.Name = "wakeTimeLable";
            this.wakeTimeLable.Size = new System.Drawing.Size(69, 19);
            this.wakeTimeLable.TabIndex = 2;
            this.wakeTimeLable.Text = "復帰予定";
            // 
            // SleepCountdown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(558, 142);
            this.ControlBox = false;
            this.Controls.Add(this.wakeTimeLable);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.sleepButton);
            this.Controls.Add(this.countLabel);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SleepCountdown";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tvmaid";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label countLabel;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Button sleepButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label wakeTimeLable;
    }
}