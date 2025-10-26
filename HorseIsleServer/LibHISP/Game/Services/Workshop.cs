using System.Collections.Generic;
using System.Linq;

namespace HISP.Game.Services
{
    public class Workshop
    {
        public Workshop()
        {
            craftableItems = new List<CraftableItem>();
        }
        public class RequiredItem
        {
            public int RequiredItemId;
            public int RequiredItemCount;
        }
        public class CraftableItem
        {
            public CraftableItem()
            {
                requiredItems = new List<RequiredItem>();
            }
            public int Id;
            public int GiveItemId;
            public int MoneyCost;
            private List<RequiredItem> requiredItems;

            public void AddRequiredItem(RequiredItem item)
            {
                requiredItems.Add(item);
            }
            public RequiredItem[] RequiredItems
            {
                get
                {
                    return requiredItems.ToArray();
                }
            }
        }
        public int X;
        public int Y;
        private List<CraftableItem> craftableItems;
        private static List<Workshop> workshops = new List<Workshop>();
        public void AddCraftableItem(CraftableItem craftItem)
        {
            craftableItems.Add(craftItem);
        }
        public static void AddWorkshop(Workshop wkShop)
        {
            workshops.Add(wkShop);
        }
        public CraftableItem[] CraftableItems
        {
            get
            {
                return craftableItems.ToArray();
            }
        }
        public static Workshop[] Workshops
        {
            get
            {
                return workshops.ToArray();
            }
        }
        public static Workshop GetWorkshopAt(int x, int y)
        {
            return Workshops.First(o => o.X == x && o.Y == y);
        }

        public static bool CraftIdExists(int id)
        {
            return Workshops.SelectMany(o => o.CraftableItems).Any(o => o.Id == id);
        }
        public static CraftableItem GetCraftId(int id)
        {
            return Workshops.SelectMany(o => o.CraftableItems).First(o => o.Id == id);
        }

    }
}
