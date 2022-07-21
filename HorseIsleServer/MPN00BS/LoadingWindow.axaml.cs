using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace MPN00BS
{
    public partial class LoadingWindow : Window
    {


        public void OnServerStarted()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Hide();
                new SystemTrayIcon().Show();
                this.Close();
            });
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
