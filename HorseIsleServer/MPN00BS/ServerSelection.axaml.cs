using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using HISP.Server;
using System;
using System.IO;

namespace MPN00BS
{
    public partial class ServerSelection : Window
    {
        private string lastServerPath
        {
            get{
                ServerStarter.SetConfigDir();
                return Path.Combine(ConfigReader.ConfigDirectory, "lastserver.ini");
            }
        }

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
            readLastServer(lastServerPath);
        }

        private void joinServer_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            writeLastServer(lastServerPath);
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
