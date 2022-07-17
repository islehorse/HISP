// An HTTP Server, and Horse Isle Server, Flash Player, in a single package
// Idea is to just be open and play.

using HISP.Game;
using HISP.Game.Chat;
using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Game.Services;
using HISP.Game.SwfModules;
using HISP.Security;
using HISP.Server;
using HTTP;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HISP.Noobs
{
    public static class Program
    {
        public static Random rand = new Random(Guid.NewGuid().GetHashCode());
        private static LoadingForm lfrm;
        public static string BaseDir;
        private static ContentServer cs;
        private static void addToList(string path)
        {
            string Name = path.Remove(0, Path.Combine(Directory.GetCurrentDirectory(), "client").Length);
            Name = Name.Replace("\\", "/");

            ContentItem ci = new ContentItem(Name, path);
            cs.Contents.Add(ci);

        }

        public static void OnShutdown()
        {
            if (!Process.GetCurrentProcess().CloseMainWindow())
                Process.GetCurrentProcess().Close();
        }

        public static void StartLRFrm()
        {
            if (lfrm.ShowDialog() == DialogResult.Cancel)
            {
                GameServer.ShutdownServer();
            }
        }

        public static void IncrementProgress()
        {
            if (lfrm.InvokeRequired)
            {
                lfrm.Invoke(() =>
                {
                    lfrm.StartProgress.Increment(1);
                });
            }
            else
            {
                lfrm.StartProgress.Increment(1);
            }
        }
        public static string GetOctlet()
        {
            return rand.Next(0, 255).ToString();
        }


        public static void Main(string[] args)
        {
            BaseDir = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "HISP", "N00BS");
            Directory.CreateDirectory(BaseDir);


            // Start Web Server
            try
            {
                cs = new ContentServer("127.0.0.1");
                string[] fileList = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "client"), "*", SearchOption.AllDirectories);
                foreach (string file in fileList)
                    addToList(file);
            }
            catch (Exception e)
            {
                MessageBox.Show("Web server failed to start: " + e.Message, "Error starting web server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MpOrSp mporsp = new MpOrSp();
            if (mporsp.ShowDialog() != DialogResult.OK)
                return;

            if (mporsp.Mutliplayer)
            {
                ServerSelection ssel = new ServerSelection();
                ssel.ShowDialog();
            }
            else
            {
                lfrm = new LoadingForm();
                Task startForm = new Task(StartLRFrm);
                startForm.Start();

                ConfigReader.ConfigurationFileName = Path.Combine(BaseDir, "server.properties");
                ConfigReader.OpenConfig();
                ConfigReader.SqlLite = true;
                ConfigReader.LogLevel = 0;
                ConfigReader.CrossDomainPolicyFile = Path.Combine(BaseDir, "CrossDomainPolicy.xml");

                // Compatibility patch
                if (File.Exists(Path.Combine(BaseDir, "game1.db.db"))) { 
                    File.Move(Path.Combine(BaseDir, "game1.db.db"), Path.Combine(BaseDir, "game1.db"));
                }

                ConfigReader.DatabaseName = Path.Combine(BaseDir, "game1");


                IncrementProgress();
                Database.OpenDatabase();
                IncrementProgress();


                if (Database.GetUsers().Length <= 0)
                {
                    RegisterForm rfrm = new RegisterForm();
                    if (rfrm.ShowDialog() == DialogResult.Cancel)
                        GameServer.ShutdownServer();

                }

                // Start HI1 Server
                IncrementProgress();

                Entry.SetShutdownCallback(OnShutdown);
                IncrementProgress();

                CrossDomainPolicy.GetPolicy();
                IncrementProgress();

                GameDataJson.ReadGamedata();
                IncrementProgress();

                Map.OpenMap();
                IncrementProgress();

                World.ReadWorldData();
                IncrementProgress();

                Treasure.Init();
                IncrementProgress();

                DroppedItems.Init();
                IncrementProgress();

                WildHorse.Init();
                IncrementProgress();

                Drawingroom.LoadAllDrawingRooms();
                IncrementProgress();

                Brickpoet.LoadPoetryRooms();
                IncrementProgress();

                Multiroom.CreateMultirooms();
                IncrementProgress();

                Auction.LoadAllAuctionRooms();
                IncrementProgress();

                Command.RegisterCommands();
                IncrementProgress();

                Item.DoSpecialCases();
                IncrementProgress();
                try
                {
                    GameServer.StartServer();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Horse Isle server failed to start: " + e.Message, "Error starting hi1 server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                IncrementProgress();

                lfrm.DialogResult = DialogResult.OK;

                SystemTrayIcon stry = new SystemTrayIcon();
                stry.ShowDialog();

                // Finally, shutdown server
                GameServer.ShutdownServer();
            }


        }
    }
}
