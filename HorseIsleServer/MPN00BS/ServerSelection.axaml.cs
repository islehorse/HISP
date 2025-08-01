using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using System;

namespace MPN00BS
{
    public partial class ServerSelection : Window
    {
        public ServerSelection()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            serverIp = this.FindControl<TextBox>("serverIp");
            serverPort = this.FindControl<NumericUpDown>("serverPort");
        }
        private void joinServer_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ServerStarter.StartHttpServer();
            ServerStarter.StartHorseIsleClient(OnClientExit, serverIp.Text, Convert.ToInt32(serverPort.Value));
        }

        private void OnClientExit()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
            	ServerStarter.ShutdownHTTPServer();
            	this.Close();
                Environment.Exit(0);
            });
        }
    }
}
