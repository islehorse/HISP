using HISP.Server;
using System;
using System.Drawing;
using System.Globalization;

namespace HISP.Game
{
    public class Messages
    {
        public static int RequiredChatViolations;

        // Mod isle
        public static string ModIsleMessage;

        // Ranch
        public static string RanchUnownedRanchFormat;
        public static string RanchYouCouldPurchaseThisRanch;
        public static string RanchYouAllreadyOwnARanch;
        public static string RanchSubscribersOnly;
        public static string RanchDescriptionOthersFormat;
        public static string RanchUnownedRanchClicked;
        public static string RanchClickMessageFormat;

        public static string RanchDorothyShoesMessage;
        public static string RanchDorothyShoesPrisonIsleMessage;

        public static string RanchCantAffordRanch;
        public static string RanchRanchBroughtMessageFormat;
        public static string RanchSavedRanchDescripton;
        public static string RanchDefaultRanchTitle;

        public static string RanchEditDescriptionMetaFormat;
        public static string RanchTitleFormat;
        public static string RanchYourDescriptionFormat;

        public static string RanchSellAreYouSure;
        public static string RanchSoldFormat;

        // Ranch: Build.
        public static string RanchCanBuildOneOfTheFollowingInThisSpot;
        public static string RanchBuildingEntryFormat;
        public static string RanchCantAffordThisBuilding;
        public static string RanchBuildingInformationFormat;
        public static string RanchBuildingComplete;
        public static string RanchBuildingAlreadyHere;
        public static string RanchTornDownRanchBuildingFormat;
        public static string RanchViewBuildingFormat;
        public static string RanchBarnHorsesFormat;

        // Ranch: Upgrade
        public static string UpgradedMessage;
        public static string UpgradeCannotAfford;
        public static string UpgradeCurrentUpgradeFormat;
        public static string UpgradeNextUpgradeFormat;

        // Ranch: Special
        public static string BuildingRestHere;
        public static string BuildingGrainSilo;
        public static string BuildingBarnFormat;
        public static string BuildingBigBarnFormat;
        public static string BuildingGoldBarnFormat;
        public static string BuildingWaterWell;
        public static string BuildingWindmillFormat;
        public static string BuildingWagon;
        public static string BuildingTrainingPen;
        public static string BuildingVegatableGarden;

        public static string RanchTrainAllAttempt;
        public static string RanchTrainSuccess;
        public static string RanchTrainCantTrain;
        public static string RanchHorsesFullyRested;
        public static string RanchWagonDroppedYouOff;

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

        // Misc Stats

        public static string StatMiscHeader;
        public static string StatMiscNoneRecorded;
        public static string StatMiscEntryFormat;

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

        // Libary
        public static string LibaryMainMenu;
        public static string LibaryFindNpc;
        public static string LibaryFindNpcSearchResultsHeader;
        public static string LibaryFindNpcSearchResultFormat;
        public static string LibaryFindNpcSearchNoResults;
        public static string LibaryFindNpcLimit5;

        public static string HorseBreedFormat;
        public static string HorseRelativeFormat;
        public static string BreedViewerFormat;


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
        public static string TransportFormat;
        public static string TransportCostFormat;
        public static string TransportWagonFree;

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
        public static string DroppedItemTileIsFull;
        public static string ItemInformationFormat;

        // Pond
        public static string PondHeader;
        public static string PondGoFishing;
        public static string PondNoFishingPole;
        public static string PondNoEarthWorms;
        public static string PondDrinkHereIfSafe;
        public static string PondHorseDrinkFormat;

        public static string PondNotThirstyFormat;
        public static string PondDrinkFullFormat;
        public static string PondCantDrinkHpLowFormat;
        public static string PondDrinkOhNoesFormat;

        // Mud Hole

        public static string MudHoleNoHorses;
        public static string MudHoleRuinedGroomFormat;

        // Competition Gear

        public static string EquipCompetitionGearFormat;
        public static string RemoveCompetitionGear;

        // Jewelry
        public static string EquipJewelryFormat;
        public static string MaxJewelryMessage;
        public static string RemoveJewelry;

        // Books (Libary) 
        public static string BooksOfHorseIsle;
        public static string BookEntryFormat;
        public static string BookReadFormat;

        // Awards (Libary)
        public static string AwardsAvalible;
        public static string AwardEntryFormat;

        // Locations (Libary)
        public static string LocationKnownIslands;
        public static string LocationKnownTowns;
        public static string LocationIslandFormat;
        public static string LocationTownFormat;
        public static string LocationDescriptionFormat;

        // Minigames (Libary)
        public static string MinigameSingleplayer;
        public static string MinigameTwoplayer;
        public static string MinigameMultiplayer;
        public static string MinigameCompetitions;
        public static string MinigameEntryFormat;

        // Companion (Libary)
        public static string CompanionViewFormat;
        public static string CompanionEntryFormat;

        // Tack (Libary)
        public static string TackViewSetFormat;
        public static string TackSetPeiceFormat;

        // Workshop
        public static string WorkshopCraftEntryFormat;
        public static string WorkshopRequiresFormat;
        public static string WorkshopRequireEntryFormat;
        public static string WorkshopAnd;

        public static string WorkshopNoRoomInInventory;
        public static string WorkshopMissingRequiredItem;
        public static string WorkshopCraftingSuccess;
        public static string WorkshopCannotAfford;

        // Horse
        public static string BreedViewerMaximumStats;
        public static string AdvancedStatFormat;
        public static string BasicStatFormat;
        public static string HorsesHere;
        public static string WildHorseFormat;
        public static string HorseCaptureTimer;
        public static string YouCapturedTheHorse;
        public static string HorseEvadedCapture;
        public static string TooManyHorses;
        public static string HorsesMenuHeader;
        public static string UpdateHorseCategory;
        public static string HorseEntryFormat;
        public static string ViewBaiscStats;
        public static string ViewAdvancedStats;
        public static string HorseBuckedYou;

