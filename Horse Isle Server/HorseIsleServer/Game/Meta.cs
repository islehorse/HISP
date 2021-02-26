using HISP.Game.Horse;
using HISP.Game.Inventory;
using HISP.Game.Services;
using HISP.Player;
using HISP.Server;
using HISP.Game.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace HISP.Game
{
    public class Meta
    {
        // Meta

        private static string buildLocationString(int x, int y)
        {
            string areaString = buildAreaString(x, y);
            if (areaString != "")
                areaString = Messages.LocationFormat.Replace("%META%", areaString);
            return areaString;
        }
        private static string buildAreaString(int x, int y)
        {
            string locationString = "";

            if (World.InArea(x, y))
                locationString += Messages.AreaFormat.Replace("%AREA%", World.GetArea(x, y).Name);
            if (World.InTown(x, y))
                locationString += Messages.TownFormat.Replace("%TOWN%", World.GetTown(x, y).Name);
            if (World.InIsle(x, y))
                locationString += Messages.IsleFormat.Replace("%ISLE%", World.GetIsle(x, y).Name);
            return locationString;
        }

        private static string buildPlayersHere(User fromUser, int x, int y)
        {
            string playersHere = "";
            User[] playersAt = GameServer.GetUsersAt(x, y, true, true);
            if(playersAt.Length > 1)
            {
                playersHere += Messages.PlayersHere;
                int count = 0;
                foreach(User playerAt in playersAt)
                {
                    if (playerAt.Id == fromUser.Id)
                        continue;
                    string buttons = "";
                    buttons += Messages.FormatPlayerHereProfileButton(playerAt.Id);
                    buttons += Messages.FormatPlayerHereSocialButtton(playerAt.Id);
                    buttons += Messages.FormatPlayerHereTradeButton(playerAt.Id);
                    if (fromUser.Friends.IsFriend(playerAt.Id))
                        buttons += Messages.FormatPlayerHereTagButton(playerAt.Id);
                    else
                        buttons += Messages.FormatPlayerHereBuddyButton(playerAt.Id);
                    buttons += Messages.FormatPlayerHerePMButton(playerAt.Username);

                    playersHere += Messages.FormatPlayerHereMenu(playerAt.GetPlayerListIcon(), playerAt.Username,buttons);
                    count++;
                }

                if(count >= 2)
                    playersHere += Messages.PlayerHereMulitpleMenuFormat;

                if (count <= 0)
                    return "";
            }

            return playersHere;
        }

        private static string buildNearbyString(int x, int y, bool showNearbyPlayersHeader=true)
        {
            string playersNearby = "";

            User[] nearbyUsers = GameServer.GetNearbyUsers(x, y, true, true);
            int count = 0;
            if (nearbyUsers.Length > 1)
            {
                if(showNearbyPlayersHeader)
                {
                    playersNearby += Messages.NearbyPlayers;
                }

                string usersWest = "";
                string usersNorth = "";
                string usersEast = "";
                string usersSouth = "";
                
                foreach (User nearbyUser in nearbyUsers)
                {
                    if (nearbyUser.X == x && nearbyUser.Y == y) // not yourself
                        continue;

                    int xDiff = x - nearbyUser.X;
                    int yDiff = y - nearbyUser.Y;
                    double angle =  (Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
                    angle -= 45;
                    angle = (angle % 360 + 360) % 360;

                    if (angle >= 270 && angle <= 360 )
                        usersWest += " " + nearbyUser.Username + " ";
                    else if (angle >= 90 && angle <= 180)
                        usersEast += " " + nearbyUser.Username + " ";
                    else if (angle >= 180 && angle <= 270)
                        usersSouth += " " + nearbyUser.Username + " ";
                    else if (angle >= 0 && angle <= 90)
                        usersNorth += " " + nearbyUser.Username + " ";


                    count++;
                }

                if (usersEast != "")
                    playersNearby += " " + Messages.East + usersEast + Messages.Seperator;
                if (usersWest != "")
                    playersNearby += " " + Messages.West + usersWest + Messages.Seperator;
                if (usersSouth != "")
                    playersNearby += " " + Messages.South + usersSouth + Messages.Seperator;
                if (usersNorth != "")
                    playersNearby += " " + Messages.North + usersNorth + Messages.Seperator;



            }
            if(count <= 0)
            {
                return "";
            }
            return playersNearby;

        }
        private static string buildShopInfo(Shop shop, IInventory shopperInventory)
        {
            string message = "";
            InventoryItem[] itemList = shop.Inventory.GetItemList();

            // Get shops stock
            message += Messages.ThingsIAmSelling;
            foreach (InventoryItem item in itemList)
            {
                message += "^R1";
                Item.ItemInformation itemInfo = Item.GetItemById(item.ItemId);

                int count = item.ItemInstances.Count;
                string countStr = count.ToString();
                if (item.Infinite)
                    countStr = Messages.InfinitySign;


                message += Messages.FormatShopEntry(itemInfo.IconId, countStr, itemInfo.Name, shop.CalculateBuyCost(itemInfo));

                message += Messages.FormatBuyItemButton(itemInfo.Id);
                if (count >= 5)
                    message += Messages.FormatBuy5ItemButton(itemInfo.Id);
                if (count >= 25)
                    message += Messages.FormatBuy25ItemButton(itemInfo.Id);

                message += Messages.FormatItemInformationByIdButton(itemInfo.Id);

            }

            // Check whats avalilble to be sold
            message += "^R1" + Messages.ThingsYouSellMe;
            InventoryItem[] shopperItemList = shopperInventory.GetItemList();

            foreach (InventoryItem shopperitem in shopperItemList)
            {
                Item.ItemInformation itemInfo = Item.GetItemById(shopperitem.ItemId);

                // Prevent items that cannot be sold to this shopkeeper.
                if (!shop.CanSell(itemInfo))
                    continue;


                int count = shopperitem.ItemInstances.Count;
                string countStr = count.ToString();


                message += "^R1";
                message += Messages.FormatShopEntry(itemInfo.IconId, countStr, itemInfo.Name, shop.CalculateSellCost(itemInfo));
                message += Messages.FormatSellButton(shopperitem.ItemInstances[0].RandomId);
                message += Messages.FormatSellAllButton(itemInfo.Id);
                message += Messages.FormatItemInformationButton(shopperitem.ItemInstances[0].RandomId);
            }

            message += "^R1" + Messages.ExitThisPlace;
            return message;
        }


        private static string buildWishingWell(User user)
        {
            string message = "";
            bool hasCoins = user.Inventory.HasItemId(Item.WishingCoin);
            if (!hasCoins)
            {
                message += Messages.NoWishingCoins;
            }
            else
            {
                InventoryItem wishingCoins = user.Inventory.GetItemByItemId(Item.WishingCoin);
                int totalCoins = wishingCoins.ItemInstances.Count;
                message += Messages.FormatNumberOfWishingCoins(totalCoins);
                message += Messages.WishingWellMeta;
            }

            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;


        }

        private static string buildCommonInfo(User user, int x, int y)
        {
            string message = "";
            message += buildPlayersHere(user, x, y);
            message += buildNearbyString(x, y);

            // Dropped Items
            DroppedItems.DroppedItem[] Items = DroppedItems.GetItemsAt(x, y);
            if (Items.Length == 0)
                message += Messages.NothingMessage;
            else
            {
                message += Messages.ItemsOnGroundMessage;
                foreach (DroppedItems.DroppedItem item in Items)
                {
                    Item.ItemInformation itemInfo = item.Instance.GetItemInfo();
                    message += Messages.FormatGrabItemMessage(itemInfo.Name, item.Instance.RandomId, itemInfo.IconId);
                }
                if (Items.Length > 1)
                    message += Messages.GrabAllItemsButton;
            }
            return message;
        }

        private static string buildWornJewelery(User user)
        {
            string message = Messages.JewelrySelected;
            if (user.EquipedJewelry.Slot1 != null)
                message += Messages.FormatJewelrySlot1(user.EquipedJewelry.Slot1.Name, user.EquipedJewelry.Slot1.IconId);
            if (user.EquipedJewelry.Slot2 != null)
                message += Messages.FormatJewelrySlot2(user.EquipedJewelry.Slot2.Name, user.EquipedJewelry.Slot2.IconId);
            if (user.EquipedJewelry.Slot3 != null)
                message += Messages.FormatJewelrySlot3(user.EquipedJewelry.Slot3.Name, user.EquipedJewelry.Slot3.IconId);
            if (user.EquipedJewelry.Slot4 != null)
                message += Messages.FormatJewelrySlot4(user.EquipedJewelry.Slot4.Name, user.EquipedJewelry.Slot4.IconId);

            if (message == Messages.JewelrySelected)
                message = Messages.NoJewerlyEquipped;

            return message;
        }

        private static string buildMultiroom(string id, User user)
        {

            string message = Messages.MultiroomPlayersParticipating;
            foreach (User userOnTile in GameServer.GetUsersOnSpecialTileCode("MULTIROOM-" + id))
            {
                if (userOnTile.Id == user.Id)
                    continue;
                message += Messages.FormatMultiroomParticipent(userOnTile.Username);
            }
            if (id[0] == 'P') // Poet
            {
                int lastPoet = Database.GetLastPlayer(id);
                string username = "";
                if (lastPoet != -1)
                    username = Database.GetUsername(lastPoet);

                message += Messages.FormatLastPoet(username);
            }
            if (id[0] == 'D') // Drawning room
            {
                int lastDraw = Database.GetLastPlayer(id);
                string username = "";
                if (lastDraw != -1)
                    username = Database.GetUsername(lastDraw);

                message += Messages.FormatLastToDraw(username);
            }

            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildEquippedCompetitionGear(User user)
        {
            string message = Messages.CompetitionGearSelected;
            if (user.EquipedCompetitionGear.Head != null)
                message += Messages.FormatCompetitionGearHead(user.EquipedCompetitionGear.Head.Name, user.EquipedCompetitionGear.Head.IconId);
            if (user.EquipedCompetitionGear.Body != null)
                message += Messages.FormatCompetitionGearBody(user.EquipedCompetitionGear.Body.Name, user.EquipedCompetitionGear.Body.IconId);
            if (user.EquipedCompetitionGear.Legs != null)
                message += Messages.FormatCompetitionGearLegs(user.EquipedCompetitionGear.Legs.Name, user.EquipedCompetitionGear.Legs.IconId);
            if (user.EquipedCompetitionGear.Feet != null)
                message += Messages.FormatCompetitionGearFeet(user.EquipedCompetitionGear.Feet.Name, user.EquipedCompetitionGear.Feet.IconId);

            if (message == Messages.CompetitionGearSelected)
                message = Messages.NoCompetitionGear;

            return message;

        }

        private static string buildHorseListIndependantlyOfUserInstance(int userId)
        {
            string message = "";
            int i = 1;


            foreach (HorseInfo.Category category in HorseInfo.HorseCategories)
            {
                HorseInstance[] horsesInCategory = Database.GetPlayerHorsesInCategory(userId, category.Name).OrderBy(o => o.Name).ToArray();
                if (horsesInCategory.Length > 0)
                {
                    message += category.MetaOthers;
                    foreach (HorseInstance instance in horsesInCategory)
                    {
                        message += Messages.FormatHorseEntry(i, instance.Name, instance.Breed.Name, instance.RandomId, instance.AutoSell > 0);
                        i++;
                    }
                }
            }

            return message;

        }
        public static string buildLibary()
        {
            return Messages.LibaryMainMenu + Messages.ExitThisPlace + Messages.MetaTerminator;
        }
        private static string buildNpc(User user, int x, int y)
        {
            string message = "";
            Npc.NpcEntry[] entries = Npc.GetNpcByXAndY(x, y);
            foreach (Npc.NpcEntry ent in entries)
            {
                if (ent.AdminOnly && !user.Administrator)
                    continue;

                if (ent.RequiresQuestIdCompleted != 0)
                    if (user.Quests.GetTrackedQuestAmount(ent.RequiresQuestIdCompleted) <= 0)
                        continue;

                if (ent.RequiresQuestIdNotCompleted != 0)
                    if (user.Quests.GetTrackedQuestAmount(ent.RequiresQuestIdNotCompleted) >= 1)
                        continue;

                message += Messages.FormatNpcStartChatMessage(ent.IconId, ent.Name, ent.ShortDescription, ent.Id);
                if(ent.Chatpoints.Length > 0)
                {
                    if (ent.LongDescription != "")
                        message += Messages.FormatNpcInformationButton(ent.Id);
                    message += Messages.FormatNpcTalkButton(ent.Id);
                }
                else
                {
                    message += Messages.NpcNoChatpoints;
                }
                message += "^R1";
            }
            return message;
        }
        public static string buildVenusFlyTrap(User user)
        {
            int moneyLost = GameServer.RandomNumberGenerator.Next(0, 100);
            if (moneyLost > user.Money)
                moneyLost = user.Money;
            user.Money -= moneyLost;
            return Messages.FormatVenusFlyTrapMeta(moneyLost);
        }
        public static string buildInn(Inn inn)
        {
            string message = Messages.InnBuyMeal;
            foreach (Item.ItemInformation item in inn.MealsOffered)
            {
                message += Messages.FormatInnItemEntry(item.IconId, item.Name, inn.CalculateBuyCost(item), item.Id);
            }
            message += Messages.InnBuyRest;
            foreach (Item.ItemInformation item in inn.RestsOffered)
            {
                message += Messages.FormatInnItemEntry(item.IconId, item.Name, inn.CalculateBuyCost(item), item.Id);
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string SelectPlayerStatFormat(int statValue)
        {
            int curValue = 1000;
            int devisibleBy = Convert.ToInt32(Math.Floor((decimal)curValue / Messages.StatPlayerFormats.Length));

            for (int i = 0; i < Messages.StatPlayerFormats.Length; i++)
            {
                curValue -= devisibleBy;
                if (statValue >= curValue)
                    return Messages.StatPlayerFormats[i];

            }
            throw new Exception("A mathematically impossible error occured. please check wether the laws of physics still apply.");
        }

        public static string BuildTradeAdd(Trade trade)
        {
            string message = Messages.FormatTradeWhatToOffer(trade.OtherTrade.Trader.Username);
            message += Messages.TradeOfferMoney;
            message += Messages.TradeOfferHorse;
            foreach(HorseInstance horse in trade.Trader.HorseInventory.HorseList.OrderBy(o => o.Name))
            {
                if (horse.Leaser > 0)
                    continue;

                bool tacked = (horse.Equipment.Saddle != null || horse.Equipment.SaddlePad != null || horse.Equipment.Bridle != null || horse.Equipment.Companion != null);
                message += Messages.FormatTradeOfferHorse(horse.Name, tacked, horse.RandomId);
            }

            if(trade.Trader.Inventory.Count >= trade.Trader.MaxItems)
            {
                message += Messages.TradeOfferItemOtherPlayerInvFull;
            }
            else
            {
                message += Messages.TradeOfferItem;
                foreach(InventoryItem item in trade.Trader.Inventory.GetItemList())
                {
                    Item.ItemInformation itemInfo = Item.GetItemById(item.ItemId);
                    message += Messages.FormatTradeOfferItem(itemInfo.IconId, itemInfo.Name, item.ItemInstances.Count, item.ItemId);
                }
            }

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;

        }

        public static string BuildTradeAddItem(int totalItems)
        {
            string message = "";
            message += Messages.FormatTradeOfferItemSubmenu(totalItems);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildTradeAddMoney(int curMoney)
        {
            string message = "";
            message += Messages.FormatTradeOfferMoneySubmenu(curMoney);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildTrade(Trade trade)
        {

            string message = "";
            message += Messages.FormatTradeWithPlayer(trade.OtherTrade.Trader.Username);

            if (trade.Stage == "DONE" && trade.OtherTrade.Stage == "DONE")
                message += Messages.TradeFinalReview;
            else if (trade.Stage == "DONE")
                message += Messages.TradeWaitingForOtherDone;
            else if (trade.OtherTrade.Stage == "DONE")
                message += Messages.TradeOtherPlayerIsDone;


            message += Messages.FormatTradeYourOffering(trade.OtherTrade.Trader.Username);
            if (trade.MoneyOffered == 0 && trade.ItemsOffered.Count == 0 && trade.HorsesOffered.Count == 0)
                message += Messages.TradeOfferingNothing;
            if (trade.MoneyOffered > 0)
                message += Messages.FormatTradeMoneyOffer(trade.MoneyOffered);
            if(trade.HorsesOffered.Count > 0)
                foreach(HorseInstance horse in trade.HorsesOffered)
                    message += Messages.FormatTradeHorseOffer(horse.Name, horse.RandomId);
            if(trade.ItemsOffered.Count > 0)
                foreach(ItemInstance[] item in trade.ItemsOffered)
                {
                    Item.ItemInformation itemInfo = item[0].GetItemInfo();
                    string name = itemInfo.Name;
                    if (item.Length > 1)
                        name = itemInfo.PluralName;

                    message += Messages.FormatTradeItemOffer(itemInfo.IconId, item.Length, name);
                }

            if(trade.Stage == "OPEN")
                message += Messages.TradeAddItems;

            message += Messages.FormatTradeOtherOffering(trade.OtherTrade.Trader.Username);
            if (trade.OtherTrade.MoneyOffered == 0 && trade.OtherTrade.ItemsOffered.Count == 0 && trade.OtherTrade.HorsesOffered.Count == 0)
                message += Messages.TradeOfferingNothing;
            if (trade.OtherTrade.MoneyOffered > 0)
                message += Messages.FormatTradeMoneyOffer(trade.OtherTrade.MoneyOffered);
            if (trade.OtherTrade.HorsesOffered.Count > 0)
                foreach (HorseInstance horse in trade.OtherTrade.HorsesOffered)
                    message += Messages.FormatTradeHorseOffer(horse.Name, horse.RandomId);
            if (trade.OtherTrade.ItemsOffered.Count > 0)
                foreach (ItemInstance[] item in trade.OtherTrade.ItemsOffered)
                {
                    Item.ItemInformation itemInfo = item[0].GetItemInfo();
                    string name = itemInfo.Name;
                    if (item.Length > 1)
                        name = itemInfo.PluralName;

                    message += Messages.FormatTradeItemOffer(itemInfo.IconId, item.Length, name);
                }

            if (trade.Stage == "OPEN")
                message += Messages.TradeWhenDoneClick;
            if (((trade.Stage == "DONE" || trade.Stage == "ACCEPTED") && (trade.OtherTrade.Stage == "DONE" || trade.Stage == "ACCEPTED")) )
                message += Messages.TradeAcceptTrade;

            message += Messages.TradeCancelAnytime;

            return message;
        }

        public static string buildTackPeiceLibary(Item.ItemInformation item)
        {
            string message = "";
            message += Messages.FormatTackSetPeice(item.Name, item.Description);
            return message;
        }

        public static string BuildTackLibary()
        {
            string message = "";

            foreach (Tack.TackSet set in Tack.TackSets.OrderBy(o => o.SortPosition()).ToArray())
            {
                string[] setSwfs = set.GetSwfNames();
                string swf = "breedviewer.swf?terrain=book2&breed=tackonly";
                if (setSwfs.Length >= 1)
                    swf += "&saddle=" + setSwfs[0];
                if (setSwfs.Length >= 2)
                    swf += "&saddlepad=" + setSwfs[1];
                if (setSwfs.Length >= 3)
                    swf += "&bridle=" + setSwfs[2];
                swf += "&j=";

                message += Messages.FormatTackSetView(set.IconId, set.SetName, swf);

                // Write all peices
                try
                {
                    message += buildTackPeiceLibary(set.GetSaddle());
                    message += buildTackPeiceLibary(set.GetSaddlePad());
                    message += buildTackPeiceLibary(set.GetBridle());
                }
                catch (Exception e)
                {
                    Logger.ErrorPrint(e.Message);
                }
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildMinigamesLibary()
        {
            string message = "";
            message += Messages.MinigameSingleplayer;
            foreach (World.SpecialTile tile in World.SpecialTiles.OrderBy(o => o.Title))
            {
                if (tile.TypeFlag == "1PLAYER")
                {
                    string mapXy = Messages.FormatMapLocation(tile.X, tile.Y);
                    message += Messages.FormatMinigameEntry(tile.Title, mapXy);
                }
            }
            message += Messages.MinigameTwoplayer;
            foreach (World.SpecialTile tile in World.SpecialTiles.OrderBy(o => o.Title))
            {
                if (tile.TypeFlag == "2PLAYER")
                {
                    string mapXy = Messages.FormatMapLocation(tile.X, tile.Y);
                    message += Messages.FormatMinigameEntry(tile.Title, mapXy);
                }
            }
            message += Messages.MinigameMultiplayer;
            foreach (World.SpecialTile tile in World.SpecialTiles.OrderBy(o => o.Title))
            {
                if (tile.TypeFlag == "MULTIPLAYER")
                {
                    string mapXy = Messages.FormatMapLocation(tile.X, tile.Y);
                    message += Messages.FormatMinigameEntry(tile.Title, mapXy);
                }
            }
            message += Messages.MinigameCompetitions;
            foreach (World.SpecialTile tile in World.SpecialTiles.OrderBy(o => o.Title))
            {
                if (tile.TypeFlag == "ARENA")
                {
                    string mapXy = Messages.FormatMapLocation(tile.X, tile.Y);
                    message += Messages.FormatMinigameEntry(tile.Title, mapXy);
                }
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildCompanionLibary()
        {
            string message = "";
            foreach (Item.ItemInformation itm in Item.Items.OrderBy(o => o.GetMiscFlag(0)).ToArray())
            {
                if (itm.Type == "COMPANION" && itm.EmbedSwf != null)
                {
                    string swf = "breedviewer.swf?terrain=book2&breed=tackonly&companion=" + itm.EmbedSwf + "&j=";
                    message += Messages.FormatCompanionViewButton(itm.IconId, itm.Name, swf);
                    message += Messages.FormatCompanionEntry(itm.Description);
                }
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildLocationsLibary()
        {
            string message = "";
            message += Messages.LocationKnownIslands;
            foreach (World.Waypoint waypoint in World.Waypoints.OrderBy(o => o.Name).ToArray())
            {
                if (waypoint.Type == "ISLE")
                {
                    string mapxy = Messages.FormatMapLocation(waypoint.PosX, waypoint.PosY);
                    message += Messages.FormatIslandLocation(waypoint.Name, mapxy);
                    message += Messages.FormatLocationDescription(waypoint.Description);
                }
            }
            message += Messages.LocationKnownTowns;
            foreach (World.Waypoint waypoint in World.Waypoints.OrderBy(o => o.Name).ToArray())
            {
                if (waypoint.Type == "TOWN")
                {
                    string mapxy = Messages.FormatMapLocation(waypoint.PosX, waypoint.PosY);
                    message += Messages.FormatTownLocation(waypoint.Name, mapxy);
                    message += Messages.FormatLocationDescription(waypoint.Description);
                }
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildRanchSearchResults(string search)
        {
            string message = "";
            message += Messages.LibaryFindRanchResultsHeader;
            int totalResults = 0;
            foreach(Ranch ranch in Ranch.Ranches)
            {
                if(ranch.OwnerId != -1)
                {
                    string ranchOwnerName = Database.GetUsername(ranch.OwnerId);
                    if(ranchOwnerName.ToLower().Contains(search.ToLower()))
                    {
                        message += Messages.FormatRanchSearchResult(ranchOwnerName, ranch.X, ranch.Y);
                        totalResults++;
                    }
                }
                if (totalResults >= 10)
                    break;
            }
            if (totalResults == 0)
                message += Messages.LibaryFindRanchResultsNoResults;

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildRanchSearchLibary()
        {
            string message = "";
            message += Messages.LibaryFindRanch;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildAwardsLibary()
        {
            string message = "";
            message += Messages.AwardsAvalible;
            foreach (Award.AwardEntry award in Award.GlobalAwardList.OrderBy(o => o.Sort).ToArray())
            {
                message += Messages.FormatAwardEntry(award.IconId, award.Title, award.MoneyBonus, award.Description);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildBookReadLibary(Book book)
        {
            string message = "";
            message = Messages.FormatBookReadMeta(book.Author, book.Title, book.Text);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildBooksLibary()
        {
            string message = "";
            message += Messages.BooksOfHorseIsle;
            foreach (Book libaryBook in Book.LibaryBooks.OrderBy(o => o.Title).ToArray())
            {
                message += Messages.FormatBookEntry(libaryBook.Title, libaryBook.Author, libaryBook.Id);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildHorseReleased()
        {
            string message = "";
            message += Messages.HorseReleasedMeta;
            message += Messages.BackToMapHorse;
            message += Messages.MetaTerminator;
            return message;

        }

        public static string BuildTopHighscores(string gameName)
        {
            Highscore.HighscoreTableEntry[] scores = Database.GetTopScores(gameName, 20);
            if (scores.Length <= 0)
                return "ERROR: No scores recorded.";
            string message = "";

            message += Messages.FormatHighscoreHeader(gameName);

            for (int i = 0; i < scores.Length; i++)
            {
                message += Messages.FormatHighscoreListEntry(i + 1, scores[i].Score, Database.GetUsername(scores[i].UserId), scores[i].TimesPlayed);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildTopTimes(string gameName)
        {
            Highscore.HighscoreTableEntry[] scores = Database.GetTopScores(gameName, 20);
            if (scores.Length <= 0)
                return "No times recorded.";
            string message = "";

            message += Messages.FormatBestTimeHeader(gameName);

            for (int i = 0; i < scores.Length; i++)
            {
                message += Messages.FormatBestTimeListEntry(i + 1, scores[i].Score, Database.GetUsername(scores[i].UserId), scores[i].TimesPlayed);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildMinigameRankingsForUser(User user)
        {
            string message = Messages.HighscoreHeaderMeta;
            foreach (Highscore.HighscoreTableEntry highscore in user.Highscores.HighscoreList)
            {
                if (highscore.Type == "SCORE")
                    message += Messages.FormatHighscoreStat(highscore.GameName, Database.GetRanking(highscore.Score, highscore.GameName), highscore.Score, highscore.TimesPlayed);
                else if (highscore.Type == "TIME")
                    message += Messages.FormatBestTimeStat(highscore.GameName, Database.GetRanking(highscore.Score, highscore.GameName), highscore.Score, highscore.TimesPlayed);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildPrivateNotes(User user)
        {
            string message = "";
            message += Messages.FormatPrivateNotes(user.PrivateNotes);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildNpcInfo(Npc.NpcEntry npcInfo)
        {
            string message = "";
            message += Messages.FormatNpcInformation(npcInfo.Name, npcInfo.LongDescription);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildItemInfo(Item.ItemInformation itemInfo)
        {
            string message = "";
            message += Messages.FormatItemInformation(itemInfo.Name, itemInfo.Description);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildRanchTraining(User user)
        {
            string message = "";
            message += Messages.RanchTrainAllAttempt;
            int horsesTrained = 0;
            foreach(HorseInstance horse in user.HorseInventory.HorseList)
            {
                if(horse.BasicStats.Mood < 200)
                {
                    message += Messages.FormatRanchTrainBadMood(horse.Name);
                }
                if(horse.TrainTimer == 0)
                {
                    horse.AdvancedStats.Speed += 1;
                    horse.AdvancedStats.Strength += 1;
                    horse.AdvancedStats.Conformation += 1;
                    horse.AdvancedStats.Agility += 1;
                    horse.AdvancedStats.Endurance += 1;
                    horse.BasicStats.Experience += 1;
                    horse.TrainTimer = 720;
                    horsesTrained++;
                    message += Messages.FormatRanchTrain(horse.Name, 1, 1, 1, 1, 1, 1);
                }
                else
                {
                    message += Messages.FormatRanchTrainFail(horse.Name, horse.TrainTimer);                                                                                                   
                }
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildTransportInfo(User user, Transport.TransportPoint transportPoint)
        {
            string message = "";
            // Build list of locations
            for (int i = 0; i < transportPoint.Locations.Length; i++)
            {
                int transportLocationId = transportPoint.Locations[i];
                Transport.TransportLocation transportLocation = Transport.GetTransportLocation(transportLocationId);
                string costFormat = Messages.FormatTransportCost(transportLocation.Cost);
                if(transportLocation.Type == "WAGON")
                {
                    if (user.OwnedRanch != null)
                    {
                        if (user.OwnedRanch.GetBuildingCount(7) > 0) // Wagon
                        {
                            costFormat = Messages.TransportWagonFree; 
                        }
                    }
                }


                message += Messages.FormatTransportMessage(transportLocation.Type, transportLocation.LocationTitle, costFormat, transportLocation.Id, transportLocation.GotoX, transportLocation.GotoY);
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildStatsMenu(User user)
        {
            string message = Messages.FormatStatsBar(user.Username);

            string areaString = buildAreaString(user.X, user.Y);
            if (areaString != "")
                message += Messages.FormatStatsArea(areaString);
            message += Messages.FormatMoneyStat(user.Money);
            if (!user.Subscribed)
                message += Messages.FormatFreeTime(user.FreeMinutes);
            message += Messages.FormatPlayerDescriptionForStatsMenu(user.ProfilePage);
            message += Messages.FormatExperience(user.Experience);
            message += Messages.FormatHungryStat(Messages.FormatPlayerStat(SelectPlayerStatFormat(user.Hunger), Messages.StatHunger));
            message += Messages.FormatThirstStat(Messages.FormatPlayerStat(SelectPlayerStatFormat(user.Thirst), Messages.StatThirst));
            message += Messages.FormatTiredStat(Messages.FormatPlayerStat(SelectPlayerStatFormat(user.Tiredness), Messages.StatTired));
            message += Messages.FormatGenderStat(user.Gender);
            message += Messages.FormatJewelryStat(buildWornJewelery(user));
            message += Messages.FormatCompetitionGearStat(buildEquippedCompetitionGear(user));
            message += Messages.StatsPrivateNotesButton;
            message += Messages.StatsQuestsButton;
            message += Messages.StatsMinigameRankingButton;
            message += Messages.StatsAwardsButton;
            message += Messages.StatsMiscButton;

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;

            return message;
        }

        public static string BuildWildHorseList(User user)
        {
            string message = "";
            WildHorse[] horses = WildHorse.GetHorsesAt(user.X, user.Y);
            if (horses.Length > 0)
            {
                message = Messages.HorsesHere;
                foreach (WildHorse horse in horses)
                {
                    bool vowel = (horse.Instance.Breed.Name[0].ToString().ToLower() == "a" || horse.Instance.Breed.Name[0].ToString().ToLower() == "i" || horse.Instance.Breed.Name[0].ToString().ToLower() == "u" || horse.Instance.Breed.Name[0].ToString().ToLower() == "e" || horse.Instance.Breed.Name[0].ToString().ToLower() == "o");
                    message += Messages.FormatWildHorse(horse.Instance.Name, horse.Instance.Breed.Name, horse.Instance.RandomId, vowel);
                }
            }
            return message;
        }
        public static string BuildAwardList(User user)
        {
            string message = Messages.AwardHeader;
            if (user.Awards.AwardsEarned.Length <= 0)
                message += Messages.NoAwards;
            else
                foreach (Award.AwardEntry award in user.Awards.AwardsEarned)
                    message += Messages.FormatAwardEntry(award.IconId, award.Title, award.MoneyBonus);




            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildQuestLog(User user)
        {
            string message = "";
            message += Messages.QuestLogHeader;
            Quest.QuestEntry[] questList = Quest.GetPublicQuestList();
            foreach (Quest.QuestEntry quest in questList)
            {
                string fmsg = "";
                if (user.Quests.GetTrackedQuestAmount(quest.Id) > 0)
                    fmsg = Messages.QuestCompleted;
                else
                    fmsg = Messages.QuestNotCompleted;

                foreach (int questId in quest.RequiresQuestIdCompleteStatsMenu)
                {
                    if (user.Quests.GetTrackedQuestAmount(questId) > 0)
                        continue;
                    fmsg = Messages.QuestNotAvalible;
                    break;
                }

                message += Messages.FormatQuestLogQuest(quest.Title, quest.QuestPointsEarned, quest.Difficulty, fmsg);
            }

            int totalComplete = Quest.GetTotalQuestsComplete(user);
            int totalQuestPoints = Quest.GetTotalQuestPoints();

            message += Messages.FormatQuestFooter(totalComplete, questList.Length, user.QuestPoints, totalQuestPoints);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;

            return message;
        }



        public static string BuildNearbyList(User user)
        {
            string message = "";
            message += Messages.NearbyPlayersListHeader;
            User[] nearbyUsers = GameServer.GetNearbyUsers(user.X, user.Y, false, true);
            foreach (User nearbyUser in nearbyUsers)
            {
                if (nearbyUser.Stealth)
                    continue;

                if (nearbyUser.Id == user.Id)
                    continue;


                int icon = nearbyUser.GetPlayerListIcon();
                string iconFormat = "";
                if (icon != -1)
                    iconFormat = Messages.FormatIconFormat(icon);

                message += Messages.FormatPlayerEntry(iconFormat, nearbyUser.Username, nearbyUser.Id, (DateTime.UtcNow - nearbyUser.LoginTime).Minutes, nearbyUser.X, nearbyUser.Y, nearbyUser.Idle);
            }

            message += Messages.PlayerListIconInformation;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;

            return message;
        }

        public static string BuildPlayerListAlphabetical()
        {
            string message = Messages.PlayerListAllAlphabeticalHeader;
            GameClient[] clients = GameServer.ConnectedClients;
            List<User> onlineUsers = new List<User>();

            foreach (GameClient client in clients)
            {
                if (client.LoggedIn)
                {
                    if (client.LoggedinUser.Stealth)
                        continue;
                    onlineUsers.Add(client.LoggedinUser);
                }
            }

            onlineUsers = onlineUsers.OrderBy(o => o.Username).ToList();

            foreach (User onlineUser in onlineUsers)
            {

                int icon = onlineUser.GetPlayerListIcon();
                string iconFormat = "";
                if (icon != -1)
                    iconFormat = Messages.FormatIconFormat(icon);

                message += Messages.FormatPlayerEntry(iconFormat, onlineUser.Username, onlineUser.Id, (DateTime.UtcNow - onlineUser.LoginTime).Minutes, onlineUser.X, onlineUser.Y, onlineUser.Idle);
            }

            message += Messages.PlayerListIconInformation;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;

            return message;
        }

        public static string BuildPlayerList()
        {
            string message = Messages.PlayerListAllHeader;
            GameClient[] clients = GameServer.ConnectedClients;
            foreach (GameClient client in clients)
            {
                if (client.LoggedIn)
                {
                    if (client.LoggedinUser.Stealth)
                        continue;

                    int icon = client.LoggedinUser.GetPlayerListIcon();
                    string iconFormat = "";
                    if (icon != -1)
                        iconFormat = Messages.FormatIconFormat(icon);

                    message += Messages.FormatPlayerEntry(iconFormat, client.LoggedinUser.Username, client.LoggedinUser.Id, (DateTime.UtcNow - client.LoggedinUser.LoginTime).Minutes, client.LoggedinUser.X, client.LoggedinUser.Y, client.LoggedinUser.Idle);
                }
            }

            message += Messages.PlayerListIconInformation;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;

            return message;
        }

        public static string BuildBuddyList(User user)
        {
            string message = Messages.BuddyListHeader;
            foreach (int id in user.Friends.List.ToArray())
            {
                try
                {
                    User friend = GameServer.GetUserById(id);
                    if (friend.Stealth)
                        continue;

                    int icon = friend.GetPlayerListIcon();
                    string iconFormat = "";
                    if (icon != -1)
                        iconFormat = Messages.FormatIconFormat(icon);

                    message += Messages.FormatOnlineBuddyEntry(iconFormat, friend.Username, friend.Id, (DateTime.UtcNow - friend.LoginTime).Minutes, friend.X, friend.Y);

                }
                catch (KeyNotFoundException) { }

            }
            message += Messages.BuddyListOfflineBuddys;

            foreach (int id in user.Friends.List.ToArray())
            {
                if (GameServer.IsUserOnline(id))
                    continue;

                message += Messages.BuddyListOfflineBuddys;
                string username = Database.GetUsername(id);
                int minutes = (DateTime.UtcNow - Converters.UnixTimeStampToDateTime(Database.GetPlayerLastLogin(id))).Minutes;

                message += Messages.FormatOfflineBuddyEntry(username, id, minutes);
            }

            message += Messages.PlayerListIconInformation;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;

            return message;
        }

        public static string BuildBreedViewerLibary(HorseInfo.Breed breed)
        {
            string message = Messages.FormatHorseBreedPreview(breed.Name, breed.Description);
            message += Messages.BreedViewerMaximumStats;
            message += Messages.FormatHorseAdvancedStat(breed.BaseStats.Speed * 2, 0, 0, breed.BaseStats.Speed * 2);
            message += Messages.FormatHorseAdvancedStat(breed.BaseStats.Strength * 2, 0, 0, breed.BaseStats.Strength * 2);
            message += Messages.FormatHorseAdvancedStat(breed.BaseStats.Conformation * 2, 0, 0, breed.BaseStats.Conformation * 2);
            message += Messages.FormatHorseAdvancedStat(breed.BaseStats.Agility * 2, 0, 0, breed.BaseStats.Agility * 2);
            message += Messages.FormatHorseAdvancedStat(breed.BaseStats.Endurance * 2, 0, 0, breed.BaseStats.Endurance * 2);
            message += Messages.FormatHorseAdvancedStat(breed.BaseStats.Inteligence * 2, 0, 0, breed.BaseStats.Inteligence * 2);
            message += Messages.FormatHorseAdvancedStat(breed.BaseStats.Personality * 2, 0, 0, breed.BaseStats.Personality * 2);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildHorseBreedListLibary()
        {
            string message = "";
            foreach (HorseInfo.Breed breed in HorseInfo.Breeds.OrderBy(o => o.Name).ToList())
            {
                if (breed.Swf == "")
                    continue;
                if (breed.Type == "horse")
                    message += Messages.FormatHorseBreed(breed.Name, breed.Id);
                else
                    message += Messages.FormatHorseRelative(breed.Name, breed.Id);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildNpcSearch(string search)
        {
            List<Npc.NpcEntry> foundNpcs = new List<Npc.NpcEntry>();
            foreach (Npc.NpcEntry npc in Npc.NpcList)
            {
                if (npc.Name.ToLower().Contains(search.ToLower()))
                {
                    if (npc.LibarySearchable)
                    {
                        if (foundNpcs.Count >= 5)
                            break;
                        foundNpcs.Add(npc);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
            string message = Messages.LibaryFindNpcSearchNoResults;
            if (foundNpcs.Count >= 1)
            {
                message = Messages.LibaryFindNpcSearchResultsHeader;
                foreach (Npc.NpcEntry npc in foundNpcs)
                {
                    string searchResult = Messages.FormatNpcSearchResult(npc.Name, npc.ShortDescription, npc.X, npc.Y);
                    Logger.DebugPrint(searchResult);
                    message += searchResult;
                }
            }
            if (foundNpcs.Count >= 5)
            {
                message += Messages.LibaryFindNpcLimit5;
            }

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;

        }

        public static string BuildFindNpcMenu()
        {
            string message = Messages.LibaryFindNpc;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildWhisperSearchResults(WildHorse[] results)
        {
            string message = Messages.WhispererSearchingAmoungHorses;
            if (results.Length <= 0)
            {
                message = Messages.WhispererNoneFound;
            }
            else
            {
                List<Point> locations = new List<Point>();
                foreach (WildHorse result in results)
                {
                    Point location = new Point();
                    location.X = result.X;
                    location.Y = result.Y;
                    locations.Add(location);
                }
                string mapxys = Messages.FormatMapLocations(locations.ToArray());
                message += Messages.FormatWhispererHorseFoundMeta(mapxys);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        private static string buildFountain()
        {
            return Messages.FountainMeta;
        }
        private static string buildPond(User user)
        {
            string message = Messages.PondHeader;
            if (!user.Inventory.HasItemId(Item.FishingPole))
            {
                message += Messages.PondNoFishingPole;
            }
            else if (!user.Inventory.HasItemId(Item.Earthworm))
            {
                message += Messages.PondNoEarthWorms;
            }
            else
            {
                message += Messages.PondGoFishing;
            }
            message += Messages.PondDrinkHereIfSafe;
            foreach (HorseInstance horse in user.HorseInventory.HorseList)
            {
                message += Messages.FormatPondDrinkHorseFormat(horse.Name, horse.BasicStats.Thirst, 1000, horse.RandomId);
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildMudHole(User user)
        {
            string message = "";
            if (user.HorseInventory.HorseList.Length > 0)
            {
                int rngHorseIndex = GameServer.RandomNumberGenerator.Next(0, user.HorseInventory.HorseList.Length);
                HorseInstance horse = user.HorseInventory.HorseList[rngHorseIndex];
                horse.BasicStats.Groom = 0;
                message += Messages.FormatMudHoleGroomDestroyed(horse.Name);
            }
            else
            {
                message += Messages.MudHoleNoHorses;
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildPassword()
        {
            return Messages.PasswordEntry + Messages.ExitThisPlace + Messages.MetaTerminator;
        }

        private static string buildHorseWhisperer()
        {
            string message = "";
            foreach (HorseInfo.Breed breed in HorseInfo.Breeds.OrderBy(o => o.Name).ToList())
            {
                if (breed.Swf == "")
                    continue;
                if (breed.SpawnInArea == "none")
                    continue;
                message += Messages.FormatWhispererHorseBreedButton(breed.Name, breed.Id);
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }

        private static string buildBank(User user)
        {
            double moneyMade = 0;
            if (user.BankInterest > user.BankMoney)
            {
                moneyMade = user.BankInterest - user.BankMoney;
                if (moneyMade > 100000000)
                    moneyMade = 100000000;
                user.BankMoney += moneyMade;

            }

            string message = "";
            moneyMade = Math.Floor(moneyMade);
            if (moneyMade > 0)
                message += Messages.FormatBankIntrestMadeMeta(Convert.ToUInt64(moneyMade));

            message += Messages.FormatBankCarryingMeta(user.Money, Convert.ToUInt64(Math.Floor(user.BankMoney)));
            message += Messages.BankWhatToDo;
            message += Messages.FormatBankOptionsMeta(user.Money, Convert.ToUInt64(Math.Floor(user.BankMoney)));
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;


            return message;
        }
        public static string BuildMinigamePlayers()
        {
            string message = Messages.CityHallTop25MinigamePlayers;
            int placing = 1;
            foreach (int userId in Database.GetMinigamePlayers())
            {
                string username = Database.GetUsername(userId);
                int totalMinigames = Database.GetPlayerTotalMinigamesPlayed(userId);

                message += Messages.FormatCityHallTopMinigamePlayers(placing, totalMinigames, username);
                placing++;
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildMostExperienedHoses()
        {
            string message = Messages.CityHallTop25ExperiencedHorses;

            int placing = 1;
            foreach (HorseInstance horse in Database.GetMostExperiencedHorses())
            {
                message += Messages.FormatCityHallTopExperiencedHorses(placing, horse.BasicStats.Experience, Database.GetUsername(horse.Owner), horse.Name);
                placing++;
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildExperiencedPlayers()
        {
            string message = Messages.CityHallTop25ExperiencedPlayers;
            int placing = 1;
            foreach (int userId in Database.GetAdventurousPlayers())
            {
                string username = Database.GetUsername(userId);
                int exp = Database.GetExperience(userId);

                message += Messages.FormatCityHallTopExperiencedPlayersEntry(placing, exp, username);
                placing++;
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildAdventurousPlayers()
        {
            string message = Messages.CityHallTop25AdventurousPlayers;
            int placing = 1;
            foreach (int userId in Database.GetAdventurousPlayers())
            {
                string username = Database.GetUsername(userId);
                int questPoints = Database.GetPlayerQuestPoints(userId);

                message += Messages.FormatCityHallTopAdventurousPlayersEntry(placing, questPoints, username);
                placing++;
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildMostSpoiledHorses()
        {
            string message = Messages.CityHallTop100SpoiledHorses;
            foreach(HorseInstance horse in Database.GetMostSpoiledHorses())
            {
                message += Messages.FormatCityHallTopSpoiledHorseEntry(horse.Spoiled, Database.GetUsername(horse.Owner), horse.Name);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildRichestPlayers()
        {
            string message = Messages.CityHallTop25Players;
            int placing = 1;
            foreach(int userId in Database.GetRichestPlayers())
            {
                string username = Database.GetUsername(userId);
                double totalMoney = Math.Floor(Database.GetPlayerMoney(userId) + Database.GetPlayerBankMoney(userId));

                message += Messages.FormatCityHallTopPlayerEntry(placing, totalMoney, username);
                placing++;
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildHorseGame(User user, string swf)
        {
            string message = Messages.HorseGamesSelectHorse;
            int placing = 1;
            foreach(HorseInstance horse in user.HorseInventory.HorseList.OrderBy(o => o.Name).ToArray())
            {
                if (horse.Leaser > 0)
                    continue;

                message += Messages.FormatHorseGamesEntry(placing, horse.Name, swf + ".swf?ID=" + horse.RandomId + "&SP=" + horse.AdvancedStats.Speed + "&ST=" + horse.AdvancedStats.Strength + "&CO=" + horse.AdvancedStats.Conformation + "&AG=" + horse.AdvancedStats.Agility + "&EN=" + horse.AdvancedStats.Endurance + "&IN=" + horse.AdvancedStats.Inteligence + "&PE=" + horse.AdvancedStats.Personality + "&");
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildMostValuedRanches()
        {
            string message = Messages.CityHallTop25Ranches;
            int total = 1;

            foreach(Ranch ranch in Ranch.Ranches.OrderByDescending(o => o.InvestedMoney).ToList())
            {
                if (ranch.OwnerId == -1)
                    continue;


                message += Messages.FormatCityHallTopRanchEntry(total, Database.GetUsername(ranch.OwnerId), ranch.InvestedMoney, Messages.FormatMapLocation(ranch.X, ranch.Y));

                if (total > 26)
                    break;

                total++;
            }

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildTopAutoSellHorses()
        {
            string message = Messages.CityHallCheapestAutoSells;
            HorseInstance[] horses = Database.GetCheapestHorseAutoSell();
            foreach(HorseInstance horse in horses)
            {
                if(horse.AutoSell > 0)
                    message += Messages.FormatCityHallCheapAutoSellEntry(horse.AutoSell, Database.GetUsername(horse.Owner), horse.Name, horse.Color, horse.Breed.Name, horse.BasicStats.Experience);
            }
            message += Messages.CityHallMostExpAutoSells;
            horses = Database.GetBiggestExpAutoSell();
            foreach (HorseInstance horse in horses)
            {
                if(horse.AutoSell > 0)
                    message += Messages.FormatCityHallBestExpAutoSellEntry(horse.BasicStats.Experience, Database.GetUsername(horse.Owner), horse.Name, horse.AutoSell, horse.Color, horse.Breed.Name);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildRanchEdit(Ranch ranch)
        {
            return Messages.FormatRanchEditDescriptonMeta(ranch.Title, ranch.Description) + Messages.BackToMap + Messages.MetaTerminator;
        }
        public static string BuildRanchSellConfirmation()
        {
            return Messages.RanchSellAreYouSure + Messages.BackToMap + Messages.MetaTerminator;
        }
        public static string BuildRanchUpgrade(Ranch ranch)
        {
            string message = "";
            Ranch.RanchUpgrade currentUpgrade = ranch.GetRanchUpgrade();
            bool canUpgrade = Ranch.RanchUpgrade.RanchUpgradeExists(currentUpgrade.Id + 1);
            
            string upgrade = "";
            if(canUpgrade)
            {
                Ranch.RanchUpgrade nextUpgrade = Ranch.RanchUpgrade.GetRanchUpgradeById(currentUpgrade.Id + 1);
                upgrade = Messages.FormatNextUpgrade(nextUpgrade.Title, nextUpgrade.Cost);
            }
            message += Messages.FormatCurrentUpgrade(currentUpgrade.Title, currentUpgrade.Description, upgrade, ranch.GetSellPrice());
            message += Messages.BackToMap;
            return message;
        }
        public static string BuildRanchBuildingsAvalible(Ranch ranch, int slot)
        {
            string message = "";
            if(ranch.GetBuilding(slot-1) != null)
            {
                Ranch.RanchBuilding building = ranch.GetBuilding(slot - 1);
                message += Messages.FormatBuildingAlreadyPlaced(building.Title, building.Id, building.GetTeardownPrice());
            }
            else
            {
                message += Messages.RanchCanBuildOneOfTheFollowingInThisSpot;
                foreach (Ranch.RanchBuilding building in Ranch.RanchBuilding.RanchBuildings)
                {
                    message += Messages.FormatBuildingEntry(building.Title, building.Cost, building.Id);
                }
            }

            message += Messages.BackToMap;
            return message;
        }
        public static string BuildRanchBuilding(Ranch ranch, Ranch.RanchUpgrade upgrade)
        {
            string message = "";
            message += Messages.FormatViewBuilding(upgrade.Title, upgrade.Description);

            message += Messages.BackToMap;
            return message;
        }
        public static string BuildRanchBuilding(Ranch ranch, Ranch.RanchBuilding building)
        {
            string message = "";
            message += Messages.FormatViewBuilding(building.Title, building.Description);
            if(building.Id == 1 || building.Id == 10 || building.Id == 11) // Barn, Big Barn, Gold Barn
            {
                int ranchOwner = ranch.OwnerId; // TOCTOU BAD

                if (ranchOwner != -1)
                {
                    string horsesList = buildHorseListIndependantlyOfUserInstance(ranchOwner); 
                    message += Messages.FormatBarn(horsesList);
                }
            }
            message += Messages.BackToMap;
            return message;
        }
        private static string buildRanch(User user, int ranchId)
        {
            
            string message = "";

            if(!Ranch.RanchExists(ranchId)) // Ghost ranchs exist.. apparently?
            {
                user.MetaPriority = false;
                return BuildMetaInfo(user, user.X, user.Y);
            }

            Ranch ranch = Ranch.GetRanchById(ranchId);
            bool mine = (ranch.OwnerId == user.Id);
            string swfModule = ranch.GetSwf(mine);

            byte[] moduleSwf = PacketBuilder.CreateSwfModulePacket(swfModule, PacketBuilder.PACKET_SWF_MODULE_FORCE);
            user.LoggedinClient.SendPacket(moduleSwf);

            if (mine) // This is My DS.
            {
                user.DoRanchActions();

                string title = ranch.Title;
                if (title == "" || title == null)
                    title = Messages.RanchDefaultRanchTitle;
                message += Messages.FormatRanchTitle(Database.GetUsername(ranch.OwnerId), title);
                message += Messages.BuildingRestHere;

                int numbBarns = ranch.GetBuildingCount(1);
                int numbWaterWell = ranch.GetBuildingCount(2);
                int numbGrainSilo = ranch.GetBuildingCount(3);
                int numbTrainingPen = ranch.GetBuildingCount(6);
                int numbWagon = ranch.GetBuildingCount(7);
                int numbWindmill = ranch.GetBuildingCount(8);
                int numbGarden = ranch.GetBuildingCount(9);
                int numbBigBarn = ranch.GetBuildingCount(10);
                int numbGoldBarn = ranch.GetBuildingCount(11);
                

                if (numbBarns > 0)
                    message += Messages.FormatBuildingBarn(numbBarns, numbBarns * 4);
                if (numbBigBarn > 0)
                    message += Messages.FormatBuildingBarn(numbBigBarn, numbBigBarn * 8);
                if (numbGoldBarn > 0)
                    message += Messages.FormatBuildingBarn(numbGoldBarn, numbGoldBarn * 12);
                if (numbBarns > 0 || numbBigBarn > 0 || numbGoldBarn > 0)
                    message += Messages.RanchHorsesFullyRested;
                if (numbWaterWell > 0)
                    message += Messages.BuildingWaterWell;
                if (numbGrainSilo > 0)
                    message += Messages.BuildingGrainSilo;
                if (numbWindmill > 0)
                    message += Messages.FormatBuildingWindmill(numbWindmill, 5000 * numbWindmill);
                if (numbGarden > 0)
                    message += Messages.BuildingVegatableGarden;
                if (numbWagon > 0)
                    message += Messages.BuildingWagon;
                if (numbTrainingPen > 0)
                    message += Messages.BuildingTrainingPen;

                message += Messages.FormatRanchYoursDescription(ranch.Description);
            }
            else if(ranch.OwnerId == -1) // No mans sky
            {

                message += Messages.FormatRanchUnownedMeta(ranch.Value);
                if (user.OwnedRanch == null)
                {
                    if (user.Subscribed)
                        message += Messages.RanchYouCouldPurchaseThisRanch;
                    else
                        message += Messages.RanchSubscribersOnly;
                }
                else
                {
                    message += Messages.RanchYouAllreadyOwnARanch;
                }
            }
            else
            {
                string title = ranch.Title;
                if (title == "" || title == null)
                    title = Messages.RanchDefaultRanchTitle;

                message += Messages.FormatRanchTitle(Database.GetUsername(ranch.OwnerId), title);
                message += Messages.FormatRanchDescOthers(ranch.Description);
            }

            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;

            return message;
        }
        public static string BuildAuctionHorseList(User user)
        {
            string message = Messages.AuctionListHorse;
            foreach(HorseInstance horse in user.HorseInventory.HorseList)
            {
                if (horse.Leaser > 0)
                    continue;
                if (horse.Category != "TRADING")
                    continue;

                bool tacked = (horse.Equipment.Saddle != null || horse.Equipment.SaddlePad != null || horse.Equipment.Bridle != null || horse.Equipment.Companion != null);

                message += Messages.FormatAuctionHorseListEntry(horse.Name, tacked, horse.RandomId);
            }

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildAuction(User user, Auction auction)
        {
            string message = "";
            message += Messages.AuctionsRunning;
            foreach(Auction.AuctionEntry entry in auction.AuctionEntries.ToArray())
            {
                message += Messages.FormatAuctionHorseEntry(Database.GetUsername(entry.OwnerId), entry.Horse.Color, entry.Horse.Breed.Name, entry.Horse.Gender, entry.Horse.BasicStats.Experience, entry.Completed ? "" : Messages.FormatAuctionViewHorseButton(entry.Horse.RandomId));
                if (!entry.Completed)
                    message += Messages.FormatAuctionGoingTo(entry.TimeRemaining, Database.GetUsername(entry.HighestBidder), entry.HighestBid, entry.RandomId);
                else
                {
                    if (entry.HighestBidder == entry.OwnerId)
                        message += Messages.AuctionNotSold;
                    else
                        message += Messages.FormatAuctionSoldTo(Database.GetUsername(entry.HighestBidder), entry.HighestBid);
                }
            }
            User[] users = GameServer.GetUsersAt(user.X, user.Y, true, true);
            List<string> usernames = new List<string>();
            foreach(User userInst in users)
            {
                usernames.Add(userInst.Username);
            }
            message += Messages.FormatAuctionPlayersHere(string.Join(", ", usernames.ToArray()));
            message += Messages.AuctionAHorse;

            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildWorkshop(User user)
        {
            Workshop shop = Workshop.GetWorkshopAt(user.X, user.Y);
            string message = "";
            foreach (Workshop.CraftableItem craft in shop.CraftableItems)
            {
                Item.ItemInformation itmInfo = Item.GetItemById(craft.GiveItemId);
                message += Messages.FormatWorkshopCraftEntry(itmInfo.IconId, itmInfo.Name, craft.MoneyCost, itmInfo.Id, craft.Id);
                // Get requirements
                List<string> Requirements = new List<string>();
                foreach (Workshop.RequiredItem reqItem in craft.RequiredItems)
                {

                    Item.ItemInformation requiredItemInfo = Item.GetItemById(reqItem.RequiredItemId);
                    string requirementTxt;
                    if (reqItem.RequiredItemCount <= 1)
                        requirementTxt = Messages.FormatWorkshopRequireEntry(reqItem.RequiredItemCount, requiredItemInfo.Name);
                    else
                        requirementTxt = Messages.FormatWorkshopRequireEntry(reqItem.RequiredItemCount, requiredItemInfo.PluralName);
                    Requirements.Add(requirementTxt);
                }
                message += Messages.FormatWorkshopRequirements(string.Join(Messages.WorkshopAnd, Requirements.ToArray()));
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildGroomer(User User, Groomer groomer)
        {
            string message = "";
            int totalPrice = 0;
            int count = 0;
            foreach(HorseInstance horse in User.HorseInventory.HorseList)
            {
                message += Messages.FormatHorseGroomCurrentlyAt(horse.Name, horse.BasicStats.Groom, 1000);
                if(horse.BasicStats.Groom < groomer.Max)
                {
                    int price = groomer.CalculatePrice(horse.BasicStats.Groom);
                    totalPrice += price;
                    count++;
                    message += Messages.FormatGroomerApplyService(price, horse.RandomId);
                }
            }
            message += Messages.FormatGroomerApplyAllService(count, totalPrice);
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildVet(User user, Vet vet)
        {
            string message = "";
            int totalPrice = 0;


            foreach (HorseInstance horse in user.HorseInventory.HorseList)
            {
                message += Messages.FormatVetServiceHorseMeta(horse.Name, horse.BasicStats.Health, 1000);

                if (horse.BasicStats.Health >= 1000)
                    message += Messages.VetSerivcesNotNeeded;
                else
                {
                    int price = vet.CalculatePrice(horse.BasicStats.Health);
                    totalPrice += price;

                    message += Messages.FormatVetApplyServiceMeta(price, horse.RandomId);
                }

            }
            if (user.HorseInventory.HorseList.Length > 0)
                message += Messages.FormatVetApplyAllServiceMeta(totalPrice);
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildBarn(User user, Barn barn)
        {
            string message = "";
            int totalPrice = 0;
            foreach (HorseInstance horse in user.HorseInventory.HorseList)
            {
                message += Messages.FormatBarnHorseStatus(horse.Name, horse.BasicStats.Tiredness, horse.BasicStats.Hunger, horse.BasicStats.Thirst);

                int price = barn.CalculatePrice(horse.BasicStats.Tiredness, horse.BasicStats.Hunger, horse.BasicStats.Thirst);
                if(price > 0)
                {
                    totalPrice += price;
                    message += Messages.FormatBarnLetHorseRelax(price, horse.RandomId);
                }
                else
                {
                    message += Messages.BarnHorseMaxed;
                }

            }
            message += Messages.FormatBarnLetAllHorsesReleax(totalPrice);
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildFarrier(User user, Farrier farrier)
        {
            string message = "";
            int totalPrice = 0;
            int maxShoes = 1000;
            foreach (HorseInstance horse in user.HorseInventory.HorseList)
            {
                message += Messages.FormatFarrierCurrentShoes(horse.Name, horse.BasicStats.Shoes, maxShoes);
                if (horse.BasicStats.Shoes < farrier.IronShoesAmount)
                    message += Messages.FormatFarrierApplyIron(farrier.IronCost, farrier.IronShoesAmount, horse.RandomId);

                if (horse.BasicStats.Shoes < farrier.SteelShoesAmount)
                {
                    totalPrice += farrier.SteelCost;
                    message += Messages.FormatFarrierApplySteel(farrier.SteelCost, farrier.SteelShoesAmount, horse.RandomId);
                }

            }
            if(totalPrice > 0)
                message += Messages.FormatFarrierApplySteelToAll(totalPrice, farrier.SteelShoesAmount);
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildRiddlerRiddle(User user)
        {
            Riddler riddle = Riddler.GetRandomRiddle(user);
            user.LastRiddle = riddle;
            string message = "";
            message += Messages.FormatRiddlerRiddle(riddle.Riddle);
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }

       
        public static string BuildHorseEscapedMessage()
        {
            string message = Messages.HorseEvadedCapture;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildHorseEscapedAnyway()
        {
            string message = Messages.HorseEscapedAnyway;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildHorseCaughtMessage()
        {
            string message = Messages.YouCapturedTheHorse;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildAbuseReportPage()
        {
            string reportReasons = "";
            foreach(AbuseReport.ReportReason reason in AbuseReport.ReportReasons)
            {
                reportReasons += Messages.FormatAbuseReportReason(reason.Id, reason.Name);
            }
            return Messages.FormatAbuseReportMetaPage(reportReasons);
        }
        private static string buildHorseList(User user, bool youView=true)
        {
            string message = "";
            int i = 1;
            foreach (HorseInfo.Category category in HorseInfo.HorseCategories)
            {
                HorseInstance[] horsesInCategory = user.HorseInventory.GetHorsesInCategory(category).OrderBy(o => o.Name).ToArray(); 
                if (horsesInCategory.Length > 0)
                {
                    if (youView)
                        message += category.Meta;
                    else
                        message += category.MetaOthers;
                    foreach (HorseInstance instance in horsesInCategory)
                    {
                        message += Messages.FormatHorseEntry(i, instance.Name, instance.Breed.Name, instance.RandomId, instance.AutoSell > 0);
                        i++;
                    }
                }
            }
            return message;

        }
        public static string BuildHorseInventory(User user)
        {
            string message = Messages.FormatHorseHeader(user.MaxHorses, user.HorseInventory.HorseList.Length);

            message += buildHorseList(user);
            message += Messages.ViewBaiscStats;
            message += Messages.ViewAdvancedStats;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildPlayerList(User user)
        {
            string message = "";
            message += Messages.PlayerListHeader;
            message += Messages.PlayerListSelectFromFollowing;
            message += Messages.FormatPlayerBuddyList(user.Friends.Count);
            message += Messages.PlayerListOfNearby;
            message += Messages.FormatPlayerList(GameServer.GetNumberOfPlayers() - 1);
            message += Messages.PlayerListOfPlayersAlphabetically;

            message += Messages.FormatMapAllBuddiesList(Messages.FormatMapLocations(GameServer.GetAllBuddyLocations(user)));
            message += Messages.FormatMapAllPlayersList(Messages.FormatMapLocations(GameServer.GetAllPlayerLocations(user)));

            message += Messages.PlayerListAbuseReport;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;

            return message;
        }
        public static string BuildMailLetter(Mailbox.Mail mailMessage, int itemRandomId)
        {
            DateTime time = Converters.UnixTimeStampToDateTime(mailMessage.Timestamp);
            string amOrPm = "am";
            string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            string minutes = time.Minute.ToString();
            if (minutes.Length <= 1)
                minutes = "0" + minutes;
            int hours = time.Hour;
            if (hours == 0)
            {
                amOrPm = "am";
                hours = 12;
            }
            if (hours > 12)
            {
                hours -= 12;
                amOrPm = "pm";
            }

           
            string date = months[time.Month-1] + " " + time.Day + ", " + time.Year + " " + hours + ":" + minutes + amOrPm;
            string message = Messages.FormatMailReadMessage(Database.GetUsername(mailMessage.FromUser), date, mailMessage.Subject, mailMessage.Message, itemRandomId);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildMailList(User user, InventoryItem mailMessageForPlayer)
        {
            string message = Messages.MailSelectFromFollowing;
            foreach(ItemInstance inst in mailMessageForPlayer.ItemInstances)
            {
                Mailbox.Mail mail = user.MailBox.GetMessageByRandomId(inst.Data);
                message += Messages.FormatMailEntry(mail.Subject, Database.GetUsername(mail.FromUser), inst.RandomId);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildInventoryInfo(PlayerInventory inv)
        {
            string message = "";
            message += Messages.FormatPlayerInventoryHeaderMeta(inv.Count, inv.BaseUser.MaxItems);
            InventoryItem[] items = inv.GetItemList();
            foreach(InventoryItem item in items)
            {
                Item.ItemInformation itemInfo = Item.GetItemById(item.ItemId);
                string title = itemInfo.Name;
                if (item.ItemInstances.Count > 1 && itemInfo.PluralName != "")
                    title = itemInfo.PluralName;


                message += Messages.FormatPlayerInventoryItemMeta(itemInfo.IconId, item.ItemInstances.Count, title);

                int randomId = item.ItemInstances[0].RandomId;
                if (itemInfo.Type != "QUEST" && itemInfo.Type != "TEXT" &&  !(itemInfo.Id == Item.DorothyShoes || itemInfo.Id == Item.Telescope) && World.CanDropItems(inv.BaseUser.X, inv.BaseUser.Y))
                    message += Messages.FormatItemDropButton(randomId);

                if (itemInfo.Id == Item.DorothyShoes || itemInfo.Id == Item.Telescope)
                    message += Messages.FormatItemUseButton(randomId);

                if (itemInfo.Id == Item.Present)
                    message += Messages.FormatItemOpenButton(randomId);

                if (itemInfo.Type == "CLOTHES" || itemInfo.Type == "JEWELRY")
                    message += Messages.FormatWearButton(randomId);

                if (itemInfo.Type == "TEXT")
                    message += Messages.FormatItemReadButton(item.ItemId);

                if (itemInfo.Type == "PLAYERFOOD")
                    message += Messages.FormatItemConsumeButton(randomId);

                if (Item.IsThrowable(itemInfo.Id))
                    message += Messages.FormatItemThrowButton(randomId);

                message += Messages.FormatItemInformationButton(randomId);
                message += "^R1";
            }

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildHorseFeedMenu(HorseInstance horse, User user)
        {
            string message = "";
            message += Messages.FormatHorseCurrentStatus(horse.Name);
            message += Messages.FormatHorseBasicStat(horse.BasicStats.Health, horse.BasicStats.Hunger, horse.BasicStats.Thirst, horse.BasicStats.Mood, horse.BasicStats.Tiredness, horse.BasicStats.Groom, horse.BasicStats.Shoes);
            message += Messages.HorseHoldingHorseFeed;
            foreach(InventoryItem item in user.Inventory.GetItemList())
            {
                Item.ItemInformation itemInfo = item.ItemInstances[0].GetItemInfo();
                if(itemInfo.Type == "HORSEFOOD")
                {
                    message += Messages.FormatHorseFeedEntry(itemInfo.IconId, item.ItemInstances.Count, itemInfo.Name, item.ItemInstances[0].RandomId);
                }
                
            }
            message += Messages.BackToHorse;
            return message;
        }

        public static string BuildAutoSellMenu(HorseInstance horse)
        {
            string message = "";
            message += Messages.FormatAutoSellMenu(horse.AutoSell);
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }


        public static string BuildHorseReleaseConfirmationMessage(HorseInstance horse)
        {
            string message = Messages.FormatHorseAreYouSureMessage(horse.RandomId);
            message += Messages.BackToMapHorse;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildHorseCompanionEquipMenu(HorseInstance horse, User user)
        {
            string message = "";
            message += Messages.FormatHorseCompanionMenuHeader(horse.Name);
            if (horse.Equipment.Companion != null)
                message += Messages.FormatHorseCompanionSelected(horse.Equipment.Companion.IconId, horse.Equipment.Companion.Name);
            message += Messages.HorseCompanionMenuCurrentlyAvalibleCompanions;
            foreach (InventoryItem item in user.Inventory.GetItemList())
            {
                Item.ItemInformation itemInfo = item.ItemInstances[0].GetItemInfo();
                if(itemInfo.Type == "COMPANION")
                {
                    message += Messages.FormatHorseCompanionOption(itemInfo.IconId, item.ItemInstances.Count, itemInfo.Name, item.ItemId);
                }
            }
            message += Messages.BackToHorse;
            return message;
        }

        public static string BuildAllBasicStats(User user)
        {
            string message = Messages.HorseAllBasicStats;
            foreach(HorseInstance horse in user.HorseInventory.HorseList)
            {
                message += Messages.FormaHorseAllBasicStatsEntry(horse.Name, horse.Color, horse.Breed.Name, horse.Gender, horse.BasicStats.Experience);
                message += Messages.FormatHorseBasicStat(horse.BasicStats.Health, horse.BasicStats.Hunger, horse.BasicStats.Thirst, horse.BasicStats.Mood, horse.BasicStats.Tiredness, horse.BasicStats.Groom, horse.BasicStats.Shoes);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildHorseDescriptionEditMeta(HorseInstance horse)
        {
            string message = Messages.FormatDescriptionEditMeta(horse.Name, horse.Description);
            message += Messages.BackToHorse;
            return message;
        }
        public static string BuildHorseInformation(HorseInstance horse, User user)
        {
            bool isMyHorse = horse.Owner == user.Id;

            string message = "";
            if (isMyHorse)
                message += Messages.FormatHorseNameYours(horse.Name);
            else
                message += Messages.FormatHorseNameOthers(horse.Name);

            message += Messages.FormatHorseDescription(horse.Description);
            message += Messages.FormatHorseHandsHigh(horse.Color, horse.Breed.Name, horse.Gender, HorseInfo.CalculateHands(horse.AdvancedStats.Height, false));
            message += Messages.FormatHorseExperience(horse.BasicStats.Experience);
            

            if (horse.TrainTimer > 0)
                message += Messages.FormatTrainableIn(horse.TrainTimer);
            else
                message += Messages.HorseIsTrainable;
            
            if(horse.Leaser != 0)
            {
                message += Messages.FormatHorseIsLeased(horse.LeaseTime);
            }

            if (isMyHorse)
            {
                if (user.CurrentlyRidingHorse == null)
                    message += Messages.FormatMountButton(horse.RandomId);
                else
                    message += Messages.FormatDisMountButton(horse.RandomId);


                message += Messages.FormatFeedButton(horse.RandomId);
                if (horse.Leaser == 0)
                {
                    message += Messages.FormatTackButton(horse.RandomId);
                }
                message += Messages.FormatPetButton(horse.RandomId);
                if (horse.Leaser == 0)
                {
                    message += Messages.FormatProfileButton(horse.RandomId);

                    if (horse.Equipment.Saddle == null && horse.Equipment.SaddlePad == null && horse.Equipment.Bridle == null && horse.Equipment.Companion == null)
                    {
                        string autoSellMessage = Messages.HorseNoAutoSell;
                        if (horse.AutoSell > 0)
                            autoSellMessage = Messages.FormatAutoSellPrice(horse.AutoSell);
                        message += Messages.FormatAutoSell(autoSellMessage);
                        if (horse.AutoSell > 0)
                            message += Messages.HorseChangeAutoSell;
                        else
                            message += Messages.HorseSetAutoSell;
                    }
                    else
                    {
                        message += Messages.HorseCantAutoSellTacked;
                    }
                }
                else
                {
                    message += "^R1";
                }
            }


            if (horse.Leaser == 0)
            {
                if (isMyHorse)
                    message += Messages.FormatHorseCategory(horse.Category, Messages.HorseMarkAsCategory);
                else
                    message += Messages.FormatHorseCategory(horse.Category, "");
            }

            message += Messages.HorseStats;

            // Energy == Tiredness
            message += Messages.FormatHorseBasicStat(horse.BasicStats.Health, horse.BasicStats.Hunger, horse.BasicStats.Thirst, horse.BasicStats.Mood, horse.BasicStats.Tiredness, horse.BasicStats.Groom, horse.BasicStats.Shoes);
            message += Messages.HorseTacked;

            if (horse.Equipment.Saddle != null)
                message += Messages.FormatHorseTackEntry(horse.Equipment.Saddle.IconId, horse.Equipment.Saddle.Name, horse.Equipment.Saddle.Id);
             
            if (horse.Equipment.SaddlePad != null)
                message += Messages.FormatHorseTackEntry(horse.Equipment.SaddlePad.IconId, horse.Equipment.SaddlePad.Name, horse.Equipment.SaddlePad.Id);
            if (horse.Equipment.Bridle != null)
                message += Messages.FormatHorseTackEntry(horse.Equipment.Bridle.IconId, horse.Equipment.Bridle.Name, horse.Equipment.Bridle.Id);

            if(horse.Leaser == 0)
            {
                message += Messages.HorseCompanion;
                if (horse.Equipment.Companion != null)
                    if (isMyHorse)
                        message += Messages.FormatHorseCompanionEntry(horse.Equipment.Companion.IconId, horse.Equipment.Companion.Name, Messages.HorseCompanionChangeButton, horse.Equipment.Companion.Id);
                    else
                        message += Messages.FormatHorseCompanionEntry(horse.Equipment.Companion.IconId, horse.Equipment.Companion.Name, "", horse.Equipment.Companion.Id);
                else
                    if (isMyHorse)
                    message += Messages.HorseNoCompanion;
            }

            message += Messages.FormatHorseAdvancedStats(horse.Spoiled, horse.MagicUsed);

            HorseInfo.StatCalculator speedStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.SPEED);
            HorseInfo.StatCalculator strengthStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.STRENGTH);
            HorseInfo.StatCalculator conformationStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.CONFORMATION);
            HorseInfo.StatCalculator agilityStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.AGILITY);
            HorseInfo.StatCalculator enduranceStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.ENDURANCE);
            HorseInfo.StatCalculator inteligenceStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.INTELIGENCE);
            HorseInfo.StatCalculator personalityStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.PERSONALITY);

            message += Messages.FormatHorseAdvancedStat(speedStat.BreedValue, speedStat.CompanionOffset, speedStat.TackOffset, speedStat.MaxValue);
            message += Messages.FormatHorseAdvancedStat(strengthStat.BreedValue, strengthStat.CompanionOffset, strengthStat.TackOffset, strengthStat.MaxValue);
            message += Messages.FormatHorseAdvancedStat(conformationStat.BreedValue, conformationStat.CompanionOffset, conformationStat.TackOffset, conformationStat.MaxValue);
            message += Messages.FormatHorseAdvancedStat(agilityStat.BreedValue, agilityStat.CompanionOffset, agilityStat.TackOffset, agilityStat.MaxValue);
            message += Messages.FormatHorseAdvancedStat(enduranceStat.BreedValue, enduranceStat.CompanionOffset, enduranceStat.TackOffset, enduranceStat.MaxValue);
            message += Messages.FormatHorseAdvancedStat(inteligenceStat.BreedValue, inteligenceStat.CompanionOffset, inteligenceStat.TackOffset, inteligenceStat.MaxValue);
            message += Messages.FormatHorseAdvancedStat(personalityStat.BreedValue, personalityStat.CompanionOffset, personalityStat.TackOffset, personalityStat.MaxValue);

            message += Messages.FormatHorseBreedDetails(horse.Breed.Name, horse.Breed.Description);
            message += Messages.FormatHorseHeight(Convert.ToInt32(Math.Floor(HorseInfo.CalculateHands(horse.Breed.BaseStats.MinHeight,false))), Convert.ToInt32(Math.Floor(HorseInfo.CalculateHands(horse.Breed.BaseStats.MaxHeight,false))));
            
            message += Messages.FormatPossibleColors(horse.Breed.Colors);
            if (horse.Leaser == 0)
            {
                if (isMyHorse)
                {
                    bool canRelease = true;
                    if (World.InTown(user.X, user.Y))
                        canRelease = false;


                    if (World.InSpecialTile(user.X, user.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(user.X, user.Y);
                        if (tile.Code != null)
                            canRelease = false;
                    }

                    if (canRelease)
                        message += Messages.FormatHorseReleaseButton(horse.Breed.Type.ToUpper());
                }
            }


            message += Messages.HorseOthers;
            if (isMyHorse)
                message += buildHorseList(user);
            else
                message += buildHorseListIndependantlyOfUserInstance(horse.Owner);

            message += Messages.BackToMapHorse;
            message += Messages.MetaTerminator;

            return message; 

        }

        public static string BuildAllStats(User user)
        {
            string message = Messages.HorseAllStatsHeader;
            foreach(HorseInstance horse in user.HorseInventory.HorseList)
            {
                HorseInfo.StatCalculator speedStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.SPEED);
                HorseInfo.StatCalculator strengthStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.STRENGTH);
                HorseInfo.StatCalculator conformationStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.CONFORMATION);
                HorseInfo.StatCalculator agilityStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.AGILITY);
                HorseInfo.StatCalculator enduranceStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.ENDURANCE);
                HorseInfo.StatCalculator inteligenceStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.INTELIGENCE);
                HorseInfo.StatCalculator personalityStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.PERSONALITY);

                message += Messages.FormatAllStatsEntry(horse.Name, horse.Color, horse.Breed.Name, horse.Gender, horse.BasicStats.Experience);
                message += Messages.FormatCompactedBasicStats(horse.BasicStats.Health, horse.BasicStats.Hunger, horse.BasicStats.Thirst, horse.BasicStats.Mood, horse.BasicStats.Tiredness, horse.BasicStats.Groom, horse.BasicStats.Shoes);
                message += Messages.FormatCompactedAdvancedStats(speedStat.Total, strengthStat.Total, conformationStat.Total, agilityStat.Total, enduranceStat.Total, inteligenceStat.Total, personalityStat.Total);
            }
            message += Messages.HorseAllStatsLegend;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }

        public static string BuildMiscStats(User user)
        {
            string message = Messages.StatMiscHeader;
            if (user.TrackedItems.TrackingItems.Length <= 0)
                message += Messages.StatMiscNoneRecorded;
            foreach(Tracking.TrackedItem trackedItem in user.TrackedItems.TrackingItems)
            {
                try
                {
                    message += Messages.FormatMiscStatsEntry(Tracking.GetTrackedItemsStatsMenuName(trackedItem.What), trackedItem.Count);
                }
                catch(KeyNotFoundException)
                {
                    Logger.ErrorPrint(user.Username + " Has tracked items in db that dont have a value associated.");
                    continue;
                }
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }


        public static string BuildTackMenu(HorseInstance horse, User user)
        {
            string message = Messages.FormatTackedAsFollowedMessage(horse.Name);
            if(horse.Equipment.Saddle != null)
                message += Messages.FormatUnEquipSaddle(horse.Equipment.Saddle.IconId, horse.Equipment.Saddle.Name);
            if (horse.Equipment.SaddlePad != null)
                message += Messages.FormatUnEquipSaddlePad(horse.Equipment.SaddlePad.IconId, horse.Equipment.SaddlePad.Name);
            if (horse.Equipment.Bridle != null)
                message += Messages.FormatUnEquipBridle(horse.Equipment.Bridle.IconId, horse.Equipment.Bridle.Name);
            message += Messages.HorseTackInInventory;
            foreach(InventoryItem item in user.Inventory.GetItemList())
            {
                Item.ItemInformation itemInfo = item.ItemInstances[0].GetItemInfo();
                if (itemInfo.Type == "TACK")
                {
                    message += Messages.FormatHorseEquip(itemInfo.IconId, item.ItemInstances.Count, itemInfo.Name, itemInfo.Id);
                }
            }

            message += Messages.BackToHorse;
            return message;
        }
        public static string BuildNpcChatpoint(User user, Npc.NpcEntry npc, Npc.NpcChat chatpoint)
        {
            bool hideReplys = false;
            if (chatpoint.ActivateQuestId != 0)
            {
                Quest.QuestEntry quest = Quest.GetQuestById(chatpoint.ActivateQuestId);

                Quest.QuestResult result = Quest.ActivateQuest(user, quest, true);
                if (result.QuestCompleted)
                {
                    user.MetaPriority = true;
                    if (result.SetChatpoint != -1)
                        Npc.SetDefaultChatpoint(user, npc, result.SetChatpoint);
                    if (result.GotoChatpoint != -1)
                        chatpoint = Npc.GetNpcChatpoint(npc, result.GotoChatpoint);
                    if (result.NpcChat != null)
                        chatpoint.ChatText = result.NpcChat;
                }
                else
                {
                    if (result.GotoChatpoint != -1)
                        chatpoint = Npc.GetNpcChatpoint(npc, result.GotoChatpoint);
                    if (result.NpcChat != null)
                        chatpoint.ChatText = result.NpcChat;
                    
                    if (result.HideRepliesOnFail)
                        hideReplys = true;
                }
            }


            string message = "";
            message += Messages.FormatNpcChatpoint(npc.Name, npc.ShortDescription, chatpoint.ChatText);
            foreach(Npc.NpcReply reply in chatpoint.Replies)
            {
                if(reply.RequiresQuestIdCompleted != 0)
                    if (user.Quests.GetTrackedQuestAmount(reply.RequiresQuestIdCompleted) <= 0)
                        continue;
                if (reply.RequiresQuestIdNotCompleted != 0)
                    if (user.Quests.GetTrackedQuestAmount(reply.RequiresQuestIdNotCompleted) >= 1)
                        continue;
                if (hideReplys)
                    continue;
                message += Messages.FormatNpcReply(reply.ReplyText, reply.Id);
            }
            message += Messages.BackToMap + Messages.MetaTerminator;
            return message;
        }

        public static string BuildMetaInfo(User user, int x, int y)
        {
            string message = "";
            message += buildLocationString(x, y);
            message += BuildWildHorseList(user);
            message += buildNpc(user, x, y);

            message += buildCommonInfo(user, x, y);
            return message;
        }
        public static string BuildPawneerOrderFound(HorseInstance instance)
        {
            string message = Messages.FormatPawneerOrderHorseFound(instance.Breed.Name, instance.Color, instance.Gender, instance.AdvancedStats.Height, instance.AdvancedStats.Personality, instance.AdvancedStats.Inteligence);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildPawneerOrderGenderList(HorseInfo.Breed breed, string color)
        {
            string message = Messages.FormatPawneerOrderSelectGender(color, breed.Name);
            foreach (string gender in breed.GenderTypes())
                message += Messages.FormatPawneerOrderGenderEntry(Converters.CapitalizeFirstLetter(gender), gender);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildPawneerOrderColorList(HorseInfo.Breed breed)
        {
            string message = Messages.FormatPawneerOrderSelectColor(breed.Name);
            foreach (string color in breed.Colors)
                message += Messages.FormatPawneerOrderColorEntry(color);

            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;

        }
        public static string BuildPawneerOrderBreedList()
        {
            string message = Messages.PawneerOrderSelectBreed;
            foreach (HorseInfo.Breed breed in HorseInfo.Breeds.OrderBy(o => o.Name).ToList())
            {
                if (breed.Swf == "")
                    continue;
                if (breed.SpawnInArea == "none")
                    continue;
                message += Messages.FormatPawneerOrderBreedEntry(breed.Name, breed.Id);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildPawneerConfimation(HorseInstance horse)
        {
            string message = "";
            message += Messages.FormatPawneerConfirmPawn(horse.Breed.Name, horse.RandomId);
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildSanta(User user)
        {
            string message = Messages.SantaHiddenText;
            InventoryItem[] items = user.Inventory.GetItemList();
            foreach (InventoryItem item in items)
            {
                Item.ItemInformation itemInfo = Item.GetItemById(item.ItemId);
                int randomId = item.ItemInstances[0].RandomId;
                if (itemInfo.Type != "QUEST" && itemInfo.Type != "TEXT" && itemInfo.Type != "COMPANION" && itemInfo.Id != Item.Present)
                    message += Messages.FormatSantaItemEntry(itemInfo.IconId, itemInfo.Name, randomId);
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildTrainer(User user, Trainer trainer)
        {
            string message = "";
            message += Messages.FormatTrainerHeaderFormat(trainer.ImprovesStat, trainer.MoneyCost, trainer.ImprovesAmount, trainer.ExperienceGained);

            
            foreach (HorseInstance horse in user.HorseInventory.HorseList.OrderBy(o => o.Name).ToList())
            {
                HorseInfo.StatCalculator speedStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.SPEED);
                HorseInfo.StatCalculator strengthStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.STRENGTH);
                HorseInfo.StatCalculator conformationStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.CONFORMATION);
                HorseInfo.StatCalculator agilityStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.AGILITY);
                HorseInfo.StatCalculator enduranceStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.ENDURANCE);

                HorseInfo.StatCalculator statCalculator;
                switch (trainer.ImprovesStat.ToUpper())
                {
                    case "SPEED":
                        statCalculator = speedStat;
                        break;
                    case "STRENGTH":
                        statCalculator = strengthStat;
                        break;
                    case "AGILITY":
                        statCalculator = agilityStat;
                        break;
                    case "ENDURANCE":
                        statCalculator = enduranceStat;
                        break;
                    case "CONFORMATION":
                        statCalculator = conformationStat;
                        break;
                    default:
                        statCalculator = speedStat;
                        break;
                }
                
                if(statCalculator.BreedValue < statCalculator.MaxValue)
                    message += Messages.FormatTrainerTrainInEntry(horse.Name, statCalculator.BreedValue, statCalculator.MaxValue, horse.RandomId);
                else
                    message += Messages.FormatTrainerFullyTrained(horse.Name, statCalculator.BreedValue);

            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildPawneer(User user)
        {
            string message = "";
            if (user.Inventory.HasItemId(Item.PawneerOrder))
                message += Messages.PawneerOrderMeta;
            message += Messages.PawneerUntackedHorsesICanBuy;
            foreach(HorseInstance horse in user.HorseInventory.HorseList.OrderBy(o => o.Name).ToList())
            {
                if(horse.Category == "TRADING" && horse.Equipment.Bridle == null && horse.Equipment.Saddle == null && horse.Equipment.SaddlePad == null && horse.Equipment.Companion == null)
                {
                    message += Messages.FormatPawneerHorseEntry(horse.Name, Pawneer.CalculateTotalPrice(horse), horse.RandomId);
                }
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildLeaserOnLeaseInfo(Leaser leaser)
        {
            string mesasge = "";
            mesasge += leaser.OnLeaseText;
            mesasge += Messages.BackToMap;
            mesasge += Messages.MetaTerminator;
            return mesasge;
        }
        private static string buildLeaser(User user, Leaser[] leasers)
        {
            string message = "";
            foreach(Leaser leaser in leasers)
            {
                message += leaser.Info;
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildComposeMailMenu()
        {
            string message = Messages.CityHallMailSendMeta;
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildTownHall(User user)
        {
            if(user.MailBox.UnreadMailCount > 0)
            {
                
                byte[] RipOffAOLSound = PacketBuilder.CreatePlaysoundPacket(Messages.MailSe);
                user.LoggedinClient.SendPacket(RipOffAOLSound);

                byte[] mailReceivedText = PacketBuilder.CreateChat(Messages.MailReceivedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.LoggedinClient.SendPacket(mailReceivedText);

                user.MailBox.ReadAllMail();
            }

            string message = Messages.CityHallMenu;
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        private static string buildArena(User user, Arena arena)
        {
            string message = Messages.FormatArenaEventName(arena.Type);
            if(arena.Mode == "TAKINGENTRIES")
            {
                int minutes = World.ServerTime.Minutes % 60;

                int lastMinutes = minutes - (minutes % arena.RaceEvery);
                int lastHours = (lastMinutes / 60);

                string amOrPm = "am";
                if (lastHours == 0)
                {
                    amOrPm = "am";
                    lastHours = 12;
                }
                if (lastHours > 12)
                {
                    lastHours -= 12;
                    amOrPm = "pm";
                }

                message += Messages.FormatArenaCurrentlyTakingEntries(lastHours, lastMinutes, amOrPm, arena.RaceEvery - minutes);
                if (arena.Entries.Count > arena.Slots)
                {
                    message += Messages.ArenaCompetitionFull;
                }
                else if (!arena.UserHasHorseEntered(user))
                {
                    
                    foreach(HorseInstance horseInstance in user.HorseInventory.HorseList)
                    {
                        if (horseInstance.Leaser > 0)
                            continue;

                        if(horseInstance.Equipment.Saddle != null && horseInstance.Equipment.SaddlePad != null && horseInstance.Equipment.Bridle != null)
                            message += Messages.FormatArenaEnterHorseButton(horseInstance.Name, arena.EntryCost, horseInstance.RandomId);
                    }
                }
                else
                {
                    message += Messages.ArenaYouHaveHorseEntered;
                }
                
            }
            else if(arena.Mode == "COMPETING")
            {
                message += Messages.ArenaCompetitionInProgress;
            }

            message += Messages.ArenaCurrentCompetitors;
            foreach(Arena.ArenaEntry entries in arena.Entries)
            {
                message += Messages.FormatArenaCompetingHorseEntry(entries.EnteredUser.Username, entries.EnteredHorse.Name, entries.EnteredHorse.RandomId);
            }
            message += Messages.ExitThisPlace;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildSpecialTileInfo(User user, World.SpecialTile specialTile)
        {
            string message = "";

            if (specialTile.Code == null)
                message += buildLocationString(specialTile.X, specialTile.Y);


            if (specialTile.Title != null && specialTile.Title != "")
                message += Messages.FormatTileName(specialTile.Title);


            if (specialTile.Description != null && specialTile.Description != "")
                message += specialTile.Description;
            message += Messages.Seperator; // <BR>

            string npc = buildNpc(user, specialTile.X, specialTile.Y);
            message += npc;

            if (specialTile.Code == null || specialTile.Code == "")
                message += buildCommonInfo(user, specialTile.X, specialTile.Y);
            else
            {

                user.MetaPriority = true;

                string TileCode = specialTile.Code;

                string TileArg = "";
                if (TileCode.Contains("-"))
                {
                    TileArg = TileCode.Split('-')[1];
                    TileCode = TileCode.Split('-')[0];
                }
                if (TileCode == "EXITABLE")
                    message += Messages.ExitThisPlace;

                if (TileCode == "TRANSPORT")
                {
                    Transport.TransportPoint point = Transport.GetTransportPoint(specialTile.X, specialTile.Y);
                    message += Meta.BuildTransportInfo(user, point);
                }

                if (TileCode == "STRAWPILE")
                {
                    if (user.Inventory.HasItemId(Item.Pitchfork))
                        message += Messages.HasPitchforkMeta + Messages.ExitThisPlace + Messages.MetaTerminator;
                    else
                        message += Messages.NoPitchforkMeta + Messages.ExitThisPlace + Messages.MetaTerminator;
                }

                if (TileCode == "STORE")
                {
                    int ShopID = int.Parse(TileArg);
                    Shop shop = Shop.GetShopById(ShopID);
                    user.LastShoppedAt = shop;
                    message += buildShopInfo(shop, user.Inventory);

                }
                if(TileCode == "TOWNHALL")
                {
                    message += buildTownHall(user);
                }
                if (TileCode == "VET")
                {
                    message += buildVet(user, Vet.GetVetById(int.Parse(TileArg)));
                }
                if(TileCode == "GROOMER")
                {
                    message += buildGroomer(user, Groomer.GetGroomerById(int.Parse(TileArg)));
                }
                if (TileCode == "FARRIER")
                {
                    message += buildFarrier(user, Farrier.GetFarrierById(int.Parse(TileArg)));
                }
                if(TileCode == "BARN")
                {
                    message += buildBarn(user, Barn.GetBarnById(int.Parse(TileArg)));
                }
                if (TileCode == "BANK")
                {
                    message += buildBank(user);
                }
                if (TileCode == "WISHINGWELL")
                {
                    message += buildWishingWell(user);
                }
                if(TileCode == "HORSEPAWNEER")
                {
                    message += buildPawneer(user);
                }
                if (TileCode == "VENUSFLYTRAP")
                {
                    message += buildVenusFlyTrap(user);
                }
                if (TileCode == "RIDDLER")
                {
                    message += buildRiddlerRiddle(user);
                }
                if(TileCode == "ARENA")
                {
                    message += buildArena(user, Arena.GetAreaById(int.Parse(TileArg)));
                }
                if(TileCode == "AUCTION")
                {
                    user.MetaPriority = false;
                    message += buildAuction(user, Auction.GetAuctionRoomById(int.Parse(TileArg)));
                }
                if(TileCode == "TRAINER")
                {
                    message += buildTrainer(user, Trainer.GetTrainerById(int.Parse(TileArg)));
                }
                if(TileCode == "HORSELEASER")
                {
                    message += buildLeaser(user, Leaser.GetLeasersById(int.Parse(TileArg)));
                }
                if (TileCode == "LIBRARY")
                {
                    message += buildLibary();
                }
                if (TileCode == "POND")
                {
                    message += buildPond(user);
                }
                if(TileCode == "HORSES")
                {
                    message += buildHorseGame(user, TileArg);
                }
                if (TileCode == "WORKSHOP")
                {
                    message += buildWorkshop(user);
                }
                if (TileCode == "MUDHOLE")
                {
                    message += buildMudHole(user);
                }
                if (TileCode == "RANCH")
                {
                    message += buildRanch(user, int.Parse(TileArg));
                }
                if(TileCode == "SANTA")
                {
                    message += buildSanta(user);
                }
                if (TileCode == "MULTIROOM")
                {
                    user.MetaPriority = false;
                    if (TileArg != "")
                        message += buildMultiroom(TileArg, user);
                }
                if (TileCode == "PASSWORD")
                {
                    message += buildPassword();
                }
                if (TileCode == "HORSEWHISPERER")
                {
                    message += buildHorseWhisperer();
                }
                if (TileCode == "INN")
                {
                    int InnID = int.Parse(TileArg);
                    Inn inn = Inn.GetInnById(InnID);
                    user.LastVisitedInn = inn;
                    message += buildInn(inn);
                }
                if (TileCode == "FOUNTAIN")
                {
                    message += buildFountain();

                }
            }



            return message;
        }


    }
}
