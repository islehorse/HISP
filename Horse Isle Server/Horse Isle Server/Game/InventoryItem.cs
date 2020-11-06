using System;
using System.Collections.Generic;

namespace HISP.Game
{
    class InventoryItem
    {
        public InventoryItem()
        {
            ItemInstances = new List<ItemInstance>();
            Infinite = false;
            ItemId = 0;
        }

        public int ItemId;
        public bool Infinite;
        public List<ItemInstance> ItemInstances;
    }

}
