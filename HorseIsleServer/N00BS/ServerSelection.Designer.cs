namespace HISP
{
    partial class ServerSelection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerSelection));
            this.joinServer = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.serverIp = new System.Windows.Forms.TextBox();
            this.portNumber = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.portNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // joinServer
            // 
            this.joinServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.joinServer.Location = new System.Drawing.Point(10, 34);
            this.joinServer.Name = "joinServer";
            this.joinServer.Size = new System.Drawing.Size(576, 31);
            this.joinServer.TabIndex = 0;
            this.joinServer.Text = "Join Server";
            this.joinServer.UseVisualStyleBackColor = true;
            this.joinServer.Click += new System.EventHandler(this.joinServer_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Server IP:";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(389, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Server PORT:";
            // 
            // severIp
            // 
            this.serverIp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serverIp.Location = new System.Drawing.Point(73, 6);
            this.serverIp.Name = "severIp";
            this.serverIp.Size = new System.Drawing.Size(310, 23);
            this.serverIp.TabIndex = 5;
            this.serverIp.Text = "game.islehorse.com";
            // 
            // portNumber
            // 
            this.portNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.portNumber.Location = new System.Drawing.Point(468, 6);
            this.portNumber.Maximum = new decimal(new int[] {
            65565,
            0,
            0,
            0});
            this.portNumber.Name = "portNumber";
            this.portNumber.Size = new System.Drawing.Size(120, 23);
            this.portNumber.TabIndex = 6;
            this.portNumber.Value = new decimal(new int[] {
            12321,
            0,
            0,
            0});
            // 
            // ServerSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 77);
            this.Controls.Add(this.portNumber);
            this.Controls.Add(this.serverIp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.joinServer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(614, 116);
            this.Name = "ServerSelection";
            this.Text = "Server Selection";
            ((System.ComponentModel.ISupportInitialize)(this.portNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button joinServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serverIp;
        private System.Windows.Forms.NumericUpDown portNumber;
    }
}