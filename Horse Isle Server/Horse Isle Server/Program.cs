using System;
using System.IO;
using System.Reflection;
using HISP.Game;
using HISP.Security;
using HISP.Server;
namespace HISP
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "HISP - Horse Isle Server Emulator";
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            ConfigReader.OpenConfig();
            CrossDomainPolicy.GetPolicy();
            Database.OpenDatabase();
            GameDataJson.ReadGamedata();
            Map.OpenMap();
            World.ReadWorldData();
            DroppedItems.Init();
            GameServer.StartServer();

        }
    }
}
