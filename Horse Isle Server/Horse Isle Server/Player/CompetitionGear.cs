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
        public const int MISC_FLAG_HEAD = 1;
        public const int MISC_FLAG_BODY = 2;
        public const int MISC_FLAG_LEGS = 3;
        public const int MISC_FLAG_FEET = 4;

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
                if (value == null)
                {
                    head = null;
                    Database.SetCompetitionGearHeadPeice(playerId, 0);
                    return;
                }
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
                if (value == null)
                {
                    body = null;
                    Database.SetCompetitionGearBodyPeice(playerId, 0);
                    return;
                }
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
                if (value == null)
                {
                    legs = null;
                    Database.SetCompetitionGearLegPeice(playerId, 0);
                    return;
                }
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
                if (value == null)
                {
                    feet = null;
                    Database.SetCompetitionGearFeetPeice(playerId, 0);
                    return;
                }
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
