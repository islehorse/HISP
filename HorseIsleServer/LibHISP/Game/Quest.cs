using System;
using System.Collections.Generic;
using System.Linq;

using HISP.Game.Inventory;
using HISP.Player;
using HISP.Server;
using HISP.Game.Items;

namespace HISP.Game
{
    public class Quest
    {
        public const string Shovel = "SHOVEL";
        public const string Binoculars = "BINOCS";
        public const string Rake = "RAKE";
        public const string MagnifyingGlass = "MAGNIFY";
        public const int CloudIslesQuest = 1373;
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

            public int[] RequiresQuestIdCompleteStatsMenu; // Not sure what this is for.
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
            public int SetNpcChatpoint;
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

        private static List<QuestEntry> questList = new List<QuestEntry>();
        public static void AddQuestEntry(QuestEntry quest)
        {
            questList.Add(quest);
        }
        private static QuestEntry[] QuestList
        {
            get
            {
                return questList.ToArray();
            }
        }
        public static int GetTotalQuestPoints()
        {
            int totalQp = 0;
            QuestEntry[] quests = GetPublicQuestList();
            foreach (QuestEntry quest in quests)
            {
                totalQp += quest.QuestPointsEarned;
            }
            return totalQp;
        }


        public static int GetTotalQuestsComplete(User user)
        {
            QuestEntry[] questList = GetPublicQuestList();
            int totalComplete = 0;
            foreach (QuestEntry quest in questList)
            {
                if (user.Quests.GetTrackedQuestAmount(quest.Id) > 0)
                    totalComplete++;
            }
            return totalComplete;
        }


        public static QuestEntry[] GetPublicQuestList()
        {
            QuestEntry[] quests = QuestList.OrderBy(o => o.Title).ToArray();
            List<QuestEntry> sortedQuests = new List<QuestEntry>();
            foreach (QuestEntry quest in quests)
            {
                if (quest.Title != null)
                    sortedQuests.Add(quest);

            }
            return sortedQuests.ToArray();
        }

        public class QuestResult
        {
            public QuestResult()
            {
                NpcChat = null;
                SetChatpoint = -1;
                GotoChatpoint = -1;
                HideRepliesOnFail = false;
                QuestCompleted = false;
            }
            public string NpcChat;
            public int SetChatpoint;
            public int GotoChatpoint;
            public bool HideRepliesOnFail;
            public bool QuestCompleted;
        }

        public static bool CanComplete(User user, QuestEntry quest)
        {
            // Has completed other required quests?
            foreach (int questId in quest.RequiresQuestIdCompleted)
                if (user.Quests.GetTrackedQuestAmount(questId) < 1)
                    return false;

            // Has NOT competed other MUST NOT BE required quests
            foreach (int questId in quest.RequiresQuestIdNotCompleted)
                if (user.Quests.GetTrackedQuestAmount(questId) > 1)
                    return false;

            if (quest.Tracked)
            {

                // Has allready tracked this quest?
                if (user.Quests.GetTrackedQuestAmount(quest.Id) >= quest.MaxRepeats)
                    return false;

            }

            // Check if user has award unlocked
            if (quest.AwardRequired != 0)
                if (!user.Awards.HasAward(Award.GetAwardById(quest.AwardRequired)))
                    return false;

            // Check if i have required items
            foreach (QuestItemInfo itemInfo in quest.ItemsRequired)
            {
                bool hasThisItem = false;
                InventoryItem[] items = user.Inventory.GetItemList();
                foreach (InventoryItem item in items)
                {
                    if (item.ItemId == itemInfo.ItemId && item.ItemInstances.Length >= itemInfo.Quantity)
                    {
                        hasThisItem = true;
                        break;
                    }
                }
                if (!hasThisItem)
                    return false;
            }

            // Have enough money?
            if (user.Money < quest.MoneyCost)
                return false;


            return true;
        }

