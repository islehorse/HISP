using HISP.Noobs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HISP
{
    public partial class ServerSelection : Form
    {
        public ServerSelection()
        {
            InitializeComponent();
        }


        public void clientExited(object sender, EventArgs e)
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
        public void joinServer_Click(object sender, EventArgs e)
        {
            this.Hide();
            Process clientProcess = new Process();
            clientProcess.StartInfo.FileName = "flash.dll";
            clientProcess.StartInfo.Arguments = "http://" + Program.IP + "/horseisle.swf?SERVER=" + serverIp.Text + "&PORT=" + portNumber.Value.ToString();

            clientProcess.StartInfo.RedirectStandardOutput = true;
            clientProcess.StartInfo.RedirectStandardError = true;

            clientProcess.EnableRaisingEvents = true;
            clientProcess.Exited += clientExited;
            clientProcess.Start();
            clientProcess.WaitForExit();
        }
    }
}
