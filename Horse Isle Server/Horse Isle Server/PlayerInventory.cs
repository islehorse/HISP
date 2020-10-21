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
        private List<ItemInstance> instances = new List<ItemInstance>();

        public PlayerInventory(User forUser)
        {
            baseUser = forUser;
            instances = Database.GetPlayerInventory(baseUser.Id);
        }
        
        public int Count
        {
            get
            {
                return instances.Count;
            }
        }

        public void Add(ItemInstance item)
        {
            instances.Add(item);
            Database.AddItemToInventory(baseUser.Id, item);
        }

        public ItemInstance[] GetItemList()
        {
            return instances.ToArray();
        }

        public void Remove(ItemInstance item)
        {
            instances.Remove(item);
            Database.RemoveItemFromInventory(baseUser.Id, item);
        }
    }
}
