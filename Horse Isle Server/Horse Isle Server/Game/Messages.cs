using HISP.Server;
using System;
using System.Drawing;

namespace HISP.Game
{
    class Messages
    {
        public static int RequiredChatViolations;
        public static int DefaultInventoryMax;

        // Tools
        public static string BinocularsNothing;
        public static string MagnifyNothing;
        public static string RakeNothing;
        public static string ShovelNothing;

        // Stats Page
        public static string StatsBarFormat;
        public static string StatsAreaFormat;
        public static string StatsMoneyFormat;
        public static string StatsFreeTimeFormat;
        public static string StatsDescriptionFormat;
        public static string StatsExpFormat;
        public static string StatsQuestpointsFormat;
        public static string StatsHungerFormat;
        public static string StatsThirstFormat;
        public static string StatsTiredFormat;
        public static string StatsGenderFormat;
        public static string StatsJewelFormat;
        public static string StatsCompetitionGearFormat;

        public static string JewelrySlot1Format;
        public static string JewelrySlot2Format;
        public static string JewelrySlot3Format;
        public static string JewelrySlot4Format;

        public static string CompetitionGearHeadFormat;
        public static string CompetitionGearBodyFormat;
        public static string CompetitionGearLegsFormat;
        public static string CompetitionGearFeetFormat;

        public static string StatsPrivateNotesButton;
        public static string StatsQuestsButton;
        public static string StatsMinigameRankingButton;
        public static string StatsAwardsButton;
        public static string StatsMiscButton;

        public static string NoJewerlyEquipped;
        public static string NoCompetitionGear;
        public static string JewelrySelected;
        public static string CompetitionGearSelected;

        public static string StatHunger;
        public static string StatThirst;
        public static string StatTired;

        public static string[] StatPlayerFormats;

        // Quests Completed Page
        public static string QuestLogHeader;
        public static string QuestFormat;

        public static string QuestNotCompleted;
        public static string QuestNotAvalible;
        public static string QuestCompleted;

        public static string QuestFooterFormat;

        // Announcements
        public static string NewUserMessage;
        public static string WelcomeFormat;
        public static string MotdFormat;
        public static string IdleWarningFormat;
        public static string LoginMessageFormat;
        public static string LogoutMessageFormat;

        // Records
        public static string ProfileSavedMessage;
        public static string PrivateNotesSavedMessage;
        public static string PrivateNotesMetaFormat;

        // Hay Pile

        public static string HasPitchforkMeta;
        public static string NoPitchforkMeta;

        // Chat

        public static string GlobalChatFormat;
        public static string AdsChatFormat;
        public static string BuddyChatFormat;
        public static string NearChatFormat;
        public static string IsleChatFormat;
        public static string HereChatFormat;
        public static string DirectChatFormat;
        public static string ModChatFormat;
        public static string AdminChatFormat;

        public static string AdminCommandFormat;
        public static string PlayerCommandFormat;
        public static string MuteHelp;

        public static string GlobalChatFormatForModerators;
        public static string DirectChatFormatForModerators;

        public static string IsleChatFormatForSender;
        public static string NearChatFormatForSender;
        public static string HereChatFormatForSender;
        public static string AdsChatFormatForSender;
        public static string BuddyChatFormatForSender;
        public static string DirectChatFormatForSender;
        public static string AdminChatFormatForSender;
        public static string ModChatFormatForSender;

        public static string ChatViolationMessageFormat;
        public static string PasswordNotice;
        public static string CapsNotice;
        public static string RandomMovement;

        // Transport

        public static string CantAffordTransport;
        public static string WelcomeToAreaFormat;

        //Dropped Items

        public static string NothingMessage;
        public static string ItemsOnGroundMessage;
        public static string GrabItemFormat;
        public static string GrabAllItemsButton;
        public static string GrabAllItemsMessage;
        public static string GrabbedItemMessage;
        public static string GrabbedItemButInventoryFull;
        public static string GrabbedAllItemsButInventoryFull;
        public static string GrabbedAllItemsMessage;
        public static string DroppedAnItemMessage;
        public static string ItemInformationFormat;


