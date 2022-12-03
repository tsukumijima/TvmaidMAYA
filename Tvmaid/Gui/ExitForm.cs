using System;
using System.Windows.Forms;

namespace Tvmaid
{
    public partial class ExitForm : Form
    {
        public ExitForm(int timeout)
        {
            InitializeComponent();

            this.progressBar.Maximum = timeout;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (progressBar.Maximum > progressBar.Value + 1)
                this.progressBar.Value++;
        }
    }
}
