using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game
{
    // Inventory
    class InventoryException : Exception { };
    class InventoryFullException : InventoryException { };
    class InventoryMaxStackException : InventoryException { };
}
