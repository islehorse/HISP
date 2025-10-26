using HISP.Server;
using HISP.Util;
using System.Linq;

namespace HISP.Player
{
    public class Award
    {
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
            {
                AddAward(GetAwardById(awardid), false);
            }

        }



    }
}
