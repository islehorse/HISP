using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Chat
    {
        public static bool isCommand(User user, string message)
        {
            if (message.Length < 1)
                return false;

            if (user.Administrator || user.Moderator)
                if (message[0] == '%')
                    return true;
            if (message[0] == '!')
                return true;
 
        }
        public static bool isAppropriate(string message)
        {
            if (!ConfigReader.BadWords)
                return true;
            else
                return true; // Fuck Censorship
        }

        public static void HandleMessage(User user, byte channel, string message)
        {

        }

    }
}
