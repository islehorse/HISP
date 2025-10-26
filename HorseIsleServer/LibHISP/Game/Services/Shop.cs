using HISP.Game.Inventory;
using HISP.Server;
using HISP.Game.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HISP.Game.Services
{
    public class Shop
    {
        public int Id;
        public string[] BuysItemTypes;
        public int BuyPricePercentage;
        public int SellPricePercentage;
        public ShopInventory Inventory;

        public Shop(int[] infiniteStocks, int id)
        {
            this.Inventory = new ShopInventory(this);
            this.Id = id;

            foreach (int stock in infiniteStocks)
            {
                if (Item.ItemIdExist(stock))
                    this.Inventory.AddInfinity(Item.GetItemById(stock));
                else
                    Logger.WarnPrint("Item ID: " + stock + " doesn't exist, but shop " + id + " stocks it");
            }

            ItemInstance[] instances = Database.GetShopInventory(this.Id);
            foreach (ItemInstance instance in instances)
            {
                this.Inventory.Add(instance, false);
            }

            Shop.ShopList.Add(this);
        }
        
        public UInt64 CalculateBuyCost(Item.ItemInformation item)
        {
            return Convert.ToUInt64(Math.Round((double)item.SellPrice * ((double)SellPricePercentage / 100.0d)));
        }
        public UInt64 CalculateSellCost(Item.ItemInformation item)
        {
            return Convert.ToUInt64(Math.Round((double)item.SellPrice * ((double)BuyPricePercentage / 100.0d)));
        }

        public bool CanSell(Item.ItemInformation item)
        {
            return BuysItemTypes.Any(o => o == item.Type);
        }
        // Static Functions 
        public static List<Shop> ShopList = new List<Shop>();
        public static Shop GetShopById(int id)
        {
            return ShopList.First(o => o.Id == id);
        }

    }
}
