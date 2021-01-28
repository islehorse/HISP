using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game
{
    class Tracking
    {
        public enum TrackableItem
        { 
            PirateTreasure,
            Transport,
            HorseCapture,
            Crafting,
            WishingWell,
            Training,
            ArenaLoss,
            Trading,
            HorseLease,
            AutoSells,
            PegasusTeamup,
            TackShopGiveaway,
            QuizWin,
            RiddleWin,
            IsleCardsGameWin,
            HorsePawn,
            WaterbaloonGameWin,
            UnicornTeamup,
            PotOfGold,
            GameUpdates,
            UnipegTeamup
        }

        public class TrackedItem
        {
            public TrackedItem(User sbaseUser, TrackableItem what, int itmcount)
            {
                What = what;
                count = itmcount;
                baseUser = sbaseUser;
            }

            public TrackableItem What;
            public int Count
            {
                get
                {
                    return count;
                }
                set
                {
                    Database.SetTrackedItemCount(baseUser.Id, What, value);
                    count = value;
                }
            }

            private int count;
            private User baseUser;
        }
        private List<TrackedItem> trackingItems = new List<TrackedItem>();
        private User baseUser;
        public TrackedItem[] TrackingItems
        {
            get
            {
                return trackingItems.ToArray();
            }
        }
        public Tracking(User user)
        {
            baseUser = user;
            
            for(int i = 0; i < 20; i++)
            {
                TrackableItem item = (TrackableItem)i;
                if(Database.HasTrackedItem(user.Id, item))
                {
                    TrackedItem trackedItem = new TrackedItem(baseUser, item, Database.GetTrackedCount(user.Id, item));
                    trackingItems.Add(trackedItem);
                }
            }
        }

        public TrackedItem GetTrackedItem(TrackableItem what)
        {
            foreach(TrackedItem trackedItem in TrackingItems)
            {
                if (trackedItem.What == what)
                    return trackedItem;
            }

            // if it doesnt exist- create it
            TrackedItem item = new TrackedItem(baseUser, what, 0);
            Database.AddTrackedItem(baseUser.Id, what, 0);
            trackingItems.Add(item);
            return item;
        }

        public struct TrackedItemStatsMenu
        {
            public string What;
            public string Value;
        }
        public static List<TrackedItemStatsMenu> TrackedItemsStatsMenu = new List<TrackedItemStatsMenu>();
        public static string GetTrackedItemsStatsMenuName(TrackableItem item)
        {
            foreach(TrackedItemStatsMenu trackedItem in TrackedItemsStatsMenu)
            {
                if (trackedItem.What == item.ToString())
                    return trackedItem.Value;
            }

            throw new KeyNotFoundException("no such tracked item found.");
        }

    }
}
