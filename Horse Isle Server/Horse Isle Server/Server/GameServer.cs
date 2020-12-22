using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HISP.Player;
using HISP.Game;
using HISP.Security;
using HISP.Game.Chat;

namespace HISP.Server
{
    class GameServer
    {

        public static Socket ServerSocket;
        
        public static GameClient[] ConnectedClients // Done to prevent Enumerator Changed errors.
        {
            get {
                return connectedClients.ToArray();
            }
        }

        public static int IdleTimeout;
        public static int IdleWarning;

        public static Random RandomNumberGenerator = new Random();

        /*
         *  Private stuff 
         */
        private static int gameTickSpeed = 4320; // Changing this to ANYTHING else will cause desync with the client.
        private static List<GameClient> connectedClients = new List<GameClient>();
        private static Timer serverTimer;
        private static void onTick(object state)
        {
            World.TickWorldClock();

            if (World.ServerTime.Minutes % 30 == 0)
            {
                DroppedItems.Update();
            }
            serverTimer.Change(gameTickSpeed, gameTickSpeed);
        }

        /*
         * This section is where all the event handlers live, 
         * eg: OnMovementPacket is whenever the server receies a movement request from the client.
         */

        public static void OnCrossdomainPolicyRequest(GameClient sender)
        {
            Logger.DebugPrint("Cross-Domain-Policy request received from: " + sender.RemoteIp);

            byte[] crossDomainPolicyResponse = CrossDomainPolicy.GetPolicy(); 

            sender.SendPacket(crossDomainPolicyResponse);
        }
        public static void OnUserInfoRequest(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested user information when not logged in.");
                return;
            }
            Database.AddOnlineUser(sender.LoggedinUser.Id, sender.LoggedinUser.Administrator, sender.LoggedinUser.Moderator, sender.LoggedinUser.Subscribed);
            
            Logger.DebugPrint(sender.LoggedinUser.Username + " Requested user information.");

            User user = sender.LoggedinUser;

            byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
            sender.SendPacket(MovementPacket);

            byte[] WelcomeMessage = PacketBuilder.CreateWelcomeMessage(user.Username);
            sender.SendPacket(WelcomeMessage);

            byte[] WorldData = PacketBuilder.CreateWorldData(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, World.GetWeather());
            sender.SendPacket(WorldData);

            byte[] SecCodePacket = PacketBuilder.CreateSecCode(user.SecCodeSeeds, user.SecCodeInc, user.Administrator, user.Moderator);
            sender.SendPacket(SecCodePacket);

            byte[] BaseStatsPacketData = PacketBuilder.CreatePlayerData(user.Money, GameServer.GetNumberOfPlayers(), user.MailBox.MailCount);
            sender.SendPacket(BaseStatsPacketData);

