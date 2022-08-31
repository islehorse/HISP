using HISP.Game.Items;
using HISP.Server;

namespace HISP.Game.Inventory
{
    public class InventoryItem
    {
        public InventoryItem()
        {
            itemInstances = new ThreadSafeList<ItemInstance>();
            Infinite = false;
            ItemId = 0;
        }

        public int ItemId;
        public bool Infinite;
        private ThreadSafeList<ItemInstance> itemInstances;
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
