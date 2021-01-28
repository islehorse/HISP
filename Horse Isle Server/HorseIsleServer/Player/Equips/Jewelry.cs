using HISP.Game;
using HISP.Server;

namespace HISP.Player.Equips
{
    class Jewelry
    {
        
        private int playerId;
        public Jewelry(int PlayerId)
        {
            playerId = PlayerId;
            if (!Database.HasJewelry(PlayerId))
                Database.InitJewelry(PlayerId);
            int itemId = Database.GetJewelrySlot1(PlayerId);
            if (itemId != 0)
                slot1 = Item.GetItemById(itemId);

            itemId = Database.GetJewelrySlot2(PlayerId);
            if (itemId != 0)
                slot2 = Item.GetItemById(itemId);

            itemId = Database.GetJewelrySlot3(PlayerId);
            if (itemId != 0)
                slot3 = Item.GetItemById(itemId);

            itemId = Database.GetJewelrySlot4(PlayerId);
            if (itemId != 0)
                slot4 = Item.GetItemById(itemId);

        }
        public Item.ItemInformation Slot1
        {
            get
            {
                return slot1;
            }
            set
            {
                if (value == null)
                {
                    slot1 = null;
                    Database.SetJewelrySlot1(playerId, 0);
                    return;
                }
                Database.SetJewelrySlot1(playerId, value.Id);
                slot1 = value;
            }
        }
        public Item.ItemInformation Slot2
        {
            get
            {
                return slot2;
            }
            set
            {
                if (value == null)
                {
                    slot2 = null;
                    Database.SetJewelrySlot2(playerId, 0);
                    return;
                }
                Database.SetJewelrySlot2(playerId, value.Id);
                slot2 = value;
            }
        }
        public Item.ItemInformation Slot3
        {
            get
            {
                return slot3;
            }
            set
            {
                if (value == null)
                {
                    slot3 = null;
                    Database.SetJewelrySlot3(playerId, 0);
                    return;
                }
                Database.SetJewelrySlot3(playerId, value.Id);
                slot3 = value;
            }
        }
        public Item.ItemInformation Slot4
        {
            get
            {
                return slot4;
            }
            set
            {
                if (value == null)
                {
                    slot4 = null;
                    Database.SetJewelrySlot4(playerId, 0);
                    return;
                }
                Database.SetJewelrySlot4(playerId, value.Id);
                slot4 = value;
            }
        }


        private Item.ItemInformation slot1;
        private Item.ItemInformation slot2;
        private Item.ItemInformation slot3;
        private Item.ItemInformation slot4;

    }
}
