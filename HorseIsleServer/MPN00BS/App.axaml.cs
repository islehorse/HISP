using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HISP.Server;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MPN00BS
{
    public partial class App : Application
    {
#if OS_LINUX
        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);

#endif

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if OS_LINUX
            chmod(Path.Combine(Directory.GetCurrentDirectory(), "flashplayer", "LINUX", "flash.elf"), 511);
#endif
        }

        public bool CheckServerRunningAndShowMessage()
        {
            if (!ServerStarter.HasServerStarted)
            {
                MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                return true;
            }
            return false;
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) desktop.MainWindow = new MpOrSp();
            ConfigReader.OpenConfig();

            TrayIcon.GetIcons(Application.Current).First().Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Server").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Chat").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Swear Filter").IsChecked = ConfigReader.EnableSwearFilter;
            TrayIcon.GetIcons(Application.Current).First().Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Server").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Chat").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Corrections").IsChecked = ConfigReader.EnableCorrections;
            TrayIcon.GetIcons(Application.Current).First().Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Server").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Chat").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Non Violation Checks").IsChecked = ConfigReader.EnableNonViolations;
            TrayIcon.GetIcons(Application.Current).First().Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Server").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Chat").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Spam Filter").IsChecked = ConfigReader.EnableSpamFilter;
            
            TrayIcon.GetIcons(Application.Current).First().Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Server").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Game").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "All Users Subscribed").IsChecked = ConfigReader.AllUsersSubbed;
            TrayIcon.GetIcons(Application.Current).First().Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Server").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Game").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Fix Offical Bugs").IsChecked = ConfigReader.FixOfficalBugs;
            TrayIcon.GetIcons(Application.Current).First().Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Server").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Game").Menu.Select(o => (NativeMenuItem)o).First(o => o.Header == "Enable EOL Features").IsChecked = ConfigReader.EnableEolFeatures;

            base.OnFrameworkInitializationCompleted();
        }

        public void createAccountCommand(object? sender, EventArgs e)
        {
            new RegisterWindow().Show();
        }

        public void resetPasswordCommand(object? sender, EventArgs e)
        {
            if (!ServerStarter.HasServerStarted)
            {
                if (CheckServerRunningAndShowMessage()) return;
                MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            new ResetWindow().Show();
        }


        public void editServerPropertiesCommand(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            Process p = new Process();
            p.StartInfo.FileName = ConfigReader.ConfigurationFileName;
            p.StartInfo.UseShellExecute = true;
            p.Start();

        }

        public void openServerFolderCommand(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            Process p = new Process();
            p.StartInfo.FileName = ConfigReader.ConfigDirectory;
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }


        public void shutdownServerCommand(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            GameServer.ShutdownServer();

        }

        public void toggleSwearFilter(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            bool enab = !ConfigReader.EnableSwearFilter;
            ServerStarter.ModifyConfig("enable_word_filter", enab.ToString().ToLowerInvariant());
            ConfigReader.EnableSwearFilter = enab;

            ((NativeMenuItem)sender).IsChecked = enab;

        }

        public void toggleCorrections(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            bool enab = !ConfigReader.EnableCorrections;
            ServerStarter.ModifyConfig("enable_corrections", enab.ToString().ToLowerInvariant());
            ConfigReader.EnableCorrections = enab;

            ((NativeMenuItem)sender).IsChecked = enab;

        }


        public void toggleNonVioChecks(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            bool enab = !ConfigReader.EnableNonViolations;
            ServerStarter.ModifyConfig("enable_non_violation_check", enab.ToString().ToLowerInvariant());
            ConfigReader.EnableNonViolations = enab;

            ((NativeMenuItem)sender).IsChecked = enab;

        }

        public void toggleSpamFilter(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            bool enab = !ConfigReader.EnableSpamFilter;
            ServerStarter.ModifyConfig("enable_spam_filter", enab.ToString().ToLowerInvariant());
            ConfigReader.EnableSpamFilter = enab;

            ((NativeMenuItem)sender).IsChecked = enab;

        }

        public void toggleAllUsersSubbed(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            bool enab = !ConfigReader.AllUsersSubbed;
            ServerStarter.ModifyConfig("all_users_subscribed", enab.ToString().ToLowerInvariant());
            ConfigReader.AllUsersSubbed = enab;

            ((NativeMenuItem)sender).IsChecked = enab;
        }

        public void toggleFixOfficalBugs(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            bool enab = !ConfigReader.FixOfficalBugs;
            ServerStarter.ModifyConfig("fix_offical_bugs", enab.ToString().ToLowerInvariant());
            ConfigReader.FixOfficalBugs = enab;

            ((NativeMenuItem)sender).IsChecked = enab;
        }

        public void toggleEolFeatures(object? sender, EventArgs e)
        {
            if (CheckServerRunningAndShowMessage()) return;

            bool enab = !ConfigReader.EnableEolFeatures;
            ServerStarter.ModifyConfig("enable_eol_features", enab.ToString().ToLowerInvariant());
            ConfigReader.FixOfficalBugs = enab;

            ((NativeMenuItem)sender).IsChecked = enab;
        }

    }
}
