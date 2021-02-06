using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Services
{
    public class Workshop
    {
        public Workshop()
        {
            CraftableItems = new List<CraftableItem>();
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
                RequiredItems = new List<RequiredItem>();
            }
            public int Id;
            public int GiveItemId;
            public int MoneyCost;
            public List<RequiredItem> RequiredItems;
        }
        public int X;
        public int Y;
        public List<CraftableItem> CraftableItems;

        public static List<Workshop> Workshops = new List<Workshop>();

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
