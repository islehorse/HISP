using HISP.Server;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace HISP
{
    public partial class SystemTrayIcon : Form
    {
        Process clientProcess = new Process();

        public SystemTrayIcon()
        {
            InitializeComponent();
        }

        private void createNewUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegisterForm frm = new RegisterForm();
            frm.ShowDialog();
        }

        private void closeServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SystemTrayIcon_Load(object sender, EventArgs e)
        {
            clientProcess.StartInfo.FileName = "flash.dll";
            clientProcess.StartInfo.Arguments = "http://127.0.0.1/horseisle.swf?SERVER=127.0.0.1&PORT=12321";

            clientProcess.StartInfo.RedirectStandardOutput = true;
            clientProcess.StartInfo.RedirectStandardError = true;

            clientProcess.EnableRaisingEvents = true;
            clientProcess.Exited += clientExited;
            clientProcess.Start();

        }

        private void clientExited(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() =>
                {
                    this.Close();
                });
            }
            else
            {
                this.Close();
            }

            
        }

        private void SystemTrayIcon_FormClosing(object sender, FormClosingEventArgs e)
        {
            HispNotifyIcon.Visible = false;
            clientProcess.Kill();
        }
    }
}
