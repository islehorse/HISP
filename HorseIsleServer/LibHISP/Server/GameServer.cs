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
using HISP.Util;

namespace HISP.Server
{
    public class GameServer
    {
        public static Socket ServerSocket;


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
        private static int gameTickSpeed = 4800; // Changing this to ANYTHING else will cause desync with the client.
        private static int totalMinutesElapsed = 0;
        private static int oneMinute = 1000 * 60;
        private static Timer gameTimer; // Controls in-game time.
        private static Timer minuteTimer; // ticks every real world minute.
        private static void onGameTick(object state)
        {
            // Tick the game clock.
            World.TickWorldClock();

            // Start all events with this RaceEvery set.
            Arena.StartArenas(World.ServerTime.Minutes);

            // Decrement horse train timer
            Database.DecHorseTrainTimeout();

            // Write time to database:
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
                            GameServer.GetUserById(ranchOwner).AddMoney(moneyToAdd);
                        else
                        {
                            try
                            {
                                Database.SetPlayerMoney(Database.GetPlayerMoney(ranchOwner) + moneyToAdd, ranchOwner);
                            }
                            catch (OverflowException)
                            {
                                Database.SetPlayerMoney(2147483647, ranchOwner);
                            }
                        }
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


            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client == null)
                    continue;

                if (client.LoggedIn)
                {
                    if (!client.User.MajorPriority && !client.User.MinorPriority)
                        UpdateArea(client);

                    byte[] BaseStatsPacketData = PacketBuilder.CreateMoneyPlayerCountAndMail(client.User.Money, GameServer.GetNumberOfPlayers(), client.User.MailBox.UnreadMailCount);
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

        // HI1 Protocol

        public static void OnPlayerInteration(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested Bird Map when not logged in.");
                return;
            }
            if(packet.Length >= 2)
            {
                byte method = packet[1];
                switch (method)
                {
                    case PacketBuilder.PLAYER_INTERACTION_TRADE_REJECT:
                        if (sender.User.TradingWith != null)
                            sender.User.TradingWith.CancelTrade();
                        break;
                    case PacketBuilder.PLAYER_INTERACTION_ACCEPT:
                        if (sender.User.TradingWith != null)
                            sender.User.TradingWith.AcceptTrade();
                        break;
                    case PacketBuilder.PLAYER_INTERACTION_PROFILE:
                        string packetStr = Encoding.UTF8.GetString(packet);
                        string playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        int playerId = -1;
                        try
                        {
                            playerId = int.Parse(playerIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to view profile of User ID NaN.");
                            break;
                        }

                        if (IsUserOnline(playerId))
                        {
                            User user = GetUserById(playerId);
                            sender.User.MajorPriority = true;

                            byte[] metaTag = PacketBuilder.CreateMeta(Meta.BuildStatsMenu(user, true));
                            sender.SendPacket(metaTag);
                        }
                        break;
                    case PacketBuilder.PLAYER_INTERACTION_MUTE:
                        packetStr = Encoding.UTF8.GetString(packet);
                        playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        playerId = -1;
                        try
                        {
                            playerId = int.Parse(playerIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to MUTE User ID NaN.");
                            break;
                        }

                        if (IsUserOnline(playerId))
                        {
                            User user = GetUserById(playerId);
                            if (!sender.User.MutePlayer.IsUserMuted(user))
                                sender.User.MutePlayer.MuteUser(user);

                            byte[] nowMuting = PacketBuilder.CreateChat(Messages.FormatNowMutingPlayer(user.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(nowMuting);

                            sender.User.MajorPriority = true;
                            byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildPlayerListMenu(sender.User));
                            sender.SendPacket(metaPacket);
                        }
                        break;
                    case PacketBuilder.PLAYER_INTERACTION_UNMUTE:
                        packetStr = Encoding.UTF8.GetString(packet);
                        playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        playerId = -1;
                        try
                        {
                            playerId = int.Parse(playerIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to UNMUTE User ID NaN.");
                            break;
                        }

                        if (IsUserOnline(playerId))
                        {
                            User user = GetUserById(playerId);
                            if (sender.User.MutePlayer.IsUserMuted(user))
                                sender.User.MutePlayer.UnmuteUser(user);

                            byte[] stoppedMuting = PacketBuilder.CreateChat(Messages.FormatStoppedMutingPlayer(user.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(stoppedMuting);

                            sender.User.MajorPriority = true;
                            byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildPlayerListMenu(sender.User));
                            sender.SendPacket(metaPacket);
                        }
                        break;
                    case PacketBuilder.PLAYER_INTERACTION_REMOVE_BUDDY:
                        packetStr = Encoding.UTF8.GetString(packet);
                        playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        playerId = -1;
                        try
                        {
                            playerId = int.Parse(playerIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to remove User ID NaN as a buddy.");
                            break;
                        }


                        if (sender.User.Friends.IsFriend(playerId))
                        {
                            sender.User.Friends.RemoveFriend(playerId);

                            byte[] friendRemoved = PacketBuilder.CreateChat(Messages.FormatAddBuddyRemoveBuddy(Database.GetUsername(playerId)), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(friendRemoved);

                            sender.User.MajorPriority = true;
                            byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildPlayerListMenu(sender.User));
                            sender.SendPacket(metaPacket);
                        }

                        break;
                    case PacketBuilder.PLAYER_INTERACTION_TAG:
                        packetStr = Encoding.UTF8.GetString(packet);
                        playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        playerId = -1;
                        try
                        {
                            playerId = int.Parse(playerIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to trade with User ID NaN.");
                            break;
                        }

                        if (IsUserOnline(playerId))
                        {
                            User user = GetUserById(playerId); ;
                            string TAGYourIT = Messages.FormatTagYourIt(user.Username, sender.User.Username);
                            int totalBuds = 0;
                            foreach (int friendId in sender.User.Friends.List)
                            {
                                if (friendId == sender.User.Id)
                                    continue;

                                if (IsUserOnline(friendId))
                                {
                                    User buddy = GetUserById(friendId);
                                    byte[] tagYourItPacket = PacketBuilder.CreateChat(TAGYourIT, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    buddy.Client.SendPacket(tagYourItPacket);
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
                        playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        playerId = -1;
                        try
                        {
                            playerId = int.Parse(playerIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to add friend with User ID NaN.");
                            break;
                        }
                        if (IsUserOnline(playerId))
                        {
                            User userToAdd = GetUserById(playerId);
                            sender.User.Friends.AddFriend(userToAdd);
                        }
                        break;
                    case PacketBuilder.PLAYER_INTERACTION_ADD_ITEM:
                        if (sender.User.TradingWith == null)
                            break;
                        if (packet.Length < 5)
                            break;

                        packetStr = Encoding.UTF8.GetString(packet);
                        string idStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        char firstChar = idStr[0];
                        switch (firstChar)
                        {
                            case '3': // Trade Money

                                if (sender.User.Bids.Length > 0)
                                {
                                    byte[] cantBuyWhileAuctioning = PacketBuilder.CreateChat(Messages.AuctionNoOtherTransactionAllowed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantBuyWhileAuctioning);
                                    break;
                                }

                                sender.User.TradeMenuPriority = true;
                                sender.User.AttemptingToOfferItem = -1;
                                byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildTradeAddMoney(sender.User.TradingWith.MoneyOffered));
                                sender.SendPacket(metaPacket);

                                break;
                            case '2': // Trade Horse
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

                                if (!sender.User.HorseInventory.HorseIdExist(horseRandomId))
                                    break;

                                HorseInstance horse = sender.User.HorseInventory.GetHorseById(horseRandomId);
                                if (!sender.User.TradingWith.HorsesOffered.Contains(horse))
                                    sender.User.TradingWith.OfferHorse(horse);

                                UpdateArea(sender);

                                if (sender.User.TradingWith != null)
                                    if (!sender.User.TradingWith.OtherTrade.Trader.TradeMenuPriority)
                                        UpdateArea(sender.User.TradingWith.OtherTrade.Trader.Client);

                                break;
                            case '1': // Trade Item
                                string itemIdStr = idStr.Substring(1);
                                int itemId = -1;
                                try
                                {
                                    itemId = int.Parse(itemIdStr);
                                }
                                catch (FormatException)
                                {
                                    break;
                                }

                                if (!sender.User.Inventory.HasItemId(itemId))
                                    break;

                                sender.User.TradeMenuPriority = true;
                                sender.User.AttemptingToOfferItem = itemId;
                                InventoryItem item = sender.User.Inventory.GetItemByItemId(itemId);
                                byte[] addItemPacket = PacketBuilder.CreateMeta(Meta.BuildTradeAddItem(item.ItemInstances.Length));
                                sender.SendPacket(addItemPacket);
                                break;

                        }
                        break;
                    case PacketBuilder.PLAYER_INTERACTION_TRADE:
                        packetStr = Encoding.UTF8.GetString(packet);
                        playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        playerId = -1;
                        try
                        {
                            playerId = int.Parse(playerIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to trade with User ID NaN.");
                            break;
                        }
                        if (IsUserOnline(playerId))
                        {
                            User user = GetUserById(playerId);
                            byte[] tradeMsg = PacketBuilder.CreateChat(Messages.TradeRequiresBothPlayersMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(tradeMsg);

                            sender.User.PendingTradeTo = user.Id;

                            if (user.PendingTradeTo == sender.User.Id)
                            {
                                // Start Trade
                                Trade tradeWithYou = new Trade(sender.User);
                                Trade tradeWithOther = new Trade(user);
                                tradeWithYou.OtherTrade = tradeWithOther;
                                tradeWithOther.OtherTrade = tradeWithYou;

                                sender.User.TradingWith = tradeWithYou;
                                user.TradingWith = tradeWithOther;

                                UpdateArea(sender);
                                UpdateArea(user.Client);
                            }

                        }
                        break;
                    default:
                        Logger.DebugPrint("Unknown Player interaction Method: 0x" + method.ToString("X") + " Packet: " + BitConverter.ToString(packet).Replace("-", " "));
                        break;
                }
                return;
            }
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
                    string playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " tried to socialize with User ID NaN.");
                        break;
                    }

                    if(IsUserOnline(playerId))
                    {
                        sender.User.SocializingWith = GetUserById(playerId);
                        
                        sender.User.SocializingWith.AddSocailizedWith(sender.User);
                        sender.User.MajorPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildSocialMenu(sender.User.CurrentlyRidingHorse != null));
                        sender.SendPacket(metaPacket);
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + " tried to socialize with User #"+playerId.ToString()+" but there not online.");
                    }
                    break;
                case PacketBuilder.SOCIALS_USE:
                    int socialId = Convert.ToInt32(packet[2] - (byte)0x21);
                    SocialType.Social social = SocialType.GetSocial(socialId);
                    /*
                     *  Check if player being socialed with
                     *  is actually on this tile, not muted, etc
                     */
                    if (sender.User.SocializingWith != null && social.BaseSocialType.Type != "GROUP")
                    {
                        if (sender.User.SocializingWith.MuteAll || sender.User.SocializingWith.MuteSocials)
                        {
                            byte[] cantSocialize = PacketBuilder.CreateChat(Messages.PlayerIgnoringAllSocials, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantSocialize);
                            break;
                        }
                        
                        if (sender.User.SocializingWith.MutePlayer.IsUserMuted(sender.User))
                        {
                            byte[] cantSocialize = PacketBuilder.CreateChat(Messages.PlayerIgnoringYourSocials, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantSocialize);
                            break;
                        }
                    
                        if(sender.User.SocializingWith.X != sender.User.X && sender.User.SocializingWith.Y != sender.User.Y)
                        {
                            byte[] playerNotNearby = PacketBuilder.CreateChat(Messages.SocialPlayerNoLongerNearby, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(playerNotNearby);
                            break;
                        }
                    }
                    

                    if(social.ForEveryone != null)
                    {
                        foreach (User user in GetUsersAt(sender.User.X, sender.User.Y, true, true))
                        {
                            if (social.BaseSocialType.Type != "GROUP")
                                if (user.Id == sender.User.SocializingWith.Id)
                                    continue;

                            if (user.Id == sender.User.Id)
                                continue;

                            if (user.MuteAll || user.MuteSocials)
                                continue;
                            string socialTarget = "";
                            if(sender.User.SocializingWith != null)
                                socialTarget = sender.User.SocializingWith.Username;
                            byte[] msgEveryone = PacketBuilder.CreateChat(Messages.FormatSocialMessage(social.ForEveryone, socialTarget, sender.User.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            user.Client.SendPacket(msgEveryone);
                        }

                    }
                    if(social.ForTarget != null)
                    {
                        if(sender.User.SocializingWith != null)
                        {
                            if (social.BaseSocialType.Type != "GROUP")
                            {
                                byte[] msgTarget = PacketBuilder.CreateChat(Messages.FormatSocialMessage(social.ForTarget, sender.User.SocializingWith.Username, sender.User.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.User.SocializingWith.Client.SendPacket(msgTarget);
                            }
                        }
                    }
                    if(social.ForSender != null)
                    {
                        string socialTarget = "";
                        if (sender.User.SocializingWith != null)
                            socialTarget = sender.User.SocializingWith.Username;

                        byte[] msgSender = PacketBuilder.CreateChat(Messages.FormatSocialMessage(social.ForSender, socialTarget, sender.User.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(msgSender);

                        
                    }

                    if (social.SoundEffect != null)
                    {
                        foreach (User user in GetUsersAt(sender.User.X, sender.User.Y, true, true))
                        {

                            if (user.MuteAll || user.MuteSocials)
                                continue;

                            byte[] soundEffect = PacketBuilder.CreatePlaySound(social.SoundEffect);
                            user.Client.SendPacket(soundEffect);
                        }
                    }

                    break;
                default:
                    Logger.ErrorPrint(sender.User.Username + " unknown social: " + method.ToString("X") + " packet dump: " + BitConverter.ToString(packet).Replace("-", " "));
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

            if(sender.User.Inventory.HasItemId(Item.Telescope))
            {
                byte[] birdMapPacket = PacketBuilder.CreateBirdMap(sender.User.X, sender.User.Y);
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
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid sized auction packet: " + BitConverter.ToString(packet).Replace("-", " "));
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
                    if(World.InSpecialTile(sender.User.X, sender.User.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                        if(tile.Code != null)
                        {
                            if(tile.Code.StartsWith("AUCTION-"))
                            {
                                Auction auctionRoom = Auction.GetAuctionRoomById(int.Parse(tile.Code.Split('-')[1]));
                                int auctionEntryId = -1;
                                string packetStr = Encoding.UTF8.GetString(packet);
                                string auctionEntryStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
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
                                entry.Bid(sender.User, bidAmount);

                                UpdateAreaForAll(tile.X, tile.Y, true, null);
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

            if(packet.Length < 2)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid sized horse interaction packet: " + BitConverter.ToString(packet).Replace("-", " "));
                return;
            }

            byte method = packet[1];
            switch(method)
            {
                case PacketBuilder.HORSE_LIST:
                    sender.User.MajorPriority = true;
                    byte[] metaTags = PacketBuilder.CreateMeta(Meta.BuildHorseInventory(sender.User));
                    sender.SendPacket(metaTags);
                    break;
                case PacketBuilder.HORSE_PROFILE:
                    byte methodProfileEdit = packet[2]; 
                    if(methodProfileEdit == PacketBuilder.HORSE_PROFILE_EDIT)
                    {
                        if (sender.User.LastViewedHorse != null)
                        {
                            sender.User.MajorPriority = true;
                            byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseDescriptionEditMeta(sender.User.LastViewedHorse));
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.User.Username + "Trying to edit description of no horse");
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
                    string randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseFeedInst = sender.User.HorseInventory.GetHorseById(randomId);

                        sender.User.LastViewedHorse = horseFeedInst;
                        sender.User.MajorPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseFeedMenu(horseFeedInst, sender.User));
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to feed at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_PET:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horsePetInst = sender.User.HorseInventory.GetHorseById(randomId);
                        sender.User.LastViewedHorse = horsePetInst;
                        int randMoodAddition = RandomNumberGenerator.Next(1, 20);
                        int randTiredMinus = RandomNumberGenerator.Next(1, 10);

                        string msgs = "";
                        if (horsePetInst.BasicStats.Mood + randMoodAddition >= 1000)
                        {
                            msgs += Messages.HorsePetTooHappy;
                        }

                        if (horsePetInst.BasicStats.Tiredness - randTiredMinus <= 0)
                        {
                            msgs += Messages.HorsePetTooTired;
                        }

                        horsePetInst.BasicStats.Tiredness -= randTiredMinus;
                        horsePetInst.BasicStats.Mood += randMoodAddition;

                        byte[] petMessagePacket = PacketBuilder.CreateChat(Messages.FormatHorsePetMessage(msgs,randMoodAddition, randTiredMinus), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(petMessagePacket);

                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to feed at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_VET_SERVICE_ALL:

                    if (World.InSpecialTile(sender.User.X, sender.User.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                        if(tile.Code != null)
                        {
                            if (tile.Code.StartsWith("VET-"))
                            {
                                string[] vetInfo = tile.Code.Split('-');
                                int vetId = int.Parse(vetInfo[1]);
                                Vet vet = Vet.GetVetById(vetId);
                                int price = 0;

                                foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
                                    price += vet.CalculatePrice(horse.BasicStats.Health);
                                if (price == 0)
                                {
                                    byte[] notNeededMessagePacket = PacketBuilder.CreateChat(Messages.VetServicesNotNeededAll, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(notNeededMessagePacket);
                                    break;
                                }
                                else if (sender.User.Money >= price)
                                {
                                    foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
                                        horse.BasicStats.Health = 1000;

                                    byte[] healedMessagePacket = PacketBuilder.CreateChat(Messages.VetAllFullHealthRecoveredMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(healedMessagePacket);

                                    sender.User.TakeMoney(price);

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
                    }
                    break;
                case PacketBuilder.HORSE_VET_SERVICE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    if (randomIdStr == "NaN")
                        break;
                        
                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseVetServiceInst = sender.User.HorseInventory.GetHorseById(randomId);
                        sender.User.LastViewedHorse = horseVetServiceInst;

                        if(World.InSpecialTile(sender.User.X, sender.User.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                            if(tile.Code != null)
                            {
                                if (tile.Code.StartsWith("VET-"))
                                {
                                    string[] vetInfo = tile.Code.Split('-');
                                    int vetId = int.Parse(vetInfo[1]);

                                    Vet vet = Vet.GetVetById(vetId);
                                    int price = vet.CalculatePrice(horseVetServiceInst.BasicStats.Health);
                                    if (sender.User.Money >= price)
                                    {
                                        horseVetServiceInst.BasicStats.Health = 1000;
                                        sender.User.TakeMoney(price);

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
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to use vet services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_SHOE_STEEL:
                case PacketBuilder.HORSE_SHOE_IRON:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseFarrierServiceInst = sender.User.HorseInventory.GetHorseById(randomId);
                        sender.User.LastViewedHorse = horseFarrierServiceInst;

                        if (World.InSpecialTile(sender.User.X, sender.User.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                            if(tile.Code != null)
                            {
                                if (tile.Code.StartsWith("FARRIER-"))
                                {
                                    string[] farrierInfo = tile.Code.Split('-');
                                    int farrierId = int.Parse(farrierInfo[1]);

                                    Farrier farrier = Farrier.GetFarrierById(farrierId);
                                    int price = 0;
                                    int incAmount = 0;
                                    string msg = "";

                                    if (method == PacketBuilder.HORSE_SHOE_STEEL)
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

                                    if (sender.User.Money >= price)
                                    {
                                        horseFarrierServiceInst.BasicStats.Shoes = incAmount;
                                        sender.User.TakeMoney(price);

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
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to use farrier services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_SHOE_ALL:
                    if (World.InSpecialTile(sender.User.X, sender.User.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                        if(tile.Code != null) 
                        {
                            if (tile.Code.StartsWith("FARRIER-"))
                            {
                                string[] farrierInfo = tile.Code.Split('-');
                                int farrierId = int.Parse(farrierInfo[1]);

                                Farrier farrier = Farrier.GetFarrierById(farrierId);

                                int totalPrice = 0;
                                foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
                                {
                                    if (horse.BasicStats.Shoes < farrier.SteelShoesAmount)
                                    {
                                        totalPrice += farrier.SteelCost;
                                    }
                                }

                                if (sender.User.Money >= totalPrice)
                                {
                                    foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
                                    {
                                        if (horse.BasicStats.Shoes < farrier.SteelShoesAmount)
                                        {
                                            horse.BasicStats.Shoes = farrier.SteelShoesAmount;
                                        }
                                    }
                                    sender.User.TakeMoney(totalPrice);

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
                        
                    }
                    break;
                case PacketBuilder.HORSE_GROOM_SERVICE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance groomHorseInst = sender.User.HorseInventory.GetHorseById(randomId);
                        sender.User.LastViewedHorse = groomHorseInst;

                        if (World.InSpecialTile(sender.User.X, sender.User.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                            if(tile.Code != null)
                            {
                                if (tile.Code.StartsWith("GROOMER-"))
                                {
                                    string[] groomerInfo = tile.Code.Split('-');
                                    int groomerId = int.Parse(groomerInfo[1]);

                                    Groomer groomer = Groomer.GetGroomerById(groomerId);
                                    int price = groomer.CalculatePrice(groomHorseInst.BasicStats.Groom);


                                    if (sender.User.Money >= price)
                                    {
                                        groomHorseInst.BasicStats.Groom = groomer.Max;
                                        sender.User.TakeMoney(price);

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
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to use groomer services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_GROOM_SERVICE_ALL:
                    if (World.InSpecialTile(sender.User.X, sender.User.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                        if(tile.Code != null)
                        {
                            if (tile.Code.StartsWith("GROOMER-"))
                            {
                                string[] groomerInfo = tile.Code.Split('-');
                                int groomId = int.Parse(groomerInfo[1]);
                                Groomer groomer = Groomer.GetGroomerById(groomId);
                                int price = 0;
                                int count = 0;

                                foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
                                {
                                    if (horse.BasicStats.Groom < groomer.Max)
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
                                else if (sender.User.Money >= price)
                                {
                                    foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
                                        if (horse.BasicStats.Groom < groomer.Max)
                                            horse.BasicStats.Groom = groomer.Max;

                                    byte[] groomedAllHorsesPacket = PacketBuilder.CreateChat(Messages.GroomerBestToHisAbilitiesALL, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(groomedAllHorsesPacket);

                                    sender.User.TakeMoney(price);

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
                    }
                    break;
                case PacketBuilder.HORSE_BARN_SERVICE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance barnHorseInst = sender.User.HorseInventory.GetHorseById(randomId);
                        sender.User.LastViewedHorse = barnHorseInst;

                        if (World.InSpecialTile(sender.User.X, sender.User.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                            if (tile.Code != null)
                            {
                                if (tile.Code.StartsWith("BARN-"))
                                {
                                    string[] barnInfo = tile.Code.Split('-');
                                    int barnId = int.Parse(barnInfo[1]);

                                    Barn barn = Barn.GetBarnById(barnId);
                                    int price = barn.CalculatePrice(barnHorseInst.BasicStats.Tiredness, barnHorseInst.BasicStats.Hunger, barnHorseInst.BasicStats.Thirst); ;


                                    if (sender.User.Money >= price)
                                    {
                                        barnHorseInst.BasicStats.Tiredness = 1000;
                                        barnHorseInst.BasicStats.Hunger = 1000;
                                        barnHorseInst.BasicStats.Thirst = 1000;
                                        sender.User.TakeMoney(price);

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
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to use groomer services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_BARN_SERVICE_ALL:
                    if (World.InSpecialTile(sender.User.X, sender.User.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                        if(tile.Code != null)
                        {
                            if (tile.Code.StartsWith("BARN-"))
                            {
                                string[] barnInfo = tile.Code.Split('-');
                                int barnId = int.Parse(barnInfo[1]);
                                Barn barn = Barn.GetBarnById(barnId);
                                int totalPrice = 0;

                                foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
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
                                else if (sender.User.Money >= totalPrice)
                                {
                                    foreach (HorseInstance horse in sender.User.HorseInventory.HorseList)
                                    {
                                        horse.BasicStats.Tiredness = 1000;
                                        horse.BasicStats.Thirst = 1000;
                                        horse.BasicStats.Hunger = 1000;
                                    }

                                    byte[] barnedAllHorsesPacket = PacketBuilder.CreateChat(Messages.BarnAllHorsesFullyFed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(barnedAllHorsesPacket);

                                    sender.User.TakeMoney(totalPrice);

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
                    }
                    break;
                case PacketBuilder.HORSE_TRAIN:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    if (randomIdStr == "NaN")
                        break;

                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance trainHorseInst = sender.User.HorseInventory.GetHorseById(randomId);
                        sender.User.LastViewedHorse = trainHorseInst;

                        if (World.InSpecialTile(sender.User.X, sender.User.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                            if (tile.Code != null)
                            {
                                if (tile.Code.StartsWith("TRAINER-"))
                                {
                                    if (trainHorseInst.TrainTimer > 0)
                                    {
                                        byte[] trainSuccessfulMessage = PacketBuilder.CreateChat(Messages.FormatTrainerCantTrainAgainIn(trainHorseInst.TrainTimer), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(trainSuccessfulMessage);
                                        break;
                                    }
                                    string[] trainerInfo = tile.Code.Split('-');
                                    int trainerId = int.Parse(trainerInfo[1]);

                                    Trainer trainer = Trainer.GetTrainerById(trainerId);

                                    if (sender.User.Money >= trainer.MoneyCost)
                                    {
                                        sender.User.TakeMoney(trainer.MoneyCost);
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
                                        if (!sender.User.Subscribed)
                                            trainHorseInst.TrainTimer = 1440;
                                        else
                                            trainHorseInst.TrainTimer = 720;

                                        byte[] trainSuccessfulMessage = PacketBuilder.CreateChat(Messages.FormatTrainedInStatFormat(trainHorseInst.Name, trainer.ImprovesStat), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(trainSuccessfulMessage);


                                        sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Training).Count++;

                                        if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Training).Count >= 1000)
                                            sender.User.Awards.AddAward(Award.GetAwardById(26)); // Pro Trainer
                                        if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Training).Count >= 10000)
                                            sender.User.Awards.AddAward(Award.GetAwardById(53)); // Top Trainer

                                        UpdateArea(sender);
                                    }
                                    else
                                    {
                                        byte[] cantAffordPacket = PacketBuilder.CreateChat(Messages.TrainerCantAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(cantAffordPacket);
                                    }

                                }

                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to use trauber services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_GIVE_FEED:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if(sender.User.LastViewedHorse == null)
                    {
                        Logger.InfoPrint(sender.User.Username + " Tried to feed a non existant horse.");
                        break;
                    }
                    if (sender.User.Inventory.HasItem(randomId))
                    {
                        InventoryItem item = sender.User.Inventory.GetItemByRandomid(randomId);
                        Item.ItemInformation itemInfo = item.ItemInstances[0].GetItemInfo();
                        HorseInstance horseInstance = sender.User.LastViewedHorse;
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
                        sender.User.Inventory.Remove(item.ItemInstances[0]);

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
                            int newHeight = RandomNumberGenerator.Next(horseInstance.Breed.BaseStats.MinHeight, horseInstance.Breed.BaseStats.MaxHeight+1);
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

                        sender.User.MajorPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseFeedMenu(sender.User.LastViewedHorse, sender.User));
                        sender.SendPacket(metaPacket);
                            
                        
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to feed a non existant item to a horse.");
                        break;
                    }
                case PacketBuilder.HORSE_ENTER_ARENA:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseInstance = sender.User.HorseInventory.GetHorseById(randomId);
                        if(World.InSpecialTile(sender.User.X, sender.User.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                            if(tile.Code != null)
                            {
                                if (tile.Code.StartsWith("ARENA-"))
                                {
                                    string[] arenaInfo = tile.Code.Split('-');
                                    int arenaId = int.Parse(arenaInfo[1]);
                                    Arena arena = Arena.GetAreaById(arenaId);
                                    if (!Arena.UserHasEnteredHorseInAnyArena(sender.User))
                                    {
                                        if (horseInstance.BasicStats.Thirst <= 200)
                                        {
                                            byte[] tooThirsty = PacketBuilder.CreateChat(Messages.ArenaTooThirsty, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(tooThirsty);
                                            break;
                                        }
                                        else if (horseInstance.BasicStats.Hunger <= 200)
                                        {
                                            byte[] tooHungry = PacketBuilder.CreateChat(Messages.ArenaTooHungry, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(tooHungry);
                                            break;
                                        }
                                        else if (horseInstance.BasicStats.Shoes <= 200)
                                        {
                                            byte[] needsFarrier = PacketBuilder.CreateChat(Messages.ArenaNeedsFarrier, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(needsFarrier);
                                            break;
                                        }
                                        else if (horseInstance.BasicStats.Tiredness <= 200)
                                        {
                                            byte[] tooTired = PacketBuilder.CreateChat(Messages.ArenaTooTired, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(tooTired);
                                            break;
                                        }
                                        else if (horseInstance.BasicStats.Health <= 200)
                                        {
                                            byte[] needsVet = PacketBuilder.CreateChat(Messages.ArenaNeedsVet, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(needsVet);
                                            break;
                                        }

                                        if (sender.User.Money >= arena.EntryCost)
                                        {
                                            arena.AddEntry(sender.User, horseInstance);
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
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + " Tried to enter a non existant horse into a competition.");
                        break;
                    }
                    break;
                case PacketBuilder.HORSE_RELEASE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        if(World.InTown(sender.User.X, sender.User.Y))
                        {
                            Logger.HackerPrint(sender.User.Username + " Tried to reelease a horse while inside a town....");
                            break;
                        }


                        HorseInstance horseReleaseInst = sender.User.HorseInventory.GetHorseById(randomId);
                        if(sender.User.CurrentlyRidingHorse != null)
                        {
                            if(horseReleaseInst.RandomId == sender.User.CurrentlyRidingHorse.RandomId) 
                            {
                                byte[] errorChatPacket = PacketBuilder.CreateChat(Messages.HorseCantReleaseTheHorseYourRidingOn, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(errorChatPacket);
                                break;
                            }

                        }

                        if (horseReleaseInst.Description == "")
                            horseReleaseInst.Description += Messages.FormatHorseReleasedBy(sender.User.Username);

                        Logger.InfoPrint(sender.User.Username + " RELEASED HORSE: " + horseReleaseInst.Name + " (a " + horseReleaseInst.Breed.Name + ").");

                        sender.User.HorseInventory.DeleteHorse(horseReleaseInst);
                        new WildHorse(horseReleaseInst, sender.User.X, sender.User.Y, 60, true);
                        
                        sender.User.LastViewedHorse = horseReleaseInst;
                        sender.User.MajorPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseReleased());
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to release at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_TACK:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseTackInst = sender.User.HorseInventory.GetHorseById(randomId);
                        
                        sender.User.LastViewedHorse = horseTackInst;
                        sender.User.MajorPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildTackMenu(horseTackInst, sender.User));
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to tack at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_DRINK:
                    if(World.InSpecialTile(sender.User.X, sender.User.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                        if(tile.Code != "POND")
                        {
                            Logger.HackerPrint(sender.User.Username + " Tried to drink from a pond when not on one.");
                            break;
                        }
                    }

                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseDrinkInst = sender.User.HorseInventory.GetHorseById(randomId);

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
                        Logger.HackerPrint(sender.User.Username + " Tried to tack at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_TACK_EQUIP:

                    int itemId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    string itemIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        itemId = int.Parse(itemIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if(Item.ItemIdExist(itemId))
                    {
                        if(sender.User.LastViewedHorse != null)
                        {
                            if(sender.User.LastViewedHorse.AutoSell > 0)
                            {
                                byte[] failMessagePacket = PacketBuilder.CreateChat(Messages.HorseTackFailAutoSell, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(failMessagePacket);
                                break;
                            }

                            if(sender.User.Inventory.HasItemId(itemId))
                            {
                                Item.ItemInformation itemInfo = Item.GetItemById(itemId);
                                if (itemInfo.Type == "TACK")
                                {
                                    switch (itemInfo.GetMiscFlag(0))
                                    {
                                        case 1: // Saddle
                                            if(sender.User.LastViewedHorse.Equipment.Saddle != null)
                                                sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.Saddle.Id));
                                            Database.SetSaddle(sender.User.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.User.LastViewedHorse.Equipment.Saddle = itemInfo;
                                            break;
                                        case 2: // Saddle Pad
                                            if (sender.User.LastViewedHorse.Equipment.SaddlePad != null)
                                                sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.SaddlePad.Id));
                                            Database.SetSaddlePad(sender.User.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.User.LastViewedHorse.Equipment.SaddlePad = itemInfo;
                                            break;
                                        case 3: // Bridle
                                            if (sender.User.LastViewedHorse.Equipment.Bridle != null)
                                                sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.Bridle.Id));
                                            Database.SetBridle(sender.User.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.User.LastViewedHorse.Equipment.Bridle = itemInfo;
                                            break;
                                    }


                                    sender.User.Inventory.Remove(sender.User.Inventory.GetItemByItemId(itemId).ItemInstances[0]); // Remove item from inventory.

                                    sender.User.MajorPriority = true;
                                    byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildTackMenu(sender.User.LastViewedHorse, sender.User));
                                    sender.SendPacket(metaPacket);

                                    byte[] equipMsgPacket = PacketBuilder.CreateChat(Messages.FormatEquipTackMessage(itemInfo.Name, sender.User.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(equipMsgPacket);

                                }
                                else if(itemInfo.Type == "COMPANION")
                                {
                                    if (sender.User.LastViewedHorse.Equipment.Companion != null)
                                        sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.Companion.Id));
                                    Database.SetCompanion(sender.User.LastViewedHorse.RandomId, itemInfo.Id);
                                    sender.User.LastViewedHorse.Equipment.Companion = itemInfo;

                                    sender.User.Inventory.Remove(sender.User.Inventory.GetItemByItemId(itemId).ItemInstances[0]); // Remove item from inventory.

                                    sender.User.MajorPriority = true;
                                    byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseCompanionEquipMenu(sender.User.LastViewedHorse, sender.User));
                                    sender.SendPacket(metaPacket);

                                    byte[] equipMsgPacket = PacketBuilder.CreateChat(Messages.FormatHorseCompanionEquipMessage(sender.User.LastViewedHorse.Name, itemInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(equipMsgPacket);
                                }
                                else
                                {
                                    Logger.ErrorPrint(sender.User.Username + " tried to equip a tack item to a hrose but that item was not of type \"TACK\".");
                                }
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " tried to equip tack he doesnt have");
                                break;
                            }
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.User.Username + " tried to equip tack to a horse when not viewing one.");
                            break;
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " tried to equip tack he doesnt exist");
                        break;
                    }

                    break;
                case PacketBuilder.HORSE_TACK_UNEQUIP:
                    if (sender.User.LastViewedHorse != null)
                    {
                        byte equipSlot = packet[2];
                        switch(equipSlot)
                        {
                            case 0x31: // Saddle
                                if (sender.User.LastViewedHorse.Equipment.Saddle != null)
                                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.Saddle.Id));
                                Database.ClearSaddle(sender.User.LastViewedHorse.RandomId);
                                sender.User.LastViewedHorse.Equipment.Saddle = null;
                                break;
                            case 0x32: // Saddle Pad
                                if (sender.User.LastViewedHorse.Equipment.SaddlePad != null)
                                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.SaddlePad.Id));
                                Database.ClearSaddlePad(sender.User.LastViewedHorse.RandomId);
                                sender.User.LastViewedHorse.Equipment.SaddlePad = null;
                                break;
                            case 0x33: // Bridle
                                if (sender.User.LastViewedHorse.Equipment.Bridle != null)
                                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.Bridle.Id));
                                Database.ClearBridle(sender.User.LastViewedHorse.RandomId);
                                sender.User.LastViewedHorse.Equipment.Bridle = null;
                                break;
                            case 0x34: // Companion
                                if (sender.User.LastViewedHorse.Equipment.Companion != null)
                                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(sender.User.LastViewedHorse.Equipment.Companion.Id));
                                Database.ClearCompanion(sender.User.LastViewedHorse.RandomId);
                                sender.User.LastViewedHorse.Equipment.Companion = null;
                                goto companionRemove;
                            default:
                                Logger.ErrorPrint("Unknown equip slot: " + equipSlot.ToString("X"));
                                break;
                        }
                        byte[] itemUnequipedMessage = PacketBuilder.CreateChat(Messages.FormatUnEquipTackMessage(sender.User.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemUnequipedMessage);

                        if(sender.User.CurrentlyRidingHorse != null)
                        {
                            if(sender.User.CurrentlyRidingHorse.RandomId == sender.User.LastViewedHorse.RandomId)
                            {
                                byte[] disMounted = PacketBuilder.CreateChat(Messages.FormatHorseDismountedBecauseTackedMessage(sender.User.CurrentlyRidingHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.User.Facing %= 5;
                                sender.User.CurrentlyRidingHorse = null;
                                sender.SendPacket(disMounted);
                            }
                        }

                        sender.User.MajorPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildTackMenu(sender.User.LastViewedHorse, sender.User));
                        sender.SendPacket(metaPacket);
                        break;
                    companionRemove:;
                        itemUnequipedMessage = PacketBuilder.CreateChat(Messages.FormatHorseCompanionRemoveMessage(sender.User.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemUnequipedMessage);

                        sender.User.MajorPriority = true;
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseCompanionEquipMenu(sender.User.LastViewedHorse, sender.User));
                        sender.SendPacket(metaPacket);
                        break;

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + " Tried to unequip items from non existnat horse");
                    }
                    break;
                case PacketBuilder.HORSE_DISMOUNT:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    if(randomIdStr == "") // F7 Shortcut
                    { 
                        if(sender.User.CurrentlyRidingHorse != null)
                        {

                            byte[] stopRidingHorseMessagePacket = PacketBuilder.CreateChat(Messages.HorseStopRidingMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(stopRidingHorseMessagePacket);

                            StopRidingHorse(sender);
                        }
                        else
                        {
                            if(sender.User.HorseInventory.HorseIdExist(sender.User.LastRiddenHorse))
                               StartRidingHorse(sender, sender.User.LastRiddenHorse);
                        }
                        break;
                    }

                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {

                        byte[] stopRidingHorseMessagePacket = PacketBuilder.CreateChat(Messages.HorseStopRidingMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(stopRidingHorseMessagePacket);

                        StopRidingHorse(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to dismount at a non existant horse.");
                        break;
                    }
                    break;
                case PacketBuilder.HORSE_MOUNT:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        StartRidingHorse(sender, randomId);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to mount at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_LOOK:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    HorseInstance horseInst;
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        return;
                    }
                    if(sender.User.HorseInventory.HorseIdExist(randomId))
                    {
                        horseInst = sender.User.HorseInventory.GetHorseById(randomId);
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
                            Logger.HackerPrint(sender.User.Username + " Tried to look at a non existant horse.");
                            break;
                        }
                    }

                    break;
                case PacketBuilder.HORSE_ESCAPE:
                    if(WildHorse.DoesHorseExist(sender.User.CapturingHorseId))
                    {
                        WildHorse capturing = WildHorse.GetHorseById(sender.User.CapturingHorseId);
                        sender.User.CapturingHorseId = 0;

                        if (capturing.X == sender.User.X && capturing.Y == sender.User.Y)
                        {
                            capturing.Escape();
                            Logger.InfoPrint(sender.User.Username + " Failed to capture: " + capturing.Instance.Breed.Name + " new location: " + capturing.X + ", " + capturing.Y);

                        }
                    }
                    sender.User.MajorPriority = true;
                    byte[] hoseEscaped = PacketBuilder.CreateMeta(Meta.BuildHorseEscapedMessage());
                    sender.SendPacket(hoseEscaped);
                    break;
                case PacketBuilder.HORSE_CAUGHT:
                    if (WildHorse.DoesHorseExist(sender.User.CapturingHorseId))
                    {
                        WildHorse capturing = WildHorse.GetHorseById(sender.User.CapturingHorseId);
                        sender.User.CapturingHorseId = 0;

                        if (capturing.X == sender.User.X && capturing.Y == sender.User.Y) 
                        {
                            try
                            {
                                capturing.Capture(sender.User);
                            }
                            catch (InventoryFullException)
                            {
                                byte[] chatMsg = PacketBuilder.CreateChat(Messages.TooManyHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMsg);
                                break;
                            }

                            sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count++;

                            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count >= 100)
                                sender.User.Awards.AddAward(Award.GetAwardById(24)); // Wrangler
                            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count >= 1000)
                                sender.User.Awards.AddAward(Award.GetAwardById(25)); // Pro Wrangler

                            Logger.InfoPrint(sender.User.Username + " Captured a: " + capturing.Instance.Breed.Name);

                            sender.User.MajorPriority = true;
                            byte[] horseCaught = PacketBuilder.CreateMeta(Meta.BuildHorseCaughtMessage());
                            sender.SendPacket(horseCaught);

                            break;
                        }
                    }
                    sender.User.MajorPriority = true;
                    byte[] horseAllreadyCaught = PacketBuilder.CreateMeta(Meta.BuildHorseEscapedAnyway());
                    sender.SendPacket(horseAllreadyCaught);
                    break;
                case PacketBuilder.HORSE_TRY_CAPTURE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    try
                    {
                        randomId = int.Parse(randomIdStr);
                        
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid randomid to horse interaction packet ");
                        return;
                    }
                    if (!WildHorse.DoesHorseExist(randomId))
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to catch a horse that doesnt exist.");
                        return;
                    }

                    if(sender.User.HorseInventory.HorseList.Length >= sender.User.MaxHorses)
                    {
                        byte[] caughtTooManyHorses = PacketBuilder.CreateChat(Messages.HorseCatchTooManyHorsesMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(caughtTooManyHorses);
                        return;
                    }

                    sender.User.CapturingHorseId = randomId;
                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.HorseCaptureTimer, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(chatPacket);
                    byte[] swfModulePacket = PacketBuilder.CreateSwfModule("catchhorse", PacketBuilder.PACKET_SWF_MODULE_FORCE);
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
            string dynamicInputStr = packetStr.Substring(1, (packetStr.Length - 1) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
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
                        Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input ");
                        return;
                    }

                    switch(inputId) 
                    {
                        case 1: // Bank
                            if (dynamicInput.Length >= 3)
                            {
                                Int64 moneyDeposited = 0;
                                Int64 moneyWithdrawn = 0;
                                try
                                {
                                    moneyDeposited = Int64.Parse(dynamicInput[1]);
                                    moneyWithdrawn = Int64.Parse(dynamicInput[2]);
                                }
                                catch (Exception)
                                {
                                    Logger.ErrorPrint(sender.User.Username + " tried to deposit/witthdraw NaN money....");
                                    UpdateArea(sender);
                                    break;
                                }
                                
                                // Check if trying to deposit more than can be held in the bank.

                                if (Convert.ToInt64(sender.User.BankMoney) + moneyDeposited > 9999999999)
                                {
                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.BankCantHoldThisMuch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                    UpdateArea(sender);
                                    break;
                                }

                                // Check if trying to deposit more than 2.1B

                                if (moneyWithdrawn > 2100000000)
                                {
                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.BankYouCantHoldThisMuch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                    UpdateArea(sender);
                                    break;
                                }


                                if((moneyDeposited <= sender.User.Money) && moneyDeposited != 0)
                                {
                                    sender.User.TakeMoney(Convert.ToInt32(moneyDeposited));
                                    sender.User.BankMoney += moneyDeposited;

                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatDepositedMoneyMessage(Convert.ToInt32(moneyDeposited)), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                }

                                if ((moneyWithdrawn <= sender.User.BankMoney) && moneyWithdrawn != 0)
                                {
                                    sender.User.BankMoney -= moneyWithdrawn;
                                    sender.User.AddMoney(Convert.ToInt32(moneyWithdrawn));

                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatWithdrawMoneyMessage(Convert.ToInt32(moneyWithdrawn)), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                }

                                UpdateArea(sender);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 2: // Send Mail
                            if(dynamicInput.Length >= 4)
                            {
                                string to = dynamicInput[1];
                                string subject = dynamicInput[2];
                                string message = dynamicInput[3];

                                if(sender.User.Money >= 3)
                                {
                                    if(Database.CheckUserExist(to))
                                    {
                                        int playerId = Database.GetUserId(to);

                                        sender.User.TakeMoney(3);
                                        Mailbox.Mail mailMessage = new Mailbox.Mail();
                                        mailMessage.RandomId = RandomID.NextRandomId();
                                        mailMessage.FromUser = sender.User.Id;
                                        mailMessage.ToUser = playerId;
                                        mailMessage.Timestamp = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                                        mailMessage.Read = false;
                                        mailMessage.Subject = subject;
                                        mailMessage.Message = message;

                                        if(IsUserOnline(playerId))
                                        {
                                            User user = GetUserById(playerId);
                                            user.MailBox.AddMail(mailMessage);

                                            byte[] BaseStatsPacketData = PacketBuilder.CreateMoneyPlayerCountAndMail(user.Money, GameServer.GetNumberOfPlayers(), user.MailBox.UnreadMailCount);
                                            user.Client.SendPacket(BaseStatsPacketData);
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
                                    if(sender.User.AttemptingToOfferItem == -1) // Money
                                    {
                                        string answer = dynamicInput[1];
                                        int amountMoney = -1;
                                        try
                                        {
                                            amountMoney = int.Parse(answer);
                                        }
                                        catch (Exception)
                                        {
                                            byte[] tooMuchMoney = PacketBuilder.CreateChat(Messages.TradeMoneyOfferTooMuch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(tooMuchMoney);
                                            break;
                                        }
                                        if(amountMoney < 0)
                                        {
                                            Logger.ErrorPrint(sender.User.Username + " tried to trade less than 0$");
                                        }
                                        if(amountMoney >= sender.User.Money)
                                        {
                                            byte[] tooMuchMoney = PacketBuilder.CreateChat(Messages.TradeMoneyOfferTooMuch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(tooMuchMoney);
                                            break;
                                        }

                                        sender.User.TradingWith.MoneyOffered = amountMoney;

                                        UpdateArea(sender);
                                        if(sender.User.TradingWith != null)
                                            if (!sender.User.TradingWith.OtherTrade.Trader.TradeMenuPriority)
                                                UpdateArea(sender.User.TradingWith.OtherTrade.Trader.Client);
                                        break;
                                    }



                                    if (Item.ItemIdExist(sender.User.AttemptingToOfferItem))
                                    {
                                        string answer = dynamicInput[1];
                                        int itemCount = -1;
                                        try
                                        {
                                            itemCount = int.Parse(answer);
                                        }
                                        catch(FormatException)
                                        {
                                            Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (Item TRADE, id is NaN)");
                                        }

                                        InventoryItem item = sender.User.Inventory.GetItemByItemId(sender.User.AttemptingToOfferItem);

                                        if (itemCount <= 0)
                                        {
                                            byte[] MustBeAtleast1 = PacketBuilder.CreateChat(Messages.TradeItemOfferAtleast1, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(MustBeAtleast1);
                                            break;
                                        }
                                        if(itemCount > item.ItemInstances.Length)
                                        {
                                            byte[] TooMuchItems = PacketBuilder.CreateChat(Messages.FormatTradeItemOfferTooMuch(item.ItemInstances.Length, itemCount), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            sender.SendPacket(TooMuchItems);
                                            break;
                                        }

                                        foreach(ItemInstance[] existingItems in sender.User.TradingWith.ItemsOffered)
                                        {
                                            if(existingItems[0].ItemId == sender.User.AttemptingToOfferItem)
                                            {
                                                sender.User.TradingWith.RemoveOfferedItems(existingItems);
                                                break;
                                            }
                                        }


                                        
                                        ItemInstance[] items = new ItemInstance[itemCount];
                                        for (int i = 0; i < itemCount; i++)
                                        {
                                            items[i] = item.ItemInstances[i];
                                        }
                                        sender.User.TradingWith.OfferItems(items);

                                        UpdateArea(sender);
                                        if (sender.User.TradingWith != null)
                                            if (!sender.User.TradingWith.OtherTrade.Trader.TradeMenuPriority)
                                                UpdateArea(sender.User.TradingWith.OtherTrade.Trader.Client);
                                    }
                                    break;
                                }
                                else
                                {
                                    Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (Item TRADE, wrong size)");
                                    break;
                                }
                            }
                        case 6: // Riddle Room
                            if (dynamicInput.Length >= 2)
                            {
                                if (sender.User.LastRiddle != null)
                                {
                                    string answer = dynamicInput[1];
                                    if(sender.User.LastRiddle.CheckAnswer(sender.User, answer))
                                        sender.User.LastRiddle = null;
                                    UpdateArea(sender);
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (LastRiddle, wrong size)");
                                break;
                            }
                        case 5: // Horse Description
                            if (dynamicInput.Length >= 3)
                            {
                                if(sender.User.LastViewedHorse != null)
                                {
                                    string desc = dynamicInput[2]; 
                                    string name = dynamicInput[1];
                                    name.Trim();
                                    desc.Trim();
                                    
                                    if(name.Length > 50)
                                    {
                                        byte[] horseNameTooLongPacket = PacketBuilder.CreateChat(Messages.HorseNameTooLongError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(horseNameTooLongPacket);
                                        break;
                                    }

                                    if (desc.Length > 250)
                                    {
                                        byte[] horseNameTooLongPacket = PacketBuilder.CreateChat(Messages.HorseNameTooLongError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(horseNameTooLongPacket);
                                        break;
                                    }

                                    object filterReason = ChatMsg.FilterMessage(name);
                                    if (filterReason != null)
                                    {
                                        byte[] msg = PacketBuilder.CreateChat(Messages.HorseNameViolationsError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(msg);
                                        return;
                                    }

                                    filterReason = ChatMsg.FilterMessage(desc);
                                    if (filterReason != null)
                                    {
                                        ChatMsg.Reason reason = (ChatMsg.Reason)filterReason;
                                        byte[] msg = PacketBuilder.CreateChat(Messages.FormatHorseProfileBlocked(reason.Message), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(msg);
                                        return;
                                    }

                                    sender.User.MajorPriority = true;
                                    sender.User.LastViewedHorse.Name = dynamicInput[1];
                                    sender.User.LastViewedHorse.Description = dynamicInput[2];
                                    byte[] horseProfileSavedPacket = PacketBuilder.CreateChat(Messages.FormatHorseSavedProfileMessage(sender.User.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(horseProfileSavedPacket);
                                    UpdateHorseMenu(sender, sender.User.LastViewedHorse);
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (NPC Search, wrong size)");
                                break;
                            }
                        case 4: // NPC Search
                            if(dynamicInput.Length >= 2)
                            {
                                sender.User.MajorPriority = true;
                                string metaWindow = Meta.BuildNpcSearch(dynamicInput[1]);
                                byte[] metaPacket = PacketBuilder.CreateMeta(metaWindow);
                                sender.SendPacket(metaPacket);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (NPC Search, wrong size)");
                                break;
                            }
                        case 7: // Private Notes
                            if (dynamicInput.Length >= 2)
                            {
                                sender.User.PrivateNotes = dynamicInput[1];
                                UpdateStats(sender);
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.PrivateNotesSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 10: // Change auto sell price
                            if (dynamicInput.Length >= 2)
                            {
                                if (sender.User.LastViewedHorse != null)
                                {
                                    sender.User.MajorPriority = true;
                                    HorseInstance horseInstance = sender.User.LastViewedHorse;
                                    int newSellPrice = 0;
                                    try
                                    {
                                        newSellPrice = int.Parse(dynamicInput[1]);
                                    }
                                    catch (Exception)
                                    {
                                        newSellPrice = 2147483647; // too high
                                    }

                                    if(newSellPrice > 500000000 || newSellPrice < 0)
                                    {
                                        byte[] priceTooHigh = PacketBuilder.CreateChat(Messages.HorseAutoSellValueTooHigh, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(priceTooHigh);
                                        break;
                                    }
                                    byte[] sellPricePacket;
                                    if (newSellPrice > 0)
                                        sellPricePacket = PacketBuilder.CreateChat(Messages.FormatAutoSellConfirmedMessage(newSellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    else
                                        sellPricePacket = PacketBuilder.CreateChat(Messages.HorseAutoSellRemoved, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(sellPricePacket);
                                    horseInstance.AutoSell = newSellPrice;

                                    UpdateHorseMenu(sender, sender.User.LastViewedHorse);
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (autosell, wrong size)");
                                break;
                            }
                            break;
                        case 11: // Ranch Description Edit
                            if (dynamicInput.Length >= 2)
                            {
                                string title = dynamicInput[1];
                                string desc = dynamicInput[2];
                                if(sender.User.OwnedRanch != null)
                                {
                                    title.Trim();
                                    desc.Trim();
                                    if(title.Length > 100)
                                    {
                                        byte[] tooLongPacket = PacketBuilder.CreateChat(Messages.RanchSavedTitleTooLongError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(tooLongPacket);
                                        break;
                                    }
                                    if (desc.Length > 4000)
                                    {
                                        byte[] tooLongPacket = PacketBuilder.CreateChat(Messages.RanchSavedTitleTooLongError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(tooLongPacket);
                                        break;
                                    }

                                    object filterReason = ChatMsg.FilterMessage(title);
                                    if (filterReason != null)
                                    {
                                        byte[] msg = PacketBuilder.CreateChat(Messages.RanchSavedTitleViolationsError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(msg);
                                        return;
                                    }

                                    filterReason = ChatMsg.FilterMessage(desc);
                                    if (filterReason != null)
                                    {
                                        ChatMsg.Reason reason = (ChatMsg.Reason)filterReason;
                                        byte[] msg = PacketBuilder.CreateChat(Messages.FormatRanchDesriptionBlocked(reason.Message), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(msg);
                                        return;
                                    }


                                    sender.User.OwnedRanch.Title = title;
                                    sender.User.OwnedRanch.Description = desc;
                                }
                                byte[] descriptionEditedMessage = PacketBuilder.CreateChat(Messages.RanchSavedRanchDescripton, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(descriptionEditedMessage);

                                /*
                                 * Pinto bug: Saving ranch description will take you to the STATS menu
                                 * instead of just back to your ranch.
                                 */

                                if (ConfigReader.FixOfficalBugs)
                                    UpdateArea(sender);
                                else
                                    UpdateStats(sender); 
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (ranch description, wrong size)");
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

                                    Database.AddReport(sender.User.Username, userName, reason);
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
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 13: // Libary Ranch Search
                            if (dynamicInput.Length >= 2)
                            {
                                string searchQuery = dynamicInput[1];
                                sender.User.MajorPriority = true;
                                byte[] serachResponse = PacketBuilder.CreateMeta(Meta.BuildRanchSearchResults(searchQuery));
                                sender.SendPacket(serachResponse);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 14:
                            if(dynamicInput.Length >= 1)
                            {
                                string password = dynamicInput[1];
                                // Get current tile
                                if(World.InSpecialTile(sender.User.X, sender.User.Y))
                                {
                                    World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                                    if(tile.Code != null)
                                    {
                                        if (tile.Code.StartsWith("PASSWORD-"))
                                        {
                                            string[] args = tile.Code.Replace("!", "-").Split('-');
                                            if (args.Length >= 3)
                                            {
                                                string expectedPassword = args[1];
                                                int questId = int.Parse(args[2]);
                                                if (password.ToLower() == expectedPassword.ToLower())
                                                {
                                                    Quest.CompleteQuest(sender.User, Quest.GetQuestById(questId), false);
                                                }
                                                else
                                                {
                                                    Quest.QuestResult result = Quest.FailQuest(sender.User, Quest.GetQuestById(questId), true);
                                                    if (result.NpcChat == null || result.NpcChat == "")
                                                        result.NpcChat = Messages.IncorrectPasswordMessage;
                                                    byte[] ChatPacket = PacketBuilder.CreateChat(result.NpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                                    sender.SendPacket(ChatPacket);
                                                }
                                            }
                                            else
                                            {
                                                Logger.ErrorPrint(sender.User.Username + " Send invalid password input request. (Too few arguments!)");
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Logger.ErrorPrint(sender.User.Username + " Send password input request. (Not on password tile!)");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    Logger.HackerPrint(sender.User.Username + " Sent a password while not in a special tile.");
                                    break;
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid password request, (wrong size)");
                                break;
                            }

                            break;
                        case 15: // Real Time Quiz
                            if (dynamicInput.Length >= 2)
                            {
                                if(QuizEvent != null)
                                {
                                    if (sender.User.InRealTimeQuiz)
                                    {
                                        RealTimeQuiz.Participent participent = QuizEvent.JoinEvent(sender.User);
                                        string answer = dynamicInput[1];
                                        participent.CheckAnswer(answer);
                                    }
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (RealTimeQuiz, wrong size)");
                                break;
                            }
                        default:
                            Logger.ErrorPrint("Unknown dynamic input: " + inputId.ToString() + " packet dump: " + BitConverter.ToString(packet).Replace("-", " "));
                            break;
                    }


                }
                else
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to send a invalid dynamic input (wrong size)");
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
            if(packet.Length < 2)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent playerinfo packet of wrong size");
            }

            byte method = packet[1];
            switch(method)
            {
                case PacketBuilder.PLAYERINFO_PLAYER_LIST:
                    sender.User.MajorPriority = true;
                    byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildPlayerListMenu(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
            }

        }
        public static void OnDynamicButtonPressed(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Clicked dynamic button when not logged in.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            string buttonIdStr = packetStr.Substring(1, (packetStr.Length - 1) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

            switch(buttonIdStr)
            {
                case "2": // Compose Mail
                    if(sender.User.Money <= 3)
                    {
                        byte[] cantAffordPostage = PacketBuilder.CreateChat(Messages.CityHallCantAffordPostageMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(cantAffordPostage);
                        break;
                    }
                    sender.User.MajorPriority = true;
                    byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildComposeMailMenu());
                    sender.SendPacket(metaPacket);
                    break;
                case "3": // Quest Log
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildQuestLog(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "4": // View Horse Breeds
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseBreedListLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "5": // Back to horse
                    if (sender.User.LastViewedHorse != null)
                        UpdateHorseMenu(sender, sender.User.LastViewedHorse);
                    break;
                case "6": // Equip companion
                    if (sender.User.LastViewedHorse != null)
                    {
                        sender.User.MajorPriority = true;
                        HorseInstance horseInstance = sender.User.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseCompanionEquipMenu(horseInstance,sender.User));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "7": // TP To nearest wagon (ranch)
                    if(sender.User.OwnedRanch != null)
                    {
                        if(sender.User.OwnedRanch.GetBuildingCount(7) > 0) // Wagon
                        {
                            int ranchX = sender.User.OwnedRanch.X;
                            int ranchY = sender.User.OwnedRanch.Y;

                            double smallestDistance = Double.PositiveInfinity;
                            int smalestTransportPointId = 0;
                            for (int i = 0; i < Transport.TransportPoints.Count; i++) 
                            {
                                Transport.TransportPoint tpPoint = Transport.TransportPoints[i];

                                if(Transport.GetTransportLocation(tpPoint.Locations[0]).Type == "WAGON") // is wagon?
                                {
                                    double distance = Helper.PointsToDistance(ranchX, ranchY, tpPoint.X, tpPoint.Y);
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
                            sender.User.Teleport(newX, newY);
                        }
                    }
                    break;
                case "8":
                    if(sender.User.LastViewedHorse != null)
                    {
                        sender.User.MajorPriority = true;
                        HorseInstance horseInstance = sender.User.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseReleaseConfirmationMessage(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "9": // View Tack (Libary)
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildTackLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "10": // View Companions (Libary)
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildCompanionLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "11": // Randomize horse name
                    if (sender.User.LastViewedHorse != null)
                    {
                        sender.User.MajorPriority = true;
                        HorseInstance horseInstance = sender.User.LastViewedHorse;
                        horseInstance.ChangeNameWithoutUpdatingDatabase(HorseInfo.GenerateHorseName());
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseDescriptionEditMeta(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "12": // View Minigames (Libary)
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildMinigamesLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "13": // Train All (Ranch)
                    if (sender.User.OwnedRanch != null)
                    {
                        if(sender.User.OwnedRanch.GetBuildingCount(6) > 0) // Training Pen
                        {
                            sender.User.MajorPriority = true;
                            metaPacket = PacketBuilder.CreateMeta(Meta.BuildRanchTraining(sender.User));
                            sender.SendPacket(metaPacket);
                        }
                    }
                    break;
                case "14": // Most Valued Ranches
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildMostValuedRanches());
                    sender.SendPacket(metaPacket);
                    break;
                case "15": // Most Richest Players
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildRichestPlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "16": // Most Adventurous Players
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildAdventurousPlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "17": // Most Experienced Players
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildExperiencedPlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "18": // Best Minigame Players
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildMinigamePlayers());
                    sender.SendPacket(metaPacket);
                    break;
                case "19": // Most Experienced Horses
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildMostExperienedHoses());
                    sender.SendPacket(metaPacket);
                    break;
                case "20": // Minigame Rankings
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildMinigameRankingsForUser(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "21": // Private Notes
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildPrivateNotes(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "22": // View Locations (Libary)
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildLocationsLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "23": // View Awards (Libary)
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildAwardsLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "26": // Buy Horse (Auto Sell)
                    if(sender.User.LastViewedHorseOther != null)
                    {
                        bool isOnRanch = false;
                        bool isOnPlayer = false;
                        HorseInstance horseToSell = sender.User.LastViewedHorseOther;
                        if (Ranch.GetOwnedRanch(horseToSell.Owner))
                        {
                            Ranch ranch = Ranch.GetRanchOwnedBy(horseToSell.Owner);
                            if(sender.User.X == ranch.X && sender.User.Y == ranch.Y)
                            {
                                isOnRanch = true;
                            }

                        }
                        if(GameServer.IsUserOnline(horseToSell.Owner))
                        {
                            User user = GameServer.GetUserById(horseToSell.Owner);
                            if (user.X == sender.User.X && user.Y == sender.User.Y)
                            {
                                isOnPlayer = true;
                            }
                        }

                        if (isOnRanch || isOnPlayer)
                        {
                            
                            if (horseToSell.AutoSell == 0)
                                break;
                            if(sender.User.Money >= horseToSell.AutoSell)
                            {
                                if (sender.User.HorseInventory.HorseList.Length + 1 > sender.User.MaxHorses)
                                {
                                    byte[] tooManyHorses = PacketBuilder.CreateChat(Messages.AutoSellTooManyHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(tooManyHorses);
                                    break;
                                }

                                sender.User.TakeMoney(horseToSell.AutoSell);

                                if (IsUserOnline(horseToSell.Owner))
                                {
                                    User seller = GetUserById(horseToSell.Owner);
                                    seller.HorseInventory.DeleteHorse(horseToSell, false);
                                    seller.AddMoney(horseToSell.AutoSell);

                                    byte[] horseBrought = PacketBuilder.CreateChat(Messages.FormatAutoSellSold(horseToSell.Name, horseToSell.AutoSell, sender.User.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    seller.Client.SendPacket(horseBrought);
                                }
                                else
                                {
                                    Database.AddMessageToQueue(horseToSell.Owner, Messages.FormatAutoSellSoldOffline(horseToSell.Name, horseToSell.AutoSell, sender.User.Username));
                                    try
                                    {
                                        Database.SetPlayerMoney((Database.GetPlayerMoney(horseToSell.Owner) + horseToSell.AutoSell), horseToSell.Owner);
                                    }
                                    catch (OverflowException)
                                    {
                                        Database.SetPlayerMoney(2147483647, horseToSell.Owner);
                                    }
                                }

                                horseToSell.Owner = sender.User.Id;
                                horseToSell.AutoSell = 0;
                                sender.User.HorseInventory.AddHorse(horseToSell, false);

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
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildAwardList(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "27": // Ranch Edit
                    if(sender.User.OwnedRanch != null)
                    {
                        sender.User.MajorPriority = true;
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildRanchEdit(sender.User.OwnedRanch));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "29": // Auto Sell Horses
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildTopAutoSellHorses());
                    sender.SendPacket(metaPacket);
                    break;
                case "31":
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildRanchSearchLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "35": // Buddy List
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildBuddyList(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "36": // Nearby list
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildNearbyList(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "37": // All Players List
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildPlayerList(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "40": // All Players Alphabetical
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildPlayerListAlphabetical(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "30": // Find NPC
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildFindNpcMenu());
                    sender.SendPacket(metaPacket);
                    break;
                case "25": // Set auto sell price
                    if (sender.User.LastViewedHorse != null)
                    {
                        sender.User.MajorPriority = true;
                        HorseInstance horseInstance = sender.User.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildAutoSellMenu(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "33": // View All stats (Horse)
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildAllBasicStats(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "34": // View Basic stats (Horse)
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildAllStats(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "38": // Read Books
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildBooksLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "41": // Put horse into auction
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildAuctionHorseList(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "47":
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildPawneerOrderBreedList());
                    sender.SendPacket(metaPacket);
                    break;
                case "53": // Misc Stats / Tracked Items
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildMiscStats(sender.User));
                    sender.SendPacket(metaPacket);
                    break;
                case "58": // Add new item to trade
                    if(sender.User.TradingWith != null)
                    {
                        sender.User.TradeMenuPriority = true;
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildTradeAdd(sender.User.TradingWith));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "59": // Done
                    if (sender.User.TradingWith != null)
                    {
                        sender.User.TradingWith.Stage = "DONE";

                        if (sender.User.TradingWith != null)
                            if (sender.User.TradingWith.OtherTrade.Trader.TradeMenuPriority == false)
                                UpdateArea(sender.User.TradingWith.OtherTrade.Trader.Client);
                        UpdateArea(sender);

                    }
                    break;
                case "60": // Ranch Sell
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildRanchSellConfirmation());
                    sender.SendPacket(metaPacket);
                    break;
                case "61": // Most Spoiled Horse
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildMostSpoiledHorses());
                    sender.SendPacket(metaPacket);
                    break;
                case "28c1": // Abuse Report
                    sender.User.MajorPriority = true;
                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildAbuseReportPage());
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
                    if (sender.User.LastViewedHorse != null)
                    {
                        sender.User.LastViewedHorse.Category = category;
                        byte[] categoryChangedPacket = PacketBuilder.CreateChat(Messages.FormatHorseSetToNewCategory(category), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(categoryChangedPacket);

                        sender.User.MajorPriority = true;
                        UpdateHorseMenu(sender, sender.User.LastViewedHorse);
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
                            Logger.DebugPrint(sender.User.Username + " Tried to read a book of id NaN");
                            break;
                        };

                        if(Book.BookExists(bookId))
                        {
                            Book book = Book.GetBookById(bookId);
                            sender.User.MajorPriority = true;
                            metaPacket = PacketBuilder.CreateMeta(Meta.BuildBookReadLibary(book));
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.HackerPrint(sender.User.Username + "Tried to read a book that doesnt exist.");
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
                            Logger.DebugPrint(sender.User.Username + " Tried to whisper a horse with BreedId NaN.");
                            break;
                        };

                        if (sender.User.Money < 50000)
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
                        sender.User.MajorPriority = true;

                        byte[] pricingMessage = PacketBuilder.CreateChat(Messages.FormatWhispererPrice(cost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(pricingMessage);

                        byte[] serachResultMeta = PacketBuilder.CreateMeta(Meta.BuildWhisperSearchResults(horsesFound.ToArray()));
                        sender.SendPacket(serachResultMeta);

                        sender.User.TakeMoney(cost);
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
                            Logger.DebugPrint(sender.User.Username + " Sent invalid libary breed viewer request.");
                            break;
                        };
                        sender.User.MajorPriority = true;
                        string metaTag = Meta.BuildBreedViewerLibary(horseBreed);
                        metaPacket = PacketBuilder.CreateMeta(metaTag);
                        sender.SendPacket(metaPacket);

                        string swf = "breedviewer.swf?terrain=book&breed=" + horseBreed.Swf + "&j=";
                        byte[] loadSwf = PacketBuilder.CreateSwfModule(swf, PacketBuilder.PACKET_SWF_MODULE_FORCE);
                        sender.SendPacket(loadSwf);

                        break;

                    }
                    else if (buttonIdStr.StartsWith("50c"))
                    {
                        string gender = buttonIdStr.Substring(3);
                        if (sender.User.PawneerOrderBreed != null)
                        {
                            if (sender.User.PawneerOrderBreed.GenderTypes().Contains(gender))
                            {
                                if (sender.User.Inventory.HasItemId(Item.PawneerOrder))
                                {
                                    sender.User.PawneerOrderGender = gender;

                                    HorseInstance horseInstance = new HorseInstance(sender.User.PawneerOrderBreed);
                                    horseInstance.Color = sender.User.PawneerOrderColor;
                                    horseInstance.Gender = sender.User.PawneerOrderGender;
                                    horseInstance.Name = "Pawneer Order";

                                    sender.User.Inventory.Remove(sender.User.Inventory.GetItemByItemId(Item.PawneerOrder).ItemInstances[0]);
                                    sender.User.HorseInventory.AddHorse(horseInstance, true, true);

                                    sender.User.MajorPriority = true;
                                    metaPacket = PacketBuilder.CreateMeta(Meta.BuildPawneerOrderFound(horseInstance));
                                    sender.SendPacket(metaPacket);
                                    break;
                                }
                            }
                        }
                        Logger.ErrorPrint(sender.User.Username + " Error occured when doing a Pawneer Order.");
                        break;
                    }
                    else if (buttonIdStr.StartsWith("49c"))
                    {
                        string color = buttonIdStr.Substring(3);
                        if (sender.User.PawneerOrderBreed != null)
                        {
                            if (sender.User.PawneerOrderBreed.Colors.Contains(color))
                            {
                                sender.User.PawneerOrderColor = color;

                                sender.User.MajorPriority = true;
                                metaPacket = PacketBuilder.CreateMeta(Meta.BuildPawneerOrderGenderList(sender.User.PawneerOrderBreed, color));
                                sender.SendPacket(metaPacket);
                                break;
                            }
                        }
                        Logger.ErrorPrint(sender.User.Username + " Asked for a horse of an unknown color " + color);
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
                            Logger.DebugPrint(sender.User.Username + " Tried to pawner order a horse with id NaN.");
                            break;
                        }
                        sender.User.PawneerOrderBreed = breed;

                        sender.User.MajorPriority = true;
                        metaPacket = PacketBuilder.CreateMeta(Meta.BuildPawneerOrderColorList(breed));
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
                            Logger.DebugPrint(sender.User.Username + " Tried to pawn a horse with id NaN.");
                            break;
                        }

                        if (sender.User.HorseInventory.HorseIdExist(horseId))
                        {
                            HorseInstance inst = sender.User.HorseInventory.GetHorseById(horseId);
                            int price = Pawneer.CalculateTotalPrice(inst);
                            string name = inst.Name;

                            sender.User.HorseInventory.DeleteHorse(inst); // 1000% a "distant land.."
                            sender.User.LastViewedHorse = null;

                            sender.User.AddMoney(price);
                            byte[] soldHorseMessage = PacketBuilder.CreateChat(Messages.FormatPawneerSold(name, price), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(soldHorseMessage);

                            sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count++;
                            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count >= 100)
                                sender.User.Awards.AddAward(Award.GetAwardById(44)); // Vendor
                            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count >= 1000)
                                sender.User.Awards.AddAward(Award.GetAwardById(45)); // Pro Vendor
                            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorsePawn).Count >= 10000)
                                sender.User.Awards.AddAward(Award.GetAwardById(52)); // Top Vendor

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
                            Logger.DebugPrint(sender.User.Username + " Tried to pawn a horse with id NaN.");
                            break;
                        }

                        if (sender.User.HorseInventory.HorseIdExist(horseId))
                        {
                            HorseInstance inst = sender.User.HorseInventory.GetHorseById(horseId);

                            sender.User.MajorPriority = true;
                            byte[] confirmScreen = PacketBuilder.CreateMeta(Meta.BuildPawneerConfimation(inst));
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
                            Logger.DebugPrint(sender.User.Username + " Tried to auction a horse with id NaN.");
                            break;
                        }
                        if (sender.User.HorseInventory.HorseIdExist(horseId))
                        {
                            HorseInstance inst = sender.User.HorseInventory.GetHorseById(horseId);

                            if(World.InSpecialTile(sender.User.X, sender.User.Y))
                            {
                                World.SpecialTile tile = World.GetSpecialTile(sender.User.X, sender.User.Y);
                                if(tile.Code == null || !tile.Code.StartsWith("AUCTION-"))
                                {
                                    Logger.ErrorPrint("Cant find auction room that " + sender.User.Username + " Is trying to place a horse in.");
                                    return;
                                }
                                Auction auctionRoom = Auction.GetAuctionRoomById(int.Parse(tile.Code.Split('-')[1]));
                                if(auctionRoom.HasUserPlacedAuctionAllready(sender.User))
                                {
                                    byte[] cantPlaceAuction = PacketBuilder.CreateChat(Messages.AuctionOneHorsePerPlayer, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantPlaceAuction);
                                    break;
                                }
                                if (sender.User.Money >= 1000)
                                {
                                    sender.User.TakeMoney(1000);
                                    Auction.AuctionEntry entry = new Auction.AuctionEntry(8, 0, sender.User.Id);
                                    entry.Horse = inst;
                                    entry.OwnerId = sender.User.Id;
                                    entry.Completed = false;
                                    inst.Hidden = true;
                                    auctionRoom.AddEntry(entry);
                                    UpdateArea(sender);
                                    UpdateAreaForAll(sender.User.X, sender.User.Y, true);
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
                            Logger.HackerPrint(sender.User.Username + " Tried to auction a horse they did not have.");
                            break;
                        }
                    }
                    if (Leaser.LeaserButtonIdExists(buttonIdStr))
                    {
                        Leaser horseLeaser = Leaser.GetLeaserByButtonId(buttonIdStr);

                        if(sender.User.Money >= horseLeaser.Price)
                        {
                            if(sender.User.HorseInventory.HorseList.Length + 1 > sender.User.MaxHorses)
                            {
                                byte[] cantManageHorses = PacketBuilder.CreateChat(Messages.HorseLeaserHorsesFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cantManageHorses);
                                break;
                            }
                            else
                            {
                                sender.User.MajorPriority = true;
                                sender.User.TakeMoney(horseLeaser.Price);
                                
                                HorseInstance leaseHorse = horseLeaser.GenerateLeaseHorse();
                                
                                if(leaseHorse.Breed.Id == 170) // UniPeg
                                {
                                    sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnipegTeamup).Count++;
                                    if(sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnipegTeamup).Count >= 5)
                                        sender.User.Awards.AddAward(Award.GetAwardById(55)); // UniPeg Friend
                                }
                                else if(leaseHorse.Breed.Type == "unicorn")
                                {
                                    sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnicornTeamup).Count++;
                                    if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.UnicornTeamup).Count >= 5)
                                        sender.User.Awards.AddAward(Award.GetAwardById(42)); // Unicorn Friend
                                }
                                else if(leaseHorse.Breed.Type == "pegasus")
                                {
                                    sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PegasusTeamup).Count++;
                                    if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PegasusTeamup).Count >= 5)
                                        sender.User.Awards.AddAward(Award.GetAwardById(43)); // Pegasus Friend
                                }

                                sender.User.HorseInventory.AddHorse(leaseHorse);

                                byte[] addedHorseMeta = PacketBuilder.CreateMeta(Meta.BuildLeaserOnLeaseInfo(horseLeaser));
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
                        sender.User.MajorPriority = true;
                        metaPacket = PacketBuilder.CreateMeta(AbuseReport.GetReasonById(buttonIdStr).Meta);
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
            if(packet.Length <= 2)
            {
                Logger.ErrorPrint(sender.User.Username + "Sent invalid Arena Scored Packet.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            string scoreStr = packetStr.Substring(1, (packet.Length - 1) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
            int score = -1;
            try
            {
                score = int.Parse(scoreStr);
            }
            catch(FormatException)
            {
                Logger.ErrorPrint(sender.User.Username + " Scored NAN in an arena.");
                return;
            }

            if(Arena.UserHasEnteredHorseInAnyArena(sender.User))
            {
                byte[] waitingOnResults = PacketBuilder.CreateChat(Messages.FormatArenaYourScore(score), PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(waitingOnResults);

                Arena enteredArena = Arena.GetArenaUserEnteredIn(sender.User);
                enteredArena.SubmitScore(sender.User, score);
            }
            else
            {
                Logger.ErrorPrint(sender.User.Username + " Scored in an arena while not in one");
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

            // Prevent duplicate requests.
            if (sender.User.UserInfoSend) 
                return;

            sender.User.UserInfoSend = true;

            // This allows the website to view that the player is online.
            Database.AddOnlineUser(sender.User.Id, sender.User.Administrator, sender.User.Moderator, sender.User.Subscribed, sender.User.NewPlayer);
            
            Logger.DebugPrint(sender.User.Username + " Requested user information.");
            User user = sender.User;

            // Send player current location & map data
            byte[] MovementPacket = PacketBuilder.CreateMovement(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
            sender.SendPacket(MovementPacket);

            // Send "Welcome to the Secret Land of Horses" message.
            byte[] WelcomeMessage = PacketBuilder.CreateChat(Messages.FormatWelcomeMessage(user.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(WelcomeMessage);

            // Send weather effects, and current server time.
            byte[] WorldData = PacketBuilder.CreateTimeAndWeatherUpdate(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, sender.User.GetWeatherSeen());
            sender.SendPacket(WorldData);

            // if the player is logging in for the first time, send Welcome newest rider of Horse Isle message.
            if (sender.User.NewPlayer)
            {
                byte[] NewUserMessage = PacketBuilder.CreateChat(Messages.NewUserMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(NewUserMessage);
            }

            // Send Security Codes, used (badly) to verify Minigame Rewards
            byte[] SecCodePacket = PacketBuilder.CreateSecCode(user.SecCodeSeeds, user.SecCodeInc, user.Administrator, user.Moderator);
            sender.SendPacket(SecCodePacket);

            // Send player money count, total players and total unread mail.
            byte[] BaseStatsPacketData = PacketBuilder.CreateMoneyPlayerCountAndMail(user.Money, GameServer.GetNumberOfPlayers(), user.MailBox.UnreadMailCount);
            sender.SendPacket(BaseStatsPacketData);

            // Sends Meta Window information (Nearby, current tile, etc)
            UpdateArea(sender);

            /*
             *  Send all nearby players locations to the client
             *  if there not nearby, say there at 1000,1000.
             */
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (client.User.Id != user.Id)
                    {
                        if(IsOnScreen(client.User.X, client.User.Y, sender.User.X, sender.User.Y))
                        {
                            byte[] PlayerInfo = PacketBuilder.CreatePlayerInfoUpdateOrCreate(client.User.X, client.User.Y, client.User.Facing, client.User.CharacterId, client.User.Username);
                            sender.SendPacket(PlayerInfo);
                        }
                        else
                        {
                            byte[] PlayerInfo = PacketBuilder.CreatePlayerInfoUpdateOrCreate(1000+4, 1000+1, client.User.Facing, client.User.CharacterId, client.User.Username);
                            sender.SendPacket(PlayerInfo);
                        }
                    }
                }
            }

            /*
             *  Update all nearby users 
             *  that the new player logged in.
             */
            foreach (User nearbyUser in GameServer.GetNearbyUsers(sender.User.X, sender.User.Y, false, false))
                if (nearbyUser.Id != sender.User.Id)
                    if(!nearbyUser.MajorPriority)
                        if(!nearbyUser.MinorPriority)
                            UpdateArea(nearbyUser.Client);

            /*
             * Send a list of isles, towns and areas to the player
             * This is used for the world map.
             */

            byte[] IsleData = PacketBuilder.CreatePlaceData(World.Isles.ToArray(), World.Towns.ToArray(), World.Areas.ToArray());
            sender.SendPacket(IsleData);

            // Tells the client which tiles are passable, which the player should appear ontop of and which it should be below.
            byte[] TileFlags = PacketBuilder.CreateTileOverlayFlags(Map.OverlayTileDepth);
            sender.SendPacket(TileFlags);

            // Send Todays Note:
            byte[] MotdData = PacketBuilder.CreateMotd(Messages.FormatMotd(ConfigReader.Motd));
            sender.SendPacket(MotdData);

            // Send riddle annoucement
            if (RiddleEvent != null)
                if (RiddleEvent.Active)
                    RiddleEvent.ShowStartMessage(sender);

            /*
             *  Gives Queued Items
             *  When you buy a PO from the store on the website
             *  its added to this queued items list.
             */
            DoItemPurchases(sender);

            // Send Queued Messages
            string[] queuedMessages = Database.GetMessageQueue(sender.User.Id);
            foreach(string queuedMessage in queuedMessages)
            {
                byte[] msg = PacketBuilder.CreateChat(Messages.MessageQueueHeader+queuedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(msg);
            }
            Database.ClearMessageQueue(sender.User.Id);

            // Send "Playername Logged in" message
            byte[] loginMessageBytes = PacketBuilder.CreateChat(Messages.FormatLoginMessage(sender.User.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    if (!client.User.MuteLogins && !client.User.MuteAll)
                        if (client.User.Id != sender.User.Id)
                            client.SendPacket(loginMessageBytes);


            /*
             *  Send players nearby to you 
             *  your position, otherwise just send 1000,1000
             */
            byte[] yourPlayerInfo = PacketBuilder.CreatePlayerInfoUpdateOrCreate(sender.User.X, sender.User.Y, sender.User.Facing, sender.User.CharacterId, sender.User.Username);
            byte[] yourPlayerInfoOffscreen = PacketBuilder.CreatePlayerInfoUpdateOrCreate(1000 + 4, 1000 + 1, sender.User.Facing, sender.User.CharacterId, sender.User.Username);

            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (client.User.Id != sender.User.Id)
                    {
                        if (IsOnScreen(client.User.X, client.User.Y, sender.User.X, sender.User.Y))
                            client.SendPacket(yourPlayerInfo);
                        else
                            client.SendPacket(yourPlayerInfoOffscreen);
                    }
                }
            }

        }

        public static void OnSwfModuleCommunication(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " tried to send swf communication when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid swf commmunication Packet");
                return;
            }


            byte module = packet[1];
            switch(module)
            {
                case PacketBuilder.SWFMODULE_INVITE:
                    if(packet.Length < 3)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid 2PLAYER INVITE Packet (WRONG SIZE)");
                        break;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (Exception) { };

                    if(IsUserOnline(playerId))
                    {
                        User toInvite = GetUserById(playerId);
                        TwoPlayer twoPlayerGame = new TwoPlayer(toInvite, sender.User, false);
                    }
                    break;
                case PacketBuilder.SWFMODULE_ACCEPT:
                    if (packet.Length < 3)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid 2PLAYER ACCEPT Packet (WRONG SIZE)");
                        break;
                    }
                    packetStr = Encoding.UTF8.GetString(packet);
                    playerIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    playerId = -1;
                    try
                    {
                        playerId = int.Parse(playerIdStr);
                    }
                    catch (Exception) { };

                    if (IsUserOnline(playerId))
                    {
                        User toAccept = GetUserById(playerId);
                        if(TwoPlayer.IsPlayerInvitingPlayer(toAccept, sender.User))
                        {
                            TwoPlayer twoPlayerGame = TwoPlayer.GetGameInvitingPlayer(toAccept, sender.User);
                            twoPlayerGame.Accept(sender.User);
                        }
                    }
                    break;
                case PacketBuilder.SWFMODULE_DRAWINGROOM:
                    if(packet.Length < 3)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent invalid DRAWINGROOM packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if(packet[2] == PacketBuilder.DRAWINGROOM_GET_DRAWING)
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
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
                            Logger.ErrorPrint(sender.User.Username + " tried to load an invalid drawing room: " + roomId);
                            break;   
                        }

                        if(room.Drawing != "")
                        {
                            byte[] drawingPacket = PacketBuilder.CreateDrawingUpdate(room.Drawing);
                            sender.SendPacket(drawingPacket);
                        }

                    }
                    else if(packet[2] == PacketBuilder.DRAWINGROOM_SAVE)
                    {
                        if (packet.Length < 4)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
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
                            Logger.ErrorPrint(sender.User.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        if (!Database.SavedDrawingsExist(sender.User.Id))
                            Database.CreateSavedDrawings(sender.User.Id);

                        int slotNo = 0;
                        byte slot = packet[4];
                        switch (slot)
                        {
                            case 0x29: // Slot 1
                                Database.SaveDrawingSlot1(sender.User.Id, room.Drawing);
                                slotNo = 1;
                                break;
                            case 0x2A: // Slot 2
                                Database.SaveDrawingSlot2(sender.User.Id, room.Drawing);
                                slotNo = 2;
                                break;
                            case 0x2B: // Slot 3
                                Database.SaveDrawingSlot3(sender.User.Id, room.Drawing);
                                slotNo = 3;
                                break;
                        }

                        byte[] savedDrawingMessage = PacketBuilder.CreateChat(Messages.FormatDrawingRoomSaved(slotNo), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(savedDrawingMessage);

                        break;
                    }
                    else if (packet[2] == PacketBuilder.DRAWINGROOM_LOAD)
                    {
                        if (packet.Length < 4)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        if(!sender.User.Subscribed)
                        {
                            byte[] notSubscribedCantLoad = PacketBuilder.CreateChat(Messages.DrawingCannotLoadNotSubscribed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notSubscribedCantLoad);
                            break;
                        }

                        int roomId = packet[3] - 40;
                        Drawingroom room;
                        try{
                            room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch (KeyNotFoundException){
                            Logger.ErrorPrint(sender.User.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        if (!Database.SavedDrawingsExist(sender.User.Id))
                            Database.CreateSavedDrawings(sender.User.Id);

                        int slotNo = 0;
                        byte slot = packet[4];
                        string drawingToAdd = "";
                        switch (slot)
                        {
                            case 0x29: // Slot 1
                                drawingToAdd = Database.LoadDrawingSlot1(sender.User.Id);
                                slotNo = 1;
                                break;
                            case 0x2A: // Slot 2
                                drawingToAdd = Database.LoadDrawingSlot2(sender.User.Id);
                                slotNo = 2;
                                break;
                            case 0x2B: // Slot 3
                                drawingToAdd = Database.LoadDrawingSlot3(sender.User.Id);
                                slotNo = 3;
                                break;
                        }

                        try {
                            room.Drawing += drawingToAdd;
                        }
                        catch(DrawingroomFullException){
                            byte[] roomFullMessage = PacketBuilder.CreateChat(Messages.DrawingPlzClearLoad, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(roomFullMessage);
                            break;
                        }
                        
                        Database.SetLastPlayer("D" + room.Id.ToString(), sender.User.Id);
                        UpdateDrawingForAll("D" + room.Id, sender, drawingToAdd, true);

                        byte[] loadedDrawingMessage = PacketBuilder.CreateChat(Messages.FormatDrawingRoomLoaded(slotNo), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(loadedDrawingMessage);

                        break;
                    }
                    else // Default action- draw line
                    {
                        if (packet.Length < 4)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        if(!sender.User.Subscribed)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Tried to draw while not subscribed.");
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
                            Logger.ErrorPrint(sender.User.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        packetStr = Encoding.UTF8.GetString(packet);
                        
                        string drawing = packetStr.Substring(3, (packetStr.Length - 3) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        if (drawing.Contains("X!")) // Clear byte
                        {  
                            room.Drawing = "";
                            goto update;
                        }

                        try { 
                            room.Drawing += drawing;
                        }
                        catch (DrawingroomFullException)
                        {
                            byte[] roomFullMessage = PacketBuilder.CreateChat(Messages.DrawingPlzClearDraw, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(roomFullMessage);
                            break;
                        }
                    update:;
                        Database.SetLastPlayer("D" + room.Id.ToString(), sender.User.Id);
                        UpdateDrawingForAll("D" + room.Id, sender, drawing, false);
                    }

                    break;
                case PacketBuilder.SWFMODULE_BRICKPOET:
                    if(packet.Length < 4)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent invalid BRICKPOET packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if(packet[2] == PacketBuilder.BRICKPOET_LIST_ALL)
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid BRICKPOET LIST ALL packet (swf communication, WRONG SIZE)");
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
                            Logger.ErrorPrint(sender.User.Username + " tried to load an invalid brickpoet room: " + roomId);
                            break;
                        }
                        // Send list of peices
                        byte[] poetPacket = PacketBuilder.CreateBrickPoetList(room);
                        sender.SendPacket(poetPacket);

                    }
                    else if(packet[3] == PacketBuilder.BRICKPOET_MOVE)
                    {
                        if (packet.Length < 0xA)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid BRICKPOET MOVE packet (swf communication, WRONG SIZE)");
                            break;
                        }
                        packetStr = Encoding.UTF8.GetString(packet);
                        if(!packetStr.Contains('|'))
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid BRICKPOET MOVE packet (swf communication, NO | SEPERATOR)");
                            break;
                        }
                        string[] args = packetStr.Split('|');
                        if(args.Length < 5)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid BRICKPOET MOVE Packet (swf communication, NOT ENOUGH | SEPERATORS.");
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
                        catch (Exception e)
                        {
                            Logger.ErrorPrint(sender.User.Username + " brickpoet - "+e.ToString());
                            break;
                        }
                        // Change location in Database
                        peice.X = x;
                        peice.Y = y;

                        foreach(User user in GetUsersOnSpecialTileCode("MULTIROOM-" + "P" + roomId.ToString())) // Send to each user!
                        {
                            if (user.Id == sender.User.Id)
                                continue;

                            byte[] updatePoetRoomPacket = PacketBuilder.CreateBrickPoetMove(peice);
                            user.Client.SendPacket(updatePoetRoomPacket);
                            
                        }

                        if (Database.GetLastPlayer("P" + roomId) != sender.User.Id)
                        {
                            Database.SetLastPlayer("P" + roomId, sender.User.Id);
                            UpdateAreaForAll(sender.User.X, sender.User.Y, true);
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
                    if ( packet.Length < 5 )
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent invalid DRESSUPROOM packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if (packet[2] == PacketBuilder.DRESSUPROOM_LIST_ALL)
                    {
                        int roomId = packet[3] - 40;
                        Dressup.DressupRoom room = Dressup.GetDressupRoom(roomId);

                        if (room.DressupPeices.Length > 0)
                        {
                            byte[] allDressupsResponse = PacketBuilder.CreateDressupRoomPeiceLoad(room.DressupPeices);
                            sender.SendPacket(allDressupsResponse);
                        }

                    }
                    else // Move
                    {
                        if (packet.Length < 8)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        int roomId = packet[2] - 40;
                        if (roomId <= 0)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, INVALID ROOM)");
                            break;
                        }
                        Dressup.DressupRoom room = Dressup.GetDressupRoom(roomId);

                        packetStr = Encoding.UTF8.GetString(packet);
                        string moveStr = packetStr.Substring(3, (packetStr.Length - 3) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                        string[] moves = moveStr.Split('|');

                        if(moves.Length < 3)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, MOVES WRONG SIZE)");
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
                            Logger.ErrorPrint(sender.User.Username + " Sent invalid DRESSUPROOM MOVE packet (swf communication, INVALID LOCATION)");
                            break;
                        }

                        Dressup.DressupPeice peice = room.GetDressupPeice(peiceId);
                        // Update database entries
                        peice.X = Convert.ToInt32(Math.Round(moveToX));
                        peice.Y = Convert.ToInt32(Math.Round(moveToY));
                        peice.Active = active;

                        // Forward to other users
                        byte[] movePeicePacket = PacketBuilder.CreateDressupRoomPeiceMove(peice.PeiceId, moveToX, moveToY, peice.Active);
                        User[] users = GetUsersAt(sender.User.X, sender.User.Y, true, true);
                        foreach(User user in users)
                        {
                            if (user.Id != sender.User.Id)
                                user.Client.SendPacket(movePeicePacket);
                        }
                    }
                    break;
                case PacketBuilder.SWFMODULE_BROADCAST:
                    byte[] response = PacketBuilder.CreateForwardedSwfModule(packet);
                    foreach (User user in GetUsersAt(sender.User.X, sender.User.Y))
                    {
                        if (user.Id == sender.User.Id)
                            continue;
                        user.Client.SendPacket(response);
                    }
                    break;
                case PacketBuilder.SWFMODULE_OPPONENT: 
                    if(TwoPlayer.IsPlayerInGame(sender.User))
                    {
                        TwoPlayer twoPlayerGame = TwoPlayer.GetTwoPlayerGameInProgress(sender.User);

                        User otherUser = null;
                        if (twoPlayerGame.Invitee.Id == sender.User.Id)
                            otherUser = twoPlayerGame.Inviting;
                        else if (twoPlayerGame.Inviting.Id == sender.User.Id)
                            otherUser = twoPlayerGame.Invitee;

                        response = PacketBuilder.CreateForwardedSwfModule(packet);
                        otherUser.Client.SendPacket(response);
                    }
                    break;
                case PacketBuilder.SWFMODULE_CLOSE:
                    if (TwoPlayer.IsPlayerInGame(sender.User))
                    {
                        TwoPlayer twoPlayerGame = TwoPlayer.GetTwoPlayerGameInProgress(sender.User);

                        User otherUser = null;
                        if (twoPlayerGame.Invitee.Id == sender.User.Id)
                            otherUser = twoPlayerGame.Inviting;
                        else if (twoPlayerGame.Inviting.Id == sender.User.Id)
                            otherUser = twoPlayerGame.Invitee;

                        response = PacketBuilder.Create2PlayerClose();
                        otherUser.Client.SendPacket(response);

                        twoPlayerGame.CloseGame(sender.User);

                        
                    }
                    break;
                case PacketBuilder.SWFMODULE_ARENA:
                    if (Arena.UserHasEnteredHorseInAnyArena(sender.User))
                    { 
                        Arena arena = Arena.GetArenaUserEnteredIn(sender.User);
                        response = PacketBuilder.CreateForwardedSwfModule(packet);
                        foreach (Arena.ArenaEntry entry in arena.Entries.ToArray())
                        {
                            if (entry.EnteredUser.Id == sender.User.Id)
                                continue;
                            if(entry.EnteredUser.Client.LoggedIn)
                            entry.EnteredUser.Client.SendPacket(response);
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

            if(packet.Length < 3)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid wish Packet");
                return;
            }

            if (!sender.User.Inventory.HasItemId(Item.WishingCoin))
            {
                Logger.HackerPrint(sender.User.Username + " Tried to use a wishing well while having 0 coins.");
                return;
            }

            InventoryItem wishingCoinInvItems = sender.User.Inventory.GetItemByItemId(Item.WishingCoin);
            byte wishType = packet[1];
            string message = "";

            byte[] chatMsg = PacketBuilder.CreateChat(Messages.TossedCoin, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(chatMsg);

            switch(wishType)
            {
                case PacketBuilder.WISH_MONEY:
                    int gainMoney = RandomNumberGenerator.Next(500, 1000);
                    sender.User.AddMoney(gainMoney);
                    message = Messages.FormatWishMoneyMessage(gainMoney);
                    break;
                case PacketBuilder.WISH_ITEMS:
                    Item.ItemInformation[] wishableItmes = Item.GetAllWishableItems();
                    int item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    Item.ItemInformation itm = wishableItmes[item];
                    item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    Item.ItemInformation itm2 = wishableItmes[item];

                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(itm.Id));
                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(itm2.Id));

                    message = Messages.FormatWishThingsMessage(itm.Name, itm2.Name);
                    break;
                case PacketBuilder.WISH_WORLDPEACE:
                    byte[] tooDeep = PacketBuilder.CreateChat(Messages.WorldPeaceOnlySoDeep, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(tooDeep);

                    wishableItmes = Item.GetAllWishableItems();
                    item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    int earnMoney = RandomNumberGenerator.Next(0, 500);
                    itm = wishableItmes[item];


                    sender.User.AddMoney(earnMoney);
                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(itm.Id));

                    message = Messages.FormatWishWorldPeaceMessage(earnMoney, itm.Name);
                    break;
                default:
                    Logger.ErrorPrint("Unknnown Wish type: " + wishType.ToString("X"));
                    break;
            }
            sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count++;

            if(sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count >= 100)
                sender.User.Awards.AddAward(Award.GetAwardById(30)); // Well Wisher

            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count >= 1000)
                sender.User.Awards.AddAward(Award.GetAwardById(31)); // Star Wisher

            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count >= 10000)
                sender.User.Awards.AddAward(Award.GetAwardById(51)); // Extraordanary Wisher

            byte[] msg = PacketBuilder.CreateChat(message, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(msg);

            sender.User.Inventory.Remove(wishingCoinInvItems.ItemInstances[0]);
            UpdateArea(sender);
        }
        public static void OnKeepAlive(GameClient sender, byte[] packet)
        {
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid update Packet");
                return;
            }

            if (packet[1] == PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                Logger.DebugPrint("Received KEEP_ALIVE from: " + sender.User.Username);
                return;
            }
        }
        public static void OnStatsPacket(GameClient sender, byte[] packet)
        {
            if(!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested stats when not logged in.");
                return;
            }
            if(packet.Length < 2)
            {
                Logger.ErrorPrint(sender.User.Username + "Sent an invalid Stats Packet");
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
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid Profile Packet");
                return;
            }

            byte method = packet[1];
            if (method == PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                UpdateStats(sender);
            }
            if (method == PacketBuilder.VIEW_PROFILE)
            {
                sender.User.MajorPriority = true;
                string profilePage = sender.User.ProfilePage;
                byte[] profilePacket = PacketBuilder.CreateProfilePage(profilePage);
                sender.SendPacket(profilePacket);
            }
            else if (method == PacketBuilder.SAVE_PROFILE)
            {

                string packetStr = Encoding.UTF8.GetString(packet);
                if (packet.Length <= 3 || !packetStr.Contains('|'))
                {
                    Logger.ErrorPrint(sender.User.Username + " Sent an invalid Profile SAVE Packet");
                    return;
                }

                int characterId = (packet[2] - 20) * 64 + (packet[3] - 20);

                string profilePage = packetStr.Split('|')[1];
                profilePage = profilePage.Substring(0, profilePage.Length - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                sender.User.CharacterId = characterId;
                
                if (profilePage.Length > 4000)
                {
                    byte[] notSaved = PacketBuilder.CreateChat(Messages.ProfileTooLongMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(notSaved);
                    return;
                }

                object filterReason = ChatMsg.FilterMessage(profilePage);
                if(filterReason != null)
                {
                    ChatMsg.Reason reason = (ChatMsg.Reason)filterReason;
                    byte[] msg = PacketBuilder.CreateChat(Messages.FormatProfileSavedBlocked(reason.Message), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(msg);
                    return;
                }

                sender.User.ProfilePage = profilePage;

                Logger.DebugPrint(sender.User.Username + " Changed to character id: " + characterId + " and set there Profile Description to '" + profilePage + "'");

                byte[] chatPacket = PacketBuilder.CreateChat(Messages.ProfileSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatPacket);

                UpdateArea(sender);
                UpdateUserFacingAndLocation(sender.User);
            }
            else if (method == PacketBuilder.SECCODE_AWARD)
            {
                byte[] ExpectedSecCode = sender.User.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.User.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent a seccode AWARD request with invalid size");
                        return;
                    }

                    string packetStr = Encoding.UTF8.GetString(packet);
                    string awardIdStr = packetStr.Substring(6, (packetStr.Length - 6) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    int value = -1;
                    try
                    {
                        value = int.Parse(awardIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent correct sec code, but invalid awardid value");
                        return;
                    }

                    sender.User.Awards.AddAward(Award.GetAwardById(value));
                    return;
                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.User.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_SCORE || method == PacketBuilder.SECCODE_TIME || method == PacketBuilder.SECCODE_WINLOOSE)
            {
                bool time = (method == PacketBuilder.SECCODE_TIME);
                bool winloose = (method == PacketBuilder.SECCODE_WINLOOSE);

                byte[] ExpectedSecCode = sender.User.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.User.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent a seccode score/time/winloose request with invalid size");
                        return;
                    }

                    
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string gameInfoStr = packetStr.Substring(6, (packetStr.Length - 6) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    if (winloose)
                    {
                        string gameTitle = gameInfoStr.Substring(1);
                        byte pmethod = packet[6];
                        if(pmethod == PacketBuilder.WINLOOSE_WIN)
                        {
                            sender.User.Highscores.Win(gameTitle);
                            byte[] winMsg = PacketBuilder.CreateChat(Messages.Format2PlayerRecordWin(gameTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(winMsg);
                        }
                        else if(pmethod == PacketBuilder.WINLOOSE_LOOSE)
                        {
                            sender.User.Highscores.Loose(gameTitle);
                            byte[] looseMsg = PacketBuilder.CreateChat(Messages.Format2PlayerRecordLose(gameTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(looseMsg);
                        }

                        if (sender.User.Highscores.HighscoreList.Length >= 30)
                            sender.User.Awards.AddAward(Award.GetAwardById(12)); // Minigame Player

                        if (sender.User.Highscores.HighscoreList.Length >= 60)
                            sender.User.Awards.AddAward(Award.GetAwardById(13)); // Minigame Master

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.User.Id) >= 1000)
                            sender.User.Awards.AddAward(Award.GetAwardById(14)); // Minigame Nut

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.User.Id) >= 10000)
                            sender.User.Awards.AddAward(Award.GetAwardById(15)); // Minigame Crazy
                        return;
                    }
                    if (gameInfoStr.Contains("|"))
                    {
                        string[] gameInfo = gameInfoStr.Split('|');
                        if (gameInfo.Length < 2)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent a invalid seccode score request");
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
                            Logger.ErrorPrint(sender.User.Username + " Sent correct sec code, but invalid score value");
                            return;
                        }
                        Highscore.HighscoreTableEntry[] scores = Database.GetTopScores(gameTitle, 5, !time);
                        bool bestScoreEver = false;
                        if (scores.Length >= 1)
                            bestScoreEver = scores[0].Score <= value;

                        bool newHighscore = sender.User.Highscores.UpdateHighscore(gameTitle, value, time);
                        if(bestScoreEver && !time)
                        {
                            byte[] bestScoreBeaten = PacketBuilder.CreateChat(Messages.BeatBestHighscore, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(bestScoreBeaten);
                            sender.User.AddMoney(2500);
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
                        
                        if(sender.User.Highscores.HighscoreList.Length >= 30)
                            sender.User.Awards.AddAward(Award.GetAwardById(12)); // Minigame Player

                        if (sender.User.Highscores.HighscoreList.Length >= 60)
                            sender.User.Awards.AddAward(Award.GetAwardById(13)); // Minigame Master

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.User.Id) >= 1000)
                            sender.User.Awards.AddAward(Award.GetAwardById(14)); // Minigame Nut

                        if (Database.GetPlayerTotalMinigamesPlayed(sender.User.Id) >= 10000)
                            sender.User.Awards.AddAward(Award.GetAwardById(15)); // Minigame Crazy

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + " didnt send a game name AND a score.");
                        return;
                    }

                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.User.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_MONEY)
            {

                byte[] ExpectedSecCode = sender.User.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.User.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent a seccode money request with invalid size");
                        return;
                    }

                    string packetStr = Encoding.UTF8.GetString(packet);
                    string gameInfoStr = packetStr.Substring(6, (packetStr.Length - 6) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    if (gameInfoStr.Contains("|"))
                    {
                        string[] moneyInfo = gameInfoStr.Split('|');
                        if (moneyInfo.Length < 2)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent a invalid money score request");
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
                            Logger.ErrorPrint(sender.User.Username + " Sent correct sec code, but invalid money value");
                            return;
                        }

                        int moneyEarned = value * 10;
                        Logger.InfoPrint(sender.User.Username + " Earned $" + moneyEarned + " In: " + id);

                        sender.User.AddMoney(moneyEarned);
                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatMoneyEarnedMessage(moneyEarned), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + " didnt send a game name AND a score.");
                        return;
                    }

                }
            }
            else if (method == PacketBuilder.SECCODE_GIVE_ITEM)
            {
                byte[] ExpectedSecCode = sender.User.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.User.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent a seccode item request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, (packetStr.Length - 6) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (Item.ItemIdExist(value))
                    {
                        ItemInstance itm = new ItemInstance(value);

                        Item.ItemInformation itemInfo = Item.GetItemById(value);
                        string messageToSend = Messages.FormatYouEarnedAnItemMessage(itemInfo.Name);
                        try
                        {
                            sender.User.Inventory.Add(itm);
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
                        Logger.HackerPrint(sender.User.Username + " Sent correct sec code, but tried to give an non existant item");
                        return;
                    }

                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.User.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_DELETE_ITEM)
            {
                byte[] ExpectedSecCode = sender.User.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.User.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent a seccode item request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, (packetStr.Length - 6) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (sender.User.Inventory.HasItemId(value))
                    {
                        InventoryItem item = sender.User.Inventory.GetItemByItemId(value);
                        sender.User.Inventory.Remove(item.ItemInstances[0]);

                        Item.ItemInformation itemInfo = Item.GetItemById(value);
                        byte[] lostItemMessage = PacketBuilder.CreateChat(Messages.FormatYouLostAnItemMessage(itemInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(lostItemMessage);

                        UpdateArea(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Sent correct sec code, but tried to delete an non existant item");
                        return;
                    }

                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.User.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_QUEST)
            {
                byte[] ExpectedSecCode = sender.User.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.User.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent a seccode quest request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, (packetStr.Length - 6) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.HackerPrint(sender.User.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (Quest.DoesQuestExist(value))
                    {
                        Quest.QuestEntry questEntry = Quest.GetQuestById(value);
                        Quest.ActivateQuest(sender.User, questEntry);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Sent correct sec code, but tried to activate a non existant quest");
                        return;
                    }


                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.User.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.PROFILE_HIGHSCORES_LIST)
            {
                sender.User.MajorPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                byte[] metaTag = PacketBuilder.CreateMeta(Meta.BuildTopHighscores(gameName));
                sender.SendPacket(metaTag);
            }
            else if (method == PacketBuilder.PROFILE_BESTTIMES_LIST)
            {
                sender.User.MajorPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                byte[] metaTag = PacketBuilder.CreateMeta(Meta.BuildTopTimes(gameName));
                sender.SendPacket(metaTag);
            }
            else if (method == PacketBuilder.PROFILE_WINLOOSE_LIST)
            {
                sender.User.MajorPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                byte[] metaTag = PacketBuilder.CreateMeta(Meta.BuildTopWinners(gameName));
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


            User loggedInUser = sender.User;

            /*
             *  Player stuff
             */

            // Store this for later... do it now to avoid TOCTOU.
            User[] onScreenBefore = GetOnScreenUsers(loggedInUser.X, loggedInUser.Y, true, true);

            // Leave Multirooms 
            Multiroom.LeaveAllMultirooms(loggedInUser);

            loggedInUser.PendingBuddyRequestTo = null;

            // Close Social Windows
            foreach (User sUser in loggedInUser.BeingSocializedBy)
                UpdateArea(sUser.Client);
            loggedInUser.ClearSocailizedWith();


            if (loggedInUser.CurrentlyRidingHorse != null)
            {
                if(loggedInUser.CurrentlyRidingHorse.BasicStats.Experience < 25)
                {
                    if(GameServer.RandomNumberGenerator.Next(0, 100) == 97)
                    {
                        loggedInUser.CurrentlyRidingHorse.BasicStats.Experience++;
                        byte[] horseBuckedMessage;
                        if(loggedInUser.CurrentlyRidingHorse.Breed.Type == "llama")
                            horseBuckedMessage = PacketBuilder.CreateChat(Messages.HorseLlamaBuckedYou, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        else if (loggedInUser.CurrentlyRidingHorse.Breed.Type == "camel")
                            horseBuckedMessage = PacketBuilder.CreateChat(Messages.HorseCamelBuckedYou, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        else
                            horseBuckedMessage = PacketBuilder.CreateChat(Messages.HorseBuckedYou, PacketBuilder.CHAT_BOTTOM_RIGHT);

                        sender.User.CurrentlyRidingHorse = null;
                        sender.User.Facing %= 5;
                        sender.SendPacket(horseBuckedMessage);
                    }
                }
            }

            // Randomly move if thirst, hunger, tiredness too low-

            byte movementDirection = packet[1];

            if (loggedInUser.Thirst <= 0 || loggedInUser.Hunger <= 0 || loggedInUser.Tiredness <= 0)
            {
                if (RandomNumberGenerator.Next(0, 10) == 7)
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
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatThirst.ToUpper(), Messages.StatThirstDizzy), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }
                            else if (loggedInUser.Hunger <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatHunger.ToUpper(), Messages.StatHungerStumble), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }
                            /*
                             * Doesnt appear to acturally exist.
                             * 
                            else if (loggedInUser.Tiredness <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatTired.ToUpper()), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }
                            */
                        }
                    }
                }
            }



            int onHorse = 0;
            int facing = sender.User.Facing;
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
                byte[] moveResponse = PacketBuilder.CreateMovement(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, Direction, true);
                sender.SendPacket(moveResponse);
                goto Complete;               
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
                        goto Complete;
                    }
                }

                byte[] moveResponse = PacketBuilder.CreateMovement(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, direction, true);
                sender.SendPacket(moveResponse);
            }
            else
            {
                byte[] moveResponse = PacketBuilder.CreateMovement(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_NONE, false);
                sender.SendPacket(moveResponse);
            }
            Complete:;

            // Cancel Trades
            if (loggedInUser.TradingWith != null)
                if ((loggedInUser.TradingWith.Trader.X != loggedInUser.X) && (loggedInUser.TradingWith.Trader.Y != loggedInUser.Y))
                    loggedInUser.TradingWith.CancelTradeMoved();
            
            // Pac-man the world.
            if (loggedInUser.X > Map.Width)
                loggedInUser.Teleport(2, loggedInUser.Y);
            else if (loggedInUser.X < 2)
                loggedInUser.Teleport(Map.Width - 2, loggedInUser.Y);
            else if (loggedInUser.Y > Map.Height - 2)
                loggedInUser.Teleport(loggedInUser.X, 2);
            else if (loggedInUser.Y < 2)
                loggedInUser.Teleport(loggedInUser.X, Map.Height - 2);


            User[] onScreenNow = GetOnScreenUsers(loggedInUser.X, loggedInUser.Y, true, true);

            User[] goneOffScreen = onScreenBefore.Except(onScreenNow).ToArray();
            User[] goneOnScreen = onScreenNow.Except(onScreenBefore).ToArray();

            foreach (User offScreenUsers in goneOffScreen)
            {
                if (offScreenUsers.Id == loggedInUser.Id)
                    continue;

                byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(1000 + 4, 1000 + 1, loggedInUser.Facing, loggedInUser.CharacterId, loggedInUser.Username);
                offScreenUsers.Client.SendPacket(playerInfoBytes);
            }

            foreach (User onScreenUsers in goneOnScreen)
            {
                if (onScreenUsers.Id == loggedInUser.Id)
                    continue;

                byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(onScreenUsers.X, onScreenUsers.Y, onScreenUsers.Facing, onScreenUsers.CharacterId, onScreenUsers.Username);
                loggedInUser.Client.SendPacket(playerInfoBytes);
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
            Logger.InfoPrint(sender.User.Username + " Clicked \"Quit Game\".. Disconnecting");
            sender.Disconnect();
        }
        public static void OnNpcInteraction(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent npc interaction packet when not logged in.");
                return;
            }
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid npc interaction packet.");
                return;
            }
            byte action = packet[1];
            if (action == PacketBuilder.NPC_START_CHAT)
            {

                string packetStr = Encoding.UTF8.GetString(packet);
                string number = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                int chatId = 0;
                
                try
                {
                    chatId = int.Parse(number);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to start talking to an NPC with id that is NaN.");
                    return;
                }
                if(!Npc.NpcExists(chatId))
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to start talking to an NPC that doesnt exist.");
                    return;
                }
                sender.User.MajorPriority = true;
                Npc.NpcEntry entry = Npc.GetNpcById(chatId);
                
                if(entry.Chatpoints.Length <= 0)
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to start talking to an NPC with no chatpoints.");
                    return;
                }

                int defaultChatpointId = Npc.GetDefaultChatpoint(sender.User, entry);
                Npc.NpcChat startingChatpoint = Npc.GetNpcChatpoint(entry, defaultChatpointId);

                string metaInfo = Meta.BuildNpcChatpoint(sender.User, entry, startingChatpoint);
                byte[] metaPacket = PacketBuilder.CreateMeta(metaInfo);
                sender.SendPacket(metaPacket);

                sender.User.LastTalkedToNpc = entry;
            }
            else if (action == PacketBuilder.NPC_CONTINUE_CHAT)
            {
                string packetStr = Encoding.UTF8.GetString(packet);
                string number = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                int replyId = 0;
                try
                {
                    replyId = int.Parse(number);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to reply to an NPC with replyid that is NaN.");
                    return;
                }

                Npc.NpcEntry lastNpc = sender.User.LastTalkedToNpc;
                Npc.NpcReply reply;
                try
                {
                    reply = Npc.GetNpcReply(lastNpc, replyId);
                }
                catch(KeyNotFoundException)
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to reply with replyid that does not exist.");
                    return;
                }

                if (reply.GotoChatpoint == -1)
                {
                    UpdateArea(sender);
                    return;
                }
                sender.User.MajorPriority = true;
                string metaInfo = Meta.BuildNpcChatpoint(sender.User, lastNpc, Npc.GetNpcChatpoint(lastNpc, reply.GotoChatpoint));
                byte[] metaPacket = PacketBuilder.CreateMeta(metaInfo);
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
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid transport packet.");
                return;
            }


            string packetStr = Encoding.UTF8.GetString(packet);
            string number = packetStr.Substring(1, (packetStr.Length - 1) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

            int transportid;
            try
            {
                transportid =  Int32.Parse(number);
            }
            catch(FormatException)
            {
                Logger.ErrorPrint(sender.User.Username + " Tried to use a transport with id that is NaN.");
                return;
            }
            try
            {
                Transport.TransportPoint transportPoint = Transport.GetTransportPoint(sender.User.X, sender.User.Y);
                if (transportPoint.X != sender.User.X && transportPoint.Y != sender.User.Y)
                {
                    Logger.HackerPrint(sender.User.Username + " Tried to use transport id: " + transportid.ToString() + " while not the correct transport point!");
                    return;
                }

                Transport.TransportLocation transportLocation = Transport.GetTransportLocation(transportid);
                int cost = transportLocation.Cost;

                if (transportLocation.Type == "WAGON")
                {
                    if(sender.User.OwnedRanch != null)
                    {
                        if(sender.User.OwnedRanch.GetBuildingCount(7) > 0) // Wagon
                        {
                            cost = 0;
                        }
                    }
                }

                if (sender.User.Bids.Length > 0)
                {
                    byte[] cantBuyWhileAuctioning = PacketBuilder.CreateChat(Messages.AuctionNoOtherTransactionAllowed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantBuyWhileAuctioning);
                    return;
                }


                if (sender.User.Money >= cost)
                {
                    string swfToLoad = Messages.BoatCutscene;
                    if (transportLocation.Type == "WAGON")
                        swfToLoad = Messages.WagonCutscene;

                    if (transportLocation.Type != "ROWBOAT")
                    {
                        byte[] swfModulePacket = PacketBuilder.CreateSwfModule(swfToLoad, PacketBuilder.PACKET_SWF_MODULE_CUTSCENE);
                        sender.SendPacket(swfModulePacket);
                    }

                    sender.User.Teleport(transportLocation.GotoX, transportLocation.GotoY);
                    sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count++;


                    if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count >= 500)
                        sender.User.Awards.AddAward(Award.GetAwardById(27)); // Traveller
                    if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count >= 5000)
                        sender.User.Awards.AddAward(Award.GetAwardById(28)); // Globetrotter

                    byte[] welcomeToIslePacket = PacketBuilder.CreateChat(Messages.FormatWelcomeToAreaMessage(transportLocation.LocationTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(welcomeToIslePacket);

                    if(cost > 0)
                        sender.User.TakeMoney(cost);
                }
                else
                {
                    byte[] cantAfford = PacketBuilder.CreateChat(Messages.CantAffordTransport, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantAfford);
                }
            }
            catch (KeyNotFoundException)
            {
                Logger.HackerPrint(sender.User.Username + " Tried to use transport id: " + transportid.ToString() + " while not on a transport point!");
            }

         
        }
        public static void OnRanchPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent ranch packet when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid ranch packet.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            byte method = packet[1];

            if (method == PacketBuilder.RANCH_INFO)
            {
                string buildingIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.User.Username + " tried to get info for building id NaN");
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
                    Logger.ErrorPrint(sender.User.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_SELL)
            {
                string NanSTR = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                if (NanSTR == "NaN")
                {
                    if (sender.User.OwnedRanch == null)
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to sell there ranch when they didnt own one.");
                        return;
                    }
                    int sellPrice = sender.User.OwnedRanch.GetSellPrice();
                    sender.User.AddMoney(sellPrice);
                    byte[] sellPacket = PacketBuilder.CreateChat(Messages.FormatRanchSoldMessage(sellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.User.OwnedRanch.OwnerId = -1;
                    sender.SendPacket(sellPacket);

                    // Change map sprite.
                    User[] users = GetUsersAt(sender.User.X, sender.User.Y, true, true);
                    foreach (User user in users)
                    {
                        byte[] MovementPacket = PacketBuilder.CreateMovement(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
                        user.Client.SendPacket(MovementPacket);
                    }
                    UpdateAreaForAll(sender.User.X, sender.User.Y, true);
                }
                else
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to sell there ranch without sending NaN.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_UPGRADE)
            {
                string NanSTR = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                if (NanSTR == "NaN")
                {
                    if (sender.User.OwnedRanch != null)
                    {
                        Ranch.RanchUpgrade currentUpgrade = sender.User.OwnedRanch.GetRanchUpgrade();

                        if (!Ranch.RanchUpgrade.RanchUpgradeExists(currentUpgrade.Id + 1))
                        {
                            Logger.ErrorPrint(sender.User.Username + " Tried to upgrade there ranch when it was max upgrade.");
                            return;
                        }

                        Ranch.RanchUpgrade nextUpgrade = Ranch.RanchUpgrade.GetRanchUpgradeById(currentUpgrade.Id + 1);
                        if (sender.User.Money >= nextUpgrade.Cost)
                        {
                            sender.User.TakeMoney(nextUpgrade.Cost);
                            sender.User.OwnedRanch.InvestedMoney += nextUpgrade.Cost;
                            sender.User.OwnedRanch.UpgradedLevel++;

                            byte[] upgraded = PacketBuilder.CreateChat(Messages.UpgradedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(upgraded);

                            // Change map sprite.
                            User[] users = GetUsersAt(sender.User.X, sender.User.Y, true, true);
                            foreach (User user in users)
                            {
                                byte[] MovementPacket = PacketBuilder.CreateMovement(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
                                user.Client.SendPacket(MovementPacket);
                            }
                            UpdateAreaForAll(sender.User.X, sender.User.Y, true);
                        }
                        else
                        {
                            byte[] cantAfford = PacketBuilder.CreateChat(Messages.UpgradeCannotAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAfford);
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to upgrade there ranch when they didnt own one.");
                    }
                }
                else
                {
                    Logger.ErrorPrint(sender.User.Username + " Tried to upgrade there ranch without sending NaN.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_REMOVE)
            {
                string buildingIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.User.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);
                    int ranchBuild = sender.User.LastClickedRanchBuilding;
                    if (ranchBuild <= 0)
                        return;
                    if (sender.User.OwnedRanch != null)
                    {
                        if (ranchBuild > sender.User.OwnedRanch.GetRanchUpgrade().Limit)
                        {
                            Logger.HackerPrint(sender.User.Username + " Tried to remove more buildings than the limit.");
                            return;
                        }
                        
                        Ranch.RanchBuilding ranchBuilding = sender.User.OwnedRanch.GetBuilding(ranchBuild - 1);

                        if (ranchBuilding == null)
                            return;

                        if (ranchBuilding.Id == buildingId)
                        {
                            sender.User.OwnedRanch.SetBuilding(ranchBuild - 1, null);
                            sender.User.AddMoney(ranchBuilding.GetTeardownPrice());
                            sender.User.OwnedRanch.InvestedMoney -= building.Cost;
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatBuildingTornDown(ranchBuilding.GetTeardownPrice()), PacketBuilder.CHAT_BOTTOM_RIGHT);

                            sender.SendPacket(chatPacket);
                            UpdateAreaForAll(sender.User.X, sender.User.Y, true);
                            return;
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.User.Username + " Tried to remove bulidingid: " + buildingId + " from building slot " + ranchBuild + " but the building was not found there.");
                        }

                    }
                    Logger.HackerPrint(sender.User.Username + " Tried to remove in a ranch when they dont own one.");
                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.User.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_BUILD)
            {
                string buildingIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.User.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);
                    int ranchBuild = sender.User.LastClickedRanchBuilding;
                    if (ranchBuild == 0)
                        return;
                    if (sender.User.OwnedRanch != null)
                    {
                        if (ranchBuild > sender.User.OwnedRanch.GetRanchUpgrade().Limit)
                        {
                            Logger.HackerPrint(sender.User.Username + " Tried to build more buildings than the limit.");
                            return;
                        }

                        if (sender.User.Money >= building.Cost)
                        {
                            sender.User.OwnedRanch.SetBuilding(ranchBuild - 1, building);
                            sender.User.OwnedRanch.InvestedMoney += building.Cost;
                            sender.User.TakeMoney(building.Cost);
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.RanchBuildingComplete, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                            UpdateAreaForAll(sender.User.X, sender.User.Y, true);
                            return;

                        }
                        else
                        {
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.RanchCantAffordThisBuilding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                            return;
                        }
                    }
                    Logger.HackerPrint(sender.User.Username + " Tried to build in a ranch when they dont own one.");
                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.User.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_BUY)
            {
                string nan = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                if (nan == "NaN")
                {
                    if (Ranch.IsRanchHere(sender.User.X, sender.User.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.User.X, sender.User.Y);
                        if (sender.User.Money >= ranch.Value)
                        {
                            byte[] broughtRanch = PacketBuilder.CreateChat(Messages.FormatRanchBroughtMessage(ranch.Value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(broughtRanch);
                            sender.User.TakeMoney(ranch.Value);
                            ranch.OwnerId = sender.User.Id;
                            ranch.InvestedMoney += ranch.Value;
                            UpdateAreaForAll(sender.User.X, sender.User.Y, true);

                        }
                        else
                        {
                            byte[] cantAfford = PacketBuilder.CreateChat(Messages.RanchCantAffordRanch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAfford);
                        }
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + " Tried to buy a non existant ranch.");
                        return;
                    }
                }
                else
                {
                    Logger.ErrorPrint(sender.User.Username + " Sent RANCH_BUY without \"NaN\".");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_CLICK)
            {
                if (packet.Length < 5)
                {
                    Logger.ErrorPrint(sender.User.Username + " Sent an invalid ranch click packet.");
                    return;
                }
                byte action = packet[2];
                if (action == PacketBuilder.RANCH_CLICK_BUILD)
                {
                    if (Ranch.IsRanchHere(sender.User.X, sender.User.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.User.X, sender.User.Y);
                        if (sender.User.OwnedRanch != null)
                        {
                            if (sender.User.OwnedRanch.Id == ranch.Id)
                            {
                                int buildSlot = packet[3] - 40;
                                sender.User.LastClickedRanchBuilding = buildSlot;
                                sender.User.MajorPriority = true;

                                if (buildSlot == 0)
                                {
                                    byte[] buildingsAvalible = PacketBuilder.CreateMeta(Meta.BuildRanchUpgrade(ranch));
                                    sender.SendPacket(buildingsAvalible);

                                }
                                else
                                {
                                    byte[] buildingsAvalible = PacketBuilder.CreateMeta(Meta.BuildRanchBuildingsAvalible(ranch, buildSlot));
                                    sender.SendPacket(buildingsAvalible);
                                }


                                return;
                            }
                        }
                    }

                    Logger.HackerPrint(sender.User.Username + " Tried to build in a ranch they didnt own.");
                    return;
                }
                else if (action == PacketBuilder.RANCH_CLICK_NORM)
                {
                    if (Ranch.IsRanchHere(sender.User.X, sender.User.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.User.X, sender.User.Y);
                        int buildSlot = packet[3] - 40;
                        sender.User.MajorPriority = true;

                        if (buildSlot == 0) // Main Building
                        {
                            byte[] upgradeDescription = PacketBuilder.CreateMeta(Meta.BuildRanchBuilding(ranch, ranch.GetRanchUpgrade()));
                            sender.SendPacket(upgradeDescription);
                        }
                        else // Other Building
                        {
                            byte[] buildingDescription = PacketBuilder.CreateMeta(Meta.BuildRanchBuilding(ranch, ranch.GetBuilding(buildSlot - 1)));
                            sender.SendPacket(buildingDescription);
                        }
                        return;
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + " sent an Unknown ranch packet " + BitConverter.ToString(packet).Replace("-", " "));
                    }
                }
            }
            else
            {
                Logger.ErrorPrint(sender.User.Username + " sent an Unknown ranch packet " + BitConverter.ToString(packet).Replace("-", " "));
            }
        }
        public static void OnChatPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent chat packet when not logged in.");
                return;
            }

            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid chat packet.");
                return;
            }


            string packetStr = Encoding.UTF8.GetString(packet);

            ChatMsg.ChatChannel channel = (ChatMsg.ChatChannel)packet[1];
            string message = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

            Logger.DebugPrint(sender.User.Username + " Attempting to say '" + message + "' in channel: " + channel.ToString());

            string nameTo = null;
            if (channel == ChatMsg.ChatChannel.Dm)
            {
                nameTo = ChatMsg.GetDmRecipiant(message);
                message = ChatMsg.GetDmMessage(message);
            }

            if (message == "")
                return;

            if (message.StartsWith("/"))
            {
                string channelString = message.Split(' ')[0].ToLower();
                string newMessage = string.Join(' ', message.Split(' ').Skip(1));
                message = newMessage.Trim();
                
                if (message == "")
                    channelString = "/";

                switch (channelString)
                {
                    case "/$":
                    case "/ads":
                        channel = ChatMsg.ChatChannel.Ads;
                        break;
                    case "/a":
                    case "/all":
                        channel = ChatMsg.ChatChannel.All;
                        break;
                    case "/h":
                    case "/here":
                        channel = ChatMsg.ChatChannel.Here;
                        break;
                    case "/n":
                    case "/near":
                        channel = ChatMsg.ChatChannel.Near;
                        break;
                    case "/b":
                    case "/buddy":
                        channel = ChatMsg.ChatChannel.Buddies;
                        break;
                    case "/i":
                    case "/island":
                        channel = ChatMsg.ChatChannel.Isle;
                        break;
                    case "/admin":
                        if (sender.User.Administrator)
                            channel = ChatMsg.ChatChannel.Admin;
                        else
                            return;
                        break;
                    case "/mod":
                        if (sender.User.Moderator)
                            channel = ChatMsg.ChatChannel.Mod;
                        else
                            return;
                        break;
                    default:
                        channel = ChatMsg.ChatChannel.Dm;
                        nameTo = channelString.Substring(1).Trim(); 
                        break;
                }

                if (message == "") // this is how pinto does it, im serious.
                {
                    channel = ChatMsg.ChatChannel.Dm;
                    nameTo = "";
                }
            }

            message = message.Trim();

            if (channel == ChatMsg.ChatChannel.All && message.Length > 150)
            {
                byte[] tooLong = PacketBuilder.CreateChat(Messages.GlobalChatTooLong, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(tooLong);
                return;
            }

            if (channel == ChatMsg.ChatChannel.Ads && message.Length > 150)
            {
                byte[] tooLong = PacketBuilder.CreateChat(Messages.AdsChatTooLong, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(tooLong);
                return;
            }
            if (ChatMsg.ProcessCommand(sender.User, message))
            {
                Logger.DebugPrint(sender.User.Username + " Attempting to run command '" + message + "' in channel: " + channel.ToString());
                return;
            }

            // Check events
            if (RiddleEvent.Active) 
                if(RiddleEvent.CheckRiddle(message))
                    RiddleEvent.Win(sender.User);
                
           

            // Check if player is muting channel

            if( (sender.User.MuteGlobal && channel == ChatMsg.ChatChannel.All) || (sender.User.MuteAds && channel == ChatMsg.ChatChannel.Ads) || (sender.User.MuteHere && channel == ChatMsg.ChatChannel.Here) && (sender.User.MuteBuddy && channel == ChatMsg.ChatChannel.Buddies) && (sender.User.MuteNear && channel == ChatMsg.ChatChannel.Near) && (sender.User.MuteIsland && channel == ChatMsg.ChatChannel.Isle))
            {
                byte[] cantSendMessage = PacketBuilder.CreateChat(Messages.CantSendInMutedChannel, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(cantSendMessage);
                return;
            }

            if(sender.User.MutePrivateMessage && channel == ChatMsg.ChatChannel.Dm)
            {
                byte[] cantSendDmMessage = PacketBuilder.CreateChat(Messages.CantSendPrivateMessageWhileMuted, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(cantSendDmMessage);
                return;
            }

            Object violationReason = ChatMsg.FilterMessage(message);
            if (violationReason != null)
            {
                sender.User.ChatViolations += 1;
                string chatViolationMessage = Messages.FormatGlobalChatViolationMessage((ChatMsg.Reason)violationReason);
                byte[] chatViolationPacket = PacketBuilder.CreateChat(chatViolationMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatViolationPacket);
                return;
            }

            byte chatSide = ChatMsg.GetSide(channel);
            message = ChatMsg.DoCorrections(message);
            message = ChatMsg.EscapeMessage(message);

            string failedReason = ChatMsg.NonViolationChecks(sender.User, message);
            if (failedReason != null)
            {
                byte[] failedMessage = PacketBuilder.CreateChat(failedReason, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(failedMessage);
                return;
            }

            GameClient[] recipiants = ChatMsg.GetRecipiants(sender.User, channel, nameTo);

            
            if(channel == ChatMsg.ChatChannel.Dm)
            {
                if(recipiants.Length <= 0)
                {
                    byte[] cantFindPlayer = PacketBuilder.CreateChat(Messages.CantFindPlayerToPrivateMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantFindPlayer);

                    return;
                }
                else
                {
                    nameTo = recipiants[0].User.Username;
                }
            }
            
            // Spam filter
            if(ConfigReader.EnableSpamFilter)
            {
                if (channel == ChatMsg.ChatChannel.Ads)
                {
                    if (!sender.User.CanUseAdsChat && !sender.User.Administrator)
                    {
                        byte[] cantSendInAds = PacketBuilder.CreateChat(Messages.AdsOnlyOncePerMinute, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(cantSendInAds);

                        return;
                    }
                    sender.User.CanUseAdsChat = false;
                }
                else if (channel == ChatMsg.ChatChannel.All)
                {
                    if (sender.User.TotalGlobalChatMessages <= 0 && !sender.User.Administrator)
                    {
                        byte[] globalLimited = PacketBuilder.CreateChat(Messages.GlobalChatLimited, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(globalLimited);

                        return;
                    }
                    sender.User.TotalGlobalChatMessages--;
                }
            }


            // Muted user checks
            if(channel == ChatMsg.ChatChannel.Dm) 
            {
                try
                {
                    User userTo = GetUserByNameStartswith(nameTo);
                    if (sender.User.MutePlayer.IsUserMuted(userTo))
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
                    else if (userTo.MutePlayer.IsUserMuted(sender.User))
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
            string formattedMessage = ChatMsg.FormatChatForOthers(sender.User, channel, message);
            string formattedMessageSender = ChatMsg.FormatChatForSender(sender.User, channel, message, nameTo);
            

            byte[] chatPacketOthers = PacketBuilder.CreateChat(formattedMessage, chatSide);
            byte[] chatPacketSender = PacketBuilder.CreateChat(formattedMessageSender, chatSide);
            byte[] playDmSound = PacketBuilder.CreatePlaySound(ChatMsg.PrivateMessageSound);

            // Send to clients ...
            foreach (GameClient recipiant in recipiants)
            {
                recipiant.SendPacket(chatPacketOthers);
                
                if (channel == ChatMsg.ChatChannel.Dm)
                    recipiant.SendPacket(playDmSound);
            }

            // Send to sender
            sender.SendPacket(chatPacketSender);

            // AutoReply
            if (channel == ChatMsg.ChatChannel.Dm)
            {
                foreach (GameClient recipiant in recipiants)
                {
                    if (recipiant.User.AutoReplyText != "")
                    {
                        string formattedMessageAuto = ChatMsg.FormatChatForOthers(recipiant.User, channel, recipiant.User.AutoReplyText, true);
                        string formattedMessageSenderAuto = ChatMsg.FormatChatForSender(recipiant.User, channel, recipiant.User.AutoReplyText, nameTo, true);

                        byte[] chatPacketAutoOthers = PacketBuilder.CreateChat(formattedMessageAuto, chatSide);
                        sender.SendPacket(chatPacketAutoOthers);

                        byte[] chatPacketAutoSender = PacketBuilder.CreateChat(formattedMessageSenderAuto, chatSide);
                        recipiant.SendPacket(chatPacketAutoSender);
                    }
                }

            }

        }
        public static void OnClickPacket(GameClient sender, byte[] packet)
        {

            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Send click packet when not logged in.");
                return;
            }
            if (packet.Length < 5)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid Click Packet");
                return;
            }
            
            string packetStr = Encoding.UTF8.GetString(packet);
            if(packetStr.Contains("|"))
            {
                string packetContents = packetStr.Substring(1, (packetStr.Length - 1) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
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
                    Logger.ErrorPrint(sender.User.Username + " Sent a click packet with non-string xy value.");
                    return;
                }

                Logger.DebugPrint(sender.User.Username + " Clicked on tile: " + Map.GetTileId(x, y, false).ToString() + "(overlay: " + Map.GetTileId(x, y, true).ToString() + ") at " + x.ToString() + "," + y.ToString());


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
                User[] users = GetUsersAt(x, y, false, true);
                if (users.Length > 0) // Player here?
                {
                    string usernameStr = "";

                    for(int i = 0; i < users.Length; i++)
                    {
                        usernameStr += users[i].Username;
                        if (i + 1 < users.Length)
                            usernameStr += ", ";
                    }

                    returnedMsg = Messages.FormatPlayerHereMessage(usernameStr);
                }
                byte[] tileInfoPacket = PacketBuilder.CreateTileClickInfo(returnedMsg);
                // Debug tile id information
                //byte[] tileInfoPacket = PacketBuilder.CreateTileClickInfo("ground: " + (Map.GetTileId(x, y, false)-1).ToString() + ", overlay: " + (Map.GetTileId(x, y, true)-1).ToString());
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
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                return;
            }

            byte action = packet[1];
            switch(action)
            {
                case PacketBuilder.ITEM_PICKUP_ALL:
                    string chatMsg = Messages.GrabAllItemsMessage;
                    DroppedItems.DroppedItem[] droppedItems = DroppedItems.GetItemsAt(sender.User.X, sender.User.Y);

                    foreach (DroppedItems.DroppedItem item in droppedItems)
                    {
                        try
                        {
                            sender.User.Inventory.Add(item.Instance);
                            DroppedItems.RemoveDroppedItem(item);
                        }
                        catch (InventoryException)
                        {
                            chatMsg = Messages.GrabbedAllItemsButInventoryFull;
                        }
                    }

                    UpdateAreaForAll(sender.User.X, sender.User.Y);

                    byte[] chatMessage = PacketBuilder.CreateChat(chatMsg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(chatMessage);

                    break;
                case PacketBuilder.ITEM_PICKUP:
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch(FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                        return;
                    }

                    try
                    {
                        DroppedItems.DroppedItem item = DroppedItems.GetDroppedItemById(randomId);
                        try
                        {
                            sender.User.Inventory.Add(item.Instance);
                        }
                        catch (InventoryException)
                        {
                            byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.GrabbedItemButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(inventoryFullMessage);
                            break;
                        }

                        
                        DroppedItems.RemoveDroppedItem(item);

                        UpdateAreaForAll(sender.User.X, sender.User.Y);

                        chatMessage = PacketBuilder.CreateChat(Messages.GrabbedItemMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatMessage);
                    }
                    catch(KeyNotFoundException)
                    {
                        byte[] pickedUp = PacketBuilder.CreateChat(Messages.DroppedItemCouldntPickup, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(pickedUp);

                        Logger.HackerPrint(sender.User.Username + " Tried to grab a non existing object.");
                        return;
                    }

                    break;
                case PacketBuilder.ITEM_REMOVE:
                    char toRemove = (char)packet[2];
                    switch(toRemove)
                    {
                        case '1':
                            if(sender.User.EquipedCompetitionGear.Head != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Head.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedCompetitionGear.Head = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '2':
                            if (sender.User.EquipedCompetitionGear.Body != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Body.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedCompetitionGear.Body = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '3':
                            if (sender.User.EquipedCompetitionGear.Legs != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Legs.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedCompetitionGear.Legs = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '4':
                            if (sender.User.EquipedCompetitionGear.Feet != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Feet.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedCompetitionGear.Feet = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '5':
                            if (sender.User.EquipedJewelry.Slot1 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedJewelry.Slot1.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedJewelry.Slot1 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '6':
                            if (sender.User.EquipedJewelry.Slot2 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedJewelry.Slot2.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedJewelry.Slot2 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '7':
                            if (sender.User.EquipedJewelry.Slot3 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedJewelry.Slot3.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedJewelry.Slot3 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '8':
                            if (sender.User.EquipedJewelry.Slot4 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.User.EquipedJewelry.Slot4.Id);
                                sender.User.Inventory.AddIgnoringFull(itemInstance);
                                sender.User.EquipedJewelry.Slot4 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.User.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        default:
                            Logger.InfoPrint(sender.User.Username + "Unimplemented  \"remove worn item\" ItemInteraction packet: " + BitConverter.ToString(packet).Replace("-", " "));
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
                    string itemidStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int itemId = 0;

                    try
                    {
                        itemId = Int32.Parse(itemidStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. (THROW) " + BitConverter.ToString(packet));
                        return;
                    }
                    if (sender.User.Inventory.HasItemId(itemId))
                    {
                        if (!Item.IsThrowable(itemId))
                        {
                            Logger.HackerPrint(sender.User.Username + " Tried to throw an item that isnt throwable.");
                            return;
                        }

                        ItemInstance curItem = sender.User.Inventory.GetItemByItemId(itemId).ItemInstances[0];
                        User[] userAt = GetReallyNearbyUsers(sender.User.X, sender.User.Y);

                        while (true)
                        {
                            int userIndx = RandomNumberGenerator.Next(0, userAt.Length);

                            if (userAt.Length > 1)
                                if (userAt[userIndx].Id == sender.User.Id)
                                    continue;

                            Item.ThrowableItem throwableItem = Item.GetThrowableItem(curItem.ItemId);

                            if (userAt[userIndx].Id == sender.User.Id)
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
                                ModsRevengeEvent.Payout(sender.User, userAt[userIndx]);
                            }

                            byte[] thrownForYou = PacketBuilder.CreateChat(Messages.FormatThrownItemMessage(throwableItem.ThrowMessage, userAt[userIndx].Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            byte[] thrownForOthers = PacketBuilder.CreateChat(Messages.FormatThrownItemMessage(throwableItem.HitMessage, sender.User.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

                            sender.SendPacket(thrownForYou);
                            userAt[userIndx].Client.SendPacket(thrownForOthers);
                            
                            break;
                        }

                        sender.User.Inventory.Remove(curItem);
                        UpdateInventory(sender);
                        
                    }
                    break;
                case PacketBuilder.ITEM_WRAP:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                        return;
                    }
                    if (sender.User.Inventory.HasItem(randomId))
                    {
                        ItemInstance curItem = sender.User.Inventory.GetItemByRandomid(randomId).ItemInstances[0];
                        ItemInstance wrappedItem = new ItemInstance(Item.Present, -1, curItem.ItemId);

                        try
                        {
                            sender.User.Inventory.Add(wrappedItem);
                            sender.User.Inventory.Remove(curItem);
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
                    randomIdStr = packetStr.Substring(2, (packet.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                        return;
                    }
                    if (sender.User.Inventory.HasItem(randomId))
                    {
                        InventoryItem item = sender.User.Inventory.GetItemByRandomid(randomId);
                        int newItem = item.ItemInstances[0].Data;
                        if(newItem == 0)
                        {
                            sender.User.Inventory.Remove(item.ItemInstances[0]);
                            
                            byte[] itemOpenFailedNothingInside = PacketBuilder.CreateChat(Messages.SantaCantOpenNothingInside, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(itemOpenFailedNothingInside);

                            UpdateInventory(sender);

                            break;
                        }

                        try
                        {
                            sender.User.Inventory.Add(new ItemInstance(newItem));
                            sender.User.Inventory.Remove(item.ItemInstances[0]);
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
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);

                    if(randomIdStr == "") // f12 ranch shortcut
                    {
                        if (sender.User.Inventory.HasItemId(Item.DorothyShoes))
                        {
                            InventoryItem itm = sender.User.Inventory.GetItemByItemId(Item.DorothyShoes);
                            Item.UseItem(sender.User, itm.ItemInstances[0]);
                            return;
                        }
                        else
                        {
                            byte[] noShoesMessage = PacketBuilder.CreateChat(Messages.RanchNoDorothyShoesMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(noShoesMessage);
                            return;
                        }
                    }

                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                        return;
                    }
                    if (sender.User.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.User.Inventory.GetItemByRandomid(randomId);
                        Item.UseItem(sender.User, itm.ItemInstances[0]);
                    }
                    break;
                case PacketBuilder.ITEM_WEAR:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                        return;
                    }
                    if (sender.User.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.User.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                     
                        Item.ItemInformation itemInf = instance.GetItemInfo();
                        if(itemInf.Type == "CLOTHES")
                        {
                            switch (itemInf.GetMiscFlag(0))
                            {
                                case CompetitionGear.MISC_FLAG_HEAD:
                                    if (sender.User.EquipedCompetitionGear.Head == null)
                                        sender.User.EquipedCompetitionGear.Head = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Head.Id);
                                        sender.User.Inventory.AddIgnoringFull(itemInstance);
                                        sender.User.EquipedCompetitionGear.Head = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_BODY:
                                    if (sender.User.EquipedCompetitionGear.Body == null)
                                        sender.User.EquipedCompetitionGear.Body = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Body.Id);
                                        sender.User.Inventory.AddIgnoringFull(itemInstance);
                                        sender.User.EquipedCompetitionGear.Body = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_LEGS:
                                    if (sender.User.EquipedCompetitionGear.Legs == null)
                                        sender.User.EquipedCompetitionGear.Legs = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Legs.Id);
                                        sender.User.Inventory.AddIgnoringFull(itemInstance);
                                        sender.User.EquipedCompetitionGear.Legs = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_FEET:
                                    if (sender.User.EquipedCompetitionGear.Feet == null)
                                        sender.User.EquipedCompetitionGear.Feet = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.User.EquipedCompetitionGear.Feet.Id);
                                        sender.User.Inventory.AddIgnoringFull(itemInstance);
                                        sender.User.EquipedCompetitionGear.Feet = itemInf;
                                    }
                                    break;
                                default: 
                                    Logger.ErrorPrint(itemInf.Name + " Has unknown misc flags.");
                                    return;
                            }
                            sender.User.Inventory.Remove(instance);
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatEquipCompetitionGearMessage(itemInf.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                        }
                        else if(itemInf.Type == "JEWELRY")
                        {
                            bool addedJewelry = false;
                            if (sender.User.EquipedJewelry.Slot1 == null)
                            {
                                sender.User.EquipedJewelry.Slot1 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.User.EquipedJewelry.Slot2 == null)
                            {
                                sender.User.EquipedJewelry.Slot2 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.User.EquipedJewelry.Slot3 == null)
                            {
                                sender.User.EquipedJewelry.Slot3 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.User.EquipedJewelry.Slot4 == null)
                            {
                                sender.User.EquipedJewelry.Slot4 = itemInf;
                                addedJewelry = true;
                            }

                            if(addedJewelry)
                            {
                                sender.User.Inventory.Remove(instance);
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
                        Logger.HackerPrint(sender.User.Username + " Tried to wear an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_DRINK:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string idStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    if(idStr == "NaN") // Fountain
                    {
                        string msg = Messages.FountainDrankYourFull;
                        bool looseMoney = RandomNumberGenerator.Next(0, 20) == 18;
                        if(looseMoney)
                        {
                            int looseAmount = RandomNumberGenerator.Next(0, 100);
                            if (looseAmount > sender.User.Money)
                                looseAmount = sender.User.Money;
                            sender.User.TakeMoney(looseAmount);
                            msg = Messages.FormatDroppedMoneyMessage(looseAmount);
                        }

                        sender.User.Thirst = 1000;
                        byte[] drankFromFountainMessage = PacketBuilder.CreateChat(msg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(drankFromFountainMessage);
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.User.Username + "Sent unknown ITEM_DRINK command id: " + idStr);
                    }
                    break;
                case PacketBuilder.ITEM_CONSUME:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                        return;
                    }

                    if (sender.User.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.User.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        sender.User.Inventory.Remove(instance);
                        Item.ItemInformation itmInfo = instance.GetItemInfo();
                        bool toMuch = Item.ConsumeItem(sender.User, itmInfo);

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
                        Logger.HackerPrint(sender.User.Username + " Tried to consume an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_DROP:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. (drop)"+BitConverter.ToString(packet));
                        return;
                    }

                    if(sender.User.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.User.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        if(DroppedItems.GetItemsAt(sender.User.X, sender.User.Y).Length > 25)
                        {
                            byte[] tileIsFullPacket = PacketBuilder.CreateChat(Messages.DroppedItemTileIsFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(tileIsFullPacket);
                            break;
                        }
                        DroppedItems.AddItem(instance, sender.User.X, sender.User.Y);
                        sender.User.Inventory.Remove(instance);
                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.DroppedAnItemMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);
                        UpdateInventory(sender);

                        UpdateAreaForAll(sender.User.X, sender.User.Y, false, sender.User);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to drop an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_SHOVEL:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.User.Username + " Used ITEM_SHOVEL with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.User, Quest.Shovel, sender.User.X, sender.User.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.ShovelNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_RAKE:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.User.Username + " Used ITEM_RAKE with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.User, Quest.Rake, sender.User.X, sender.User.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.RakeNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_MAGNIFYING:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.User.Username + " Used ITEM_MAGNIFYING with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.User, Quest.MagnifyingGlass, sender.User.X, sender.User.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.MagnifyNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_BINOCULARS:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.User.Username + " Used ITEM_BINOCULARS with 3rd byte not 0x14.");
                    if(!Quest.UseTool(sender.User, Quest.Binoculars, sender.User.X, sender.User.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.BinocularsNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_CRAFT:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string craftIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int craftId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        craftId = Int32.Parse(craftIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " tried to craft using craft id NaN.");
                        return;
                    }
                    if(Workshop.CraftIdExists(craftId))
                    {
                        Workshop.CraftableItem itm = Workshop.GetCraftId(craftId);
                        if(itm.MoneyCost <= sender.User.Money) // Check money
                        {
                            foreach(Workshop.RequiredItem reqItem in itm.RequiredItems)
                            {
                                if (sender.User.Inventory.HasItemId(reqItem.RequiredItemId))
                                {
                                    if (sender.User.Inventory.GetItemByItemId(reqItem.RequiredItemId).ItemInstances.Length < reqItem.RequiredItemCount)
                                        goto failMissingItem;
                                }
                                else
                                    goto failMissingItem;
                            }

                            // Finally create the items
                            try
                            {
                                sender.User.Inventory.Add(new ItemInstance(itm.GiveItemId));
                            }
                            catch(InventoryException)
                            {
                                byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.WorkshopNoRoomInInventory, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(inventoryFullMessage);
                                break;
                            }
                            sender.User.TakeMoney(itm.MoneyCost);

                            // Remove the required items..
                            foreach(Workshop.RequiredItem reqItem in itm.RequiredItems) 
                                for(int i = 0; i < reqItem.RequiredItemCount; i++)
                                    sender.User.Inventory.Remove(sender.User.Inventory.GetItemByItemId(reqItem.RequiredItemId).ItemInstances[0]);

                            sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count++;

                            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count >= 100)
                                sender.User.Awards.AddAward(Award.GetAwardById(22)); // Craftiness
                            if (sender.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count >= 1000)
                                sender.User.Awards.AddAward(Award.GetAwardById(23)); // Workmanship

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
                    randomIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    randomId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object sell packet.");
                        return;
                    }

                    if (!sender.User.Inventory.HasItem(randomId))
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to sell a item that they doesnt have in there inventory");
                        return;
                    }

                    InventoryItem invItem = sender.User.Inventory.GetItemByRandomid(randomId);
                    itemId = invItem.ItemId;
                    goto doSell;
                case PacketBuilder.ITEM_SELL_ALL:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string itemIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object sell packet.");
                        return;
                    }

                    if (!sender.User.Inventory.HasItemId(itemId))
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to sell a item that they doesnt have in there inventory");
                        return;
                    }
                    invItem = sender.User.Inventory.GetItemByItemId(itemId);

                    totalSold = invItem.ItemInstances.Length;
                    message = 2;
                    goto doSell;
                doSell:;

                    Item.ItemInformation itemInfo = Item.GetItemById(itemId);
                    Shop shop = sender.User.LastShoppedAt;
                    if (shop != null)
                    {
                        UInt64 sellPrice = Convert.ToUInt64(shop.CalculateSellCost(itemInfo) * Convert.ToUInt64(totalSold));
                        if (shop.CanSell(itemInfo))
                        {
                            // Check if goes over 2.1b
                            if (Convert.ToUInt64(sender.User.Money) + sellPrice > 2100000000)
                            {
                                byte[] cantSellMoneyCapCheck = PacketBuilder.CreateChat(Messages.CannotSellYoudGetTooMuchMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cantSellMoneyCapCheck);
                                break; 
                            }

                            // Remove items
                            for (int i = 0; i < totalSold; i++)
                            {
                                ItemInstance itemInstance = invItem.ItemInstances[0];
                                sender.User.Inventory.Remove(itemInstance);
                                shop.Inventory.Add(itemInstance);
                            }

                            if (sellPrice < 2147483647) // Sanity Check (yes i checked it earlier)
                                sender.User.AddMoney(Convert.ToInt32(sellPrice));

                            UpdateAreaForAll(sender.User.X, sender.User.Y, true);
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

                                byte[] soldItemMessage = PacketBuilder.CreateChat(Messages.FormatSellAllMessage(name, sellPrice, totalSold), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(soldItemMessage);
                            }

                        }
                        else
                        {
                            Logger.HackerPrint(sender.User.Username + " Tried to sell a item that was not avalible to be sold.");
                        }
                    }
                    break;

                case PacketBuilder.ITEM_BUY_AND_CONSUME:
                    packetStr = Encoding.UTF8.GetString(packet);
                    itemIdStr = packetStr.Substring(2, (packetStr.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object buy and consume packet.");
                        return;
                    }
                    if (!Item.ItemIdExist(itemId))
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to buy an itemid that doesnt even exist.");
                        break;
                    }

                    Inn lastInn = sender.User.LastVisitedInn;
                    if (lastInn != null)
                    {
                        try
                        {
                            itemInfo = lastInn.GetStockedItem(itemId);
                            int price = lastInn.CalculateBuyCost(itemInfo);
                            if(sender.User.Money >= price)
                            {
                                sender.User.TakeMoney(price);
                                bool toMuch = Item.ConsumeItem(sender.User, itemInfo);

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
                            Logger.HackerPrint(sender.User.Username + " Tried to buy and consume an item not stocked by the inn there standing on.");
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to buy and consume item while not in a inn.");
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
                    itemIdStr = packetStr.Substring(2, (packet.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object buy packet.");
                        return;
                    }

                    if(!Item.ItemIdExist(itemId))
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to buy an itemid that doesnt even exist.");
                        break;
                    }

                    itemInfo = Item.GetItemById(itemId);
                    shop = sender.User.LastShoppedAt;
                    if (shop != null)
                    {
                        UInt64 buyCost = Convert.ToUInt64(shop.CalculateBuyCost(itemInfo) * Convert.ToUInt64(count));
                        if (sender.User.Bids.Length > 0)
                        {
                            byte[] cantBuyWhileAuctioning = PacketBuilder.CreateChat(Messages.AuctionNoOtherTransactionAllowed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantBuyWhileAuctioning);
                            return;
                        }

                        if (Convert.ToUInt64(sender.User.Money) < buyCost)
                        {
                            byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.CantAfford1, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAffordMessage);
                            return;
                        }
                        if (shop.Inventory.HasItemId(itemId))
                        {
                            if (shop.Inventory.GetItemByItemId(itemId).ItemInstances.Length < count)
                            {
                                Logger.HackerPrint(sender.User.Username + " Tried to buy more of an item than is in stock.");
                                break;
                            }


                            // Check we wont overflow the inventory
                            if (sender.User.Inventory.HasItemId(itemId)) 
                            {
                                InventoryItem items = sender.User.Inventory.GetItemByItemId(itemId);
                                if (items.ItemInstances.Length + count > Item.MAX_STACK)
                                {
                                    goto showError;
                                }

                            }
                            else if(sender.User.Inventory.Count + 1 > sender.User.MaxItems)
                            {
                                goto showError;
                            }

                            for (int i = 0; i < count; i++)
                            {
                                ItemInstance itemInstance = shop.Inventory.GetItemByItemId(itemId).ItemInstances[0];
                                try
                                {
                                    sender.User.Inventory.Add(itemInstance);
                                }
                                catch (InventoryException)
                                {
                                    Logger.ErrorPrint("Failed to add: " + itemInfo.Name + " to " + sender.User.Username + " inventory.");
                                    break;
                                }
                                shop.Inventory.Remove(itemInstance);
                            }

                            if(buyCost < 2147483647) // Sanity Check (yes i checked it earlier)
                                sender.User.TakeMoney(Convert.ToInt32(buyCost));


                            // Send chat message to client.
                            UpdateAreaForAll(sender.User.X, sender.User.Y, true);
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
                            Logger.HackerPrint(sender.User.Username + " Tried to buy a item that was not for sale.");
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to buy an item while not in a store.");
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
                    randomIdStr = packetStr.Substring(2, (packet.Length - 2) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    randomId = 0;
                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                        return;
                    }

                    if (!sender.User.Inventory.HasItem(randomId))
                    {
                        Logger.HackerPrint(sender.User.Username + " Tried to rip someone elses mail. " + randomId.ToString());
                        return;
                    }

                    InventoryItem ripItems = sender.User.Inventory.GetItemByRandomid(randomId);
                    foreach (ItemInstance item in ripItems.ItemInstances)
                    {
                        if (item.RandomId == randomId)
                        {
                            if (item.Data == 0)
                                continue;
                            sender.User.MailBox.RipUpMessage(sender.User.MailBox.GetMessageByRandomId(item.Data));
                            break;
                        }
                    }
                    break;
                case PacketBuilder.ITEM_VIEW:
                    byte method = packet[2];
                    if (method == PacketBuilder.ITEM_LOOK)
                    {
                        packetStr = Encoding.UTF8.GetString(packet);
                        itemIdStr = packetStr.Substring(3, (packetStr.Length - 3) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        itemId = 0;
                        try
                        {
                            itemId = Int32.Parse(itemIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                            return;
                        }

                        if (itemId == Item.MailMessage)
                        {
                            if (!sender.User.Inventory.HasItemId(Item.MailMessage))
                            {
                                Logger.ErrorPrint(sender.User.Username + " Tried to view a mail message when they didnt have one.");
                                return;
                            }

                            sender.User.MajorPriority = true;
                            byte[] mailList = PacketBuilder.CreateMeta(Meta.BuildMailList(sender.User, sender.User.Inventory.GetItemByItemId(Item.MailMessage)));
                            sender.SendPacket(mailList);
                            break;
                        }
                    }
                    else if(method == PacketBuilder.ITEM_READ)
                    {
                        packetStr = Encoding.UTF8.GetString(packet);
                        randomIdStr = packetStr.Substring(3, (packetStr.Length - 3) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                        randomId = 0;
                        try
                        {
                            randomId = Int32.Parse(randomIdStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. " + BitConverter.ToString(packet));
                            return;
                        }

                        if (!sender.User.Inventory.HasItem(randomId))
                        {
                            Logger.HackerPrint(sender.User.Username + " Tried to view someone elses mail. " + randomId.ToString());
                            return;
                        }

                        InventoryItem items = sender.User.Inventory.GetItemByRandomid(randomId);
                        foreach (ItemInstance item in items.ItemInstances)
                        {
                            if (item.RandomId == randomId)
                            {
                                if (item.Data == 0)
                                    continue;

                                sender.User.MajorPriority = true;
                                byte[] readMail = PacketBuilder.CreateMeta(Meta.BuildMailLetter(sender.User.MailBox.GetMessageByRandomId(item.Data), randomId));
                                sender.SendPacket(readMail);
                                break;
                            }
                        }
                        break;

                    }


                    Logger.ErrorPrint(sender.User.Username + " Unknown Method- " + method.ToString("X") + "  " + BitConverter.ToString(packet).Replace("-", " "));
                    break;
                case PacketBuilder.PACKET_INFORMATION:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string valueStr = packetStr.Substring(3, (packetStr.Length - 3) - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
                    int value = 0;
                    try
                    {
                        value = Int32.Parse(valueStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.User.Username + " Sent an invalid object interaction packet. "+BitConverter.ToString(packet));
                        return;
                    }
                    if (packet[2] == PacketBuilder.ITEM_INFORMATON)
                    {
                        itemId = -1;
                        if (sender.User.Inventory.HasItem(value))
                            itemId = sender.User.Inventory.GetItemByRandomid(value).ItemId;
                        else if (DroppedItems.IsDroppedItemExist(value))
                            itemId = DroppedItems.GetDroppedItemById(value).Instance.ItemId;
                        if (itemId == -1)
                        {
                            Logger.HackerPrint(sender.User.Username + " asked for details of non existiant item.");
                            return;
                        }
                        sender.User.MajorPriority = true;
                        Item.ItemInformation info = Item.GetItemById(itemId);
                        string infoMessage = Meta.BuildItemInfo(info);
                        byte[] metaPacket = PacketBuilder.CreateMeta(infoMessage);
                        sender.SendPacket(metaPacket);
                    }
                    if (packet[2] == PacketBuilder.ITEM_INFORMATON_ID)
                    {
                        sender.User.MajorPriority = true;
                        if (!Item.ItemIdExist(value))
                        {
                            Logger.HackerPrint(sender.User.Username + " asked for details of non existiant item.");
                            return;
                        }

                        Item.ItemInformation info = Item.GetItemById(value);
                        string infoMessage = Meta.BuildItemInfo(info);
                        byte[] metaPacket = PacketBuilder.CreateMeta(infoMessage);
                        sender.SendPacket(metaPacket);
                    }
                    else if(packet[2] == PacketBuilder.NPC_INFORMATION)
                    {
                        if(Npc.NpcExists(value))
                        {
                            sender.User.MajorPriority = true;
                            Npc.NpcEntry npc = Npc.GetNpcById(value);
                            string infoMessage = Meta.BuildNpcInfo(npc);
                            byte[] metaPacket = PacketBuilder.CreateMeta(infoMessage);
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.HackerPrint(sender.User.Username + " asked for details of non existiant npc.");
                            return;
                        }
                    }

                    break;
                default:
                    Logger.WarnPrint(sender.User.Username + " Sent an unknown Item Interaction Packet type: " + action.ToString() + ", Packet Dump: " + BitConverter.ToString(packet).Replace('-', ' '));
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

            if (packet.Length < 1)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid inventory request packet.");
                return;
            }

            UpdateInventory(sender);
        }
        public static void OnUserLogin(GameClient sender, byte[] packet)
        {
            Logger.DebugPrint("Login request received from: " + sender.RemoteIp);

            string loginRequestString = Encoding.UTF8.GetString(packet).Substring(1);

            if (!loginRequestString.Contains('|') || packet.Length < 2)
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
                    int userId = Database.GetUserId(username);

                    if(Database.IsUserBanned(userId))
                    {
                        Logger.DebugPrint(sender.RemoteIp + " Tried to login to : " + username + " but, the account was banned.");
                        byte[] userBannedPacket = PacketBuilder.CreateLogin(false, Messages.LoginFailedReasonBanned);
                        sender.SendPacket(userBannedPacket);
                        return;
                    }

                    if(Database.IsIpBanned(sender.RemoteIp))
                    {
                        Logger.DebugPrint(sender.RemoteIp + " Tried to login to : " + username + " but, the IP was banned.");
                        byte[] ipBannedPacket = PacketBuilder.CreateLogin(false, Messages.FormatIpBannedMessage(sender.RemoteIp));
                        sender.SendPacket(ipBannedPacket);
                        return;
                    }

                    sender.Login(userId);
                    sender.User.Password = password;

                    byte[] responsePacket = PacketBuilder.CreateLogin(true);
                    sender.SendPacket(responsePacket);

                    Logger.DebugPrint(sender.RemoteIp + " Logged into : " + sender.User.Username + " (ADMIN: " + sender.User.Administrator + " MOD: " + sender.User.Moderator + ")");

                }
                else
                {
                    Logger.WarnPrint(sender.RemoteIp + " Attempted to login to: " + username + " with incorrect password ");
                    byte[] ResponsePacket = PacketBuilder.CreateLogin(false);
                    sender.SendPacket(ResponsePacket);
                }
            }

        }

        public static void OnDisconnect(GameClient sender)
        {
            if (sender.LoggedIn)
            {
                Database.SetPlayerLastLogin(Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()), sender.User.Id); // Set last login date
                Database.RemoveOnlineUser(sender.User.Id);

                // Leave multirooms
                Multiroom.LeaveAllMultirooms(sender.User);
                TwoPlayer.TwoPlayerRemove(sender.User);

                // Remove Trade Reference
                sender.User.TradingWith = null;
                sender.User.PendingTradeTo = 0;

                // Leave open water balloon game
                if (WaterBalloonEvent != null)
                    if(WaterBalloonEvent.Active)
                        WaterBalloonEvent.LeaveEvent(sender.User);

                // Leave open quiz.
                if (QuizEvent != null)
                    QuizEvent.LeaveEvent(sender.User);

                ModsRevengeEvent.LeaveEvent(sender.User);

                // Delete Arena Entries
                if (Arena.UserHasEnteredHorseInAnyArena(sender.User))
                {
                    Arena arena = Arena.GetArenaUserEnteredIn(sender.User);
                    arena.DeleteEntry(sender.User);
                }


                // Send disconnect message
                byte[] logoutMessageBytes = PacketBuilder.CreateChat(Messages.FormatLogoutMessage(sender.User.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
                foreach (GameClient client in GameClient.ConnectedClients)
                    if (client.LoggedIn)
                        if (!client.User.MuteLogins && !client.User.MuteAll)
                            if (client.User.Id != sender.User.Id)
                                client.SendPacket(logoutMessageBytes);

                // Tell clients of diconnect (remove from chat)
                byte[] playerRemovePacket = PacketBuilder.CreatePlayerLeave(sender.User.Username);
                foreach (GameClient client in GameClient.ConnectedClients)
                    if (client.LoggedIn)
                        if (client.User.Id != sender.User.Id)
                            client.SendPacket(playerRemovePacket);
            }

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
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.User.Stealth)
                        continue;
                    if (!includeMuted && client.User.MuteIsland)
                        continue;
                    if (World.InTown(client.User.X, client.User.Y))
                        if (World.GetIsle(client.User.X, client.User.Y).Name == town.Name)
                            usersInTown.Add(client.User);
                }

            return usersInTown.ToArray();
        }
        public static User[] GetUsersInIsle(World.Isle isle, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersInIsle = new List<User>();
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.User.Stealth)
                        continue;
                    if (!includeMuted && client.User.MuteIsland)
                        continue;
                    if (World.InIsle(client.User.X, client.User.Y))
                        if (World.GetIsle(client.User.X, client.User.Y).Name == isle.Name)
                            usersInIsle.Add(client.User);
                }

            return usersInIsle.ToArray();
        }

        public static User[] GetUsersOnSpecialTileCode(string code)
        {
            List<User> userList = new List<User>();

            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {

                    if (World.InSpecialTile(client.User.X, client.User.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(client.User.X, client.User.Y);

                        if (tile.Code == code)
                        {
                            userList.Add(client.User);
                        }
                    }
                }
            }
            return userList.ToArray();
        }
        public static User[] GetUsersAt(int x, int y, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersHere = new List<User>();
            foreach(GameClient client in GameClient.ConnectedClients)
            {
                if(client.LoggedIn)
                {
                    if (!includeStealth && client.User.Stealth)
                        continue;
                    if (!includeMuted && client.User.MuteNear)
                        continue;
                    if (client.User.X == x && client.User.Y == y)
                        usersHere.Add(client.User);
                }
            }
            return usersHere.ToArray();
        }
        public static User GetUserByNameStartswith(string username)
        {
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (client.User.Username.ToLower().StartsWith(username.ToLower()))
                        return client.User;
                }
            }
            throw new KeyNotFoundException("User was not found.");
        }
        public static User GetUserByName(string username)
        {
            foreach(GameClient client in GameClient.ConnectedClients)
            {
                if(client.LoggedIn)
                {
                    if (client.User.Username.ToLower() == username.ToLower())
                        return client.User;
                }
            }
            throw new KeyNotFoundException("User was not found.");
        }

        public static User GetUserById(int id)
        {
            foreach(GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.User.Id == id)
                        return client.User;
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

            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (startX <= client.User.X && endX >= client.User.X && startY <= client.User.Y && endY >= client.User.Y)
                        usersNearby.Add(client.User);
                }
            }

            return usersNearby.ToArray();
        }


        public static bool IsOnScreen(int screenX, int screenY, int playerX, int playerY)
        {
            int startX = screenX - 9;
            int endX = screenX + 9;
            int startY = screenY - 8;
            int endY = screenY + 9;
            if (startX <= playerX && endX >= playerX && startY <= playerY && endY >= playerY)
                return true;
            else
                return false;
        }
        public static User[] GetOnScreenUsers(int x, int y, bool includeStealth = false, bool includeMuted = false)
        {

            List<User> usersOnScreen = new List<User>();

            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.User.Stealth)
                        continue;
                    if (!includeMuted && client.User.MuteNear)
                        continue;
                    if (IsOnScreen(x,y,client.User.X, client.User.Y))
                        usersOnScreen.Add(client.User);
                }

            return usersOnScreen.ToArray();
        }

        public static User[] GetNearbyUsers(int x, int y, bool includeStealth=false, bool includeMuted=false)
        {
            int startX = x - 15;
            int endX = x + 15;
            int startY = y - 19;
            int endY = y + 19;
            List<User> usersNearby = new List<User>();

            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.User.Stealth)
                        continue;
                    if (!includeMuted && client.User.MuteNear)
                        continue;
                    if (startX <= client.User.X && endX >= client.User.X && startY <= client.User.Y && endY >= client.User.Y)
                        usersNearby.Add(client.User);
                }
            }

            return usersNearby.ToArray();
        }
        public static int GetNumberOfPlayers(bool includeStealth=false)
        {
            int count = 0;
            foreach(GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.User.Stealth)
                        continue;
                    if (!client.User.Stealth)
                        count++;
                }
            
            return count;
        }

        public static Point[] GetAllBuddyLocations(User caller)
        {
            List<Point> allLocations = new List<Point>();

            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {

                    if (!caller.Friends.List.Contains(client.User.Id))
                        continue;
                    

                    if (!client.User.Stealth)
                        allLocations.Add(new Point(client.User.X, client.User.Y));

                }
            }

            return allLocations.ToArray();
        }

        public static Point[] GetAllPlayerLocations(User caller)
        {
            List<Point> allLocations = new List<Point>();
            
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                {

                    if (client.User.Id == caller.Id) 
                        continue;
                    
                    if (!client.User.Stealth)
                        allLocations.Add(new Point(client.User.X, client.User.Y));
                    
                }

                        
            }
            return allLocations.ToArray();
        }
        public static int GetNumberOfPlayersListeningToAdsChat()
        {
            int count = 0;
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    if (!client.User.MuteAds)
                        count++;
            }
            return count;
        }

        public static void CheckMail(User user)
        {
            if (user.MailBox.UnreadMailCount > 0)
            {

                byte[] RipOffAOLSound = PacketBuilder.CreatePlaySound(Messages.MailSe);
                user.Client.SendPacket(RipOffAOLSound);

                byte[] mailReceivedText = PacketBuilder.CreateChat(Messages.MailReceivedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.Client.SendPacket(mailReceivedText);

                user.MailBox.ReadAllMail();
            }
        }
        public static int GetNumberOfModsOnline()
        {
            int count = 0;
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    if(client.User.Moderator)
                        count++;
            }
            return count;
        }

        public static int GetNumberOfBuddiesOnline(User user)
        {
            int total = 0;
            foreach(int bud in user.Friends.List.ToArray())
            {
                if (IsUserOnline(bud))
                {
                    total++;
                }
            }
            return total;
        }

        public static int GetNumberOfAdminsOnline()
        {
            int count = 0;
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.User.Administrator)
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
            foreach (User nearbyUser in GameServer.GetNearbyUsers(client.User.X, client.User.Y, false, false))
                if (nearbyUser.Id != client.User.Id)
                    if(!nearbyUser.MajorPriority)
                        if(!nearbyUser.MinorPriority)
                            UpdateArea(nearbyUser.Client);

            UpdateUserFacingAndLocation(client.User);
        }

        public static void UpdateDrawingForAll(string id, GameClient sender, string drawing, bool includingSender=false)
        {
            World.SpecialTile[] tiles = World.GetSpecialTilesByCode("MULTIROOM-" + id);
            foreach (World.SpecialTile tile in tiles)
            {
                UpdateAreaForAll(tile.X, tile.Y, true, null);
                User[] usersHere = GameServer.GetUsersAt(tile.X, tile.Y, true, true);
                foreach (User user in usersHere)
                {
                    if (!includingSender)
                        if (user.Id == sender.User.Id)
                            continue;

                    byte[] patchDrawing = PacketBuilder.CreateDrawingUpdate(drawing);
                    user.Client.SendPacket(patchDrawing);
                }
            }
        }
        public static void UpdateHorseMenu(GameClient forClient, HorseInstance horseInst)
        {

            forClient.User.MajorPriority = true;

            int TileID = Map.GetTileId(forClient.User.X, forClient.User.Y, false);
            string type = Map.TerrainTiles[TileID - 1].Type;

            if (horseInst.Owner == forClient.User.Id)
                forClient.User.LastViewedHorse = horseInst;
            else
                forClient.User.LastViewedHorseOther = horseInst;

            byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildHorseInformation(horseInst, forClient.User));
            forClient.SendPacket(metaPacket);

            string loadSwf = HorseInfo.BreedViewerSwf(horseInst, type);
            byte[] swfPacket = PacketBuilder.CreateSwfModule(loadSwf, PacketBuilder.PACKET_SWF_MODULE_FORCE);
            forClient.SendPacket(swfPacket);
        }
        public static void UpdateInventory(GameClient forClient)
        {
            if (!forClient.LoggedIn)
                return;
            forClient.User.MajorPriority = true;
            byte[] metaPacket = PacketBuilder.CreateMeta(Meta.BuildInventoryInfo(forClient.User.Inventory));
            forClient.SendPacket(metaPacket);
        }

        public static void UpdateWeather(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update weather information when not logged in.");
                return;
            }

            string lastWeather = forClient.User.LastSeenWeather;
            string weather = forClient.User.GetWeatherSeen();
            if (lastWeather != weather)
            {
                byte[] WeatherUpdate = PacketBuilder.CreateWeatherUpdate(weather);
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

            byte[] WorldData = PacketBuilder.CreateTimeAndWeatherUpdate(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, forClient.User.GetWeatherSeen());
            forClient.SendPacket(WorldData);
        }
        public static void UpdatePlayer(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update player information when not logged in.");
                return;
            }
            byte[] PlayerData = PacketBuilder.CreateMoneyPlayerCountAndMail(forClient.User.Money, GameServer.GetNumberOfPlayers(), forClient.User.MailBox.UnreadMailCount);
            forClient.SendPacket(PlayerData);
        }

        public static void UpdateUserFacingAndLocation(User user)
        {
            byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(user.X, user.Y, user.Facing, user.CharacterId, user.Username);

            foreach (User onScreenUser in GetOnScreenUsers(user.X, user.Y, true, true))
                if (onScreenUser.Id != user.Id)
                    onScreenUser.Client.SendPacket(playerInfoBytes);
        }
        public static void UpdateAreaForAll(int x, int y, bool ignoreMetaPrio=false, User exceptMe=null)
        {
            foreach(GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.User.X == x && client.User.Y == y)
                        if(!client.User.MinorPriority || ignoreMetaPrio)
                            if(!client.User.MajorPriority)
                                if(client.User != exceptMe)
                                    UpdateArea(client);
            }
        }
        
        public static void UpdateArea(GameClient forClient)
        {
            if(forClient == null)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update tile information when not connected.");
                return;
            }
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update tile information when not logged in.");
                return;
            }

            if(forClient.User.TradingWith != null)
            {
                if (!forClient.User.TradingWith.OtherTrade.Trader.Client.LoggedIn)
                {
                    forClient.User.TradingWith.InteruptTrade();
                    return;
                }

                if (forClient.User.TradingWith.OtherTrade.Trader.TradingWith == null)
                {
                    forClient.User.TradingWith.InteruptTrade();
                    return;
                }

                forClient.User.MajorPriority = true;
                forClient.User.TradeMenuPriority = false;
                byte[] tradeMeta = PacketBuilder.CreateMeta(Meta.BuildTrade(forClient.User.TradingWith));
                forClient.SendPacket(tradeMeta);
                return;
            }

            forClient.User.MajorPriority = false;
            forClient.User.MinorPriority = false;

            string locationStr;

            int tileX = forClient.User.X;
            int tileY = forClient.User.Y;
            if (!World.InSpecialTile(tileX, tileY))
            {
                if (forClient.User.InRealTimeQuiz)
                    return;
                locationStr = Meta.BuildMetaInfo(forClient.User, tileX, tileY);
            }
            else
            {
                World.SpecialTile specialTile = World.GetSpecialTile(tileX, tileY);
                if (specialTile.AutoplaySwf != null && specialTile.AutoplaySwf != "")
                {
                    byte[] swfModulePacket = PacketBuilder.CreateSwfModule(specialTile.AutoplaySwf,PacketBuilder.PACKET_SWF_MODULE_GENTLE);
                    forClient.SendPacket(swfModulePacket);
                }

                if (forClient.User.InRealTimeQuiz && QuizEvent != null)
                {
                    QuizEvent.JoinEvent(forClient.User).UpdateParticipent();
                    return;
                }

                if (specialTile.Code != null)
                    if (!ProcessMapCodeWithArg(forClient, specialTile))
                        return;
                locationStr = Meta.BuildSpecialTileInfo(forClient.User, specialTile);
            }


            byte[] areaMessage = PacketBuilder.CreateMeta(locationStr);
            forClient.SendPacket(areaMessage);

        }
        public static void UpdateStats(GameClient client)
        {
            if (!client.LoggedIn)
                return;

            client.User.MajorPriority = true;
            string metaWind = Meta.BuildStatsMenu(client.User);
            byte[] statsPacket = PacketBuilder.CreateMeta(metaWind);
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
            foreach (GameClient connectedClient in GameClient.ConnectedClients)
            {
                if (connectedClient.LoggedIn)
                {
                    if (connectedClient.User.Inventory.HasItemId(id))
                    {
                        InventoryItem invItm = connectedClient.User.Inventory.GetItemByItemId(id);
                        foreach (ItemInstance itm in invItm.ItemInstances.ToArray())
                            connectedClient.User.Inventory.Remove(itm);
                    }
                }
            }

            // Remove from shops
            foreach(Shop shop in Shop.ShopList)
            {
                if (shop.Inventory.HasItemId(id))
                {
                    InventoryItem invItm = shop.Inventory.GetItemByItemId(id);
                    foreach (ItemInstance itm in invItm.ItemInstances.ToArray())
                        shop.Inventory.Remove(itm);
                }

            }
            DroppedItems.DeleteAllItemsWithId(id); // Delete all dropped items
            Database.DeleteAllItemsFromUsers(id); // Delete from offline players
        }

        public static void StartRidingHorse(GameClient sender, int horseRandomId)
        {
            HorseInstance horseMountInst = sender.User.HorseInventory.GetHorseById(horseRandomId);

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

            sender.User.CurrentlyRidingHorse = horseMountInst;

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
                sender.User.Awards.AddAward(Award.GetAwardById(40)); // Camel Rider

                incBy = 13;
            }
            if(horseMountInst.Breed.Type == "llama")
            {
                sender.User.Awards.AddAward(Award.GetAwardById(41)); // Llama Rider

                incBy = 14;
            }
            if (horseMountInst.Breed.Type == "unicorn")
            {
                incBy = 15;
            }
            if (horseMountInst.Breed.Type == "pegasus")
            {
                incBy = 16;
            }
            if (horseMountInst.Breed.Id == 170) // Unipeg
            {
                incBy = 17;
            }

            incBy *= 5;
            sender.User.Facing %= 5;
            sender.User.Facing += incBy;
            sender.User.LastRiddenHorse = horseRandomId;

            UpdateUserFacingAndLocation(sender.User);

            byte[] updatePlayer = PacketBuilder.CreateMovement(sender.User.X, sender.User.Y, sender.User.CharacterId, sender.User.Facing, PacketBuilder.DIRECTION_NONE, true);
            sender.SendPacket(updatePlayer);

            if (sender.User.HorseWindowOpen)
                UpdateArea(sender);
        }
        public static void DoItemPurchases(GameClient sender)
        {
            if (!sender.LoggedIn)
                return;

            Item.ItemPurchaseQueueItem[] queueItems = Database.GetItemPurchaseQueue(sender.User.Id);
            foreach (Item.ItemPurchaseQueueItem queueItem in queueItems)
            {
                for (int i = 0; i < queueItem.ItemCount; i++)
                {
                    sender.User.Inventory.AddIgnoringFull(new ItemInstance(queueItem.ItemId));
                }
            }
            Database.ClearItemPurchaseQueue(sender.User.Id);

        }
        public static void StopRidingHorse(GameClient sender)
        {
            sender.User.CurrentlyRidingHorse = null;

            sender.User.Facing %= 5;
            UpdateUserFacingAndLocation(sender.User);

            byte[] updatePlayer = PacketBuilder.CreateMovement(sender.User.X, sender.User.Y, sender.User.CharacterId, sender.User.Facing, PacketBuilder.DIRECTION_NONE, true);
            sender.SendPacket(updatePlayer);

            if (sender.User.HorseWindowOpen)
                UpdateArea(sender);
        }
        public static bool ProcessMapCodeWithArg(GameClient forClient, World.SpecialTile tile)
        {
            string mapCode = tile.Code;
            if (mapCode == null)
                return false;
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
                            forClient.User.Teleport(newX, newY);
                            if (World.InIsle(tile.X, tile.Y))
                            {
                                World.Isle isle = World.GetIsle(tile.X, tile.Y);
                                int tileset = isle.Tileset;
                                int overlay = Map.GetTileId(tile.X, tile.Y, true);
                                if (tileset == 6 && overlay == 249) // warp point
                                {
                                    byte[] swfPacket = PacketBuilder.CreateSwfModule("warpcutscene", PacketBuilder.PACKET_SWF_MODULE_CUTSCENE);
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

                forClient.User.Tiredness = 1000;
                foreach(HorseInstance horse in forClient.User.HorseInventory.HorseList)
                {
                    horse.BasicStats.Tiredness = 1000;
                }
            }
            return true;
        }
        public static void OnShutdown()
        {
            if(ServerSocket != null)
                ServerSocket.Dispose();
            if (gameTimer != null)
                gameTimer.Dispose();
            if (minuteTimer != null)
                minuteTimer.Dispose();
        }
        public static void ShutdownServer(string shutdownReason = "No reason provided.")
        {
            Logger.InfoPrint("Server shutting down; " + shutdownReason);
            try
            {
                GameClient.OnShutdown(shutdownReason);
                GameServer.OnShutdown();
                Database.OnShutdown();
            }
            catch (Exception) { }

            Entry.OnShutdown();
        }


        public static void StartServer()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress hostIP = IPAddress.Parse(ConfigReader.BindIP);
            IPEndPoint ep = new IPEndPoint(hostIP, ConfigReader.Port);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(0xFFFF);
            gameTimer = new Timer(new TimerCallback(onGameTick), null, gameTickSpeed, gameTickSpeed);
            minuteTimer = new Timer(new TimerCallback(onMinuteTick), null, oneMinute, oneMinute);
            Logger.InfoPrint("Binding to ip: " + ConfigReader.BindIP + " On port: " + ConfigReader.Port.ToString());

            
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += GameClient.CreateClient;
            GameClient.CreateClient(null, e);
        }

    }
}
