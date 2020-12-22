using System;
using System.Collections.Generic;
using HISP.Game;
using HISP.Server;
namespace HISP.Player
{
    class User
    {

        public int Id;
        public string Username;
        public bool Administrator;
        public bool Moderator;
        public bool NewPlayer = false;
        public GameClient LoggedinClient;
        public CompetitionGear EquipedCompetitionGear;
        public bool MuteAds = false;
        public bool MuteGlobal = false;
        public bool MuteIsland = false;
        public bool MuteNear = false;
        public bool MuteHere = false;
        public bool MuteBuddy = false;
        public bool MutePrivateMessage = false;
        public bool MuteBuddyRequests = false;
        public bool MuteSocials = false;
        public bool MuteLogins = false;
        public string Gender;
        public bool MetaPriority = false;

        public int Facing;
        public Mailbox MailBox;
        public Friends Friends;
        public string Password; // For chat filter.
        public PlayerInventory Inventory;
        public Npc.NpcEntry LastTalkedToNpc;
        public Shop LastShoppedAt;
        public PlayerQuests Quests;
        public int FreeMinutes
        {
            get
            {
                return freeMinutes;
            }
            set
            {
                Database.SetFreeTime(Id, value);
                freeMinutes = value;
            }
        }
        public bool Subscribed
        { 
            get
            {
                if (ConfigReader.AllUsersSubbed)
                    return true;
                
                int Timestamp = Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());
                if(Timestamp > subscribedUntil) // sub expired.
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
                    Database.AddOnlineUser(this.Id, this.Administrator, this.Moderator, this.Subscribed);

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
        public string ProfilePage { 
            get 
            { 
                return profilePage; 
            }
            set 
            { 
                Database.SetPlayerProfile(value, Id);
                profilePage = value;
            } 
        }

        public int Money
        {
            get
            {
                return money;
            }
            set
            {
                money = value;
                Database.SetPlayerMoney(value, Id);
                GameServer.UpdatePlayer(LoggedinClient);
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

        public int BankMoney
        {
            get
            {
                return bankMoney;
            }
            set
            {
                Database.SetPlayerBankMoney(value, Id);
                bankMoney = value;
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

        private int chatViolations;
        private int charId;
        private int subscribedUntil;
        private bool subscribed;
        private string profilePage;
        private int x;
        private bool stealth = false;
        private int y;
        private int money;
        private int freeMinutes;
        private int questPoints;
        private int bankMoney;
        private int experience; 

        public byte[] SecCodeSeeds = new byte[3];
        public  int SecCodeInc = 0;
        public int SecCodeCount = 0;

        public void Teleport(int newX, int newY)
        {
            Logger.DebugPrint("Teleporting: " + Username + " to: " + newX.ToString() + "," + newY.ToString());

            X = newX;
            Y = newY;

            byte[] MovementPacket = PacketBuilder.CreateMovementPacket(X, Y, CharacterId, Facing, PacketBuilder.DIRECTION_TELEPORT, true);
            LoggedinClient.SendPacket(MovementPacket);
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

            Id = UserId;
            Username = Database.GetUsername(UserId);
            
            Administrator = Database.CheckUserIsAdmin(Username);
            Moderator = Database.CheckUserIsModerator(Username);

            chatViolations = Database.GetChatViolations(UserId);
            x = Database.GetPlayerX(UserId);
            y = Database.GetPlayerY(UserId);
            charId = Database.GetPlayerCharId(UserId);

            Facing = PacketBuilder.DIRECTION_DOWN;
            freeMinutes = Database.GetFreeTime(UserId);
            experience = Database.GetExperience(UserId);
            money = Database.GetPlayerMoney(UserId);
            bankMoney = Database.GetPlayerBankMoney(UserId);
            questPoints = Database.GetPlayerQuestPoints(UserId);
            subscribed = Database.IsUserSubscribed(UserId);
            subscribedUntil = Database.GetUserSubscriptionExpireDate(UserId);
            profilePage = Database.GetPlayerProfile(UserId);
            Gender = Database.GetGender(UserId);
            MailBox = new Mailbox(this);



            // Generate SecCodes


            SecCodeSeeds[0] = (byte)GameServer.RandomNumberGenerator.Next(40, 60);
            SecCodeSeeds[1] = (byte)GameServer.RandomNumberGenerator.Next(40, 60);
            SecCodeSeeds[2] = (byte)GameServer.RandomNumberGenerator.Next(40, 60);
            SecCodeInc = (byte)GameServer.RandomNumberGenerator.Next(40, 60);

            // Make some friends! (Get a life!)

            Friends = new Friends(this);

            LoggedinClient = baseClient;

            Inventory = new PlayerInventory(this);

            Quests = new PlayerQuests(this);
        }
    }
}
