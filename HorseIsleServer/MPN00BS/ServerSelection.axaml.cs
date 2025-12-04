using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using System;
using System.IO;

namespace MPN00BS
{
    public partial class ServerSelection : Window
    {
        const string lastserver = "lastserver.ini";
        private void readLastServer(string config)
        {
            if (File.Exists(config))
            {
                string[] lines = File.ReadAllLines(config);
                foreach (string line in lines)
                {
                    if (line.Contains("="))
                    {
                        string[] kvp = line.Split("=");
                        if (kvp.Length >= 2)
                        {
                            if (kvp[0] == "server_ip")
                                serverIp.Text = kvp[1];
                            if (kvp[0] == "server_port")
                                serverPort.Text = kvp[1];
                        }
                    }
                }
            }
        }

        private void writeLastServer(string config)
        {
            File.WriteAllLines(config, new string[]
            {
                String.Join("=", new string[] { "server_ip", serverIp.Text}),
                String.Join("=", new string[] { "server_port", serverPort.Text}),
            });
        }
        public ServerSelection()
        {
            InitializeComponent(true);
            readLastServer(lastserver);
        }

        private void joinServer_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            writeLastServer(lastserver);
            ServerStarter.StartHttpServer();
            ServerStarter.StartHorseIsleClient(OnClientExit, serverIp.Text, Convert.ToInt32(serverPort.Value));
        }

        private void OnClientExit()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
            	this.Close();
                Environment.Exit(0);
            });
        }
    }
}