            UpdateArea(sender);

            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (client.LoggedinUser.Id != user.Id)
                    {
                        byte[] PlayerInfo = PacketBuilder.CreatePlayerInfoUpdateOrCreate(client.LoggedinUser.X, client.LoggedinUser.Y, client.LoggedinUser.Facing, client.LoggedinUser.CharacterId, client.LoggedinUser.Username);
                        sender.SendPacket(PlayerInfo);
                    }
                }
            }

            foreach (User nearbyUser in GameServer.GetNearbyUsers(sender.LoggedinUser.X, sender.LoggedinUser.Y, false, false))
                if (nearbyUser.Id != sender.LoggedinUser.Id)
                    UpdateArea(nearbyUser.LoggedinClient);

            byte[] IsleData = PacketBuilder.CreatePlaceData(World.Isles.ToArray(), World.Towns.ToArray(), World.Areas.ToArray());
            sender.SendPacket(IsleData);

            byte[] TileFlags = PacketBuilder.CreateTileOverlayFlags(Map.OverlayTileDepth);
            sender.SendPacket(TileFlags);

            byte[] MotdData = PacketBuilder.CreateMotd();
            sender.SendPacket(MotdData);


        }
        public static void OnKeepAlive(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested update when not logged in.");
                return;
            }
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid update Packet");
                return;
            }

            if (packet[1] == PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                Logger.DebugPrint("Sending " + sender.LoggedinUser.Username + " updated info...");
                UpdatePlayer(sender);
            }
        }
        public static void OnStatsPacket(GameClient sender, byte[] packet)
        {
            if(!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested stats when not logged in.");
                return;
            }
            if(packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + "Sent an invalid Stats Packet");
                return;
            }


        }
        public static void OnProfilePacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested to change profile page when not logged in.");
                return;
            }
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Profile Packet");
                return;
            }

            byte method = packet[1];
            if(method == PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                string metaWind = Meta.BuildStatsMenu(sender.LoggedinUser);
                byte[] statsPacket = PacketBuilder.CreateMetaPacket(metaWind);
                sender.SendPacket(statsPacket);
            }
            if (method == PacketBuilder.VIEW_PROFILE)
            {
                sender.LoggedinUser.MetaPriority = true;
                string profilePage = sender.LoggedinUser.ProfilePage;
                profilePage = profilePage.Replace("<", "[");
                profilePage = profilePage.Replace(">", "]");
                byte[] profilePacket = PacketBuilder.CreateProfilePacket(profilePage);
                sender.SendPacket(profilePacket);
            }
            else if (method == PacketBuilder.SAVE_PROFILE)
            {

                string packetStr = Encoding.UTF8.GetString(packet);
                if (packet.Length < 3 || !packetStr.Contains('|'))
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Profile SAVE Packet");
                    return;
                }

                int characterId = (packet[2] - 20) * 64 + (packet[3] - 20);

                string profilePage = packetStr.Split('|')[1];
                profilePage = profilePage.Substring(0, profilePage.Length - 2);
                profilePage = profilePage.Replace("[", "<");
                profilePage = profilePage.Replace("]", ">");
                sender.LoggedinUser.CharacterId = characterId;
                sender.LoggedinUser.ProfilePage = profilePage;

                Logger.DebugPrint(sender.LoggedinUser.Username + " Changed to character id: " + characterId + " and set there Profile Description to '" + profilePage + "'");

                byte[] chatPacket = PacketBuilder.CreateChat(Messages.ProfileSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatPacket);

                UpdateArea(sender);
                UpdateUserInfo(sender.LoggedinUser);
            }
            else if (method == PacketBuilder.SECCODE_ITEM)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (Item.ItemIdExist(value))
                    {
                        ItemInstance itm = new ItemInstance(value);
                        sender.LoggedinUser.Inventory.Add(itm);
                        Item.ItemInformation itemInfo = Item.GetItemById(value);
                        byte[] earnedItemMessage = PacketBuilder.CreateChat(Messages.FormatYouEarnedAnItemMessage(itemInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(earnedItemMessage);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but tried to give an non existant item");
                        return;
                    }

                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_QUEST)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (Quest.DoesQuestExist(value))
                    {
                        Quest.QuestEntry questEntry = Quest.GetQuestById(value);
                        Quest.ActivateQuest(sender.LoggedinUser, questEntry);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but tried to activate a non existant quest");
                        return;
                    }


                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError,PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            

        }
        public static void OnMovementPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent movement packet when not logged in.");
                return;
            }

            User loggedInUser = sender.LoggedinUser;
            byte movementDirection = packet[1]; 

            if (movementDirection == PacketBuilder.MOVE_ESCAPE)
            {

                byte Direction;
                if (World.InSpecialTile(loggedInUser.X, loggedInUser.Y))
                {

                    int newX = loggedInUser.X;
                    int newY = loggedInUser.Y;

                    World.SpecialTile tile = World.GetSpecialTile(loggedInUser.X, loggedInUser.Y);
                    if (tile.ExitX != 0)
                        newX = tile.ExitX;
                    if (tile.ExitY != 0)
                        newY = tile.ExitY;
                    else
                        if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1))
                            newY += 1;



                    if (loggedInUser.X + 1 == newX && loggedInUser.Y == newY)
                        Direction = PacketBuilder.DIRECTION_RIGHT;
                    else if (loggedInUser.X - 1 == newX && loggedInUser.Y == newY)
                        Direction = PacketBuilder.DIRECTION_LEFT;
                    else if (loggedInUser.Y + 1 == newY && loggedInUser.X == newX)
                        Direction = PacketBuilder.DIRECTION_DOWN;
                    else if (loggedInUser.Y - 1 == newY && loggedInUser.X == newX)
                        Direction = PacketBuilder.DIRECTION_UP;
                    else
                        Direction = PacketBuilder.DIRECTION_TELEPORT;

                    loggedInUser.X = newX;
                    loggedInUser.Y = newY;


                }
                else
                {
                    if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1))
                        loggedInUser.Y += 1;

                    Direction = PacketBuilder.DIRECTION_DOWN;
                }

                Logger.DebugPrint("Exiting player: " + loggedInUser.Username + " to: " + loggedInUser.X + "," + loggedInUser.Y);
                byte[] moveResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, Direction, Direction, true);
                sender.SendPacket(moveResponse);
            }

            if (movementDirection == PacketBuilder.MOVE_UP)
            {
                loggedInUser.Facing = PacketBuilder.DIRECTION_UP;
                if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y - 1))
                {
                    loggedInUser.Y -= 1;

                    byte[] moveUpResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, sender.LoggedinUser.Facing, PacketBuilder.DIRECTION_UP, true);
                    sender.SendPacket(moveUpResponse);
                }
                else
                {
                    byte[] moveUpResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, sender.LoggedinUser.Facing, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveUpResponse);
                }
            }
            else if (movementDirection == PacketBuilder.MOVE_LEFT)
            {
                loggedInUser.Facing = PacketBuilder.DIRECTION_LEFT;
                if (Map.CheckPassable(loggedInUser.X - 1, loggedInUser.Y))
                {
                    loggedInUser.X -= 1;
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_LEFT, true);
                    sender.SendPacket(moveLeftResponse);
                }
                else
                {
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveLeftResponse);
                }
            }
            else if (movementDirection == PacketBuilder.MOVE_RIGHT)
            {
                loggedInUser.Facing = PacketBuilder.DIRECTION_RIGHT;
                if (Map.CheckPassable(loggedInUser.X + 1, loggedInUser.Y))
                {
                    loggedInUser.X += 1;
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_RIGHT, true);
                    sender.SendPacket(moveLeftResponse);
                }
                else
                {
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveLeftResponse);
                }
            }
            else if (movementDirection == PacketBuilder.MOVE_DOWN)
            {
                loggedInUser.Facing = PacketBuilder.DIRECTION_DOWN;
                if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1))
                {
                    loggedInUser.Y += 1;
                    byte[] moveDownResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_DOWN, true);
                    sender.SendPacket(moveDownResponse);
                }
                else
                {
                    byte[] moveDownResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveDownResponse);
                }

            }
            else if(movementDirection == PacketBuilder.MOVE_UPDATE)
            {
                Update(sender);
                return;
            }


            Update(sender);
        }
        public static void OnQuitPacket(GameClient sender, byte[] packet)
        {
            if(!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent quit packet when not logged in.");
                return;    
            }
            Logger.InfoPrint(sender.LoggedinUser.Username + " Clicked \"Quit Game\".. Disconnecting");
            sender.Disconnect();
        }
        public static void OnNpcInteraction(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent npc interaction packet when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid npc interaction packet.");
                return;
            }
            byte action = packet[1];
            if (action == PacketBuilder.NPC_START_CHAT)
            {

                string packetStr = Encoding.UTF8.GetString(packet);
                string number = packetStr.Substring(2, packetStr.Length - 4);
                int chatId = 0;
                try
                {
                    chatId = int.Parse(number);
                }
                catch (InvalidOperationException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to start talking to an NPC with id that is NaN.");
                    return;
                }
                sender.LoggedinUser.MetaPriority = true;
                Npc.NpcEntry entry = Npc.GetNpcById(chatId);
                string metaInfo = Meta.BuildChatpoint(sender.LoggedinUser, entry, entry.Chatpoints[0]);
                byte[] metaPacket = PacketBuilder.CreateMetaPacket(metaInfo);
                sender.SendPacket(metaPacket);

                sender.LoggedinUser.LastTalkedToNpc = entry;
            }
            else if (action == PacketBuilder.NPC_CONTINUE_CHAT)
            {
                string packetStr = Encoding.UTF8.GetString(packet);
                string number = packetStr.Substring(2, packetStr.Length - 4);
                int replyId = 0;
                try
                {
                    replyId = int.Parse(number);
                }
                catch (InvalidOperationException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to reply to an NPC with replyid that is NaN.");
                    return;
                }

                Npc.NpcEntry lastNpc = sender.LoggedinUser.LastTalkedToNpc;
                Npc.NpcReply reply;
                try
                {
                    reply = Npc.GetNpcReply(lastNpc, replyId);
                }
                catch(KeyNotFoundException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to reply with replyid that does not exist.");
                    return;
                }

                if (reply.GotoChatpoint == -1)
                {
                    UpdateArea(sender);
                    return;
                }
                sender.LoggedinUser.MetaPriority = true;
                string metaInfo = Meta.BuildChatpoint(sender.LoggedinUser, lastNpc, Npc.GetNpcChatpoint(lastNpc, reply.GotoChatpoint));
                byte[] metaPacket = PacketBuilder.CreateMetaPacket(metaInfo);
                sender.SendPacket(metaPacket);
                return;
            }
        }
        public static void OnTransportUsed(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent transport packet when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid transport packet.");
                return;
            }


            string packetStr = Encoding.UTF8.GetString(packet);
            string number = packetStr.Substring(1, packetStr.Length - 3);

            int transportid;
            try
            {
                transportid =  Int32.Parse(number);
            }
            catch(InvalidOperationException)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to use a transport with id that is NaN.");
                return;
            }
            try
            {
                Transport.TransportPoint transportPoint = Transport.GetTransportPoint(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                if (transportPoint.X != sender.LoggedinUser.X && transportPoint.Y != sender.LoggedinUser.Y)
                {
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use transport id: " + transportid.ToString() + " while not the correct transport point!");
                    return;
                }

                Transport.TransportLocation transportLocation = Transport.GetTransportLocation(transportid);


                if (sender.LoggedinUser.Money >= transportLocation.Cost)
                {
                    

                    string swfToLoad = Messages.BoatCutscene;
                    if (transportLocation.Type == "WAGON")
                        swfToLoad = Messages.WagonCutscene;

                    if (transportLocation.Type != "ROWBOAT")
                    {
                        byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket(swfToLoad, PacketBuilder.PACKET_SWF_CUTSCENE);
                        sender.SendPacket(swfModulePacket);
                    }

                    sender.LoggedinUser.Teleport(transportLocation.GotoX, transportLocation.GotoY);

                    byte[] welcomeToIslePacket = PacketBuilder.CreateChat(Messages.FormatWelcomeToAreaMessage(transportLocation.LocationTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(welcomeToIslePacket);

                    sender.LoggedinUser.Money -= transportLocation.Cost;
                }
                else
                {
                    byte[] cantAfford = PacketBuilder.CreateChat(Messages.CantAffordTransport, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantAfford);
                }
            }
            catch (KeyNotFoundException)
            {
                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use transport id: " + transportid.ToString() + " while not on a transport point!");
            }

         
        }
        public static void OnChatPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent chat packet when not logged in.");
                return;
            }

            if (packet.Length < 4)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid chat packet.");
                return;
            }


            string packetStr = Encoding.UTF8.GetString(packet);

            Chat.ChatChannel channel = (Chat.ChatChannel)packet[1];
            string message = packetStr.Substring(2, packetStr.Length - 4);

            if (Chat.ProcessCommand(sender.LoggedinUser, message))
            {
                Logger.DebugPrint(sender.LoggedinUser.Username + " Attempting to run command '" + message + "' in channel: " + channel.ToString());
                return;
            }
           

            Logger.DebugPrint(sender.LoggedinUser.Username + " Attempting to say '" + message + "' in channel: " + channel.ToString());

            string nameTo = null;
            if (channel == Chat.ChatChannel.Dm)
            {
                nameTo = Chat.GetDmRecipiant(message);
                message = Chat.GetDmMessage(message);
            }

            if (message == "")
                return;

            Object violationReason = Chat.FilterMessage(message);
            if (violationReason != null)
            {
                sender.LoggedinUser.ChatViolations += 1;
                string chatViolationMessage = Messages.FormatGlobalChatViolationMessage((Chat.Reason)violationReason);
                byte[] chatViolationPacket = PacketBuilder.CreateChat(chatViolationMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatViolationPacket);
                return;
            }

            byte chatSide = Chat.GetSide(channel);
            message = Chat.DoCorrections(message);
            message = Chat.EscapeMessage(message);


            string failedReason = Chat.NonViolationChecks(sender.LoggedinUser, message);
            if (failedReason != null)
            {
                byte[] failedMessage = PacketBuilder.CreateChat(failedReason, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(failedMessage);
                return;
            }

            GameClient[] recipiants = Chat.GetRecipiants(sender.LoggedinUser, channel, nameTo);

            // Finally send chat message.
            string formattedMessage = Chat.FormatChatForOthers(sender.LoggedinUser, channel, message);
            string formattedMessageSender = Chat.FormatChatForSender(sender.LoggedinUser, channel, message, nameTo);
            byte[] chatPacketOthers = PacketBuilder.CreateChat(formattedMessage, chatSide);
            byte[] chatPacketSender = PacketBuilder.CreateChat(formattedMessageSender, chatSide);
            byte[] playDmSound = PacketBuilder.CreatePlaysoundPacket(Chat.PrivateMessageSound);
            // Send to clients ...
            foreach (GameClient recipiant in recipiants)
            {
                recipiant.SendPacket(chatPacketOthers);
                if (channel == Chat.ChatChannel.Dm)
                    recipiant.SendPacket(playDmSound);
            }

            // Send to sender
            sender.SendPacket(chatPacketSender);
        }
        public static void OnClickPacket(GameClient sender, byte[] packet)
        {

            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Send click packet when not logged in.");
                return;
            }
            if (packet.Length < 6)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Click Packet");
                return;
            }
            
            string packetStr = Encoding.UTF8.GetString(packet);
            if(packetStr.Contains("|"))
            {
                string packetContents = packetStr.Substring(1, packetStr.Length - 3);
                string[] xy = packetContents.Split('|');
                int x = 0;
                int y = 0;

                try
                {
                    x = int.Parse(xy[0])+4;
                    y = int.Parse(xy[1])+1;
                }
                catch(FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a click packet with non-string xy value.");
                    return;
                }

                // Get description of tile 
                string returnedMsg = Messages.NothingInterestingHere;
                if(World.InSpecialTile(x, y))
                {
                    World.SpecialTile tile = World.GetSpecialTile(x, y);
                    if (tile.Title != null)
                        returnedMsg = tile.Title;
                }

                byte[] tileInfoPacket = PacketBuilder.CreateClickTileInfoPacket(returnedMsg);
                sender.SendPacket(tileInfoPacket);
            }
        }
        public static void OnItemInteraction(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent object interaction packet when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                return;
            }

            byte action = packet[1];
            switch(action)
            {
                case PacketBuilder.ITEM_PICKUP_ALL:
                    string chatMsg = Messages.GrabAllItemsMessage;
                    DroppedItems.DroppedItem[] droppedItems = DroppedItems.GetItemsAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                    foreach(DroppedItems.DroppedItem item in droppedItems)
                    {
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(item.instance);
                        }
                        catch (InventoryException)
                        {
                            chatMsg = Messages.GrabbedAllItemsButInventoryFull;
                            break;
                        }
                            
                        DroppedItems.RemoveDroppedItem(item);
                    }
                    UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);

                    byte[] chatMessage = PacketBuilder.CreateChat(chatMsg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(chatMessage);

                    break;
                case PacketBuilder.ITEM_PICKUP:
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string randomIdStr = packetStr.Substring(2, packet.Length - 4);
                    int randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch(InvalidOperationException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    try
                    {
                        DroppedItems.DroppedItem item = DroppedItems.GetDroppedItemById(randomId);
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(item.instance);
                        }
                        catch (InventoryException)
                        {
                            byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.GrabbedItemButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(inventoryFullMessage);
                            break;
                        }

                        
                        DroppedItems.RemoveDroppedItem(item);

                        UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);

                        chatMessage = PacketBuilder.CreateChat(Messages.GrabbedItemMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatMessage);
                    }
                    catch(KeyNotFoundException)
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to grab a non existing object.");
                        return;
                    }

                    break;
                case PacketBuilder.ITEM_WEAR:
                    const int MISC_FLAG_HEAD = 1;
                    const int MISC_FLAG_BODY = 2;
                    const int MISC_FLAG_LEGS = 3;
                    const int MISC_FLAG_FEET = 4;

                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        sender.LoggedinUser.Inventory.Remove(instance);

                        Item.ItemInformation itemInf = instance.GetItemInfo();
                        if(itemInf.MiscFlags.Length <= 0)
                        {
                            Logger.ErrorPrint(itemInf.Name + " Has no misc flags.");
                        }    
                        switch(itemInf.MiscFlags[0])
                        {
                            case MISC_FLAG_HEAD:
                                if(sender.LoggedinUser.EquipedCompetitionGear.Head == null)
                                    sender.LoggedinUser.EquipedCompetitionGear.Head = itemInf;
                                else
                                {
                                    ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Head.Id);
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                    sender.LoggedinUser.EquipedCompetitionGear.Head = itemInf;
                                }
                                break;
                            case MISC_FLAG_BODY:
                                if (sender.LoggedinUser.EquipedCompetitionGear.Body == null)
                                    sender.LoggedinUser.EquipedCompetitionGear.Body = itemInf;
                                else
                                {
                                    ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Body.Id);
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                    sender.LoggedinUser.EquipedCompetitionGear.Body = itemInf;
                                }
                                break;
                            case MISC_FLAG_LEGS:
                                if (sender.LoggedinUser.EquipedCompetitionGear.Legs == null)
                                    sender.LoggedinUser.EquipedCompetitionGear.Legs = itemInf;
                                else
                                {
                                    ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Legs.Id);
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                    sender.LoggedinUser.EquipedCompetitionGear.Legs = itemInf;
                                }
                                break;
                            case MISC_FLAG_FEET:
                                if (sender.LoggedinUser.EquipedCompetitionGear.Feet == null)
                                    sender.LoggedinUser.EquipedCompetitionGear.Feet = itemInf;
                                else
                                {
                                    ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Feet.Id);
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                    sender.LoggedinUser.EquipedCompetitionGear.Feet = itemInf;
                                }
                                break;
                        }




                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatEquipItemMessage(itemInf.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);
                        UpdateInventory(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to wear an item they did not have.");
                    }
                    break;


                case PacketBuilder.ITEM_DROP:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    if(sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        DroppedItems.AddItem(instance, sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        sender.LoggedinUser.Inventory.Remove(instance);
                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.DroppedAnItemMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);
                        UpdateInventory(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to drop an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_SHOVEL:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_SHOVEL with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.LoggedinUser, Quest.Shovel, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.ShovelNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_RAKE:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_RAKE with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.LoggedinUser, Quest.Rake, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.RakeNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_MAGNIFYING:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_MAGNIFYING with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.LoggedinUser, Quest.MagnifyingGlass, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.MagnifyNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_BINOCULARS:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_BINOCULARS with 3rd byte not 0x14.");
                    if(!Quest.UseTool(sender.LoggedinUser, Quest.Binoculars, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.BinocularsNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_SELL: // Handles selling an item.
                    int totalSold = 1;
                    int message = 1;

                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object sell packet.");
                        return;
                    }

                    if (!sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell a item that they doesnt have in there inventory");
                        return;
                    }

                    InventoryItem invItem = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                    int itemId = invItem.ItemId;
                    goto doSell;
                case PacketBuilder.ITEM_SELL_ALL:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string itemIdStr = packetStr.Substring(2, packet.Length - 2);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object sell packet.");
                        return;
                    }

                    if (!sender.LoggedinUser.Inventory.HasItemId(itemId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell a item that they doesnt have in there inventory");
                        return;
                    }
                    invItem = sender.LoggedinUser.Inventory.GetItemByItemId(itemId);

                    totalSold = invItem.ItemInstances.Count;
                    message = 2;
                    goto doSell;
                doSell:;

                    Item.ItemInformation itemInfo = Item.GetItemById(itemId);
                    Shop shop = sender.LoggedinUser.LastShoppedAt;
                    if (shop != null)
                    {
                        int sellPrice = shop.CalculateSellCost(itemInfo) * totalSold;
                        if (shop.CanSell(itemInfo))
                        {
                            for(int i = 0; i < totalSold; i++)
                            {
                                ItemInstance itemInstance = invItem.ItemInstances[0];
                                sender.LoggedinUser.Inventory.Remove(itemInstance);
                                shop.Inventory.Add(itemInstance);
                            }

                            sender.LoggedinUser.Money += sellPrice;

                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if(message == 1)
                            {
                                byte[] soldItemMessage = PacketBuilder.CreateChat(Messages.FormatSellMessage(itemInfo.Name, sellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(soldItemMessage);
                            }
                            if(message == 2)
                            {
                                string name = itemInfo.Name;
                                if (totalSold > 1)
                                    name = itemInfo.PluralName;

                                byte[] soldItemMessage = PacketBuilder.CreateChat(Messages.FormatSellAllMessage(name, sellPrice,totalSold), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(soldItemMessage);
                            }

                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell a item that was not avalible to be sold.");
                        }
                    }
                    break;


                case PacketBuilder.ITEM_BUY: // Handles buying an item.
                    message = 1;
                    int count = 1;
                    goto doPurchase;
                case PacketBuilder.ITEM_BUY_5:
                    message = 2;
                    count = 5;
                    goto doPurchase;
                case PacketBuilder.ITEM_BUY_25:
                    message = 3;
                    count = 25;
                doPurchase:;
                    packetStr = Encoding.UTF8.GetString(packet);
                    itemIdStr = packetStr.Substring(2, packet.Length - 2);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object buy packet.");
                        return;
                    }

                    itemInfo = Item.GetItemById(itemId);
                    shop = sender.LoggedinUser.LastShoppedAt;
                    if (shop != null)
                    {
                        int buyCost = shop.CalculateBuyCost(itemInfo) * count;
                        if (sender.LoggedinUser.Money < buyCost)
                        {
                            byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.CantAfford1, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAffordMessage);
                            return;
                        }
                        if (shop.Inventory.HasItemId(itemId))
                        {
                            if (shop.Inventory.GetItemByItemId(itemId).ItemInstances.Count < count)
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy more of an item than is in stock.");
                                break;
                            }


                            // Check we wont overflow the inventory
                            if (sender.LoggedinUser.Inventory.HasItemId(itemId)) 
                            {
                                InventoryItem items = sender.LoggedinUser.Inventory.GetItemByItemId(itemId);
                                if (items.ItemInstances.Count + count > ConfigReader.MAX_STACK)
                                {
                                    goto showError;
                                }

                            }
                            else if(sender.LoggedinUser.Inventory.Count + 1 > Messages.DefaultInventoryMax)
                            {
                                goto showError;
                            }

                            for (int i = 0; i < count; i++)
                            {
                                ItemInstance itemInstance = shop.Inventory.GetItemByItemId(itemId).ItemInstances[0];
                                try
                                {
                                    sender.LoggedinUser.Inventory.Add(itemInstance);
                                }
                                catch (InventoryException)
                                {
                                    Logger.ErrorPrint("Failed to add: " + itemInfo.Name + " to " + sender.LoggedinUser.Username + " inventory.");
                                    break;
                                }
                                shop.Inventory.Remove(itemInstance);
                            }

                            sender.LoggedinUser.Money -= buyCost;


                            // Send chat message to client.
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if (message == 1)
                            {
                                byte[] broughtItemMessage = PacketBuilder.CreateChat(Messages.FormatBuyMessage(itemInfo.Name, buyCost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(broughtItemMessage);
                            }
                            else if (message == 2)
                            {
                                byte[] broughtItemMessage = PacketBuilder.CreateChat(Messages.FormatBuy5Message(itemInfo.PluralName, buyCost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(broughtItemMessage);
                            }
                            else if (message == 3)
                            {
                                byte[] broughtItemMessage = PacketBuilder.CreateChat(Messages.FormatBuy25Message(itemInfo.PluralName, buyCost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(broughtItemMessage);
                            }
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy a item that was not for sale.");
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy an item while not in a store.");
                    }


                    break;

                showError:;
                    if (message == 1)
                    {
                        byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.Brought1ButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(inventoryFullMessage);
                    }
                    else if (message == 2)
                    {

                        byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.Brought5ButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(inventoryFullMessage);
                    }
                    else if (message == 3)
                    {

                        byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.Brought25ButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(inventoryFullMessage);
                    }
                    break;

                case PacketBuilder.PACKET_INFORMATION:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string valueStr = packetStr.Substring(3, packet.Length - 3);
                    int value = 0;
                    try
                    {
                        value = Int32.Parse(valueStr);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (packet[2] == PacketBuilder.ITEM_INFORMATON)
                    {
                        itemId = -1;
                        if (sender.LoggedinUser.Inventory.HasItem(value))
                            itemId = sender.LoggedinUser.Inventory.GetItemByRandomid(value).ItemId;
                        else if (DroppedItems.IsDroppedItemExist(value))
                            itemId = DroppedItems.GetDroppedItemById(value).instance.ItemId;
                        if (itemId == -1)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " asked for details of non existiant item.");
                            return;
                        }
                        sender.LoggedinUser.MetaPriority = true;
                        Item.ItemInformation info = Item.GetItemById(itemId);
                        string infoMessage = Meta.BuildItemInfo(info);
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(infoMessage);
                        sender.SendPacket(metaPacket);
                    }
                    if (packet[2] == PacketBuilder.ITEM_INFORMATON_ID)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        if (!Item.ItemIdExist(value))
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " asked for details of non existiant item.");
                            return;
                        }

                        Item.ItemInformation info = Item.GetItemById(value);
                        string infoMessage = Meta.BuildItemInfo(info);
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(infoMessage);
                        sender.SendPacket(metaPacket);
                    }
                    else if(packet[2] == PacketBuilder.NPC_INFORMATION)
                    {
                        if(Npc.NpcExists(value))
                        {
                            sender.LoggedinUser.MetaPriority = true;
                            Npc.NpcEntry npc = Npc.GetNpcById(value);
                            string infoMessage = Meta.BuildNpcInfo(npc);
                            byte[] metaPacket = PacketBuilder.CreateMetaPacket(infoMessage);
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " asked for details of non existiant npc.");
                            return;
                        }
                    }

                    break;
                default:
                    Logger.WarnPrint(sender.LoggedinUser.Username + " Sent an unknown Item Interaction Packet type: " + action.ToString() + ", Packet Dump: " + BitConverter.ToString(packet).Replace('-', ' '));
                    break;
            }

        }
        public static void OnInventoryRequested(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent chat packet when not logged in.");
                return;
            }

            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid inventory request packet.");
                return;
            }

            UpdateInventory(sender);
        }
        public static void OnLoginRequest(GameClient sender, byte[] packet)
        {
            Logger.DebugPrint("Login request received from: " + sender.RemoteIp);

            string loginRequestString = Encoding.UTF8.GetString(packet).Substring(1);

            if (!loginRequestString.Contains('|') || packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid login request");
                return;
            }

            if (packet[1] != PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                string[] loginParts = loginRequestString.Split('|');
                if (loginParts.Length < 3)
                {
                    Logger.ErrorPrint(sender.RemoteIp + " Sent a login request of invalid length. " + loginRequestString);
                    return;
                }

                int version = int.Parse(loginParts[0]);
                string encryptedUsername = loginParts[1];
                string encryptedPassword = loginParts[2];
                string username = Authentication.DecryptLogin(encryptedUsername);
                string password = Authentication.DecryptLogin(encryptedPassword);

                if (Authentication.CheckPassword(username, password))
                {
                    // Obtain user information
                    int userId = Database.GetUserid(username);
                    sender.Login(userId);
                    sender.LoggedinUser.Password = password;

                    byte[] ResponsePacket = PacketBuilder.CreateLoginPacket(true);
                    sender.SendPacket(ResponsePacket);

                    Logger.DebugPrint(sender.RemoteIp + " Logged into : " + sender.LoggedinUser.Username + " (ADMIN: " + sender.LoggedinUser.Administrator + " MOD: " + sender.LoggedinUser.Moderator + ")");

                    // Send login message
                    byte[] loginMessageBytes = PacketBuilder.CreateChat(Messages.FormatLoginMessage(sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
                    foreach (GameClient client in ConnectedClients)
                        if (client.LoggedIn)
                            if (!client.LoggedinUser.MuteLogins)
                                if (client.LoggedinUser.Id != userId)
                                        client.SendPacket(loginMessageBytes);

                    UpdateUserInfo(sender.LoggedinUser);

                }
                else
                {
                    Logger.WarnPrint(sender.RemoteIp + " Attempted to login to: " + username + " with incorrect password " + password);
                    byte[] ResponsePacket = PacketBuilder.CreateLoginPacket(false);
                    sender.SendPacket(ResponsePacket);
                }
            }

        }
        public static void OnDisconnect(GameClient sender)
        {
            connectedClients.Remove(sender);
            Logger.DebugPrint("owoo disconnect");
            if (sender.LoggedIn)
            {
                Database.RemoveOnlineUser(sender.LoggedinUser.Id);
                // Send disconnect message
                byte[] logoutMessageBytes = PacketBuilder.CreateChat(Messages.FormatLogoutMessage(sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
                foreach (GameClient client in ConnectedClients)
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteLogins)
                            if (client.LoggedinUser.Id != sender.LoggedinUser.Id)
                                client.SendPacket(logoutMessageBytes);
                // Tell clients of diconnect (remove from chat)
                byte[] playerRemovePacket = PacketBuilder.CreatePlayerLeavePacket(sender.LoggedinUser.Username);
                foreach (GameClient client in ConnectedClients)
                    if (client.LoggedIn)
                        if (client.LoggedinUser.Id != sender.LoggedinUser.Id)
                            client.SendPacket(playerRemovePacket);
            }

        }

        /*
         *  Get(Some Information)
         */

        public static User[] GetUsersUsersInIsle(World.Isle isle, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersInIsle = new List<User>();
            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteIsland)
                        continue;
                    if (World.InIsle(client.LoggedinUser.X, client.LoggedinUser.Y))
                        if (World.GetIsle(client.LoggedinUser.X, client.LoggedinUser.Y).Name == isle.Name)
                            usersInIsle.Add(client.LoggedinUser);
                }

            return usersInIsle.ToArray();
        }
        public static User[] GetUsersAt(int x, int y, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersHere = new List<User>();
            foreach(GameClient client in ConnectedClients)
            {
                if(client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteNear)
                        continue;
                    if (client.LoggedinUser.X == x && client.LoggedinUser.Y == y)
                        usersHere.Add(client.LoggedinUser);
                }
            }
            return usersHere.ToArray();
        }
        public static User[] GetNearbyUsers(int x, int y, bool includeStealth=false, bool includeMuted=false)
        {
            int startX = x - 15;
            int endX = x + 15;
            int startY = y - 19;
            int endY = y + 19;
            List<User> usersNearby = new List<User>();

            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteNear)
                        continue;
                    if (startX <= client.LoggedinUser.X && endX >= client.LoggedinUser.X && startY <= client.LoggedinUser.Y && endY >= client.LoggedinUser.Y)
                        usersNearby.Add(client.LoggedinUser);
                }

            return usersNearby.ToArray();
        }
        public static int GetNumberOfPlayers(bool includeStealth=false)
        {
            int count = 0;
            foreach(GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!client.LoggedinUser.Stealth)
                        count++;
                }
            
            return count;
        }
        public static int GetNumberOfModsOnline()
        {
            int count = 0;
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if(client.LoggedinUser.Moderator)
                        count++;
            }
            return count;
        }
        public static int GetNumberOfAdminsOnline()
        {
            int count = 0;
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.Administrator)
                        count++;
            }
            return count;
        }

        /*
         *  Update game state functions.
         */

        public static void Update(GameClient client)
        {
            UpdateArea(client);
            foreach (User nearbyUser in GameServer.GetNearbyUsers(client.LoggedinUser.X, client.LoggedinUser.Y, false, false))
                if (nearbyUser.Id != client.LoggedinUser.Id)
                    if(!nearbyUser.MetaPriority)
                        UpdateArea(nearbyUser.LoggedinClient);


            UpdateUserInfo(client.LoggedinUser);
        }
        public static void UpdateInventory(GameClient forClient)
        {
            if (!forClient.LoggedIn)
                return;
            forClient.LoggedinUser.MetaPriority = true;
            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildInventoryInfo(forClient.LoggedinUser.Inventory));
            forClient.SendPacket(metaPacket);
        }
        public static void UpdateWorld(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update world information when not logged in.");
                return;
            }

            byte[] WorldData = PacketBuilder.CreateWorldData(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, World.GetWeather());
            forClient.SendPacket(WorldData);
        }
        public static void UpdatePlayer(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update player information when not logged in.");
                return;
            }
            byte[] PlayerData = PacketBuilder.CreatePlayerData(forClient.LoggedinUser.Money, GameServer.GetNumberOfPlayers(), forClient.LoggedinUser.MailBox.MailCount);
            forClient.SendPacket(PlayerData);
        }
        public static void UpdateUserInfo(User user)
        {
            byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(user.X, user.Y, user.Facing, user.CharacterId, user.Username);



            List<User> users = new List<User>();
            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (client.LoggedinUser.Id != user.Id)
                        client.SendPacket(playerInfoBytes);
                }


        }
        public static void UpdateAreaForAll(int x, int y)
        {
            foreach(GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.X == x && client.LoggedinUser.Y == y)
                        UpdateArea(client);
            }
        }
        public static void UpdateArea(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update tile information when not logged in.");
                return;
            }

            string LocationStr = "";
            if (!World.InSpecialTile(forClient.LoggedinUser.X, forClient.LoggedinUser.Y))
            {
                LocationStr = Meta.BuildMetaInfo(forClient.LoggedinUser, forClient.LoggedinUser.X, forClient.LoggedinUser.Y);
            }
            else
            {
                World.SpecialTile specialTile = World.GetSpecialTile(forClient.LoggedinUser.X, forClient.LoggedinUser.Y);
                if (specialTile.AutoplaySwf != null && specialTile.AutoplaySwf != "")
                {
                    byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket(specialTile.AutoplaySwf,PacketBuilder.PACKET_SWF_MODULE_GENTLE);
                    forClient.SendPacket(swfModulePacket);
                }
                if (specialTile.Code != null)
                    if (!ProcessMapCodeWithArg(forClient, specialTile.Code))
                        return;
                LocationStr = Meta.BuildSpecialTileInfo(forClient.LoggedinUser, specialTile);
            }
            byte[] AreaMessage = PacketBuilder.CreateMetaPacket(LocationStr);
            forClient.SendPacket(AreaMessage);
            forClient.LoggedinUser.MetaPriority = false;

        }

        /*
         *   Other...
         */
        public static bool ProcessMapCodeWithArg(GameClient forClient, string mapCode)
        {
            if(mapCode.Contains('-'))
            {
                string[] codeInfo = mapCode.Split('-');
                string command = codeInfo[0];
                string paramaters = codeInfo[1];

                if(command == "JUMP")
                {
                    if(paramaters.Contains(','))
                    {
                        string[] args = paramaters.Split(',');
                        try
                        {
                            int newX = int.Parse(args[0]);
                            int newY = int.Parse(args[1]);
                            forClient.LoggedinUser.Teleport(newX, newY);
                            return false;
                        }
                        catch(Exception)
                        {
                            return true;
                        }
                    }
                }
            }
            return true;
        }
        public static void StartServer()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress hostIP = IPAddress.Parse(ConfigReader.BindIP);
            IPEndPoint ep = new IPEndPoint(hostIP, ConfigReader.Port);
            ServerSocket.Bind(ep);
            Logger.InfoPrint("Binding to ip: " + ConfigReader.BindIP + " On port: " + ConfigReader.Port.ToString());
            ServerSocket.Listen(10000);
            serverTimer = new Timer(new TimerCallback(onTick), null, gameTickSpeed, gameTickSpeed);
            while (true)
            {
                Logger.InfoPrint("Waiting for new connections...");

                Socket cientSocket = ServerSocket.Accept();
                GameClient client = new GameClient(cientSocket);
                connectedClients.Add(client);
            }
        }

        
        

    }
}
