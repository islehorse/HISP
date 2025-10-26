using HISP.Game.Services;
using HISP.Server;
using HISP.Game.Items;

using System.Collections.Generic;
using System.Linq;
using HISP.Util;
using System;

namespace HISP.Game.Inventory
{
    public class ShopInventory : IInventory
    {
        private Shop baseShop;
        private ThreadSafeList<InventoryItem> inventoryItems;
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
            inventoryItems = new ThreadSafeList<InventoryItem>();
        }

        private void addItem(ItemInstance item, bool addToDatabase)
        {
            
            
            foreach (InventoryItem invetoryItem in inventoryItems)
            {
                if (invetoryItem.ItemId == item.ItemId)
                {
                    if (invetoryItem.Infinite)
                    {
                        addToDatabase = false;
                        goto retrn;
                    }
                    // no need to add +1, theres allready infinite quanity.


                    invetoryItem.AddItem(item);

                    goto retrn;
                }
            }

            InventoryItem inventoryItem = new InventoryItem();

            inventoryItem.ItemId = item.ItemId;
            inventoryItem.Infinite = false;
            inventoryItem.AddItem(item);
            inventoryItems.Add(inventoryItem);
            
            retrn:
            {
                if (addToDatabase)
                    Database.AddItemToShopInventory(baseShop.Id, item);
                return;
            };

        }

        public void AddInfinity(Item.ItemInformation itemInfo)
        {
            if (HasItemId(itemInfo.Id))
                return;

            InventoryItem inventoryItem = new InventoryItem();
            inventoryItem.ItemId = itemInfo.Id;
            inventoryItem.Infinite = true;

            for(int i = 0; i < 25; i++) // add 25
                inventoryItem.AddItem(new ItemInstance(inventoryItem.ItemId));

            inventoryItems.Add(inventoryItem);
        }
        public void Add(ItemInstance item, bool addToDb)
        {
            addItem(item, addToDb);
        }

        public void Add(ItemInstance item)
        {
            addItem(item, true);
        }

        public InventoryItem GetItemByItemId(int itemId)
        {
            InventoryItem[] items = GetItemList();
            foreach (InventoryItem item in items)
            {
                if (item.ItemId == itemId)
                {
                    return item;
                }
            }
            throw new KeyNotFoundException("id: " + itemId + " not found in shop inventory");
        }

        public InventoryItem GetItemByRandomid(int randomId)
        {
            InventoryItem[] items = GetItemList();
            foreach (InventoryItem item in items)
            {
                ItemInstance[] instances = item.ItemInstances.ToArray();
                foreach (ItemInstance instance in instances)
                {
                    if (instance.RandomId == randomId)
                        return item;
                }
            }
            throw new KeyNotFoundException("random id: " + randomId + " not found in shop inventory");
        }
        public Int64 GetSortPos(InventoryItem item)
        {
            if (item == null)
                return 0;

            Int64 bias = Item.Items.Length;
            Int64 sortBy = (Item.GetItemById(item.ItemId).SortBy * bias) + item.ItemId;
            if (item.Infinite)
                sortBy *= (bias*-1);

            Logger.DebugPrint("Sort position of: " + Item.GetItemById(item.ItemId).Name + " is: " + sortBy);

            return sortBy;
        }
        public InventoryItem[] GetItemList()
        {
            return inventoryItems.OrderBy(o => GetSortPos(o)).ToArray();
        }

        public bool HasItem(int randomId)
        {
            InventoryItem[] items = GetItemList();
            foreach (InventoryItem item in items)
            {
                ItemInstance[] instances = item.ItemInstances.ToArray();
                foreach (ItemInstance instance in instances)
                {
                    if (instance.RandomId == randomId)
                        return true;
                }
            }
            return false;
        }

        public bool HasItemId(int itemId)
        {
            InventoryItem[] items = GetItemList();
            foreach (InventoryItem item in items)
            {
                if (item.ItemId == itemId)
                {
                    return true;
                }
            }
            return false;
        }


        public void Remove(ItemInstance item)
        {

            foreach (InventoryItem inventoryItem in inventoryItems)
            {
                if (item.ItemId == inventoryItem.ItemId)
                {
                    foreach (ItemInstance instance in inventoryItem.ItemInstances)
                    {
                        if (instance.RandomId == item.RandomId)
                        {
                            inventoryItem.RemoveItem(instance);

                            if (inventoryItem.ItemInstances.Length <= 0)
                                inventoryItems.Remove(inventoryItem);
                            

                            if (!inventoryItem.Infinite) // no need to bug the database.
                                Database.RemoveItemFromShopInventory(baseShop.Id, item);
                            else
                                inventoryItem.AddItem(new ItemInstance(inventoryItem.ItemId)); // Gen new item in inventory to replace it.
                            return;
                        }
                    }
                }
            }

            Logger.ErrorPrint("Tried to remove item : " + item.RandomId + " from inventory when it was not in it");
        }
    }
}
