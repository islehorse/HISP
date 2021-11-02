using System;
using HISP.Game;
using HISP.Game.Items;
using HISP.Game.Horse;
using HISP.Game.SwfModules;
using HISP.Security;
using HISP.Server;
using HISP.Game.Services;
using System.IO;

namespace HISP
{
    public class Program
    {
        static void Main(string[] args)
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

            GameServer.StartServer();

            while (true) { }; 
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception execpt = (Exception)e.ExceptionObject;
            
            Logger.ErrorPrint("HISP HAS CRASHED :(");
            Logger.ErrorPrint("Unhandled Exception: " + execpt.ToString());
            Logger.ErrorPrint(execpt.Message);
            Logger.ErrorPrint("");
            Logger.ErrorPrint(execpt.StackTrace);

            while (true) { };
        }
    }
}
