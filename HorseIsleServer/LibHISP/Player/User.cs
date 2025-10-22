﻿using System;
using System.Collections.Generic;
using System.Linq;

using HISP.Game;
using HISP.Server;
using HISP.Player.Equips;
using HISP.Game.Services;
using HISP.Game.Inventory;
using HISP.Game.Horse;
using HISP.Util;


namespace HISP.Player
{
    public class User
    {
        private ThreadSafeList<Auction.AuctionBid> bids = new ThreadSafeList<Auction.AuctionBid>();
        private ThreadSafeList<User> beingSocializedBy = new ThreadSafeList<User>();

        private int chatViolations;
        private int charId;
        private int subscribedUntil;
        private bool subscribed;
        private string profilePage;
        private string privateNotes;
        private int x;
        private bool stealth = false;
        private int y;
        private int questPoints;
        private double bankMoney;
        private int experience;
        private int hunger;
        private int thirst;
        private int tired;
        private bool noClip = false;
        private bool administrator = false;
        private bool moderator = false;

        public bool NoClip
        {
            get
            {
                if(CurrentlyRidingHorse != null)
                {
                    if(CurrentlyRidingHorse.Breed != null)
                    {
                        if (CurrentlyRidingHorse.Breed.Type == "pegasus")
                        {
                            return true;
                        }
                        if (CurrentlyRidingHorse.Breed.Id == 170)
                        {
                            return true;
                        }
                    }
                }
                return this.noClip;
            }
            set
            {
                this.noClip = value;
            }
        }


        public bool Administrator
        {
            get
            {
                return this.administrator;
            }
            set
            {
                this.administrator = value;
                Database.SetUserAdmin(Id, this.administrator);
            }
        }

        public bool Moderator
        {
            get
            {
                if (administrator)
                    return true;
                return moderator;
            }
            set
            {
                moderator = value;
                Database.SetUserMod(Id, moderator);
            }
        }

        public Trade TradingWith = null;
        public int AttemptingToOfferItem;
        public bool TradeMenuPriority = false;
        public byte[] SecCodeSeeds = new byte[3];
        public int SecCodeInc = 0;
        public int SecCodeCount = 0;
        public int Id;
        public string Username;
        public bool NewPlayer = false;
        public GameClient Client;
        public CompetitionGear EquipedCompetitionGear;
        public Jewelry EquipedJewelry;
        public bool MuteAds = false;
        public bool MuteGlobal = false;
        public bool MuteIsland = false;
        public bool MuteNear = false;
        public bool MuteHere = false;
        public bool MuteBuddy = false;
        public bool MutePrivateMessage = false;
        public bool MuteBuddyRequests = false;
        public bool MuteSocials = false;
        public bool MuteAll = false;
        public bool MuteLogins = false;
        public string Gender;
        public bool UserInfoSend = false;
        public bool MajorPriority = false;
        public bool MinorPriority = false;
        public bool HorseWindowOpen = false;
        public bool Idle;
        public int Facing;
        public HorseInfo.Breed PawneerOrderBreed = null;
        public string PawneerOrderColor = "";
        public string PawneerOrderGender = "";
        public bool InRealTimeQuiz = false;
        public int PendingTradeTo;
        public Mailbox MailBox;
        public Friends Friends;
        
        // For chat filter.
        public string Password; 

