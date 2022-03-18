using HISP.Server;
using System;
using System.IO;

namespace HISP
{
    public static class Program
    {
        public static bool ShuttingDown = false;
        public static string LogFile;
        public static string BaseDir;

        public static void OnShutdown()
        {
            ShuttingDown = true;
        }
        public static void LogToFile(string text)
        {
            File.AppendAllTextAsync(text, LogFile);
        }
        public static void LogStdout(string text)
        {
            Console.Out.WriteAsync(text + Console.Out.NewLine);
        }
        public static void Main(string[] args)
        {
            string BaseDir = Directory.GetCurrentDirectory();
#if DEB_PACKAGE
            ConfigReader.ConfigurationFileName = "/etc/hisp.conf"
            LogFile = "/var/log/hisp.log"
            Logger.SetCallback(LogToFile);
#else
            Logger.SetCallback(LogStdout);
#endif

            Entry.SetShutdownCallback(OnShutdown);
            Entry.Start();

            while (!ShuttingDown) {  /* Allow asyncronous operations to happen. */ };
            
        }
    }
}
