using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Messages
    {

        public static string LoginMessage(string username)
        {
            return Gamedata.LoginMessage.Replace("%USERNAME%", username);
        }

        public static string LocationData(int x, int y)
        {
            string message = "";
            try
            {
                World.Isle isle = World.GetIsle(x, y);
                message = Gamedata.AreaMessage.Replace("%AREA%", isle.Name);
            }
            catch (Exception) { }

            int[] itemIds = World.GetDroppedItems(x, y);
            if (itemIds.Length == 0)
                message += Gamedata.NothingMessage;

            return message;
        }
    }
}
