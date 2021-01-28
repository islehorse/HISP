using HISP.Security;
namespace HISP.Game
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
