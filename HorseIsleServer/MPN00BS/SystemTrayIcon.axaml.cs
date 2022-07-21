using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HISP.Server;

namespace MPN00BS
{
    public partial class SystemTrayIcon : Window
    {
        public bool swearFilterEnabled
        {
            get
            {
                return ConfigReader.BadWords;
            }
        }
        public bool correctionsEnabled
        {
            get
            {
                return ConfigReader.DoCorrections;
            }
        }
        public bool nonVioChecksEnabled
        {
            get
            {
                return ConfigReader.DoNonViolations;
            }
        }
        public bool spamFilterEnabled
        {
            get
            {
                return ConfigReader.EnableSpamFilter;
            }
        }
        public bool allUsersSubbed
        {
            get
            {
                return ConfigReader.AllUsersSubbed;
            }
        }
        public bool fixOfficalBugs
        {
            get
            {
                return ConfigReader.FixOfficalBugs;
            }
        }
        private void OnClientExit()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Close();
            });
        }

        public SystemTrayIcon()
        {
            InitializeComponent();
            this.Hide();
            ServerStarter.StartHorseIsleClient(OnClientExit, "127.0.0.1", 12321);

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
