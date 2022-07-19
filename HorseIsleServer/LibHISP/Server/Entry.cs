using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Game.Services;
using HISP.Game.SwfModules;
using HISP.Game.Chat;
using HISP.Security;

using System;
using System.Diagnostics;

namespace HISP.Server
{
    public static class Entry
    {
        // "Entry Point"

        private static void defaultOnShutdownCallback()
        {
            Process.GetCurrentProcess().Close();
        }

        public static Action OnShutdown = defaultOnShutdownCallback;

        public static void SetShutdownCallback(Action callback)
        {
            OnShutdown = callback;
        }
        
        public static void RegisterCrashHandler()
        {
#if (!DEBUG)
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HispCrashHandler);
#endif
        }

        public static void Start()
        {
            RegisterCrashHandler();
            Console.Title = ServerVersion.GetBuildString();
            ConfigReader.OpenConfig();
            CrossDomainPolicy.GetPolicy();
            Database.OpenDatabase();
            GameDataJson.ReadGamedata();

            Map.OpenMap();
            World.ReadWorldData();
            Treasure.Init();

            DroppedItems.Init();
            WildHorse.Init();

            Drawingroom.LoadAllDrawingRooms();
            Brickpoet.LoadPoetryRooms();
            Multiroom.CreateMultirooms();

            Auction.LoadAllAuctionRooms();

            Item.DoSpecialCases();
            Command.RegisterCommands();

            GameServer.StartServer();
        }

        private static void HispCrashHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception execpt = (Exception)e.ExceptionObject;


            string crashMsg = "HISP HAS CRASHED :(";
            crashMsg += "Build: " + ServerVersion.GetBuildString();
            crashMsg += "Unhandled Exception: " + execpt.Message;
            crashMsg += execpt.StackTrace;

            Logger.CrashPrint(crashMsg);
            
        }
    }
}
