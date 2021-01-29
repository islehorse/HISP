using HISP.Game.Horse;
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

        private static string buildMultiroom(string id, User user)
        {
            string message = Messages.MultiroomPlayersParticipating; 
            foreach(User userOnTile in GameServer.GetUsersOnSpecialTileCode("MULTIROOM-"+id))
            {
                if (userOnTile.Id == user.Id)
                    continue;
                message += Messages.FormatMultiroomParticipent(userOnTile.Username);
            }
            if(id[0] == 'P') // Poet
            {
                int lastPoet = Database.GetLastPlayer(id);
                string username = "";
                if(lastPoet != -1)
                    username = Database.GetUsername(lastPoet);

                message += Messages.FormatLastPoet(username);
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

        public static string BuildHorseList(User user)
        {
            string message = "";
            WildHorse[] horses = WildHorse.GetHorsesAt(user.X, user.Y);
            if(horses.Length > 0)
            {
                message = Messages.HorsesHere;
                foreach(WildHorse horse in horses)
                {
                    message += Messages.FormatWildHorse(horse.Instance.Name, horse.Instance.Breed.Name, horse.Instance.RandomId);
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
        
        public static string BuildHorseList()
        {
            string message = "";
            foreach(HorseInfo.Breed breed in HorseInfo.Breeds.OrderBy(o => o.Name).ToList())
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
            foreach(Npc.NpcEntry npc in Npc.NpcList)
            {
                if(npc.Name.ToLower().Contains(search.ToLower()))
                {
                    if(npc.LibarySearchable)
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
            if(foundNpcs.Count >= 1)
            {
                message = Messages.LibaryFindNpcSearchResultsHeader;
                foreach(Npc.NpcEntry npc in foundNpcs)
                {
                    string searchResult = Messages.FormatNpcSearchResult(npc.Name, npc.ShortDescription, npc.X, npc.Y);
                    Logger.DebugPrint(searchResult);
                    message += searchResult;
                }
            }
            if(foundNpcs.Count >= 5)
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
        private static string buildFountain()
        {
            return Messages.FountainMeta;
        }
        private static string buildPond(User user)
        {
            string message = Messages.PondHeader;
            if(!user.Inventory.HasItemId(Item.FishingPole))
            {
                message += Messages.PondNoFishingPole;
            }
            else if(!user.Inventory.HasItemId(Item.Earthworm))
            {
                message += Messages.PondNoEarthWorms;
            }
            else
            {
                message += Messages.PondGoFishing;
            }
            message += Messages.PondDrinkHereIfSafe;
            foreach(HorseInstance horse in user.HorseInventory.HorseList)
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
            if(user.HorseInventory.HorseList.Length > 0)
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
            foreach(HorseInfo.Breed breed in HorseInfo.Breeds.OrderBy(o => o.Name).ToList())
            {
                if (breed.Swf == "")
                    continue;
                if(breed.SpawnInArea == "none")
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

        private static string buildVet(Vet vet, User user)
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
            message += Messages.FormatVetApplyAllServiceMeta(totalPrice);
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
                if(TileCode == "VET")
                {
                    int VetId = int.Parse(TileArg);
                    Vet vet = Vet.GetVetById(VetId);
                    message += buildVet(vet, user);
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
                if(TileCode == "LIBRARY")
                {
                    message += buildLibary();
                }
                if(TileCode == "POND")
                {
                    message += buildPond(user);
                }
                if(TileCode == "MUDHOLE")
                {
                    message += buildMudHole(user);
                }
                if(TileCode == "MULTIROOM")
                {
                    user.MetaPriority = false; // acturally want to track updates here >-<
                    message += buildMultiroom(TileArg, user);
                }
                if(TileCode == "PASSWORD")
                {
                    message += buildPassword();
                }
                if(TileCode == "HORSEWHISPERER")
                {
                    message += buildHorseWhisperer();
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
        public static string BuildHorseEscapedMessage()
        {
            string message = Messages.HorseEvadedCapture;
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
        private static string buildHorseList(User user)
        {
            string message = "";
            int i = 1;
            foreach (HorseInfo.Category category in HorseInfo.HorseCategories)
            {
                HorseInstance[] horsesInCategory = user.HorseInventory.GetHorsesInCategory(category);
                if (horsesInCategory.Length > 0)
                {
                    message += category.Meta;
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
            // TODO: calculate max number based on ranch barns owned.
            string message = Messages.FormatHorseHeader(user.HorseInventory.MaxHorses, user.HorseInventory.HorseList.Length);

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
                message += Messages.FormaHorseAllBasicStatsEntry(horse.Name, horse.Color, horse.Breed.Name, horse.Sex, horse.BasicStats.Experience);
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
            string message = "";
            message += Messages.FormatHorseName(horse.Name);
            message += Messages.FormatHorseDescription(horse.Description);
            message += Messages.FormatHorseHandsHigh(horse.Color, horse.Breed.Name, horse.Sex, Convert.ToInt32(Math.Floor(HorseInfo.CalculateHands(horse.AdvancedStats.Height))));
            message += Messages.FormatHorseExperience(horse.BasicStats.Experience);
            
            if (horse.TrainTimer > 0)
                message += Messages.FormatTrainableIn(horse.TrainTimer);
            else
                message += Messages.HorseIsTrainable;

            if (user.CurrentlyRidingHorse == null)
                message += Messages.FormatMountButton(horse.RandomId);
            else
                message += Messages.FormatDisMountButton(horse.RandomId);

            message += Messages.FormatFeedButton(horse.RandomId);
            message += Messages.FormatTackButton(horse.RandomId);
            message += Messages.FormatPetButton(horse.RandomId);
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

            message += Messages.FormatHorseCategory(horse.Category);
            message += Messages.HorseStats;

            // What is energy?
            message += Messages.FormatHorseBasicStat(horse.BasicStats.Health, horse.BasicStats.Hunger, horse.BasicStats.Thirst, horse.BasicStats.Mood, horse.BasicStats.Tiredness, horse.BasicStats.Groom, horse.BasicStats.Shoes);
            message += Messages.HorseTacked;

            if (horse.Equipment.Saddle != null)
                message += Messages.FormatHorseTackEntry(horse.Equipment.Saddle.IconId, horse.Equipment.Saddle.Name, horse.Equipment.Saddle.Id);
             
            if (horse.Equipment.SaddlePad != null)
                message += Messages.FormatHorseTackEntry(horse.Equipment.SaddlePad.IconId, horse.Equipment.SaddlePad.Name, horse.Equipment.SaddlePad.Id);
            if (horse.Equipment.Bridle != null)
                message += Messages.FormatHorseTackEntry(horse.Equipment.Bridle.IconId, horse.Equipment.Bridle.Name, horse.Equipment.Bridle.Id);

            message += Messages.HorseCompanion;
            if (horse.Equipment.Companion != null)
                message += Messages.FormatHorseCompanionEntry(horse.Equipment.Companion.IconId, horse.Equipment.Companion.Name, horse.Equipment.Companion.Id);
            else
                message += Messages.HorseNoCompanion;

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
            message += Messages.FormatHorseAdvancedStat(agilityStat.BreedValue, personalityStat.CompanionOffset, personalityStat.TackOffset, personalityStat.MaxValue);

            message += Messages.FormatHorseBreedDetails(horse.Breed.Name, horse.Breed.Description);
            message += Messages.FormatHorseHeight(Convert.ToInt32(Math.Floor(HorseInfo.CalculateHands(horse.Breed.BaseStats.MinHeight))), Convert.ToInt32(Math.Floor(HorseInfo.CalculateHands(horse.Breed.BaseStats.MaxHeight))));
            
            message += Messages.FormatPossibleColors(horse.Breed.Colors);

            bool canRelease = true;
            if(World.InTown(user.X, user.Y))
                canRelease = false;
            

            if (World.InSpecialTile(user.X, user.Y))
            {
                World.SpecialTile tile = World.GetSpecialTile(user.X, user.Y);
                if (tile.Code != null)
                    canRelease = false;
            }

            if(canRelease)
                message += Messages.HorseReleaseButton;

            message += Messages.HorseOthers;
            message += buildHorseList(user);

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

                message += Messages.FormatAllStatsEntry(horse.Name, horse.Color, horse.Breed.Name, horse.Sex, horse.BasicStats.Experience);
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
        public static string BuildChatpoint(User user, Npc.NpcEntry npc, Npc.NpcChat chatpoint)
        {
            bool hideReplys = false;
            if (chatpoint.ActivateQuestId != 0)
            {
                Quest.QuestEntry quest = Quest.GetQuestById(chatpoint.ActivateQuestId);
                if (Quest.ActivateQuest(user, quest, true))
                {
                    user.MetaPriority = true;
                    if (quest.SetNpcChatpoint != -1)
                        Npc.SetDefaultChatpoint(user, npc, quest.SetNpcChatpoint);
                    if (quest.GotoNpcChatpoint != -1)
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
            message += BuildHorseList(user);
            message += buildNpc(user, x, y);


            message += buildCommonInfo(x, y);
            return message;
        }

    }
}