        public PlayerInventory Inventory;
        public Npc.NpcEntry LastTalkedToNpc;
        public Shop LastShoppedAt;
        public Inn LastVisitedInn;
        public HorseInventory HorseInventory;
        public HorseInstance LastViewedHorse;
        public HorseInstance LastViewedHorseOther;
        public int LastRiddenHorse = 0;
        public HorseInstance CurrentlyRidingHorse;
        public Tracking TrackedItems;
        public Ranch OwnedRanch = null;
        public PlayerQuests Quests;
        public Highscore Highscores;
        public MutedPlayers MutePlayer;
        public Riddler LastRiddle;
        public Award Awards;
        public User SocializingWith;
        public User PendingBuddyRequestTo;
        public Dance ActiveDance;
        public bool CanUseAdsChat = true;
        public int CapturingHorseId;
        public DateTime LoginTime;
        public string LastSeenWeather;
        public string AutoReplyText = "";
        public int LastClickedRanchBuilding = 0;
        public int TotalGlobalChatMessages = 1;
        public User[] BeingSocializedBy
        {
            get
            {
                return beingSocializedBy.ToArray();
            }
        }
        public Auction.AuctionBid[] Bids
        {
            get
            {
                return bids.ToArray();
            }
        }
        public int MaxItems
        {
            get
            {
                int baseValue = 40;
                if (Subscribed)
                {
                    if (OwnedRanch != null)
                    {
                        baseValue += (20 * OwnedRanch.GetBuildingCount(4)); // Shed
                        if (baseValue > 80) // 2 sheds max!
                            baseValue = 80;
                    }
                }
                return baseValue;
            }
        }
        public int MaxHorses
        {
            get
            {
                if (Subscribed)
                {
                    int baseValue = 11;
                    if(OwnedRanch != null)
                    {
                        baseValue += OwnedRanch.GetBuildingCount(1) * 4; // Barn
                        baseValue += OwnedRanch.GetBuildingCount(10) * 8; // Big Barn
                        baseValue += OwnedRanch.GetBuildingCount(11) * 12; // Gold Barn
                    }
                    return baseValue;
                }

                return 7; 
            }
        }
        
        public void TakeMoney(int amount)
        {
            int money = Money;
            money -= amount;
            Database.SetPlayerMoney(money, Id);
            GameServer.UpdatePlayer(Client);
        }

        public void SetMoney(int amount)
        {
            Database.SetPlayerMoney(amount, Id);
            GameServer.UpdatePlayer(Client);
        }

        public void AddMoney(int amount)
        {
            int money = Money;
            try
            {
                checked
                {
                    money += amount;
                }
            }
            catch(OverflowException)
            {
                money = Int32.MaxValue;
            }

            Database.SetPlayerMoney(money, Id);
            GameServer.UpdatePlayer(Client);
        }
        public string GetWeatherSeen()
        {
            string weather = "SUNNY";
            if (World.InTown(this.X, this.Y))
                weather = World.GetTown(this.X, this.Y).Weather;
            if (World.InIsle(this.X, this.Y))
                weather = World.GetIsle(this.X, this.Y).Weather;
            LastSeenWeather = weather;
            return weather;
        }
        public void DoRanchActions()
        {
            if(OwnedRanch != null)
            {
                Tiredness = 1000; // All ranches fully rest you.
                
                if(OwnedRanch.GetBuildingCount(2) > 0)
                {
                    Thirst = 1000;
                    foreach (HorseInstance horse in HorseInventory.HorseList)
                        horse.BasicStats.Thirst = 1000;
                }

                if (OwnedRanch.GetBuildingCount(3) > 0)
                {
                    foreach (HorseInstance horse in HorseInventory.HorseList)
                        horse.BasicStats.Hunger = 1000;
                }
                if(OwnedRanch.GetBuildingCount(9) > 0)
                {
                    Hunger = 1000;
                }
                if( (OwnedRanch.GetBuildingCount(1) > 0)|| (OwnedRanch.GetBuildingCount(10) > 0) || (OwnedRanch.GetBuildingCount(11) > 0))
                {

                    foreach (HorseInstance horse in HorseInventory.HorseList)
                        horse.BasicStats.Tiredness = 1000;
                }

            }
        }
        public DateTime SubscribedUntil
        {
            get
            {
                return Helper.UnixTimeStampToDateTime(subscribedUntil);
            }
            set
            {
                subscribedUntil = Convert.ToInt32(((DateTimeOffset)value).ToUnixTimeSeconds());
                Database.SetUserSubscriptionStatus(Id, subscribedUntil);
            }
        }
        public int FreeMinutes
        {
            get
            {
                int freeTime = Database.GetFreeTime(Id);
                return freeTime;
            }
            set
            {
                Database.SetFreeTime(Id, value);
            }
        }
        public bool Subscribed
        { 
            get
            {
                if (ConfigReader.AllUsersSubbed)
                    return true;

                if (Administrator)
                    return true;
                
                int timestamp = Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());
                if(timestamp > subscribedUntil && subscribed) // sub expired.
                {
                    Logger.InfoPrint(Username + "'s Subscription expired. (timestamp now: " + timestamp + " exp date: " + subscribedUntil+" )");
                    Database.SetUserSubscriptionStatus(this.Id, false);
                    subscribed = false;
                }

                return subscribed;
            }
            set
            {
                subscribed = value;
                Database.SetUserSubscriptionStatus(this.Id, value);
            }
        }
        public bool Stealth
        {
            get
            {
                return stealth;
            }
            set
            {
                if (value)
                    Database.RemoveOnlineUser(this.Id);
                else
                    Database.AddOnlineUser(this.Id, this.Administrator, this.Moderator, this.Subscribed, this.NewPlayer);

                stealth = value;
            }
        }
        public int ChatViolations
        {
            get
            {
                return chatViolations;
            }
            set
            {
                Database.SetChatViolations(value,Id);
                chatViolations = value;
            }
        }

