using HISP.Game.Inventory;
using HISP.Game.Services;
using HISP.Player;
using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HISP.Game
{
    class Meta
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


        private static string buildNearbyString(int x, int y)
        {
            string playersNearby = "";

            User[] nearbyUsers = GameServer.GetNearbyUsers(x, y, true, true);
            if (nearbyUsers.Length > 1)
            {
                playersNearby += Messages.Seperator;
                playersNearby += Messages.NearbyPlayers;
                playersNearby += Messages.Seperator;

                string usersWest = "";
                string usersNorth = "";
                string usersEast = "";
                string usersSouth = "";
                foreach (User nearbyUser in nearbyUsers)
                {
                    if (nearbyUser.X < x)
                    {
                        usersWest += " " + nearbyUser.Username + " ";
                    }
                    else if (nearbyUser.X > x)
                    {
                        usersEast += " " + nearbyUser.Username + " ";
                    }
                    else if (nearbyUser.Y > y)
                    {
                        usersSouth += " " + nearbyUser.Username + " ";
                    }
                    else if (nearbyUser.Y < y)
                    {
                        usersNorth += " " + nearbyUser.Username + " ";
                    }
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

            foreach(InventoryItem shopperitem in shopperItemList)
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
            if(!hasCoins)
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

        private static string buildCommonInfo(int x, int y)
        {
            string message = "";
            message += buildNearbyString(x, y);

            // Dropped Items
            DroppedItems.DroppedItem[] Items = DroppedItems.GetItemsAt(x, y);
            if (Items.Length == 0)
                message += Messages.NothingMessage;
            else
            {
                message += Messages.ItemsOnGroundMessage;
                foreach(DroppedItems.DroppedItem item in Items)
                {
                    Item.ItemInformation itemInfo = item.instance.GetItemInfo();
                    message += Messages.FormatGrabItemMessage(itemInfo.Name, item.instance.RandomId, itemInfo.IconId);
                }
                if(Items.Length > 1)
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

        private static string buildNpc(User user, int x, int y)
        {
            string message = "";
            Npc.NpcEntry[] entries = Npc.GetNpcByXAndY(x, y);
            if (entries.Length > 0)
                message += Messages.Seperator;
            foreach (Npc.NpcEntry ent in entries)
            {
                if (ent.AdminOnly && !user.Administrator)
                    continue;

                if (ent.RequiresQuestIdCompleted != 0)
                    if (user.Quests.GetTrackedQuestAmount(ent.RequiresQuestIdNotCompleted) <= 0)
                        continue;

                if (ent.RequiresQuestIdNotCompleted != 0)
                    if (user.Quests.GetTrackedQuestAmount(ent.RequiresQuestIdCompleted) >= 1)
                        continue;

                message += Messages.FormatNpcStartChatMessage(ent.IconId, ent.Name, ent.ShortDescription, ent.Id);
                if (ent.LongDescription != "")
                    message += Messages.FormatNpcInformationButton(ent.Id);
                message += Messages.FormatNpcTalkButton(ent.Id);
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
            foreach(Item.ItemInformation item in inn.MealsOffered)
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

        public static string BuildTopHighscores(string gameName)
        {
            Highscore.HighscoreTableEntry[] scores = Database.GetTopScores(gameName, 20);
            if (scores.Length <= 0)
                return "No scores recorded.";
            string message = "";

            message += Messages.FormatHighscoreHeader(gameName);

            for (int i = 0; i < scores.Length; i++)
            {
                message += Messages.FormatHighscoreListEntry(i+1, scores[i].Score, Database.GetUsername(scores[i].UserId), scores[i].TimesPlayed);
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
                message += Messages.FormatBestTimeListEntry(i+1, scores[i].Score, Database.GetUsername(scores[i].UserId), scores[i].TimesPlayed);
            }
            message += Messages.BackToMap;
            message += Messages.MetaTerminator;
            return message;
        }
        public static string BuildMinigameRankingsForUser(User user)
        {
            string message = Messages.HighscoreHeaderMeta;
            foreach(Highscore.HighscoreTableEntry highscore in user.Highscores.HighscoreList)
            {
                if (highscore.Type == "SCORE")
                    message += Messages.FormatHighscoreStat(highscore.GameName, Database.GetRanking(highscore.Score, highscore.GameName), highscore.Score, highscore.TimesPlayed);
                else if(highscore.Type == "TIME")
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
        public static string BuildTransportInfo(Transport.TransportPoint transportPoint)
        {
            string message = "";
            // Build list of locations
            for (int i = 0; i < transportPoint.Locations.Length; i++)
            {
                int transportLocationId = transportPoint.Locations[i];
                Transport.TransportLocation transportLocation = Transport.GetTransportLocation(transportLocationId);
                message += Messages.FormatTransportMessage(transportLocation.Type, transportLocation.LocationTitle, transportLocation.Cost, transportLocation.Id, transportLocation.GotoX, transportLocation.GotoY);
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
            if(!user.Subscribed)
                message += Messages.FormatFreeTime(user.FreeMinutes);
            message += Messages.FormatPlayerDescriptionForStatsMenu(user.ProfilePage);
            message += Messages.FormatExperience(user.Experience);
            message += Messages.FormatHungryStat(Messages.FormatPlayerStat(SelectPlayerStatFormat(user.Hunger), Messages.StatHunger));
            message += Messages.FormatThirstStat(Messages.FormatPlayerStat(SelectPlayerStatFormat(user.Thirst), Messages.StatThirst));
            message += Messages.FormatTiredStat(Messages.FormatPlayerStat(SelectPlayerStatFormat(user.Thirst), Messages.StatTired));
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


        public static string BuildAwardList(User user)
        {
            string message = Messages.AwardHeader;
            if (user.Awards.AwardsEarned.Length <= 0)
                message += Messages.NoAwards;
            else
                foreach(Award.AwardEntry award in user.Awards.AwardsEarned)
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

                foreach(int questId in quest.RequiresQuestIdCompleteStatsMenu)
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
            string message = Messages.NearbyPlayersListHeader;
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
            foreach(GameClient client in clients)
            {
                if(client.LoggedIn)
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
            foreach(int id in user.Friends.List.ToArray())
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

            foreach(int id in user.Friends.List.ToArray())
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

        private static string buildFountain()
        {
            return Messages.FountainMeta;
        }
        private static string buildPassword()
        {
            return Messages.PasswordEntry + Messages.ExitThisPlace + Messages.MetaTerminator;
        }
        private static string buildBank(User user)
        {
            double moneyMade = 0;
            if (user.BankInterest > user.BankMoney)
            {
                moneyMade = user.BankInterest - user.BankMoney;
                user.BankMoney = user.BankInterest;

            }
            user.BankInterest = user.BankMoney;

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


        public static string BuildSpecialTileInfo(User user, World.SpecialTile specialTile)
        {
            string message = "";

            if (specialTile.Code == null)
                message += buildLocationString(specialTile.X, specialTile.Y)+Messages.Seperator;


            if (specialTile.Title != null && specialTile.Title != "")
                message += Messages.FormatTileName(specialTile.Title) + Messages.Seperator; 

            if (specialTile.Description != null && specialTile.Description != "")
                message += specialTile.Description;
            
            string npc = buildNpc(user, specialTile.X, specialTile.Y);
            message += npc;

            if (specialTile.Code == null)
                message += buildCommonInfo(specialTile.X, specialTile.Y);
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
                    message += Meta.BuildTransportInfo(point);
                }

                if (TileCode == "STRAWPILE")
                {
                    if (user.Inventory.HasItemId(Item.Pitchfork))
                        message += Messages.HasPitchforkMeta;
                    else
                        message += Messages.NoPitchforkMeta;
                }

                if (TileCode == "STORE")
                {
                    int ShopID = int.Parse(TileArg);
                    Shop shop = Shop.GetShopById(ShopID);
                    user.LastShoppedAt = shop;
                    message += buildShopInfo(shop,user.Inventory);

                }
                if(TileCode == "BANK")
                {
                    message += buildBank(user);
                }
                if(TileCode == "WISHINGWELL")
                {
                    message += buildWishingWell(user);
                }
                if(TileCode == "VENUSFLYTRAP")
                {
                    message += buildVenusFlyTrap(user);
                }
                if(TileCode == "PASSWORD")
                {
                    message += buildPassword();
                }
                if(TileCode == "INN")
                {
                    int InnID = int.Parse(TileArg);
                    Inn inn = Inn.GetInnById(InnID);
                    user.LastVisitedInn = inn;
                    message += buildInn(inn);
                }
                if(TileCode == "FOUNTAIN")
                {
                    message += buildFountain();

                }
            }
             


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
        public static string BuildInventoryInfo(PlayerInventory inv)
        {
            string message = "";
            message += Messages.FormatPlayerInventoryHeaderMeta(inv.Count, Messages.DefaultInventoryMax);
            InventoryItem[] items = inv.GetItemList();
            foreach(InventoryItem item in items)
            {
                Item.ItemInformation itemInfo = Item.GetItemById(item.ItemId);
                string title = itemInfo.Name;
                if (item.ItemInstances.Count > 1 && itemInfo.PluralName != "")
                    title = itemInfo.PluralName;


                message += Messages.FormatPlayerInventoryItemMeta(itemInfo.IconId, item.ItemInstances.Count, title);

                int randomId = item.ItemInstances[0].RandomId;
                if (itemInfo.Type != "QUEST" && itemInfo.Type != "TEXT" && World.CanDropItems(inv.BaseUser.X, inv.BaseUser.Y))
                    message += Messages.FormatItemDropButton(randomId);

                if (itemInfo.Id == Item.Present || itemInfo.Id == Item.DorothyShoes || itemInfo.Id == Item.Telescope)
                    message += Messages.FormatItemUseButton(randomId);

                if (itemInfo.Type == "CLOTHES" || itemInfo.Type == "JEWELRY")
                    message += Messages.FormatWearButton(randomId);

                if (itemInfo.Type == "TEXT")
                    message += Messages.FormatItemReadButton(randomId);

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
        public static string BuildChatpoint(User user, Npc.NpcEntry npc, Npc.NpcChat chatpoint)
        {
            bool hideReplys = false;
            if (chatpoint.ActivateQuestId != 0)
            {
                Quest.QuestEntry quest = Quest.GetQuestById(chatpoint.ActivateQuestId);
                if (Quest.ActivateQuest(user, quest, true))
                {
                    user.MetaPriority = true;
                    if(quest.GotoNpcChatpoint != -1)
                        chatpoint = Npc.GetNpcChatpoint(npc,quest.GotoNpcChatpoint);
                    if (quest.SuccessNpcChat != null)
                        chatpoint.ChatText = quest.SuccessNpcChat;
                }
                else
                {
                    if (quest.GotoNpcChatpoint != -1)
                        chatpoint = Npc.GetNpcChatpoint(npc, quest.GotoNpcChatpoint);
                    if (quest.FailNpcChat != null)
                        chatpoint.ChatText = quest.FailNpcChat;
                    
                    if (quest.HideReplyOnFail)
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
                    if (user.Quests.GetTrackedQuestAmount(reply.RequiresQuestIdCompleted) >= 1)
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

            message += buildNpc(user, x, y);


            message += buildCommonInfo(x, y);
            return message;
        }

    }
}