        public static string HorseRidingMessageFormat;
        public static string HorseNameFormat;
        public static string HorseDescriptionFormat;
        public static string HorseHandsHeightFormat;
        public static string HorseExperienceEarnedFormat;

        public static string HorseTrainableInFormat;
        public static string HorseIsTrainable;

        public static string HorseCannotMountUntilTackedMessage;
        public static string HorseDismountedBecauseNotTackedMessageFormat;
        public static string HorseMountButtonFormat;
        public static string HorseDisMountButtonFormat;
        public static string HorseFeedButtonFormat;
        public static string HorseTackButtonFormat;
        public static string HorsePetButtonFormat;
        public static string HorseProfileButtonFormat;

        public static string HorseNoAutoSell;
        public static string HorseAutoSellFormat;
        public static string HorseAutoSellPriceFormat;
        public static string HorseCantAutoSellTacked;
        public static string HorseCurrentlyCategoryFormat;
        public static string HorseStats;
        public static string HorseTacked;
        public static string HorseTackFormat;
        public static string HorseCompanion;
        public static string HorseCompanionFormat;
        public static string HorseNoCompanion;

        public static string HorseAdvancedStatsFormat;
        public static string HorseBreedDetailsFormat;
        public static string HorseHeightRangeFormat;
        public static string HorsePossibleColorsFormat;
        public static string HorseReleaseButton;
        public static string HorseOthers;

        public static string HorseDescriptionEditFormat;
        public static string HorseSavedProfileMessageFormat;
        public static string HorseEquipTackMessageFormat;
        public static string HorseUnEquipTackMessageFormat;
        public static string HorseStopRidingMessage;

        public static string HorsePetMessageFormat;
        public static string HorsePetTooHappy;
        public static string HorsePetTooTired;
        public static string HorseSetNewCategoryMessageFormat;

        public static string HorseAutoSellMenuFormat;
        public static string HorseIsAutoSell;
        public static string HorseAutoSellConfirmedFormat;
        public static string HorseAutoSellRemoved;

        public static string HorseChangeAutoSell;
        public static string HorseSetAutoSell;

        public static string HorseTackFailAutoSell;
        public static string HorseAreYouSureYouWantToReleaseFormat;
        public static string HorseCantReleaseTheHorseYourRidingOn;
        public static string HorseReleasedMeta;
        public static string HorseReleasedBy;

        // All Stats (basic)
        public static string HorseAllBasicStats;
        public static string HorseBasicStatEntryFormat;

        // All Stats (all)

        public static string HorseAllStatsHeader;
        public static string HorseNameEntryFormat;
        public static string HorseBasicStatsCompactedFormat;
        public static string HorseAdvancedStatsCompactedFormat;
        public static string HorseAllStatsLegend;

        // Horse compainion menu
        public static string HorseCompanionMenuHeaderFormat;
        public static string HorseCompnaionMenuCurrentCompanionFormat;
        public static string HorseCompanionEntryFormat;
        public static string HorseCompanionEquipMessageFormat;
        public static string HorseCompanionRemoveMessageFormat;
        public static string HorseCompanionMenuCurrentlyAvalibleCompanions;

        // Horse Feed Menu
        public static string HorseCurrentStatusFormat;
        public static string HorseHoldingHorseFeed;
        public static string HorsefeedFormat;
        public static string HorseNeighsThanks;
        public static string HorseCouldNotFinish;

        // Tack horse menu
        public static string HorseTackedAsFollowsFormat;
        public static string HorseUnEquipSaddleFormat;
        public static string HorseUnEquipSaddlePadFormat;
        public static string HorseUnEquipBridleFormat;
        public static string HorseTackInInventory;
        public static string HorseEquipFormat;
        public static string BackToHorse;

        // Treasure
        public static string PirateTreasureFormat;


        // Groomer

        public static string GroomerBestToHisAbilitiesFormat;
        public static string GroomerCannotAffordMessage;
        public static string GroomerHorseCurrentlyAtFormat;
        public static string GroomerApplyServiceFormat;
        public static string GroomerApplyServiceForAllFormat;
        public static string GroomerCannotImprove;

        // Vet
        public static string VetServiceHorseFormat;
        public static string VetSerivcesNotNeeded;
        public static string VetApplyServicesFormat;

        public static string VetApplyServicesForAllFormat;
        public static string VetFullHealthRecoveredMessageFormat;

        public static string VetServicesNotNeededAll;
        public static string VetAllFullHealthRecoveredMessage;
        public static string VetCannotAffordMessage;

        // Horse Whisperer

        public static string WhispererHorseLocateButtonFormat;
        public static string WhispererServiceCostYouFormat;

        public static string WhispererServiceCannotAfford;
        public static string WhispererSearchingAmoungHorses;
        public static string WhispererNoneFound;
        public static string WhispererHorsesFoundFormat;

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

        // Wishing Well

        public static string NoWishingCoins;
        public static string WishingWellMeta;
        public static string YouHaveWishingCoinsFormat;

        public static string TossedCoin;
        public static string WishItemsFormat;
        public static string WishMoneyFormat;
        public static string WishWorldPeaceFormat;
        public static string WorldPeaceOnlySoDeep;


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

        // Bank
        public static string BankMadeInIntrestFormat;
        public static string BankCarryingFormat;
        public static string BankWhatToDo;
        public static string BankOptionsFormat;

        public static string BankWithdrewMoneyFormat;
        public static string BankDepositedMoneyFormat;

        // Npc
        public static string NpcStartChatFormat;
        public static string NpcNoChatpoints;
        public static string NpcChatpointFormat;
        public static string NpcReplyFormat;
        public static string NpcInformationButton;
        public static string NpcTalkButton;
        public static string NpcInformationFormat;

        // Sec Codes
        public static string InvalidSecCodeError;
        public static string YouEarnedAnItemFormat;
        public static string YouEarnedAnItemButInventoryWasFullFormat;
        public static string YouLostAnItemFormat;
        public static string YouEarnedMoneyFormat;
        public static string BeatHighscoreFormat;
        public static string BeatBestHighscore;
        public static string BeatBestTimeFormat;

