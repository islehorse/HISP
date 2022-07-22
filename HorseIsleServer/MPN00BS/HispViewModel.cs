using HISP.Server;
using MiniMvvm;
using System;
using System.Diagnostics;
using System.IO;

namespace MPN00BS.ViewModels
{
    public class HispViewModel : ViewModelBase
    {
        public HispViewModel()
        {
            ServerStarter.ReadServerProperties();
            swearFilterHeader = (ConfigReader.EnableSwearFilter ? "Disable" : "Enable") + " Swear Filter";

            createAccountCommand = MiniCommand.Create(() =>
            {
                if (!ServerStarter.HasServerStarted)
                {
                    MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
                new RegisterWindow().Show();
            });

            resetPasswordCommand = MiniCommand.Create(() =>
            {
                if (!ServerStarter.HasServerStarted)
                {
                    MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                new ResetWindow().Show();
            });

            editServerPropertiesCommand = MiniCommand.Create(() =>
            {
                if (!ServerStarter.HasServerStarted)
                {
                    MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(ServerStarter.BaseDir, "server.properties");
                p.StartInfo.UseShellExecute = true;
                p.Start();

            });

            openServerFolderCommand = MiniCommand.Create(() =>
            {
                if (!ServerStarter.HasServerStarted)
                {
                    MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                Process p = new Process();
                p.StartInfo.FileName = ServerStarter.BaseDir;
                p.StartInfo.UseShellExecute = true;
                p.Start();
            });

            shutdownServerCommand = MiniCommand.Create(() =>
            {
                if (!ServerStarter.HasServerStarted)
                {
                    MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                GameServer.ShutdownServer();
            });


            toggleSwearFilter = MiniCommand.Create(() =>
            {
                if (!ServerStarter.HasServerStarted)
                {
                    MessageBox.Show(null, "There is no Horse Isle Server running yet.", "Server not Started.", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                bool enab = !ConfigReader.EnableSwearFilter;
                ServerStarter.ModifyConfig("enable_word_filter", enab.ToString().ToLowerInvariant());
                ConfigReader.EnableSwearFilter = enab;
                swearFilterHeader = (ConfigReader.EnableSwearFilter ? "Disable" : "Enable") + " Swear Filter";
            });
            
        }

 


        public String swearFilterHeader { get; set; }
        public MiniCommand shutdownServerCommand { get; }
        public MiniCommand createAccountCommand { get; }
        public MiniCommand editServerPropertiesCommand { get; }
        public MiniCommand openServerFolderCommand { get; }
        public MiniCommand toggleSwearFilter { get; }
        public MiniCommand resetPasswordCommand { get; }

    }
}