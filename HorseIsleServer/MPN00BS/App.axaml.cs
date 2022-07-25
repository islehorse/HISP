using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MPN00BS.ViewModels;
using System.IO;
using System.Runtime.InteropServices;

namespace MPN00BS
{
    public partial class App : Application
    {
#if OS_LINUX
        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);

#endif
        public App()
        {
            this.DataContext = new HispViewModel();
        }
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if OS_LINUX
            chmod(Path.Combine(Directory.GetCurrentDirectory(), "flashplayer", "LINUX", "flash.elf"), 777);
#endif
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
