using System;


namespace HISP.Game
{
    // Inventory
    class InventoryException : Exception { };
    class InventoryFullException : InventoryException { };
    class InventoryMaxStackException : InventoryException { };
}
