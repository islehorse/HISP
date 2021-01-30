using System;


namespace HISP.Game
{
    // Inventory
    public class InventoryException : Exception { };
    public class InventoryFullException : InventoryException { };
    public class InventoryMaxStackException : InventoryException { };
}
