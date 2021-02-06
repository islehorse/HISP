using HISP.Player;
using HISP.Server;
using HISP.Game.Items;
using System;
using System.Collections.Generic;

namespace HISP.Game.Chat
{
    public class Command
    {

        public static bool Give(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;
            if (!user.Administrator)
                return false;
            if(args[0] == "OBJECT")
            {
                int itemId = 0;
                try
                {
                    itemId = int.Parse(args[1]);
                    Item.GetItemById(itemId);
                    ItemInstance newItemInstance = new ItemInstance(itemId);
                    user.Inventory.AddIgnoringFull(newItemInstance);
                    
                }
                catch(Exception)
                {
                    return false;
                }
            }
            else if (args[0] == "MONEY")
            {
                int money = 0;
                try
                {
                    money = int.Parse(args[1]);
                    user.Money += money;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (args[0] == "QUEST")
            {
                int questId = 0;
                try
                {
                    questId = int.Parse(args[1]);
                    Quest.ActivateQuest(user, Quest.GetQuestById(questId));
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);
            return true;
        }

        public static bool UnBan(string message, string[] args, User user)
        {
            if(args.Length <= 0)
                return false;
            if(!user.Administrator || !user.Moderator)
                return false;

            try{
                string userName = args[0];
                int id = Database.GetUserid(userName);
                Database.UnBanUser(id);
            }
            catch(Exception e)
            {
                Logger.ErrorPrint(e.Message);
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            return true;
        }

        public static bool Ban(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;
            if(!user.Administrator || !user.Moderator)
                return false;
            try{
                string userName = args[0];
                int id = Database.GetUserid(userName);
                string ip = Database.GetIpAddress(id);
                string reason = "NONE SPECIFIED";
                if (args.Length >= 2)
                {
                    reason = string.Join(" ", args, 1, args.Length - 1);
                }

                Database.BanUser(id, ip, reason);
            }
            catch(Exception)
            {
                return false;
            }
            try{
                User bannedUser = GameServer.GetUserByName(args[0]);
                bannedUser.LoggedinClient.Kick(Messages.KickReasonBanned);
            }
            catch(KeyNotFoundException){};

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            return true;
        }

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

        public static bool NoClip(string message, string[] args, User user)
        {
            if (!user.Administrator)
                return false;
            
            user.NoClip = !user.NoClip;
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);
            return true;
        }

        public static bool Kick(string message, string[] args, User user)
        {
            if (!user.Administrator || !user.Moderator)
                return false;
            if (args.Length <= 0)
                return false;

            try
            {
                User toKick = GameServer.GetUserByName(args[0]);

                if (args.Length >= 2)
                {
                    string reason = string.Join(" ", args, 1, args.Length - 1);
                    toKick.LoggedinClient.Kick(reason);
                }
                else
                    toKick.LoggedinClient.Kick(Messages.KickReasonKicked);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);
            return true;
        }

        public static bool Goto(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;
            if (!user.Administrator)
                return false;
            if(args[0] == "PLAYER")
            {
                if(args.Length < 2)
                    return false;
                try
                {
                    User teleportTo = GameServer.GetUserByName(args[1]);
                    user.Teleport(teleportTo.X, teleportTo.Y);
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
            }
            else if(args[0] == "AREA")
            {
                if (args.Length < 2)
                    return false;
                
                try
                {
                    string area = string.Join(" ", args, 1, args.Length - 1);
                    bool teleported = false;
                    foreach(World.Waypoint waypnt in World.Waypoints)
                    {
                        if(waypnt.Name.ToLower().StartsWith(area.ToLower()))
                        {
                            user.Teleport(waypnt.PosX, waypnt.PosY);
                            teleported = true;
                        }
                    }
                    if(!teleported)
                        return false;
                }
                catch(Exception)
                {
                    return false;
                }
            }
            if(args[0].Contains(","))
            {
                try
                {
                    string[] xy = args[0].Split(',');
                    int x = int.Parse(xy[0]);
                    int y = int.Parse(xy[1]);
                    user.Teleport(x, y);
                }
                catch(FormatException)
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
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message.Substring(1));

            if (args.Length <= 0)
            {
                formattedmessage += Messages.MuteHelp;
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
                formattedmessage += Messages.MuteHelp;
                goto leave;
            }

        leave:;
            
            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            return true;
        }
    }
}