        // Competition Gear
        public static string EquipCompetitionGearFormat;
        public static string RemoveCompetitionGear;

        // Jewelry
        public static string EquipJewelryFormat;
        public static string MaxJewelryMessage;
        public static string RemoveJewelry;

        // Consume

        public static string ConsumeItemFormat;
        public static string ConsumedButMaxReached;

        // Inventory
        public static string InventoryItemFormat;
        public static string InventoryHeaderFormat;

        public static string ItemDropButton;
        public static string ItemInformationButton;
        public static string ItemInformationByIdButton;
        public static string ItemConsumeButton;
        public static string ItemThrowButton;
        public static string ItemUseButton;
        public static string ItemWearButton;
        public static string ItemReadButton;

        public static string ShopEntryFormat;
        public static string ShopBuyButton;
        public static string ShopBuy5Button;
        public static string ShopBuy25Button;

        public static string SellButton;
        public static string SellAllButton;

        // Highscore List
        public static string HighscoreHeaderMeta;
        public static string HighscoreFormat;
        public static string BestTimeFormat;

        public static string GameBestTimeFormat;
        public static string GameBestTimeHeaderFormat;
        public static string GameHighScoreHeaderFormat;
        public static string GameHighScoreFormat;

        // Awards

        public static string AwardHeader;
        public static string NoAwards;
        public static string AwardFormat;
        

        // Shop
        public static string ThingsIAmSelling;
        public static string ThingsYouSellMe;
        public static string InfinitySign;
        public static string CantAfford1;
        public static string CantAfford5;
        public static string CantAfford25;
        public static string Brought1Format;
        public static string Brought1ButInventoryFull;
        public static string Brought5ButInventoryFull;
        public static string Brought25ButInventoryFull;
        public static string Brought5Format;
        public static string Brought25Format;
        public static string Sold1Format;
        public static string SoldAllFormat;

        // Npc
        public static string NpcStartChatFormat;
        public static string NpcChatpointFormat;
        public static string NpcReplyFormat;
        public static string NpcInformationButton;
        public static string NpcTalkButton;
        public static string NpcInformationFormat;

        // Sec Codes
        public static string InvalidSecCodeError;
        public static string YouEarnedAnItemFormat;
        public static string YouEarnedMoneyFormat;
        public static string BeatHighscoreFormat;
        public static string BeatBestTimeFormat;

        // Player List
        public static string PlayerListHeader;
        public static string PlayerListSelectFromFollowing;
        public static string PlayerListOfBuddiesFormat;
        public static string PlayerListOfNearby;
        public static string PlayerListOfPlayersFormat;
        public static string PlayerListOfPlayersAlphabetically;
        public static string PlayerListMapAllBuddiesForamt;
        public static string PlayerListMapAllPlayersFormat;
        public static string PlayerListAbuseReport;

        public static int ThreeMonthSubscripitionIcon;
        public static int YearSubscriptionIcon;
        public static int NewUserIcon;
        public static int MonthSubscriptionIcon;
        public static int AdminIcon;
        public static int ModeratorIcon;

        public static string BuddyListHeader;
        public static string BuddyListOnlineBuddyEntryFormat;
        public static string BuddyListOfflineBuddys;
        public static string BuddyListOfflineBuddyEntryFormat;

        public static string PlayerListIconFormat;
        public static string PlayerListIconInformation;

        // Meta
        public static string IsleFormat;
        public static string TownFormat;
        public static string AreaFormat;
        public static string LocationFormat;
        public static string TransportFormat;
        public static string NearbyPlayers;
        public static string North;
        public static string East;
        public static string South;
        public static string West;

        public static string TileFormat;
        public static string Seperator;
        
        public static string ExitThisPlace;
        public static string BackToMap;
        public static string LongFullLine;
        public static string MetaTerminator;

        // Disconnect Messages
        public static string KickReasonBanned;
        public static string KickReasonDuplicateLogin;
        public static string KickReasonIdleFormat;
        public static string KickReasonNoTime;

