using HISP.Player;
using HISP.Server;
using HISP.Game.Items;
using HISP.Game.Events;
using HISP.Game.Horse;
using HISP.Game.Inventory;

using System.Linq;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HISP.Game.Chat
{
    public class Command
    {
        private static User findNamePartial(string name)
        {
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client == null)
                    continue;
                if (client.LoggedIn)
                {
                    if (client.User.Username.ToLower().StartsWith(name.ToLower()))
                    {
                        return client.User;
                    }
                }
            }
            throw new KeyNotFoundException("name not found");
        }

        public static void RegisterCommands()
        {
            // Admin Commands
            new CommandRegister('%', "GIVE", "OBJECT <itemid / RANDOM> [username / ALL]\nMONEY <amount> [username]\nHORSE <breedid> [username]\nQUEST <questid> [FORCE]\nAWARD <awardid> [username]", Command.Give, true, false);
            new CommandRegister('%', "SWF", "<swf> [username / ALL]", Command.Swf, true, false);
            new CommandRegister('%', "GOTO", "<x>,<y>\nPLAYER <playername>\nAREA <locationname>\nNPC <npcname>", Command.Goto, true, false);
            new CommandRegister('%', "JUMP", "<username> HERE", Command.Jump, true, false);
            new CommandRegister('%', "NOCLIP", "", Command.NoClip, true, false);
            new CommandRegister('%', "MODHORSE", "<slot id> <stat> <value>", Command.ModHorse, true, false);
            new CommandRegister('%', "DELITEM", "<item id> [username]", Command.DelItem, true, false);
            new CommandRegister('%', "SHUTDOWN", "", Command.Shutdown, true, false);
            new CommandRegister('%', "CALL", "HORSE", Command.CallHorse, true, false);
            new CommandRegister('%', "MESSAGE", "<message>", Command.Message, true, false);
            new CommandRegister('%', "PERMISSION", "<username> <admin / moderator / normal>", Command.Permission, true, false);

            // Moderator commands
            new CommandRegister('%', "RULES", "<username>", Command.Rules, false, true);
            new CommandRegister('%', "PRISON", "<username>", Command.Prison, false, true);
            new CommandRegister('%', "STEALTH", "", Command.Stealth, false, true);
            new CommandRegister('%', "KICK", "<username> [reason]", Command.Kick, false, true);
            new CommandRegister('%', "BAN", "<username> [reason]", Command.Ban, false, true);
            new CommandRegister('%', "UNBAN", "<username>", Command.UnBan, false, true);
            new CommandRegister('%', "ESCAPE", "", Command.Escape, false, true);

            // User commands
            new CommandRegister('%', "VERSION", "", Command.Version, false, false);
            new CommandRegister('%', "HELP", "", Command.Help, false, false);

            new CommandRegister('!', "MUTE", "ALL\nGLOBAL\nISLAND\nNEAR\nHERE\nBUDDY\nPM\nBR\nSOCIALS\nLOGINS", Command.Mute, false, false);
            new CommandRegister('!', "UNMUTE", "ALL\nGLOBAL\nISLAND\nNEAR\nHERE\nBUDDY\nPM\nBR\nSOCIALS\nLOGINS", Command.UnMute, false, false);
            new CommandRegister('!', "HEAR", "ALL\nGLOBAL\nISLAND\nNEAR\nHERE\nBUDDY\nPM\nBR\nSOCIALS\nLOGINS", Command.UnMute, false, false);
            new CommandRegister('!', "AUTOREPLY", "[message]", Command.AutoReply, false, false);
            new CommandRegister('!', "QUIZ", "", Command.Quiz, false, false);
            new CommandRegister('!', "WARP", "<username / location>", Command.Warp, false, false);
            new CommandRegister('!', "DANCE", "<udlr>", Command.Dance, false, false);
        }

        public static bool Help(string message, string[] args, User user)
        {

            foreach (CommandRegister cmd in CommandRegister.RegisteredCommands)
            {
                if (!cmd.HasPermission(user)) continue;
                
                user.Client.SendPacket(PacketBuilder.CreateChat(Messages.FormatHispHelpUsage(cmd.CmdLetter, cmd.CmdName, cmd.CmdUsage).Replace("\n", "<BR>\t"), PacketBuilder.CHAT_BOTTOM_LEFT));
            }

            user.Client.SendPacket(PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT));

            return true;
        }

        public static bool Permission(string message, string[] args, User user)
        {
            if (args.Length < 2)
                return false;
            string username = args[0].Trim();
            string privledgeLevel = args[1].ToUpper().Trim();

            try
            {
                User modifyUser = findNamePartial(username);

                if (privledgeLevel == "NORMAL")
                {
                    modifyUser.Administrator = false;
                    modifyUser.Moderator = false;
                }
                else if (privledgeLevel == "ADMIN")
                {
                    modifyUser.Administrator = true;
                    modifyUser.Moderator = true;
                }
                else if (privledgeLevel == "MOD" || privledgeLevel == "MODERATOR")
                {
                    modifyUser.Administrator = false;
                    modifyUser.Moderator = true;
                }
                else
                {
                    return false;
                }
            }
            catch (KeyNotFoundException) 
            {
                try
                {
                    int playerId = Database.GetUserid(username);

                    if (privledgeLevel == "NORMAL")
                    {
                        Database.SetUserAdmin(playerId, false);
                        Database.SetUserMod(playerId, false);
                    }
                    else if (privledgeLevel == "ADMIN")
                    {
                        Database.SetUserAdmin(playerId, true);
                        Database.SetUserMod(playerId, true);
                    }
                    else if (privledgeLevel == "MOD" || privledgeLevel == "MODERATOR")
                    {
                        Database.SetUserAdmin(playerId, false);
                        Database.SetUserMod(playerId, true);
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (KeyNotFoundException) { return false; };
            };

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            

            return true;
        }

        public static bool Message(string message, string[] args, User user)
        {

            string serverAnnoucement = String.Join(" ", args);

            byte[] chatLeftPacket = PacketBuilder.CreateChat(Messages.FormatServerAnnoucement(serverAnnoucement), PacketBuilder.CHAT_BOTTOM_LEFT);
            byte[] chatRightPacket = PacketBuilder.CreateChat(Messages.FormatServerAnnoucement(serverAnnoucement), PacketBuilder.CHAT_BOTTOM_RIGHT);

            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    client.SendPacket(chatLeftPacket);
                    client.SendPacket(chatRightPacket);
                }
            }

            return true;
        }
        public static bool Shutdown(string message, string[] args, User user)
        {  

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            GameServer.ShutdownServer("Administrator initiated");

            return true;
        }
        public static bool Give(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;
            if(args[0].ToUpper() == "OBJECT")
            {
                int itemId = 0;
                try
                {
                    if(args[1] != "RANDOM")
                    {
                        itemId = int.Parse(args[1]);
                    }
                    else
                    {
                        itemId = Item.GetRandomItem().Id;
                    }

                    Item.GetItemById(itemId); // Calling this makes sure this item id exists.

                    ItemInstance newItemInstance = new ItemInstance(itemId);
                    
                    if (itemId == Item.Present)
                        newItemInstance.Data = Item.GetRandomItem().Id;

                    if (args.Length >= 3)
                    {
                        if(args[2] == "ALL")
                        {
                            foreach (GameClient client in GameClient.ConnectedClients)
                            {
                                if (client.LoggedIn)
                                {
                                    ItemInstance itmInstance = new ItemInstance(itemId);

                                    if (itemId == Item.Present)
                                        itmInstance.Data = Item.GetRandomItem().Id;

                                    client.User.Inventory.AddIgnoringFull(itmInstance);
                                }
                            }
                        }
                        else
                        {
                            findNamePartial(args[2]).Inventory.AddIgnoringFull(newItemInstance);
                        }
                    }
                    else
                    {
                        user.Inventory.AddIgnoringFull(newItemInstance);
                    }
                }
                catch(Exception)
                {
                    return false;
                }
            }
            else if (args[0].ToUpper() == "HORSE")
            {
                int horseId = 0;
                try
                {
                    horseId = int.Parse(args[1]);
                    HorseInstance horse = new HorseInstance(HorseInfo.GetBreedById(horseId));

                    if (args.Length >= 3)
                    {
                        findNamePartial(args[2]).HorseInventory.AddHorse(horse);
                    }
                    else
                    {
                        user.HorseInventory.AddHorse(horse);
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if(args[0].ToUpper() == "AWARD")
            {
                int awardId = 0;
                try
                {
                    awardId = int.Parse(args[1]);
                    if (args.Length >= 3)
                    {
                        findNamePartial(args[2]).Awards.AddAward(Award.GetAwardById(awardId));
                    }
                    else
                    {
                        user.Awards.AddAward(Award.GetAwardById(awardId));
                    }

                }
                catch (Exception)
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
                    if (args.Length >= 3)
                    {
                        findNamePartial(args[2]).AddMoney(money);
                    }
                    else
                    {
                        user.AddMoney(money);
                    }
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
                        if (args[2].ToUpper() == "FORCE")
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
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }

        public static bool Swf(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;


            try
            {
                string swfName = args[0];
                string swfUser = user.Username;
                if (args.Length <= 2)
                    swfUser = args[1];

                byte[] packetBytes = PacketBuilder.CreateSwfModule(swfName, PacketBuilder.PACKET_SWF_MODULE_FORCE);
                if (swfUser.ToUpper() == "ALL")
                {
                    foreach (GameClient client in GameClient.ConnectedClients)
                    {
                        if (client.LoggedIn)
                            client.SendPacket(packetBytes);
                    }
                }
                else
                {
                    User player = findNamePartial(swfUser);
                    player.Client.SendPacket(packetBytes);
                }
            }
            catch (Exception)
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            return true;
        }

        public static bool UnBan(string message, string[] args, User user)
        {
            if(args.Length <= 0)
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

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            return true;
        }

        public static bool Version(string message, string[] args, User user)
        {
            // Get current version and send to client
            byte[] versionPacket = PacketBuilder.CreateChat(ServerVersion.GetBuildString(), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(versionPacket);

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatPlayerCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }
        public static bool Ban(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;

            try
            {
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
                bannedUser.Client.Kick(Messages.KickReasonBanned);
            }
            catch(KeyNotFoundException){};

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            return true;
        }
        public static bool Escape(string message, string[] args, User user)
        {

            user.Teleport(Map.ModIsleX, Map.ModIsleY);

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message) + Messages.ModIsleMessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }

        public static bool Stealth(string message, string[] args, User user)
        {

            user.Stealth = !user.Stealth;
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }
        public static bool NoClip(string message, string[] args, User user)
        {
            user.NoClip = !user.NoClip;
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }

        public static bool Rules(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;

            try
            {
                User toSend = GameServer.GetUserByName(args[0]);

                toSend.Teleport(Map.RulesIsleX, Map.RulesIsleY);
                byte[] studyTheRulesMsg = PacketBuilder.CreateChat(Messages.RulesIsleSentMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                toSend.Client.SendPacket(studyTheRulesMsg);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message)+Messages.FormatRulesCommandMessage(args[0]), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }

        public static bool Prison(string message, string[] args, User user)
        {
            
            if (args.Length <= 0)
                return false;

            try
            {
                User toSend = GameServer.GetUserByName(args[0]);
                
                toSend.Teleport(Map.PrisonIsleX, Map.PrisonIsleY);
                byte[] dontDoTheTime = PacketBuilder.CreateChat(Messages.PrisonIsleSentMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                toSend.Client.SendPacket(dontDoTheTime);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message) + Messages.FormatPrisonCommandMessage(args[0]), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;

        }
        public static bool Kick(string message, string[] args, User user)
        {

            if (args.Length <= 0)
                return false;

            try
            {
                User toKick = GameServer.GetUserByName(args[0]);

                if (args.Length >= 2)
                {
                    string reason = string.Join(" ", args, 1, args.Length - 1);
                    toKick.Client.Kick(reason);
                }
                else
                {
                    toKick.Client.Kick(Messages.KickReasonKicked);
                }
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }

        public static bool Jump(string message, string[] args, User user)
        {
            if (args.Length < 2)
                return false;


            try
            {
                User tp = findNamePartial(args[0]);
                if (args[1].ToUpper() == "HERE")
                    tp.Teleport(user.X, user.Y);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }

        public static bool DelItem(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;

            int itemId = 0;
            try
            {
                itemId = int.Parse(args[0]);
                User target = user;
                if (args.Length > 1)
                    target = findNamePartial(args[1]);

                if (target.Inventory.HasItemId(itemId))
                {
                    InventoryItem itm = target.Inventory.GetItemByItemId(itemId);
                    
                    foreach (ItemInstance instance in itm.ItemInstances)
                    {
                        target.Inventory.Remove(instance);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            return true;
        }
        public static bool Goto(string message, string[] args, User user)
        {
            if (args.Length <= 0)
                return false;
            if(args[0].ToUpper() == "PLAYER")
            {
                if(args.Length < 2)
                    return false;
                try
                {
                    User tpTo = findNamePartial(args[1]);
                    user.Teleport(tpTo.X, tpTo.Y);
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

        

            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            
            return true;
        }

        public static bool ModHorse(string message, string[] args, User user)
        {
            if (args.Length < 3)
                return false;

            
            int id = 0;
            int amount = 0;
            try
            {
                id = int.Parse(args[0]);
                if(args[1].ToUpper() != "COLOR")
                    amount = int.Parse(args[2]);
            }
            catch (Exception)
            {
                return false;
            }


            int i = 0;
            foreach (HorseInfo.Category category in HorseInfo.HorseCategories)
            {
                HorseInstance[] horsesInCategory = user.HorseInventory.GetHorsesInCategory(category).OrderBy(o => o.Name).ToArray();
                if (horsesInCategory.Length > 0)
                {
                    foreach (HorseInstance instance in horsesInCategory)
                    {
                        i++;

                        if(i == id)
                        {
                            switch (args[1].ToUpper())
                            {
                                case "INTELLIGENCE":
                                    instance.AdvancedStats.Inteligence = amount;
                                    break;
                                case "PERSONALITY":
                                    instance.AdvancedStats.Personality = amount;
                                    break;
                                case "HEIGHT":
                                    instance.AdvancedStats.Height = amount;
                                    break;
                                case "COLOR":
                                    instance.Color = args[2].ToLower();
                                    break;
                                case "EXPERIENCE":
                                    instance.BasicStats.Experience = amount;
                                    break;
                                case "SPEED":
                                    instance.AdvancedStats.Speed = amount;
                                    break;
                                case "STRENGTH":
                                    instance.AdvancedStats.Strength = amount;
                                    break;
                                case "CONFORMATION":
                                    instance.AdvancedStats.Conformation = amount;
                                    break;
                                case "ENDURANCE":
                                    instance.AdvancedStats.Endurance = amount;
                                    break;
                                case "AGILITY":
                                    instance.AdvancedStats.Agility = amount;
                                    break;
                            }
                        }
                    }
                }
            }


            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAdminCommandCompleteMessage(message), PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }

        public static bool Warp(string message, string[] args, User user)
        {

            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message);

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

            string areaName = string.Join(" ", args).ToLower();
            areaName = areaName.Trim();
            if (args.Length <= 0)
                areaName = "horse isle";
            try
            {
                User tp = GameServer.GetUserByName(areaName);

                user.Teleport(tp.X, tp.Y);
                formattedmessage += Messages.SuccessfullyWarpedToPlayer;
                goto playSwf;

            }
            catch (KeyNotFoundException)
            {
                foreach (World.Waypoint waypoint in World.Waypoints)
                {
                    if (waypoint.Name.ToLower().StartsWith(areaName))
                    {
                        user.Teleport(waypoint.PosX, waypoint.PosY);
                        formattedmessage += Messages.SuccessfullyWarpedToLocation;
                        goto playSwf;
                    }
                }

                goto cantUnderstandCommand;
            }                

            playSwf:;
            byte[] swfPacket = PacketBuilder.CreateSwfModule("warpcutscene", PacketBuilder.PACKET_SWF_MODULE_CUTSCENE);
            user.Client.SendPacket(swfPacket);


            sendText:;
            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            return true;
        }

        public static bool CallHorse(string message, string[] args, User user)
        {

            if (args.Length <= 0)
                return false;
            string formattedmessage = "";
            try
            {
                if (args[0].ToUpper() != "HORSE")
                    return false;

                formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message);

                WildHorse horse = WildHorse.WildHorses[GameServer.RandomNumberGenerator.Next(0, WildHorse.WildHorses.Length)];
                horse.X = user.X;
                horse.Y = user.Y;

                GameServer.UpdateAreaForAll(user.X, user.Y);
            }
            catch (Exception)
            {
                return false;
            }
            

            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;

        }

        public static bool AutoReply(string message, string[] args, User user)
        {
            string replyMessage = string.Join(" ", args);
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message);
            replyMessage = replyMessage.Trim();

            if (replyMessage.Length > 1024)
            {
                byte[] tooLong = PacketBuilder.CreateChat(Messages.AutoReplyTooLong, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.Client.SendPacket(tooLong);

                return false;
            }

            Object violationReason = ChatMsg.FilterMessage(replyMessage);
            if (violationReason != null)
            {
                byte[] hasVios = PacketBuilder.CreateChat(Messages.AutoReplyHasViolations, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.Client.SendPacket(hasVios);

                return false;
            }

            user.AutoReplyText = replyMessage;

            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;
        }
        public static bool Dance(string message, string[] args, User user)
        {
            string moves = string.Join(" ", args).ToLower();
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message);

            if (user.ActiveDance != null)
                user.ActiveDance.Dispose();

            user.ActiveDance = new Dance(user, moves);

            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);
            return true;

        }

        public static bool Quiz(string message, string[] args, User user)
        {
            bool quizActive = (GameServer.QuizEvent != null);
            if(user.InRealTimeQuiz)
            {
                byte[] cantEnterRealTimeQuiz = PacketBuilder.CreateChat(Messages.EventAlreadyEnteredRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.Client.SendPacket(cantEnterRealTimeQuiz);
                return false;
            }
            if (quizActive)
            {
                string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message);

                RealTimeQuiz.Participent participent = GameServer.QuizEvent.JoinEvent(user);

                if(participent.Quit)
                {
                    byte[] quizQuit = PacketBuilder.CreateChat(Messages.EventQuitRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    user.Client.SendPacket(quizQuit);

                    return false;
                }
                
                participent.UpdateParticipent();
                byte[] enteredRealTimeQuiz = PacketBuilder.CreateChat(Messages.EventEnteredRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.Client.SendPacket(enteredRealTimeQuiz);

                byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
                user.Client.SendPacket(chatPacket);
                return true;

            } 
            else
            {
                byte[] quizUnavailable = PacketBuilder.CreateChat(Messages.EventUnavailableRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.Client.SendPacket(quizUnavailable);
                return false;
            }

        }

        public static bool Mute(string message, string[] args, User user)
        {
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message);

            if (args.Length <= 0)
            {
                formattedmessage += Messages.MuteHelp;
            }
            else
            {

                string muteType = args[0];

                if (muteType.ToUpper() == "GLOBAL")
                {
                    user.MuteGlobal = true;
                }
                else if (muteType.ToUpper() == "ISLAND")
                {
                    user.MuteIsland = true;
                }
                else if (muteType.ToUpper() == "NEAR")
                {
                    user.MuteNear = true;
                }
                else if (muteType.ToUpper() == "HERE")
                {
                    user.MuteHere = true;
                }
                else if (muteType.ToUpper() == "BUDDY")
                {
                    user.MuteBuddy = true;
                }
                else if (muteType.ToUpper() == "SOCIALS")
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
                else if (muteType.ToUpper() == "ADS")
                {
                    user.MuteAds = true;
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
                }
            }
            
            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            return true;
        }

        public static bool UnMute(string message, string[] args, User user)
        {
            string formattedmessage = Messages.FormatPlayerCommandCompleteMessage(message);

            if (args.Length <= 0)
            {
                formattedmessage += Messages.UnMuteHelp;
            }
            else
            {
                string muteType = args[0];

                if (muteType.ToUpper() == "GLOBAL")
                {
                    user.MuteGlobal = false;
                }
                else if (muteType.ToUpper() == "ISLAND")
                {
                    user.MuteIsland = false;
                }
                else if (muteType.ToUpper() == "ADS")
                {
                    user.MuteAds = false;
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
                }
            }


            byte[] chatPacket = PacketBuilder.CreateChat(formattedmessage, PacketBuilder.CHAT_BOTTOM_LEFT);
            user.Client.SendPacket(chatPacket);

            return true;
        }
    }
}
