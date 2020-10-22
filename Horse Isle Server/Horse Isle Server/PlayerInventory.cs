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
                Add(instance);
            }
        }
        
        public int Count
        {
            get
            {
                return inventoryItems.Count;
            }
        }
        
        public void Add(ItemInstance item)
        {
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
                            return;
                        }
                    }
                }
            }

            Logger.ErrorPrint("Tried to remove item : " + item.RandomID + " from inventory when it was not in it");
        }

    }
}
