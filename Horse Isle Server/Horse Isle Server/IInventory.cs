using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
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


    }
}
