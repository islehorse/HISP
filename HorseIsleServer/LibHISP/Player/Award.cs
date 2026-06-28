using HISP.Server;
using HISP.Util;
using System.Linq;

namespace HISP.Player
{
    public class Award
    {
        public const int AWARD_25_QP = 1;
        public const int AWARD_50_QP = 2;
        public const int AWARD_75_QP = 3;
        public const int AWARD_100_QP = 4;
        public const int AWARD_GOOD_JUMPER = 5;
        public const int AWARD_GREAT_JUMPER = 6;
        public const int AWARD_GOOD_RACER = 7;
        public const int AWARD_GREAT_RACER = 8;
        public const int AWARD_GOOD_DRESSAGE = 9;
        public const int AWARD_GREAT_DRESSAGE = 10;
        public const int AWARD_RIDDLER_RIDDLE = 11;
        public const int AWARD_MINIGAME_PLAYER = 12;
        public const int AWARD_MINIGAME_MASTER = 13;
        public const int AWARD_MINIGAME_NUT = 14;
        public const int AWARD_MINIGAME_CRAZY = 15;
        public const int AWARD_BETA_TESTER = 16;
        public const int AWARD_GEO_WIZ = 17;
        public const int AWARD_PIRATE_TRACKER = 18;
        public const int AWARD_PRATE_STALKER = 19;
        public const int AWARD_LEPRECHAUN = 20;
        public const int AWARD_LUCKY_LEPRECHAUN = 21;
        public const int AWARD_CRAFTINESS = 22;
        public const int AWARD_WORKMANSHIP = 23;
        public const int AWARD_WRANGLER = 24;
        public const int AWARD_PRO_WRANGLER = 25;
        public const int AWARD_PRO_TRAINER = 26;
        public const int AWARD_TRAVELER = 27;
        public const int AWARD_GLOBETROTTER = 28;
        public const int AWARD_TRADER = 29;
        public const int AWARD_WELL_WISHER = 30;
        public const int AWARD_STAR_WISHER = 31;
        public const int AWARD_PERSERVERANCE = 32;
        public const int AWARD_QUICK_WIT = 33;
        public const int AWARD_RIDDLE_GENIUS = 34;
        public const int AWARD_HORSE_ANATOMY_WIZ = 35;
        public const int AWARD_DIAMOND_MINER = 36;
        public const int AWARD_COCO_NUT_AWARD = 37;
        public const int AWARD_STRONG_HORSE_AWARD = 38;
        public const int AWARD_STRONGEST_HORSE_AWARD = 39;
        public const int AWARD_CAMEL_RIDER = 40;
        public const int AWARD_LLAMA_RIDER = 41;
        public const int AWARD_UNICORN_FRIEND = 42;
        public const int AWARD_PEGASUS_FRIEND = 43;
        public const int AWARD_VENDOR = 44;
        public const int AWARD_PRO_VENDOR = 45;
        public const int AWARD_TOP_WRANGLER = 46;
        public const int AWARD_STAR_APPRENTACE = 47;
        public const int AWARD_MASTER_CRAFTSMAN = 48;
        public const int AWARD_TOUR_GUIDE = 49;
        public const int AWARD_PRO_TRADER = 50;
        public const int AWARD_EXTRODINARY_WISHER = 51;
        public const int AWARD_TOP_VENDOR = 52;
        public const int AWARD_TOP_TRAINER = 53;
        public const int AWARD_QUIZ_GENIUS = 54;
        public const int AWARD_UNIPEG_FRIEND = 55;

        public struct AwardEntry
        {
            public int Id;
            public int Sort;
            public string Title;
            public int IconId;
            public int MoneyBonus;
            public string CompletionText;
            public string Description;
        }

        public static AwardEntry[] GlobalAwardList;

        public static AwardEntry GetAwardById(int id)
        {
            return GlobalAwardList.First(o => o.Id == id);
        }


        private ThreadSafeList<AwardEntry> awardsEarned;
        private User baseUser;
        public AwardEntry[] AwardsEarned
        {
            get
            {
                return awardsEarned.ToArray();
            }
        }

        public bool HasAward(AwardEntry award)
        {
            return AwardsEarned.Any(o => o.Id == award.Id);
        }

        public void AddAward(AwardEntry award,bool addToDatabase=true)
        {
            if (HasAward(award))
                return;

            if (addToDatabase)
            {
                Database.AddAward(baseUser.Id, award.Id);

                baseUser.AddMoney(award.MoneyBonus);

                byte[] chatPacket = PacketBuilder.CreateChat(award.CompletionText, PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.Client.SendPacket(chatPacket);
            }
                

            awardsEarned.Add(award);
        }

        public Award(User user)
        {
            baseUser = user;
            int[] awards = Database.GetAwards(user.Id);
            awardsEarned = new ThreadSafeList<AwardEntry>();

            foreach (int awardid in awards)
                AddAward(GetAwardById(awardid), false);

        }



    }
}
