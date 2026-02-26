using HISP.Game.Items;
using System.Linq;

namespace HISP.Game.Inventory
{

    public abstract class InventoryBase
    {
        public abstract void Add(ItemInstance item);
        public abstract void Remove(ItemInstance item);
        public abstract int Count
        {
            get;
        }

        public abstract InventoryItem[] Items {
            get;
        }

        public virtual bool HasItem(int randomId)
        {
            return Items.SelectMany(o => o.ItemInstances).Any(o => o.RandomId == randomId);
        }

        public virtual bool HasItemId(int itemId)
        {
            return Items.Any(o => o.ItemId == itemId);
        }

        public virtual InventoryItem GetItemByItemId(int itemId)
        {
            return Items.First(o => o.ItemId == itemId);
        }

        public virtual InventoryItem GetItemByRandomid(int randomId)
        {
            return Items.First(o => o.ItemInstances.Any(o => o.RandomId == randomId));
        }

        public virtual int GetItemCount(int randomId)
        {
            return GetItemByRandomid(randomId).ItemInstances.Length;
        }
        public virtual int GetItemCountById(int itemId)
        {
            return GetItemByItemId(itemId).ItemInstances.Length;
        }



    }
}
