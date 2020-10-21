using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    interface IInventory
    {

        void Add(ItemInstance item);
        void Remove(ItemInstance item);

        int Count
        {
            get;
        }

        ItemInstance[] GetItemList();


    }
}
