namespace HISP.Noobs
{
    partial class SystemTrayIcon
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemTrayIcon));
            this.HispNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.HispContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.usersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetUserPasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableSwearFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableCorrectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableNonvioChecksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableSpamFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allUsersSubscribedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixOfficalBugsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editServerPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openServerFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HispContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // HispNotifyIcon
            // 
            this.HispNotifyIcon.ContextMenuStrip = this.HispContextMenuStrip;
            this.HispNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("HispNotifyIcon.Icon")));
            this.HispNotifyIcon.Text = "Horse Isle";
            this.HispNotifyIcon.Visible = true;
            this.HispNotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HispNotifyIcon_MouseClick);
            // 
            // HispContextMenuStrip
            // 
            this.HispContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.usersToolStripMenuItem,
            this.serverToolStripMenuItem});
            this.HispContextMenuStrip.Name = "HispContextMenuStrip";
            this.HispContextMenuStrip.Size = new System.Drawing.Size(107, 48);
            // 
            // usersToolStripMenuItem
            // 
            this.usersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewUserToolStripMenuItem,
            this.resetUserPasswordToolStripMenuItem});
            this.usersToolStripMenuItem.Name = "usersToolStripMenuItem";
            this.usersToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.usersToolStripMenuItem.Text = "Users";
            // 
            // createNewUserToolStripMenuItem
            // 
            this.createNewUserToolStripMenuItem.Name = "createNewUserToolStripMenuItem";
            this.createNewUserToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.createNewUserToolStripMenuItem.Text = "Create new user";
            this.createNewUserToolStripMenuItem.Click += new System.EventHandler(this.createNewUserToolStripMenuItem_Click);
            // 
            // resetUserPasswordToolStripMenuItem
            // 
            this.resetUserPasswordToolStripMenuItem.Name = "resetUserPasswordToolStripMenuItem";
            this.resetUserPasswordToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.resetUserPasswordToolStripMenuItem.Text = "Reset user password";
            this.resetUserPasswordToolStripMenuItem.Click += new System.EventHandler(this.resetUserPasswordToolStripMenuItem_Click);
            // 
            // serverToolStripMenuItem
            // 
            this.serverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeServerToolStripMenuItem,
            this.configToolStripMenuItem,
            this.gameToolStripMenuItem,
            this.advancedToolStripMenuItem});
            this.serverToolStripMenuItem.Name = "serverToolStripMenuItem";
            this.serverToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.serverToolStripMenuItem.Text = "Server";
            // 
            // closeServerToolStripMenuItem
            // 
            this.closeServerToolStripMenuItem.Name = "closeServerToolStripMenuItem";
            this.closeServerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.closeServerToolStripMenuItem.Text = "Shutdown server";
            this.closeServerToolStripMenuItem.Click += new System.EventHandler(this.closeServerToolStripMenuItem_Click);
            // 
            // configToolStripMenuItem
            // 
            this.configToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disableSwearFilterToolStripMenuItem,
            this.disableCorrectionsToolStripMenuItem,
            this.disableNonvioChecksToolStripMenuItem,
            this.disableSpamFilterToolStripMenuItem});
            this.configToolStripMenuItem.Name = "configToolStripMenuItem";
            this.configToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.configToolStripMenuItem.Text = "Chat";
            // 
            // disableSwearFilterToolStripMenuItem
            // 
            this.disableSwearFilterToolStripMenuItem.CheckOnClick = true;
            this.disableSwearFilterToolStripMenuItem.Name = "disableSwearFilterToolStripMenuItem";
            this.disableSwearFilterToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.disableSwearFilterToolStripMenuItem.Text = "Disable swear filter";
            this.disableSwearFilterToolStripMenuItem.CheckedChanged += new System.EventHandler(this.disableSwearFilterToolStripMenuItem_CheckedChanged);
            // 
            // disableCorrectionsToolStripMenuItem
            // 
            this.disableCorrectionsToolStripMenuItem.CheckOnClick = true;
            this.disableCorrectionsToolStripMenuItem.Name = "disableCorrectionsToolStripMenuItem";
            this.disableCorrectionsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.disableCorrectionsToolStripMenuItem.Text = "Disable corrections";
            this.disableCorrectionsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.disableCorrectionsToolStripMenuItem_CheckedChanged);
            // 
            // disableNonvioChecksToolStripMenuItem
            // 
            this.disableNonvioChecksToolStripMenuItem.CheckOnClick = true;
            this.disableNonvioChecksToolStripMenuItem.Name = "disableNonvioChecksToolStripMenuItem";
            this.disableNonvioChecksToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.disableNonvioChecksToolStripMenuItem.Text = "Disable non-vio checks";
            this.disableNonvioChecksToolStripMenuItem.CheckedChanged += new System.EventHandler(this.disableNonvioChecksToolStripMenuItem_CheckedChanged);
            // 
            // disableSpamFilterToolStripMenuItem
            // 
            this.disableSpamFilterToolStripMenuItem.CheckOnClick = true;
            this.disableSpamFilterToolStripMenuItem.Name = "disableSpamFilterToolStripMenuItem";
            this.disableSpamFilterToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.disableSpamFilterToolStripMenuItem.Text = "Disable spam filter";
            this.disableSpamFilterToolStripMenuItem.CheckedChanged += new System.EventHandler(this.disableSpamFilterToolStripMenuItem_CheckedChanged);
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allUsersSubscribedToolStripMenuItem,
            this.fixOfficalBugsToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.gameToolStripMenuItem.Text = "Game";
            // 
            // allUsersSubscribedToolStripMenuItem
            // 
            this.allUsersSubscribedToolStripMenuItem.CheckOnClick = true;
            this.allUsersSubscribedToolStripMenuItem.Name = "allUsersSubscribedToolStripMenuItem";
            this.allUsersSubscribedToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.allUsersSubscribedToolStripMenuItem.Text = "All users subscribed";
            this.allUsersSubscribedToolStripMenuItem.CheckedChanged += new System.EventHandler(this.allUsersSubscribedToolStripMenuItem_CheckedChanged);
            // 
            // fixOfficalBugsToolStripMenuItem
            // 
            this.fixOfficalBugsToolStripMenuItem.CheckOnClick = true;
            this.fixOfficalBugsToolStripMenuItem.Name = "fixOfficalBugsToolStripMenuItem";
            this.fixOfficalBugsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.fixOfficalBugsToolStripMenuItem.Text = "Fix offical bugs";
            this.fixOfficalBugsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.fixOfficalBugsToolStripMenuItem_CheckedChanged);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editServerPropertiesToolStripMenuItem,
            this.openServerFolderToolStripMenuItem});
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.advancedToolStripMenuItem.Text = "Advanced";
            // 
            // editServerPropertiesToolStripMenuItem
            // 
            this.editServerPropertiesToolStripMenuItem.Name = "editServerPropertiesToolStripMenuItem";
            this.editServerPropertiesToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.editServerPropertiesToolStripMenuItem.Text = "Edit server.properties";
            this.editServerPropertiesToolStripMenuItem.Click += new System.EventHandler(this.editServerPropertiesToolStripMenuItem_Click);
            // 
            // openServerFolderToolStripMenuItem
            // 
            this.openServerFolderToolStripMenuItem.Name = "openServerFolderToolStripMenuItem";
            this.openServerFolderToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.openServerFolderToolStripMenuItem.Text = "Open server folder";
            this.openServerFolderToolStripMenuItem.Click += new System.EventHandler(this.openServerFolderToolStripMenuItem_Click);
            // 
            // SystemTrayIcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(90, 92);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SystemTrayIcon";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "SystemTrayIcon";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SystemTrayIcon_FormClosing);
            this.Load += new System.EventHandler(this.SystemTrayIcon_Load);
            this.HispContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon HispNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip HispContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem usersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createNewUserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetUserPasswordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editServerPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openServerFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableSwearFilterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableCorrectionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableNonvioChecksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableSpamFilterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allUsersSubscribedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixOfficalBugsToolStripMenuItem;
    }
}