using System;
using System.Collections.Generic;


namespace HISP.Game
{
    class InventoryItem
    {
        public InventoryItem() 
        {
            ItemInstances = new List<ItemInstance>();
        }

        public int ItemId;
        public List<ItemInstance> ItemInstances;
    }

    interface IInventory
    {

        void Add(ItemInstance item);
        void Remove(ItemInstance item);
        int Count
        {
            get;
        }

        InventoryItem[] GetItemList();


        bool HasItem(int randomId);
        bool HasItemId(int itemId);

        InventoryItem GetItemByItemId(int itemId);

        InventoryItem GetItemByRandomid(int randomId);


    }
}
