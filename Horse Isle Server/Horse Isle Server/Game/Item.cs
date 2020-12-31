using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game
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
            public string SpawnInZone;
            public string SpawnOnTileType;
            public string SpawnOnSpecialTile;
            public string SpawnNearSpecialTile;
        }
        public class ItemInformation
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

        public struct ThrowableItem
        {
            public int Id;
            public string Message;
        }

        public static List<ItemInformation> Items = new List<ItemInformation>();
        public static List<ThrowableItem> ThrowableItems = new List<ThrowableItem>();

        public static int Present;
        public static int MailMessage;
        public static int DorothyShoes;
        public static int PawneerOrder;
        public static int Telescope;
        public static int Pitchfork;
        
        public static bool ConsumeItem(User user, ItemInformation itmInfo)
        {

            bool toMuch = false;
            foreach (Item.Effects effect in itmInfo.Effects)
            {
                switch (effect.EffectsWhat)
                {
                    case "TIREDNESS":
                        if (user.Tiredness + effect.EffectAmount > 1000)
                            toMuch = true;
                        user.Tiredness += effect.EffectAmount;
                        break;
                    case "THIRST":
                        if (user.Thirst + effect.EffectAmount > 1000)
                            toMuch = true;
                        user.Thirst += effect.EffectAmount;
                        break;
                    case "HUNGER":
                        if (user.Hunger + effect.EffectAmount > 1000)
                            toMuch = true;
                        user.Hunger += effect.EffectAmount;
                        break;
                    default:
                        Logger.ErrorPrint("Unknown effect: " + effect.EffectsWhat);
                        break;

                }
            }
            return toMuch;
        }
        public static bool IsThrowable(int id)
        {
            foreach(ThrowableItem itm in ThrowableItems)
            {
                if(itm.Id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public static ThrowableItem GetThrowableItem(int id)
        {
            foreach (ThrowableItem itm in ThrowableItems)
            {
                if (itm.Id == id)
                {
                    return itm;
                }
            }
            throw new KeyNotFoundException("id: " + id + " is not a throwable item.");
        }

        public static bool ItemIdExist(int id)
        {
            try
            {
                GetItemById(id);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }

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
