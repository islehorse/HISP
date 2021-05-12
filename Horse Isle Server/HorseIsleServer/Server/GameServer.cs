using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Drawing;

using HISP.Player;
using HISP.Game;
using HISP.Security;
using HISP.Game.Chat;
using HISP.Player.Equips;
using HISP.Game.Services;
using HISP.Game.Inventory;
using HISP.Game.SwfModules;
using HISP.Game.Horse;
using HISP.Game.Events;
using HISP.Game.Items;

namespace HISP.Server
{
    public class GameServer
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

        // Events
        public static RealTimeRiddle RiddleEvent = RealTimeRiddle.GetRandomRiddle();
        public static TackShopGiveaway TackShopGiveawayEvent = null;
        public static RealTimeQuiz QuizEvent = null;
        public static WaterBalloonGame WaterBalloonEvent = new WaterBalloonGame();
        public static IsleCardTradingGame IsleCardTrading;
        public static ModsRevenge ModsRevengeEvent = new ModsRevenge();

        /*
         *  Private stuff 
         */
        private static int gameTickSpeed = 480; // Changing this to ANYTHING else will cause desync with the client.
        private static int totalMinutesElapsed = 0;
        private static int oneMinute = 1000 * 60;
        private static List<GameClient> connectedClients = new List<GameClient>();
        private static Timer gameTimer; // Controls in-game time.
        private static Timer minuteTimer; // ticks every real world minute.
        private static int lastServerTime = 0;
        private static void onGameTick(object state)
        {
            World.TickWorldClock();
            if(World.ServerTime.Minutes != lastServerTime)
            {
                Arena.StartArenas(World.ServerTime.Minutes);

                Database.DecHorseTrainTimeout();

                // write time to database:
                Database.SetServerTime(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years);

                // Ranch Windmill Payments
                if (World.ServerTime.Minutes % 720 == 0) // every 12 hours
                {
                    Logger.DebugPrint("Paying windmill owners . . . ");
                    foreach (Ranch ranch in Ranch.Ranches)
                    {
                        int ranchOwner = ranch.OwnerId;
                        if (ranchOwner != -1)
                        {
                            int moneyToAdd = 5000 * ranch.GetBuildingCount(8); // Windmill
                            if (GameServer.IsUserOnline(ranchOwner))
                                GameServer.GetUserById(ranchOwner).Money += moneyToAdd;
                            else
                                Database.SetPlayerMoney(Database.GetPlayerMoney(ranchOwner) + moneyToAdd, ranchOwner);
                        }
                    }
                }

                if((World.StartDate != -1)) // Birthday tokens
                {
                    int curTime = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    if (curTime >= World.StartDate + 378691200)
                    {
                        Logger.InfoPrint("Your server has been running for 12 years! Adding birthday tokens");
                        Database.SetStartTime(-1);
                        World.StartDate = -1;


                        AddItemToAllUsersEvenOffline(Item.BirthdayToken, 10);
                    }
 
                }

                gameTimer.Change(gameTickSpeed, gameTickSpeed);
                lastServerTime = World.ServerTime.Minutes;
            }

        }
        private static void onMinuteTick(object state)
        {
            totalMinutesElapsed++;

            if (totalMinutesElapsed % 8 == 0)
            {
                Database.IncAllUsersFreeTime(1);
            }

            if (totalMinutesElapsed % 25 == 0)
            {

                Logger.DebugPrint("Randomizing Weather...");
                foreach (World.Town town in World.Towns)
                {
                    if (RandomNumberGenerator.Next(0, 100) < 25)
                    {
                        town.Weather = town.SelectRandomWeather();
                    }
                }

                foreach (World.Isle isle in World.Isles)
                {
                    if (RandomNumberGenerator.Next(0, 100) < 25)
                    {
                        isle.Weather = isle.SelectRandomWeather();
                    }
                }
            }

            /*
             *  EVENTS 
             */

            // Mods Revenge
            if(totalMinutesElapsed % ((60*8)+15) == 0)
            {
                ModsRevengeEvent.StartEvent();
            }
            // Isle Card Trading Game
            if(totalMinutesElapsed % (60 *2) == 0)
            {
                IsleCardTrading = new IsleCardTradingGame();
                IsleCardTrading.StartEvent();
            }
            // Water Balloon Game
            if(totalMinutesElapsed % (60 * 2) == 0)
            {
                WaterBalloonEvent.StartEvent();
            }
            // Tack Shop Giveaway
            if(totalMinutesElapsed % ((60 * 3)+5) == 0)
            {
                TackShopGiveawayEvent = new TackShopGiveaway();
                TackShopGiveawayEvent.StartEvent();
            }
            // Real Time Riddle
            if(totalMinutesElapsed % (RealTimeRiddle.LastWon ? 20 : 15) == 0)
            {
                RiddleEvent = RealTimeRiddle.GetRandomRiddle();
                RiddleEvent.StartEvent();   
            }
            // Real Time Quiz
            if(totalMinutesElapsed % (60 + 30) == 0)
            {
                QuizEvent = new RealTimeQuiz();
                QuizEvent.StartEvent();
            }


            if (totalMinutesElapsed % 60 == 0) // Do spoils
            {
                foreach (HorseInstance horse in Database.GetMostSpoiledHorses())
                {
                    horse.BasicStats.Health = 1000;
                    horse.BasicStats.Mood = 1000;
                    horse.BasicStats.Hunger = 1000;
                    horse.BasicStats.Thirst = 1000;
                }
            }

            if (totalMinutesElapsed % 5 == 0)
            {
                Treasure.AddValue();
            }


            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (!client.LoggedinUser.MetaPriority)
                        Update(client);
                    byte[] BaseStatsPacketData = PacketBuilder.CreatePlayerData(client.LoggedinUser.Money, GameServer.GetNumberOfPlayers(), client.LoggedinUser.MailBox.UnreadMailCount);
                    client.SendPacket(BaseStatsPacketData);

                    UpdateWorld(client);
                    UpdatePlayer(client);
                }
            }

            foreach(Auction auction in Auction.AuctionRooms.ToArray())
            {
                foreach(Auction.AuctionEntry entry in auction.AuctionEntries.ToArray())
                {
                   entry.TimeRemaining--;
                   if (entry.Completed)
                        auction.DeleteEntry(entry);
                   else if (entry.TimeRemaining <= 0)
                        entry.Completed = true;

                    auction.UpdateAuctionRoom();
                }
            }



            Database.IncPlayerTirednessForOfflineUsers();

            // Offline player handling w sql black magic...

            Database.DecrementHorseLeaseTimeForOfflinePlayers();
            Database.TpOfflinePlayersBackToUniterForOfflinePlayers();
            Database.DeleteExpiredLeasedHorsesForOfflinePlayers();

            DroppedItems.DespawnItems();
            DroppedItems.GenerateItems();


            WildHorse.Update();
            Npc.WanderNpcs();
            minuteTimer.Change(oneMinute, oneMinute);
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

        public static void OnPlayerInteration(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested Bird Map when not logged in.");
                return;
            }
            byte method = packet[1];
            switch(method)
            {
                case PacketBuilder.PLAYER_INTERACTION_TRADE_REJECT:
                    if (sender.LoggedinUser.TradingWith != null)
                        sender.LoggedinUser.TradingWith.CancelTrade();
                    break;
                case PacketBuilder.PLAYER_INTERACTION_ACCEPT:
                    if (sender.LoggedinUser.TradingWith != null)
                        sender.LoggedinUser.TradingWith.AcceptTrade();
                    break;
                case PacketBuilder.PLAYER_INTERACTION_PROFILE:
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    int playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to view profile of User ID NaN.");
                        break;
                    }

                    if(IsUserOnline(playerId))
                    {
                        User user = GetUserById(playerId);
                        sender.LoggedinUser.MetaPriority = true;

                        byte[] metaTag = PacketBuilder.CreateMetaPacket(Meta.BuildStatsMenu(user, true));
                        sender.SendPacket(metaTag);
                    }
                    break;
                case PacketBuilder.PLAYER_INTERACTION_MUTE:
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to MUTE User ID NaN.");
                        break;
                    }

                    if (IsUserOnline(playerId))
                    {
                        User user = GetUserById(playerId);
                        if(!sender.LoggedinUser.MutePlayer.IsUserMuted(user))
                            sender.LoggedinUser.MutePlayer.MuteUser(user);

                        byte[] nowMuting = PacketBuilder.CreateChat(Messages.FormatNowMutingPlayer(user.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(nowMuting);

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerListMenu(sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case PacketBuilder.PLAYER_INTERACTION_UNMUTE:
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to UNMUTE User ID NaN.");
                        break;
                    }

                    if (IsUserOnline(playerId))
                    {
                        User user = GetUserById(playerId);
                        if (sender.LoggedinUser.MutePlayer.IsUserMuted(user))
                            sender.LoggedinUser.MutePlayer.UnmuteUser(user);

                        byte[] stoppedMuting = PacketBuilder.CreateChat(Messages.FormatStoppedMutingPlayer(user.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(stoppedMuting);

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerListMenu(sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case PacketBuilder.PLAYER_INTERACTION_REMOVE_BUDDY:
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to remove User ID NaN as a buddy.");
                        break;
                    }


                    if(sender.LoggedinUser.Friends.IsFriend(playerId))
                    {
                        sender.LoggedinUser.Friends.RemoveFriend(playerId);

                        byte[] friendRemoved = PacketBuilder.CreateChat(Messages.FormatAddBuddyRemoveBuddy(Database.GetUsername(playerId)), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(friendRemoved);

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerListMenu(sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                    }

                    break;
                case PacketBuilder.PLAYER_INTERACTION_TAG:
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to trade with User ID NaN.");
                        break;
                    }

                    if (IsUserOnline(playerId))
                    {
                        User user = GetUserById(playerId);;
                        string TAGYourIT = Messages.FormatTagYourIt(user.Username, sender.LoggedinUser.Username);
                        int totalBuds = 0;
                        foreach(int friendId in sender.LoggedinUser.Friends.List)
                        {
                            if (friendId == sender.LoggedinUser.Id)
                                continue;

                            if(IsUserOnline(friendId))
                            {
                                User buddy = GetUserById(friendId);
                                byte[] tagYourItPacket = PacketBuilder.CreateChat(TAGYourIT, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                buddy.LoggedinClient.SendPacket(tagYourItPacket);
                                totalBuds++;
                            }
                        }
                        string budStr = Messages.FormatTagTotalBuddies(totalBuds);

                        byte[] tagYouItPacket = PacketBuilder.CreateChat(TAGYourIT + budStr, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(tagYouItPacket);

                    }
                    break;
                case PacketBuilder.PLAYER_INTERACTION_ADD_BUDDY:
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to trade with User ID NaN.");
                        break;
                    }
                    if (IsUserOnline(playerId))
                    {
                        User userToAdd = GetUserById(playerId);
                        sender.LoggedinUser.Friends.AddFriend(userToAdd);
                    }
                    break;
                case PacketBuilder.PLAYER_INTERACTION_ADD_ITEM:
                    if (sender.LoggedinUser.TradingWith == null)
                        break;
                    if (packet.Length < 5)
                        break;

                    packetStr = Encoding.UTF8.GetString(packet);
                    string idStr = packetStr.Substring(2, packetStr.Length - 4);
                    char firstChar = idStr[0];
                    switch(firstChar)
                    {
                        case '3': // Money

                            if (sender.LoggedinUser.Bids.Count > 0)
                            {
                                byte[] cantBuyWhileAuctioning = PacketBuilder.CreateChat(Messages.AuctionNoOtherTransactionAllowed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cantBuyWhileAuctioning);
                                break;
                            }

                            sender.LoggedinUser.TradeMenuPriority = true;
                            sender.LoggedinUser.AttemptingToOfferItem = -1;
                            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTradeAddMoney(sender.LoggedinUser.TradingWith.MoneyOffered));
                            sender.SendPacket(metaPacket);

                            break;
                        case '2': // Horse
                            string horseRandomIdStr = idStr.Substring(1);
                            int horseRandomId = -1;
                            try
                            {
                                horseRandomId = int.Parse(horseRandomIdStr);
                            }
                            catch (FormatException)
                            {
                                break;
                            }

                            if (!sender.LoggedinUser.HorseInventory.HorseIdExist(horseRandomId))
                                break;

                            HorseInstance horse = sender.LoggedinUser.HorseInventory.GetHorseById(horseRandomId);
                            if(!sender.LoggedinUser.TradingWith.HorsesOffered.Contains(horse))
                                sender.LoggedinUser.TradingWith.HorsesOffered.Add(horse);

                            UpdateArea(sender);

                            if (sender.LoggedinUser.TradingWith != null)
                                if (!sender.LoggedinUser.TradingWith.OtherTrade.Trader.TradeMenuPriority)
                                    UpdateArea(sender.LoggedinUser.TradingWith.OtherTrade.Trader.LoggedinClient);

                            break;
                        case '1': // Item
                            string itemIdStr = idStr.Substring(1);
                            int itemId = -1;
                            try
                            {
                                itemId = int.Parse(itemIdStr);
                            }
                            catch(FormatException)
                            {
                                break;
                            }

                            if (!sender.LoggedinUser.Inventory.HasItemId(itemId))
                                break;

                            sender.LoggedinUser.TradeMenuPriority = true;
                            sender.LoggedinUser.AttemptingToOfferItem = itemId;
                            InventoryItem item = sender.LoggedinUser.Inventory.GetItemByItemId(itemId);
                            byte[] addItemPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTradeAddItem(item.ItemInstances.Count));
                            sender.SendPacket(addItemPacket);
                            break;

                    }
                    break;
                case PacketBuilder.PLAYER_INTERACTION_TRADE:
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch(FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to trade with User ID NaN.");
                        break;
                    }
                    if(IsUserOnline(playerId))
                    {
                        User user = GetUserById(playerId);
                        byte[] tradeMsg = PacketBuilder.CreateChat(Messages.TradeRequiresBothPlayersMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(tradeMsg);

                        sender.LoggedinUser.PendingTradeTo = user.Id;

                        if (user.PendingTradeTo == sender.LoggedinUser.Id)
                        {
                            // Start Trade
                            Trade tradeWithYou = new Trade(sender.LoggedinUser);
                            Trade tradeWithOther = new Trade(user);
                            tradeWithYou.OtherTrade = tradeWithOther;
                            tradeWithOther.OtherTrade = tradeWithYou;

                            sender.LoggedinUser.TradingWith = tradeWithYou;
                            user.TradingWith = tradeWithOther;

                            UpdateArea(sender);
                            UpdateArea(user.LoggedinClient);
                        }

                    }
                    break;
                default:
                    Logger.DebugPrint("Unknown Player interaction Method: 0x" + method.ToString("X") + " Packet: "+BitConverter.ToString(packet).Replace("-", " "));
                    break;
            }
            return;
        }
        public static void OnSocialPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Tried to be socialable, but has no account and therefor no friends.");
                return;
            }
            byte method = packet[1];

            switch (method)
            {
                case PacketBuilder.SOCIALS_MENU:
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    int playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to socialize with User ID NaN.");
                        break;
                    }

                    if(IsUserOnline(playerId))
                    {
                        sender.LoggedinUser.SocializingWith = GetUserById(playerId);
                        
                        sender.LoggedinUser.SocializingWith.BeingSocializedBy.Add(sender.LoggedinUser);
                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildSocialMenu(sender.LoggedinUser.CurrentlyRidingHorse != null));
                        sender.SendPacket(metaPacket);
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to socialize with User #"+playerId.ToString()+" but there not online.");
                    }
                    break;
                case PacketBuilder.SOCIALS_USE:
                    int socialId = Convert.ToInt32(packet[2] - (byte)0x21);
                    SocialType.Social social = SocialType.GetSocial(socialId);

                    if (sender.LoggedinUser.SocializingWith != null)
                    {

                        if (sender.LoggedinUser.SocializingWith.MuteAll || sender.LoggedinUser.SocializingWith.MuteSocials)
                        {
                            byte[] cantSocialize = PacketBuilder.CreateChat(Messages.PlayerIgnoringAllSocials, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantSocialize);
                            break;
                        }
                        else if (sender.LoggedinUser.SocializingWith.MutePlayer.IsUserMuted(sender.LoggedinUser))
                        {
                            byte[] cantSocialize = PacketBuilder.CreateChat(Messages.PlayerIgnoringYourSocials, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantSocialize);
                            break;
                        }
                    
                        if(sender.LoggedinUser.SocializingWith.X != sender.LoggedinUser.X && sender.LoggedinUser.SocializingWith.Y != sender.LoggedinUser.Y)
                        {
                            byte[] playerNotNearby = PacketBuilder.CreateChat(Messages.SocialPlayerNoLongerNearby, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(playerNotNearby);
                            break;
                        }
                    }
                    

                    foreach (User user in GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true))
                    {
                        if (social.BaseSocialType.Type != "GROUP")
                            if (user.Id == sender.LoggedinUser.SocializingWith.Id)
                                continue;

                        if (user.Id == sender.LoggedinUser.Id)
                            continue;

                        if (user.MuteAll || user.MuteSocials)
                            continue;
                        string socialTarget = "";
                        if(sender.LoggedinUser.SocializingWith != null)
                            socialTarget = sender.LoggedinUser.SocializingWith.Username;

                        byte[] msgEveryone = PacketBuilder.CreateChat(Messages.FormatSocialMessage(social.ForEveryone, socialTarget, sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        user.LoggedinClient.SendPacket(msgEveryone);
                    }
                    if(social.ForTarget != null)
                    {
                        if(sender.LoggedinUser.SocializingWith != null)
                        {
                            if (social.BaseSocialType.Type != "GROUP")
                            {
                                byte[] msgTarget = PacketBuilder.CreateChat(Messages.FormatSocialMessage(social.ForTarget, sender.LoggedinUser.SocializingWith.Username, sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.LoggedinUser.SocializingWith.LoggedinClient.SendPacket(msgTarget);
                            }
                        }
                    }
                    if(social.ForSender != null)
                    {
                        string socialTarget = "";
                        if (sender.LoggedinUser.SocializingWith != null)
                            socialTarget = sender.LoggedinUser.SocializingWith.Username;

                        byte[] msgSender = PacketBuilder.CreateChat(Messages.FormatSocialMessage(social.ForSender, socialTarget, sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(msgSender);

                        
                    }

                    foreach(User user in GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true))
                    {
                        if (social.SoundEffect != null)
                        {
                            if (user.MuteAll || user.MuteSocials)
                                continue;

                            byte[] soundEffect = PacketBuilder.CreatePlaysoundPacket(social.SoundEffect);
                            user.LoggedinClient.SendPacket(soundEffect);
                        }
                    }

                    break;
                default:
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " unknown social: " + method.ToString("X") + " packet dump: " + BitConverter.ToString(packet).Replace("-", " "));
                    break;
            }

        }
        public static void OnBirdMapRequested(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested Bird Map when not logged in.");
                return;
            }

            if(sender.LoggedinUser.Inventory.HasItemId(Item.Telescope))
            {
                byte[] birdMapPacket = PacketBuilder.CreateBirdMap(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                sender.SendPacket(birdMapPacket);
            }
            else
            {
                byte[] noTelescopeMessage = PacketBuilder.CreateChat(Messages.NoTelescope, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(noTelescopeMessage);
            }
        }

        public static void OnAuctionPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent auction packet when not logged in.");
                return;
            }
            if (packet.Length < 4)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid sized auction packet: " + BitConverter.ToString(packet).Replace("-", " "));
                return;
            }
            byte method = packet[1];
            int bidAmount = 0;
            switch (method)
            {
                case PacketBuilder.AUCTION_BID_100:
                    bidAmount = 100;
                    goto doBids;
                case PacketBuilder.AUCTION_BID_1K:
                    bidAmount = 1000;
                    goto doBids;
                case PacketBuilder.AUCTION_BID_10K:
                    bidAmount = 10000;
                    goto doBids;
                case PacketBuilder.AUCTION_BID_100K:
                    bidAmount = 100000;
                    goto doBids;
                case PacketBuilder.AUCTION_BID_1M:
                    bidAmount = 1000000;
                    goto doBids;
                case PacketBuilder.AUCTION_BID_10M:
                    bidAmount = 10000000;
                    goto doBids;
                case PacketBuilder.AUCITON_BID_100M:
                    bidAmount = 100000000;
                    doBids:;
                    if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if(tile.Code != null)
                        {
                            if(tile.Code.StartsWith("AUCTION-"))
                            {
                                Auction auctionRoom = Auction.GetAuctionRoomById(int.Parse(tile.Code.Split('-')[1]));
                                int auctionEntryId = -1;
                                string packetStr = Encoding.UTF8.GetString(packet);
                                string auctionEntryStr = packetStr.Substring(2, packetStr.Length - 4);
                                try
                                {
                                    auctionEntryId = int.Parse(auctionEntryStr);
                                }
                                catch(FormatException)
                                {
                                    Logger.ErrorPrint("Cant find auciton entry id NaN.");
                                    break;
                                }
                                if (!auctionRoom.HasAuctionEntry(auctionEntryId))
                                    break;
                                Auction.AuctionEntry entry = auctionRoom.GetAuctionEntry(auctionEntryId);
                                entry.Bid(sender.LoggedinUser, bidAmount);

                            }
                        }
                    }
                    break;
                default:
                    Logger.ErrorPrint("Unknown method id: 0x" + method.ToString("X"));
                    break;
            
            }

        }
        public static void OnHorseInteraction(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent horse interaction when not logged in.");
                return;
            }

            if(packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid sized horse interaction packet: " + BitConverter.ToString(packet).Replace("-", " "));
                return;
            }

            byte method = packet[1];
            switch(method)
            {
                case PacketBuilder.HORSE_LIST:
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] metaTags = PacketBuilder.CreateMetaPacket(Meta.BuildHorseInventory(sender.LoggedinUser));
                    sender.SendPacket(metaTags);
                    break;
                case PacketBuilder.HORSE_PROFILE:
                    byte methodProfileEdit = packet[2]; 
                    if(methodProfileEdit == PacketBuilder.HORSE_PROFILE_EDIT)
                    {
                        if (sender.LoggedinUser.LastViewedHorse != null)
                        {
                            sender.LoggedinUser.MetaPriority = true;
                            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseDescriptionEditMeta(sender.LoggedinUser.LastViewedHorse));
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + "Trying to edit description of no horse");
                        }
                    }
                    else
                    {
                        Logger.InfoPrint(BitConverter.ToString(packet).Replace("-", " "));
                    }
                    break;
                case PacketBuilder.HORSE_FEED:
                    int randomId = 0;
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseFeedInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);

                        sender.LoggedinUser.LastViewedHorse = horseFeedInst;
                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseFeedMenu(horseFeedInst, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to feed at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_PET:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horsePetInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = horsePetInst;
                        int randMoodAddition = RandomNumberGenerator.Next(1, 20);
                        int randTiredMinus = RandomNumberGenerator.Next(1, 10);



                        string msgs = "";
                        if (horsePetInst.BasicStats.Mood + randMoodAddition >= 1000)
                            msgs += Messages.HorsePetTooHappy;
                        if (horsePetInst.BasicStats.Tiredness - randTiredMinus <= 0)
                            msgs += Messages.HorsePetTooTired;

                        horsePetInst.BasicStats.Tiredness -= randTiredMinus;
                        horsePetInst.BasicStats.Mood += randMoodAddition;

                        byte[] petMessagePacket = PacketBuilder.CreateChat(Messages.FormatHorsePetMessage(msgs,randMoodAddition, randTiredMinus), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(petMessagePacket);

                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to feed at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_VET_SERVICE_ALL:

                    if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (tile.Code.StartsWith("VET-"))
                        {
                            string[] vetInfo = tile.Code.Split('-');
                            int vetId = int.Parse(vetInfo[1]);
                            Vet vet = Vet.GetVetById(vetId);
                            int price = 0;

                            foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                                price += vet.CalculatePrice(horse.BasicStats.Health);
                            if(price == 0)
                            {
                                byte[] notNeededMessagePacket = PacketBuilder.CreateChat(Messages.VetServicesNotNeededAll, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(notNeededMessagePacket);
                                break;
                            }
                            else if (sender.LoggedinUser.Money >= price)
                            {
                                foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                                    horse.BasicStats.Health = 1000;

                                byte[] healedMessagePacket = PacketBuilder.CreateChat(Messages.VetAllFullHealthRecoveredMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(healedMessagePacket);

                                sender.LoggedinUser.Money -= price;

                            }
                            else
                            {
                                byte[] cannotAffordMessagePacket = PacketBuilder.CreateChat(Messages.VetCannotAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cannotAffordMessagePacket);
                                break;
                            }
                            UpdateArea(sender);
                        }
                    }
                    break;
                case PacketBuilder.HORSE_VET_SERVICE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);

                    if (randomIdStr == "NaN")
                        break;
                        
                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseVetServiceInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = horseVetServiceInst;

                        if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if(tile.Code.StartsWith("VET-"))
                            {
                                string[] vetInfo = tile.Code.Split('-');
                                int vetId = int.Parse(vetInfo[1]);

                                Vet vet = Vet.GetVetById(vetId);
                                int price = vet.CalculatePrice(horseVetServiceInst.BasicStats.Health);
                                if(sender.LoggedinUser.Money >= price)
                                {
                                    horseVetServiceInst.BasicStats.Health = 1000;
                                    sender.LoggedinUser.Money -= price;

                                    byte[] messagePacket = PacketBuilder.CreateChat(Messages.FormatVetHorseAtFullHealthMessage(horseVetServiceInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(messagePacket);
                                }
                                else
                                {
                                    byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.VetCannotAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantAffordMessage);
                                    break;
                                }
                                UpdateArea(sender);
                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use vet services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_SHOE_STEEL:
                case PacketBuilder.HORSE_SHOE_IRON:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseFarrierServiceInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = horseFarrierServiceInst;

                        if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if (tile.Code.StartsWith("FARRIER-"))
                            {
                                string[] farrierInfo = tile.Code.Split('-');
                                int farrierId = int.Parse(farrierInfo[1]);

                                Farrier farrier = Farrier.GetFarrierById(farrierId);
                                int price = 0;
                                int incAmount = 0;
                                string msg = "";

                                if(method == PacketBuilder.HORSE_SHOE_STEEL)
                                {
                                    price = farrier.SteelCost;
                                    incAmount = farrier.SteelShoesAmount;
                                    msg = Messages.FormatFarrierPutOnSteelShoesMessage(incAmount, 1000);
                                }
                                else
                                {
                                    price = farrier.IronCost;
                                    incAmount = farrier.IronShoesAmount;
                                    msg = Messages.FormatFarrierPutOnIronShoesMessage(incAmount, 1000);
                                }

                                if (sender.LoggedinUser.Money >= price)
                                {
                                    horseFarrierServiceInst.BasicStats.Shoes = incAmount;
                                    sender.LoggedinUser.Money -= price;

                                    byte[] messagePacket = PacketBuilder.CreateChat(msg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(messagePacket);
                                }
                                else
                                {
                                    byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.FarrierShoesCantAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantAffordMessage);
                                    break;
                                }
                                UpdateArea(sender);
                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use farrier services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_SHOE_ALL:
                    if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (tile.Code.StartsWith("FARRIER-"))
                        {
                            string[] farrierInfo = tile.Code.Split('-');
                            int farrierId = int.Parse(farrierInfo[1]);

                            Farrier farrier = Farrier.GetFarrierById(farrierId);

                            int totalPrice = 0;
                            foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                            {
                                if (horse.BasicStats.Shoes < farrier.SteelShoesAmount)
                                {
                                    totalPrice += farrier.SteelCost;
                                }
                            }

                            if (sender.LoggedinUser.Money >= totalPrice)
                            {
                                foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                                {
                                    if (horse.BasicStats.Shoes < farrier.SteelShoesAmount)
                                    {
                                        horse.BasicStats.Shoes = farrier.SteelShoesAmount;
                                    }
                                }
                                sender.LoggedinUser.Money -= totalPrice;

                                byte[] messagePacket = PacketBuilder.CreateChat(Messages.FormatFarrierPutOnSteelShoesAllMesssage(farrier.SteelShoesAmount, 1000), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(messagePacket);
                            }
                            else
                            {
                                byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.FarrierShoesCantAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cantAffordMessage);
                                break;
                            }
                            UpdateArea(sender);
                        }
                    }
                    break;
                case PacketBuilder.HORSE_GROOM_SERVICE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance groomHorseInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = groomHorseInst;

                        if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if (tile.Code.StartsWith("GROOMER-"))
                            {
                                string[] groomerInfo = tile.Code.Split('-');
                                int groomerId = int.Parse(groomerInfo[1]);

                                Groomer groomer = Groomer.GetGroomerById(groomerId);
                                int price = groomer.CalculatePrice(groomHorseInst.BasicStats.Groom);


                                if (sender.LoggedinUser.Money >= price)
                                {
                                    groomHorseInst.BasicStats.Groom = groomer.Max;
                                    sender.LoggedinUser.Money -= price;

                                    byte[] messagePacket = PacketBuilder.CreateChat(Messages.FormatHorseGroomedToBestAbilities(groomHorseInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(messagePacket);
                                }
                                else
                                {
                                    byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.GroomerCannotAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantAffordMessage);
                                    break;
                                }
                                UpdateArea(sender);
                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use groomer services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_GROOM_SERVICE_ALL:
                    if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (tile.Code.StartsWith("GROOMER-"))
                        {
                            string[] groomerInfo = tile.Code.Split('-');
                            int groomId = int.Parse(groomerInfo[1]);
                            Groomer groomer = Groomer.GetGroomerById(groomId);
                            int price = 0;
                            int count = 0;

                            foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                            {
                                if(horse.BasicStats.Groom < groomer.Max)
                                {
                                    price += groomer.CalculatePrice(horse.BasicStats.Groom);
                                    count++;
                                }
                            }
                            if (count == 0)
                            {
                                byte[] notNeededMessagePacket = PacketBuilder.CreateChat(Messages.GroomerDontNeed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(notNeededMessagePacket);
                                break;
                            }
                            else if (sender.LoggedinUser.Money >= price)
                            {
                                foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                                    if (horse.BasicStats.Groom < groomer.Max)
                                        horse.BasicStats.Groom = groomer.Max;

                                byte[] groomedAllHorsesPacket = PacketBuilder.CreateChat(Messages.GroomerBestToHisAbilitiesALL, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(groomedAllHorsesPacket);

                                sender.LoggedinUser.Money -= price;

                            }
                            else
                            {
                                byte[] cannotAffordMessagePacket = PacketBuilder.CreateChat(Messages.GroomerCannotAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cannotAffordMessagePacket);
                                break;
                            }
                            UpdateArea(sender);
                        }
                    }
                    break;
                case PacketBuilder.HORSE_BARN_SERVICE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance barnHorseInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = barnHorseInst;

                        if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if (tile.Code.StartsWith("BARN-"))
                            {
                                string[] barnInfo = tile.Code.Split('-');
                                int barnId = int.Parse(barnInfo[1]);

                                Barn barn = Barn.GetBarnById(barnId);
                                int price = barn.CalculatePrice(barnHorseInst.BasicStats.Tiredness, barnHorseInst.BasicStats.Hunger, barnHorseInst.BasicStats.Thirst); ;


                                if (sender.LoggedinUser.Money >= price)
                                {
                                    barnHorseInst.BasicStats.Tiredness = 1000;
                                    barnHorseInst.BasicStats.Hunger = 1000;
                                    barnHorseInst.BasicStats.Thirst = 1000;
                                    sender.LoggedinUser.Money -= price;

                                    byte[] messagePacket = PacketBuilder.CreateChat(Messages.FormatBarnHorseFullyFed(barnHorseInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(messagePacket);
                                }
                                else
                                {
                                    byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.BarnCantAffordService, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantAffordMessage);
                                    break;
                                }
                                UpdateArea(sender);
                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use groomer services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_BARN_SERVICE_ALL:
                    if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (tile.Code.StartsWith("BARN-"))
                        {
                            string[] barnInfo = tile.Code.Split('-');
                            int barnId = int.Parse(barnInfo[1]);
                            Barn barn = Barn.GetBarnById(barnId);
                            int totalPrice = 0;

                            foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                            {
                                int price = barn.CalculatePrice(horse.BasicStats.Tiredness, horse.BasicStats.Hunger, horse.BasicStats.Thirst);
                                if (price > 0)
                                    totalPrice += price;
                            }
                            if (totalPrice == 0)
                            {
                                byte[] notNeededMessagePacket = PacketBuilder.CreateChat(Messages.BarnServiceNotNeeded, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(notNeededMessagePacket);
                                break;
                            }
                            else if (sender.LoggedinUser.Money >= totalPrice)
                            {
                                foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                                {
                                    horse.BasicStats.Tiredness = 1000;
                                    horse.BasicStats.Thirst = 1000;
                                    horse.BasicStats.Hunger = 1000;
                                }

                                byte[] barnedAllHorsesPacket = PacketBuilder.CreateChat(Messages.BarnAllHorsesFullyFed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(barnedAllHorsesPacket);

                                sender.LoggedinUser.Money -= totalPrice;

                            }
                            else
                            {
                                byte[] cannotAffordMessagePacket = PacketBuilder.CreateChat(Messages.BarnCantAffordService, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cannotAffordMessagePacket);
                                break;
                            }
                            UpdateArea(sender);
                        }
                    }
                    break;
                case PacketBuilder.HORSE_TRAIN:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance trainHorseInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = trainHorseInst;

                        if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if (tile.Code.StartsWith("TRAINER-"))
                            {
                                if(trainHorseInst.TrainTimer > 0)
                                {
                                    byte[] trainSuccessfulMessage = PacketBuilder.CreateChat(Messages.FormatTrainerCantTrainAgainIn(trainHorseInst.TrainTimer), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(trainSuccessfulMessage);
                                    break;
                                }
                                string[] trainerInfo = tile.Code.Split('-');
                                int trainerId = int.Parse(trainerInfo[1]);

                                Trainer trainer = Trainer.GetTrainerById(trainerId);

                                if(sender.LoggedinUser.Money >= trainer.MoneyCost)
                                {
                                    sender.LoggedinUser.Money -= trainer.MoneyCost;
                                    trainHorseInst.BasicStats.Mood -= trainer.MoodCost;
                                    trainHorseInst.BasicStats.Thirst -= trainer.ThirstCost;
                                    trainHorseInst.BasicStats.Hunger -= trainer.HungerCost;


                                    switch (trainer.ImprovesStat.ToUpper())
                                    {
                                        case "SPEED":
                                            trainHorseInst.AdvancedStats.Speed += trainer.ImprovesAmount;
                                            break;
                                        case "STRENGTH":
                                            trainHorseInst.AdvancedStats.Strength += trainer.ImprovesAmount;
                                            break;
                                        case "AGILITY":
                                            trainHorseInst.AdvancedStats.Agility += trainer.ImprovesAmount;
                                            break;
                                        case "ENDURANCE":
                                            trainHorseInst.AdvancedStats.Endurance += trainer.ImprovesAmount;
                                            break;
                                        case "CONFORMATION":
                                            trainHorseInst.AdvancedStats.Conformation += trainer.ImprovesAmount;
                                            break;
                                        default:
                                            trainHorseInst.AdvancedStats.Speed += trainer.ImprovesAmount;
                                            break;
                                    }
                                    trainHorseInst.BasicStats.Experience += trainer.ExperienceGained;
                                    if(sender.LoggedinUser.Subscribed)
                                        trainHorseInst.TrainTimer = 1440;
                                    else
                                        trainHorseInst.TrainTimer = 720;

                                    byte[] trainSuccessfulMessage = PacketBuilder.CreateChat(Messages.FormatTrainedInStatFormat(trainHorseInst.Name, trainer.ImprovesStat), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(trainSuccessfulMessage);


                                    sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Training).Count++;

                                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Training).Count >= 1000)
                                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(26)); // Pro Trainer
                                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Training).Count >= 10000)
                                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(53)); // Top Trainer

                                    UpdateArea(sender);
                                }
                                else
                                {
                                    byte[] cantAffordPacket = PacketBuilder.CreateChat(Messages.TrainerCantAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantAffordPacket);
                                }

                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use trauber services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_GIVE_FEED:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if(sender.LoggedinUser.LastViewedHorse == null)
                    {
                        Logger.InfoPrint(sender.LoggedinUser.Username + " Tried to feed a non existant horse.");
                        break;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem item = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        Item.ItemInformation itemInfo = item.ItemInstances[0].GetItemInfo();
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        bool tooMuch = false;
                        bool changePersonality = false;
                        bool changeInteligence = false;
                        
                        foreach(Item.Effects effect in itemInfo.Effects)
                        {
                            switch(effect.EffectsWhat)
                            {
                                case "HEALTH":
                                    if (horseInstance.BasicStats.Health + effect.EffectAmount > 1000)
                                        tooMuch = true;
                                    horseInstance.BasicStats.Health += effect.EffectAmount;
                                    break;
                                case "HUNGER":
                                    if (horseInstance.BasicStats.Hunger + effect.EffectAmount > 1000)
                                        tooMuch = true;
                                    horseInstance.BasicStats.Hunger += effect.EffectAmount;
                                    break;
                                case "MOOD":
                                    if (horseInstance.BasicStats.Mood + effect.EffectAmount > 1000)
                                        tooMuch = true;
                                    horseInstance.BasicStats.Mood += effect.EffectAmount;
                                    break;
                                case "GROOM":
                                    if (horseInstance.BasicStats.Groom + effect.EffectAmount > 1000)
                                        tooMuch = true;
                                    horseInstance.BasicStats.Groom += effect.EffectAmount;
                                    break;
                                case "SHOES":
                                    if (horseInstance.BasicStats.Shoes + effect.EffectAmount > 1000)
                                        tooMuch = true;
                                    horseInstance.BasicStats.Shoes += effect.EffectAmount;
                                    break;
                                case "THIRST":
                                    if (horseInstance.BasicStats.Thirst + effect.EffectAmount > 1000)
                                        tooMuch = true;
                                    horseInstance.BasicStats.Thirst += effect.EffectAmount;
                                    break;
                                case "TIREDNESS":
                                    if (horseInstance.BasicStats.Tiredness + effect.EffectAmount > 1000)
                                        tooMuch = true;
                                    horseInstance.BasicStats.Tiredness += effect.EffectAmount;
                                    break;
                                case "INTELLIGENCEOFFSET":
                                    horseInstance.AdvancedStats.Inteligence += effect.EffectAmount;
                                    changeInteligence = true;
                                    break;
                                case "PERSONALITYOFFSET":
                                    horseInstance.AdvancedStats.Personality += effect.EffectAmount;
                                    changePersonality = true;
                                    break;
                                case "SPOILED":
                                    horseInstance.Spoiled += effect.EffectAmount;
                                    break;
                            }
                        }
                        sender.LoggedinUser.Inventory.Remove(item.ItemInstances[0]);

                        if(changePersonality)
                        {
                            byte[] personalityIncreased = PacketBuilder.CreateChat(Messages.HorseFeedPersonalityIncreased, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(personalityIncreased);
                        }
                        if (changeInteligence)
                        {
                            byte[] inteligenceIncreased = PacketBuilder.CreateChat(Messages.HorseFeedInteligenceIncreased, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(inteligenceIncreased);
                        }

                        if(item.ItemId == Item.MagicDroplet)
                        {
                            string oldColor = horseInstance.Color;
                            string newColor = horseInstance.Breed.Colors[RandomNumberGenerator.Next(0, horseInstance.Breed.Colors.Length)];

                            horseInstance.Color = newColor;
                            horseInstance.MagicUsed++;

                            byte[] magicDropletUsed = PacketBuilder.CreateChat(Messages.FormatHorseFeedMagicDropletUsed(oldColor, newColor), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(magicDropletUsed);
                        }

                        if(item.ItemId == Item.MagicBean)
                        {
                            double oldH = HorseInfo.CalculateHands(horseInstance.AdvancedStats.Height, false);
                            int newHeight = RandomNumberGenerator.Next(horseInstance.Breed.BaseStats.MinHeight, horseInstance.Breed.BaseStats.MaxHeight);
                            double newH = HorseInfo.CalculateHands(newHeight, false);

                            horseInstance.AdvancedStats.Height = newHeight;
                            horseInstance.MagicUsed++;

                            byte[] magicBeansUsed = PacketBuilder.CreateChat(Messages.FormatHorseFeedMagicBeanUsed(oldH, newH), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(magicBeansUsed);
                        }

                        byte[] horseNeighThanksPacket = PacketBuilder.CreateChat(Messages.HorseNeighsThanks, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(horseNeighThanksPacket);

                        if (tooMuch)
                        {
                            byte[] horseCouldntFinishItAll = PacketBuilder.CreateChat(Messages.HorseCouldNotFinish, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(horseCouldntFinishItAll);
                        }

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseFeedMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                            
                        
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to feed a non existant item to a horse.");
                        break;
                    }
                case PacketBuilder.HORSE_ENTER_ARENA:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseInstance = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if (tile.Code.StartsWith("ARENA-"))
                            {
                                string[] arenaInfo = tile.Code.Split('-');
                                int arenaId = int.Parse(arenaInfo[1]);
                                Arena arena = Arena.GetAreaById(arenaId);
                                if (!Arena.UserHasEnteredHorseInAnyArena(sender.LoggedinUser))
                                {
                                    if (horseInstance.BasicStats.Thirst <= 300)
                                    {
                                        byte[] tooThirsty = PacketBuilder.CreateChat(Messages.ArenaTooThirsty, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(tooThirsty);
                                        break;
                                    }
                                    else if (horseInstance.BasicStats.Hunger <= 300)
                                    {
                                        byte[] tooHungry = PacketBuilder.CreateChat(Messages.ArenaTooHungry, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(tooHungry);
                                        break;
                                    }
                                    else if (horseInstance.BasicStats.Shoes <= 300)
                                    {
                                        byte[] needsFarrier = PacketBuilder.CreateChat(Messages.ArenaNeedsFarrier, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(needsFarrier);
                                        break;
                                    }
                                    else if (horseInstance.BasicStats.Tiredness <= 300)
                                    {
                                        byte[] tooTired = PacketBuilder.CreateChat(Messages.ArenaTooTired, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(tooTired);
                                        break;
                                    }
                                    else if (horseInstance.BasicStats.Health <= 300)
                                    {
                                        byte[] needsVet = PacketBuilder.CreateChat(Messages.ArenaNeedsVet, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(needsVet);
                                        break;
                                    }



                                    if (sender.LoggedinUser.Money >= arena.EntryCost)
                                    {
                                        arena.AddEntry(sender.LoggedinUser, horseInstance);
                                        sender.LoggedinUser.Money -= arena.EntryCost;

                                        byte[] enteredIntoCompetition = PacketBuilder.CreateChat(Messages.ArenaEnteredInto, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(enteredIntoCompetition);
                                        UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                                        break;
                                    }
                                    else
                                    {
                                        byte[] cantAffordEntryFee = PacketBuilder.CreateChat(Messages.ArenaCantAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(cantAffordEntryFee);
                                        break;
                                    }
                                }
                                else
                                {
                                    byte[] allreadyEntered = PacketBuilder.CreateChat(Messages.ArenaAlreadyEntered, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(allreadyEntered);
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to enter a non existant horse into a competition.");
                        break;
                    }
                    break;
                case PacketBuilder.HORSE_RELEASE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        if(World.InTown(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to reelease a horse while inside a town....");
                            break;
                        }


                        HorseInstance horseReleaseInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        if(sender.LoggedinUser.CurrentlyRidingHorse != null)
                        {
                            if(horseReleaseInst.RandomId == sender.LoggedinUser.CurrentlyRidingHorse.RandomId) 
                            {
                                byte[] errorChatPacket = PacketBuilder.CreateChat(Messages.HorseCantReleaseTheHorseYourRidingOn, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(errorChatPacket);
                                break;
                            }

                        }

                        if (horseReleaseInst.Description == "")
                            horseReleaseInst.Description += Messages.FormatHorseReleasedBy(sender.LoggedinUser.Username);

                        Logger.InfoPrint(sender.LoggedinUser.Username + " RELEASED HORSE: " + horseReleaseInst.Name + " (a " + horseReleaseInst.Breed.Name + ").");

                        sender.LoggedinUser.HorseInventory.DeleteHorse(horseReleaseInst);
                        new WildHorse(horseReleaseInst, sender.LoggedinUser.X, sender.LoggedinUser.Y, 60, true);
                        
                        sender.LoggedinUser.LastViewedHorse = horseReleaseInst;
                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseReleased());
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to release at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_TACK:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseTackInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        
                        sender.LoggedinUser.LastViewedHorse = horseTackInst;
                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackMenu(horseTackInst, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to tack at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_DRINK:
                    if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if(tile.Code != "POND")
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to drink from a pond when not on one.");
                            break;
                        }
                    }

                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseDrinkInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);

                        if(horseDrinkInst.BasicStats.Health < 200)
                        {
                            byte[] hpToLow = PacketBuilder.CreateChat(Messages.FormatPondHpLowMessage(horseDrinkInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(hpToLow);
                            break;
                        }

                        if(horseDrinkInst.BasicStats.Thirst < 1000)
                        {
                            horseDrinkInst.BasicStats.Thirst = 1000;
                            byte[] drinkFull = PacketBuilder.CreateChat(Messages.FormatPondDrinkFull(horseDrinkInst.Name),PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(drinkFull);

                            if(RandomNumberGenerator.Next(0, 100) < 25)
                            {
                                horseDrinkInst.BasicStats.Health -= 200;
                                byte[] ohNoes = PacketBuilder.CreateChat(Messages.FormatPondDrinkOhNoes(horseDrinkInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(ohNoes);
                            }

                            UpdateArea(sender);
                        }
                        else
                        {
                            byte[] notThirsty = PacketBuilder.CreateChat(Messages.FormatPondNotThirsty(horseDrinkInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notThirsty);
                            break;
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to tack at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_TACK_EQUIP:

                    int itemId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    string itemIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        itemId = int.Parse(itemIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if(Item.ItemIdExist(itemId))
                    {
                        if(sender.LoggedinUser.LastViewedHorse != null)
                        {
                            if(sender.LoggedinUser.LastViewedHorse.AutoSell > 0)
                            {
                                byte[] failMessagePacket = PacketBuilder.CreateChat(Messages.HorseTackFailAutoSell, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(failMessagePacket);
                                break;
                            }

                            if(sender.LoggedinUser.Inventory.HasItemId(itemId))
                            {
                                Item.ItemInformation itemInfo = Item.GetItemById(itemId);
                                if (itemInfo.Type == "TACK")
                                {
                                    switch (itemInfo.GetMiscFlag(0))
                                    {
                                        case 1: // Saddle
                                            if(sender.LoggedinUser.LastViewedHorse.Equipment.Saddle != null)
                                                sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Saddle.Id));
                                            Database.SetSaddle(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.LoggedinUser.LastViewedHorse.Equipment.Saddle = itemInfo;
                                            break;
                                        case 2: // Saddle Pad
                                            if (sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad != null)
                                                sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad.Id));
                                            Database.SetSaddlePad(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad = itemInfo;
                                            break;
                                        case 3: // Bridle
                                            if (sender.LoggedinUser.LastViewedHorse.Equipment.Bridle != null)
                                                sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Bridle.Id));
                                            Database.SetBridle(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.LoggedinUser.LastViewedHorse.Equipment.Bridle = itemInfo;
                                            break;
                                    }


                                    sender.LoggedinUser.Inventory.Remove(sender.LoggedinUser.Inventory.GetItemByItemId(itemId).ItemInstances[0]); // Remove item from inventory.

                                    sender.LoggedinUser.MetaPriority = true;
                                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                                    sender.SendPacket(metaPacket);

                                    byte[] equipMsgPacket = PacketBuilder.CreateChat(Messages.FormatEquipTackMessage(itemInfo.Name, sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(equipMsgPacket);

                                }
                                else if(itemInfo.Type == "COMPANION")
                                {
                                    if (sender.LoggedinUser.LastViewedHorse.Equipment.Companion != null)
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Companion.Id));
                                    Database.SetCompanion(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                    sender.LoggedinUser.LastViewedHorse.Equipment.Companion = itemInfo;

                                    sender.LoggedinUser.Inventory.Remove(sender.LoggedinUser.Inventory.GetItemByItemId(itemId).ItemInstances[0]); // Remove item from inventory.

                                    sender.LoggedinUser.MetaPriority = true;
                                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCompanionEquipMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                                    sender.SendPacket(metaPacket);

                                    byte[] equipMsgPacket = PacketBuilder.CreateChat(Messages.FormatHorseCompanionEquipMessage(sender.LoggedinUser.LastViewedHorse.Name, itemInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(equipMsgPacket);
                                }
                                else
                                {
                                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to equip a tack item to a hrose but that item was not of type \"TACK\".");
                                }
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " tried to equip tack he doesnt have");
                                break;
                            }
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to equip tack to a horse when not viewing one.");
                            break;
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " tried to equip tack he doesnt exist");
                        break;
                    }

                    break;
                case PacketBuilder.HORSE_TACK_UNEQUIP:
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        byte equipSlot = packet[2];
                        switch(equipSlot)
                        {
                            case 0x31: // Saddle
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.Saddle != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Saddle.Id));
                                Database.ClearSaddle(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.Saddle = null;
                                break;
                            case 0x32: // Saddle Pad
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad.Id));
                                Database.ClearSaddlePad(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad = null;
                                break;
                            case 0x33: // Bridle
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.Bridle != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Bridle.Id));
                                Database.ClearBridle(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.Bridle = null;
                                break;
                            case 0x34: // Companion
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.Companion != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Companion.Id));
                                Database.ClearCompanion(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.Companion = null;
                                goto companionRemove;
                            default:
                                Logger.ErrorPrint("Unknown equip slot: " + equipSlot.ToString("X"));
                                break;
                        }
                        byte[] itemUnequipedMessage = PacketBuilder.CreateChat(Messages.FormatUnEquipTackMessage(sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemUnequipedMessage);

                        if(sender.LoggedinUser.CurrentlyRidingHorse != null)
                        {
                            if(sender.LoggedinUser.CurrentlyRidingHorse.RandomId == sender.LoggedinUser.LastViewedHorse.RandomId)
                            {
                                byte[] disMounted = PacketBuilder.CreateChat(Messages.FormatHorseDismountedBecauseTackedMessage(sender.LoggedinUser.CurrentlyRidingHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.LoggedinUser.Facing %= 5;
                                sender.LoggedinUser.CurrentlyRidingHorse = null;
                                sender.SendPacket(disMounted);
                            }
                        }

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;
                    companionRemove:;
                        itemUnequipedMessage = PacketBuilder.CreateChat(Messages.FormatHorseCompanionRemoveMessage(sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemUnequipedMessage);

                        sender.LoggedinUser.MetaPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCompanionEquipMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to unequip items from non existnat horse");
                    }
                    break;
                case PacketBuilder.HORSE_DISMOUNT:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);

                    if(randomIdStr == "") // F7 Shortcut
                    { 
                        if(sender.LoggedinUser.CurrentlyRidingHorse != null)
                        {
                            StopRidingHorse(sender);
                        }
                        else
                        {
                            if(sender.LoggedinUser.HorseInventory.HorseIdExist(sender.LoggedinUser.LastRiddenHorse))
                               StartRidingHorse(sender, sender.LoggedinUser.LastRiddenHorse);
                        }
                        break;
                    }

                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        StopRidingHorse(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to dismount at a non existant horse.");
                        break;
                    }
                    break;
                case PacketBuilder.HORSE_MOUNT:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        StartRidingHorse(sender, randomId);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to mount at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_LOOK:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    HorseInstance horseInst;
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        return;
                    }
                    if(sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        horseInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        UpdateHorseMenu(sender, horseInst);
                    }
                    else
                    {
                        try
                        { // Not your horse? possibly viewed inside a ranch?
                            horseInst = Database.GetPlayerHorse(randomId);
                            UpdateHorseMenu(sender, horseInst);
                            break;
                        }
                        catch(Exception)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to look at a non existant horse.");
                            break;
                        }
                    }

                    break;
                case PacketBuilder.HORSE_ESCAPE:
                    if(WildHorse.DoesHorseExist(sender.LoggedinUser.CapturingHorseId))
                    {
                        WildHorse capturing = WildHorse.GetHorseById(sender.LoggedinUser.CapturingHorseId);
                        sender.LoggedinUser.CapturingHorseId = 0;

                        if (capturing.X == sender.LoggedinUser.X && capturing.Y == sender.LoggedinUser.Y)
                        {
                            capturing.Escape();
                            Logger.InfoPrint(sender.LoggedinUser.Username + " Failed to capture: " + capturing.Instance.Breed.Name + " new location: " + capturing.X + ", " + capturing.Y);

                        }
                    }
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] hoseEscaped = PacketBuilder.CreateMetaPacket(Meta.BuildHorseEscapedMessage());
                    sender.SendPacket(hoseEscaped);
                    break;
                case PacketBuilder.HORSE_CAUGHT:
                    if (WildHorse.DoesHorseExist(sender.LoggedinUser.CapturingHorseId))
                    {
                        WildHorse capturing = WildHorse.GetHorseById(sender.LoggedinUser.CapturingHorseId);
                        sender.LoggedinUser.CapturingHorseId = 0;

                        if (capturing.X == sender.LoggedinUser.X && capturing.Y == sender.LoggedinUser.Y) 
                        {
                            try
                            {
                                capturing.Capture(sender.LoggedinUser);
                            }
                            catch (InventoryFullException)
                            {
                                byte[] chatMsg = PacketBuilder.CreateChat(Messages.TooManyHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMsg);
                                break;
                            }

                            sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count++;

                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count >= 100)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(24)); // Wrangler
                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count >= 1000)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(25)); // Pro Wrangler

                            Logger.InfoPrint(sender.LoggedinUser.Username + " Captured a: " + capturing.Instance.Breed.Name);

                            sender.LoggedinUser.MetaPriority = true;
                            byte[] horseCaught = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCaughtMessage());
                            sender.SendPacket(horseCaught);

                            break;
                        }
                    }
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] horseAllreadyCaught = PacketBuilder.CreateMetaPacket(Meta.BuildHorseEscapedAnyway());
                    sender.SendPacket(horseAllreadyCaught);
                    break;
                case PacketBuilder.HORSE_TRY_CAPTURE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);
                        
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        return;
                    }
                    if (!WildHorse.DoesHorseExist(randomId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to catch a horse that doesnt exist.");
                        return;
                    }
                    sender.LoggedinUser.CapturingHorseId = randomId;
                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.HorseCaptureTimer, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(chatPacket);
                    byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket("catchhorse", PacketBuilder.PACKET_SWF_MODULE_FORCE);
                    sender.SendPacket(swfModulePacket);

                    break;
                default:
                    Logger.DebugPrint("Unknown horse packet: " + BitConverter.ToString(packet).Replace("-", " "));
                    break;
            }
        }

        public static void OnDynamicInputReceived(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent dyamic input when not logged in.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            string dynamicInputStr = packetStr.Substring(1, packetStr.Length - 3);
            if(dynamicInputStr.Contains("|"))
            {
                string[] dynamicInput = dynamicInputStr.Split('|');
                if(dynamicInput.Length >= 1)
                {
                    int inputId = 0;
                    try
                    {
                        inputId = int.Parse(dynamicInput[0]);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input ");
                        return;
                    }

                    switch(inputId) 
                    {
                        case 1: // Bank
                            if (dynamicInput.Length >= 2)
                            {
                                int moneyDeposited = 0;
                                Int64 moneyWithdrawn = 0;
                                try
                                {
                                    moneyDeposited = int.Parse(dynamicInput[1]);
                                    moneyWithdrawn = Int64.Parse(dynamicInput[2]);
                                }
                                catch (Exception)
                                {
                                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to deposit/witthdraw NaN money....");
                                    UpdateArea(sender);
                                    break;
                                }

                                if((moneyDeposited <= sender.LoggedinUser.Money) && moneyDeposited != 0)
                                {
                                    sender.LoggedinUser.Money -= moneyDeposited;
                                    sender.LoggedinUser.BankMoney += Convert.ToUInt64(moneyDeposited);

                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatDepositedMoneyMessage(moneyDeposited), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                }

                                if ((moneyWithdrawn <= sender.LoggedinUser.BankMoney) && moneyWithdrawn != 0)
                                {
                                    sender.LoggedinUser.BankMoney -= moneyWithdrawn;
                                    sender.LoggedinUser.Money += Convert.ToInt32(moneyWithdrawn);

                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatWithdrawMoneyMessage(Convert.ToInt32(moneyWithdrawn)), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                }

                                UpdateArea(sender);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 2: // Send Mail
                            if(dynamicInput.Length >= 4)
                            {
                                string to = dynamicInput[1];
                                string subject = dynamicInput[2];
                                string message = dynamicInput[3];

                                if(sender.LoggedinUser.Money >= 3)
                                {
                                    if(Database.CheckUserExist(to))
                                    {
                                        int playerId = Database.GetUserid(to);

                                        sender.LoggedinUser.Money -= 3;
                                        Mailbox.Mail mailMessage = new Mailbox.Mail();
                                        mailMessage.RandomId = RandomID.NextRandomId();
                                        mailMessage.FromUser = sender.LoggedinUser.Id;
                                        mailMessage.ToUser = playerId;
                                        mailMessage.Timestamp = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                                        mailMessage.Read = false;
                                        mailMessage.Subject = subject;
                                        mailMessage.Message = message;

                                        if(IsUserOnline(playerId))
                                        {
                                            User user = GetUserById(playerId);
                                            user.MailBox.AddMail(mailMessage);

                                            byte[] BaseStatsPacketData = PacketBuilder.CreatePlayerData(user.Money, GameServer.GetNumberOfPlayers(), user.MailBox.UnreadMailCount);
                                            user.LoggedinClient.SendPacket(BaseStatsPacketData);
                                        }
                                        else
                                        {
                                            Database.AddMail(mailMessage.RandomId, mailMessage.ToUser, mailMessage.FromUser, mailMessage.Subject, mailMessage.Message, mailMessage.Timestamp, mailMessage.Read);
                                        }

                                        byte[] mailMessageSent = PacketBuilder.CreateChat(Messages.FormatCityHallSendMailMessage(to.ToLower()),PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(mailMessageSent);

                                        UpdateArea(sender);
                                    }
                                    else
                                    {
                                        byte[] userDontExistFormat = PacketBuilder.CreateChat(Messages.FormatCityHallCantFindPlayerMessage(to), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(userDontExistFormat);
                                    }
                                }
 
                            }
                            break;
                        case 3: // Add Item or Money to Trade
                            {
                                if (dynamicInput.Length >= 2)
                                {
                                    if(sender.LoggedinUser.AttemptingToOfferItem == -1) // Money
                                    {
                                        string answer = dynamicInput[1];
                                        int amountMoney = -1;
                                        try
                                        {
                                            amountMoney = int.Parse(answer);
                                        }
                                        catch (FormatException)
                                        {
                                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (Money TRADE, amount is NaN)");
                                        }

                                        if(sender.LoggedinUser.Money <= amountMoney)
                                        {
                                            byte[] tooMuchMoney = PacketBuilder.CreateChat(Messages.TradeMoneyOfferTooMuch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(tooMuchMoney);
                                            break;
                                        }

                                        sender.LoggedinUser.TradingWith.MoneyOffered = amountMoney;

                                        UpdateArea(sender);
                                        if(sender.LoggedinUser.TradingWith != null)
                                            if (!sender.LoggedinUser.TradingWith.OtherTrade.Trader.TradeMenuPriority)
                                                UpdateArea(sender.LoggedinUser.TradingWith.OtherTrade.Trader.LoggedinClient);
                                        break;
                                    }



                                    if (Item.ItemIdExist(sender.LoggedinUser.AttemptingToOfferItem))
                                    {
                                        string answer = dynamicInput[1];
                                        int itemCount = -1;
                                        try
                                        {
                                            itemCount = int.Parse(answer);
                                        }
                                        catch(FormatException)
                                        {
                                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (Item TRADE, id is NaN)");
                                        }

                                        InventoryItem item = sender.LoggedinUser.Inventory.GetItemByItemId(sender.LoggedinUser.AttemptingToOfferItem);

                                        if (itemCount <= 0)
                                        {
                                            byte[] MustBeAtleast1 = PacketBuilder.CreateChat(Messages.TradeItemOfferAtleast1, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(MustBeAtleast1);
                                            break;
                                        }
                                        if(itemCount > item.ItemInstances.Count)
                                        {
                                            byte[] TooMuchItems = PacketBuilder.CreateChat(Messages.FormatTradeItemOfferTooMuch(item.ItemInstances.Count, itemCount), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(TooMuchItems);
                                            break;
                                        }

                                        foreach(ItemInstance[] existingItems in sender.LoggedinUser.TradingWith.ItemsOffered)
                                        {
                                            if(existingItems[0].ItemId == sender.LoggedinUser.AttemptingToOfferItem)
                                            {
                                                sender.LoggedinUser.TradingWith.ItemsOffered.Remove(existingItems);
                                                break;
                                            }
                                        }


                                        
                                        ItemInstance[] items = new ItemInstance[itemCount];
                                        for (int i = 0; i < itemCount; i++)
                                        {
                                            items[i] = item.ItemInstances[i];
                                        }
                                        sender.LoggedinUser.TradingWith.ItemsOffered.Add(items);

                                        UpdateArea(sender);
                                        if (sender.LoggedinUser.TradingWith != null)
                                            if (!sender.LoggedinUser.TradingWith.OtherTrade.Trader.TradeMenuPriority)
                                                UpdateArea(sender.LoggedinUser.TradingWith.OtherTrade.Trader.LoggedinClient);
                                    }
                                    break;
                                }
                                else
                                {
                                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (Item TRADE, wrong size)");
                                    break;
                                }
                            }
                        case 6: // Riddle Room
                            if (dynamicInput.Length >= 2)
                            {
                                if (sender.LoggedinUser.LastRiddle != null)
                                {
                                    string answer = dynamicInput[1];
                                    if(sender.LoggedinUser.LastRiddle.CheckAnswer(sender.LoggedinUser, answer))
                                        sender.LoggedinUser.LastRiddle = null;
                                    UpdateArea(sender);
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (LastRiddle, wrong size)");
                                break;
                            }
                        case 5: // Horse Description
                            if (dynamicInput.Length >= 3)
                            {
                                if(sender.LoggedinUser.LastViewedHorse != null)
                                {
                                    sender.LoggedinUser.MetaPriority = true;
                                    sender.LoggedinUser.LastViewedHorse.Name = dynamicInput[1];
                                    sender.LoggedinUser.LastViewedHorse.Description = dynamicInput[2];
                                    byte[] horseNameSavedPacket = PacketBuilder.CreateChat(Messages.FormatHorseSavedProfileMessage(sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(horseNameSavedPacket);
                                    UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (NPC Search, wrong size)");
                                break;
                            }
                        case 4: // NPC Search
                            if(dynamicInput.Length >= 2)
                            {
                                sender.LoggedinUser.MetaPriority = true;
                                string metaWindow = Meta.BuildNpcSearch(dynamicInput[1]);
                                byte[] metaPacket = PacketBuilder.CreateMetaPacket(metaWindow);
                                sender.SendPacket(metaPacket);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (NPC Search, wrong size)");
                                break;
                            }
                        case 7: // Private Notes
                            if (dynamicInput.Length >= 2)
                            {
                                sender.LoggedinUser.PrivateNotes = dynamicInput[1];
                                UpdateStats(sender);
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.PrivateNotesSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 10: // Change auto sell price
                            if (dynamicInput.Length >= 2)
                            {
                                if (sender.LoggedinUser.LastViewedHorse != null)
                                {
                                    sender.LoggedinUser.MetaPriority = true;
                                    HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                                    int newSellPrice = 0;
                                    try
                                    {
                                        newSellPrice = int.Parse(dynamicInput[1]);
                                    }
                                    catch (FormatException)
                                    {
                                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to set sell price to non int value.");
                                        break;
                                    }

                                    byte[] sellPricePacket;
                                    if (newSellPrice > 0)
                                        sellPricePacket = PacketBuilder.CreateChat(Messages.FormatAutoSellConfirmedMessage(newSellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    else
                                        sellPricePacket = PacketBuilder.CreateChat(Messages.HorseAutoSellRemoved, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(sellPricePacket);
                                    horseInstance.AutoSell = newSellPrice;

                                    UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (autosell, wrong size)");
                                break;
                            }
                            break;
                        case 11: // Ranch Description Edit
                            if (dynamicInput.Length >= 2)
                            {
                                string title = dynamicInput[1];
                                string desc = dynamicInput[2];
                                if(sender.LoggedinUser.OwnedRanch != null)
                                {
                                    sender.LoggedinUser.OwnedRanch.Title = title;
                                    sender.LoggedinUser.OwnedRanch.Description = desc;
                                }
                                byte[] descriptionEditedMessage = PacketBuilder.CreateChat(Messages.RanchSavedRanchDescripton, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(descriptionEditedMessage);
                                // Completely forgot! public server opens your stats menu when you save your ranch info like DUH!!
                                UpdateStats(sender); 
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (ranch description, wrong size)");
                                break;
                            }
                            break;
                        case 12: // Abuse Report
                            if (dynamicInput.Length >= 2)
                            {
                                string userName = dynamicInput[1];
                                string reason = dynamicInput[2];
                                if(Database.CheckUserExist(userName))
                                {
                                    if(reason == "")
                                    {
                                        byte[] validReasonPlz = PacketBuilder.CreateChat(Messages.AbuseReportProvideValidReason, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(validReasonPlz);
                                        break;
                                    }

                                    Database.AddReport(sender.LoggedinUser.Username, userName, reason);
                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.AbuseReportFiled, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                    UpdateArea(sender);
                                    break;
                                }
                                else
                                {
                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAbuseReportPlayerNotFound(userName), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                    break;
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 13: // Libary Ranch Search
                            if (dynamicInput.Length >= 2)
                            {
                                string searchQuery = dynamicInput[1];
                                sender.LoggedinUser.MetaPriority = true;
                                byte[] serachResponse = PacketBuilder.CreateMetaPacket(Meta.BuildRanchSearchResults(searchQuery));
                                sender.SendPacket(serachResponse);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 14:
                            if(dynamicInput.Length >= 1)
                            {
                                string password = dynamicInput[1];
                                // Get current tile
                                if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                                {
                                    World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                                    if(tile.Code.StartsWith("PASSWORD-"))
                                    {
                                        string[] args = tile.Code.Replace("!","-").Split('-');
                                        if(args.Length >= 3)
                                        {
                                            string expectedPassword = args[1];
                                            int questId = int.Parse(args[2]);
                                            if(password.ToLower() == expectedPassword.ToLower())
                                            {
                                                Quest.CompleteQuest(sender.LoggedinUser, Quest.GetQuestById(questId), false);
                                            }
                                            else
                                            {
                                                Quest.QuestResult result = Quest.FailQuest(sender.LoggedinUser, Quest.GetQuestById(questId), true);
                                                if (result.NpcChat == null || result.NpcChat == "")
                                                    result.NpcChat = Messages.IncorrectPasswordMessage;
                                                byte[] ChatPacket = PacketBuilder.CreateChat(result.NpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                                sender.SendPacket(ChatPacket);
                                            }
                                        }
                                        else
                                        {
                                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Send invalid password input request. (Too few arguments!)");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Send password input request. (Not on password tile!)");
                                        break;
                                    }
                                }
                                else
                                {
                                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent a password while not in a special tile.");
                                    break;
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid password request, (wrong size)");
                                break;
                            }

                            break;
                        case 15: // Real Time Quiz
                            if (dynamicInput.Length >= 2)
                            {
                                if(QuizEvent != null)
                                {
                                    if (sender.LoggedinUser.InRealTimeQuiz)
                                    {
                                        RealTimeQuiz.Participent participent = QuizEvent.JoinEvent(sender.LoggedinUser);
                                        string answer = dynamicInput[1];
                                        participent.CheckAnswer(answer);
                                    }
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (RealTimeQuiz, wrong size)");
                                break;
                            }
                        default:
                            Logger.ErrorPrint("Unknown dynamic input: " + inputId.ToString() + " packet dump: " + BitConverter.ToString(packet).Replace("-", " "));
                            break;
                    }


                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (wrong size)");
                    return;
                }
            }
            

        }

        public static void OnPlayerInfoPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requests player info when not logged in.");
                return;
            }
            if(packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent playerinfo packet of wrong size");
            }

            byte method = packet[1];
            switch(method)
            {
                case PacketBuilder.PLAYERINFO_PLAYER_LIST:
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerListMenu(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
            }

        }
        public static void OnDynamicButtonPressed(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Clicked dyamic button when not logged in.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            string buttonIdStr = packetStr.Substring(1, packetStr.Length - 3);

            switch(buttonIdStr)
            {
                case "2": // Compose Mail
                    if(sender.LoggedinUser.Money <= 3)
                    {
                        byte[] cantAffordPostage = PacketBuilder.CreateChat(Messages.CityHallCantAffordPostageMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(cantAffordPostage);
                        break;
                    }
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildComposeMailMenu());
                    sender.SendPacket(metaPacket);
                    break;
                case "3": // Quest Log
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildQuestLog(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "4": // View Horse Breeds
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseBreedListLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "5": // Back to horse
                    if (sender.LoggedinUser.LastViewedHorse != null)
                        UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                    break;
                case "6": // Equip companion
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCompanionEquipMenu(horseInstance,sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "7": // TP To nearest wagon (ranch)
                    if(sender.LoggedinUser.OwnedRanch != null)
                    {
                        if(sender.LoggedinUser.OwnedRanch.GetBuildingCount(7) > 0) // Wagon
                        {
                            int ranchX = sender.LoggedinUser.OwnedRanch.X;
                            int ranchY = sender.LoggedinUser.OwnedRanch.Y;

                            double smallestDistance = Double.PositiveInfinity;
                            int smalestTransportPointId = 0;
                            for (int i = 0; i < Transport.TransportPoints.Count; i++) 
                            {
                                Transport.TransportPoint tpPoint = Transport.TransportPoints[i];

                                if(Transport.GetTransportLocation(tpPoint.Locations[0]).Type == "WAGON") // is wagon?
                                {
                                    double distance = Converters.PointsToDistance(ranchX, ranchY, tpPoint.X, tpPoint.Y);
                                    if(distance < smallestDistance)
                                    {
                                        smallestDistance = distance;
                                        smalestTransportPointId = i;
                                    }
                                }
                            }
                            Transport.TransportPoint newPoint = Transport.TransportPoints[smalestTransportPointId];

                            int newX = newPoint.X;
                            int newY = newPoint.Y;

                            if (World.InSpecialTile(newX, newY))
                            {
                                World.SpecialTile tile = World.GetSpecialTile(newX, newY);
                                if (tile.ExitX != 0)
                                    newX = tile.ExitX;
                                if (tile.ExitY != 0)
                                    newY = tile.ExitY;
                            }

                            byte[] transported = PacketBuilder.CreateChat(Messages.RanchWagonDroppedYouOff, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(transported);
                            sender.LoggedinUser.Teleport(newX, newY);
                        }
                    }
                    break;
                case "8":
                    if(sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseReleaseConfirmationMessage(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "9": // View Tack (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "10": // View Companions (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildCompanionLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "11": // Randomize horse name
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        horseInstance.ChangeNameWithoutUpdatingDatabase(HorseInfo.GenerateHorseName());
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseDescriptionEditMeta(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "12": // View Minigames (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMinigamesLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "13": // Train All (Ranch)
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        if(sender.LoggedinUser.OwnedRanch.GetBuildingCount(6) > 0) // Training Pen
                        {
                            sender.LoggedinUser.MetaPriority = true;
                            metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRanchTraining(sender.LoggedinUser));
                            sender.SendPacket(metaPacket);
                        }
                    }
                    break;
                case "14": // Most Valued Ranches
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMostValuedRanches());
                    sender.SendPacket(metaPacket);
                    break;
                case "15": // Most Richest Players
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRichestPlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "16": // Most Adventurous Players
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAdventurousPlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "17": // Most Experienced Players
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildExperiencedPlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "18": // Best Minigame Players
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMinigamePlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "19": // Most Experienced Horses
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMostExperienedHoses());
                    sender.SendPacket(metaPacket);
                    break;
                case "20": // Minigame Rankings
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMinigameRankingsForUser(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "21": // Private Notes
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPrivateNotes(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "22": // View Locations (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildLocationsLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "23": // View Awards (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAwardsLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "26": // Buy Horse (Auto Sell)
                    if(sender.LoggedinUser.LastViewedHorseOther != null)
                    {
                        bool isOnRanch = false;
                        bool isOnPlayer = false;
                        HorseInstance horseToSell = sender.LoggedinUser.LastViewedHorseOther;
                        if (Ranch.IsRanchOwned(horseToSell.Owner))
                        {
                            Ranch ranch = Ranch.GetRanchOwnedBy(horseToSell.Owner);
                            if(sender.LoggedinUser.X == ranch.X && sender.LoggedinUser.Y == ranch.Y)
                            {
                                isOnRanch = true;
                            }

                        }
                        if(GameServer.IsUserOnline(horseToSell.Owner))
                        {
                            User user = GameServer.GetUserById(horseToSell.Owner);
                            if (user.X == sender.LoggedinUser.X && user.Y == sender.LoggedinUser.Y)
                            {
                                isOnPlayer = true;
                            }
                        }

                        if (isOnRanch || isOnPlayer)
                        {
                            
                            if (horseToSell.AutoSell == 0)
                                break;
                            if(sender.LoggedinUser.Money >= horseToSell.AutoSell)
                            {
                                if (sender.LoggedinUser.HorseInventory.HorseList.Length + 1 > sender.LoggedinUser.MaxHorses)
                                {
                                    byte[] tooManyHorses = PacketBuilder.CreateChat(Messages.AutoSellTooManyHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(tooManyHorses);
                                    break;
                                }

                                if(IsUserOnline(horseToSell.Owner))
                                {
                                    User seller = GetUserById(horseToSell.Owner);
                                    sender.LoggedinUser.HorseInventory.DeleteHorse(horseToSell, false);

                                    byte[] horseBrought = PacketBuilder.CreateChat(Messages.FormatAutoSellSold(horseToSell.Name, horseToSell.AutoSell, sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    seller.LoggedinClient.SendPacket(horseBrought);
                                }
                                else
                                {
                                    Database.AddMessageToQueue(horseToSell.Owner, Messages.FormatAutoSellSoldOffline(horseToSell.Name, horseToSell.AutoSell, sender.LoggedinUser.Username));
                                }

                                horseToSell.Owner = sender.LoggedinUser.Id;
                                horseToSell.AutoSell = 0;
                                sender.LoggedinUser.HorseInventory.AddHorse(horseToSell, false);

                                byte[] success = PacketBuilder.CreateChat(Messages.FormatAutoSellSuccess(horseToSell.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(success);

                                UpdateArea(sender);
                                break;
                            }
                            else
                            {
                                byte[] noMoney = PacketBuilder.CreateChat(Messages.AutoSellInsufficentFunds, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(noMoney);
                                break;
                            }

                        }
                        else
                        {
                            byte[] notInRightPlace = PacketBuilder.CreateChat(Messages.AutoSellNotStandingInSamePlace, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notInRightPlace);
                            break;
                        }

                        
                    }
                    break;
                case "24": // Award List
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAwardList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "27": // Ranch Edit
                    if(sender.LoggedinUser.OwnedRanch != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRanchEdit(sender.LoggedinUser.OwnedRanch));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "29": // Auto Sell Horses
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTopAutoSellHorses());
                    sender.SendPacket(metaPacket);
                    break;
                case "31":
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRanchSearchLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "35": // Buddy List
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildBuddyList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "36": // Nearby list
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildNearbyList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "37": // All Players List
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "40": // All Players Alphabetical
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerListAlphabetical(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "30": // Find NPC
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildFindNpcMenu());
                    sender.SendPacket(metaPacket);
                    break;
                case "25": // Set auto sell price
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAutoSellMenu(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "33": // View All stats (Horse)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAllBasicStats(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "34": // View Basic stats (Horse)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAllStats(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "38": // Read Books
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildBooksLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "41": // Put horse into auction
                    sender.LoggedinUser.MetaPriority = true;
                    sender.LoggedinUser.ListingAuction = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAuctionHorseList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "47":
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPawneerOrderBreedList());
                    sender.SendPacket(metaPacket);
                    break;
                case "53": // Misc Stats / Tracked Items
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMiscStats(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "58": // Add new item to trade
                    if(sender.LoggedinUser.TradingWith != null)
                    {
                        sender.LoggedinUser.TradeMenuPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTradeAdd(sender.LoggedinUser.TradingWith));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "59": // Done
                    if (sender.LoggedinUser.TradingWith != null)
                    {
                        sender.LoggedinUser.TradingWith.Stage = "DONE";

                        if (sender.LoggedinUser.TradingWith != null)
                            if (sender.LoggedinUser.TradingWith.OtherTrade.Trader.TradeMenuPriority == false)
                                UpdateArea(sender.LoggedinUser.TradingWith.OtherTrade.Trader.LoggedinClient);
                        UpdateArea(sender);

                    }
                    break;
                case "60": // Ranch Sell
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRanchSellConfirmation());
                    sender.SendPacket(metaPacket);
                    break;
                case "61": // Most Spoiled Horse
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMostSpoiledHorses());
                    sender.SendPacket(metaPacket);
                    break;
                case "28c1": // Abuse Report
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAbuseReportPage());
                    sender.SendPacket(metaPacket);
                    break;
                case "52c1": // Horse set to KEEPER
                    string category = "KEEPER";
                    goto setCategory;
                case "52c2": // Horse set to TRAINING
                    category = "TRAINING";
                    goto setCategory;
                case "52c3": // Horse set to TRADING
                    category = "TRADING";
                    goto setCategory;
                case "52c4": // Horse set to RETIRED
                    category = "RETIRED";
                    goto setCategory;
                setCategory:;
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.LastViewedHorse.Category = category;
                        byte[] categoryChangedPacket = PacketBuilder.CreateChat(Messages.FormatHorseSetToNewCategory(category), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(categoryChangedPacket);

                        sender.LoggedinUser.MetaPriority = true;
                        UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                    }
                    break;

                default:
                    if(buttonIdStr.StartsWith("39c")) // Book Read
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int bookId = -1;
                        try
                        {
                            bookId = int.Parse(idStr);
                        }
                        catch (FormatException)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to read a book of id NaN");
                            break;
                        };

                        if(Book.BookExists(bookId))
                        {
                            Book book = Book.GetBookById(bookId);
                            sender.LoggedinUser.MetaPriority = true;
                            metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildBookReadLibary(book));
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + "Tried to read a book that doesnt exist.");
                        }
                        break;
                    }
                    if (buttonIdStr.StartsWith("32c")) // Horse Whisperer
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int breedId = -1;
                        try
                        {
                            breedId = int.Parse(idStr);
                        }
                        catch (FormatException)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to whisper a horse with BreedId NaN.");
                            break;
                        };

                        if (sender.LoggedinUser.Money < 50000)
                        {
                            byte[] cannotAffordMessage = PacketBuilder.CreateChat(Messages.WhispererServiceCannotAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cannotAffordMessage);
                            break;
                        }

                        List<WildHorse> horsesFound = new List<WildHorse>();
                        foreach (WildHorse horse in WildHorse.WildHorses)
                        {
                            if (horse.Instance.Breed.Id == breedId)
                            {
                                horsesFound.Add(horse);
                            }
                        }
                        int cost = 0;
                        if (horsesFound.Count >= 1)
                        {
                            cost = 50000;
                        }
                        else
                        {
                            cost = 10000;
                        }
                        sender.LoggedinUser.MetaPriority = true;

                        byte[] pricingMessage = PacketBuilder.CreateChat(Messages.FormatWhispererPrice(cost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(pricingMessage);

                        byte[] serachResultMeta = PacketBuilder.CreateMetaPacket(Meta.BuildWhisperSearchResults(horsesFound.ToArray()));
                        sender.SendPacket(serachResultMeta);

                        sender.LoggedinUser.Money -= cost;
                        break;
                    }
                    else if (buttonIdStr.StartsWith("4c")) // Libary Breed Search
                    {
                        string idStr = buttonIdStr.Substring(2);
                        int breedId = -1;
                        HorseInfo.Breed horseBreed;
                        try
                        {
                            breedId = int.Parse(idStr);
                            horseBreed = HorseInfo.GetBreedById(breedId);
                        }
                        catch (Exception)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Sent invalid libary breed viewer request.");
                            break;
                        };
                        sender.LoggedinUser.MetaPriority = true;
                        string metaTag = Meta.BuildBreedViewerLibary(horseBreed);
                        metaPacket = PacketBuilder.CreateMetaPacket(metaTag);
                        sender.SendPacket(metaPacket);

                        string swf = "breedviewer.swf?terrain=book&breed=" + horseBreed.Swf + "&j=";
                        byte[] loadSwf = PacketBuilder.CreateSwfModulePacket(swf, PacketBuilder.PACKET_SWF_MODULE_FORCE);
                        sender.SendPacket(loadSwf);

                        break;

                    }
                    else if (buttonIdStr.StartsWith("50c"))
                    {
                        string gender = buttonIdStr.Substring(3);
                        if (sender.LoggedinUser.PawneerOrderBreed != null)
                        {
                            if (sender.LoggedinUser.PawneerOrderBreed.GenderTypes().Contains(gender))
                            {
                                if (sender.LoggedinUser.Inventory.HasItemId(Item.PawneerOrder))
                                {
                                    sender.LoggedinUser.PawneerOrderGender = gender;

                                    HorseInstance horseInstance = new HorseInstance(sender.LoggedinUser.PawneerOrderBreed);
                                    horseInstance.Color = sender.LoggedinUser.PawneerOrderColor;
                                    horseInstance.Gender = sender.LoggedinUser.PawneerOrderGender;
                                    horseInstance.Name = "Pawneer Order";

                                    sender.LoggedinUser.Inventory.Remove(sender.LoggedinUser.Inventory.GetItemByItemId(Item.PawneerOrder).ItemInstances[0]);
                                    sender.LoggedinUser.HorseInventory.AddHorse(horseInstance);

                                    sender.LoggedinUser.MetaPriority = true;
                                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPawneerOrderFound(horseInstance));
                                    sender.SendPacket(metaPacket);
                                    break;
                                }
                            }
                        }
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Error occured when doing a Pawneer Order.");
                        break;
                    }
                    else if (buttonIdStr.StartsWith("49c"))
                    {
                        string color = buttonIdStr.Substring(3);
                        if (sender.LoggedinUser.PawneerOrderBreed != null)
                        {
                            if (sender.LoggedinUser.PawneerOrderBreed.Colors.Contains(color))
                            {
                                sender.LoggedinUser.PawneerOrderColor = color;

                                sender.LoggedinUser.MetaPriority = true;
                                metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPawneerOrderGenderList(sender.LoggedinUser.PawneerOrderBreed, color));
                                sender.SendPacket(metaPacket);
                                break;
                            }
                        }
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Asked for a horse of an unknown color " + color);
                        break;
                    }
                    else if (buttonIdStr.StartsWith("48c")) // Pawneer Order Breed Select
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int breedId = -1;
                        HorseInfo.Breed breed;
                        try
                        {
                            breedId = int.Parse(idStr);
                            breed = HorseInfo.GetBreedById(breedId);
                        }
                        catch (Exception)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to pawner order a horse with id NaN.");
                            break;
                        }
                        sender.LoggedinUser.PawneerOrderBreed = breed;

                        sender.LoggedinUser.MetaPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPawneerOrderColorList(breed));
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else if (buttonIdStr.StartsWith("43c")) // Pawn Horse Confirm
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int horseId = -1;
                        try
                        {
                            horseId = int.Parse(idStr);
                        }
                        catch (FormatException)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to pawn a horse with id NaN.");
                            break;
                        }

                        if (sender.LoggedinUser.HorseInventory.HorseIdExist(horseId))
                        {
                            HorseInstance inst = sender.LoggedinUser.HorseInventory.GetHorseById(horseId);
                            int price = Pawneer.CalculateTotalPrice(inst);
                            string name = inst.Name;

                            sender.LoggedinUser.HorseInventory.DeleteHorse(inst); // 1000% a "distant land.."
                            sender.LoggedinUser.LastViewedHorse = null;

                            sender.LoggedinUser.Money += price;
                            byte[] soldHorseMessage = PacketBuilder.CreateChat(Messages.FormatPawneerSold(name, price), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(soldHorseMessage);

                            sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count++;
                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count >= 100)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(44)); // Vendor
                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count >= 1000)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(45)); // Pro Vendor
                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count >= 10000)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(52)); // Top Vendor

                            UpdateArea(sender);

                            break;
                        }
                        else
                        {
                            byte[] cantFindHorse = PacketBuilder.CreateChat(Messages.PawneerHorseNotFound, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantFindHorse);
                        }
                        break;
                    }
                    else if (buttonIdStr.StartsWith("51c")) // Pawn Horse
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int horseId = -1;
                        try
                        {
                            horseId = int.Parse(idStr);
                        }
                        catch (FormatException)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to pawn a horse with id NaN.");
                            break;
                        }

                        if (sender.LoggedinUser.HorseInventory.HorseIdExist(horseId))
                        {
                            HorseInstance inst = sender.LoggedinUser.HorseInventory.GetHorseById(horseId);

                            sender.LoggedinUser.MetaPriority = true;
                            byte[] confirmScreen = PacketBuilder.CreateMetaPacket(Meta.BuildPawneerConfimation(inst));
                            sender.SendPacket(confirmScreen);
                            break;
                        }
                        else
                        {
                            byte[] cantFindHorse = PacketBuilder.CreateChat(Messages.PawneerHorseNotFound, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantFindHorse);
                        }
                        break;
                    }
                    else if (buttonIdStr.StartsWith("42c"))
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int horseId = -1;
                        try
                        {
                            horseId = int.Parse(idStr);
                        }
                        catch (FormatException)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to auction a horse with id NaN.");
                            break;
                        }
                        if (sender.LoggedinUser.HorseInventory.HorseIdExist(horseId))
                        {
                            HorseInstance inst = sender.LoggedinUser.HorseInventory.GetHorseById(horseId);

                            if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                            {
                                World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                                if(tile.Code == null || !tile.Code.StartsWith("AUCTION-"))
                                {
                                    Logger.ErrorPrint("Cant find auction room that " + sender.LoggedinUser.Username + " Is trying to place a horse in.");
                                    return;
                                }
                                Auction auctionRoom = Auction.GetAuctionRoomById(int.Parse(tile.Code.Split('-')[1]));
                                if(auctionRoom.HasUserPlacedAuctionAllready(sender.LoggedinUser))
                                {
                                    byte[] cantPlaceAuction = PacketBuilder.CreateChat(Messages.AuctionOneHorsePerPlayer, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantPlaceAuction);
                                    break;
                                }
                                if (sender.LoggedinUser.Money >= 1000)
                                {
                                    sender.LoggedinUser.Money -= 1000;
                                    Auction.AuctionEntry entry = new Auction.AuctionEntry(8, 0, sender.LoggedinUser.Id);
                                    entry.Horse = inst;
                                    entry.OwnerId = sender.LoggedinUser.Id;
                                    entry.Completed = false;
                                    inst.Hidden = true;
                                    auctionRoom.AddEntry(entry);
                                    UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                                    break;
                                }
                                else
                                {
                                    byte[] cantAffordAuctionMsg = PacketBuilder.CreateChat(Messages.AuctionCantAffordAuctionFee, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantAffordAuctionMsg);
                                }
                            }
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to auction a horse they did not have.");
                            break;
                        }
                    }
                    if (Leaser.LeaserButtonIdExists(buttonIdStr))
                    {
                        Leaser horseLeaser = Leaser.GetLeaserByButtonId(buttonIdStr);

                        if(sender.LoggedinUser.Money >= horseLeaser.Price)
                        {
                            if(sender.LoggedinUser.HorseInventory.HorseList.Length + 1 > sender.LoggedinUser.MaxHorses)
                            {
                                byte[] cantManageHorses = PacketBuilder.CreateChat(Messages.HorseLeaserHorsesFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cantManageHorses);
                                break;
                            }
                            else
                            {
                                sender.LoggedinUser.MetaPriority = true;
                                sender.LoggedinUser.Money -= horseLeaser.Price;
                                
                                HorseInstance leaseHorse = horseLeaser.GenerateLeaseHorse();
                                
                                if(leaseHorse.Breed.Id == 170) // UniPeg
                                {
                                    sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnipegTeamup).Count++;
                                    if(sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnipegTeamup).Count >= 5)
                                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(55)); // UniPeg Friend
                                }
                                else if(leaseHorse.Breed.Type == "unicorn")
                                {
                                    sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnicornTeamup).Count++;
                                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnicornTeamup).Count >= 5)
                                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(42)); // Unicorn Friend
                                }
                                else if(leaseHorse.Breed.Type == "pegasus")
                                {
                                    sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PegasusTeamup).Count++;
                                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PegasusTeamup).Count >= 5)
                                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(43)); // Pegasus Friend
                                }

                                sender.LoggedinUser.HorseInventory.AddHorse(leaseHorse);

                                byte[] addedHorseMeta = PacketBuilder.CreateMetaPacket(Meta.BuildLeaserOnLeaseInfo(horseLeaser));
                                sender.SendPacket(addedHorseMeta);

                                byte[] addedNewTempHorseMessage = PacketBuilder.CreateChat(Messages.HorseLeaserTemporaryHorseAdded, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(addedNewTempHorseMessage);
                                break;

                            }
                        }
                        else
                        {
                            byte[] cantAffordLease = PacketBuilder.CreateChat(Messages.HorseLeaserCantAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAffordLease);
                            break;
                        }
                        
                    }
                    if(AbuseReport.DoesReasonExist(buttonIdStr))
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(AbuseReport.GetReasonById(buttonIdStr).Meta);
                        sender.SendPacket(metaPacket);
                        break;
                    }

                    Logger.ErrorPrint("Dynamic button #" + buttonIdStr + " unknown... Packet Dump: "+BitConverter.ToString(packet).Replace("-", " "));
                    break;
            }
        }
        public static void OnArenaScored(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested user information when not logged in.");
                return;
            }
            if(packet.Length <= 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + "Sent invalid Arena Scored Packet.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            string scoreStr = packetStr.Substring(1, packet.Length - 3);
            int score = -1;
            try
            {
                score = int.Parse(scoreStr);
            }
            catch(FormatException)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Scored NAN in an arena.");
                return;
            }

            if(Arena.UserHasEnteredHorseInAnyArena(sender.LoggedinUser))
            {
                byte[] waitingOnResults = PacketBuilder.CreateChat(Messages.FormatArenaYourScore(score), PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(waitingOnResults);

                Arena enteredArena = Arena.GetArenaUserEnteredIn(sender.LoggedinUser);
                enteredArena.SubmitScore(sender.LoggedinUser, score);
            }
            else
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Scored in an arena while not in one");
            }
            return;

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

            
            byte[] WorldData = PacketBuilder.CreateWorldData(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, sender.LoggedinUser.GetWeatherSeen());
            sender.SendPacket(WorldData);

            // Send first time message;
            if (sender.LoggedinUser.NewPlayer)
            {
                byte[] NewUserMessage = PacketBuilder.CreateChat(Messages.NewUserMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(NewUserMessage);
            }


            byte[] SecCodePacket = PacketBuilder.CreateSecCode(user.SecCodeSeeds, user.SecCodeInc, user.Administrator, user.Moderator);
            sender.SendPacket(SecCodePacket);

            byte[] BaseStatsPacketData = PacketBuilder.CreatePlayerData(user.Money, GameServer.GetNumberOfPlayers(), user.MailBox.UnreadMailCount);
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
                    if(!nearbyUser.MetaPriority)
                        UpdateArea(nearbyUser.LoggedinClient);

            byte[] IsleData = PacketBuilder.CreatePlaceData(World.Isles.ToArray(), World.Towns.ToArray(), World.Areas.ToArray());
            sender.SendPacket(IsleData);

            byte[] TileFlags = PacketBuilder.CreateTileOverlayFlags(Map.OverlayTileDepth);
            sender.SendPacket(TileFlags);

            byte[] MotdData = PacketBuilder.CreateMotd();
            sender.SendPacket(MotdData);

            // Send riddle annoucement
            if (RiddleEvent != null)
                if (RiddleEvent.Active)
                    RiddleEvent.ShowStartMessage(sender);
            
            // Send Queued Messages
            string[] queuedMessages = Database.GetMessageQueue(sender.LoggedinUser.Id);
            foreach(string queuedMessage in queuedMessages)
            {
                byte[] msg = PacketBuilder.CreateChat(Messages.MessageQueueHeader+queuedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(msg);
            }
            Database.ClearMessageQueue(sender.LoggedinUser.Id);

        }

        public static void OnSwfModuleCommunication(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " tried to send swf communication when not logged in.");
                return;
            }
            if (packet.Length < 4)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid swf commmunication Packet");
                return;
            }


            byte module = packet[1];
            switch(module)
            {
                case PacketBuilder.SWFMODULE_INVITE:
                    if(packet.Length < 4)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid 2PLAYER INVITE Packet (WRONG SIZE)");
                        break;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    int playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (Exception) { };

                    if(IsUserOnline(playerId))
                    {
                        User toInvite = GetUserById(playerId);
                        TwoPlayer twoPlayerGame = new TwoPlayer(toInvite, sender.LoggedinUser, false);
                    }
                    break;
                case PacketBuilder.SWFMODULE_ACCEPT:
                    if (packet.Length < 4)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid 2PLAYER ACCEPT Packet (WRONG SIZE)");
                        break;
                    }
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (Exception) { };

                    if (IsUserOnline(playerId))
                    {
                        User toAccept = GetUserById(playerId);
                        if(TwoPlayer.IsPlayerInvitingPlayer(toAccept, sender.LoggedinUser))
                        {
                            TwoPlayer twoPlayerGame = TwoPlayer.GetGameInvitingPlayer(toAccept, sender.LoggedinUser);
                            twoPlayerGame.Accept(sender.LoggedinUser);
                        }
                    }
                    break;
                case PacketBuilder.SWFMODULE_DRAWINGROOM:
                    if(packet.Length < 3)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if(packet[2] == PacketBuilder.DRAWINGROOM_GET_DRAWING)
                    {
                        if (packet.Length < 6)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }
                        int roomId = packet[3] - 40;
                        Drawingroom room;
                        try
                        {
                           room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch(KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        
                        }

                        if(room.Drawing != "")
                        {
                            byte[] drawingPacket = PacketBuilder.CreateDrawingUpdatePacket(room.Drawing);
                            sender.SendPacket(drawingPacket);
                        }

                    }
                    else if(packet[2] == PacketBuilder.DRAWINGROOM_SAVE)
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        /*
                         *   The lack of an if case for if the user isnt subscribed
                         *   is NOT a bug thats just how pinto does it.
                         *   you can save but not load if your subscribed. weird huh?
                         */

                        int roomId = packet[3] - 40;
                        Drawingroom room;
                        try
                        {
                            room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch (KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        

                        if (!Database.SavedDrawingsExist(sender.LoggedinUser.Id))
                            Database.CreateSavedDrawings(sender.LoggedinUser.Id);

                        int slotNo = 0;
                        byte slot = packet[4];
                        switch (slot)
                        {
                            case 0x29: // Slot 1
                                Database.SaveDrawingSlot1(sender.LoggedinUser.Id, room.Drawing);
                                slotNo = 1;
                                break;
                            case 0x2A: // Slot 2
                                Database.SaveDrawingSlot2(sender.LoggedinUser.Id, room.Drawing);
                                slotNo = 2;
                                break;
                            case 0x2B: // Slot 3
                                Database.SaveDrawingSlot3(sender.LoggedinUser.Id, room.Drawing);
                                slotNo = 3;
                                break;
                        }

                        byte[] savedDrawingMessage = PacketBuilder.CreateChat(Messages.FormatDrawingRoomSaved(slotNo), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(savedDrawingMessage);

                        break;
                    }
                    else if (packet[2] == PacketBuilder.DRAWINGROOM_LOAD)
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        if(!sender.LoggedinUser.Subscribed)
                        {
                            byte[] notSubscribedCantLoad = PacketBuilder.CreateChat(Messages.DrawingCannotLoadNotSubscribed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notSubscribedCantLoad);
                            break;
                        }

                        int roomId = packet[3] - 40;
                        Drawingroom room;
                        try
                        {
                            room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch (KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        if (!Database.SavedDrawingsExist(sender.LoggedinUser.Id))
                            Database.CreateSavedDrawings(sender.LoggedinUser.Id);

                        int slotNo = 0;
                        byte slot = packet[4];
                        string drawingToAdd = "";
                        switch (slot)
                        {
                            case 0x29: // Slot 1
                                drawingToAdd = Database.LoadDrawingSlot1(sender.LoggedinUser.Id);
                                slotNo = 1;
                                break;
                            case 0x2A: // Slot 2
                                drawingToAdd = Database.LoadDrawingSlot2(sender.LoggedinUser.Id);
                                slotNo = 2;
                                break;
                            case 0x2B: // Slot 3
                                drawingToAdd = Database.LoadDrawingSlot3(sender.LoggedinUser.Id);
                                slotNo = 3;
                                break;
                        }

                        if (room.Drawing.Length + drawingToAdd.Length < 65535) // will this max out the db?
                        {
                            room.Drawing += drawingToAdd;
                            Database.SetLastPlayer("D" + room.Id.ToString(), sender.LoggedinUser.Id);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                        }
                        else
                        {
                            byte[] roomFullMessage = PacketBuilder.CreateChat(Messages.DrawingPlzClearLoad, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(roomFullMessage);
                            break;
                        }

                        room.Drawing += drawingToAdd;
                        UpdateDrawingForAll("D" + room.Id, sender, drawingToAdd, true);

                        byte[] loadedDrawingMessage = PacketBuilder.CreateChat(Messages.FormatDrawingRoomLoaded(slotNo), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(loadedDrawingMessage);

                        break;
                    }
                    else // Default action- draw line
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        if(!sender.LoggedinUser.Subscribed)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to draw while not subscribed.");
                            byte[] notSubscribedMessage = PacketBuilder.CreateChat(Messages.DrawingNotSentNotSubscribed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notSubscribedMessage);
                            break;
                        }

                        int roomId = packet[2] - 40;
                        Drawingroom room;
                        try
                        {
                            room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch (KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        packetStr = Encoding.UTF8.GetString(packet);
                        
                        string drawing = packetStr.Substring(3, packetStr.Length - 5);
                        if (drawing.Contains("X")) // Clear byte
                        {
                            room.Drawing = "";
                        }
                        else if(room.Drawing.Length + drawing.Length < 65535) // will this max out the db?
                        {
                            room.Drawing += drawing;
                            Database.SetLastPlayer("D" + room.Id.ToString(), sender.LoggedinUser.Id);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                        }
                        else
                        {
                            byte[] roomFullMessage = PacketBuilder.CreateChat(Messages.DrawingPlzClearDraw, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(roomFullMessage);
                            break;
                        }

                        UpdateDrawingForAll("D" + room.Id, sender, drawing, false);

                    }

                    break;
                case PacketBuilder.SWFMODULE_BRICKPOET:
                    if(packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if(packet[2] == PacketBuilder.BRICKPOET_LIST_ALL)
                    {
                        if (packet.Length < 6)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET LIST ALL packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        int roomId = packet[3] - 40;
                        Brickpoet.PoetryPeice[] room;
                        try // Make sure the room exists-
                        {
                            room = Brickpoet.GetPoetryRoom(roomId);
                        }
                        catch(KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid brickpoet room: " + roomId);
                            break;
                        }
                        // Send list of peices
                        byte[] poetPacket = PacketBuilder.CreateBrickPoetListPacket(room);
                        sender.SendPacket(poetPacket);

                    }
                    else if(packet[3] == PacketBuilder.BRICKPOET_MOVE)
                    {
                        if (packet.Length < 0xB)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET MOVE packet (swf communication, WRONG SIZE)");
                            break;
                        }
                        packetStr = Encoding.UTF8.GetString(packet);
                        if(!packetStr.Contains('|'))
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET MOVE packet (swf communication, NO | SEPERATOR)");
                            break;
                        }
                        string[] args = packetStr.Split('|');
                        if(args.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET MOVE Packet (swf communication, NOT ENOUGH | SEPERATORS.");
                            break;
                        }

                        int roomId = packet[2] - 40;
                        int peiceId;
                        int x;
                        int y;
                        Brickpoet.PoetryPeice[] room;
                        Brickpoet.PoetryPeice peice;

                        try // Make sure these are acturally numbers!
                        {
                            peiceId = int.Parse(args[1]);
                            x = int.Parse(args[2]);
                            y = int.Parse(args[3]);


                            room = Brickpoet.GetPoetryRoom(roomId);
                            peice = Brickpoet.GetPoetryPeice(room, peiceId);
                            
                        }
                        catch (Exception)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to move a peice in an invalid brickpoet room: " + roomId);
                            break;
                        }
                        // Change location in Database
                        peice.X = x;
                        peice.Y = y;

                        foreach(User user in GetUsersOnSpecialTileCode("MULTIROOM-" + "P" + roomId.ToString())) // Send to each user!
                        {
                            if (user.Id == sender.LoggedinUser.Id)
                                continue;

                            byte[] updatePoetRoomPacket = PacketBuilder.CreateBrickPoetMovePacket(peice);
                            user.LoggedinClient.SendPacket(updatePoetRoomPacket);
                            
                        }

                        if (Database.GetLastPlayer("P" + roomId) != sender.LoggedinUser.Id)
                        {
                            Database.SetLastPlayer("P" + roomId, sender.LoggedinUser.Id);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                        }

                        break;
                    }
                    else
                    {
                        Logger.DebugPrint(" packet dump: " + BitConverter.ToString(packet).Replace("-", " "));
                        break;
                    }

                    break;
                case PacketBuilder.SWFMODULE_DRESSUPROOM:
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRESSUPROOM packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if (packet[2] == PacketBuilder.DRESSUPROOM_LIST_ALL)
                    {
                        int roomId = packet[3] - 40;
                        Dressup.DressupRoom room = Dressup.GetDressupRoom(roomId);

                        if (room.DressupPeices.Count > 0)
                        {
                            byte[] allDressupsResponse = PacketBuilder.CreateDressupRoomPeiceResponse(room.DressupPeices.ToArray());
                            sender.SendPacket(allDressupsResponse);
                        }

                    }
                    else // Move
                    {
                        if (packet.Length < 9)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        int roomId = packet[2] - 40;
                        if (roomId <= 0)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, INVALID ROOM)");
                            break;
                        }
                        Dressup.DressupRoom room = Dressup.GetDressupRoom(roomId);

                        packetStr = Encoding.UTF8.GetString(packet);
                        string moveStr = packetStr.Substring(3, packetStr.Length - 5);

                        string[] moves = moveStr.Split('|');

                        if(moves.Length < 3)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, MOVES WRONG SIZE)");
                            break;
                        }

                        int peiceId;
                        double moveToX;
                        double moveToY;
                        bool active = true;
                        try // Make sure these are acturally numbers!
                        {
                            peiceId = int.Parse(moves[0]);
                            if (moves[1] == "D" || moves[2] == "D")
                            {
                                active = false;
                                moveToX = 0;
                                moveToY = 0;
                            }
                            else
                            { 
                                moveToX = double.Parse(moves[1]);
                                moveToY = double.Parse(moves[2]);
                            }
                        }
                        catch(FormatException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, INVALID LOCATION)");
                            break;
                        }

                        Dressup.DressupPeice peice = room.GetDressupPeice(peiceId);
                        // Update database entries
                        peice.X = Convert.ToInt32(Math.Round(moveToX));
                        peice.Y = Convert.ToInt32(Math.Round(moveToY));
                        peice.Active = active;

                        // Forward to other users
                        byte[] movePeicePacket = PacketBuilder.CreateDressupRoomPeiceMove(peice.PeiceId, moveToX, moveToY, peice.Active);
                        User[] users = GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true);
                        foreach(User user in users)
                        {
                            if (user.Id != sender.LoggedinUser.Id)
                                user.LoggedinClient.SendPacket(movePeicePacket);
                        }
                    }
                    break;
                case PacketBuilder.SWFMODULE_BANDHALL:
                    byte[] response = PacketBuilder.CreateForwardedSwfRequest(packet);
                    foreach (User user in GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        if (user.Id == sender.LoggedinUser.Id)
                            continue;
                        user.LoggedinClient.SendPacket(response);
                    }
                    break;
                case PacketBuilder.SWFMODULE_2PLAYER: 
                    if(TwoPlayer.IsPlayerInGame(sender.LoggedinUser))
                    {
                        TwoPlayer twoPlayerGame = TwoPlayer.GetTwoPlayerGameInProgress(sender.LoggedinUser);

                        User otherUser = null;
                        if (twoPlayerGame.Invitee.Id == sender.LoggedinUser.Id)
                            otherUser = twoPlayerGame.Inviting;
                        else if (twoPlayerGame.Inviting.Id == sender.LoggedinUser.Id)
                            otherUser = twoPlayerGame.Invitee;

                        response = PacketBuilder.CreateForwardedSwfRequest(packet);
                        otherUser.LoggedinClient.SendPacket(response);
                    }
                    break;
                case PacketBuilder.SWFMODULE_CLOSE:
                    if (TwoPlayer.IsPlayerInGame(sender.LoggedinUser))
                    {
                        TwoPlayer twoPlayerGame = TwoPlayer.GetTwoPlayerGameInProgress(sender.LoggedinUser);

                        User otherUser = null;
                        if (twoPlayerGame.Invitee.Id == sender.LoggedinUser.Id)
                            otherUser = twoPlayerGame.Inviting;
                        else if (twoPlayerGame.Inviting.Id == sender.LoggedinUser.Id)
                            otherUser = twoPlayerGame.Invitee;

                        response = PacketBuilder.Create2PlayerClose();
                        otherUser.LoggedinClient.SendPacket(response);

                        twoPlayerGame.CloseGame(sender.LoggedinUser);

                        
                    }
                    break;
                case PacketBuilder.SWFMODULE_ARENA:
                    if (Arena.UserHasEnteredHorseInAnyArena(sender.LoggedinUser))
                    { 
                        Arena arena = Arena.GetArenaUserEnteredIn(sender.LoggedinUser);
                        response = PacketBuilder.CreateForwardedSwfRequest(packet);
                        foreach (Arena.ArenaEntry entry in arena.Entries.ToArray())
                        {
                            if (entry.EnteredUser.Id == sender.LoggedinUser.Id)
                                continue;
                            if(entry.EnteredUser.LoggedinClient.LoggedIn)
                            entry.EnteredUser.LoggedinClient.SendPacket(response);
                        }
                        
                    }
                    break;
                default:
                    Logger.DebugPrint("Unknown moduleid : " + module + " packet dump: " + BitConverter.ToString(packet).Replace("-"," "));
                    break;

            }

        }

        public static void OnWish(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " tried to wish when not logged in.");
                return;
            }

            if(packet.Length < 4)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid wish Packet");
                return;
            }

            if (!sender.LoggedinUser.Inventory.HasItemId(Item.WishingCoin))
            {
                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use a wishing well while having 0 coins.");
                return;
            }

            InventoryItem wishingCoinInvItems = sender.LoggedinUser.Inventory.GetItemByItemId(Item.WishingCoin);
            byte wishType = packet[1];
            string message = "";

            byte[] chatMsg = PacketBuilder.CreateChat(Messages.TossedCoin, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(chatMsg);

            switch(wishType)
            {
                case PacketBuilder.WISH_MONEY:
                    int gainMoney = RandomNumberGenerator.Next(500, 1000);
                    sender.LoggedinUser.Money += gainMoney;
                    message = Messages.FormatWishMoneyMessage(gainMoney);
                    break;
                case PacketBuilder.WISH_ITEMS:
                    Item.ItemInformation[] wishableItmes = Item.GetAllWishableItems();
                    int item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    Item.ItemInformation itm = wishableItmes[item];
                    item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    Item.ItemInformation itm2 = wishableItmes[item];

                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(itm.Id));
                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(itm2.Id));

                    message = Messages.FormatWishThingsMessage(itm.Name, itm2.Name);
                    break;
                case PacketBuilder.WISH_WORLDPEACE:
                    byte[] tooDeep = PacketBuilder.CreateChat(Messages.WorldPeaceOnlySoDeep, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(tooDeep);

                    wishableItmes = Item.GetAllWishableItems();
                    item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    int earnMoney = RandomNumberGenerator.Next(0, 500);
                    itm = wishableItmes[item];


                    sender.LoggedinUser.Money += earnMoney;
                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(itm.Id));

                    message = Messages.FormatWishWorldPeaceMessage(earnMoney, itm.Name);
                    break;
                default:
                    Logger.ErrorPrint("Unknnown Wish type: " + wishType.ToString("X"));
                    break;
            }
            sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count++;

            if(sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count >= 100)
                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(30)); // Well Wisher

            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count >= 1000)
                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(31)); // Star Wisher

            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count >= 10000)
                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(51)); // Extraordanary Wisher

            byte[] msg = PacketBuilder.CreateChat(message, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(msg);

            sender.LoggedinUser.Inventory.Remove(wishingCoinInvItems.ItemInstances[0]);
            UpdateArea(sender);
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
            if (method == PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                UpdateStats(sender);
            }
            if (method == PacketBuilder.VIEW_PROFILE)
            {
                sender.LoggedinUser.MetaPriority = true;
                string profilePage = sender.LoggedinUser.ProfilePage;
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
                sender.LoggedinUser.CharacterId = characterId;
                sender.LoggedinUser.ProfilePage = profilePage;

                Logger.DebugPrint(sender.LoggedinUser.Username + " Changed to character id: " + characterId + " and set there Profile Description to '" + profilePage + "'");

                byte[] chatPacket = PacketBuilder.CreateChat(Messages.ProfileSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatPacket);

                UpdateArea(sender);
                UpdateUserFacingAndLocation(sender.LoggedinUser);
            }
            else if (method == PacketBuilder.SECCODE_AWARD)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode AWARD request with invalid size");
                        return;
                    }

                    string packetStr = Encoding.UTF8.GetString(packet);
                    string awardIdStr = packetStr.Substring(6, packetStr.Length - 6 - 2);

                    int value = -1;
                    try
                    {
                        value = int.Parse(awardIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid awardid value");
                        return;
                    }

                    sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(value));
                    return;
                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_SCORE || method == PacketBuilder.SECCODE_TIME || method == PacketBuilder.SECCODE_WINLOOSE)
            {
                bool time = (method == PacketBuilder.SECCODE_TIME);
                bool winloose = (method == PacketBuilder.SECCODE_WINLOOSE);

                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode score/time/winloose request with invalid size");
                        return;
                    }

                    
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string gameInfoStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    if (winloose)
                    {
                        string gameTitle = gameInfoStr.Substring(1);
                        byte pmethod = packet[6];
                        if(pmethod == PacketBuilder.WINLOOSE_WIN)
                        {
                            sender.LoggedinUser.Highscores.Win(gameTitle);
                            byte[] winMsg = PacketBuilder.CreateChat(Messages.Format2PlayerRecordWin(gameTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(winMsg);
                        }
                        else if(pmethod == PacketBuilder.WINLOOSE_LOOSE)
                        {
                            sender.LoggedinUser.Highscores.Loose(gameTitle);
                            byte[] looseMsg = PacketBuilder.CreateChat(Messages.Format2PlayerRecordLose(gameTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(looseMsg);
                        }

                        if (sender.LoggedinUser.Highscores.HighscoreList.Length >= 30)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(12)); // Minigame Player

                        if (sender.LoggedinUser.Highscores.HighscoreList.Length >= 60)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(13)); // Minigame Master

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.LoggedinUser.Id) >= 1000)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(14)); // Minigame Nut

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.LoggedinUser.Id) >= 10000)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(15)); // Minigame Crazy
                        return;
                    }
                    if (gameInfoStr.Contains("|"))
                    {
                        string[] gameInfo = gameInfoStr.Split('|');
                        if (gameInfo.Length < 2)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a invalid seccode score request");
                            return;
                        }

                        string gameTitle = gameInfo[0];
                        string gameScoreStr = gameInfo[1];

                        int value = -1;
                        try
                        {
                            value = int.Parse(gameScoreStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid score value");
                            return;
                        }
                        Highscore.HighscoreTableEntry[] scores = Database.GetTopScores(gameTitle, 5);
                        bool bestScoreEver = false;
                        if (scores.Length >= 1)
                            bestScoreEver = scores[0].Score <= value;

                        bool newHighscore = sender.LoggedinUser.Highscores.UpdateHighscore(gameTitle, value, time);
                        if(bestScoreEver && !time)
                        {
                            byte[] bestScoreBeaten = PacketBuilder.CreateChat(Messages.BeatBestHighscore, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(bestScoreBeaten);
                            sender.LoggedinUser.Money += 2500;
                        }
                        else if (newHighscore)
                        {
                            if(time)
                            {
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatTimeBeatenMessage(value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                            }
                            else
                            {
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatHighscoreBeatenMessage(value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                            }

                        }
                        
                        if(sender.LoggedinUser.Highscores.HighscoreList.Length >= 30)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(12)); // Minigame Player

                        if (sender.LoggedinUser.Highscores.HighscoreList.Length >= 60)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(13)); // Minigame Master

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.LoggedinUser.Id) >= 1000)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(14)); // Minigame Nut

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.LoggedinUser.Id) >= 10000)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(15)); // Minigame Crazy

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " didnt send a game name AND a score.");
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
            else if (method == PacketBuilder.SECCODE_MONEY)
            {

                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode money request with invalid size");
                        return;
                    }

                    string packetStr = Encoding.UTF8.GetString(packet);
                    string gameInfoStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    if (gameInfoStr.Contains("|"))
                    {
                        string[] moneyInfo = gameInfoStr.Split('|');
                        if (moneyInfo.Length < 2)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a invalid money score request");
                            return;
                        }

                        string id = moneyInfo[0]; // not sure what this is for?

                        string moneyStr = moneyInfo[1];
                        int value = -1;
                        try
                        {
                            value = int.Parse(moneyStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid money value");
                            return;
                        }

                        int moneyEarned = value * 10;
                        Logger.InfoPrint(sender.LoggedinUser.Username + " Earned $" + moneyEarned + " In: " + id);

                        sender.LoggedinUser.Money += moneyEarned;
                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatMoneyEarnedMessage(moneyEarned), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " didnt send a game name AND a score.");
                        return;
                    }

                }
            }
            else if (method == PacketBuilder.SECCODE_GIVE_ITEM)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode item request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (Item.ItemIdExist(value))
                    {
                        ItemInstance itm = new ItemInstance(value);

                        Item.ItemInformation itemInfo = Item.GetItemById(value);
                        string messageToSend = Messages.FormatYouEarnedAnItemMessage(itemInfo.Name);
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(itm);
                        }
                        catch(InventoryException)
                        {
                            messageToSend = Messages.FormatYouEarnedAnItemButInventoryFullMessage(itemInfo.Name);
                        }

                        byte[] earnedItemMessage = PacketBuilder.CreateChat(messageToSend, PacketBuilder.CHAT_BOTTOM_RIGHT);
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
            else if (method == PacketBuilder.SECCODE_DELETE_ITEM)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode item request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (sender.LoggedinUser.Inventory.HasItemId(value))
                    {
                        InventoryItem item = sender.LoggedinUser.Inventory.GetItemByItemId(value);
                        sender.LoggedinUser.Inventory.Remove(item.ItemInstances[0]);

                        Item.ItemInformation itemInfo = Item.GetItemById(value);
                        byte[] lostItemMessage = PacketBuilder.CreateChat(Messages.FormatYouLostAnItemMessage(itemInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(lostItemMessage);

                        UpdateArea(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but tried to delete an non existant item");
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
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode quest request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
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
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.PROFILE_HIGHSCORES_LIST)
            {
                sender.LoggedinUser.MetaPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, packetStr.Length - 4);
                byte[] metaTag = PacketBuilder.CreateMetaPacket(Meta.BuildTopHighscores(gameName));
                sender.SendPacket(metaTag);
            }
            else if (method == PacketBuilder.PROFILE_BESTTIMES_LIST)
            {
                sender.LoggedinUser.MetaPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, packetStr.Length - 4);
                byte[] metaTag = PacketBuilder.CreateMetaPacket(Meta.BuildTopTimes(gameName));
                sender.SendPacket(metaTag);
            }
            else if (method == PacketBuilder.PROFILE_WINLOOSE_LIST)
            {
                sender.LoggedinUser.MetaPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, packetStr.Length - 4);
                byte[] metaTag = PacketBuilder.CreateMetaPacket(Meta.BuildTopWinners(gameName));
                sender.SendPacket(metaTag);
            }
            else
            {
                Logger.DebugPrint("Unknown Profile Packet! " + BitConverter.ToString(packet).Replace("-", " "));
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

            /*
             *  Player stuff
             */

            // Leave Multirooms 
            Multiroom.LeaveAllMultirooms(loggedInUser);

            // Cancel Trades
            if (loggedInUser.TradingWith != null)
                if((loggedInUser.TradingWith.Trader.X != loggedInUser.X) && (loggedInUser.TradingWith.Trader.Y != loggedInUser.Y))
                    loggedInUser.TradingWith.CancelTradeMoved();
            loggedInUser.PendingBuddyRequestTo = null;

            // Close Social Windows
            foreach (User sUser in loggedInUser.BeingSocializedBy.ToArray())
                UpdateArea(sUser.LoggedinClient);
            loggedInUser.BeingSocializedBy.Clear();


            // Pac-man the world.
            if (loggedInUser.X > Map.Width)
                loggedInUser.Teleport(2, loggedInUser.Y);
            else if (loggedInUser.X < 2)
                loggedInUser.Teleport(Map.Width-2, loggedInUser.Y);
            else if (loggedInUser.Y > Map.Height-2)
                loggedInUser.Teleport(loggedInUser.X, 2);
            else if (loggedInUser.Y < 2)
                loggedInUser.Teleport(loggedInUser.X, Map.Height-2);

            if (loggedInUser.CurrentlyRidingHorse != null)
            {
                if(loggedInUser.CurrentlyRidingHorse.BasicStats.Experience < 25)
                {
                    if(GameServer.RandomNumberGenerator.Next(0, 100) >= 97 || sender.LoggedinUser.Username.ToLower() == "dream")
                    {
                        loggedInUser.CurrentlyRidingHorse.BasicStats.Experience++;
                        byte[] horseBuckedMessage;
                        if(loggedInUser.CurrentlyRidingHorse.Breed.Type == "llama")
                            horseBuckedMessage = PacketBuilder.CreateChat(Messages.HorseLlamaBuckedYou, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        else if (loggedInUser.CurrentlyRidingHorse.Breed.Type == "camel")
                            horseBuckedMessage = PacketBuilder.CreateChat(Messages.HorseCamelBuckedYou, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        else
                            horseBuckedMessage = PacketBuilder.CreateChat(Messages.HorseBuckedYou, PacketBuilder.CHAT_BOTTOM_RIGHT);

                        sender.LoggedinUser.CurrentlyRidingHorse = null;
                        sender.LoggedinUser.Facing %= 5;
                        sender.SendPacket(horseBuckedMessage);
                    }
                }
            }

            // Randomly move if thirst, hunger, tiredness too low-

            byte movementDirection = packet[1];

            if (loggedInUser.Thirst <= 0 || loggedInUser.Hunger <= 0 || loggedInUser.Tiredness <= 0)
            {
                if (RandomNumberGenerator.Next(0, 10) == 7 || sender.LoggedinUser.Username.ToLower() == "dream")
                {
                    byte[] possibleDirections = new byte[] { PacketBuilder.MOVE_UP, PacketBuilder.MOVE_DOWN, PacketBuilder.MOVE_RIGHT, PacketBuilder.MOVE_LEFT };

                    if (possibleDirections.Contains(movementDirection))
                    {
                        byte newDirection = possibleDirections[RandomNumberGenerator.Next(0, possibleDirections.Length)];
                        if (newDirection != movementDirection)
                        {
                            movementDirection = newDirection;
                            if (loggedInUser.Thirst <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatThirst.ToUpper()), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }
                            else if (loggedInUser.Hunger <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatHunger.ToUpper()), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }
                            else if (loggedInUser.Tiredness <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatTired.ToUpper()), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }

                        }
                    }
                }
            }



            int onHorse = 0;
            int facing = sender.LoggedinUser.Facing;
            while (facing >= 5)
            {
                facing -= 5;
                onHorse++;
            }
            byte direction = 0;
            int newX = loggedInUser.X;
            int newY = loggedInUser.Y;
            bool moveTwo = false;

            if (movementDirection == PacketBuilder.MOVE_ESCAPE) // Exit this place / X Button
            {

                byte Direction;
                if (World.InSpecialTile(loggedInUser.X, loggedInUser.Y))
                {

                    World.SpecialTile tile = World.GetSpecialTile(loggedInUser.X, loggedInUser.Y);
                    if (tile.ExitX != 0)
                        newX = tile.ExitX;
                    if (tile.ExitY != 0)
                        newY = tile.ExitY;
                    else
                        if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1) || loggedInUser.NoClip)
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
                    if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1) || loggedInUser.NoClip)
                        loggedInUser.Y += 1;

                    Direction = PacketBuilder.DIRECTION_DOWN;
                }

                loggedInUser.Facing = Direction + (onHorse * 5);
                Logger.DebugPrint("Exiting player: " + loggedInUser.Username + " to: " + loggedInUser.X + "," + loggedInUser.Y);
                byte[] moveResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, Direction, true);
                sender.SendPacket(moveResponse);
                Update(sender);
                return;
            }

            if (movementDirection == PacketBuilder.MOVE_UP)
            {
                direction = PacketBuilder.DIRECTION_UP;
                if (Map.CheckPassable(newX, newY - 1) || loggedInUser.NoClip)
                    newY -= 1;
                

                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX, newY - 1) || loggedInUser.NoClip)
                    {
                        newY -= 1;
                        moveTwo = true;
                    }
            }
            else if (movementDirection == PacketBuilder.MOVE_LEFT)
            {
                direction = PacketBuilder.DIRECTION_LEFT;
                if (Map.CheckPassable(newX - 1, newY) || loggedInUser.NoClip)
                    newX -= 1;


                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX - 1, newY) || loggedInUser.NoClip)
                    {
                        newX -= 1;
                        moveTwo = true;
                    }
            }
            else if (movementDirection == PacketBuilder.MOVE_RIGHT)
            {
                direction = PacketBuilder.DIRECTION_RIGHT;
                if (Map.CheckPassable(newX + 1, newY) || loggedInUser.NoClip)
                    newX += 1;


                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX + 1, newY) || loggedInUser.NoClip)
                    {
                        newX += 1;
                        moveTwo = true;
                    }
            }
            else if (movementDirection == PacketBuilder.MOVE_DOWN)
            {
                direction = PacketBuilder.DIRECTION_DOWN;
                if (Map.CheckPassable(newX, newY + 1) || loggedInUser.NoClip)
                    newY += 1;


                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX, newY + 1) || loggedInUser.NoClip)
                    {
                        newY += 1;
                        moveTwo = true;
                    }
            }
            else if(movementDirection == PacketBuilder.MOVE_UPDATE)
            {
                UpdateArea(sender);
                return;
            }

            loggedInUser.Facing = direction + (onHorse * 5);
            if (loggedInUser.Y != newY || loggedInUser.X != newX)
            {
                if (moveTwo)
                    direction += 20;

                loggedInUser.Y = newY;
                loggedInUser.X = newX;

                // Check Treasures
                if (Treasure.IsTileTreasure(loggedInUser.X, loggedInUser.Y))
                {
                    Treasure treasure = Treasure.GetTreasureAt(loggedInUser.X, loggedInUser.Y);
                    if (treasure.Type == "RAINBOW")
                    {
                        treasure.CollectTreasure(loggedInUser);
                        Update(sender);
                        return;
                    }
                }

                byte[] moveResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, direction, true);
                sender.SendPacket(moveResponse);
            }
            else
            {
                byte[] moveResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_NONE, false);
                sender.SendPacket(moveResponse);
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
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to start talking to an NPC with id that is NaN.");
                    return;
                }
                
                if(!Npc.NpcExists(chatId))
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to start talking to an NPC that doesnt exist.");
                    return;
                }
                sender.LoggedinUser.MetaPriority = true;

                Npc.NpcEntry entry = Npc.GetNpcById(chatId);
                
                if(entry.Chatpoints.Length <= 0)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to start talking to an NPC with no chatpoints.");
                    return;
                }

                int defaultChatpointId = Npc.GetDefaultChatpoint(sender.LoggedinUser, entry);

                Npc.NpcChat startingChatpoint = Npc.GetNpcChatpoint(entry, defaultChatpointId);

                string metaInfo = Meta.BuildNpcChatpoint(sender.LoggedinUser, entry, startingChatpoint);
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
                catch (FormatException)
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
                string metaInfo = Meta.BuildNpcChatpoint(sender.LoggedinUser, lastNpc, Npc.GetNpcChatpoint(lastNpc, reply.GotoChatpoint));
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
            catch(FormatException)
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
                int cost = transportLocation.Cost;

                if (transportLocation.Type == "WAGON")
                {
                    if(sender.LoggedinUser.OwnedRanch != null)
                    {
                        if(sender.LoggedinUser.OwnedRanch.GetBuildingCount(7) > 0) // Wagon
                        {
                            cost = 0;
                        }
                    }
                }

                if (sender.LoggedinUser.Bids.Count > 0)
                {
                    byte[] cantBuyWhileAuctioning = PacketBuilder.CreateChat(Messages.AuctionNoOtherTransactionAllowed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantBuyWhileAuctioning);
                    return;
                }


                if (sender.LoggedinUser.Money >= cost)
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
                    sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count++;


                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count >= 500)
                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(27)); // Traveller
                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count >= 5000)
                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(28)); // Globetrotter

                    byte[] welcomeToIslePacket = PacketBuilder.CreateChat(Messages.FormatWelcomeToAreaMessage(transportLocation.LocationTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(welcomeToIslePacket);

                    if(cost > 0)
                        sender.LoggedinUser.Money -= cost;
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
        public static void OnRanchPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent ranch packet when not logged in.");
                return;
            }
            if (packet.Length < 4)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid ranch packet.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            byte method = packet[1];

            if (method == PacketBuilder.RANCH_INFO)
            {
                string buildingIdStr = packetStr.Substring(2, packetStr.Length - 4);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);

                    byte[] ranchBuild = PacketBuilder.CreateChat(Messages.FormatBuildingInformaton(building.Title, building.Description), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(ranchBuild);

                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_SELL)
            {
                string NanSTR = packetStr.Substring(2, packetStr.Length - 4);
                if (NanSTR == "NaN")
                {
                    if (sender.LoggedinUser.OwnedRanch == null)
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell there ranch when they didnt own one.");
                        return;
                    }
                    int sellPrice = sender.LoggedinUser.OwnedRanch.GetSellPrice();
                    sender.LoggedinUser.Money += sellPrice;
                    byte[] sellPacket = PacketBuilder.CreateChat(Messages.FormatRanchSoldMessage(sellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.LoggedinUser.OwnedRanch.OwnerId = -1;
                    sender.SendPacket(sellPacket);

                    // Change map sprite.
                    User[] users = GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true);
                    foreach (User user in users)
                    {
                        byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
                        user.LoggedinClient.SendPacket(MovementPacket);
                    }
                    UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to sell there ranch without sending NaN.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_UPGRADE)
            {
                string NanSTR = packetStr.Substring(2, packetStr.Length - 4);
                if (NanSTR == "NaN")
                {
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        Ranch.RanchUpgrade currentUpgrade = sender.LoggedinUser.OwnedRanch.GetRanchUpgrade();

                        if (!Ranch.RanchUpgrade.RanchUpgradeExists(currentUpgrade.Id + 1))
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to upgrade there ranch when it was max upgrade.");
                            return;
                        }

                        Ranch.RanchUpgrade nextUpgrade = Ranch.RanchUpgrade.GetRanchUpgradeById(currentUpgrade.Id + 1);
                        if (sender.LoggedinUser.Money >= nextUpgrade.Cost)
                        {
                            sender.LoggedinUser.Money -= nextUpgrade.Cost;
                            sender.LoggedinUser.OwnedRanch.InvestedMoney += nextUpgrade.Cost;
                            sender.LoggedinUser.OwnedRanch.UpgradedLevel++;

                            byte[] upgraded = PacketBuilder.CreateChat(Messages.UpgradedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(upgraded);

                            // Change map sprite.
                            User[] users = GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true);
                            foreach (User user in users)
                            {
                                byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
                                user.LoggedinClient.SendPacket(MovementPacket);
                            }
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                        }
                        else
                        {
                            byte[] cantAfford = PacketBuilder.CreateChat(Messages.UpgradeCannotAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAfford);
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to upgrade there ranch when they didnt own one.");
                    }
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to upgrade there ranch without sending NaN.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_REMOVE)
            {
                string buildingIdStr = packetStr.Substring(2, packetStr.Length - 4);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);
                    int ranchBuild = sender.LoggedinUser.LastClickedRanchBuilding;
                    if (ranchBuild == 0)
                        return;
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        if (ranchBuild > sender.LoggedinUser.OwnedRanch.GetRanchUpgrade().Limit)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to remove more buildings than the limit.");
                            return;
                        }
                        Ranch.RanchBuilding ranchBuilding = sender.LoggedinUser.OwnedRanch.GetBuilding(ranchBuild - 1);
                        if (ranchBuilding.Id == buildingId)
                        {
                            sender.LoggedinUser.OwnedRanch.SetBuilding(ranchBuild - 1, null);
                            sender.LoggedinUser.Money += ranchBuilding.GetTeardownPrice();
                            sender.LoggedinUser.OwnedRanch.InvestedMoney -= building.Cost;
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatBuildingTornDown(ranchBuilding.GetTeardownPrice()), PacketBuilder.CHAT_BOTTOM_RIGHT);

                            sender.SendPacket(chatPacket);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                            return;
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to remove bulidingid: " + buildingId + " from building slot " + ranchBuild + " but the building was not found there.");
                        }

                    }
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to remove in a ranch when they dont own one.");
                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_BUILD)
            {
                string buildingIdStr = packetStr.Substring(2, packetStr.Length - 4);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);
                    int ranchBuild = sender.LoggedinUser.LastClickedRanchBuilding;
                    if (ranchBuild == 0)
                        return;
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        if (ranchBuild > sender.LoggedinUser.OwnedRanch.GetRanchUpgrade().Limit)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to build more buildings than the limit.");
                            return;
                        }

                        if (sender.LoggedinUser.Money >= building.Cost)
                        {
                            sender.LoggedinUser.OwnedRanch.SetBuilding(ranchBuild - 1, building);
                            sender.LoggedinUser.OwnedRanch.InvestedMoney += building.Cost;
                            sender.LoggedinUser.Money -= building.Cost;
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.RanchBuildingComplete, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                            return;

                        }
                        else
                        {
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.RanchCantAffordThisBuilding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                            return;
                        }
                    }
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to build in a ranch when they dont own one.");
                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_BUY)
            {
                string nan = packetStr.Substring(2, packetStr.Length - 4);
                if (nan == "NaN")
                {
                    if (Ranch.IsRanchHere(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (sender.LoggedinUser.Money >= ranch.Value)
                        {
                            byte[] broughtRanch = PacketBuilder.CreateChat(Messages.FormatRanchBroughtMessage(ranch.Value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(broughtRanch);
                            sender.LoggedinUser.Money -= ranch.Value;
                            ranch.OwnerId = sender.LoggedinUser.Id;
                            ranch.InvestedMoney += ranch.Value;
                            sender.LoggedinUser.OwnedRanch = ranch;
                            sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(Item.DorothyShoes));
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);

                        }
                        else
                        {
                            byte[] cantAfford = PacketBuilder.CreateChat(Messages.RanchCantAffordRanch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAfford);
                        }
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to buy a non existant ranch.");
                        return;
                    }
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent RANCH_BUY without \"NaN\".");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_CLICK)
            {
                if (packet.Length < 6)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid ranch click packet.");
                    return;
                }
                byte action = packet[2];
                if (action == PacketBuilder.RANCH_CLICK_BUILD)
                {
                    if (Ranch.IsRanchHere(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (sender.LoggedinUser.OwnedRanch != null)
                        {
                            if (sender.LoggedinUser.OwnedRanch.Id == ranch.Id)
                            {
                                int buildSlot = packet[3] - 40;
                                sender.LoggedinUser.LastClickedRanchBuilding = buildSlot;
                                sender.LoggedinUser.MetaPriority = true;

                                if (buildSlot == 0)
                                {
                                    byte[] buildingsAvalible = PacketBuilder.CreateMetaPacket(Meta.BuildRanchUpgrade(ranch));
                                    sender.SendPacket(buildingsAvalible);

                                }
                                else
                                {
                                    byte[] buildingsAvalible = PacketBuilder.CreateMetaPacket(Meta.BuildRanchBuildingsAvalible(ranch, buildSlot));
                                    sender.SendPacket(buildingsAvalible);
                                }


                                return;
                            }
                        }
                    }

                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to build in a ranch they didnt own.");
                    return;
                }
                else if (action == PacketBuilder.RANCH_CLICK_NORM)
                {
                    if (Ranch.IsRanchHere(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        int buildSlot = packet[3] - 40;
                        sender.LoggedinUser.MetaPriority = true;

                        if (buildSlot == 0) // Main Building
                        {
                            byte[] upgradeDescription = PacketBuilder.CreateMetaPacket(Meta.BuildRanchBuilding(ranch, ranch.GetRanchUpgrade()));
                            sender.SendPacket(upgradeDescription);
                        }
                        else // Other Building
                        {
                            byte[] buildingDescription = PacketBuilder.CreateMetaPacket(Meta.BuildRanchBuilding(ranch, ranch.GetBuilding(buildSlot - 1)));
                            sender.SendPacket(buildingDescription);
                        }
                        return;
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " sent an Unknown ranch packet " + BitConverter.ToString(packet).Replace("-", " "));
                    }
                }
            }
            else
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " sent an Unknown ranch packet " + BitConverter.ToString(packet).Replace("-", " "));
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

            
           

            Logger.DebugPrint(sender.LoggedinUser.Username + " Attempting to say '" + message + "' in channel: " + channel.ToString());

            string nameTo = null;
            if (channel == Chat.ChatChannel.Dm)
            {
                nameTo = Chat.GetDmRecipiant(message);
                message = Chat.GetDmMessage(message);
            }

            if (message == "")
                return;
            
            if(message.StartsWith("/"))
            {
                string channelString = message.Split(' ')[0].ToLower();
                string newMessage = string.Join(' ', message.Split(' ').Skip(1));
                message = newMessage;
                switch(channelString)
                {
                    case "/$":
                    case "/ads":
                        channel = Chat.ChatChannel.Ads;
                        break;
                    case "/all":
                        channel = Chat.ChatChannel.All;
                        break;
                    case "/here":
                        channel = Chat.ChatChannel.Here;
                        break;
                    case "/near":
                        channel = Chat.ChatChannel.Near;
                        break;
                    case "/buddy":
                        channel = Chat.ChatChannel.Buddies;
                        break;
                    case "/island":
                        channel = Chat.ChatChannel.Isle;
                        break;
                    case "/admin":
                        if (sender.LoggedinUser.Administrator)
                            channel = Chat.ChatChannel.Admin;
                        else
                            return;
                        break;
                    case "/mod":
                        if (sender.LoggedinUser.Moderator)
                            channel = Chat.ChatChannel.Mod;
                        else
                            return;
                        break;
                    default:
                        channel = Chat.ChatChannel.Dm;
                        nameTo = channelString.Substring(1);
                        break;
                }
            }

            if (Chat.ProcessCommand(sender.LoggedinUser, message))
            {
                Logger.DebugPrint(sender.LoggedinUser.Username + " Attempting to run command '" + message + "' in channel: " + channel.ToString());
                return;
            }

            // Check events
            if (RiddleEvent.Active) 
                if(RiddleEvent.CheckRiddle(message))
                    RiddleEvent.Win(sender.LoggedinUser);
                
           
            // Check if player is muting channel

            if( (sender.LoggedinUser.MuteGlobal && channel == Chat.ChatChannel.All) || (sender.LoggedinUser.MuteAds && channel == Chat.ChatChannel.Ads) || (sender.LoggedinUser.MuteHere && channel == Chat.ChatChannel.Here) && (sender.LoggedinUser.MuteBuddy && channel == Chat.ChatChannel.Buddies) && (sender.LoggedinUser.MuteNear && channel == Chat.ChatChannel.Near) && (sender.LoggedinUser.MuteIsland && channel == Chat.ChatChannel.Isle))
            {
                byte[] cantSendMessage = PacketBuilder.CreateChat(Messages.CantSendInMutedChannel, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(cantSendMessage);
                return;
            }

            if(sender.LoggedinUser.MutePrivateMessage && channel == Chat.ChatChannel.Dm)
            {
                byte[] cantSendDmMessage = PacketBuilder.CreateChat(Messages.CantSendPrivateMessageWhileMuted, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(cantSendDmMessage);
                return;
            }

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

            // Spam Protection

            if(channel == Chat.ChatChannel.Dm)
            {
                try
                {
                    nameTo = GetUserByName(nameTo).Username;
                }
                catch(KeyNotFoundException)
                {
                    byte[] cantFindPlayer = PacketBuilder.CreateChat(Messages.CantFindPlayerToPrivateMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantFindPlayer);

                    return;
                }
            }
            else if(channel == Chat.ChatChannel.Ads)
            {
                if(!sender.LoggedinUser.CanUseAdsChat)
                {
                    byte[] cantSendInAds = PacketBuilder.CreateChat(Messages.AdsOnlyOncePerMinute, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantSendInAds);

                    return;
                }
                sender.LoggedinUser.CanUseAdsChat = false;
            }
            else if(channel == Chat.ChatChannel.All)
            {
                if(sender.LoggedinUser.TotalGlobalChatMessages <= 0)
                {
                    byte[] globalLimited = PacketBuilder.CreateChat(Messages.GlobalChatLimited, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(globalLimited);

                    return;
                }
                sender.LoggedinUser.TotalGlobalChatMessages--;
            }

            // Muted user checks
            if(channel == Chat.ChatChannel.Dm) 
            {
                try
                {
                    User userTo = GetUserByName(nameTo);
                    if (sender.LoggedinUser.MutePlayer.IsUserMuted(userTo))
                    {
                        byte[] dmWasBlocked = PacketBuilder.CreateChat(Messages.FormatCantSendYourIgnoringPlayer(userTo.Username), PacketBuilder.CHAT_DM_RIGHT);
                        sender.SendPacket(dmWasBlocked);
                        return;
                    }
                    else if (userTo.MutePrivateMessage)
                    {
                        byte[] dmWasBlocked = PacketBuilder.CreateChat(Messages.FormatPlayerIgnoringAllPms(userTo.Username), PacketBuilder.CHAT_DM_RIGHT);
                        sender.SendPacket(dmWasBlocked);
                        return;
                    }
                    else if (userTo.MutePlayer.IsUserMuted(sender.LoggedinUser))
                    {
                        byte[] dmWasBlocked = PacketBuilder.CreateChat(Messages.FormatPlayerIgnoringYourPms(userTo.Username), PacketBuilder.CHAT_DM_RIGHT);
                        sender.SendPacket(dmWasBlocked);
                        return;
                    }
                }
                catch (KeyNotFoundException)
                {
                    return;
                }
            }    
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

                Logger.DebugPrint(sender.LoggedinUser.Username + " Clicked on tile: " + Map.GetTileId(x, y, false).ToString() + "(overlay: " + Map.GetTileId(x, y, true).ToString() + ") at " + x.ToString() + "," + y.ToString());


                // Get description of tile 
                string returnedMsg = Messages.NothingInterestingHere;
                if(World.InSpecialTile(x, y))
                {
                    World.SpecialTile tile = World.GetSpecialTile(x, y);
                    if (tile.Title != null)
                        returnedMsg = tile.Title;
                }
                if(Ranch.IsRanchHere(x, y)) // Ranch here?
                {
                    Ranch ranch = Ranch.GetRanchAt(x, y);
                    if(ranch.OwnerId == -1)
                    {
                        returnedMsg = Messages.RanchUnownedRanchClicked;
                    }
                    else
                    {
                        string title = ranch.Title;
                        if (title == null || title == "")
                            title = Messages.RanchDefaultRanchTitle;
                        returnedMsg = Messages.FormatRanchClickMessage(Database.GetUsername(ranch.OwnerId), title);
                    }
                }
                if(GetUsersAt(x,y, false, true).Length > 0) // Player here?
                {
                    returnedMsg = Messages.FormatPlayerHereMessage(GetUsersAt(x, y, false, true)[0].Username);
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

                    foreach (DroppedItems.DroppedItem item in droppedItems)
                    {
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(item.Instance);
                            DroppedItems.RemoveDroppedItem(item);
                        }
                        catch (InventoryException)
                        {
                            chatMsg = Messages.GrabbedAllItemsButInventoryFull;
                        }
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
                    catch(FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    try
                    {
                        DroppedItems.DroppedItem item = DroppedItems.GetDroppedItemById(randomId);
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(item.Instance);
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
                case PacketBuilder.ITEM_REMOVE:
                    char toRemove = (char)packet[2];
                    switch(toRemove)
                    {
                        case '1':
                            if(sender.LoggedinUser.EquipedCompetitionGear.Head != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Head.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Head = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '2':
                            if (sender.LoggedinUser.EquipedCompetitionGear.Body != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Body.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Body = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '3':
                            if (sender.LoggedinUser.EquipedCompetitionGear.Legs != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Legs.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Legs = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '4':
                            if (sender.LoggedinUser.EquipedCompetitionGear.Feet != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Feet.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Feet = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '5':
                            if (sender.LoggedinUser.EquipedJewelry.Slot1 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot1.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot1 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '6':
                            if (sender.LoggedinUser.EquipedJewelry.Slot2 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot2.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot2 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '7':
                            if (sender.LoggedinUser.EquipedJewelry.Slot3 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot3.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot3 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '8':
                            if (sender.LoggedinUser.EquipedJewelry.Slot4 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot4.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot4 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        default:
                            Logger.InfoPrint(sender.LoggedinUser.Username + "Unimplemented  \"remove worn item\" ItemInteraction packet: " + BitConverter.ToString(packet).Replace("-", " "));
                            break;
                    }

                    UpdateStats(sender);
                    if(toRemove >= '1' && toRemove <= '4')
                    {
                        byte[] itemRemovedMessage = PacketBuilder.CreateChat(Messages.RemoveCompetitionGear, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemRemovedMessage);
                    }
                    else if (toRemove >= '5' && toRemove <= '8')
                    {
                        byte[] itemRemovedMessage = PacketBuilder.CreateChat(Messages.RemoveJewelry, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemRemovedMessage);
                    }
                    
                    break;
                case PacketBuilder.ITEM_THROW:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string itemidStr = packetStr.Substring(2, packet.Length - 2);
                    int itemId = 0;

                    try
                    {
                        itemId = Int32.Parse(itemidStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet. (THROW)");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItemId(itemId))
                    {
                        if (!Item.IsThrowable(itemId))
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to throw an item that isnt throwable.");
                            return;
                        }

                        ItemInstance curItem = sender.LoggedinUser.Inventory.GetItemByItemId(itemId).ItemInstances[0];
                        User[] userAt = GetReallyNearbyUsers(sender.LoggedinUser.X, sender.LoggedinUser.Y);

                        while (true)
                        {
                            int userIndx = RandomNumberGenerator.Next(0, userAt.Length);

                            if (userAt.Length > 1)
                                if (userAt[userIndx].Id == sender.LoggedinUser.Id)
                                    continue;

                            Item.ThrowableItem throwableItem = Item.GetThrowableItem(curItem.ItemId);

                            if (userAt[userIndx].Id == sender.LoggedinUser.Id)
                            {
                                byte[] thrownHitYourself = PacketBuilder.CreateChat(throwableItem.HitYourselfMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(thrownHitYourself);
                                break;
                            }
                            if(itemId == Item.WaterBalloon)
                            {
                                if (WaterBalloonEvent != null)
                                    if (WaterBalloonEvent.Active)
                                        WaterBalloonEvent.AddWaterBallon(userAt[userIndx]);
                            }
                            if(itemId == Item.ModSplatterball)
                            {
                                ModsRevengeEvent.Payout(sender.LoggedinUser, userAt[userIndx]);
                            }

                            byte[] thrownForYou = PacketBuilder.CreateChat(Messages.FormatThrownItemMessage(throwableItem.ThrowMessage, userAt[userIndx].Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            byte[] thrownForOthers = PacketBuilder.CreateChat(Messages.FormatThrownItemMessage(throwableItem.HitMessage, sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

                            sender.SendPacket(thrownForYou);
                            userAt[userIndx].LoggedinClient.SendPacket(thrownForOthers);
                            
                            break;
                        }

                        sender.LoggedinUser.Inventory.Remove(curItem);
                        UpdateInventory(sender);
                        
                    }
                    break;
                case PacketBuilder.ITEM_WRAP:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        ItemInstance curItem = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId).ItemInstances[0];
                        ItemInstance wrappedItem = new ItemInstance(Item.Present, -1, curItem.ItemId);

                        try
                        {
                            sender.LoggedinUser.Inventory.Add(wrappedItem);
                            sender.LoggedinUser.Inventory.Remove(curItem);
                        }
                        catch(InventoryException)
                        {
                            byte[] cantWrapPresent = PacketBuilder.CreateChat(Messages.SantaCantWrapInvFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantWrapPresent);
                            UpdateArea(sender);
                            break;
                        }
                    }
                    byte[] wrappedObjectMessage = PacketBuilder.CreateChat(Messages.SantaWrappedObjectMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(wrappedObjectMessage);
                    UpdateArea(sender);
                    break;
                case PacketBuilder.ITEM_OPEN:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem item = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        int newItem = item.ItemInstances[0].Data;
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(new ItemInstance(newItem));
                            sender.LoggedinUser.Inventory.Remove(item.ItemInstances[0]);
                        }
                        catch(InventoryException)
                        {
                            byte[] cantOpenInvFull = PacketBuilder.CreateChat(Messages.SantaItemCantOpenInvFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantOpenInvFull);
                            break;
                        }
                        byte[] itemOpened = PacketBuilder.CreateChat(Messages.FormatSantaOpenPresent(Item.GetItemById(newItem).Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemOpened);
                        UpdateInventory(sender);
                    }
                    break;
                case PacketBuilder.ITEM_USE:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        if(itm.ItemId == Item.DorothyShoes)
                        {
                            if(World.InIsle(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                            {
                                World.Isle isle = World.GetIsle(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                                if(isle.Name == "Prison Isle")
                                {
                                    byte[] dontWorkHere = PacketBuilder.CreateChat(Messages.RanchDorothyShoesPrisonIsleMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(dontWorkHere);
                                    break;
                                }
                            }

                            if(sender.LoggedinUser.OwnedRanch == null) // How????
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use Dorothy Shoes when they did *NOT* own a ranch.");
                                sender.LoggedinUser.Inventory.Remove(itm.ItemInstances[0]);
                                break;
                            }
                            byte[] noPlaceLIke127001 = PacketBuilder.CreateChat(Messages.RanchDorothyShoesMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(noPlaceLIke127001);

                            sender.LoggedinUser.Teleport(sender.LoggedinUser.OwnedRanch.X, sender.LoggedinUser.OwnedRanch.Y);
                        }
                        else if(itm.ItemId == Item.Telescope)
                        {
                            byte[] birdMap = PacketBuilder.CreateBirdMap(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            sender.SendPacket(birdMap);
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + "Tried to use item with undefined action- ID: " + itm.ItemId);
                        }
                    }
                    break;
                case PacketBuilder.ITEM_WEAR:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                     
                        Item.ItemInformation itemInf = instance.GetItemInfo();
                        if(itemInf.Type == "CLOTHES")
                        {
                            switch (itemInf.GetMiscFlag(0))
                            {
                                case CompetitionGear.MISC_FLAG_HEAD:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Head == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Head = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Head.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Head = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_BODY:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Body == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Body = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Body.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Body = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_LEGS:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Legs == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Legs = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Legs.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Legs = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_FEET:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Feet == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Feet = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Feet.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Feet = itemInf;
                                    }
                                    break;
                                default: 
                                    Logger.ErrorPrint(itemInf.Name + " Has unknown misc flags.");
                                    return;
                            }
                            sender.LoggedinUser.Inventory.Remove(instance);
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatEquipCompetitionGearMessage(itemInf.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                        }
                        else if(itemInf.Type == "JEWELRY")
                        {
                            bool addedJewelry = false;
                            if (sender.LoggedinUser.EquipedJewelry.Slot1 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot1 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.LoggedinUser.EquipedJewelry.Slot2 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot2 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.LoggedinUser.EquipedJewelry.Slot3 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot3 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.LoggedinUser.EquipedJewelry.Slot4 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot4 = itemInf;
                                addedJewelry = true;
                            }

                            if(addedJewelry)
                            {
                                sender.LoggedinUser.Inventory.Remove(instance);
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatJewerlyEquipMessage(itemInf.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                            }
                            else
                            {
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.MaxJewelryMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                            }
                        }

                        UpdateInventory(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to wear an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_DRINK:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string idStr = packetStr.Substring(2, packet.Length - 4);
                    if(idStr == "NaN") // Fountain
                    {
                        string msg = Messages.FountainDrankYourFull;
                        bool looseMoney = RandomNumberGenerator.Next(0, 20) == 18;
                        if(looseMoney)
                        {
                            int looseAmount = RandomNumberGenerator.Next(0, 100);
                            if (looseAmount > sender.LoggedinUser.Money)
                                looseAmount = sender.LoggedinUser.Money;
                            sender.LoggedinUser.Money -= looseAmount;
                            msg = Messages.FormatDroppedMoneyMessage(looseAmount);
                        }

                        sender.LoggedinUser.Thirst = 1000;
                        byte[] drankFromFountainMessage = PacketBuilder.CreateChat(msg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(drankFromFountainMessage);
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + "Sent unknown ITEM_DRINK command id: " + idStr);
                    }
                    break;
                case PacketBuilder.ITEM_CONSUME:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 3);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        sender.LoggedinUser.Inventory.Remove(instance);
                        Item.ItemInformation itmInfo = instance.GetItemInfo();
                        bool toMuch = Item.ConsumeItem(sender.LoggedinUser, itmInfo);

                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatConsumeItemMessaege(itmInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);
                        if (toMuch)
                        {
                            chatPacket = PacketBuilder.CreateChat(Messages.ConsumedButMaxReached, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                        }

                        UpdateInventory(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to consume an item they did not have.");
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
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    if(sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        if(DroppedItems.GetItemsAt(sender.LoggedinUser.X, sender.LoggedinUser.Y).Length > 25)
                        {
                            byte[] tileIsFullPacket = PacketBuilder.CreateChat(Messages.DroppedItemTileIsFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(tileIsFullPacket);
                            break;
                        }
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
                case PacketBuilder.ITEM_CRAFT:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string craftIdStr = packetStr.Substring(2, packet.Length - 2);
                    int craftId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        craftId = Int32.Parse(craftIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to craft using craft id NaN.");
                        return;
                    }
                    if(Workshop.CraftIdExists(craftId))
                    {
                        Workshop.CraftableItem itm = Workshop.GetCraftId(craftId);
                        if(itm.MoneyCost <= sender.LoggedinUser.Money) // Check money
                        {
                            foreach(Workshop.RequiredItem reqItem in itm.RequiredItems)
                            {
                                if (sender.LoggedinUser.Inventory.HasItemId(reqItem.RequiredItemId))
                                {
                                    if (sender.LoggedinUser.Inventory.GetItemByItemId(reqItem.RequiredItemId).ItemInstances.Count < reqItem.RequiredItemCount)
                                        goto failMissingItem;
                                }
                                else
                                    goto failMissingItem;
                            }

                            // Finally create the items
                            try
                            {
                                sender.LoggedinUser.Inventory.Add(new ItemInstance(itm.GiveItemId));
                            }
                            catch(InventoryException)
                            {
                                byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.WorkshopNoRoomInInventory, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(inventoryFullMessage);
                                break;
                            }
                            sender.LoggedinUser.Money -= itm.MoneyCost;

                            // Remove the required items..
                            foreach(Workshop.RequiredItem reqItem in itm.RequiredItems) 
                                for(int i = 0; i < reqItem.RequiredItemCount; i++)
                                    sender.LoggedinUser.Inventory.Remove(sender.LoggedinUser.Inventory.GetItemByItemId(reqItem.RequiredItemId).ItemInstances[0]);

                            sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count++;

                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count >= 100)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(22)); // Craftiness
                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count >= 1000)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(23)); // Workmanship

                            byte[] itemCraftSuccess = PacketBuilder.CreateChat(Messages.WorkshopCraftingSuccess, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(itemCraftSuccess);
                            break;
                            
                        }
                        else
                        {
                            byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.WorkshopCannotAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAffordMessage);
                            break;
                        }

                        failMissingItem:
                        {
                            byte[] missingItemMessage = PacketBuilder.CreateChat(Messages.WorkshopMissingRequiredItem, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(missingItemMessage);
                            break;
                        }
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
                    catch (FormatException)
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
                    itemId = invItem.ItemId;
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
                    catch (FormatException)
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

                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
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

                case PacketBuilder.ITEM_BUY_AND_CONSUME:
                    packetStr = Encoding.UTF8.GetString(packet);
                    itemIdStr = packetStr.Substring(2, packet.Length - 3);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object buy and consume packet.");
                        return;
                    }
                    if (!Item.ItemIdExist(itemId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy an itemid that doesnt even exist.");
                        break;
                    }

                    Inn lastInn = sender.LoggedinUser.LastVisitedInn;
                    if (lastInn != null)
                    {
                        try
                        {
                            itemInfo = lastInn.GetStockedItem(itemId);
                            int price = lastInn.CalculateBuyCost(itemInfo);
                            if(sender.LoggedinUser.Money >= price)
                            {
                                sender.LoggedinUser.Money -= price;
                                bool toMuch = Item.ConsumeItem(sender.LoggedinUser, itemInfo);

                                string tooMuchMessage = Messages.ConsumedButMaxReached;
                                if (itemInfo.Effects.Length > 0)
                                    if (itemInfo.Effects[0].EffectsWhat == "TIREDNESS")
                                        tooMuchMessage = Messages.InnFullyRested;
                                if (itemInfo.Effects.Length > 1)
                                    if (itemInfo.Effects[1].EffectsWhat == "TIREDNESS")
                                        tooMuchMessage = Messages.InnFullyRested;

                                byte[] enjoyedServiceMessage = PacketBuilder.CreateChat(Messages.FormatInnEnjoyedServiceMessage(itemInfo.Name, price), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(enjoyedServiceMessage);

                                if(toMuch)
                                {
                                    byte[] toMuchMessage = PacketBuilder.CreateChat(tooMuchMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(toMuchMessage);
                                }

                                UpdateArea(sender);
                            }
                            else
                            {
                                byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.InnCannotAffordService, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cantAffordMessage);
                            }
                        }
                        catch(KeyNotFoundException)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy and consume an item not stocked by the inn there standing on.");
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy and consume item while not in a inn.");
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
                    itemIdStr = packetStr.Substring(2, packet.Length - 3);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object buy packet.");
                        return;
                    }

                    if(!Item.ItemIdExist(itemId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy an itemid that doesnt even exist.");
                        break;
                    }

                    itemInfo = Item.GetItemById(itemId);
                    shop = sender.LoggedinUser.LastShoppedAt;
                    if (shop != null)
                    {
                        int buyCost = shop.CalculateBuyCost(itemInfo) * count;
                        if (sender.LoggedinUser.Bids.Count > 0)
                        {
                            byte[] cantBuyWhileAuctioning = PacketBuilder.CreateChat(Messages.AuctionNoOtherTransactionAllowed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantBuyWhileAuctioning);
                            return;
                        }

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
                            else if(sender.LoggedinUser.Inventory.Count + 1 > sender.LoggedinUser.MaxItems)
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
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
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
                case PacketBuilder.ITEM_RIP:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;
                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    if (!sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to rip someone elses mail. " + randomId.ToString());
                        return;
                    }

                    InventoryItem ripItems = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                    foreach (ItemInstance item in ripItems.ItemInstances)
                    {
                        if (item.RandomId == randomId)
                        {
                            if (item.Data == 0)
                                continue;
                            sender.LoggedinUser.MailBox.RipUpMessage(sender.LoggedinUser.MailBox.GetMessageByRandomId(item.Data));
                            break;
                        }
                    }
                    break;
                case PacketBuilder.ITEM_VIEW:
                    byte method = packet[2];
                    if (method == PacketBuilder.ITEM_LOOK)
                    {
                        packetStr = Encoding.UTF8.GetString(packet);
                        itemIdStr = packetStr.Substring(3, packet.Length - 3);
                        itemId = 0;
                        try
                        {
                            itemId = Int32.Parse(itemIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                            return;
                        }

                        if (itemId == Item.MailMessage)
                        {
                            if (!sender.LoggedinUser.Inventory.HasItemId(Item.MailMessage))
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to view a mail message when they didnt have one.");
                                return;
                            }

                            sender.LoggedinUser.MetaPriority = true;
                            byte[] mailList = PacketBuilder.CreateMetaPacket(Meta.BuildMailList(sender.LoggedinUser, sender.LoggedinUser.Inventory.GetItemByItemId(Item.MailMessage)));
                            sender.SendPacket(mailList);
                            break;
                        }
                    }
                    else if(method == PacketBuilder.ITEM_READ)
                    {
                        packetStr = Encoding.UTF8.GetString(packet);
                        randomIdStr = packetStr.Substring(3, packet.Length - 3);
                        randomId = 0;
                        try
                        {
                            randomId = Int32.Parse(randomIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                            return;
                        }

                        if (!sender.LoggedinUser.Inventory.HasItem(randomId))
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to view someone elses mail. " + randomId.ToString());
                            return;
                        }

                        InventoryItem items = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        foreach (ItemInstance item in items.ItemInstances)
                        {
                            if (item.RandomId == randomId)
                            {
                                if (item.Data == 0)
                                    continue;

                                sender.LoggedinUser.MetaPriority = true;
                                byte[] readMail = PacketBuilder.CreateMetaPacket(Meta.BuildMailLetter(sender.LoggedinUser.MailBox.GetMessageByRandomId(item.Data), randomId));
                                sender.SendPacket(readMail);
                                break;
                            }
                        }
                        break;

                    }


                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Unknown Method- " + method.ToString("X") + "  " + BitConverter.ToString(packet).Replace("-", " "));
                    break;
                case PacketBuilder.PACKET_INFORMATION:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string valueStr = packetStr.Substring(3, packet.Length - 3);
                    int value = 0;
                    try
                    {
                        value = Int32.Parse(valueStr);
                    }
                    catch (FormatException)
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
                            itemId = DroppedItems.GetDroppedItemById(value).Instance.ItemId;
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

                    if(Database.IsUserBanned(userId))
                    {
                        Logger.DebugPrint(sender.RemoteIp + " Tried to login to : " + username + " but, the account was banned.");
                        byte[] userBannedPacket = PacketBuilder.CreateLoginPacket(false, Messages.LoginFailedReasonBanned);
                        sender.SendPacket(userBannedPacket);
                        return;
                    }

                    if(Database.IsIpBanned(sender.RemoteIp))
                    {
                        Logger.DebugPrint(sender.RemoteIp + " Tried to login to : " + username + " but, the IP was banned.");
                        byte[] ipBannedPacket = PacketBuilder.CreateLoginPacket(false, Messages.FormatIpBannedMessage(sender.RemoteIp));
                        sender.SendPacket(ipBannedPacket);
                        return;
                    }


                    sender.Login(userId);
                    sender.LoggedinUser.Password = password;

                    byte[] ResponsePacket = PacketBuilder.CreateLoginPacket(true);
                    sender.SendPacket(ResponsePacket);

                    Logger.DebugPrint(sender.RemoteIp + " Logged into : " + sender.LoggedinUser.Username + " (ADMIN: " + sender.LoggedinUser.Administrator + " MOD: " + sender.LoggedinUser.Moderator + ")");

                    // Send login message
                    byte[] loginMessageBytes = PacketBuilder.CreateChat(Messages.FormatLoginMessage(sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
                    foreach (GameClient client in ConnectedClients)
                        if (client.LoggedIn)
                            if (!client.LoggedinUser.MuteLogins && !client.LoggedinUser.MuteAll)
                                if (client.LoggedinUser.Id != userId)
                                        client.SendPacket(loginMessageBytes);

                    UpdateUserFacingAndLocation(sender.LoggedinUser);

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
            if (sender.LoggedIn)
            {
                Database.SetPlayerLastLogin(Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()), sender.LoggedinUser.Id); // Set last login date
                Database.RemoveOnlineUser(sender.LoggedinUser.Id);

                // Leave multirooms
                Multiroom.LeaveAllMultirooms(sender.LoggedinUser);
                TwoPlayer.TwoPlayerRemove(sender.LoggedinUser);

                // Remove Trade Reference
                sender.LoggedinUser.TradingWith = null;
                sender.LoggedinUser.PendingTradeTo = 0;

                // Leave open water balloon game
                if (WaterBalloonEvent != null)
                    if(WaterBalloonEvent.Active)
                        WaterBalloonEvent.LeaveEvent(sender.LoggedinUser);

                // Leave open quiz.
                if (QuizEvent != null)
                    QuizEvent.LeaveEvent(sender.LoggedinUser);

                ModsRevengeEvent.LeaveEvent(sender.LoggedinUser);

                // Delete Arena Entries
                if (Arena.UserHasEnteredHorseInAnyArena(sender.LoggedinUser))
                {
                    Arena arena = Arena.GetArenaUserEnteredIn(sender.LoggedinUser);
                    arena.DeleteEntry(sender.LoggedinUser);
                }


                // Send disconnect message
                byte[] logoutMessageBytes = PacketBuilder.CreateChat(Messages.FormatLogoutMessage(sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
                foreach (GameClient client in ConnectedClients)
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteLogins && !client.LoggedinUser.MuteAll)
                            if (client.LoggedinUser.Id != sender.LoggedinUser.Id)
                                client.SendPacket(logoutMessageBytes);

                // Tell clients of diconnect (remove from chat)
                byte[] playerRemovePacket = PacketBuilder.CreatePlayerLeavePacket(sender.LoggedinUser.Username);
                foreach (GameClient client in ConnectedClients)
                    if (client.LoggedIn)
                        if (client.LoggedinUser.Id != sender.LoggedinUser.Id)
                            client.SendPacket(playerRemovePacket);
            }
            connectedClients.Remove(sender);

        }

        /*
         *  Get(Some Information)
         */


        public static bool IsUserOnline(int id)
        {
            try
            {
                GetUserById(id);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public static User[] GetUsersInTown(World.Town town, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersInTown = new List<User>();
            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteIsland)
                        continue;
                    if (World.InTown(client.LoggedinUser.X, client.LoggedinUser.Y))
                        if (World.GetIsle(client.LoggedinUser.X, client.LoggedinUser.Y).Name == town.Name)
                            usersInTown.Add(client.LoggedinUser);
                }

            return usersInTown.ToArray();
        }
        public static User[] GetUsersInIsle(World.Isle isle, bool includeStealth = false, bool includeMuted = false)
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

        public static User[] GetUsersOnSpecialTileCode(string code)
        {
            List<User> userList = new List<User>();

            foreach (GameClient client in connectedClients)
            {
                if (client.LoggedIn)
                {

                    if (World.InSpecialTile(client.LoggedinUser.X, client.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(client.LoggedinUser.X, client.LoggedinUser.Y);

                        if (tile.Code == code)
                        {
                            userList.Add(client.LoggedinUser);
                        }
                    }
                }
            }
            return userList.ToArray();
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
        public static User GetUserByName(string username)
        {
            foreach(GameClient client in ConnectedClients)
            {
                if(client.LoggedIn)
                {
                    if (client.LoggedinUser.Username.ToLower() == username.ToLower())
                        return client.LoggedinUser;
                }
            }
            throw new KeyNotFoundException("User was not found.");
        }

        public static User GetUserById(int id)
        {
            foreach(GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.Id == id)
                        return client.LoggedinUser;
            }

            throw new KeyNotFoundException("User not found (not online?)");
        }

        public static User[] GetReallyNearbyUsers(int x, int y)
        {
            int startX = x - 3;
            int endX = x + 3;
            int startY = y - 3;
            int endY = y + 3;
            List<User> usersNearby = new List<User>();

            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (startX <= client.LoggedinUser.X && endX >= client.LoggedinUser.X && startY <= client.LoggedinUser.Y && endY >= client.LoggedinUser.Y)
                        usersNearby.Add(client.LoggedinUser);
                }

            return usersNearby.ToArray();
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

        public static Point[] GetAllBuddyLocations(User caller)
        {
            List<Point> allLocations = new List<Point>();

            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                {

                    if (!caller.Friends.List.Contains(client.LoggedinUser.Id))
                        continue;
                    

                    if (!client.LoggedinUser.Stealth)
                        allLocations.Add(new Point(client.LoggedinUser.X, client.LoggedinUser.Y));

                }
            }

            return allLocations.ToArray();
        }

        public static Point[] GetAllPlayerLocations(User caller)
        {
            List<Point> allLocations = new List<Point>();
            
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                {

                    if (client.LoggedinUser.Id == caller.Id) 
                        continue;
                    
                    if (!client.LoggedinUser.Stealth)
                        allLocations.Add(new Point(client.LoggedinUser.X, client.LoggedinUser.Y));
                    
                }

                        
            }
            return allLocations.ToArray();
        }
        public static int GetNumberOfPlayersListeningToAdsChat()
        {
            int count = 0;
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (!client.LoggedinUser.MuteAds)
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

            UpdateUserFacingAndLocation(client.LoggedinUser);
        }

        public static void UpdateDrawingForAll(string id, GameClient sender, string drawing, bool includingSender=false)
        {

            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
            User[] usersHere = GetUsersOnSpecialTileCode("MULTIROOM-D" + id);
            foreach (User user in usersHere)
            {
                if(!includingSender)
                    if (user.Id == sender.LoggedinUser.Id)
                        continue;
                

                byte[] patchDrawing = PacketBuilder.CreateDrawingUpdatePacket(drawing);
                user.LoggedinClient.SendPacket(patchDrawing);

            }
        }
        public static void UpdateHorseMenu(GameClient forClient, HorseInstance horseInst)
        {
            int TileID = Map.GetTileId(forClient.LoggedinUser.X, forClient.LoggedinUser.Y, false);
            string type = Map.TerrainTiles[TileID - 1].Type;

            if (horseInst.Owner == forClient.LoggedinUser.Id)
                forClient.LoggedinUser.LastViewedHorse = horseInst;
            else
                forClient.LoggedinUser.LastViewedHorseOther = horseInst;

            forClient.LoggedinUser.MetaPriority = true;
            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseInformation(horseInst, forClient.LoggedinUser));
            forClient.SendPacket(metaPacket);

            string loadSwf = HorseInfo.BreedViewerSwf(horseInst, type);
            byte[] swfPacket = PacketBuilder.CreateSwfModulePacket(loadSwf, PacketBuilder.PACKET_SWF_MODULE_FORCE);
            forClient.SendPacket(swfPacket);
        }
        public static void UpdateInventory(GameClient forClient)
        {
            if (!forClient.LoggedIn)
                return;
            forClient.LoggedinUser.MetaPriority = true;
            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildInventoryInfo(forClient.LoggedinUser.Inventory));
            forClient.SendPacket(metaPacket);
        }

        public static void UpdateWeather(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update weather information when not logged in.");
                return;
            }

            string lastWeather = forClient.LoggedinUser.LastSeenWeather;
            string weather = forClient.LoggedinUser.GetWeatherSeen();
            if (lastWeather != weather)
            {
                byte[] WeatherUpdate = PacketBuilder.CreateWeatherUpdatePacket(weather);
                forClient.SendPacket(WeatherUpdate);
            }
        }
        public static void UpdateWorld(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update world information when not logged in.");
                return;
            }

            byte[] WorldData = PacketBuilder.CreateWorldData(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, forClient.LoggedinUser.GetWeatherSeen());
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
        public static void UpdateUserFacingAndLocation(User user)
        {
            byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(user.X, user.Y, user.Facing, user.CharacterId, user.Username);

            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (client.LoggedinUser.Id != user.Id)
                        client.SendPacket(playerInfoBytes);
                }
        }
        public static void UpdateAreaForAll(int x, int y, bool ignoreMetaPrio=false, User exceptMe=null)
        {
            foreach(GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.X == x && client.LoggedinUser.Y == y)
                        if(!client.LoggedinUser.MetaPriority || ignoreMetaPrio)
                            if(client.LoggedinUser != exceptMe)
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

            if(forClient.LoggedinUser.TradingWith != null)
            {
                if (!forClient.LoggedinUser.TradingWith.OtherTrade.Trader.LoggedinClient.LoggedIn)
                {
                    forClient.LoggedinUser.TradingWith.InteruptTrade();
                    return;
                }

                if (forClient.LoggedinUser.TradingWith.OtherTrade.Trader.TradingWith == null)
                {
                    forClient.LoggedinUser.TradingWith.InteruptTrade();
                    return;
                }

                forClient.LoggedinUser.MetaPriority = true;
                forClient.LoggedinUser.TradeMenuPriority = false;
                byte[] tradeMeta = PacketBuilder.CreateMetaPacket(Meta.BuildTrade(forClient.LoggedinUser.TradingWith));
                forClient.SendPacket(tradeMeta);
                return;
            }

            forClient.LoggedinUser.MetaPriority = false;
            forClient.LoggedinUser.ListingAuction = false;

            string LocationStr = "";
            if (!World.InSpecialTile(forClient.LoggedinUser.X, forClient.LoggedinUser.Y))
            {
                if (forClient.LoggedinUser.InRealTimeQuiz)
                    return;
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

                if (forClient.LoggedinUser.InRealTimeQuiz && QuizEvent != null)
                {
                    QuizEvent.JoinEvent(forClient.LoggedinUser).UpdateParticipent();
                    return;
                }

                if (specialTile.Code != null)
                    if (!ProcessMapCodeWithArg(forClient, specialTile))
                        return;
                LocationStr = Meta.BuildSpecialTileInfo(forClient.LoggedinUser, specialTile);
            }


            byte[] AreaMessage = PacketBuilder.CreateMetaPacket(LocationStr);
            forClient.SendPacket(AreaMessage);

        }
        public static void UpdateStats(GameClient client)
        {
            if (!client.LoggedIn)
                return;

            client.LoggedinUser.MetaPriority = true;
            string metaWind = Meta.BuildStatsMenu(client.LoggedinUser);
            byte[] statsPacket = PacketBuilder.CreateMetaPacket(metaWind);
            client.SendPacket(statsPacket);

        }

        /*
         *   Other...
         */

        public static void AddItemToAllUsersEvenOffline(int itemId, int itemCount)
        {
            int[] allUsers = Database.GetUsers();
            foreach (int userid in allUsers)
            {
                for (int i = 0; i < itemCount; i++)
                {
                    ItemInstance itm = new ItemInstance(itemId);

                    if (GameServer.IsUserOnline(userid))
                        GameServer.GetUserById(userid).Inventory.AddWithoutDatabase(itm);

                    Database.AddItemToInventory(userid, itm);
                }
            }
        }

        public static void RemoveAllItemsOfIdInTheGame(int id)
        {
            // Remove from all online players
            foreach (GameClient connectedClient in GameServer.ConnectedClients)
            {
                if (connectedClient.LoggedIn)
                    if (connectedClient.LoggedinUser.Inventory.HasItemId(id))
                    {
                        InventoryItem invItm = connectedClient.LoggedinUser.Inventory.GetItemByItemId(id);
                        foreach (ItemInstance itm in invItm.ItemInstances.ToArray())
                            connectedClient.LoggedinUser.Inventory.Remove(itm);
                    }
            }
            DroppedItems.DeleteAllItemsWithId(id); // Delete all dropped items
            Database.DeleteAllItemsFromUsers(id); // Delete from offline players
        }

        public static void StartRidingHorse(GameClient sender, int horseRandomId)
        {
            HorseInstance horseMountInst = sender.LoggedinUser.HorseInventory.GetHorseById(horseRandomId);

            if (horseMountInst.Breed.Type != "unicorn" && horseMountInst.Breed.Type != "pegasus")
            {
                if (horseMountInst.Equipment.Saddle == null || horseMountInst.Equipment.SaddlePad == null || horseMountInst.Equipment.Bridle == null)
                {
                    byte[] horseNotTackedMessage = PacketBuilder.CreateChat(Messages.HorseCannotMountUntilTackedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(horseNotTackedMessage);
                    return;
                }
            }


            string ridingHorseMessage = Messages.FormatHorseRidingMessage(horseMountInst.Name);
            byte[] ridingHorseMessagePacket = PacketBuilder.CreateChat(ridingHorseMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(ridingHorseMessagePacket);

            sender.LoggedinUser.CurrentlyRidingHorse = horseMountInst;

            // Determine what sprite to use;
            int incBy = 0;
            switch (horseMountInst.Color)
            {
                case "brown":
                    incBy = 1;
                    break;
                case "cremello":
                case "white":
                    incBy = 2;
                    break;
                case "black":
                    incBy = 3;
                    break;
                case "chestnut":
                    incBy = 4;
                    break;
                case "bay":
                    incBy = 5;
                    break;
                case "grey":
                    incBy = 6;
                    break;
                case "dun":
                    incBy = 7;
                    break;
                case "palomino":
                    incBy = 8;
                    break;
                case "roan":
                    incBy = 9;
                    break;
                case "pinto":
                    incBy = 10;
                    break;
            }


            if (horseMountInst.Breed.Type == "zebra")
            {
                incBy = 11;
            }
            if (horseMountInst.Breed.Id == 5) // Appaloosa
            {
                if (horseMountInst.Color == "cremello")
                    incBy = 12;
            }
            if (horseMountInst.Breed.Type == "camel")
            {
                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(40)); // Camel Rider

                incBy = 13;
            }
            if(horseMountInst.Breed.Type == "llama")
            {
                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(41)); // Llama Rider

                incBy = 14;
            }
            if (horseMountInst.Breed.Type == "unicorn")
            {
                incBy = 15;
            }
            if (horseMountInst.Breed.Type == "pegasus")
            {
                incBy = 16;
                sender.LoggedinUser.NoClip = true;
            }
            if (horseMountInst.Breed.Id == 170) // Unipeg
            {
                incBy = 17;
                sender.LoggedinUser.NoClip = true;
            }

            incBy *= 5;
            sender.LoggedinUser.Facing %= 5;
            sender.LoggedinUser.Facing += incBy;
            sender.LoggedinUser.LastRiddenHorse = horseRandomId;


            byte[] rideHorsePacket = PacketBuilder.CreateHorseRidePacket(sender.LoggedinUser.X, sender.LoggedinUser.Y, sender.LoggedinUser.CharacterId, sender.LoggedinUser.Facing, 10, true);
            sender.SendPacket(rideHorsePacket);

            UpdateUserFacingAndLocation(sender.LoggedinUser);
        }
        public static void StopRidingHorse(GameClient sender)
        {
            sender.LoggedinUser.CurrentlyRidingHorse = null;

            byte[] stopRidingHorseMessagePacket = PacketBuilder.CreateChat(Messages.HorseStopRidingMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(stopRidingHorseMessagePacket);


            sender.LoggedinUser.Facing %= 5;
            byte[] rideHorsePacket = PacketBuilder.CreateHorseRidePacket(sender.LoggedinUser.X, sender.LoggedinUser.Y, sender.LoggedinUser.CharacterId, sender.LoggedinUser.Facing, 10, true);
            sender.SendPacket(rideHorsePacket);
            sender.LoggedinUser.NoClip = false;

            UpdateUserFacingAndLocation(sender.LoggedinUser);
        }
        public static bool ProcessMapCodeWithArg(GameClient forClient, World.SpecialTile tile)
        {
            string mapCode = tile.Code;
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
                            if (World.InIsle(tile.X, tile.Y))
                            {
                                World.Isle isle = World.GetIsle(tile.X, tile.Y);
                                int tileset = isle.Tileset;
                                int overlay = Map.GetTileId(tile.X, tile.Y, true);
                                if (tileset == 6 && overlay == 249) // warp point
                                {
                                    byte[] swfPacket = PacketBuilder.CreateSwfModulePacket("warpcutscene", PacketBuilder.PACKET_SWF_CUTSCENE);
                                    forClient.SendPacket(swfPacket);
                                }
                            }
                            return false;
                        }
                        catch(Exception)
                        {
                            return true;
                        }
                    }
                }
            }
            if(mapCode == "HAMMOCK")
            {
                byte[] hammockText = PacketBuilder.CreateChat(Messages.HammockText, PacketBuilder.CHAT_BOTTOM_RIGHT);
                forClient.SendPacket(hammockText);

                forClient.LoggedinUser.Tiredness = 1000;
                foreach(HorseInstance horse in forClient.LoggedinUser.HorseInventory.HorseList)
                {
                    horse.BasicStats.Tiredness = 1000;
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
            gameTimer = new Timer(new TimerCallback(onGameTick), null, gameTickSpeed, gameTickSpeed);
            minuteTimer = new Timer(new TimerCallback(onMinuteTick), null, oneMinute, oneMinute);
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
