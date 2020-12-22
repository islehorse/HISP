using HISP.Player;
using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Chat
{
    class Command
    {

        public static bool Mute(string message, string[] args, User user)
        {
            string muteType = args[0];

            if (muteType == "GLOBAL")
            {
                user.MuteGlobal = true;
            } else if (muteType == "ISLAND")
            {
                user.MuteIsland = true;
            } else if (muteType == "NEAR")
            {
                user.MuteNear = true;
            } else if (muteType == "HERE")
            {
                user.MuteHere = true;
            } else if (muteType == "BUDDY")
            {
                user.MuteBuddy = true;
            } else if (muteType == "SOCIALS")
            {
                user.MuteSocials = true;
            } else if (muteType == "ALL")
            {
                user.MuteAll = true;
            } else
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatCommandComplete(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);

            user.LoggedinClient.SendPacket(chatPacket);


            return true;
        }
    }
}
