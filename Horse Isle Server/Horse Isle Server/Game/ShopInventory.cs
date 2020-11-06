using HISP.Server;
using System;
using System.Collections.Generic;

namespace HISP.Game
{
    class ShopInventory : IInventory
    {
        private Shop baseShop;
        private List<InventoryItem> inventoryItems;
        public int Count
        {
            get
            {
                return inventoryItems.Count;
            }
        }
        public ShopInventory(Shop shopkeeper)
        {
            baseShop = shopkeeper;

            ItemInstance[] instances = Database.GetShopInventory(baseShop.Id).ToArray();
            foreach (ItemInstance instance in instances)
            {
                addItem(instance, false);
            }
        }

        private void addItem(ItemInstance item, bool addToDatabase)
        {
            if (addToDatabase)
                Database.AddItemToInventory(baseShop.Id, item);

            foreach (InventoryItem invetoryItem in inventoryItems)
            {
                if (invetoryItem.ItemId == item.ItemId)
                {
                    invetoryItem.ItemInstances.Add(item);
                    return;
                }
            }

            InventoryItem inventoryItem = new InventoryItem();

            inventoryItem.ItemId = item.ItemId;
            inventoryItem.ItemInstances.Add(item);
            inventoryItems.Add(inventoryItem);
        }

        public void AddInfinity(Item.ItemInformation itemInfo)
        {
            InventoryItem inventoryItem = new InventoryItem();
            inventoryItem.ItemId = itemInfo.Id;
            inventoryItem.Infinite = true;
            for(int i = 0; i < 25; i++) // add 25
                inventoryItem.ItemInstances.Add(new ItemInstance(inventoryItem.ItemId));
        }
        public void Add(ItemInstance item)
        {
            addItem(item, true);
        }

        public InventoryItem GetItemByItemId(int itemId)
        {
            throw new NotImplementedException();
        }

        public InventoryItem GetItemByRandomid(int randomId)
        {
            throw new NotImplementedException();
        }

        public InventoryItem[] GetItemList()
        {
            throw new NotImplementedException();
        }

        public bool HasItem(int randomId)
        {
            throw new NotImplementedException();
        }

        public bool HasItemId(int itemId)
        {
            throw new NotImplementedException();
        }

        public void Remove(ItemInstance item)
        {
            throw new NotImplementedException();
        }
    }
}
