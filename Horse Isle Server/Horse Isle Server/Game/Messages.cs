using HISP.Server;

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

        public static string CompetitionGearHeadFormat;
        public static string CompetitionGearBodyFormat;
        public static string CompetitionGearLegsFormat;
        public static string CompetitionGearFeetFormat;

        public static string StatsPrivateNotes;
        public static string StatsQuests;
        public static string StatsMinigameRanking;
        public static string StatsAwards;
        public static string StatsMisc;

        public static string NoJewerlyEquipped;
        public static string NoCompetitionGear;

        // Announcements
        public static string NewUserMessage;
        public static string WelcomeFormat;
        public static string MotdFormat;
        public static string IdleWarningFormat;
        public static string LoginMessageForamt;
        public static string LogoutMessageFormat;

        // Records
        public static string ProfileSavedMessage;

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

        public static string GlobalChatFormatForModerators;
        public static string DirectChatFormatForModerators;

        public static string IsleChatFormatForSender;
        public static string NearChatFormatForSender;
        public static string HereChatFormatForSender;
        public static string BuddyChatFormatForSender;
        public static string DirectChatFormatForSender;
        public static string AdminChatFormatForSender;
        public static string ModChatFormatForSender;

        public static string ChatViolationMessageFormat;
        public static string PasswordNotice;
        public static string CapsNotice;

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


        // Inventory
        public static string InventoryItemFormat;

        public static string InventoryHeaderFormat;

        public static string ItemDropButton;
        public static string ItemInformationButton;
        public static string ItemInformationByIdButton;
        public static string ItemConsumeButton;
        public static string ItemThrowButton;
        public static string ItemUseButton;
        public static string ItemReadButton;

        public static string ShopEntryFormat;
        public static string ShopBuyButton;
        public static string ShopBuy5Button;
        public static string ShopBuy25Button;

        public static string SellButton;
        public static string SellAllButton;

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
        public static string BanMessage;
        public static string DuplicateLogin;
        public static string IdleKickMessageFormat;

        // Swf
        public static string BoatCutscene;
        public static string WagonCutscene;
        public static string BallonCutscene;

        // Click
        public static string NothingInterestingHere;

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
            return ShopEntryFormat.Replace("%ICONID%", iconid.ToString()).Replace("%COUNT%", count).Replace("%TITLE%", name).Replace("%PRICE%", price.ToString());
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

        public static string FormatGlobalChatViolationMessage(Chat.Reason violationReason)
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


        public static string FormatTransportMessage(string method, string place, int cost, int id, int x, int y)
        {
            string xy = "";
            xy += (char)(((x - 4) / 64) + 20);
            xy += (char)(((x - 4) % 64) + 20);

            xy += (char)(((y - 1) / 64) + 20);
            xy += (char)(((y - 1) % 64) + 20);

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
            return BuddyChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbBuddies.ToString());
        }
        public static string FormatHereChatMessageForSender(int numbHere, string username, string message)
        {
            return HereChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbHere.ToString());
        }
        public static string FormatNearChatMessageForSender(int numbNear, string username, string message)
        {
            return NearChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbNear.ToString());
        }
        public static string FormatIsleChatMessageForSender(int numbIsle, string username, string message)
        {
            return IsleChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbIsle.ToString());
        }

        public static string FormatAdminChatForSender(int numbAdmins, string username, string message)
        {
            return AdminChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbAdmins.ToString());
        }

        public static string FormatModChatForSender(int numbMods, string username, string message)
        {
            return ModChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbMods.ToString());
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
            return LoginMessageForamt.Replace("%USERNAME%", username);
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
            return IdleKickMessageFormat.Replace("%KICK%", GameServer.IdleTimeout.ToString());
        }
 
    }
}
