using HISP.Server;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace HISP.Noobs
{
    public partial class SystemTrayIcon : Form
    {
        Process clientProcess = new Process();

        public SystemTrayIcon()
        {
            InitializeComponent();
            disableSwearFilterToolStripMenuItem.Checked = !ConfigReader.BadWords;
            disableCorrectionsToolStripMenuItem.Checked = !ConfigReader.DoCorrections;
            disableNonvioChecksToolStripMenuItem.Checked = !ConfigReader.DoNonViolations;
            disableSpamFilterToolStripMenuItem.Checked = !ConfigReader.EnableSpamFilter;

            allUsersSubscribedToolStripMenuItem.Checked = ConfigReader.AllUsersSubbed;
            fixOfficalBugsToolStripMenuItem.Checked = ConfigReader.FixOfficalBugs;
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

            string serverIp = ConfigReader.BindIP;
            if (serverIp == "0.0.0.0")
                serverIp = "127.0.0.1";

            clientProcess.StartInfo.Arguments = "http://127.0.0.1/horseisle.swf?SERVER=" + serverIp + "&PORT=" + ConfigReader.Port;

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

        private void editServerPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "notepad.exe";
            p.StartInfo.Arguments = Path.Combine(Program.BaseDir, "server.properties");
            p.Start();
           
        }

        private void openServerFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = Program.BaseDir;
            p.Start();
        }

        private void ModifyConfig(string okey, string value)
        {
            string[] configFile = File.ReadAllLines(ConfigReader.ConfigurationFileName);
            for (int i = 0; i < configFile.Length; i++)
            {
                string setting = configFile[i];

                if (setting.Length < 1)
                    continue;
                if (setting[0] == '#')
                    continue;
                if (!setting.Contains("="))
                    continue;

                string[] dataPair = setting.Split('=');

                string key = dataPair[0];

                if (key == okey)
                {
                    dataPair[1] = value;
                    configFile[i] = string.Join('=', dataPair);
                }
            }
            File.WriteAllLines(ConfigReader.ConfigurationFileName, configFile);
        }

        private void resetUserPasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetForm frm = new ResetForm();
            frm.ShowDialog();
        }

        private void HispNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            HispNotifyIcon.ContextMenuStrip.Show();
        }

        private void disableSwearFilterToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool enab = !disableSwearFilterToolStripMenuItem.Checked;
            ModifyConfig("enable_word_filter", enab.ToString().ToLowerInvariant());
            ConfigReader.BadWords = enab;
        }

        private void disableCorrectionsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool enab = !disableCorrectionsToolStripMenuItem.Checked;
            ModifyConfig("enable_corrections", enab.ToString().ToLowerInvariant());
            ConfigReader.DoCorrections = enab;
        }

        private void disableNonvioChecksToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool enab = !disableNonvioChecksToolStripMenuItem.Checked;
            ModifyConfig("enable_non_violation_check", enab.ToString().ToLowerInvariant());
            ConfigReader.DoNonViolations = enab;
        }

        private void disableSpamFilterToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool enab = !disableSpamFilterToolStripMenuItem.Checked;
            ModifyConfig("enable_spam_filter", enab.ToString().ToLowerInvariant());
            ConfigReader.EnableSpamFilter = enab;
        }

        private void allUsersSubscribedToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool enab = allUsersSubscribedToolStripMenuItem.Checked;
            ModifyConfig("all_users_subscribed", enab.ToString().ToLowerInvariant());
            ConfigReader.AllUsersSubbed = enab;
        }

        private void fixOfficalBugsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            bool enab = fixOfficalBugsToolStripMenuItem.Checked;
            ModifyConfig("fix_offical_bugs", enab.ToString().ToLowerInvariant());
            ConfigReader.FixOfficalBugs = enab;
        }

    }
}
