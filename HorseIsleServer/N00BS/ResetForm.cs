using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HISP.Security;
using HISP.Server;

namespace HISP
{
    public partial class ResetForm : Form
    {
        public ResetForm()
        {
            InitializeComponent();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void ValidateInput()
        {
            if(ValidateUsername() && ValidatePassword())
                ResetPassword.Enabled = true;
            else
                ResetPassword.Enabled = false;
        }

        private bool ValidatePassword()
        {
            int selStart = Password.SelectionStart;
            int selLen = Password.SelectionLength;
            Password.Text = Regex.Replace(Password.Text, "[^A-Za-z0-9]", "");
            Password.SelectionStart = selStart;
            Password.SelectionLength = selLen;

            if (Password.Text.Length < 6)
            {
                PasswordValidationFailReason.Text = "- Password must be more than 6 characters.";
                return false;
            }

            if (Password.Text.Length >= 16)
            {
                PasswordValidationFailReason.Text = "- Password must be less than 16 characters.";
                return false;
            }

            PasswordValidationFailReason.Text = "";
            return true;
        }

        private bool ValidateUsername()
        {
            int selStart = Username.SelectionStart;
            int selLen = Username.SelectionLength;
            Username.Text = Regex.Replace(Username.Text, "[^A-Za-z]", "");
            Username.SelectionStart = selStart;
            Username.SelectionLength = selLen;

            if (Username.Text.Length < 3)
            {
                UsernameValidationFailReason.Text = "- Username must be more than 3 characters.";
                return false;
            }

            if (Username.Text.Length >= 16)
            {
                UsernameValidationFailReason.Text = "- Username must be less than 16 characters.";
                return false;
            }

            if (Regex.IsMatch(Username.Text, "[A-Z]{2,}"))
            {
                UsernameValidationFailReason.Text = "- Username have the first letter of each word capitalized.";
                return false;
            }

            if (Username.Text.ToUpper()[0] != Username.Text[0])
            {
                UsernameValidationFailReason.Text = "- Username have the first letter of each word capitalized.";
                return false;
            }

            if (!Database.CheckUserExist(Username.Text))
            {
                UsernameValidationFailReason.Text = "- Username not found.";
                return false;
            }

            UsernameValidationFailReason.Text = "";
            return true;
        }

        private void Username_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void ResetPassword_Click(object sender, EventArgs e)
        {
            // Get salt
            byte[] salt = Database.GetPasswordSalt(Username.Text);

            // Hash password
            string hashsalt = BitConverter.ToString(Authentication.HashAndSalt(Password.Text, salt)).Replace("-", "");

            Database.SetPasswordHash(Username.Text, hashsalt);
            this.DialogResult = DialogResult.OK;
        }
        private void Password_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)Keys.Return)
            {
                if (ResetPassword.Enabled)
                {
                    e.Handled = true;
                    ResetPassword_Click(null, null);
                }

            }

        }

        private void Username_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (ResetPassword.Enabled)
                {
                    e.Handled = true;
                    ResetPassword_Click(null, null);
                }

            }
        }

    }
}
