using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class ItemInstance 
    {
        public int RandomId;
        public int ItemId;
        


        public Item.ItemInformation GetItemInfo()
        {
            return Item.GetItemById(ItemId);

        }

        public ItemInstance(int id,int randomId = -1)
        {
            RandomId = RandomID.NextRandomId(randomId);

            ItemId = id; 

        }

    }
}
