using HISP.Game.Inventory;
using HISP.Server;
using System;
using System.Collections.Generic;

namespace HISP.Game.Services
{
    class Shop
    {
        public int Id;

        public string[] BuysItemTypes;
        public int BuyPricePercentage;
        public int SellPricePercentage;
        public ShopInventory Inventory;

        public Shop(int[] infiniteStocks)
        {
            Id = ShopList.Count+1;
            this.Inventory = new ShopInventory(this);


            foreach(int stock in infiniteStocks)
            {
                if (Item.ItemIdExist(stock))
                    this.Inventory.AddInfinity(Item.GetItemById(stock));
                else
                    Logger.WarnPrint("Item ID: " + stock + " Does not exist.");
            }
            Shop.ShopList.Add(this);
        }
        
        public int CalculateBuyCost(Item.ItemInformation item)
        {
            return (int)Math.Round((float)item.SellPrice * (100.0 / (float)BuyPricePercentage));
        }
        public int CalculateSellCost(Item.ItemInformation item)
        {
            return (int)Math.Round((float)item.SellPrice * (100.0 / (float)SellPricePercentage));
        }

        public bool CanSell(Item.ItemInformation item)
        {
            foreach(string ItemType in BuysItemTypes)
            {
                if(ItemType == item.Type)
                {
                    return true;
                }
            }
            return false;
        }
        // Static Functions 
        public static List<Shop> ShopList = new List<Shop>();
        public static Shop GetShopById(int id)
        {
            return ShopList[id-1];
        }

    }
}
