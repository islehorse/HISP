namespace HISP.Noobs
{
    partial class RegisterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterForm));
            this.isAdmin = new System.Windows.Forms.CheckBox();
            this.isMod = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Username = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.BoySelecton = new System.Windows.Forms.RadioButton();
            this.GirlSelection = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.CreateAccount = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.UsernameValidationFailReason = new System.Windows.Forms.Label();
            this.PasswordValidationFailReason = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // isAdmin
            // 
            this.isAdmin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.isAdmin.AutoSize = true;
            this.isAdmin.Checked = true;
            this.isAdmin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.isAdmin.Location = new System.Drawing.Point(238, 169);
            this.isAdmin.Name = "isAdmin";
            this.isAdmin.Size = new System.Drawing.Size(99, 19);
            this.isAdmin.TabIndex = 6;
            this.isAdmin.Text = "Administrator";
            this.isAdmin.UseVisualStyleBackColor = true;
            this.isAdmin.CheckedChanged += new System.EventHandler(this.isAdmin_CheckedChanged);
            // 
            // isMod
            // 
            this.isMod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.isMod.AutoSize = true;
            this.isMod.Checked = true;
            this.isMod.CheckState = System.Windows.Forms.CheckState.Checked;
            this.isMod.Location = new System.Drawing.Point(238, 145);
            this.isMod.Name = "isMod";
            this.isMod.Size = new System.Drawing.Size(82, 19);
            this.isMod.TabIndex = 5;
            this.isMod.Text = "Moderator";
            this.isMod.UseVisualStyleBackColor = true;
            this.isMod.CheckedChanged += new System.EventHandler(this.isMod_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "User Details:";
            // 
            // Username
            // 
            this.Username.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Username.Location = new System.Drawing.Point(12, 27);
            this.Username.MaxLength = 16;
            this.Username.Name = "Username";
            this.Username.PlaceholderText = "Username";
            this.Username.Size = new System.Drawing.Size(325, 23);
            this.Username.TabIndex = 1;
            this.Username.TextChanged += new System.EventHandler(this.Username_TextChanged);
            this.Username.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Username_KeyPress);
            // 
            // Password
            // 
            this.Password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Password.Location = new System.Drawing.Point(12, 56);
            this.Password.MaxLength = 16;
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.PlaceholderText = "Password";
            this.Password.Size = new System.Drawing.Size(325, 23);
            this.Password.TabIndex = 2;
            this.Password.TextChanged += new System.EventHandler(this.Password_TextChanged);
            this.Password.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Password_KeyPress);
            // 
            // BoySelecton
            // 
            this.BoySelecton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BoySelecton.AutoSize = true;
            this.BoySelecton.Location = new System.Drawing.Point(17, 168);
            this.BoySelecton.Name = "BoySelecton";
            this.BoySelecton.Size = new System.Drawing.Size(45, 19);
            this.BoySelecton.TabIndex = 4;
            this.BoySelecton.TabStop = true;
            this.BoySelecton.Text = "Boy";
            this.BoySelecton.UseVisualStyleBackColor = true;
            // 
            // GirlSelection
            // 
            this.GirlSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GirlSelection.AutoSize = true;
            this.GirlSelection.Checked = true;
            this.GirlSelection.Location = new System.Drawing.Point(17, 144);
            this.GirlSelection.Name = "GirlSelection";
            this.GirlSelection.Size = new System.Drawing.Size(43, 19);
            this.GirlSelection.TabIndex = 3;
            this.GirlSelection.TabStop = true;
            this.GirlSelection.Text = "Girl";
            this.GirlSelection.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 121);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "Gender:";
            // 
            // CreateAccount
            // 
            this.CreateAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateAccount.Enabled = false;
            this.CreateAccount.Location = new System.Drawing.Point(12, 201);
            this.CreateAccount.Name = "CreateAccount";
            this.CreateAccount.Size = new System.Drawing.Size(325, 29);
            this.CreateAccount.TabIndex = 7;
            this.CreateAccount.Text = "Create Account";
            this.CreateAccount.UseVisualStyleBackColor = true;
            this.CreateAccount.Click += new System.EventHandler(this.CreateAccount_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(238, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 15);
            this.label3.TabIndex = 11;
            this.label3.Text = "Privledges:";
            // 
            // UsernameValidationFailReason
            // 
            this.UsernameValidationFailReason.AutoSize = true;
            this.UsernameValidationFailReason.ForeColor = System.Drawing.Color.Red;
            this.UsernameValidationFailReason.Location = new System.Drawing.Point(12, 82);
            this.UsernameValidationFailReason.Name = "UsernameValidationFailReason";
            this.UsernameValidationFailReason.Size = new System.Drawing.Size(241, 15);
            this.UsernameValidationFailReason.TabIndex = 12;
            this.UsernameValidationFailReason.Text = "- Username must be more than 3 characters.";
            // 
            // PasswordValidationFailReason
            // 
            this.PasswordValidationFailReason.AutoSize = true;
            this.PasswordValidationFailReason.ForeColor = System.Drawing.Color.Red;
            this.PasswordValidationFailReason.Location = new System.Drawing.Point(12, 97);
            this.PasswordValidationFailReason.Name = "PasswordValidationFailReason";
            this.PasswordValidationFailReason.Size = new System.Drawing.Size(238, 15);
            this.PasswordValidationFailReason.TabIndex = 13;
            this.PasswordValidationFailReason.Text = "- Password must be more than 6 characters.";
            // 
            // RegisterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 241);
            this.Controls.Add(this.PasswordValidationFailReason);
            this.Controls.Add(this.UsernameValidationFailReason);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CreateAccount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.GirlSelection);
            this.Controls.Add(this.BoySelecton);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Username);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.isMod);
            this.Controls.Add(this.isAdmin);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(365, 280);
            this.Name = "RegisterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.Text = "Create Account";
            this.Load += new System.EventHandler(this.RegisterForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox isAdmin;
        private System.Windows.Forms.CheckBox isMod;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Username;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.RadioButton BoySelecton;
        private System.Windows.Forms.RadioButton GirlSelection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button CreateAccount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label UsernameValidationFailReason;
        private System.Windows.Forms.Label PasswordValidationFailReason;
    }
}