        // Swf
        public static string BoatCutscene;
        public static string WagonCutscene;
        public static string BallonCutscene;

        // Click
        public static string NothingInterestingHere;

        public static string FormatIconFormat(int iconId)
        {
            return PlayerListIconFormat.Replace("%ICON%", iconId.ToString());
        }
        public static string FormatOnlineBuddyEntry(string iconFormat, string username, int userId, int time, int x, int y)
        {
            string xy = FormatMapLocation(x, y);
            return BuddyListOnlineBuddyEntryFormat.Replace("%ICONFORMAT%", iconFormat).Replace("%USERNAME%", username).Replace("%TIME%", time.ToString("N0")).Replace("%PLAYERID%", userId.ToString()).Replace("%MAPXY%", xy);
        }
        public static string FormatOfflineBuddyEntry(string username, int userId, int time)
        {
            return BuddyListOfflineBuddyEntryFormat.Replace("%USERNAME%", username).Replace("%TIME%", time.ToString("N0")).Replace("%PLAYERID%", userId.ToString());
        }
        public static string FormatConsumeItemMessaege(string itemName)
        {
            return ConsumeItemFormat.Replace("%ITEM%", itemName);
        }
        public static string FormatAwardEntry(int iconId, string title, int moneyBonus)
        {
            return AwardFormat.Replace("%ICON%", iconId.ToString()).Replace("%NAME%", title).Replace("%BONUS%", moneyBonus.ToString("N0"));
        }

        public static string FormatBestTimeHeader(string gameName)
        {
            return GameBestTimeHeaderFormat.Replace("%GAMETITLE%", gameName);
        }
        public static string FormatBestTimeListEntry(int ranking, int score, string username, int totalplays)
        {
            return GameBestTimeFormat.Replace("%RANKING%", ranking.ToString("N0")).Replace("%SCORE%", score.ToString().Insert(score.ToString().Length - 2, ".")).Replace("%USERNAME%", username).Replace("%TOTALPLAYS%", totalplays.ToString("N0"));
        }
        public static string FormatHighscoreHeader(string gameName)
        {
            return GameHighScoreHeaderFormat.Replace("%GAMETITLE%", gameName);
        }
        public static string FormatHighscoreListEntry(int ranking, int score, string username, int totalplays)
        {
            return GameHighScoreFormat.Replace("%RANKING%", ranking.ToString("N0")).Replace("%SCORE%", score.ToString("N0")).Replace("%USERNAME%", username).Replace("%TOTALPLAYS%", totalplays.ToString("N0"));
        }
        public static string FormatHighscoreStat(string gameTitle, int ranking, int score, int totalplays)
        {
            return HighscoreFormat.Replace("%GAMETITLE%", gameTitle).Replace("%RANKING%", ranking.ToString("N0")).Replace("%SCORE%", score.ToString("N0")).Replace("%TOTALPLAYS%", totalplays.ToString("N0"));
        }
        public static string FormatBestTimeStat(string gameTitle, int ranking, int score, int totalplays)
        {
            return BestTimeFormat.Replace("%GAMETITLE%", gameTitle).Replace("%RANKING%", ranking.ToString("N0")).Replace("%SCORE%", score.ToString()).Replace("%TOTALPLAYS%", totalplays.ToString("N0"));
        }
        public static string FormatMoneyEarnedMessage(int money)
        {
            return YouEarnedMoneyFormat.Replace("%MONEY%", money.ToString("N0"));
        }
        public static string FormatTimeBeatenMessage(int time)
        {
            return BeatBestTimeFormat.Replace("%TIME%", time.ToString());
        }
        public static string FormatHighscoreBeatenMessage(int score)
        {
            return BeatHighscoreFormat.Replace("%SCORE%", score.ToString());
        }
        public static string FormatQuestFooter(int totalQuestsComplete, int totalQuests, int questPoints, int totalQuestPoints)
        {
            int questsComplete = Convert.ToInt32(Math.Floor(((decimal)totalQuestsComplete / (decimal)totalQuests) * (decimal)100.0));
            int questPointsComplete = Convert.ToInt32(Math.Floor(((decimal)questPoints / (decimal)totalQuestPoints) * (decimal)100.0));
            return QuestFooterFormat.Replace("%TOTALCOMPLETED%", totalQuestsComplete.ToString("N0")).Replace("%TOTALQUESTS%", totalQuests.ToString("N0")).Replace("%TOTALPERCENT%", questsComplete.ToString()).Replace("%YOURQP%", questPoints.ToString("N0")).Replace("%YOURQP%", totalQuestPoints.ToString("N0")).Replace("%QPERCENT%", questPointsComplete.ToString()).Replace("%MAXQP%", totalQuestPoints.ToString("N0"));
        }
        public static string FormatQuestLogQuest(string questTitle, int questPoints, string difficulty, string completionStatus)
        {
            return QuestFormat.Replace("%TITLE%", questTitle).Replace("%QUESTPOINTS%", questPoints.ToString("N0")).Replace("%DIFFICULTY%", difficulty).Replace("%COMPLETION%", completionStatus);
        }

