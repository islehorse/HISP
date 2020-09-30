using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Messages
    {
        public static string NewUserMessage;
        public static string LoginFormat;
        public static string MotdFormat;

        public static string ProfileSavedMessage;

        public static string Sepeerator;

        public static string IsleFormat;
        public static string TownFormat;
        public static string AreaFormat;
        public static string LocationFormat;

        public static string TileFormat;
        public static string NothingMessage;
        public static string GetMOTD()
        {
            return MotdFormat.Replace("%MOTD%", ConfigReader.Motd);
        }
        public static string GetLoginMessage(string username)
        {
            return LoginFormat.Replace("%USERNAME%", username);
        }

        public static string LocationData(int x, int y)
        {
            string locationString = "";
            string message = "";
            if(World.InArea(x,y))
                locationString += AreaFormat.Replace("%AREA%", World.GetArea(x, y).Name);
            if (World.InTown(x, y))
                locationString += TownFormat.Replace("%TOWN%", World.GetTown(x, y).Name);
            if (World.InIsle(x, y))
                locationString += IsleFormat.Replace("%ISLE%", World.GetIsle(x, y).Name);

            if(locationString != "")
                message += LocationFormat.Replace("%META%", locationString);

            int[] itemIds = World.GetDroppedItems(x, y);
            if (itemIds.Length == 0)
                message += NothingMessage;

            return message;
        }
    }
}
