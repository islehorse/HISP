using HISP.Player;
using HISP.Server;
using HISP.Game;
using System.Collections.Generic;

namespace HISP.Game.Items
{
    public class Item
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

            public int GetMiscFlag(int no)
            {
                if(MiscFlags.Length <= no)
                    return 0;
                else
                    return MiscFlags[no];
            }

        }

        public struct ThrowableItem
        {
            public int Id;
            public string HitMessage;
            public string ThrowMessage;
            public string HitYourselfMessage;
        }

        public static List<ItemInformation> Items = new List<ItemInformation>();
        public static List<ThrowableItem> ThrowableItems = new List<ThrowableItem>();

        public static int Present;
        public static int MailMessage;
        public static int DorothyShoes;
        public static int PawneerOrder;
        public static int Telescope;
        public static int Pitchfork;
        public static int WishingCoin;
        public static int ModSplatterball;
        public static int FishingPole;
        public static int Earthworm;
        public static int BirthdayToken;
        public static int MagicBean;
        public static int MagicDroplet;
        public static ItemInformation[] GetAllWishableItems()
        {
            List<ItemInformation> itemInfo = new List<ItemInformation>();
            foreach(ItemInformation item in Items)
            {
                if (item.WishingWell)
                    itemInfo.Add(item);
            }
            return itemInfo.ToArray();
        }

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
                    case "MOOD":
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
        public static void DoSpecialCases()
        {
            Tack.GenerateTackSets();
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