        public static string FormatPrivateNotes(string privateNotes)
        {
            return PrivateNotesMetaFormat.Replace("%PRIVATENOTES%", privateNotes);
        }
        public static string FormatRandomMovementMessage(string statName)
        {
            return RandomMovement.Replace("%STAT%", statName);
        }

        public static string FormatJewerlyEquipMessage(string itemName)
        {
            return EquipJewelryFormat.Replace("%ITEM%", itemName);
        }
        public static string FormatEquipCompetitionGearMessage(string name)
        {
            return EquipCompetitionGearFormat.Replace("%ITEM%", name);
        }

        public static string FormatPlayerStat(string statFormat, string statName)
        {
            return statFormat.Replace("%STAT%", statName);
        }
        public static string FormatJewelrySlot1(string itemName, int icon)
        {
            return JewelrySlot1Format.Replace("%ITEM%", itemName).Replace("%ICON%", icon.ToString());
        }
        public static string FormatJewelrySlot2(string itemName, int icon)
        {
            return JewelrySlot2Format.Replace("%ITEM%", itemName).Replace("%ICON%", icon.ToString());
        }
        public static string FormatJewelrySlot3(string itemName, int icon)
        {
            return JewelrySlot3Format.Replace("%ITEM%", itemName).Replace("%ICON%", icon.ToString());
        }
        public static string FormatJewelrySlot4(string itemName, int icon)
        {
            return JewelrySlot4Format.Replace("%ITEM%", itemName).Replace("%ICON%", icon.ToString());
        }

        public static string FormatCompetitionGearHead(string itemName, int icon)
        {
            return CompetitionGearHeadFormat.Replace("%ITEM%", itemName).Replace("%ICON%",icon.ToString());
        }
        public static string FormatCompetitionGearBody(string itemName, int icon)
        {
            return CompetitionGearBodyFormat.Replace("%ITEM%", itemName).Replace("%ICON%", icon.ToString());
        }
        public static string FormatCompetitionGearLegs(string itemName, int icon)
        {
            return CompetitionGearLegsFormat.Replace("%ITEM%", itemName).Replace("%ICON%", icon.ToString());
        }
        public static string FormatCompetitionGearFeet(string itemName, int icon)
        {
            return CompetitionGearFeetFormat.Replace("%ITEM%", itemName).Replace("%ICON%", icon.ToString());
        }
        public static string FormatStatsBar(string username)
        {
            return StatsBarFormat.Replace("%USERNAME%", username);
        }
        public static string FormatStatsArea(string area)
        {
            return StatsAreaFormat.Replace("%AREA%", area);
        }
        public static string FormatMoneyStat(int money)
        {
            return StatsMoneyFormat.Replace("%MONEY%", money.ToString("N0"));
        }
        public static string FormatFreeTime(int freeMinutes)
        {
            return StatsFreeTimeFormat.Replace("%FREEMINUTES%", freeMinutes.ToString("N0"));
        }
        public static string FormatPlayerDescriptionForStatsMenu(string description)
        {
            return StatsDescriptionFormat.Replace("%PLAYERDESC%", description);
        }

