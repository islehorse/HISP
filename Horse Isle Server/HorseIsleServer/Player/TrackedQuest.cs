using System;
using HISP.Server;
namespace HISP
{
    public class TrackedQuest
    {
        public TrackedQuest(int playerID, int questID, int timesComplete)
        {
            playerId = playerID;
            QuestId = questID;
            timesCompleted = timesComplete;
        }
        public int QuestId;
        private int playerId;
        public int TimesCompleted
        {
            get
            {
                return timesCompleted;
            }
            set
            {
                Database.SetTrackedQuestCompletedCount(playerId, QuestId, value);
                timesCompleted = value;
            }
        }
        private int timesCompleted;

    }
}
