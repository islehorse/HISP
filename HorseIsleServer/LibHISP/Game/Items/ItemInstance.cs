namespace HISP.Game.Items
{
    public class ItemInstance 
    {
        public int UniqueId;
        public int ItemId;
        public int Data;


        public Item.ItemInformation GetItemInfo()
        {
            return Item.GetItemById(ItemId);

        }

        public ItemInstance(int id,int uniqueId = -1, int data=0)
        {
            UniqueId = UniqueID.NextUniqueId(uniqueId);
            Data = data;
            ItemId = id; 

        }

    }
}
