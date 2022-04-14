namespace HISP
{
    partial class MpOrSp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpOrSp));
            this.Singleplayer = new System.Windows.Forms.Button();
            this.Multiplayer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Singleplayer
            // 
            this.Singleplayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Singleplayer.Location = new System.Drawing.Point(11, 12);
            this.Singleplayer.Name = "Singleplayer";
            this.Singleplayer.Size = new System.Drawing.Size(581, 32);
            this.Singleplayer.TabIndex = 0;
            this.Singleplayer.Text = "Play Singleplayer";
            this.Singleplayer.UseVisualStyleBackColor = true;
            this.Singleplayer.Click += new System.EventHandler(this.Singleplayer_Click);
            // 
            // Multiplayer
            // 
            this.Multiplayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Multiplayer.Location = new System.Drawing.Point(11, 50);
            this.Multiplayer.Name = "Multiplayer";
            this.Multiplayer.Size = new System.Drawing.Size(581, 32);
            this.Multiplayer.TabIndex = 1;
            this.Multiplayer.Text = "Play Multiplayer";
            this.Multiplayer.UseVisualStyleBackColor = true;
            this.Multiplayer.Click += new System.EventHandler(this.Multiplayer_Click);
            // 
            // MpOrSp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 94);
            this.Controls.Add(this.Multiplayer);
            this.Controls.Add(this.Singleplayer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 133);
            this.Name = "MpOrSp";
            this.Text = "Select Mode";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Singleplayer;
        private System.Windows.Forms.Button Multiplayer;
    }
}