        public static string FormatExperience(int expPoints)
        {
            return StatsExpFormat.Replace("%EXPPOINTS%", expPoints.ToString("N0"));
        }
        public static string FormatQuestPoints(int questPoints)
        {
            return StatsQuestpointsFormat.Replace("%QUESTPOINTS%", questPoints.ToString("N0"));
        }
        public static string FormatHungryStat(string status)
        {
            return StatsHungerFormat.Replace("%HUNGER%", status);
        }
        public static string FormatThirstStat(string status)
        {
            return StatsThirstFormat.Replace("%THIRST%", status);
        }
        public static string FormatTiredStat(string status)
        {
            return StatsTiredFormat.Replace("%TIRED%", status);
        }
        public static string FormatGenderStat(string gender)
        {
            return StatsGenderFormat.Replace("%GENDER%", gender);
        }
        public static string FormatJewelryStat(string jewelformat)
        {
            return StatsJewelFormat.Replace("%JEWELFORMAT%", jewelformat);
        }
        public static string FormatCompetitionGearStat(string competitonGearFormat)
        {
            return StatsCompetitionGearFormat.Replace("%GEARFORMAT%", competitonGearFormat);
        }
        public static string FormatAdminCommandCompleteMessage(string command)
        {
            return AdminCommandFormat.Replace("%COMMAND%", command);
        }

        public static string FormatPlayerCommandCompleteMessage(string command)
        {
            return PlayerCommandFormat.Replace("%COMMAND%", command);
        }

