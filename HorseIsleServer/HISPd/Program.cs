using HISP.Cli.Properties;
using HISP.Server;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace HISP.Cli
{
    public static class Program
    {
        private static StreamWriter sw = null;
        private static FileStream fs = null;
        private static string logFile;
        private static EventWaitHandle shutdownHandle = null;

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
            try
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                    sw = null;
                }

            }
            catch (Exception) { };

            try
            {
                if (fs != null)
                {
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }
            catch (Exception) { };


            if(shutdownHandle != null)
                shutdownHandle.Set();
        }

        private static string formatMessage(string type, string text, bool console)
        {

            string newline = Environment.NewLine;

            string msg = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + ": [" + type + "] ";
            if (console && text.Length > (Console.WindowWidth - msg.Length) - newline.Length)
                text = text.Substring(0, (Console.WindowWidth - msg.Length) - newline.Length);

            return msg + text + newline;
        }

        public static void LogToFile(bool error, string type,string text)
        {
            sw.WriteLine(formatMessage(type, text, false));
            if (error)
                sw.Flush();
        }
        public static void LogStdout(bool error, string type, string text)
        {
            if (type == "CRASH")
                LogToFile(error, type, text);
            try
            {
                if (error)
                    Console.Error.WriteAsync(formatMessage(type, text, true));
                else
                    Console.Out.WriteAsync(formatMessage(type, text, true));
                
            }
            catch (Exception) { };
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += ProcessQuitHandler;
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, (_) => { GameServer.ShutdownServer("Server process received SIGTERM."); });
            PosixSignalRegistration.Create(PosixSignal.SIGQUIT, (_) => { GameServer.ShutdownServer("Server process received SIGQUIT."); });

            string baseDir = Directory.GetCurrentDirectory();
            Logger.SetCallback(LogStdout);
            Entry.SetShutdownCallback(OnShutdown);

            string hispConfVar = Environment.GetEnvironmentVariable("HISP_CONF_FILE");
            string hispLogVar = Environment.GetEnvironmentVariable("HISP_LOG_FILE");
            string hispBaseDir = Environment.GetEnvironmentVariable("HISP_BASE_DIR");

            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "--install-service":
#if OS_LINUX
                        File.WriteAllBytes("/etc/systemd/system/HISP.service", Resources.HISPService);
                        LogStdout(false, "INFO", "Created Service! enable it with \"sudo systemctl enable HISP\"");
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
                                        baseDir = argu[1];
                                        Directory.SetCurrentDirectory(baseDir);
                                        break;
                                    default:
                                        continue;
                                }
                            }
                        }
                        break;
                }
            }

            if (hispConfVar != null && hispConfVar != "")
            {
                ConfigReader.ConfigurationFileName = hispConfVar;
            }
            
            if (hispLogVar != null && hispLogVar != "")
            {
                LogFile = hispLogVar;
                Logger.SetCallback(LogToFile);
            }
            else if(hispLogVar == "stdout")
            {
                LogFile = Path.Combine(baseDir, "crash.log");
                Logger.SetCallback(LogStdout);
            }
            else
            {
                LogFile = Path.Combine(baseDir, "crash.log");
            }

            if (hispBaseDir != null && hispBaseDir != "")
            {
                baseDir = hispBaseDir;
                Directory.SetCurrentDirectory(baseDir);
            }

            Entry.Start();

            shutdownHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            shutdownHandle.WaitOne();
        }

        private static void ProcessQuitHandler(object sender, EventArgs e)
        {
            GameServer.ShutdownServer("HISPd process quitting.");
        }
    }
}
