using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class ItemInstance 
    {
        public int RandomID;
        public int ItemID;
        private static int prevId = 0;


        public Item.ItemInformation GetItemInfo()
        {
            return Item.GetItemById(ItemID);

        }

        public ItemInstance(int id,int randomId = -1)
        {
            prevId++;
            if (randomId == -1)
                RandomID = prevId;
            else
                RandomID = randomId;

            if (RandomID > prevId)
                prevId = RandomID + 1;

            ItemID = id; 

        }

    }
}
