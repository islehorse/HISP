using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MPN00BS
{
    public partial class MpOrSp : Window
    {
        public MpOrSp()
        {
            InitializeComponent(true);
        }
        private void playSingleplayerClick(object sender, RoutedEventArgs e)
        {
            new LoadingWindow().Show();
            this.Close();
        }

        private void playMultiplayerClick(object sender, RoutedEventArgs e)
        {
            new ServerSelection().Show();
            this.Close();

        }
    }
}
