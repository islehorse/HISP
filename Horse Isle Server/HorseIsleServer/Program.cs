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
            Brickpoet.LoadPoetryRooms();
            Auction.LoadAllAuctionRooms();
            Drawingroom.LoadAllDrawingRooms();
            Item.DoSpecialCases();

            GameServer.StartServer();

        }
    }
}
