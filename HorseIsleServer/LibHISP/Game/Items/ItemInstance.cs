using HISP.Security;
using HISP.Game;
namespace HISP.Game.Items
{
    public class ItemInstance 
    {
        public int RandomId;
        public int ItemId;
        public int Data;


        public Item.ItemInformation GetItemInfo()
        {
            return Item.GetItemById(ItemId);

        }

        public ItemInstance(int id,int randomId = -1, int data=0)
        {
            RandomId = RandomID.NextRandomId(randomId);
            Data = data;
            ItemId = id; 

        }

    }
}
