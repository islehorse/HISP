
using HISP.Server;
using HTTP;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static MPN00BS.MessageBox;

namespace MPN00BS
{

    public class ServerStarter
    {
        private static Process fp = null;
        private static ContentServer cs = null;
        private static Action HorseIsleClientExitCallback;

        private static string startupFolder = AppContext.BaseDirectory;
        private static string clientFolder = Path.Combine(startupFolder, "client");

#if OS_LINUX || OS_MACOS
        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);

#endif
        private static void addToList(string path)
        {
            string Name = path.Remove(0, clientFolder.Length);
            Name = Name.Replace("\\", "/");

            ContentItem ci = new ContentItem(Name, path);
            cs.Contents.Add(ci);

        }
        public static void ShutdownHTTPServer()
        {
            cs.Shutdown();
        }

        public static void ShowCrash(bool error, string type, string text)
        {
            if (type == "CRASH")
            {
                File.AppendAllText(Path.Combine(ConfigReader.ConfigDirectory, "crash.log"), text);
                MessageBox.Show(null, text, type, MessageBoxButtons.Ok);
            }
        }

        private static void HorseIsleClientExited(object sender, EventArgs e)
        {
            HorseIsleClientExitCallback();
        }

        public static void CloseHorseIsleClient()
        {
            if(fp != null)
                if (!fp.HasExited)
                    fp.Kill();
        }
        public static void StartHorseIsleClient(Action callback, string serverIp, int serverPort)
        {
            HorseIsleClientExitCallback = callback;

            fp = new Process();

            // find executable
#if OS_WINDOWS || DEBUG
            string executable = Path.Combine(startupFolder, "flashplayer", "WINDOWS", "flash.exe");
#elif OS_LINUX
            string executable = Path.Combine(startupFolder, "flashplayer", "LINUX", "flash.elf");
#elif OS_MACOS
            string executable = Path.Combine(startupFolder, "flashplayer", "MACOS", "flash.app", "Contents", "MacOS", "Flash Player");
#else
            MessageBox.Show(null,"ERROR: No path for flash projector specified on this platform", "Porting error", MessageBoxButtons.Ok);
            string executable = Path.Combine(startupFolder, "flashplayer", "WINDOWS", "flash.exe");
#endif

            // make file executable
#if OS_LINUX || OS_MACOS
            chmod(executable, 755);
#endif

            if (!File.Exists(executable))
            {
                MessageBox.Show(null, "ERROR: Cannot find file: \"" + executable + "\"", "File not Found error", MessageBoxButtons.Ok);
                return;
            }

            fp.StartInfo.FileName = executable;
            fp.StartInfo.Arguments = "http://" + cs.IpAddr + ":" + cs.Port + "/horseisle.swf" + "?SERVER=" + serverIp + "&PORT=" + serverPort.ToString();
            fp.StartInfo.RedirectStandardOutput = true;
            fp.StartInfo.RedirectStandardError = true;
            fp.StartInfo.WorkingDirectory = Path.GetDirectoryName(executable);
            fp.EnableRaisingEvents = true;
            fp.Exited += HorseIsleClientExited;
            fp.Start();
        }

        public static void UpdateServerProperties()
        {
            SetConfigDir();
            ServerStarter.ModifyConfig("sql_backend", "sqllite");
            ServerStarter.ModifyConfig("log_level", "0");

        }
        public static void StartHispServer(Action ProgressCallback, Action UserCreationCallback, Action ServerStartedCallback, Action OnShutdown)
        {
            AppDomain.CurrentDomain.ProcessExit += HorseIsleClientExited;
            Logger.SetCallback(ShowCrash);
            Entry.SetShutdownCallback(OnShutdown);
            UpdateServerProperties();


            // Compatibility patch
            if (File.Exists(Path.Combine(ConfigReader.ConfigDirectory, "game1.db.db"))) File.Move(Path.Combine(ConfigReader.ConfigDirectory, "game1.db.db"), Path.Combine(ConfigReader.ConfigDirectory, "game1.db"));

            try
            {
                // start the server ...
                foreach (Action startupStep in Entry.StartupSteps)
                {
                    startupStep();
                    ProgressCallback();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(null, "Horse Isle server failed to start: " + Environment.NewLine + e.Message, "Error starting hi1 server", MessageBoxButtons.Ok);
                return;
            }

            ServerStartedCallback();

            if (Database.GetUsers().Length <= 0)
                UserCreationCallback();
        }

        public static void ModifyConfig(string okey, string value)
        {
            string[] configFile = File.ReadAllLines(ConfigReader.ConfigurationFileName);
            for (int i = 0; i < configFile.Length; i++)
            {
                string setting = configFile[i];

                if (setting.Length < 1)
                    continue;
                if (setting[0] == '#')
                    continue;
                if (!setting.Contains("="))
                    continue;

                string[] dataPair = setting.Split('=');

                string key = dataPair[0];

                if (key == okey)
                {
                    dataPair[1] = value;
                    configFile[i] = string.Join('=', dataPair);
                }
            }
            File.WriteAllLines(ConfigReader.ConfigurationFileName, configFile);
        }

        public static void SetConfigDir()
        {
#if OS_WINDOWS || DEBUG
            string hispFolder = Environment.GetEnvironmentVariable("APPDATA");
            if (hispFolder == null)
                return;

            ConfigReader.ConfigDirectory = Path.Combine(hispFolder, "HISP", "N00BS");
            Directory.CreateDirectory(ConfigReader.ConfigDirectory);
#elif OS_LINUX || OS_MACOS
            string hispFolder = Environment.GetEnvironmentVariable("HOME");
            if (hispFolder == null)
                return;

            ConfigReader.ConfigDirectory = Path.Combine(hispFolder, ".HISP", "N00BS");
            Directory.CreateDirectory(ConfigReader.ConfigDirectory);
#endif
        }
        public static void StartHttpServer()
        {
            SetConfigDir();
            try
            {

                cs = new ContentServer("127.0.0.1", 12322);

                string[] fileList = Directory.GetFiles(clientFolder, "*", SearchOption.AllDirectories);
                foreach (string file in fileList) addToList(file);
            }
            catch (Exception e)
            {
                MessageBox.Show(null, "Web server failed to start: " + e.GetType().Name + " " + e.Message, "Error starting web server", MessageBoxButtons.Ok);
                return;
            }

        }
    }
}