        // Abuse Report
        public static string AbuseReportMetaFormat;
        public static string AbuseReportReasonFormat;
        public static string AbuseReportPlayerNotFoundFormat;
        public static string AbuseReportFiled;
        public static string AbuseReportProvideValidReason;

        // Player List
        public static string PlayerListAbuseReport;
        public static string PlayerListHeader;
        public static string PlayerListSelectFromFollowing;
        public static string PlayerListOfBuddiesFormat;
        public static string PlayerListOfNearby;
        public static string PlayerListOfPlayersFormat;
        public static string PlayerListOfPlayersAlphabetically;
        public static string PlayerListMapAllBuddiesForamt;
        public static string PlayerListMapAllPlayersFormat;

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

        public static string NearbyPlayersListHeader;
        public static string PlayerListEntryFormat;

        public static string PlayerListAllHeader;
        public static string PlayerListAllAlphabeticalHeader;

        public static string PlayerListIdle;
        public static string PlayerListIconFormat;
        public static string PlayerListIconInformation;

        // Meta
        public static string IsleFormat;
        public static string TownFormat;
        public static string AreaFormat;
        public static string LocationFormat;
        public static string NearbyPlayers;
        public static string North;
        public static string East;
        public static string South;
        public static string West;

        public static string TileFormat;
        public static string Seperator;
        public static string VenusFlyTrapFormat;
        public static string PasswordEntry;

        public static string ExitThisPlace;
        public static string BackToMap;
        public static string BackToMapHorse;
        public static string LongFullLine;
        public static string MetaTerminator;

        // Drawing room
        public static string DrawingLastToDrawFormat;
        public static string DrawingContentsSavedInSlotFormat;
        public static string DrawingContentsLoadedFromSlotFormat;
        public static string DrawingPlzClearLoad;
        public static string DrawingPlzClearDraw;
        public static string DrawingNotSentNotSubscribed;
        public static string DrawingCannotLoadNotSubscribed;

        // Birckpoet
        public static string LastPoetFormat;

        // Multiroom
        public static string MultiroomPlayersParticipating;
        public static string MultiroomParticipentFormat;

        // Inn
        public static string InnBuyMeal;
        public static string InnBuyRest;
        public static string InnItemEntryFormat;
        public static string InnEnjoyedServiceFormat;
        public static string InnFullyRested;
        public static string InnCannotAffordService;

        // Fountain 
        public static string FountainMeta;
        public static string FountainDrankYourFull;
        public static string FountainDroppedMoneyFormat;

        // Login Fail messages
        public static string LoginFailedReasonBanned;
        public static string LoginFailedReasonBannedIpFormat;

        // Disconnect Messages
        public static string KickReasonBanned;
        public static string KickReasonKicked;
        public static string KickReasonDuplicateLogin;
        public static string KickReasonIdleFormat;
        public static string KickReasonNoTime;

        // Riddler
        public static string RiddlerEnterAnswerFormat;
        public static string RiddlerCorrectAnswerFormat;
        public static string RiddlerIncorrectAnswer;
        public static string RiddlerAnsweredAll;

        // Password
        public static string IncorrectPasswordMessage;

        // Swf
        public static string BoatCutscene;
        public static string WagonCutscene;
        public static string BallonCutscene;

