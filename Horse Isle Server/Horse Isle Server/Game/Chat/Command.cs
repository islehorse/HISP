using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game.Chat
{
    class Command
    {

        public static bool Stickbug(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;
            if (!user.Administrator)
                return false;

            if(args[0] == "ALL")
            {
                foreach(GameClient client in GameServer.ConnectedClients)
                {
                    if(client.LoggedIn)
                    {
                        byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket("fun/stickbug.swf", PacketBuilder.PACKET_SWF_MODULE_GENTLE);
                        client.SendPacket(swfModulePacket);
                    }    
                }
            }
            else
            {
                try
                {
                    User victimUser = GameServer.GetUserByName(args[0]);
                    byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket("fun/stickbug.swf", PacketBuilder.PACKET_SWF_MODULE_GENTLE);
                    victimUser.LoggedinClient.SendPacket(swfModulePacket);
                }
                catch(KeyNotFoundException)
                {
                    return false;
                }
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            
            return true;
        }
        public static bool Mute(string message, string[] args, User user)
        {
            string mesasge = Messages.FormatPlayerCommandCompleteMessage(message.Substring(1));

            if (args.Length <= 0)
            {
                message += Messages.MuteHelp;
                goto leave;
            }
            
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
            }
            else if (muteType == "PM")
            {
                user.MutePrivateMessage = true;
            }
            else if (muteType == "BR")
            {
                user.MuteBuddyRequests = true;
            }
            else if (muteType == "LOGINS")
            {
                user.MuteLogins = true;
            }
            else if (muteType == "ALL")
            {
                user.MuteAll = true;
            } else
            {
                message += Messages.MuteHelp;
                goto leave;
            }

        leave:;
            
            byte[] chatPacket = PacketBuilder.CreateChat(message, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            return true;
        }
    }
}
