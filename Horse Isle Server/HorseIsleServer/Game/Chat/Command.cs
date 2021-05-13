using HISP.Player;
using HISP.Server;
using HISP.Game.Items;
using System;
using System.Collections.Generic;
using HISP.Game.Events;
using HISP.Game.Horse;

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
            if(args[0].ToUpper() == "OBJECT")
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
            else if (args[0].ToUpper() == "MONEY")
            {
                int money = 0;
                try
                {
                    money = int.Parse(args[1]);
                    user.AddMoney(money);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (args[0].ToUpper() == "QUEST")
            {
                int questId = 0;
                try
                {
                    questId = int.Parse(args[1]);
                    if(args.Length >= 3)
                    {
                        if (args[0].ToUpper() == "FORCE")
                        {
                            Quest.CompleteQuest(user, Quest.GetQuestById(questId));
                            goto msg;
                        }
                    }
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
        msg:;
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
        public static bool Escape(string message, string[] args, User user)
        {
            if (!user.Administrator || !user.Moderator)
                return false;


            user.Teleport(Map.ModIsleX, Map.ModIsleY);

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)) + Messages.ModIsleMessage, PacketBuilder.CHAT_BOTTOM_LEFT);
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
            if(args[0].ToUpper() == "PLAYER")
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
            else if(args[0].ToUpper() == "AREA")
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
                            break;
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
            else if(args[0].ToUpper() == "NPC")
            {
                if (args.Length < 2)
                    return false;

                try
                {
                    string npcName = string.Join(" ", args, 1, args.Length - 1);
                    bool teleported = false;
                    foreach (Npc.NpcEntry npc in Npc.NpcList)
                    {
                        if (npc.Name.ToLower().StartsWith(npcName.ToLower()))
                        {
                            user.Teleport(npc.X, npc.Y);
                            teleported = true;
                            break;
                        }
                    }
                    if (!teleported)
                        return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if(args[0].Contains(","))
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
            else
            {
                return false;
            }

        

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message.Substring(1)), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            
            return true;
        }
        public static bool Warp(string message, string[] args, User user)
        {

            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message.Substring(1));

            if (user.CurrentlyRidingHorse == null)
                goto onlyRiddenUnicorn;

            if (user.CurrentlyRidingHorse.Breed.Type == "unicorn")
                goto doCommand;

            goto onlyRiddenUnicorn;

            onlyRiddenUnicorn:;
            formattedmessage = Messages.OnlyUnicornCanWarp;
            goto sendText;
            

            cantUnderstandCommand:;
            formattedmessage += Messages.FailedToUnderstandLocation;
            goto sendText;

            
            doCommand:;

            if (args.Length <= 0)
            {
                goto cantUnderstandCommand;
            }
            else
            {
                string areaName = string.Join(" ", args).ToLower(); 
                foreach (GameClient client in GameServer.ConnectedClients)
                {
                    if (client.LoggedIn)
                    {
                        if(client.LoggedinUser.Username.ToLower().Contains(areaName))
                        {
                            user.Teleport(client.LoggedinUser.X, client.LoggedinUser.Y);
                            formattedmessage += Messages.SuccessfullyWarpedToPlayer;
                            goto playSwf;
                        }
                    }
                }

                foreach(World.Waypoint waypoint in World.Waypoints)
                {
                    if(waypoint.Name.ToLower().Contains(areaName))
                    {
                        user.Teleport(waypoint.PosX, waypoint.PosY);
                        formattedmessage += Messages.SuccessfullyWarpedToLocation;
                        goto playSwf;
                    }
                }

                goto cantUnderstandCommand;
            }

            playSwf:;
            byte[] swfPacket = PacketBuilder.CreateSwfModulePacket("warpcutscene", PacketBuilder.PACKET_SWF_CUTSCENE);
            user.LoggedinClient.SendPacket(swfPacket);


            sendText:;
            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            return true;
        }

        public static bool CallHorse(string message, string[] args, User user)
        {
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message.Substring(1));

            WildHorse horse = WildHorse.WildHorses[GameServer.RandomNumberGenerator.Next(0, WildHorse.WildHorses.Length)];
            horse.X = user.X;
            horse.Y = user.Y;

            GameServer.UpdateAreaForAll(user.X, user.Y);

            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);
            return true;

        }

        public static bool Dance(string message, string[] args, User user)
        {
            string moves = string.Join(" ", args).ToLower();
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message.Substring(1));

            if (user.ActiveDance != null)
                user.ActiveDance.Dispose();

            user.ActiveDance = new Dance(user, moves);

            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);
            return true;

        }

        public static bool Quiz(string message, string[] args, User user)
        {
            bool quizActive = (GameServer.QuizEvent != null);
            if(user.InRealTimeQuiz)
            {
                byte[] cantEnterRealTimeQuiz = PacketBuilder.CreateChat(Messages.EventAlreadyEnteredRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.LoggedinClient.SendPacket(cantEnterRealTimeQuiz);
                return false;
            }
            if (quizActive)
            {
                string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message.Substring(1));

                RealTimeQuiz.Participent participent = GameServer.QuizEvent.JoinEvent(user);

                if(participent.Quit)
                {
                    byte[] quizQuit = PacketBuilder.CreateChat(Messages.EventQuitRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    user.LoggedinClient.SendPacket(quizQuit);

                    return false;
                }
                
                participent.UpdateParticipent();
                byte[] enteredRealTimeQuiz = PacketBuilder.CreateChat(Messages.EventEnteredRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.LoggedinClient.SendPacket(enteredRealTimeQuiz);

                byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
                user.LoggedinClient.SendPacket(chatPacket);
                return true;

            } 
            else
            {
                byte[] quizUnavailable = PacketBuilder.CreateChat(Messages.EventUnavailableRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.LoggedinClient.SendPacket(quizUnavailable);
                return false;
            }

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

            if (muteType.ToUpper() == "GLOBAL")
            {
                user.MuteGlobal = true;
            } else if (muteType.ToUpper() == "ISLAND")
            {
                user.MuteIsland = true;
            } else if (muteType.ToUpper() == "NEAR")
            {
                user.MuteNear = true;
            } else if (muteType.ToUpper() == "HERE")
            {
                user.MuteHere = true;
            } else if (muteType.ToUpper() == "BUDDY")
            {
                user.MuteBuddy = true;
            } else if (muteType.ToUpper() == "SOCIALS")
            {
                user.MuteSocials = true;
            }
            else if (muteType.ToUpper() == "PM")
            {
                user.MutePrivateMessage = true;
            }
            else if (muteType.ToUpper() == "BR")
            {
                user.MuteBuddyRequests = true;
            }
            else if (muteType.ToUpper() == "LOGINS")
            {
                user.MuteLogins = true;
            }
            else if (muteType.ToUpper() == "ALL")
            {
                user.MuteAll = true;
                user.MuteGlobal = true;
                user.MuteIsland = true;
                user.MuteNear = true;
                user.MuteHere = true;
                user.MuteBuddy = true;
                user.MuteSocials = true;
                user.MutePrivateMessage = true;
                user.MuteBuddyRequests = true;
                user.MuteLogins = true;
            } 
            else
            {
                formattedmessage += Messages.MuteHelp;
                goto leave;
            }

        leave:;
            
            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            return true;
        }

        public static bool UnMute(string message, string[] args, User user)
        {
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message.Substring(1));

            if (args.Length <= 0)
            {
                formattedmessage += Messages.UnMuteHelp;
                goto leave;
            }

            string muteType = args[0];

            if (muteType.ToUpper() == "GLOBAL")
            {
                user.MuteGlobal = false;
            }
            else if (muteType.ToUpper() == "ISLAND")
            {
                user.MuteIsland = false;
            }
            else if (muteType.ToUpper() == "NEAR")
            {
                user.MuteNear = false;
            }
            else if (muteType.ToUpper() == "HERE")
            {
                user.MuteHere = false;
            }
            else if (muteType.ToUpper() == "BUDDY")
            {
                user.MuteBuddy = false;
            }
            else if (muteType.ToUpper() == "SOCIALS")
            {
                user.MuteSocials = false;
            }
            else if (muteType.ToUpper() == "PM")
            {
                user.MutePrivateMessage = false;
            }
            else if (muteType.ToUpper() == "BR")
            {
                user.MuteBuddyRequests = false;
            }
            else if (muteType.ToUpper() == "LOGINS")
            {
                user.MuteLogins = false;
            }
            else if (muteType.ToUpper() == "ALL")
            {
                user.MuteAll = false;
                user.MuteGlobal = false;
                user.MuteIsland = false;
                user.MuteNear = false;
                user.MuteHere = false;
                user.MuteBuddy = false;
                user.MuteSocials = false;
                user.MutePrivateMessage = false;
                user.MuteBuddyRequests = false;
                user.MuteLogins = false;
            }
            else
            {
                formattedmessage += Messages.UnMuteHelp;
                goto leave;
            }

        leave:;

            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.LoggedinClient.SendPacket(chatPacket);

            return true;
        }
    }
}
