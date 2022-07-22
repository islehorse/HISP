using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HISP.Server;
using MPN00BS.ViewModels;

namespace MPN00BS
{
    public partial class App : Application
    {
        public App()
        {
            this.DataContext = new HispViewModel();
        }
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MpOrSp();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
