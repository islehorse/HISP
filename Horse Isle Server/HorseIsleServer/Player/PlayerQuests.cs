using System.Collections.Generic;
using HISP.Server;

namespace HISP.Player
{
    class PlayerQuests
    {
        private List<TrackedQuest> trackedQuests = new List<TrackedQuest>();
        public User BaseUser;
        public TrackedQuest[] QuestList
        {
            get
            {
                return trackedQuests.ToArray();
            }
        }
        public void Add(int questId, int timesCompleted)
        {
            TrackedQuest quest = new TrackedQuest(BaseUser.Id, questId, 0);
            quest.TimesCompleted = timesCompleted;
            trackedQuests.Add(quest);
        }
        public int GetTrackedQuestAmount(int questId)
        {
            foreach(TrackedQuest quest in QuestList)
            {
                if (quest.QuestId == questId)
                    return quest.TimesCompleted;
            }
            return 0;
        }
        
        public void TrackQuest(int questId)
        {
            foreach (TrackedQuest quest in QuestList)
            {
                if (quest.QuestId == questId)
                {
                    quest.TimesCompleted++;
                    return;
                }
            }
            Add(questId, 1);
        }
        public PlayerQuests(User user)
        {
            BaseUser = user;
            TrackedQuest[] quests = Database.GetTrackedQuests(user.Id);
            foreach (TrackedQuest quest in quests)
                trackedQuests.Add(quest);
        }
    }
}
