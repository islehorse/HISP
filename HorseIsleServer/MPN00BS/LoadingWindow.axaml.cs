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

        public void ProgressUpdate()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                startupProgress.Value++;
                if (startupProgress.Value >= startupProgress.Maximum)
                {
                    this.Hide();
                    ServerStarter.StartHorseIsleClient(OnClientExit, "127.0.0.1", 12321);
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
            new Task( () => ServerStarter.StartHispServer(ProgressUpdate) ).Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            startupProgress = this.FindControl<ProgressBar>("startupProgress");
        }
    }
}