        public static string FormatYouEarnedAnItemMessage(string itemName)
        {
            return YouEarnedAnItemFormat.Replace("%ITEM%", itemName);
        }
        public static string FormatSellMessage(string itemName, int price)
        {
            return Sold1Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString());
        }
        public static string FormatSellAllMessage(string itemName, int price, int sellAmount)
        {
            return SoldAllFormat.Replace("%AMOUNT%",sellAmount.ToString()).Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString());
        }
        public static string FormatBuy25Message(string itemName, int price)
        {
            return Brought25Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString());
        }
        public static string FormatBuy5Message(string itemName, int price)
        {
            return Brought5Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString());
        }
        public static string FormatBuyMessage(string itemName, int price)
        {
            return Brought1Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString());
        }
        public static string FormatShopEntry(int iconid, string count, string name, int price)
        {
            return ShopEntryFormat.Replace("%ICONID%", iconid.ToString()).Replace("%COUNT%", count).Replace("%TITLE%", name).Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatWearButton(int randomId)
        {
            return ItemWearButton.Replace("%RANDOMID%", randomId.ToString());
        }
        public static string FormatItemInformationByIdButton(int itemId)
        {
            return ItemInformationByIdButton.Replace("%ITEMID%", itemId.ToString());
        }
        public static string FormatBuyItemButton(int itemId)
        {
            return ShopBuyButton.Replace("%ITEMID%", itemId.ToString());
        }
        public static string FormatBuy5ItemButton(int itemId)
        {
            return ShopBuy5Button.Replace("%ITEMID%", itemId.ToString());
        }
        public static string FormatBuy25ItemButton(int itemId)
        {
            return ShopBuy25Button.Replace("%ITEMID%", itemId.ToString());
        }
        public static string FormatSellButton(int randomId)
        {
            return SellButton.Replace("%RANDOMID%", randomId.ToString());
        }
        public static string FormatSellAllButton(int itemId)
        {
            return SellAllButton.Replace("%ITEMID%", itemId.ToString());
        }

        public static string FormatNpcInformation(string name, string description)
        {
            return NpcInformationFormat.Replace("%NAME%", name).Replace("%DESCRIPTION%", description);
        }
        public static string FormatItemInformation(string name, string description)
        {
            return ItemInformationFormat.Replace("%NAME%", name).Replace("%DESCRIPTION%", description);
        }
        public static string FormatNpcChatpoint(string name, string shortDescription, string chatText)
        {
            return NpcChatpointFormat.Replace("%NAME%", name).Replace("%DESCRIPTION%", shortDescription).Replace("%TEXT%", chatText);
        }

        public static string FormatNpcTalkButton(int npcId)
        {
            return NpcTalkButton.Replace("%ID%", npcId.ToString());
        }
        public static string FormatNpcInformationButton(int npcId)
        {
            return NpcInformationButton.Replace("%ID%", npcId.ToString());
        }

        public static string FormatNpcReply(string replyText, int replyId)
        {
            return NpcReplyFormat.Replace("%TEXT%", replyText).Replace("%ID%", replyId.ToString());
        }

        public static string FormatNpcStartChatMessage(int iconId, string name, string shortDescription, int npcId)
        {
            return NpcStartChatFormat.Replace("%ICONID%", iconId.ToString()).Replace("%NAME%", name).Replace("%DESCRIPTION%", shortDescription).Replace("%ID%", npcId.ToString());
        }

        public static string FormatGlobalChatViolationMessage(Chat.Chat.Reason violationReason)
        {
            return ChatViolationMessageFormat.Replace("%AMOUNT%", RequiredChatViolations.ToString()).Replace("%REASON%", violationReason.Message);
        }

        public static string FormatPlayerInventoryHeaderMeta(int itemCount, int maxItems)
        {
            return InventoryHeaderFormat.Replace("%ITEMCOUNT%", itemCount.ToString()).Replace("%MAXITEMS%", maxItems.ToString());
        }

        public static string FormatPlayerInventoryItemMeta(int iconid, int count, string name)
        {
            return InventoryItemFormat.Replace("%ICONID%", iconid.ToString()).Replace("%COUNT%", count.ToString()).Replace("%TITLE%", name);
        }

        public static string FormatItemThrowButton(int randomid)
        {
            return ItemThrowButton.Replace("%RANDOMID%", randomid.ToString());
        }
        public static string FormatItemConsumeButton(int randomid)
        {
            return ItemConsumeButton.Replace("%RANDOMID%", randomid.ToString());
        }
        public static string FormatItemInformationButton(int randomid)
        {
            return ItemInformationButton.Replace("%RANDOMID%", randomid.ToString());
        }

        public static string FormatItemDropButton(int randomid)
        {
            return ItemDropButton.Replace("%RANDOMID%", randomid.ToString());
        }
        public static string FormatItemUseButton(int randomid)
        {
            return ItemUseButton.Replace("%RANDOMID%", randomid.ToString());
        }
        public static string FormatItemReadButton(int randomid)
        {
            return ItemReadButton.Replace("%RANDOMID%", randomid.ToString());
        }

        // Meta
        public static string FormatTileName(string name)
        {
            return Messages.TileFormat.Replace("%TILENAME%", name);
        }
        public static string FormatGrabItemMessage(string name, int randomid, int iconid)
        {
            return GrabItemFormat.Replace("%ICONID%",iconid.ToString()).Replace("%ITEMNAME%", name).Replace("%RANDOMID%", randomid.ToString());
        }

        public static string FormatPlayerBuddyList(int amount)
        {
            return PlayerListOfBuddiesFormat.Replace("%AMOUNT%", amount.ToString("N0"));
        }
        public static string FormatPlayerList(int amount)
        {
            return PlayerListOfPlayersFormat.Replace("%AMOUNT%", amount.ToString("N0"));
        }

        public static string FormatMapAllBuddiesList(string buddyxys)
        {
            return PlayerListMapAllBuddiesForamt.Replace("%BUDDYXYLIST%", buddyxys);
        }

        public static string FormatMapAllPlayersList(string playerxys)
        {
            return PlayerListMapAllPlayersFormat.Replace("%ALLXYLIST%", playerxys);
        }

        public static string FormatMapLocations(Point[] xys)
        {
            string allXys = "";
            foreach(Point xy in xys)
            {
                allXys += FormatMapLocation(xy.X, xy.Y);
            }
            return allXys;
        }
        public static string FormatMapLocation(int x, int y)
        {
            string xy = "";
            xy += (char)(((x - 4) / 64) + 20);
            xy += (char)(((x - 4) % 64) + 20);

            xy += (char)(((y - 1) / 64) + 20);
            xy += (char)(((y - 1) % 64) + 20);
            return xy;
        }

        public static string FormatTransportMessage(string method, string place, int cost, int id, int x, int y)
        {
            string xy = FormatMapLocation(x, y);

            int iconId = 253;
            if(method == "WAGON")
                iconId = 254;
            return TransportFormat.Replace("%METHOD%", method).Replace("%PLACE%", place).Replace("%COST%", cost.ToString()).Replace("%ID%", id.ToString()).Replace("%ICON%",iconId.ToString()).Replace("%XY%", xy);
        }
        // For all
        public static string FormatGlobalChatMessage(string username, string message)
        {
            return GlobalChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatBuddyChatMessage(string username, string message)
        {
            return BuddyChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatIsleChatMessage(string username, string message)
        {
            return IsleChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatNearbyChatMessage(string username, string message)
        {
            return NearChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatHereChatMessage(string username, string message)
        {
            return HereChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatDirectMessage(string username, string message)
        {
            return DirectChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }
        public static string FormatDirectMessageForMod(string username, string message)
        {
            return DirectChatFormatForModerators.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }
        
        public static string FormatGlobalChatMessageForMod(string username, string message)
        {
            return GlobalChatFormatForModerators.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatAdsChatMessage(string username, string message)
        {
            return AdsChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatModChatMessage(string username, string message)
        {
            return ModChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatAdminChatMessage(string username, string message)
        {
            return AdminChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }


        // For Sender
        public static string FormatBuddyChatMessageForSender(int numbBuddies, string username, string message)
        {
            return BuddyChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbBuddies.ToString("N0"));
        }
        public static string FormatHereChatMessageForSender(int numbHere, string username, string message)
        {
            return HereChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbHere.ToString("N0"));
        }
        public static string FormatNearChatMessageForSender(int numbNear, string username, string message)
        {
            return NearChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbNear.ToString("N0"));
        }
        public static string FormatIsleChatMessageForSender(int numbIsle, string username, string message)
        {
            return IsleChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbIsle.ToString("N0"));
        }

        public static string FormatAdminChatForSender(int numbAdmins, string username, string message)
        {
            return AdminChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbAdmins.ToString("N0"));
        }

        public static string FormatAdsChatForSender(int numbListening, string username, string message)
        {
            return AdsChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbListening.ToString("N0"));
        }

        public static string FormatModChatForSender(int numbMods, string username, string message)
        {
            return ModChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbMods.ToString("N0"));
        }
        public static string FormatDirectChatMessageForSender(string username,string toUsername, string message)
        {
            return DirectChatFormatForSender.Replace("%FROMUSER%", username).Replace("%TOUSER%", toUsername).Replace(" %MESSAGE%", message);
        }
        public static string FormatIdleWarningMessage()
        {
            return IdleWarningFormat.Replace("%WARN%", GameServer.IdleWarning.ToString()).Replace("%KICK%", GameServer.IdleTimeout.ToString());
        }

        public static string FormatLoginMessage(string username)
        {
            return LoginMessageFormat.Replace("%USERNAME%", username);
        }

        public static string FormatLogoutMessage(string username)
        {
            return LogoutMessageFormat.Replace("%USERNAME%", username);
        }

        public static string FormatMOTD()
        {
            return MotdFormat.Replace("%MOTD%", ConfigReader.Motd);
        }
        public static string FormatWelcomeMessage(string username)
        {
            return WelcomeFormat.Replace("%USERNAME%", username);
        }

        // Transport
        public static string FormatWelcomeToAreaMessage(string placeName)
        {
            return WelcomeToAreaFormat.Replace("%PLACE%", placeName); 
        }

        // Disconnect
        public static string FormatIdleKickMessage()
        {
            return KickReasonIdleFormat.Replace("%KICK%", GameServer.IdleTimeout.ToString());
        }
 
    }
}
