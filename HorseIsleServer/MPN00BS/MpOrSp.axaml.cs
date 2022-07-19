using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MPN00BS
{
    public partial class MpOrSp : Window
    {
        public MpOrSp()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private void playSingleplayer_Click(object sender, RoutedEventArgs e)
        {
            new LoadingWindow().Show();
            this.Close();
        }

        private void playMultiplayer_Click(object sender, RoutedEventArgs e)
        {
            new ServerSelection().Show();
            this.Close();

        }
    }
}
