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
        public static string AreaMessage;
        public static string NothingMessage;
        public static string LoginFormat;

        public static string MotdFormat;

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
            string message = "";
            try
            {
                World.Isle isle = World.GetIsle(x, y);
                message = AreaMessage.Replace("%AREA%", isle.Name);
            }
            catch (Exception) { }

            int[] itemIds = World.GetDroppedItems(x, y);
            if (itemIds.Length == 0)
                message += NothingMessage;

            return message;
        }
    }
}
