using System.Collections.Generic;
using System.Linq;

using HISP.Player;
using HISP.Server;
using HISP.Game.Items;

namespace HISP.Game.Inventory
{
    
    public class PlayerInventory : IInventory
    {
        

        public User BaseUser;
        private ThreadSafeList<InventoryItem> inventoryItems;
        public PlayerInventory(User forUser)
        {
            inventoryItems = new ThreadSafeList<InventoryItem>();

            BaseUser = forUser;
            ItemInstance[] instances = Database.GetPlayerInventory(BaseUser.Id).ToArray();
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
                Database.AddItemToInventory(BaseUser.Id, item);

            foreach (InventoryItem invetoryItem in inventoryItems)
            {
                if (invetoryItem.ItemId == item.ItemId)
                {
                    invetoryItem.AddItem(item);
                    return;
                }
            }

            InventoryItem inventoryItem = new InventoryItem();

            inventoryItem.ItemId = item.ItemId;
            inventoryItem.AddItem(item);
            inventoryItems.Add(inventoryItem);
        }




        public InventoryItem[] GetItemList()
        {
            return inventoryItems.OrderBy(o => Item.GetItemById(o.ItemId).SortBy).ToArray();
        }



        public void Remove(ItemInstance item)
        {

            Database.RemoveItemFromInventory(BaseUser.Id, item);

            foreach (InventoryItem inventoryItem in inventoryItems)
            {
                if(item.ItemId == inventoryItem.ItemId)
                {
                    foreach(ItemInstance instance in inventoryItem.ItemInstances)
                    {
                        if(instance.RandomId == item.RandomId)
                        {
                            inventoryItem.RemoveItem(instance);

                            if (inventoryItem.ItemInstances.Length <= 0)
                                inventoryItems.Remove(inventoryItem);

                            return;
                        }
                    }
                }
            }

            Logger.ErrorPrint("Tried to remove item : " + item.RandomId + " from inventory when it was not in it");
        }

        public bool HasItem(int randomId)
        {
            InventoryItem[] items = GetItemList();
            foreach(InventoryItem item in items)
            {
                ItemInstance[] instances = item.ItemInstances.ToArray();
                foreach(ItemInstance instance in instances)
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
                    if (instance.RandomId == randomId)
                        return item;
                }
            }
            throw new KeyNotFoundException("random id: " + randomId + " not found in inventory");
        }
        public void AddWithoutDatabase(ItemInstance item)
        {
            addItem(item, false);
        }

        public void AddIgnoringFull(ItemInstance item)
        {
            addItem(item, true);
        }
        public void Add(ItemInstance item)
        {
            // Check if has max allready
            if(HasItemId(item.ItemId))
            {
                InventoryItem items = GetItemByItemId(item.ItemId);
                if (items.ItemInstances.Length >= Item.MAX_STACK)
                {
                    throw new InventoryMaxStackException();
                }
            }
            else if (Count >= BaseUser.MaxItems)
            {
                throw new InventoryFullException();
            }


            addItem(item, true);
        }


    }
}
