using HISP.Cli.Properties;
using HISP.Server;
using System;
using System.IO;
using System.Threading;

namespace HISP.Cli
{
    public static class Program
    {
        private static StreamWriter sw = null;
        private static FileStream fs = null;
        private static string logFile;
        private static EventWaitHandle shutdownHandle;

        public static bool ShuttingDown = false;
        public static string BaseDir;
        public static string LogFile
        {
            get
            {
                return logFile;
            }
            set
            {
                logFile = value;        
                if(sw != null)
                {
                    sw.Flush();
                    sw.Dispose();
                    sw = null;
                }
                if(fs != null)
                {
                    fs.Flush();
                    fs.Dispose();
                    fs = null;
                }

                fs = File.OpenWrite(logFile);
                sw = new StreamWriter(fs);
            }
        }

        public static void OnShutdown()
        {
            shutdownHandle.Set();
        }
        public static void LogToFile(bool error, string type,string text)
        {
            sw.WriteLineAsync(text + sw.NewLine);
        }
        public static void LogStdout(bool error, string type, string text)
        {
            if (error)
                Console.Error.WriteAsync("[" + type + "] " + text + Console.Error.NewLine);
            else
                Console.Out.WriteAsync("[" + type + "] " + text + Console.Out.NewLine);
            
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += ProcessQuitHandler;

            string BaseDir = Directory.GetCurrentDirectory();
            Logger.SetCallback(LogStdout);

            string HispConfVar = Environment.GetEnvironmentVariable("HISP_CONF_FILE");
            string HispLogVar = Environment.GetEnvironmentVariable("HISP_LOG_FILE");
            string HispBaseDir = Environment.GetEnvironmentVariable("HISP_BASE_DIR");

            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "--install-service":
                        #if OS_LINUX
                        File.WriteAllBytes("/etc/systemd/system/HISP.service", Resources.HISPService);
                        LogStdout(false, "INFO", "Crreated Service! enable it with \"sudo systemctl enable HISP\"");
                        #else
                        LogStdout(true, "ERROR", "Installing as a service unsupported on this platform");
                        #endif
                        break;
                    default:
                        if (arg.Contains("="))
                        {
                            string[] argu = arg.Split("=");
                            if (argu.Length >= 2)
                            {
                                switch (argu[0])
                                {
                                    case "--config-file":
                                        ConfigReader.ConfigurationFileName = argu[1];
                                        break;
                                    case "--log-to-file":
                                        LogFile = argu[1];
                                        Logger.SetCallback(LogToFile);
                                        break;
                                    case "--base-directory":
                                        BaseDir = argu[1];
                                        Directory.SetCurrentDirectory(BaseDir);
                                        break;
                                    default:
                                        continue;
                                }
                            }
                        }
                        break;
                }
            }

            if (HispConfVar != null)
            {
                ConfigReader.ConfigurationFileName = HispConfVar;
            }
            
            if (HispLogVar != null)
            {
                LogFile = HispLogVar;
                Logger.SetCallback(LogToFile);
            }

            if (HispBaseDir != null)
            {
                BaseDir = HispBaseDir;
                Directory.SetCurrentDirectory(BaseDir);
            }

            Entry.SetShutdownCallback(OnShutdown);
            Entry.Start();

            shutdownHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            shutdownHandle.WaitOne();
        }

        private static void ProcessQuitHandler(object sender, EventArgs e)
        {
            GameServer.ShutdownServer();
        }
    }
}
