using HISP.Server;
using HISP.Util;
using System;
using System.Collections.Generic;

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
            //99% fo the time this will work
            try
            {
                AwardEntry award = GlobalAwardList[id - 1];
                if (award.Id == id)
                    return award;
            }
            catch (Exception) { };

            // Incase it doesnt...
            foreach(AwardEntry award in GlobalAwardList)
            {
                if (award.Id == id)
                    return award;
            }

            throw new KeyNotFoundException("Award ID " + id + " Does not exist.");
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
            foreach(AwardEntry awardEntry in AwardsEarned)
            {
                if (awardEntry.Id == award.Id)
                    return true;
            }
            return false;
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
                baseUser.LoggedinClient.SendPacket(chatPacket);
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
