using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

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
            foreach(Workshop wkShop in Workshops)
            {
                if(wkShop.X == x && wkShop.Y == y)
                {
                    return wkShop;
                }
            }
            throw new KeyNotFoundException("No workshop found.");
        }

        public static bool CraftIdExists(int id)
        {
            try
            {
                GetCraftId(id);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }
        public static CraftableItem GetCraftId(int id)
        {
            foreach(Workshop wkShop in Workshops)
            {
                foreach(CraftableItem crftItem in wkShop.CraftableItems)
                {
                    if (crftItem.Id == id)
                        return crftItem;
                }
            }
            throw new KeyNotFoundException("No craft id " + id + " was found.");
        }

    }
}
