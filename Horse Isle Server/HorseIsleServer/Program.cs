using System;
using HISP.Game;
using HISP.Game.Items;
using HISP.Game.Horse;
using HISP.Game.SwfModules;
using HISP.Security;
using HISP.Server;

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
            DroppedItems.Init();
            WildHorse.Init();
            Brickpoet.LoadPoetryRooms();
            Item.DoSpecialCases();
            GameServer.StartServer();

        }
    }
}
