using System;
using HISP.Game;
using HISP.Game.Items;
using HISP.Game.Horse;
using HISP.Game.SwfModules;
using HISP.Security;
using HISP.Server;
using HISP.Game.Services;
namespace HISP
{
    public class Program
    {
        static void Main(string[] args)
        {
        #if (!DEBUG)
            try
            {
        #endif
                Console.Title = "HISP - Horse Isle Server Emulator";
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
        #if (!DEBUG)
            }
            catch(Exception e)
            {
                Logger.ErrorPrint("Server has crashed! :(");
                Logger.ErrorPrint("");
                Logger.ErrorPrint("");
                Logger.ErrorPrint("UNCAUGHT EXCEPTION!");
                Logger.ErrorPrint("");
                Logger.ErrorPrint("");
                Logger.ErrorPrint(e.Message);
                Logger.ErrorPrint("");
                Logger.ErrorPrint(e.StackTrace);
                while(true){};
            }
        #endif

        }
    }
}
