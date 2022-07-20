using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace MPN00BS
{
    public partial class LoadingWindow : Window
    {

        private void OnClientExit()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Close();
            });
        }
        public void OnServerStarted()
        {
            ServerStarter.StartHorseIsleClient(OnClientExit, "127.0.0.1", 12321);
        }
        public void OnNoUsersFound()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                new RegisterWindow().Show();
            });
        }

        public void ProgressUpdate()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                startupProgress.Value++;
                if (startupProgress.Value >= startupProgress.Maximum)
                {
                    this.Hide();
                }
            });
        }
        public LoadingWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            ServerStarter.StartHttpServer();
            new Task( () => ServerStarter.StartHispServer(ProgressUpdate, OnNoUsersFound, OnServerStarted)).Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            startupProgress = this.FindControl<ProgressBar>("startupProgress");
        }
    }
}
