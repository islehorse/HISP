using System.Collections.Generic;
using System.Linq;
using HISP.Player;
using HISP.Server;

namespace HISP.Game
{
    class Quest
    {
        public const string Shovel = "SHOVEL";
        public const string Binoculars = "BINOCS";
        public const string Rake = "RAKE";
        public const string MagnifyingGlass = "MAGNIFY";

        public struct QuestItemInfo
        {
            public int ItemId;
            public int Quantity;
        }
        public struct QuestAltActivation
        {
            public string Type;
            public int ActivateX;
            public int ActivateY; 
        }
        public struct QuestEntry
        {
            public int Id;
            public string Notes;
            public string Title;

            public int[] RequiresQuestIdComplete; // Not sure what this is for.
            public QuestAltActivation AltActivation;
            public bool Tracked; // Should we track how many times the player has completed this quest.
            // Fail Settings
            public int MaxRepeats;
            public int MoneyCost;
            public QuestItemInfo[] ItemsRequired;
            public string FailNpcChat;
            public int AwardRequired;
            public int[] RequiresQuestIdCompleted;
            public int[] RequiresQuestIdNotCompleted;

            // Success Settings
            public int MoneyEarned;
            public QuestItemInfo[] ItemsEarned;
            public int QuestPointsEarned;
            public int GotoNpcChatpoint;
            public int WarpX;
            public int WarpY;
            public string SuccessMessage;
            public string SuccessNpcChat;

            public bool HideReplyOnFail;
            public string Difficulty;
            public string Author;
            public int ChainedQuestId;
            public bool Minigame;
        }

        public static List<QuestEntry> QuestList = new List<QuestEntry>();

        public static int GetTotalQuestPoints()
        {
            int totalQp = 0;
            QuestEntry[] quests = GetPublicQuestList();
            foreach(QuestEntry quest in quests)
            {
                totalQp += quest.QuestPointsEarned;
            }
            return totalQp;
        }
        public static QuestEntry[] GetPublicQuestList()
        {
            List<QuestEntry> quests = QuestList.OrderBy(o => o.Title).ToList();
            foreach(QuestEntry quest in quests)
            {
                if (quest.Title == null)
                    quests.Remove(quest);

            }
            return quests.ToArray();
        }

        public static bool ActivateQuest(User user, QuestEntry quest, bool npcActivation = false)
        {
            
            if (quest.Tracked)
            {
                // Has completed other required quests?
                foreach (int questId in quest.RequiresQuestIdCompleted)
                    if (user.Quests.GetTrackedQuestAmount(quest.Id) < 1)
                        goto Fail;

                // Has NOT competed other MUST NOT BE required quests
                foreach (int questId in quest.RequiresQuestIdNotCompleted)
                    if (user.Quests.GetTrackedQuestAmount(quest.Id) > 1)
                        goto Fail;
                // Has allready tracked this quest?
                if (user.Quests.GetTrackedQuestAmount(quest.Id) >= quest.MaxRepeats)
                    goto Fail;

            }

            // Check if i have required items
            foreach (QuestItemInfo itemInfo in quest.ItemsRequired)
            {
                bool hasThisItem = false;
                InventoryItem[] items = user.Inventory.GetItemList();
                foreach (InventoryItem item in items)
                {
                    if (item.ItemId == itemInfo.ItemId && item.ItemInstances.Count >= itemInfo.Quantity)
                    {
                        hasThisItem = true;
                        break;
                    }
                }
                if (!hasThisItem)
                    goto Fail;
            }

            // Have enough money?
            if (user.Money < quest.MoneyCost)
                goto Fail;

            // Have required award (unimplemented)

            goto Success;

            Fail: {
                if(quest.FailNpcChat != null)
                {
                    if(!npcActivation)
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(quest.FailNpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        user.LoggedinClient.SendPacket(ChatPacket);
                    }
                }
                return false;
            };
            Success: {
                // Take Items
                foreach(QuestItemInfo itemInfo in quest.ItemsRequired)
                {
                    InventoryItem itm = user.Inventory.GetItemByItemId(itemInfo.ItemId);
                    for(int i = 0; i < itemInfo.Quantity; i++)
                        user.Inventory.Remove(itm.ItemInstances[0]);

                }
                user.Money -= quest.MoneyCost;
                // Give money
                user.Money += quest.MoneyEarned;
                // Give items
                foreach (QuestItemInfo itemInfo in quest.ItemsEarned)
                {
                    for (int i = 0; i < itemInfo.Quantity; i++)
                    {
                        ItemInstance itm = new ItemInstance(itemInfo.ItemId);
                        user.Inventory.AddIgnoringFull(itm);
                    }
                }
                if (quest.WarpX != 0 && quest.WarpY != 0)
                    user.Teleport(quest.WarpX, quest.WarpY);

                // Give quest points
                user.QuestPoints += quest.QuestPointsEarned;

                if (quest.ChainedQuestId != 0)
                    ActivateQuest(user, GetQuestById(quest.ChainedQuestId));

                if(quest.Tracked)
                    user.Quests.TrackQuest(quest.Id);

                if(quest.SuccessNpcChat != null)
                {
                    if (!npcActivation)
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(quest.SuccessNpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        user.LoggedinClient.SendPacket(ChatPacket);
                    }
                }

                if(quest.SuccessMessage != null)
                {
                    byte[] ChatPacket = PacketBuilder.CreateChat(quest.SuccessMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    user.LoggedinClient.SendPacket(ChatPacket);
                }
                return true;
            };
        }
        public static bool DoesQuestExist(int id)
        {
            try
            {
                GetQuestById(id);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }
        public static QuestEntry GetQuestById(int id)
        {
            foreach(QuestEntry quest in QuestList)
            {
                if(quest.Id == id)
                {
                    return quest;
                }
            }
            throw new KeyNotFoundException("QuestId: " + id + " Dont exist.");
        }
        public static bool UseTool(User user, string tool, int x, int y)
        {
            foreach(QuestEntry quest in QuestList)
            {
                if (quest.AltActivation.Type == tool && quest.AltActivation.ActivateX == x && quest.AltActivation.ActivateY == y)
                {
                    ActivateQuest(user, quest);
                    return true;
                }
            }
            return false;
        }
    }
}
