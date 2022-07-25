using HISP.Game.Chat;
using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Game.Services;
using HISP.Game.SwfModules;
using HISP.Game;
using HISP.Security;
using HISP.Server;
using HTTP;
using System;
using System.Diagnostics;
using System.IO;
using static MPN00BS.MessageBox;

namespace MPN00BS
{
    
    public class ServerStarter
    {
        public static bool HasServerStarted = false;
        private static Process clientProcess = new Process();
        private static Action HorseIsleClientExitCallback;
        public static string BaseDir = "";
        private static ContentServer cs = null;
        private static void addToList(string path)
        {
            string Name = path.Remove(0, Path.Combine(Directory.GetCurrentDirectory(), "client").Length);
            Name = Name.Replace("\\", "/");

            ContentItem ci = new ContentItem(Name, path);
            cs.Contents.Add(ci);

        }


        public static void ShowCrash(bool error, string type, string text)
        {
            if (type == "CRASH")
            {
                File.AppendAllText(Path.Combine(BaseDir, "crash.log"), text);
                MessageBox.Show(null, text, type, MessageBoxButtons.Ok);
            }
        }

        private static void HorseIsleClientExited(object sender, EventArgs e)
        {
            HorseIsleClientExitCallback();
        }

        public static void CloseHorseIsleClient()
        {
            clientProcess.Kill();
        }
        public static void StartHorseIsleClient(Action callback, string serverIp, int serverPort)
        {
            HorseIsleClientExitCallback = callback;

            clientProcess = new Process();
            clientProcess.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "flashplayer", "WINDOWS", "flash.exe");
            clientProcess.StartInfo.Arguments = "http://127.0.0.1/horseisle.swf?SERVER=" + serverIp + "&PORT=" + serverPort.ToString();

            clientProcess.StartInfo.RedirectStandardOutput = true;
            clientProcess.StartInfo.RedirectStandardError = true;

            clientProcess.EnableRaisingEvents = true;
            clientProcess.Exited += HorseIsleClientExited;
            clientProcess.Start();
        }

        public static void ReadServerProperties()
        {
            SetBaseDir();
            ConfigReader.ConfigurationFileName = Path.Combine(BaseDir, "server.properties");
            ConfigReader.OpenConfig();
            ConfigReader.SqlLite = true;
            ConfigReader.LogLevel = 0;
            ConfigReader.CrossDomainPolicyFile = Path.Combine(BaseDir, ConfigReader.CrossDomainPolicyFile);


            // Compatibility patch
            if (File.Exists(Path.Combine(BaseDir, "game1.db.db")))
            {
                File.Move(Path.Combine(BaseDir, "game1.db.db"), Path.Combine(BaseDir, "game1.db"));
            }

            ConfigReader.DatabaseName = Path.Combine(BaseDir, ConfigReader.DatabaseName);
        }
        public static void StartHispServer(Action ProgressCallback, Action UserCreationCallback, Action ServerStartedCallback, Action OnShutdown)
        {

            Entry.RegisterCrashHandler();
            Logger.SetCallback(ShowCrash);
            ReadServerProperties();


            ProgressCallback();
            Database.OpenDatabase();
            ProgressCallback();


            if (Database.GetUsers().Length <= 0)
            {
                UserCreationCallback();
            }


            // Start HI1 Server
            ProgressCallback();

            Entry.SetShutdownCallback(OnShutdown);
            ProgressCallback();

            CrossDomainPolicy.GetPolicy();
            ProgressCallback();

            GameDataJson.ReadGamedata();
            ProgressCallback();

            Map.OpenMap();
            ProgressCallback();

            World.ReadWorldData();
            ProgressCallback();

            Treasure.Init();
            ProgressCallback();

            DroppedItems.Init();
            ProgressCallback();

            WildHorse.Init();
            ProgressCallback();

            Drawingroom.LoadAllDrawingRooms();
            ProgressCallback();

            Brickpoet.LoadPoetryRooms();
            ProgressCallback();

            Multiroom.CreateMultirooms();
            ProgressCallback();

            Auction.LoadAllAuctionRooms();
            ProgressCallback();

            Command.RegisterCommands();
            ProgressCallback();

            Item.DoSpecialCases();
            ProgressCallback();
            try
            {
                GameServer.StartServer();
            }
            catch (Exception e)
            {
                MessageBox.Show(null, "Horse Isle server failed to start: " + e.Message, "Error starting hi1 server", MessageBoxButtons.Ok);
                return;
            }
            ProgressCallback();
            HasServerStarted = true;
            ServerStartedCallback();
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

        public static void SetBaseDir()
        {

            string hispFolder = Environment.GetEnvironmentVariable("APPDATA");
            if (hispFolder == null)
                return;

            BaseDir = Path.Combine(hispFolder, "HISP", "N00BS");
            Directory.CreateDirectory(BaseDir);
        }
        public static void StartHttpServer()
        {
            SetBaseDir();
            try
            {
                cs = new ContentServer("127.0.0.1");
                string[] fileList = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "client"), "*", SearchOption.AllDirectories);
                foreach (string file in fileList)
                    addToList(file);
            }
            catch (Exception e)
            {
                MessageBox.Show(null, "Web server failed to start: " + e.Message, "Error starting web server", MessageBoxButtons.Ok);
                return;
            }

        }
    }
}
