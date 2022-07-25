using System;
using System.Collections.Generic;
using HISP.Game.Items;

namespace HISP.Game.Inventory
{
    public class InventoryItem
    {
        public InventoryItem()
        {
            itemInstances = new List<ItemInstance>();
            Infinite = false;
            ItemId = 0;
        }

        public int ItemId;
        public bool Infinite;
        private List<ItemInstance> itemInstances;
        public void RemoveItem(ItemInstance itm)
        {
            itemInstances.Remove(itm);
        }
        public void AddItem(ItemInstance itm)
        {
            itemInstances.Add(itm);
        }
        public ItemInstance[] ItemInstances
        {
            get
            {
                return itemInstances.ToArray();
            }
        }
    }

}
