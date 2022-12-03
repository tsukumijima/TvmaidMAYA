using System;
using System.Windows.Forms;

namespace Tvmaid
{
    public partial class SleepCountdown : Form
    {
        int count = 60;

        public SleepCountdown(DateTime wakeTime)
        {
            InitializeComponent();

            wakeTimeLable.Text = "復帰予定 " + wakeTime.ToString("MM/dd HH:mm");
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            countLabel.Text = "スリープまで {0} 秒".Formatex(count);
            progressBar.Value = progressBar.Maximum - count;

            if (count == 0)
            {
                this.DialogResult = DialogResult.OK;
                timer.Stop();
            }
            else count--;
        }

        private void sleepButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
