using System;
using System.Collections.Generic;


namespace HISP.Game
{

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