        // Click
        public static string NothingInterestingHere;
        public static string FormatRanchDescOthers(string description)
        {
            return RanchDescriptionOthersFormat.Replace("%DESCRIPTION%", description);
        }
        public static string FormatRanchSoldMessage(int price)
        {
            return RanchSoldFormat.Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatRanchUnownedMeta(int price)
        {
            return RanchUnownedRanchFormat.Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatRanchClickMessage(string owner, string title)
        {
            return RanchClickMessageFormat.Replace("%USERNAME%", owner).Replace("%TITLE%", title);
        }
        public static string FormatRanchBroughtMessage(int price)
        {
            return RanchRanchBroughtMessageFormat.Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatRanchEditDescriptonMeta(string curTitle, string curDesc)
        {
            return RanchEditDescriptionMetaFormat.Replace("%RANCHTITLE%", curTitle).Replace("%RANCHDESC%", curDesc);
        }
        public static string FormatRanchTitle(string username, string title)
        {
            return RanchTitleFormat.Replace("%USERNAME%", username).Replace("%TITLE%", title);
        }
        public static string FormatRanchYoursDescription(string description)
        {
            return RanchYourDescriptionFormat.Replace("%DESCRIPTION%", description);
        }
        public static string FormatBuildingEntry(string name, int price, int buildingId)
        {
            return RanchBuildingEntryFormat.Replace("%BUILDINGNAME%", name).Replace("%PRICE%", price.ToString("N0")).Replace("%BUILDINGID%", buildingId.ToString());
        }
        public static string FormatBuildingInformaton(string name, string description)
        {
            return RanchBuildingInformationFormat.Replace("%BUILDINGNAME%", name).Replace("%BUILINGDESCRIPTION%", description);
        }
        public static string FormatBuildingAlreadyPlaced(string name, int buildingId, int price)
        {
            return RanchBuildingAlreadyHere.Replace("%BUILDINGNAME%", name).Replace("%BUILDINGID%", buildingId.ToString()).Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatBuildingTornDown(int price)
        {
            return RanchTornDownRanchBuildingFormat.Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatViewBuilding(string name, string description)
        {
            return RanchViewBuildingFormat.Replace("%BUILDINGNAME%", name).Replace("%BUILDINGDESC%", description);
        }
        public static string FormatBarn(string horseList)
        {
            return RanchBarnHorsesFormat.Replace("%HORSELIST%", horseList);
        }
        public static string FormatCurrentUpgrade(string curUpgradeName, string curUpgradeDesc, string YouCouldUpgrade, int ranchSellPrice)
        {
            return UpgradeCurrentUpgradeFormat.Replace("%UPGRADENAME%", curUpgradeName).Replace("%UPGRADEDESC%", curUpgradeDesc).Replace("%YOUCOULDUPGRADE%", YouCouldUpgrade).Replace("%SELLPRICE%", ranchSellPrice.ToString("N0"));
        }
        public static string FormatNextUpgrade(string nextUpgrade, int cost)
        {
            return UpgradeNextUpgradeFormat.Replace("%NEXTUPGRADE%", nextUpgrade).Replace("%COST%", cost.ToString("N0"));
        }
        public static string FormatBuildingBarn(int numbBarns, int numbHorses)
        {
            return BuildingBarnFormat.Replace("%COUNT%", numbBarns.ToString("N0")).Replace("%AMOUNT%", numbHorses.ToString("N0"));
        }
        public static string FormatBuildingBigBarn(int numbBarns, int numbHorses)
        {
            return BuildingBigBarnFormat.Replace("%COUNT%", numbBarns.ToString("N0")).Replace("%AMOUNT%", numbHorses.ToString("N0"));
        }
        public static string FormatBuildingGoldBarn(int numbBarns, int numbHorses)
        {
            return BuildingGoldBarnFormat.Replace("%COUNT%", numbBarns.ToString("N0")).Replace("%AMOUNT%", numbHorses.ToString("N0"));
        }
        public static string FormatBuildingWindmill(int numbWindmills, int moneyEarns)
        {
            return BuildingWindmillFormat.Replace("%COUNT%", numbWindmills.ToString("N0")).Replace("%AMOUNT%", moneyEarns.ToString("N0"));
        }
        public static string FormatTrainSuccess(string horseName)
        {
            return RanchTrainSuccess.Replace("%HORSENAME%", horseName);
        }
        public static string FormatCantTrain(string horseName)
        {
            return RanchTrainCantTrain.Replace("%HORSENAME%", horseName);
        }
        public static string FormatRiddlerRiddle(string riddle)
        {
            return RiddlerEnterAnswerFormat.Replace("%RIDDLE%", riddle);
        }
        public static string FormatRiddlerAnswerCorrect(string reason)
        {
            return RiddlerCorrectAnswerFormat.Replace("%REASON%", reason);
        }
        public static string FormatPirateTreasure(int prize)
        {
            return PirateTreasureFormat.Replace("%PRIZE%", prize.ToString("N0"));
        }
        public static string FormatWorkshopCraftEntry(int iconId, string itemName, int price, int itemId, int craftId)
        {
            return WorkshopCraftEntryFormat.Replace("%ICONID%", iconId.ToString()).Replace("%ITEMNAME%", itemName).Replace("%PRICE%", price.ToString("N0")).Replace("%ITEMID%", itemId.ToString()).Replace("%CRAFTID%", craftId.ToString());
        }
        public static string FormatWorkshopRequirements(string requiresTxt)
        {
            return WorkshopRequiresFormat.Replace("%REQUIRES%", requiresTxt);
        }
        public static string FormatWorkshopRequireEntry(int requiredCount, string itemNamePlural)
        {
            return WorkshopRequireEntryFormat.Replace("%REQCOUNT%", requiredCount.ToString("N0")).Replace("%ITEMNAME%", itemNamePlural);
        }

        public static string FormatDrawingRoomSaved(int slot)
        {
            return DrawingContentsSavedInSlotFormat.Replace("%SLOT%", slot.ToString("N0"));
        }
        public static string FormatDrawingRoomLoaded(int slot)
        {
            return DrawingContentsLoadedFromSlotFormat.Replace("%SLOT%", slot.ToString("N0"));
        }
        public static string FormatLastToDraw(string username)
        {
            return DrawingLastToDrawFormat.Replace("%USERNAME%", username);
        }
        public static string FormatGroomerApplyAllService(int count, int price)
        {
            return GroomerApplyServiceForAllFormat.Replace("%PRICE%", price.ToString("N0")).Replace("%COUNT%", count.ToString("N0"));
        }
        public static string FormatGroomerApplyService(int price, int randomid)
        {
            return GroomerApplyServiceFormat.Replace("%PRICE%", price.ToString("N0")).Replace("%RANDOMID%", randomid.ToString());
        }
        public static string FormatHorseGroomCurrentlyAt(string horseName, int currentGroom, int maxGroom)
        {
            return GroomerHorseCurrentlyAtFormat.Replace("%HORSENAME%", horseName).Replace("%TOTAL%", currentGroom.ToString()).Replace("%MAX%", maxGroom.ToString());
        }
        public static string FormatHorseGroomedToBestAbilities(string breedName)
        {
            return GroomerBestToHisAbilitiesFormat.Replace("%HORSEBREED%", breedName);
        }

        public static string FormatBookReadMeta(string author, string title, string bookText)
        {
            return BookReadFormat.Replace("%AUTHOR%", author).Replace("%TITLE%", title).Replace("%TEXT%", bookText);
        }
        public static string FormatBookEntry(string title, string author, int id)
        {
            return BookEntryFormat.Replace("%TITLE%", title).Replace("%AUTHOR%", author).Replace("%ID%", id.ToString());
        }
        public static string FormatIpBannedMessage(string Ip)
        {
            return LoginFailedReasonBannedIpFormat.Replace("%IP%", Ip);
        }
        public static string FormatAwardEntry(int iconId, string awardName, int bonusMoney, string description)
        {
            return AwardEntryFormat.Replace("%ICONID%", iconId.ToString()).Replace("%AWARDNAME%", awardName).Replace("%BONUSMONEY%",bonusMoney.ToString("N0")).Replace("%DESCRIPTION%",description);
        }

        public static string FormatLocationDescription(string description)
        {
            return LocationDescriptionFormat.Replace("%AREADESC%", description);
        }
        public static string FormatIslandLocation(string isleName, string mapXy)
        {
            return LocationIslandFormat.Replace("%ISLENAME%", isleName).Replace("%MAPXY%",mapXy);
        }
        public static string FormatTownLocation(string townName, string mapXy)
        {
            return LocationTownFormat.Replace("%TOWNNAME%", townName).Replace("%MAPXY%",mapXy);
        }
        public static string FormatMinigameEntry(string gameName, string mapXy)
        {
            return MinigameEntryFormat.Replace("%GAMENAME%",gameName).Replace("%MAPXY%",mapXy);
        }
        public static string FormatCompanionEntry(string itemDescription)
        {
            return CompanionEntryFormat.Replace("%COMPANIONDESC%", itemDescription);
        }
        public static string FormatCompanionViewButton(int iconid, string itemName, string swf)
        {
            return CompanionViewFormat.Replace("%ICONID%", iconid.ToString()).Replace("%COMPANIONNAME%",itemName).Replace("%SWF%", swf);
        }
        public static string FormatTackSetPeice(string itemName, string itemDescription)
        {
            return TackSetPeiceFormat.Replace("%ITEMNAME%",itemName).Replace("%ITEMDESC%", itemDescription);
        }

        public static string FormatTackSetView(int iconId, string tackSetName, string swf)
        {
            return TackViewSetFormat.Replace("%ICONID%",iconId.ToString()).Replace("%SETNAME%", tackSetName).Replace("%SWF%",swf);
        }

        public static string FormatWhispererHorseFoundMeta(string mapXys)
        {
            return WhispererHorsesFoundFormat.Replace("%MAPXYS%",mapXys);
        }

        public static string FormatWhispererPrice(int price)
        {
            return WhispererServiceCostYouFormat.Replace("%MONEY%",price.ToString("N0"));
        }

        public static string FormatWhispererHorseBreedButton(string breedName, int breedId)
        {
            return WhispererHorseLocateButtonFormat.Replace("%BREEDNAME%",breedName).Replace("%BREEDID%",breedId.ToString());
        }

        public static string FormatVetServiceHorseMeta(string horseName, int currentHealth, int maxHealth)
        {
            return VetServiceHorseFormat.Replace("%HORSENAME%", horseName).Replace("%CURHEALTH%", currentHealth.ToString()).Replace("%MAXHEALTH%", maxHealth.ToString());
        }

        public static string FormatVetApplyServiceMeta(int price, int randomId)
        {
            return VetApplyServicesFormat.Replace("%PRICE%", price.ToString()).Replace("%RANDOMID%", randomId.ToString());
        }

        public static string FormatVetApplyAllServiceMeta(int price)
        {
            return VetApplyServicesForAllFormat.Replace("%PRICE%", price.ToString());
        }

        public static string FormatVetHorseAtFullHealthMessage(string horseName)
        {
            return VetFullHealthRecoveredMessageFormat.Replace("%HORSENAME%", horseName);
        }


        public static string FormatPondNotThirsty(string horseName)
        {
            return PondNotThirstyFormat.Replace("%HORSENAME%", horseName);
        }
        public static string FormatPondDrinkOhNoes(string horseName)
        {
            return PondDrinkOhNoesFormat.Replace("%HORSENAME%", horseName);
        }
        public static string FormatPondDrinkFull(string horseName)
        {
            return PondDrinkFullFormat.Replace("%HORSENAME%", horseName);
        }
        public static string FormatPondHpLowMessage(string horseName)
        {
            return PondCantDrinkHpLowFormat.Replace("%HORSENAME%", horseName);
        }

        public static string FormatPondDrinkHorseFormat(string horseName, int thirst, int maxThirst, int randomId)
        {
            return PondHorseDrinkFormat.Replace("%HORSENAME%", horseName).Replace("%THIRST%", thirst.ToString()).Replace("%MAXTHIRST%", maxThirst.ToString()).Replace("%RANDOMID%", randomId.ToString());
        }
        public static string FormatMudHoleGroomDestroyed(string horseName)
        {
            return MudHoleRuinedGroomFormat.Replace("%HORSENAME%", horseName);
        }
        public static string FormatMiscStatsEntry(string statName, int value)
        {
            return StatMiscEntryFormat.Replace("%STAT%", statName).Replace("%COUNT%", value.ToString());
        }
        public static string FormatCompactedAdvancedStats(int speed, int strength, int conformation, int agility, int endurance, int inteligence, int personality)
        {
            return HorseAdvancedStatsCompactedFormat.Replace("%SPEED%", speed.ToString()).Replace("%STRENGTH%", strength.ToString()).Replace("%CONFORMATION%",conformation.ToString()).Replace("%AGILITY%", agility.ToString()).Replace("%ENDURANCE%", endurance.ToString()).Replace("%INTELIGENCE%", inteligence.ToString()).Replace("%PERSONALITY%", personality.ToString());
        }
        public static string FormatCompactedBasicStats(int health, int hunger, int thirst, int mood, int tiredness, int groom, int shoes)
        {
            int healthPercentage = Convert.ToInt32(Math.Floor((((double)health / 1000.0) * 100.0)));
            int hungerPercentage = Convert.ToInt32(Math.Floor((((double)hunger / 1000.0) * 100.0)));
            int thirstPercentage = Convert.ToInt32(Math.Floor((((double)thirst / 1000.0) * 100.0)));
            int moodPercentage = Convert.ToInt32(Math.Floor((((double)mood / 1000.0) * 100.0)));
            int tirednessPercentage = Convert.ToInt32(Math.Floor((((double)tiredness / 1000.0) * 100.0)));
            int groomPercentage = Convert.ToInt32(Math.Floor((((double)groom / 1000.0) * 100.0)));
            int shoesPercentage = Convert.ToInt32(Math.Floor((((double)shoes / 1000.0) * 100.0)));

            return HorseBasicStatsCompactedFormat.Replace("%HEALTH%", healthPercentage.ToString()).Replace("%HUNGER%", hungerPercentage.ToString()).Replace("%THIRST%", thirstPercentage.ToString()).Replace("%MOOD%", moodPercentage.ToString()).Replace("%TIREDNESS%", tirednessPercentage.ToString()).Replace("%GROOM%", groomPercentage.ToString()).Replace("%SHOES%", shoesPercentage.ToString());
        }
        public static string FormatAllStatsEntry(string horseName, string color, string breedName, string sex, int exp)
        {
            return HorseNameEntryFormat.Replace("%HORSENAME%", horseName).Replace("%COLOR%", color).Replace("%BREEDNAME%", breedName).Replace("%SEX%", sex).Replace("%EXP%", exp.ToString("N0"));
        }
        public static string FormaHorseAllBasicStatsEntry(string horseName, string color, string breedName, string sex, int exp)
        {
            return HorseBasicStatEntryFormat.Replace("%HORSENAME%", horseName).Replace("%COLOR%", color).Replace("%BREEDNAME%", breedName).Replace("%SEX%", sex).Replace("%EXP%", exp.ToString("N0"));
        }
        public static string FormatHorseReleasedBy(string username)
        {
            return HorseReleasedBy.Replace("%USERNAME%", username);
        }
        public static string FormatHorseAreYouSureMessage(int randomId)
        {
            return HorseAreYouSureYouWantToReleaseFormat.Replace("%RANDOMID%", randomId.ToString());
        }
        public static string FormatHorseCompanionRemoveMessage(string horseName)
        {
            return HorseCompanionRemoveMessageFormat.Replace("%HORSENAME%", horseName);
        }
        public static string FormatHorseCompanionEquipMessage(string horseName, string itemName)
        {
            return HorseCompanionEquipMessageFormat.Replace("%HORSENAME%", horseName).Replace("%ITEM%", itemName);
        }
        public static string FormatHorseCompanionSelected(int icon, string name)
        {
            return HorseCompnaionMenuCurrentCompanionFormat.Replace("%ICONID%", icon.ToString()).Replace("%NAME%", name);
        }
        public static string FormatHorseCompanionMenuHeader(string horseName)
        {
            return HorseCompanionMenuHeaderFormat.Replace("%HORSENAME%", horseName);
        }
        public static string FormatHorseCompanionOption(int icon, int count, string name, int id)
        {
            return HorseCompanionEntryFormat.Replace("%ICONID%", icon.ToString()).Replace("%COUNT%", count.ToString("N0")).Replace("%NAME%", name).Replace("%ID%", id.ToString());
        }
        public static string FormatHorseDismountedBecauseTackedMessage(string horsename)
        {
            return HorseDismountedBecauseNotTackedMessageFormat.Replace("%HORSENAME%", horsename);
        }
        public static string FormatAutoSellConfirmedMessage(int money)
        {
            return HorseAutoSellConfirmedFormat.Replace("%MONEY%", money.ToString("N0"));
        }
        public static string FormatAutoSellMenu(int currentAutoSellPrice)
        {
            return HorseAutoSellMenuFormat.Replace("%AUTOSELL%", currentAutoSellPrice.ToString());
        }
        public static string FormatHorseSetToNewCategory(string category)
        {
            return HorseSetNewCategoryMessageFormat.Replace("%CATEGORY%", category);
        }
        public static string FormatHorseSavedProfileMessage(string horsename)
        {
            return HorseSavedProfileMessageFormat.Replace("%HORSENAME%", horsename);
        }
        public static string FormatDescriptionEditMeta(string username, string description)
        {
            return HorseDescriptionEditFormat.Replace("%HORSENAME%", username).Replace("%DESCRIPTION%", description);
        }
        public static string FormatHorsePetMessage(string messages, int mood, int tiredness)
        {
            return HorsePetMessageFormat.Replace("%MESSAGES%", messages).Replace("%MOOD%", mood.ToString()).Replace("%TIREDNESS%", tiredness.ToString());
        }
        public static string FormatHorseCurrentStatus(string name)
        {
            return HorseCurrentStatusFormat.Replace("%HORSENAME%", name);
        }

        public static string FormatHorseFeedEntry(int icon, int count, string name, int randomId)
        {
            return HorsefeedFormat.Replace("%ICONID%", icon.ToString()).Replace("%COUNT%", count.ToString("N0")).Replace("%NAME%", name).Replace("%RANDOMID%", randomId.ToString());
        }

        public static string FormatHorseRidingMessage(string name)
        {
            return HorseRidingMessageFormat.Replace("%HORSENAME%", name);
        }
        public static string FormatEquipTackMessage(string itemName, string horseName)
        {
            return HorseEquipTackMessageFormat.Replace("%NAME%", itemName).Replace("%HORSENAME%", horseName);
        }
        public static string FormatUnEquipTackMessage(string horseName)
        {
            return HorseUnEquipTackMessageFormat.Replace("%HORSENAME%", horseName);
        }

        public static string FormatTackedAsFollowedMessage(string name)
        {
            return HorseTackedAsFollowsFormat.Replace("%NAME%", name);
        }
        public static string FormatUnEquipSaddle(int iconId, string name)
        {
            return HorseUnEquipSaddleFormat.Replace("%NAME%", name).Replace("%ICONID%", iconId.ToString());
        }
        public static string FormatUnEquipSaddlePad(int iconId, string name)
        {
            return HorseUnEquipSaddlePadFormat.Replace("%NAME%", name).Replace("%ICONID%", iconId.ToString());
        }
        public static string FormatUnEquipBridle(int iconId, string name)
        {
            return HorseUnEquipBridleFormat.Replace("%NAME%", name).Replace("%ICONID%", iconId.ToString());
        }
        public static string FormatHorseEquip(int iconId, int count, string name, int id)
        {
            return HorseEquipFormat.Replace("%ICONID%", iconId.ToString()).Replace("%COUNT%", count.ToString()).Replace("%NAME%", name).Replace("%ID%", id.ToString());
        }



        public static string FormatHorseName(string name)
        {
            return HorseNameFormat.Replace("%NAME%", name);
        }
        public static string FormatHorseDescription(string Description)
        {
            return HorseDescriptionFormat.Replace("%DESCRIPTION%", Description);
        }
        public static string FormatHorseHandsHigh(string color, string breed,string sex, double handsHigh)
        {
            return HorseHandsHeightFormat.Replace("%COLOR%", color).Replace("%SEX%", sex).Replace("%HANDS%", handsHigh.ToString(CultureInfo.InvariantCulture)).Replace("%BREED%", breed);
        }
        public static string FormatHorseExperience(int experience)
        {
            return HorseExperienceEarnedFormat.Replace("%EXP%", experience.ToString("N0"));
        }
        public static string FormatTrainableIn(int minutes)
        {
            return HorseTrainableInFormat.Replace("%TIME%", minutes.ToString());
        }

        public static string FormatDisMountButton(int randomId)
        {
            return HorseDisMountButtonFormat.Replace("%ID%", randomId.ToString());
        }
        public static string FormatMountButton(int randomId)
        {
            return HorseMountButtonFormat.Replace("%ID%", randomId.ToString());
        }
        public static string FormatFeedButton(int randomId)
        {
            return HorseFeedButtonFormat.Replace("%ID%", randomId.ToString());
        }
        public static string FormatTackButton(int randomId)
        {
            return HorseTackButtonFormat.Replace("%ID%", randomId.ToString());
        }
        public static string FormatPetButton(int randomId)
        {
            return HorsePetButtonFormat.Replace("%ID%", randomId.ToString());
        }
        public static string FormatProfileButton(int randomId)
        {
            return HorseProfileButtonFormat.Replace("%ID%", randomId.ToString());
        }

        public static string FormatAutoSellPrice(int money)
        {
            return HorseAutoSellPriceFormat.Replace("%MONEY%", money.ToString("N0"));
        }
        public static string FormatAutoSell(string autoSellStr)
        {
            return HorseAutoSellFormat.Replace("%AUTOSELL%", autoSellStr);
        }

        public static string FormatHorseCategory(string category)
        {
            return HorseCurrentlyCategoryFormat.Replace("%CATEGORY%", category);
        }
        public static string FormatHorseTackEntry(int iconId, string name, int itemId)
        {
            return HorseTackFormat.Replace("%ICON%", iconId.ToString()).Replace("%NAME%", name).Replace("%ITEMID%", itemId.ToString());
        }
        public static string FormatHorseCompanionEntry(int iconId, string name, int itemId)
        {
            return HorseCompanionFormat.Replace("%ICON%", iconId.ToString()).Replace("%NAME%", name).Replace("%ITEMID%", itemId.ToString());
        }

        public static string FormatHorseAdvancedStats(int spoiled, int magicUsed)
        {
            return HorseAdvancedStatsFormat.Replace("%SPOILED%", spoiled.ToString()).Replace("%MAGICUSED%", magicUsed.ToString());
        }
        public static string FormatHorseBreedDetails(string breedName, string description)
        {
            return HorseBreedDetailsFormat.Replace("%BREED%", breedName).Replace("%DESCRIPTION%", description);
        }
        public static string FormatHorseHeight(int minHeight, int maxHeight)
        {
            return HorseHeightRangeFormat.Replace("%MIN%", minHeight.ToString()).Replace("%MAX%", maxHeight.ToString());
        }
        public static string FormatPossibleColors(string[] colors)
        {
            return HorsePossibleColorsFormat.Replace("%COLORS%", String.Join(",", colors));
        }

        public static string FormatHorseCategoryChangedMessage(string newCategory)
        {
            return UpdateHorseCategory.Replace("%CATEGORY%", newCategory);
        }
        public static string FormatHorseEntry(int numb, string horseName, string breedName, int randomId, bool hasAutoSell)
        {
            return HorseEntryFormat.Replace("%NUMB%", numb.ToString()).Replace("%NAME%", horseName).Replace("%BREED%", breedName).Replace("%ID%", randomId.ToString()).Replace("%ISAUTOSELL%", hasAutoSell ? HorseIsAutoSell : "");
        }
        public static string FormatHorseHeader(int maxHorses, int numHorses)
        {
            return HorsesMenuHeader.Replace("%MAXHORSE%", maxHorses.ToString()).Replace("%TOTALHORSE%", numHorses.ToString());   
        }


        public static string FormatWildHorse(string name, string breed, int randomId)
        {
            return WildHorseFormat.Replace("%NAME%", name).Replace("%BREED%", breed).Replace("%RANDOMID%", randomId.ToString());
        }
        public static string FormatHorseBreedPreview(string name, string description)
        {
            return BreedViewerFormat.Replace("%NAME%", name).Replace("%DESCRIPTION%", description);
        }
        public static string FormatHorseAdvancedStat(int baseStat, int companionBoost, int tackBoost, int maxStat)
        {
            return AdvancedStatFormat.Replace("%BASE%", baseStat.ToString()).Replace("%COMPAINON%", companionBoost.ToString()).Replace("%TACK%", tackBoost.ToString()).Replace("%MAX%", maxStat.ToString());
        }
        public static string FormatHorseBasicStat(int health, int hunger, int thirst, int mood, int energy, int groom, int shoes)
        {
            return BasicStatFormat.Replace("%HEALTH%", health.ToString()).Replace("%HUNGER%", hunger.ToString()).Replace("%THIRST%", thirst.ToString()).Replace("%MOOD%", mood.ToString()).Replace("%ENERGY%", energy.ToString()).Replace("%GROOM%", groom.ToString()).Replace("%SHOES%", shoes.ToString());
        }

        public static string FormatHorseRelative(string name, int id)
        {
            return HorseRelativeFormat.Replace("%NAME%", name).Replace("%ID%", id.ToString());
        }
        public static string FormatHorseBreed(string name, int id)
        {
            return HorseBreedFormat.Replace("%NAME%", name).Replace("%ID%", id.ToString());
        }
        public static string FormatNpcSearchResult(string name, string desc,int x, int y)
        {
            string mapXy = FormatMapLocation(x, y);
            return LibaryFindNpcSearchResultFormat.Replace("%NPCNAME%", name).Replace("%MAPXY%", mapXy).Replace("%NPCDESC%", desc);
        }
        public static string FormatLastPoet(string name)
        {
            return LastPoetFormat.Replace("%USERNAME%", name);
        }
        public static string FormatMultiroomParticipent(string name)
        {
            return MultiroomParticipentFormat.Replace("%USERNAME%", name);
        }
        public static string FormatVenusFlyTrapMeta(int money)
        {
            return VenusFlyTrapFormat.Replace("%MONEY%", money.ToString("N0"));
        }
        public static string FormatBankIntrestMadeMeta(UInt64 intrestMade)
        {
            return BankMadeInIntrestFormat.Replace("%MONEY%", intrestMade.ToString("N0"));
        }
        public static string FormatBankCarryingMeta(int money, UInt64 bankMoney)
        {
            return BankCarryingFormat.Replace("%MONEY%", money.ToString("N0")).Replace("%BANKMONEY%", bankMoney.ToString("N0"));
        }
        public static string FormatBankOptionsMeta(int money, UInt64 bankMoney)
        {
            return BankOptionsFormat.Replace("%MONEY%", money.ToString("N0")).Replace("%BANKMONEY%", bankMoney.ToString("N0"));
        }

        public static string FormatDepositedMoneyMessage(int money)
        {
            return BankDepositedMoneyFormat.Replace("%MONEY%", money.ToString("N0"));
        }
        public static string FormatWithdrawMoneyMessage(int money)
        {
            return BankWithdrewMoneyFormat.Replace("%MONEY%", money.ToString("N0"));
        }

        public static string FormatNumberOfWishingCoins(int amount)
        {
            return YouHaveWishingCoinsFormat.Replace("%AMOUNT%", amount.ToString("N0"));
        }
        
        public static string FormatWishThingsMessage(string item1, string item2)
        {
            return WishItemsFormat.Replace("%ITEM%", item1).Replace("%ITEM2%", item2);
        }
        public static string FormatWishMoneyMessage(int money)
        {
            return WishMoneyFormat.Replace("%MONEY%", money.ToString("N0"));
        }
        public static string FormatWishWorldPeaceMessage(int money, string item)
        {
            return WishWorldPeaceFormat.Replace("%MONEY%", money.ToString("N0")).Replace("%ITEM%", item);
        }



        public static string FormatInnEnjoyedServiceMessage(string item, int price)
        {
            return InnEnjoyedServiceFormat.Replace("%ITEM%", item).Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatInnItemEntry(int iconId, string itemName, int price, int itemId)
        {
            return InnItemEntryFormat.Replace("%ICON%", iconId.ToString()).Replace("%NAME%", itemName).Replace("%PRICE%", price.ToString("N0")).Replace("%ID%", itemId.ToString());
        }
        public static string FormatDroppedMoneyMessage(int amount)
        {
            return FountainDroppedMoneyFormat.Replace("%MONEY%", amount.ToString("N0"));
        }
        public static string FormatAbuseReportPlayerNotFound(string username)
        {
            return AbuseReportPlayerNotFoundFormat.Replace("%USERNAME%", username);
        }
        public static string FormatAbuseReportMetaPage(string reasonsMeta)
        {
            return AbuseReportMetaFormat.Replace("%REASONS%", reasonsMeta);
        }

        public static string FormatAbuseReportReason(string id, string name)
        {
            return AbuseReportReasonFormat.Replace("%ID%", id).Replace("%NAME%", name);
        }
        public static string FormatIconFormat(int iconId)
        {
            return PlayerListIconFormat.Replace("%ICON%", iconId.ToString());
        }

        public static string FormatPlayerEntry(string iconFormat, string username, int userId, int time, int x, int y, bool idle)
        {
            string xy = FormatMapLocation(x, y);
            return PlayerListEntryFormat.Replace("%ICONFORMAT%", iconFormat).Replace("%USERNAME%", username).Replace("%PLAYERID%", userId.ToString()).Replace("%TIME%", time.ToString("N0")).Replace("%MAPXY%", xy).Replace("%IDLE%", idle ? PlayerListIdle : "");
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

        public static string FormatYouLostAnItemMessage(string itemName)
        {
            return YouLostAnItemFormat.Replace("%ITEM%", itemName);
        }
        public static string FormatYouEarnedAnItemButInventoryFullMessage(string itemName)
        {
            return YouEarnedAnItemButInventoryWasFullFormat.Replace("%ITEM%", itemName);
        }
        public static string FormatYouEarnedAnItemMessage(string itemName)
        {
            return YouEarnedAnItemFormat.Replace("%ITEM%", itemName);
        }
        public static string FormatSellMessage(string itemName, int price)
        {
            return Sold1Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatSellAllMessage(string itemName, int price, int sellAmount)
        {
            return SoldAllFormat.Replace("%AMOUNT%",sellAmount.ToString()).Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatBuy25Message(string itemName, int price)
        {
            return Brought25Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatBuy5Message(string itemName, int price)
        {
            return Brought5Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString("N0"));
        }
        public static string FormatBuyMessage(string itemName, int price)
        {
            return Brought1Format.Replace("%ITEM%", itemName).Replace("%PRICE%", price.ToString("N0"));
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

        public static string FormatTransportCost(int cost)
        {
            return TransportCostFormat.Replace("%COST%", cost.ToString("N0"));
        }
        public static string FormatTransportMessage(string method, string place, string costFormat, int id, int x, int y)
        {
            string xy = FormatMapLocation(x, y);

            int iconId = 253;
            if(method == "WAGON")
                iconId = 254;
            return TransportFormat.Replace("%METHOD%", method).Replace("%PLACE%", place).Replace("%COSTFORMAT%", costFormat).Replace("%ID%", id.ToString()).Replace("%ICON%",iconId.ToString()).Replace("%XY%", xy);
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