        public string PrivateNotes
        {
            get
            {
                return privateNotes;
            }
            set
            {

                privateNotes = value.Trim();
                Database.SetPlayerNotes(Id, privateNotes);
                
            }
        }
        public string ProfilePage { 
            get 
            { 
                return profilePage; 
            }
            set 
            {
                profilePage = value.TrimEnd();
                Database.SetPlayerProfile(profilePage, Id);
            } 
        }

        public int Money
        {
            get
            {
                return Database.GetPlayerMoney(Id);
            }
        }


        public int Experience
        {
            get
            {
                return experience;
            }
            set
            {
                Database.SetExperience(Id, value);
                experience = value;
            }
        }
        public int QuestPoints
        {
            get
            {
                return questPoints;
            }
            set
            {
                Database.SetPlayerQuestPoints(value, Id);
                questPoints = value;
            }
        }

        public double BankInterest
        {
            get
            {
                return Database.GetPlayerBankInterest(Id);
            }
            set
            {
                if (value > 9999999999.9999)
                    value = 9999999999.9999;

                Database.SetPlayerBankInterest(value, Id);
            }
        }
        public double BankMoney
        {
            get
            {
                return bankMoney;
            }
            set
            {
                if (value > 9999999999.9999)
                    value = 9999999999.9999;

                Database.SetPlayerBankMoney(value, Id);
                bankMoney = value;
                BankInterest = value;
            }
        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                Database.SetPlayerX(value, Id);
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                Database.SetPlayerY(value, Id);
                y = value;
            }
        }

        public int CharacterId
        {
            get
            {
                return charId;
            }
            set
            {
                Database.SetPlayerCharId(value, Id);
                charId = value;
            }
        }


        public int Hunger
        {
            get
            {
                return hunger;
            }
            set
            {
                if (value >= 1000)
                    value = 1000;
                if (value <= 0)
                    value = 0;
                Database.SetPlayerHunger(Id, value);
                hunger = value;
            }
        }

        public int Thirst
        {
            get
            {
                return thirst;
            }
            set
            {
                if (value >= 1000)
                    value = 1000;
                if (value <= 0)
                    value = 0;
                Database.SetPlayerThirst(Id, value);
                thirst = value;
            }
        }

        public int Tiredness
        {
            get
            {
                return tired;
            }
            set
            {
                if (value >= 1000)
                    value = 1000;
                if (value <= 0)
                    value = 0;
                Database.SetPlayerTiredness(Id, value);
                tired = value;
            }
        }
        public void ClearSocailizedWith()
        {
            beingSocializedBy.Clear();
        }
        public void RemoveSocailizedWith(User user)
        {
            beingSocializedBy.Remove(user);
        }
        public void AddSocailizedWith(User user)
        {
            beingSocializedBy.Add(user);
        }
        public void AddBid(Auction.AuctionBid bid)
        {
            bids.Add(bid);
        }

        public void RemoveBid(Auction.AuctionBid bid)
        {
            bids.Remove(bid);
        }
        public int GetPlayerListIcon()
        {
            int icon = -1;
            if (NewPlayer)
                icon = Messages.NewUserIcon;
            if (Subscribed)
            {
                int months = Helper.GetMonthsBetweenTwoDateTimes(DateTime.UtcNow, SubscribedUntil);

                if(months >= 12 + 3)
                    icon = Messages.YearSubscriptionIcon;
                else if (months >= 3)
                    icon = Messages.ThreeMonthSubscripitionIcon;
                else
                    icon = Messages.MonthSubscriptionIcon;
            }
            if (Moderator)
                icon = Messages.ModeratorIcon;
            if (Administrator)
                icon = Messages.AdminIcon;

            return icon;
        }
        public void Teleport(int newX, int newY)
        {
            Logger.DebugPrint("Teleporting: " + Username + " to: " + newX.ToString() + "," + newY.ToString());

            User[] onScreenBefore = GameServer.GetOnScreenUsers(X, Y, true, true);
            User[] onScreenNow = GameServer.GetOnScreenUsers(newX, newY, true, true);

            X = newX;
            Y = newY;

            byte[] MovementPacket = PacketBuilder.CreateMovement(X, Y, CharacterId, Facing, PacketBuilder.DIRECTION_TELEPORT, true);
            Client.SendPacket(MovementPacket);
            GameServer.UpdateWeather(Client);


            User[] goneOffScreen = onScreenBefore.Except(onScreenNow).ToArray();
            User[] goneOnScreen = onScreenNow.Except(onScreenBefore).ToArray();


            // Players now offscreen tell the client is at 1000,1000.
            foreach (User offScreenUsers in goneOffScreen)
            {
                if (offScreenUsers.Id == this.Id)
                    continue;

                byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(1000 + 4, 1000 + 1, this.Facing, this.CharacterId, this.Username);
                offScreenUsers.Client.SendPacket(playerInfoBytes);
            }

            // Tell players now on screen there locations
            foreach (User onScreenUsers in goneOnScreen)
            {
                if (onScreenUsers.Id == this.Id)
                    continue;

                byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(onScreenUsers.X, onScreenUsers.Y, onScreenUsers.Facing, onScreenUsers.CharacterId, onScreenUsers.Username);
                Client.SendPacket(playerInfoBytes);
            }


            GameServer.Update(Client);
        }

        // Insert LGBT Patch here
        public string GetPronouns(bool possessive)
        {
            if (Gender == "FEMALE")
                return possessive ? Messages.PronounFemaleHer : Messages.PronounFemaleShe;
            else if (Gender == "MALE")
                return possessive ? Messages.PronounMaleHis : Messages.PronounMaleHe;
            else
                return possessive ? Messages.PronounNeutralTheir : Messages.PronounNeutralThey;
        }

        public byte[] GenerateSecCode()
        {
            SecCodeCount = ((SecCodeCount + 1) % 3);
            
            SecCodeSeeds[SecCodeCount] = (byte)(SecCodeSeeds[SecCodeCount] + SecCodeInc);
            SecCodeSeeds[SecCodeCount] = (byte)(SecCodeSeeds[SecCodeCount] % '\\');
            
            double i = SecCodeSeeds[0] + SecCodeSeeds[1] * SecCodeSeeds[2] - SecCodeSeeds[1];
            i = Math.Abs(i);
            i %= '\\';

            byte[] SecCode = new byte[4];

            SecCode[0] = (byte)(SecCodeSeeds[0] + '!');
            SecCode[1] = (byte)(SecCodeSeeds[1] + '!');
            SecCode[2] = (byte)(SecCodeSeeds[2] + '!');
            SecCode[3] = (byte)(i + '!');

            Logger.DebugPrint("Expecting "+Username+" To send Sec Code: "+BitConverter.ToString(SecCode).Replace("-", " "));
            return SecCode;
        }

        
        public User(GameClient baseClient, int UserId)
        {
            if (!Database.CheckUserExist(UserId))
                throw new KeyNotFoundException("User " + UserId + " not found in database!");

            if (!Database.CheckUserExtExists(UserId))
            {
                Database.CreateUserExt(UserId);
                NewPlayer = true;
            }

            EquipedCompetitionGear = new CompetitionGear(UserId);
            EquipedJewelry = new Jewelry(UserId);

            Id = UserId;
            Username = Database.GetUsername(UserId);
            
            administrator = Database.GetUserAdmin(Id);
            moderator = Database.GetUserModerator(Id);

            chatViolations = Database.GetChatViolations(UserId);
            x = Database.GetPlayerX(UserId);
            y = Database.GetPlayerY(UserId);
            charId = Database.GetPlayerCharId(UserId);

            Facing = PacketBuilder.DIRECTION_DOWN;
            experience = Database.GetExperience(UserId);
            
            bankMoney = Database.GetPlayerBankMoney(UserId);
            questPoints = Database.GetPlayerQuestPoints(UserId);
            subscribed = Database.GetUserSubscribed(UserId);
            subscribedUntil = Database.GetUserSubscriptionExpireDate(UserId);
            profilePage = Database.GetPlayerProfile(UserId);
            privateNotes = Database.GetPlayerNotes(UserId);
            hunger = Database.GetPlayerHunger(UserId);
            thirst = Database.GetPlayerThirst(UserId);
            tired = Database.GetPlayerTiredness(UserId);

            if(Ranch.GetOwnedRanch(this.Id))
                OwnedRanch = Ranch.GetRanchOwnedBy(this.Id);

            Gender = Database.GetGender(UserId);
            MailBox = new Mailbox(this);
            Highscores = new Highscore(this);
            Awards = new Award(this);
            MutePlayer = new MutedPlayers(this);
            TrackedItems = new Tracking(this);
            HorseInventory = new HorseInventory(this);

            // Generate SecCodes


            SecCodeSeeds[0] = (byte)GameServer.RandomNumberGenerator.Next('!', '\\');
            SecCodeSeeds[1] = (byte)GameServer.RandomNumberGenerator.Next('!', '\\');
            SecCodeSeeds[2] = (byte)GameServer.RandomNumberGenerator.Next('!', '\\');
            SecCodeInc      = (byte)GameServer.RandomNumberGenerator.Next('!', '\\');


            Friends = new Friends(this);
            LoginTime = DateTime.UtcNow;
            Client = baseClient;
            Inventory = new PlayerInventory(this);
            Quests = new PlayerQuests(this);

            // Get auctions
            foreach(Auction auction in Auction.AuctionRooms)
            {
                foreach(Auction.AuctionEntry auctionEntry in auction.AuctionEntries)
                {
                    if(auctionEntry.HighestBidder == this.Id)
                    {
                        Auction.AuctionBid bid = new Auction.AuctionBid();
                        bid.BidUser = this;
                        bid.BidAmount = auctionEntry.HighestBid;
                        bid.AuctionItem = auctionEntry;

                        if(bid.BidAmount > 0)
                        {
                            bids.Add(bid);
                            auctionEntry.AddBid(bid);
                        }

                    }
                }
            }

        }
    }
}
