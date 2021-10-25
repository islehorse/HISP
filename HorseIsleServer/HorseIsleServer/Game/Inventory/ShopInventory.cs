using HISP.Game.Services;
using HISP.Server;
using HISP.Game.Items;

using System.Collections.Generic;
using System.Linq;

namespace HISP.Game.Inventory
{
    public class ShopInventory : IInventory
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
            inventoryItems = new List<InventoryItem>();

            foreach (ItemInstance instance in instances)
            {
                addItem(instance, false);
            }
        }

        private void addItem(ItemInstance item, bool addToDatabase)
        {
            
            
            foreach (InventoryItem invetoryItem in inventoryItems)
            {
                if (invetoryItem.ItemId == item.ItemId)
                {
                    if (invetoryItem.Infinite) // no need to add +1, theres allready infinite quanity.
                        return;

                    invetoryItem.ItemInstances.Add(item);

                    goto retrn;
                }
            }

            InventoryItem inventoryItem = new InventoryItem();

            inventoryItem.ItemId = item.ItemId;
            inventoryItem.Infinite = false;
            inventoryItem.ItemInstances.Add(item);
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
                inventoryItem.ItemInstances.Add(new ItemInstance(inventoryItem.ItemId));

            inventoryItems.Add(inventoryItem);
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
        public InventoryItem[] GetItemList()
        {
            return inventoryItems.OrderBy(o => o.ItemInstances[0].GetItemInfo().SortBy).OrderBy(o => o.Infinite).ToArray();
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
                            inventoryItem.ItemInstances.Remove(instance);

                            if (inventoryItem.ItemInstances.Count <= 0)
                                inventoryItems.Remove(inventoryItem);
                            

                            if (!inventoryItem.Infinite) // no need to bug the database.
                                Database.RemoveItemFromShopInventory(baseShop.Id, item);
                            else
                                inventoryItem.ItemInstances.Add(new ItemInstance(inventoryItem.ItemId)); // Gen new item in inventory to replace it.
                            return;
                        }
                    }
                }
            }

            Logger.ErrorPrint("Tried to remove item : " + item.RandomId + " from inventory when it was not in it");
        }
    }
}
