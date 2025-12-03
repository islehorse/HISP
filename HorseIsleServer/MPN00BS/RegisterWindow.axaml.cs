using Avalonia;
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
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent(true);
        }


        private void ValidateInput()
        {
            if (ValidateUsername() && ValidatePassword())
                createAccountButton.IsEnabled = true;
            else
                createAccountButton.IsEnabled = false;
        }

        private bool ValidatePassword()
        {
            int selStart = passwordBox.SelectionStart;
            int selEnd = passwordBox.SelectionEnd;
            if (passwordBox.Text == null)
                return false;
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
            int selStart = usernameBox.SelectionStart;
            int selEnd = usernameBox.SelectionEnd;
            if (usernameBox.Text == null)
                return false;
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

            if (Database.CheckUserExist(usernameBox.Text))
            {
                usernameValidationFailReason.Content = "- Username is already in use.";
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

        private void adminChecked(object sender, RoutedEventArgs e)
        {
            if (modCheckbox == null)
                return;

            modCheckbox.IsChecked = true;
        }

        private void modUnchecked(object sender, RoutedEventArgs e)
        {
            if (adminCheckbox == null)
                return;
            adminCheckbox.IsChecked = false;
        }

        private void CreateAccount(object sender, RoutedEventArgs e)
        {
            
            // GENDer? I hardly knew THEM!
            string gender = ((ComboBoxItem)genderSelectionBox.SelectedItem).Content.ToString();
            
            // Permissions 
            bool admin = (bool)adminCheckbox.IsChecked;
            bool mod = (bool)modCheckbox.IsChecked;
            
            // Credentials
            string password = passwordBox.Text;
            string username = usernameBox.Text;

            Authentication.CreateAccount(username, password, gender, admin, mod);

            this.Close();
        }

    }
}
