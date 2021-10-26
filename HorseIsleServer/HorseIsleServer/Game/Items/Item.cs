using HISP.Player;
using HISP.Server;
using HISP.Game;
using System.Collections.Generic;
using HISP.Game.Inventory;

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
                if (MiscFlags.Length <= no)
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

        private static List<ItemInformation> items = new List<ItemInformation>();
        private static List<ThrowableItem> throwableItems = new List<ThrowableItem>();
        public static void AddItemInfo(ItemInformation itm)
        {
            items.Add(itm);
        }
        public static void AddThrowableItem(ThrowableItem throwableItem)
        {
            throwableItems.Add(throwableItem);
        }
        public static ItemInformation[] Items
        {
            get
            {
                return items.ToArray();
            }
        }
        public static ThrowableItem[] ThrowableItems
        {
            get
            {
                return throwableItems.ToArray();
            }
        }


        public static int Present;
        public static int MailMessage;
        public static int DorothyShoes;
        public static int PawneerOrder;
        public static int Telescope;
        public static int Pitchfork;
        public static int WishingCoin;
        public static int ModSplatterball;
        public static int WaterBalloon;
        public static int FishingPole;
        public static int Earthworm;
        public static int BirthdayToken;
        public static int MagicBean;
        public static int MagicDroplet;

        public static int StallionTradingCard;
        public static int MareTradingCard;
        public static int ColtTradingCard;
        public static int FillyTradingCard;

        public static int[] TradingCards
        {
            get
            {
                return new int[4] { StallionTradingCard, MareTradingCard, ColtTradingCard, FillyTradingCard };
            }
        }

        public struct ItemPurchaseQueueItem
        {
            public int ItemId;
            public int ItemCount;
        }
        public static void UseItem(User user, ItemInstance item)
        {
            if (user.Inventory.HasItem(item.RandomId))
            {
                InventoryItem itm = user.Inventory.GetItemByRandomid(item.RandomId);
                if (itm.ItemId == Item.DorothyShoes)
                {
                    if (World.InIsle(user.X, user.Y))
                    {
                        World.Isle isle = World.GetIsle(user.X, user.Y);
                        if (isle.Name == "Prison Isle")
                        {
                            byte[] dontWorkHere = PacketBuilder.CreateChat(Messages.RanchDorothyShoesPrisonIsleMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            user.LoggedinClient.SendPacket(dontWorkHere);
                            return;
                        }
                    }

                    if (user.OwnedRanch == null) // How????
                    {
                        Logger.HackerPrint(user.Username + " Tried to use Dorothy Shoes when they did *NOT* own a ranch.");
                        user.Inventory.Remove(itm.ItemInstances[0]);
                        return;
                    }
                    byte[] noPlaceLIke127001 = PacketBuilder.CreateChat(Messages.RanchDorothyShoesMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    user.LoggedinClient.SendPacket(noPlaceLIke127001);

                    user.Teleport(user.OwnedRanch.X, user.OwnedRanch.Y);
                }
                else if (itm.ItemId == Item.Telescope)
                {
                    byte[] birdMap = PacketBuilder.CreateBirdMap(user.X, user.Y);
                    user.LoggedinClient.SendPacket(birdMap);
                }
                else
                {
                    Logger.ErrorPrint(user.Username + "Tried to use item with undefined action- ID: " + itm.ItemId);
                }
            }
        }
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