        public static QuestResult CompleteQuest(User user, QuestEntry quest, bool npcActivation = false, QuestResult res=null)
        {
            if(res == null)
                res = new QuestResult();
            // Take Items
            foreach (QuestItemInfo itemInfo in quest.ItemsRequired)
            {
                InventoryItem itm = user.Inventory.GetItemByItemId(itemInfo.ItemId);
                for (int i = 0; i < itemInfo.Quantity; i++)
                    user.Inventory.Remove(itm.ItemInstances[0]);

            }
            // Take Money
            user.TakeMoney(quest.MoneyCost);
            // Give money
            user.AddMoney(quest.MoneyEarned);
            // Give items
            foreach (QuestItemInfo itemInfo in quest.ItemsEarned)
            {
                for (int i = 0; i < itemInfo.Quantity; i++)
                {
                    ItemInstance itm = new ItemInstance(itemInfo.ItemId);
                    Item.ItemInformation itemInformation = itm.GetItemInfo();
                    if (itemInformation.Type == "CONCEPTUAL")
                        Item.ConsumeItem(user, itemInformation);
                    else
                        user.Inventory.AddIgnoringFull(itm);
                }
            }
            if (quest.WarpX != 0 && quest.WarpY != 0)
                user.Teleport(quest.WarpX, quest.WarpY);

            // Give quest points
            user.QuestPoints += quest.QuestPointsEarned;

            res.QuestCompleted = true;
            if (npcActivation)
            {
                if (quest.SuccessNpcChat != null && quest.SuccessNpcChat.Trim() != "")
                    res.NpcChat = quest.SuccessNpcChat;

                if(quest.SetNpcChatpoint != -1)
                    res.SetChatpoint = quest.SetNpcChatpoint;

                if(quest.GotoNpcChatpoint != -1)
                    res.GotoChatpoint = quest.GotoNpcChatpoint;
            }

            if (quest.Tracked)
                user.Quests.TrackQuest(quest.Id);

            if (quest.ChainedQuestId != 0)
                res = ActivateQuest(user, Quest.GetQuestById(quest.ChainedQuestId), npcActivation, res);

            if (quest.SuccessMessage != null)
            {
                byte[] ChatPacket = PacketBuilder.CreateChat(quest.SuccessMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.LoggedinClient.SendPacket(ChatPacket);
            }

            if (quest.SuccessNpcChat != null)
            {
                if (!npcActivation)
                {
                    byte[] ChatPacket = PacketBuilder.CreateChat(quest.SuccessNpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    user.LoggedinClient.SendPacket(ChatPacket);
                }
            }

            // Check if award unlocked
            int questPointsPercent = Convert.ToInt32(Math.Floor(((decimal)user.QuestPoints / (decimal)GetTotalQuestPoints()) * (decimal)100.0)); 
            if (questPointsPercent >= 25)
                user.Awards.AddAward(Award.GetAwardById(1)); // 25% Quest Completion Award.
            if (questPointsPercent >= 50)
                user.Awards.AddAward(Award.GetAwardById(2)); // 50% Quest Completion Award.
            if (questPointsPercent >= 75)
                user.Awards.AddAward(Award.GetAwardById(3)); // 75% Quest Completion Award.
            if (questPointsPercent >= 100)
                user.Awards.AddAward(Award.GetAwardById(4)); // 100% Quest Completion Award.

            // Is cloud isles quest?
            if (quest.Id == CloudIslesQuest)
            {
                byte[] swfLoadPacket = PacketBuilder.CreateSwfModule("ballooncutscene", PacketBuilder.PACKET_SWF_CUTSCENE);
                user.LoggedinClient.SendPacket(swfLoadPacket);
            }

            return res;
        }
        public static QuestResult FailQuest(User user, QuestEntry quest, bool npcActivation = false, QuestResult res=null)
        {
            if(res == null)
                res = new QuestResult();
            res.QuestCompleted = false;

            if(npcActivation)
            {
                if(quest.GotoNpcChatpoint != -1)
                   res.GotoChatpoint = quest.GotoNpcChatpoint;
                if(quest.HideReplyOnFail != false)
                    res.HideRepliesOnFail = quest.HideReplyOnFail;
                if(res.SetChatpoint != -1)
                    res.SetChatpoint = quest.SetNpcChatpoint;
            }
            if (quest.FailNpcChat != null)
            {
                if (!npcActivation)
                {
                    byte[] ChatPacket = PacketBuilder.CreateChat(quest.FailNpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    user.LoggedinClient.SendPacket(ChatPacket);
                }
                else
                {
                    if(quest.FailNpcChat != null)
                    {
                        if(npcActivation)
                        {
                            if (quest.FailNpcChat != "")
                            {
                                res.NpcChat = quest.FailNpcChat;
                            }
                        }
                    }
                }
            }
            return res;
        }
        public static QuestResult ActivateQuest(User user, QuestEntry quest, bool npcActivation = false, QuestResult res=null)
        {

            if (CanComplete(user, quest))
            {
                return CompleteQuest(user, quest, npcActivation, res);
            }
            else 
            {
                return FailQuest(user, quest, npcActivation, res);
            }

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
            throw new KeyNotFoundException("Quest Id: " + id + " Dont exist.");
        }
        public static bool UseTool(User user, string tool, int x, int y)
        {
            if (tool == Quest.Shovel)
            {
                // check Treasures
                if (Treasure.IsTileTreasure(x, y))
                {
                    Treasure treasure = Treasure.GetTreasureAt(x, y);
                    if(treasure.Type == "BURIED")
                        treasure.CollectTreasure(user);
                    return true;
                }
            }

            QuestResult result = null;

            foreach (QuestEntry quest in QuestList)
            {
                
                if (quest.AltActivation.Type == tool && quest.AltActivation.ActivateX == x && quest.AltActivation.ActivateY == y)
                {
                    result = ActivateQuest(user, quest, true, result);
                    if(result.QuestCompleted)
                    {
                        if(result.NpcChat != null)
                        {
                            byte[] ChatPacket = PacketBuilder.CreateChat(result.NpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            user.LoggedinClient.SendPacket(ChatPacket);
                        }
                        return true;
                    }
                }
            }

            if(result != null)
            {
                if (result.NpcChat != null)
                {
                    byte[] ChatPacket = PacketBuilder.CreateChat(result.NpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    user.LoggedinClient.SendPacket(ChatPacket);
                }
                return true;
            }


            return false;
        }
    }
}
