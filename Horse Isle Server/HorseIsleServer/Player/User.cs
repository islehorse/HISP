using System;
using System.Collections.Generic;
using HISP.Game;
using HISP.Server;
using HISP.Player.Equips;
using HISP.Game.Services;
using HISP.Game.Inventory;
using HISP.Game.Horse;
using System.Linq;

namespace HISP.Player
{
    public class User
    {

        public int Id;
        public string Username;
        public bool Administrator;
        public bool Moderator;
        public bool NewPlayer = false;
        public GameClient LoggedinClient;
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
        public bool NoClip = false;
        public string Gender;
        public bool MetaPriority = false;
        public List<Auction.AuctionBid> Bids = new List<Auction.AuctionBid>();
        public bool Idle;
        public int Facing;

        public int MaxItems
        {
            get
            {
                int baseValue = 40;
                if (Subscribed)
                {
                    if (OwnedRanch != null)
                    {
                        baseValue += 20 * OwnedRanch.GetBuildingCount(4); // Shed
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

                return 7; // will change when ranches are implemented.
            }
        }
        public HorseInfo.Breed PawneerOrderBreed = null;
        public string PawneerOrderColor = "";
        public string PawneerOrderGender = "";
        public bool InRealTimeQuiz = false;
        public int PendingTradeTo;
        public Mailbox MailBox;
        public Friends Friends;
        public string Password; // For chat filter.
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
        public List<User> BeingSocializedBy = new List<User>();
        public User PendingBuddyRequestTo;
        public Dance ActiveDance;
        public bool CanUseAdsChat = true;
        public int CapturingHorseId;
        public DateTime LoginTime;
        public string LastSeenWeather;
        public string AutoReplyText = "";
        public int LastClickedRanchBuilding = 0;
        public bool ListingAuction = false;
        public int TotalGlobalChatMessages = 1;

        public void TakeMoney(int amount)
        {
            int money = Money;
            money -= amount;
            Database.SetPlayerMoney(money, Id);
            GameServer.UpdatePlayer(LoggedinClient);
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
                money = 2147483647;
            }

            Database.SetPlayerMoney(money, Id);
            GameServer.UpdatePlayer(LoggedinClient);
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
                return Converters.UnixTimeStampToDateTime(subscribedUntil);
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
                
                int Timestamp = Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());
                if(Timestamp > subscribedUntil && subscribed) // sub expired.
                {
                    Logger.InfoPrint(Username + "'s Subscription expired. (timestamp now: " + Timestamp + " exp date: " + subscribedUntil+" )");
                    Database.SetUserSubscriptionStatus(this.Id, false);
                    subscribed = false;
                }

                return subscribed;
            }
            set
            {
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

        public Trade TradingWith = null;

        public int AttemptingToOfferItem;
        public bool TradeMenuPriority = false;

        public byte[] SecCodeSeeds = new byte[3];
        public  int SecCodeInc = 0;
        public int SecCodeCount = 0;

        public int GetPlayerListIcon()
        {
            int icon = -1;
            if (NewPlayer)
                icon = Messages.NewUserIcon;
            if (Subscribed)
            {
                int months = (DateTime.UtcNow.Month - SubscribedUntil.Month) + 12 * (DateTime.UtcNow.Year - SubscribedUntil.Year);
                if (months <= 1)
                    icon = Messages.MonthSubscriptionIcon;
                else if (months <= 3)
                    icon = Messages.ThreeMonthSubscripitionIcon;
                else
                    icon = Messages.YearSubscriptionIcon;
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

            X = newX;
            Y = newY;

            byte[] MovementPacket = PacketBuilder.CreateMovementPacket(X, Y, CharacterId, Facing, PacketBuilder.DIRECTION_TELEPORT, true);
            LoggedinClient.SendPacket(MovementPacket);
            GameServer.UpdateWeather(LoggedinClient);

            User[] onScreenNow = GameServer.GetOnScreenUsers(X, Y, true, true);

            User[] goneOffScreen = onScreenBefore.Except(onScreenNow).ToArray();
            User[] goneOnScreen = onScreenNow.Except(onScreenBefore).ToArray();

            foreach(User offScreenUsers in goneOffScreen)
            {
                if (offScreenUsers.Id == this.Id)
                    continue;

                byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(1000 + 4, 1000 + 1, Facing, CharacterId, Username);
                offScreenUsers.LoggedinClient.SendPacket(playerInfoBytes);
            }

            foreach (User onScreenUsers in goneOnScreen)
            {
                if (onScreenUsers.Id == this.Id)
                    continue;

                byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(onScreenUsers.X, onScreenUsers.Y, onScreenUsers.Facing, onScreenUsers.CharacterId, onScreenUsers.Username);
                LoggedinClient.SendPacket(playerInfoBytes);
            }

            // Players now offscreen tell the client is at 1000,1000.
            /* foreach (User onScreenBeforeUser in onScreenBefore)
             {
                 bool found = false;
                 foreach (User onScreenNowUser in onScreenNow)
                 {
                     if (onScreenNowUser.Id == onScreenBeforeUser.Id)
                     {
                         found = true;
                         break;
                     }
                 }

                 if (!found)
                 {
                     byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(1000 + 4, 1000 + 1, Facing, CharacterId, Username);
                     onScreenBeforeUser.LoggedinClient.SendPacket(playerInfoBytes);
                 }
             }

             // Players now onscreen tell the client there real pos
             foreach (User onScreenNowUser in onScreenNow)
             {
                 bool found = false;
                 foreach (User onScreenBeforeUser in onScreenBefore)
                 {
                     if (onScreenNowUser.Id == onScreenBeforeUser.Id)
                     {
                         found = true;
                         break;
                     }
                 }

                 if (!found)
                 {
                     byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(onScreenNowUser.X, onScreenNowUser.Y, onScreenNowUser.Facing, onScreenNowUser.CharacterId, onScreenNowUser.Username);
                     LoggedinClient.SendPacket(playerInfoBytes);
                 }
             }
            */
            GameServer.Update(LoggedinClient);
        }

        public byte[] GenerateSecCode()
        {
            var i = 0;
            SecCodeCount++;
            SecCodeSeeds[SecCodeCount % 3] = (byte)(SecCodeSeeds[SecCodeCount % 3] + SecCodeInc);
            SecCodeSeeds[SecCodeCount % 3] = (byte)(SecCodeSeeds[SecCodeCount % 3] % 92);
            i = SecCodeSeeds[0] + SecCodeSeeds[1] * SecCodeSeeds[2] - SecCodeSeeds[1];
            i = Math.Abs(i);
            i = i % 92;

            byte[] SecCode = new byte[4];
            SecCode[0] = (byte)(SecCodeSeeds[0] + 33);
            SecCode[1] = (byte)(SecCodeSeeds[1] + 33);
            SecCode[2] = (byte)(SecCodeSeeds[2] + 33);
            SecCode[3] = (byte)(i + 33);
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
            
            Administrator = Database.CheckUserIsAdmin(Username);
            Moderator = Database.CheckUserIsModerator(Username);

            chatViolations = Database.GetChatViolations(UserId);
            x = Database.GetPlayerX(UserId);
            y = Database.GetPlayerY(UserId);
            charId = Database.GetPlayerCharId(UserId);

            Facing = PacketBuilder.DIRECTION_DOWN;
            experience = Database.GetExperience(UserId);
            //money = Database.GetPlayerMoney(UserId);
            bankMoney = Database.GetPlayerBankMoney(UserId);
            questPoints = Database.GetPlayerQuestPoints(UserId);
            subscribed = Database.IsUserSubscribed(UserId);
            subscribedUntil = Database.GetUserSubscriptionExpireDate(UserId);
            profilePage = Database.GetPlayerProfile(UserId);
            privateNotes = Database.GetPlayerNotes(UserId);
            hunger = Database.GetPlayerHunger(UserId);
            thirst = Database.GetPlayerThirst(UserId);
            tired = Database.GetPlayerTiredness(UserId);

            if(Ranch.IsRanchOwned(this.Id))
            {
                OwnedRanch = Ranch.GetRanchOwnedBy(this.Id);
            }    

            Gender = Database.GetGender(UserId);
            MailBox = new Mailbox(this);
            Highscores = new Highscore(this);
            Awards = new Award(this);
            MutePlayer = new MutedPlayers(this);
            TrackedItems = new Tracking(this);
            HorseInventory = new HorseInventory(this);

            // Generate SecCodes


            SecCodeSeeds[0] = (byte)GameServer.RandomNumberGenerator.Next(40, 60);
            SecCodeSeeds[1] = (byte)GameServer.RandomNumberGenerator.Next(40, 60);
            SecCodeSeeds[2] = (byte)GameServer.RandomNumberGenerator.Next(40, 60);
            SecCodeInc = (byte)GameServer.RandomNumberGenerator.Next(40, 60);


            Friends = new Friends(this);
            LoginTime = DateTime.UtcNow;
            LoggedinClient = baseClient;
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
                            Bids.Add(bid);
                            auctionEntry.Bidders.Add(bid);
                        }

                    }
                }
            }

        }
    }
}
