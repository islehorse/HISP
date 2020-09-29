using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class User
    {
        public int Id;
        public string Username;
        public bool Administrator;
        public bool Moderator;
        public bool NewPlayer = false;
        public Mailbox MailBox;

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
                Database.SetPlayerBankMoney(value, Id);
                money = value;
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

        private int charId;
        private string profilePage;
        private int x;
        private int y;
        private int money;
        private int bankMoney;

        public byte[] SecCodeSeeds = new byte[3];
        public  int SecCodeInc = 0;
        public int SecCodeCount = 0;


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
            return SecCode;
        }

        public User(int UserId)
        {
            if (!Database.CheckUserExist(UserId))
                throw new KeyNotFoundException("User " + UserId + " not found in database!");

            if (!Database.CheckUserExtExists(UserId))
            {
                Database.CreateUserExt(UserId);
                NewPlayer = true;
            }

            Id = UserId;
            Username = Database.GetUsername(UserId);

            Administrator = Database.CheckUserIsAdmin(Username);
            Moderator = Database.CheckUserIsModerator(Username);

            x = Database.GetPlayerX(UserId);
            y = Database.GetPlayerY(UserId);
            charId = Database.GetPlayerCharId(UserId);

            money = Database.GetPlayerMoney(UserId);
            bankMoney = Database.GetPlayerBankMoney(UserId);
            
            profilePage = Database.GetPlayerProfile(UserId);

            MailBox = new Mailbox(this);

            // Generate SecCodes

            Random rng = new Random();
            SecCodeSeeds[0] = (byte)rng.Next(0, 255 - 33);
            SecCodeSeeds[1] = (byte)rng.Next(0, 255 - 33);
            SecCodeSeeds[2] = (byte)rng.Next(0, 255 - 33);
            SecCodeInc = (byte)rng.Next(0, 255 - 33);

        }
    }
}
