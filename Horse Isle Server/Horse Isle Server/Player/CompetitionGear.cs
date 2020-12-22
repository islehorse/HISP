using HISP.Game;
using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Player
{
    class CompetitionGear
    {
        private int playerId;
        public CompetitionGear(int PlayerId)
        {
            playerId = PlayerId;
            if (!Database.HasCompetitionGear(PlayerId))
                Database.InitCompetitionGear(PlayerId);
            int itemId = Database.GetCompetitionGearHeadPeice(PlayerId);
            if (itemId != 0)
                head = Item.GetItemById(itemId);

            itemId = Database.GetCompetitionGearBodyPeice(PlayerId);
            if (itemId != 0)
                body = Item.GetItemById(itemId);

            itemId = Database.GetCompetitionGearLegPeice(PlayerId);
            if (itemId != 0)
                legs = Item.GetItemById(itemId);

            itemId = Database.GetCompetitionGearFeetPeice(PlayerId);
            if (itemId != 0)
                feet = Item.GetItemById(itemId);

        }
        public Item.ItemInformation Head
        {
            get
            {
                return head;
            }
            set
            {
                Database.SetCompetitionGearHeadPeice(playerId, value.Id);
                head = value;
            }
        }
        public Item.ItemInformation Body
        {
            get
            {
                return body;
            }
            set
            {
                Database.SetCompetitionGearBodyPeice(playerId, value.Id);
                body = value;
            }
        }
        public Item.ItemInformation Legs
        {
            get
            {
                return legs;
            }
            set
            {
                Database.SetCompetitionGearLegPeice(playerId, value.Id);
                legs = value;
            }
        }
        public Item.ItemInformation Feet
        {
            get
            {
                return feet;
            }
            set
            {
                Database.SetCompetitionGearFeetPeice(playerId, value.Id);
                feet = value;
            }
        }


        private Item.ItemInformation head;
        private Item.ItemInformation body;
        private Item.ItemInformation legs;
        private Item.ItemInformation feet;

    }
}
