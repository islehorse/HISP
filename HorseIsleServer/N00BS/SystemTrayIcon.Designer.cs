namespace HISP
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
            this.serverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HispContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // HispNotifyIcon
            // 
            this.HispNotifyIcon.ContextMenuStrip = this.HispContextMenuStrip;
            this.HispNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("HispNotifyIcon.Icon")));
            this.HispNotifyIcon.Text = "Horse Isle";
            this.HispNotifyIcon.Visible = true;
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
            this.createNewUserToolStripMenuItem});
            this.usersToolStripMenuItem.Name = "usersToolStripMenuItem";
            this.usersToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.usersToolStripMenuItem.Text = "Users";
            // 
            // serverToolStripMenuItem
            // 
            this.serverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeServerToolStripMenuItem});
            this.serverToolStripMenuItem.Name = "serverToolStripMenuItem";
            this.serverToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.serverToolStripMenuItem.Text = "Server";
            // 
            // closeServerToolStripMenuItem
            // 
            this.closeServerToolStripMenuItem.Name = "closeServerToolStripMenuItem";
            this.closeServerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.closeServerToolStripMenuItem.Text = "Close Server";
            this.closeServerToolStripMenuItem.Click += new System.EventHandler(this.closeServerToolStripMenuItem_Click);
            // 
            // createNewUserToolStripMenuItem
            // 
            this.createNewUserToolStripMenuItem.Name = "createNewUserToolStripMenuItem";
            this.createNewUserToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.createNewUserToolStripMenuItem.Text = "Create New User";
            this.createNewUserToolStripMenuItem.Click += new System.EventHandler(this.createNewUserToolStripMenuItem_Click);
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
    }
}