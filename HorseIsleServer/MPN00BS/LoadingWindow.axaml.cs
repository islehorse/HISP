using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HISP.Server;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MPN00BS
{
    public partial class LoadingWindow : Window
    {
        private void OnClientExit()
        {
            Logger.InfoPrint("Client closed, shutting down server.");
            try
            {
                GameServer.ShutdownServer();
            }catch(Exception) { Environment.Exit(-1); }
        }
        public void OnServerStarted()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Hide();
                ServerStarter.StartHorseIsleClient(OnClientExit, "127.0.0.1", 12321);
            });
        }
        public void OnNoUsersFound()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                new RegisterWindow().Show();
            });
        }

        private void OnShutdown()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ServerStarter.CloseHorseIsleClient();
                this.Close();
                Environment.Exit(0);
            });
        }

        public void ProgressUpdate()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                startupProgress.Value++;
            });
        }
        public LoadingWindow()
        {
            InitializeComponent();
            ServerStarter.StartHttpServer();
            new Task( () => ServerStarter.StartHispServer(ProgressUpdate, OnNoUsersFound, OnServerStarted, OnShutdown)).Start();
        }

        private void OnServerClose(object sender, CancelEventArgs e)
        {
            GameServer.ShutdownServer();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            startupProgress = this.FindControl<ProgressBar>("startupProgress");
        }
    }
}
