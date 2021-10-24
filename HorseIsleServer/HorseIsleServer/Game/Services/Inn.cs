using HISP.Player;
using HISP.Game.Items;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Services
{
    public class Inn
    {
        private static List<Inn> listInns = new List<Inn>();
        public static Inn[] Inns
        {
            get
            {
                return listInns.ToArray();
            }
        }
        public int Id;
        public Item.ItemInformation[] RestsOffered;
        public Item.ItemInformation[] MealsOffered;
        public int BuyPercentage;
        public int CalculateBuyCost(Item.ItemInformation item)
        {
            return (int)Math.Floor((float)item.SellPrice * (100.0 / (float)BuyPercentage));
        }

        public Item.ItemInformation GetStockedItem(int itemId)
        {
            
            // Check if inn stock.. 
            foreach(Item.ItemInformation offering in RestsOffered)
            {
                if (offering.Id == itemId)
                    return offering;
            }

            foreach (Item.ItemInformation offering in MealsOffered)
            {
                if (offering.Id == itemId)
                    return offering;
            }

            throw new KeyNotFoundException("Item is not stocked by this inn.");
        }


        public Inn(int id, int[] restsOffered, int[] mealsOffered, int buyPercentage)
        {
            Id = id;
            List<Item.ItemInformation> itemInfos = new List<Item.ItemInformation>();

            foreach(int itemId in restsOffered)
            {
                itemInfos.Add(Item.GetItemById(itemId));
            }

            RestsOffered = itemInfos.ToArray();
            itemInfos.Clear();

            foreach (int itemId in mealsOffered)
            {
                itemInfos.Add(Item.GetItemById(itemId));
            }
            MealsOffered = itemInfos.ToArray();

            itemInfos.Clear();
            itemInfos = null;

            BuyPercentage = buyPercentage;
            listInns.Add(this);
        }

        public static Inn GetInnById(int id)
        {
            foreach (Inn inn in Inns)
                if (inn.Id == id)
                    return inn;
            throw new KeyNotFoundException("Inn " + id + " doesnt exist.");

        }
    }
}
