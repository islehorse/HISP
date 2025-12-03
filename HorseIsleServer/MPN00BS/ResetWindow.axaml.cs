using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HISP.Security;
using HISP.Server;
using System;
using System.Text.RegularExpressions;

namespace MPN00BS
{
    public partial class ResetWindow : Window
    {
        public ResetWindow()
        {
            InitializeComponent(true);
        }

        private void ValidateInput()
        {
            if (ValidateUsername() && ValidatePassword())
                resetPasswordButton.IsEnabled = true;
            else
                resetPasswordButton.IsEnabled = false;
        }

        private bool ValidatePassword()
        {
            if (passwordBox == null)
                return false;
            if (passwordBox.Text == null)
                return false;

            int selStart = passwordBox.SelectionStart;
            int selEnd = passwordBox.SelectionEnd;
            passwordBox.Text = Regex.Replace(passwordBox.Text, "[^A-Za-z0-9]", "");
            passwordBox.SelectionStart = selStart;
            passwordBox.SelectionEnd = selEnd;

            if (passwordBox.Text.Length < 6)
            {
                passwordValidationFailReason.Content = "- Password must be more than 6 characters.";
                return false;
            }

            if (passwordBox.Text.Length >= 16)
            {
                passwordValidationFailReason.Content = "- Password must be less than 16 characters.";
                return false;
            }

            passwordValidationFailReason.Content = "";
            return true;
        }

        private bool ValidateUsername()
        {
            if (usernameBox == null)
                return false;
            if (usernameBox.Text == null)
                return false;
            int selStart = usernameBox.SelectionStart;
            int selEnd = usernameBox.SelectionEnd;
            usernameBox.Text = Regex.Replace(usernameBox.Text, "[^A-Za-z]", "");
            usernameBox.SelectionStart = selStart;
            usernameBox.SelectionEnd = selEnd;

            if (usernameBox.Text.Length < 3)
            {
                usernameValidationFailReason.Content = "- Username must be more than 3 characters.";
                return false;
            }

            if (usernameBox.Text.Length >= 16)
            {
                usernameValidationFailReason.Content = "- Username must be less than 16 characters.";
                return false;
            }

            if (Regex.IsMatch(usernameBox.Text, "[A-Z]{2,}"))
            {
                usernameValidationFailReason.Content = "- Username have the first letter of each word capitalized.";
                return false;
            }

            if (usernameBox.Text.ToUpper()[0] != usernameBox.Text[0])
            {
                usernameValidationFailReason.Content = "- Username have the first letter of each word capitalized.";
                return false;
            }

            if (!Database.CheckUserExist(usernameBox.Text))
            {
                usernameValidationFailReason.Content = "- Username not found.";
                return false;
            }

            usernameValidationFailReason.Content = "";
            return true;
        }

        private void usernameChanged(object sender, KeyEventArgs e)
        {
            if (usernameBox == null)
                return;

            ValidateInput();
        }
        private void passwordChanged(object sender, KeyEventArgs e)
        {
            if (passwordBox == null)
                return;

            ValidateInput();

        }
        private void ResetPassword(object sender, RoutedEventArgs e)
        {
            Authentication.ChangePassword(usernameBox.Text, passwordBox.Text);
            this.Close();
        }

    }

}
