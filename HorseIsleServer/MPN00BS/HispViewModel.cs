using HISP.Server;
using MiniMvvm;
using System;
using System.Diagnostics;
using System.IO;

namespace MPN00BS.ViewModels
{
    public class HispViewModel : ViewModelBase
    {
        
        public void RefreshNames()
        {
            swearFilterHeader = (ConfigReader.EnableSwearFilter ? "Disable" : "Enable") + " Swear Filter";
            correctionsHeader = (ConfigReader.EnableCorrections ? "Disable" : "Enable") + " Corrections";
            vioChecksHeader = (ConfigReader.EnableNonViolations ? "Disable" : "Enable") + " Non-Vio Checks";
            spamFilterHeader = (ConfigReader.EnableSpamFilter ? "Disable" : "Enable") + " Spam Filter";
            allUsersSubbedHeader = (ConfigReader.AllUsersSubbed ? "Disable" : "Enable") + " All Users Subscribed";
            fixOfficalBugsHeader = (ConfigReader.FixOfficalBugs ? "Disable" : "Enable") + " Fixing Offical Bugs";
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

        public HispViewModel()
        {
            ServerStarter.ReadServerProperties();
            RefreshNames();

            createAccountCommand = MiniCommand.Create(() =>
            {
                
                new RegisterWindow().Show();
            });

            resetPasswordCommand = MiniCommand.Create(() =>
            {
                if (!ServerStarter.HasServerStarted)
                {
                    if (CheckServerRunningAndShowMessage()) return;
                    MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                new ResetWindow().Show();
            });

            editServerPropertiesCommand = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(ServerStarter.BaseDir, "server.properties");
                p.StartInfo.UseShellExecute = true;
                p.Start();

            });

            openServerFolderCommand = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                Process p = new Process();
                p.StartInfo.FileName = ServerStarter.BaseDir;
                p.StartInfo.UseShellExecute = true;
                p.Start();
            });

            shutdownServerCommand = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                GameServer.ShutdownServer();
            });


            toggleSwearFilter = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                bool enab = !ConfigReader.EnableSwearFilter;
                ServerStarter.ModifyConfig("enable_word_filter", enab.ToString().ToLowerInvariant());
                ConfigReader.EnableSwearFilter = enab;
                RefreshNames();
            });

            toggleCorrections = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                bool enab = !ConfigReader.EnableCorrections;
                ServerStarter.ModifyConfig("enable_corrections", enab.ToString().ToLowerInvariant());
                ConfigReader.EnableCorrections = enab;
                RefreshNames();
            });

            toggleNonVioChecks = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                bool enab = !ConfigReader.EnableNonViolations;
                ServerStarter.ModifyConfig("enable_non_violation_check", enab.ToString().ToLowerInvariant());
                ConfigReader.EnableNonViolations = enab;
                RefreshNames();
            });

            toggleSpamFilter = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                bool enab = !ConfigReader.EnableSpamFilter;
                ServerStarter.ModifyConfig("enable_spam_filter", enab.ToString().ToLowerInvariant());
                ConfigReader.EnableSpamFilter = enab;
                RefreshNames();
            });


            toggleAllUsersSubbed = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                bool enab = !ConfigReader.AllUsersSubbed;
                ServerStarter.ModifyConfig("all_users_subscribed", enab.ToString().ToLowerInvariant());
                ConfigReader.AllUsersSubbed = enab;
                RefreshNames();
            });


            toggleFixOfficalBugs = MiniCommand.Create(() =>
            {
                if (CheckServerRunningAndShowMessage()) return;

                bool enab = !ConfigReader.FixOfficalBugs;
                ServerStarter.ModifyConfig("fix_offical_bugs", enab.ToString().ToLowerInvariant());
                ConfigReader.FixOfficalBugs = enab;
                RefreshNames();
            });
        }


        // Binding variables

        private String _swearFilterHeader;
        public String swearFilterHeader
        {
            get
            {
                return _swearFilterHeader;
            }
            set
            {
                RaiseAndSetIfChanged(ref _swearFilterHeader, value);
            }
        }

        private String _correctionsHeader;
        public String correctionsHeader
        {
            get
            {
                return _correctionsHeader;
            }
            set
            {
                RaiseAndSetIfChanged(ref _correctionsHeader, value);
            }
        }
        private String _vioChecksHeader;
        public String vioChecksHeader
        {
            get
            {
                return _vioChecksHeader;
            }
            set
            {
                RaiseAndSetIfChanged(ref _vioChecksHeader, value);
            }
        }

        private String _spamFilterHeader;
        public String spamFilterHeader
        {
            get
            {
                return _spamFilterHeader;
            }
            set
            {
                RaiseAndSetIfChanged(ref _spamFilterHeader, value);
            }
        }

        private String _allUsersSubbedHeader;
        public String allUsersSubbedHeader
        {
            get
            {
                return _allUsersSubbedHeader;
            }
            set
            {
                RaiseAndSetIfChanged(ref _allUsersSubbedHeader, value);
            }
        }

        private String _fixOfficalBugsHeader;
        public String fixOfficalBugsHeader
        {
            get
            {
                return _fixOfficalBugsHeader;
            }
            set
            {
                RaiseAndSetIfChanged(ref _fixOfficalBugsHeader, value);
            }
        }
        // Commands
        public MiniCommand shutdownServerCommand { get; }
        public MiniCommand createAccountCommand { get; }
        public MiniCommand editServerPropertiesCommand { get; }
        public MiniCommand openServerFolderCommand { get; }
        public MiniCommand resetPasswordCommand { get; }

        public MiniCommand toggleSwearFilter { get; }
        public MiniCommand toggleCorrections { get; }
        public MiniCommand toggleNonVioChecks { get; }
        public MiniCommand toggleSpamFilter { get; }

        public MiniCommand toggleAllUsersSubbed { get; }
        public MiniCommand toggleFixOfficalBugs { get; }


    }
}