using System;
using System.Collections.Generic;

namespace HISP.Game
{
    class Shop
    {
        public int Id;

        public string[] BuysItemTypes;
        public int BuyPricePercentage;
        public int SellPricePercentage;
        public int[] InfniteStocks;
        ShopInventory Inventory;

        public Shop()
        {
            Id = shopList.Count;
            Inventory = new ShopInventory(this);
            shopList.Add(this);
        }
        
        public int CalculateBuyCost(Item.ItemInformation item)
        {
            return Math.Abs(item.SellPrice * (100 / BuyPricePercentage));
        }
        public int CalculateSellCost(Item.ItemInformation item)
        {
            return Math.Abs(item.SellPrice * (100 / SellPricePercentage));
        }



        // Static Functions 
        private static List<Shop> shopList = new List<Shop>();
        public static Shop GetShopById(int id)
        {
            return shopList[id];
        }

    }
}
