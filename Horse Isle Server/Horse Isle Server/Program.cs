using System;
using System.Drawing;
using System.Drawing.Imaging;
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

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            ConfigReader.OpenConfig();
            CrossDomainPolicy.GetPolicy();
            Database.OpenDatabase();
            Map.OpenMap();
            Gamedata.ReadGamedata();
            World.ReadWorldData();
            DroppedItems.Init();
            GameServer.StartServer();

        }
    }
}
