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

        public static void Start()
        {
            #if (!DEBUG)
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            #endif

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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception execpt = (Exception)e.ExceptionObject;

            Logger.ErrorPrint("HISP HAS CRASHED :(");
            Logger.ErrorPrint("Unhandled Exception: " + execpt.ToString());
            Logger.ErrorPrint(execpt.Message);
            Logger.ErrorPrint("");
            Logger.ErrorPrint(execpt.StackTrace);

            while (true) {  /* Allow asyncronous operations to happen. */ };
        }
    }
}
