using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class PlayerInventory : IInventory
    {
        private User baseUser;
        private List<InventoryItem> inventoryItems;
        public PlayerInventory(User forUser)
        {
            inventoryItems = new List<InventoryItem>();

            baseUser = forUser;
            ItemInstance[] instances = Database.GetPlayerInventory(baseUser.Id).ToArray();
            foreach(ItemInstance instance in instances)
            {
                addItem(instance, false);
            }
        }
        
        public int Count
        {
            get
            {
                return inventoryItems.Count;
            }
        }
        private void addItem(ItemInstance item, bool addToDatabase)
        {
            if (addToDatabase)
                Database.AddItemToInventory(baseUser.Id, item);

            foreach (InventoryItem invetoryItem in inventoryItems)
            {
                if (invetoryItem.ItemId == item.ItemID)
                {
                    invetoryItem.ItemInstances.Add(item);
                    return;
                }
            }

            InventoryItem inventoryItem = new InventoryItem();

            inventoryItem.ItemId = item.ItemID;
            inventoryItem.ItemInstances.Add(item);
            inventoryItems.Add(inventoryItem);
        }


        public void Add(ItemInstance item)
        {
            addItem(item, true);
        }


        public InventoryItem[] GetItemList()
        {
            return inventoryItems.OrderBy(o => o.ItemInstances[0].GetItemInfo().SortBy).ToArray();
        }



        public void Remove(ItemInstance item)
        {

            Database.RemoveItemFromInventory(baseUser.Id, item);

            foreach (InventoryItem inventoryItem in inventoryItems)
            {
                if(item.ItemID == inventoryItem.ItemId)
                {
                    foreach(ItemInstance instance in inventoryItem.ItemInstances)
                    {
                        if(instance.RandomID == item.RandomID)
                        {
                            inventoryItem.ItemInstances.Remove(instance);

                            if (inventoryItem.ItemInstances.Count <= 0)
                                inventoryItems.Remove(inventoryItem);

                            return;
                        }
                    }
                }
            }

            Logger.ErrorPrint("Tried to remove item : " + item.RandomID + " from inventory when it was not in it");
        }

        public bool HasItem(int randomId)
        {
            InventoryItem[] items = GetItemList();
            foreach(InventoryItem item in items)
            {
                ItemInstance[] instances = item.ItemInstances.ToArray();
                foreach(ItemInstance instance in instances)
                {
                    if (instance.RandomID == randomId)
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
            throw new KeyNotFoundException("id: " + itemId + " not found in inventory");
        }

        public InventoryItem GetItemByRandomid(int randomId)
        {
            InventoryItem[] items = GetItemList();
            foreach (InventoryItem item in items)
            {
                ItemInstance[] instances = item.ItemInstances.ToArray();
                foreach (ItemInstance instance in instances)
                {
                    if (instance.RandomID == randomId)
                        return item;
                }
            }
            throw new KeyNotFoundException("random id: " + randomId + " not found in inventory");
        }

    }
}
