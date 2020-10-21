using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Item
    {
        public struct Effects
        {
            public string EffectsWhat;
            public int EffectAmount;
        }

        public struct SpawnRules
        {
            public int SpawnCap;
            public string SpawnInArea;
            public string SpawnOnTileType;
            public string SpawnOnSpecialTile;
            public string SpawnNearSpecialTile;
        }
        public struct ItemInformation
        {
            public int Id;
            public string Name;
            public string PluralName;
            public string Description;

            public int IconId;
            public int SortBy;
            public int SellPrice;

            public string EmbedSwf;
            public bool WishingWell;
            public string Type;
            public int[] MiscFlags;
            public Effects[] Effects;

            public SpawnRules SpawnParamaters;

        }

        public static List<ItemInformation> Items = new List<ItemInformation>();

        public static ItemInformation GetItemById(int id)
        {
            foreach(ItemInformation item in Items)
            {
                if(item.Id == id)
                {
                    return item;
                }
            }
            throw new KeyNotFoundException("Item id " + id + " Not found!");
        }
    }
}
