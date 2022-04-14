namespace HISP.Noobs
{
    partial class ResetForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResetForm));
            this.label1 = new System.Windows.Forms.Label();
            this.Username = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.ResetPassword = new System.Windows.Forms.Button();
            this.UsernameValidationFailReason = new System.Windows.Forms.Label();
            this.PasswordValidationFailReason = new System.Windows.Forms.Label();
            this.SuspendLayout();
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
            this.Password.PlaceholderText = "New Password";
            this.Password.Size = new System.Drawing.Size(325, 23);
            this.Password.TabIndex = 2;
            this.Password.TextChanged += new System.EventHandler(this.Password_TextChanged);
            this.Password.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Password_KeyPress);
            // 
            // ResetPassword
            // 
            this.ResetPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetPassword.Enabled = false;
            this.ResetPassword.Location = new System.Drawing.Point(12, 118);
            this.ResetPassword.Name = "ResetPassword";
            this.ResetPassword.Size = new System.Drawing.Size(325, 29);
            this.ResetPassword.TabIndex = 7;
            this.ResetPassword.Text = "Reset Password";
            this.ResetPassword.UseVisualStyleBackColor = true;
            this.ResetPassword.Click += new System.EventHandler(this.ResetPassword_Click);
            // 
            // UsernameValidationFailReason
            // 
            this.UsernameValidationFailReason.AutoSize = true;
            this.UsernameValidationFailReason.ForeColor = System.Drawing.Color.Red;
            this.UsernameValidationFailReason.Location = new System.Drawing.Point(12, 82);
            this.UsernameValidationFailReason.Name = "UsernameValidationFailReason";
            this.UsernameValidationFailReason.Size = new System.Drawing.Size(127, 15);
            this.UsernameValidationFailReason.TabIndex = 12;
            this.UsernameValidationFailReason.Text = "- Username not found.";
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
            // ResetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 158);
            this.Controls.Add(this.PasswordValidationFailReason);
            this.Controls.Add(this.UsernameValidationFailReason);
            this.Controls.Add(this.ResetPassword);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Username);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(365, 197);
            this.Name = "ResetForm";
            this.Text = "Reset Password";
            this.Load += new System.EventHandler(this.RegisterForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Username;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Button ResetPassword;
        private System.Windows.Forms.Label UsernameValidationFailReason;
        private System.Windows.Forms.Label PasswordValidationFailReason;
    }
}