using HISP.Game.Items;

namespace HISP.Game.Inventory
{

    interface IInventory
    {

        void Add(ItemInstance item);
        void Remove(ItemInstance item);
        int Count
        {
            get;
        }

        InventoryItem[] Items {
            get;
        }

        bool HasItem(int randomId);
        bool HasItemId(int itemId);

        InventoryItem GetItemByItemId(int itemId);

        InventoryItem GetItemByRandomid(int randomId);


    }
}
