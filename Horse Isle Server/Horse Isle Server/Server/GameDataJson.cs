using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using HISP.Game;

namespace HISP.Server
{
    class GameDataJson
    {
        
        public static void ReadGamedata()
        {
            if(!File.Exists(ConfigReader.GameDataFile))
            {
                Logger.ErrorPrint("Game Data JSON File: " + ConfigReader.GameDataFile + " Does not exist!");
                throw new FileNotFoundException(ConfigReader.GameDataFile + " Not found :(");
            }
            string jsonData = File.ReadAllText(ConfigReader.GameDataFile);
            dynamic gameData = JsonConvert.DeserializeObject(jsonData);

            // Register Towns
            int totalTowns = gameData.places.towns.Count;
            for (int i = 0; i < totalTowns; i++)
            {

                World.Town town = new World.Town();
                town.StartX = gameData.places.towns[i].start_x;
                town.StartY = gameData.places.towns[i].start_y;
                town.EndX = gameData.places.towns[i].end_x;
                town.EndY = gameData.places.towns[i].end_y;
                town.Name = gameData.places.towns[i].name;

                Logger.DebugPrint("Registered Town: " + town.Name + " X " + town.StartX + "-" + town.EndX + " Y " + town.StartY + "-" + town.EndY);
                World.Towns.Add(town);
            }

            // Register Zones
            int totalZones = gameData.places.zones.Count;
            for (int i = 0; i < totalZones; i++)
            {

                World.Zone zone = new World.Zone();
                zone.StartX = gameData.places.zones[i].start_x;
                zone.StartY = gameData.places.zones[i].start_y;
                zone.EndX = gameData.places.zones[i].end_x;
                zone.EndY = gameData.places.zones[i].end_y;
                zone.Name = gameData.places.zones[i].name;

                Logger.DebugPrint("Registered Zone: " + zone.Name + " X " + zone.StartX + "-" + zone.EndX + " Y " + zone.StartY + "-" + zone.EndY);
                World.Zones.Add(zone);
            }

            // Register Areas
            int totalAreas = gameData.places.areas.Count;
            for (int i = 0; i < totalAreas; i++)
            {

                World.Area area = new World.Area();
                area.StartX = gameData.places.areas[i].start_x;
                area.StartY = gameData.places.areas[i].start_y;
                area.EndX = gameData.places.areas[i].end_x;
                area.EndY = gameData.places.areas[i].end_y;
                area.Name = gameData.places.areas[i].name;

                Logger.DebugPrint("Registered Area: " + area.Name + " X " + area.StartX + "-" + area.EndX + " Y " + area.StartY + "-" + area.EndY);
                World.Areas.Add(area);
            }

            // Register Isles
            int totalIsles = gameData.places.isles.Count;
            for(int i = 0; i < totalIsles; i++)
            {

                World.Isle isle = new World.Isle();
                isle.StartX = gameData.places.isles[i].start_x;
                isle.StartY = gameData.places.isles[i].start_y;
                isle.EndX = gameData.places.isles[i].end_x;
                isle.EndY = gameData.places.isles[i].end_y;
                isle.Tileset = gameData.places.isles[i].tileset;
                isle.Name = gameData.places.isles[i].name;

                Logger.DebugPrint("Registered Isle: " + isle.Name + " X " + isle.StartX + "-" + isle.EndX + " Y " + isle.StartY + "-" + isle.EndY + " tileset: " + isle.Tileset);
                World.Isles.Add(isle);
            }

            // Register Special Tiles
            int totalSpecialTiles = gameData.places.special_tiles.Count;
            for (int i = 0; i < totalSpecialTiles; i++)
            {

                World.SpecialTile specialTile = new World.SpecialTile();
                specialTile.X = gameData.places.special_tiles[i].x;
                specialTile.Y = gameData.places.special_tiles[i].y;
                specialTile.Title = gameData.places.special_tiles[i].title;
                specialTile.Description = gameData.places.special_tiles[i].description;
                specialTile.Code = gameData.places.special_tiles[i].code;
                if(gameData.places.special_tiles[i].exit_x != null)
                    specialTile.ExitX = gameData.places.special_tiles[i].exit_x;
                if(gameData.places.special_tiles[i].exit_x != null)
                    specialTile.ExitY = gameData.places.special_tiles[i].exit_y;
                specialTile.AutoplaySwf = gameData.places.special_tiles[i].autoplay_swf;
                specialTile.TypeFlag = gameData.places.special_tiles[i].type_flag;

                Logger.DebugPrint("Registered Special Tile: " + specialTile.Title + " X " + specialTile.X + " Y: " + specialTile.Y);
                World.SpecialTiles.Add(specialTile);
            }

            // Register Filter Reasons
            int totalReasons = gameData.messages.chat.reason_messages.Count;
            for(int i = 0; i < totalReasons; i++)
            {
                Chat.Reason reason = new Chat.Reason();
                reason.Name = gameData.messages.chat.reason_messages[i].name;
                reason.Message = gameData.messages.chat.reason_messages[i].message;
                Chat.Reasons.Add(reason);

                Logger.DebugPrint("Registered Chat Warning Reason: " + reason.Name + " (Message: " + reason.Message + ")");
            }
            // Register Filters

            int totalFilters = gameData.messages.chat.filter.Count;
            for(int i = 0; i < totalFilters; i++)
            {
                Chat.Filter filter = new Chat.Filter();
                filter.FilteredWord = gameData.messages.chat.filter[i].word;
                filter.MatchAll = gameData.messages.chat.filter[i].match_all;
                filter.Reason = Chat.GetReason((string)gameData.messages.chat.filter[i].reason_type);
                Chat.FilteredWords.Add(filter);

                Logger.DebugPrint("Registered Filtered Word: " + filter.FilteredWord + " With reason: "+filter.Reason.Name+" (Matching all: " + filter.MatchAll + ")");
            }

            // Register Corrections
            int totalCorrections = gameData.messages.chat.correct.Count;
            for (int i = 0; i < totalCorrections; i++)
            {
                Chat.Correction correction = new Chat.Correction();
                correction.FilteredWord = gameData.messages.chat.correct[i].word;
                correction.ReplacedWord = gameData.messages.chat.correct[i].new_word;
                Chat.CorrectedWords.Add(correction);

                Logger.DebugPrint("Registered Word Correction: " + correction.FilteredWord + " to "+correction.ReplacedWord);
            }

            // Register Transports

            int totalTransportPoints = gameData.transport.transport_points.Count;
            for (int i = 0; i < totalTransportPoints; i++)
            {
                Transport.TransportPoint transportPoint = new Transport.TransportPoint();
                transportPoint.X = gameData.transport.transport_points[i].x;
                transportPoint.Y = gameData.transport.transport_points[i].y;
                transportPoint.Locations = gameData.transport.transport_points[i].places.ToObject<int[]>();
                Transport.TransportPoints.Add(transportPoint);

                Logger.DebugPrint("Registered Transport Point: At X: " + transportPoint.X + " Y: " + transportPoint.Y);
            }

            int totalTransportPlaces = gameData.transport.transport_places.Count;
            for (int i = 0; i < totalTransportPlaces; i++)
            {
                Transport.TransportLocation transportPlace = new Transport.TransportLocation();
                transportPlace.Id = gameData.transport.transport_places[i].id;
                transportPlace.Cost = gameData.transport.transport_places[i].cost;
                transportPlace.GotoX = gameData.transport.transport_places[i].goto_x;
                transportPlace.GotoY = gameData.transport.transport_places[i].goto_y;
                transportPlace.Type = gameData.transport.transport_places[i].type;
                transportPlace.LocationTitle = gameData.transport.transport_places[i].place_title;
                Transport.TransportLocations.Add(transportPlace);

                Logger.DebugPrint("Registered Transport Location: "+ transportPlace.LocationTitle+" To Goto X: " + transportPlace.GotoX + " Y: " + transportPlace.GotoY);
            }

            // Register Items
            int totalItems = gameData.item.item_list.Count;
            for (int i = 0; i < totalItems; i++)
            {
                Item.ItemInformation item = new Item.ItemInformation();
                item.Id = gameData.item.item_list[i].id;
                item.Name = gameData.item.item_list[i].name;
                item.PluralName = gameData.item.item_list[i].plural_name;
                item.Description = gameData.item.item_list[i].description;
                item.IconId = gameData.item.item_list[i].icon_id;
                item.SortBy = gameData.item.item_list[i].sort_by;
                item.SellPrice = gameData.item.item_list[i].sell_price;
                item.EmbedSwf = gameData.item.item_list[i].embed_swf;
                item.WishingWell = gameData.item.item_list[i].wishing_well;
                item.Type = gameData.item.item_list[i].type;
                item.MiscFlags = gameData.item.item_list[i].misc_flags.ToObject<int[]>();
                int effectsCount = gameData.item.item_list[i].effects.Count;

                Item.Effects[] effectsList = new Item.Effects[effectsCount];
                for(int ii = 0; ii < effectsCount; ii++)
                {
                    effectsList[ii] = new Item.Effects();
                    effectsList[ii].EffectsWhat = gameData.item.item_list[i].effects[ii].effect_what;
                    effectsList[ii].EffectAmount = gameData.item.item_list[i].effects[ii].effect_amount;
                }

                item.Effects = effectsList;
                item.SpawnParamaters = new Item.SpawnRules();
                item.SpawnParamaters.SpawnCap = gameData.item.item_list[i].spawn_parameters.spawn_cap;
                item.SpawnParamaters.SpawnInZone = gameData.item.item_list[i].spawn_parameters.spawn_in_zone;
                item.SpawnParamaters.SpawnOnTileType = gameData.item.item_list[i].spawn_parameters.spawn_on_tile_type;
                item.SpawnParamaters.SpawnOnSpecialTile = gameData.item.item_list[i].spawn_parameters.spawn_on_special_tile;
                item.SpawnParamaters.SpawnNearSpecialTile = gameData.item.item_list[i].spawn_parameters.spawn_near_special_tile;

                Logger.DebugPrint("Registered Item ID: " + item.Id + " Name: " + item.Name + " spawns on: "+item.SpawnParamaters.SpawnOnTileType);
                Item.Items.Add(item);
            }

            int totalThrowable = gameData.item.throwable.Count;
            for(int i = 0; i < totalThrowable; i++)
            {
                Item.ThrowableItem throwableItem = new Item.ThrowableItem();
                throwableItem.Id = gameData.item.throwable[i].id;
                throwableItem.Message = gameData.item.throwable[i].message;
                Item.ThrowableItems.Add(throwableItem);
            }

            // Register NPCs
            Logger.DebugPrint("Registering NPCS: ");
            int totalNpcs = gameData.npc_list.Count;
            for(int i = 0; i < totalNpcs; i++)
            {
                Npc.NpcEntry npcEntry = new Npc.NpcEntry();
                npcEntry.Id = gameData.npc_list[i].id;
                npcEntry.Name = gameData.npc_list[i].name;
                npcEntry.AdminDescription = gameData.npc_list[i].admin_description;
                npcEntry.ShortDescription = gameData.npc_list[i].short_description;
                npcEntry.LongDescription = gameData.npc_list[i].long_description;
                npcEntry.Moves = gameData.npc_list[i].moves;
                npcEntry.X = gameData.npc_list[i].x;
                npcEntry.Y = gameData.npc_list[i].y;
                if (gameData.npc_list[i].stay_on != null)
                    npcEntry.StayOn = gameData.npc_list[i].stay_on;
                if (gameData.npc_list[i].requires_questid_completed != null)
                    npcEntry.RequiresQuestIdCompleted = gameData.npc_list[i].requires_questid_completed;
                if (gameData.npc_list[i].requires_questid_not_completed != null)
                    npcEntry.RequiresQuestIdNotCompleted = gameData.npc_list[i].requires_questid_not_completed;
                if (gameData.npc_list[i].udlr_script != null)
                    npcEntry.UDLRScript = gameData.npc_list[i].udlr_script;
                if (gameData.npc_list[i].udlr_start_x != null)
                    npcEntry.UDLRStartX = gameData.npc_list[i].udlr_start_x;
                if (gameData.npc_list[i].udlr_start_y != null)
                    npcEntry.UDLRStartY = gameData.npc_list[i].udlr_start_y;
                npcEntry.AdminOnly = gameData.npc_list[i].admin_only;
                npcEntry.LibarySearchable = gameData.npc_list[i].libary_searchable;
                npcEntry.IconId = gameData.npc_list[i].icon_id;

                Logger.DebugPrint("\tNPC ID:" + npcEntry.Id.ToString() + " NAME: " + npcEntry.Name);
                List<Npc.NpcChat> chats = new List<Npc.NpcChat>();
                int totalNpcChat = gameData.npc_list[i].chatpoints.Count;
                for (int ii = 0; ii < totalNpcChat; ii++)
                {
                    Npc.NpcChat npcChat = new Npc.NpcChat();
                    npcChat.Id = gameData.npc_list[i].chatpoints[ii].chatpoint_id;
                    npcChat.ChatText = gameData.npc_list[i].chatpoints[ii].chat_text;
                    npcChat.ActivateQuestId = gameData.npc_list[i].chatpoints[ii].activate_questid;

                    Logger.DebugPrint("\t\tCHATPOINT ID: " + npcChat.Id.ToString() + " TEXT: " + npcChat.ChatText);
                    int totalNpcReply = gameData.npc_list[i].chatpoints[ii].replies.Count;
                    List<Npc.NpcReply> replys = new List<Npc.NpcReply>();
                    for (int iii = 0; iii < totalNpcReply; iii++)
                    {
                        Npc.NpcReply npcReply = new Npc.NpcReply();
                        npcReply.Id = gameData.npc_list[i].chatpoints[ii].replies[iii].reply_id;
                        npcReply.ReplyText = gameData.npc_list[i].chatpoints[ii].replies[iii].reply_text;
                        npcReply.GotoChatpoint = gameData.npc_list[i].chatpoints[ii].replies[iii].goto_chatpoint;

                        if (gameData.npc_list[i].chatpoints[ii].replies[iii].requires_questid_completed != null)
                            npcReply.RequiresQuestIdCompleted = gameData.npc_list[i].chatpoints[ii].replies[iii].requires_questid_completed;

                        if (gameData.npc_list[i].chatpoints[ii].replies[iii].requires_questid_not_completed != null)
                            npcReply.RequiresQuestIdNotCompleted = gameData.npc_list[i].chatpoints[ii].replies[iii].requires_questid_not_completed;

                        Logger.DebugPrint("\t\t\tREPLY ID: " + npcReply.Id.ToString() + " TEXT: " + npcReply.ReplyText);
                        replys.Add(npcReply);

                    }
                    npcChat.Replies = replys.ToArray();
                    chats.Add(npcChat);
                }
                npcEntry.Chatpoints = chats.ToArray();
                Npc.NpcList.Add(npcEntry);
            }

            // Register Quests

            Logger.DebugPrint("Registering Quests: ");
            int totalQuests = gameData.quest_list.Count;
            for(int i = 0; i < totalQuests; i++)
            {
                Quest.QuestEntry quest = new Quest.QuestEntry();
                quest.Id = gameData.quest_list[i].id;
                quest.Notes = gameData.quest_list[i].notes;
                if(gameData.quest_list[i].title != null)
                    quest.Title = gameData.quest_list[i].title;
                quest.RequiresQuestIdComplete = gameData.quest_list[i].requires_questid_npc.ToObject<int[]>();
                if (gameData.quest_list[i].alt_activation != null)
                {
                    quest.AltActivation = new Quest.QuestAltActivation();
                    quest.AltActivation.Type = gameData.quest_list[i].alt_activation.type;
                    quest.AltActivation.ActivateX = gameData.quest_list[i].alt_activation.x;
                    quest.AltActivation.ActivateY = gameData.quest_list[i].alt_activation.y;
                }
                quest.Tracked = gameData.quest_list[i].tracked;
                quest.MaxRepeats = gameData.quest_list[i].max_repeats;
                quest.MoneyCost = gameData.quest_list[i].money_cost;
                int itemsRequiredCount = gameData.quest_list[i].items_required.Count;

                List<Quest.QuestItemInfo> itmInfo = new List<Quest.QuestItemInfo>();
                for(int ii = 0; ii < itemsRequiredCount; ii++)
                {
                    Quest.QuestItemInfo itemInfo = new Quest.QuestItemInfo();
                    itemInfo.ItemId = gameData.quest_list[i].items_required[ii].item_id;
                    itemInfo.Quantity = gameData.quest_list[i].items_required[ii].quantity;
                    itmInfo.Add(itemInfo);
                }
                quest.ItemsRequired = itmInfo.ToArray();
                if(gameData.quest_list[i].fail_npc_chat != null)
                    quest.FailNpcChat = gameData.quest_list[i].fail_npc_chat;
                quest.MoneyEarned = gameData.quest_list[i].money_gained;

                int itemsGainedCount = gameData.quest_list[i].items_gained.Count;
                itmInfo = new List<Quest.QuestItemInfo>();
                for (int ii = 0; ii < itemsGainedCount; ii++)
                {
                    Quest.QuestItemInfo itemInfo = new Quest.QuestItemInfo();
                    itemInfo.ItemId = gameData.quest_list[i].items_gained[ii].item_id;
                    itemInfo.Quantity = gameData.quest_list[i].items_gained[ii].quantity;
                    itmInfo.Add(itemInfo);
                }
                quest.ItemsEarned = itmInfo.ToArray();

                quest.QuestPointsEarned = gameData.quest_list[i].quest_points;
                quest.GotoNpcChatpoint = gameData.quest_list[i].goto_npc_chatpoint;
                if(gameData.quest_list[i].warp_x != null)
                    quest.WarpX = gameData.quest_list[i].warp_x;
                if(gameData.quest_list[i].warp_y != null)
                    quest.WarpY = gameData.quest_list[i].warp_y;
                if(gameData.quest_list[i].success_message != null)
                    quest.SuccessMessage = gameData.quest_list[i].success_message;
                if(gameData.quest_list[i].success_npc_chat != null)
                    quest.SuccessNpcChat = gameData.quest_list[i].success_npc_chat;
                if (gameData.quest_list[i].requires_awardid != null)
                    quest.AwardRequired = gameData.quest_list[i].requires_awardid;
                quest.RequiresQuestIdCompleted = gameData.quest_list[i].requires_questid_completed.ToObject<int[]>();
                quest.RequiresQuestIdNotCompleted = gameData.quest_list[i].requires_questid_not_completed.ToObject<int[]>();
                quest.HideReplyOnFail = gameData.quest_list[i].hide_reply_on_fail;
                if (gameData.quest_list[i].difficulty != null)
                    quest.Difficulty = gameData.quest_list[i].difficulty;
                if (gameData.quest_list[i].author != null)
                    quest.Author = gameData.quest_list[i].author;
                if (gameData.quest_list[i].chained_questid != null)
                    quest.ChainedQuestId = gameData.quest_list[i].chained_questid;
                quest.Minigame = gameData.quest_list[i].minigame;
                Logger.DebugPrint("Registered Quest: " + quest.Id);
                Quest.QuestList.Add(quest);
            }

            int totalShops = gameData.shop_list.Count;
            for(int i = 0; i < totalShops; i++)
            {

                Shop shop = new Shop(gameData.shop_list[i].stocks_itemids.ToObject<int[]>());
                shop.BuyPricePercentage = gameData.shop_list[i].buy_percent;
                shop.SellPricePercentage = gameData.shop_list[i].sell_percent;
                shop.BuysItemTypes = gameData.shop_list[i].buys_item_types.ToObject<string[]>();
                
                Logger.DebugPrint("Registered Shop ID: "+ shop.Id + " Selling items at " + shop.SellPricePercentage + "% and buying at " + shop.BuyPricePercentage);
            }

            Item.Present = gameData.item.special.present;
            Item.MailMessage = gameData.item.special.mail_message;
            Item.DorothyShoes = gameData.item.special.dorothy_shoes;
            Item.PawneerOrder = gameData.item.special.pawneer_order;
            Item.Telescope = gameData.item.special.telescope;
            Item.Pitchfork = gameData.item.special.pitchfork;

            // New Users
            Messages.NewUserMessage = gameData.new_user.starting_message;
            Map.NewUserStartX = gameData.new_user.starting_x;
            Map.NewUserStartY = gameData.new_user.starting_y;

            // Announcements

            Messages.WelcomeFormat = gameData.messages.welcome_format;
            Messages.MotdFormat = gameData.messages.motd_format;
            Messages.ProfileSavedMessage = gameData.messages.profile_save;
            Messages.LoginMessageForamt = gameData.messages.login_format;
            Messages.LogoutMessageFormat = gameData.messages.logout_format;

            // Stats
            Messages.StatsBarFormat = gameData.messages.meta.stats_page.stats_bar_format;
            Messages.StatsAreaFormat = gameData.messages.meta.stats_page.stats_area_format;
            Messages.StatsMoneyFormat = gameData.messages.meta.stats_page.stats_money_format;
            Messages.StatsFreeTimeFormat = gameData.messages.meta.stats_page.stats_freetime_format;
            Messages.StatsDescriptionFormat = gameData.messages.meta.stats_page.stats_description_format;
            Messages.StatsExpFormat = gameData.messages.meta.stats_page.stats_experience;
            Messages.StatsQuestpointsFormat = gameData.messages.meta.stats_page.stats_questpoints;
            Messages.StatsHungerFormat = gameData.messages.meta.stats_page.stats_hunger;
            Messages.StatsThirstFormat = gameData.messages.meta.stats_page.stats_thisrt;
            Messages.StatsTiredFormat = gameData.messages.meta.stats_page.stats_tiredness;
            Messages.StatsGenderFormat = gameData.messages.meta.stats_page.stats_gender;
            Messages.StatsJewelFormat = gameData.messages.meta.stats_page.stats_equipped;
            Messages.StatsCompetitionGearFormat = gameData.messages.meta.stats_page.stats_competion_gear;

            Messages.CompetitionGearHeadFormat = gameData.messages.meta.stats_page.competition_gear.head_format;
            Messages.CompetitionGearBodyFormat = gameData.messages.meta.stats_page.competition_gear.body_format;
            Messages.CompetitionGearLegsFormat = gameData.messages.meta.stats_page.competition_gear.legs_format;
            Messages.CompetitionGearFeetFormat = gameData.messages.meta.stats_page.competition_gear.feet_format;

            Messages.StatsPrivateNotes = gameData.messages.meta.stats_page.stats_private_notes;
            Messages.StatsQuests = gameData.messages.meta.stats_page.stats_quests;
            Messages.StatsMinigameRanking = gameData.messages.meta.stats_page.stats_minigame_ranking;
            Messages.StatsAwards = gameData.messages.meta.stats_page.stats_awards;
            Messages.StatsMisc = gameData.messages.meta.stats_page.stats_misc;


            Messages.NoJewerlyEquipped = gameData.messages.meta.stats_page.msg.no_jewelry_equipped;
            Messages.NoCompetitionGear = gameData.messages.meta.stats_page.msg.no_competition_gear;

            // Transport

            Messages.CantAffordTransport = gameData.messages.transport.not_enough_money;
            Messages.WelcomeToAreaFormat = gameData.messages.transport.welcome_to_format;

            // Chat

            Messages.ChatViolationMessageFormat = gameData.messages.chat.violation_format;
            Messages.RequiredChatViolations = gameData.messages.chat.violation_points_required;

            Messages.GlobalChatFormatForModerators = gameData.messages.chat.for_others.global_format_moderator;
            Messages.DirectChatFormatForModerators = gameData.messages.chat.for_others.dm_format_moderator;


            Messages.HereChatFormat = gameData.messages.chat.for_others.here_format;
            Messages.IsleChatFormat = gameData.messages.chat.for_others.isle_format;
            Messages.NearChatFormat = gameData.messages.chat.for_others.near_format;
            Messages.GlobalChatFormat = gameData.messages.chat.for_others.global_format;
            Messages.AdsChatFormat = gameData.messages.chat.for_others.ads_format;
            Messages.DirectChatFormat = gameData.messages.chat.for_others.dm_format;
            Messages.BuddyChatFormat = gameData.messages.chat.for_others.friend_format;
            Messages.ModChatFormat = gameData.messages.chat.for_others.mod_format;
            Messages.AdminChatFormat = gameData.messages.chat.for_others.admin_format;

            Messages.HereChatFormatForSender = gameData.messages.chat.for_sender.here_format;
            Messages.IsleChatFormatForSender = gameData.messages.chat.for_sender.isle_format;
            Messages.NearChatFormatForSender = gameData.messages.chat.for_sender.near_format;
            Messages.BuddyChatFormatForSender = gameData.messages.chat.for_sender.friend_format;
            Messages.DirectChatFormatForSender = gameData.messages.chat.for_sender.dm_format;
            Messages.ModChatFormatForSender = gameData.messages.chat.for_sender.mod_format;
            Messages.AdminChatFormatForSender = gameData.messages.chat.for_sender.admin_format;
            

            Messages.PasswordNotice = gameData.messages.chat.password_included;
            Messages.CapsNotice = gameData.messages.chat.caps_notice;

            // Dropped Items

            Messages.NothingMessage = gameData.messages.meta.dropped_items.nothing_message;
            Messages.ItemsOnGroundMessage = gameData.messages.meta.dropped_items.items_message;
            Messages.GrabItemFormat = gameData.messages.meta.dropped_items.item_format;
            Messages.ItemInformationFormat = gameData.messages.meta.dropped_items.item_information_format;
            Messages.GrabAllItemsButton = gameData.messages.meta.dropped_items.grab_all;
            Messages.DroppedAnItemMessage = gameData.messages.dropped_items.dropped_item_message;
            Messages.GrabbedAllItemsMessage = gameData.messages.dropped_items.grab_all_message;
            Messages.GrabbedItemMessage = gameData.messages.dropped_items.grab_message;
            Messages.GrabAllItemsMessage = gameData.messages.dropped_items.grab_all_message;

            Messages.GrabbedAllItemsButInventoryFull = gameData.messages.dropped_items.grab_all_but_inv_full;
            Messages.GrabbedItemButInventoryFull = gameData.messages.dropped_items.grab_but_inv_full;

            // Tools
            Messages.BinocularsNothing = gameData.messages.tools.binoculars;
            Messages.MagnifyNothing = gameData.messages.tools.magnify;
            Messages.RakeNothing = gameData.messages.tools.rake;
            Messages.ShovelNothing = gameData.messages.tools.shovel;

            // Shop
            Messages.ThingsIAmSelling = gameData.messages.meta.shop.selling;
            Messages.ThingsYouSellMe = gameData.messages.meta.shop.sell_me;
            Messages.InfinitySign = gameData.messages.meta.shop.infinity;

            Messages.CantAfford1 = gameData.messages.shop.cant_afford_1;
            Messages.CantAfford5 = gameData.messages.shop.cant_afford_5;
            Messages.CantAfford25 = gameData.messages.shop.cant_afford_25;
            Messages.Brought1Format = gameData.messages.shop.brought_1;
            Messages.Brought5Format = gameData.messages.shop.brought_5;
            Messages.Brought25Format = gameData.messages.shop.brought_25;
            Messages.Sold1Format = gameData.messages.shop.sold_1;
            Messages.SoldAllFormat = gameData.messages.shop.sold_all;

            Messages.Brought1ButInventoryFull = gameData.messages.shop.brought_1_but_inv_full;
            Messages.Brought5ButInventoryFull = gameData.messages.shop.brought_5_but_inv_full;
            Messages.Brought25ButInventoryFull = gameData.messages.shop.brought_25_but_inv_full;

            // Meta Format

            Messages.LocationFormat = gameData.messages.meta.location_format;
            Messages.IsleFormat = gameData.messages.meta.isle_format;
            Messages.TownFormat = gameData.messages.meta.town_format;
            Messages.AreaFormat = gameData.messages.meta.area_format;
            Messages.Seperator = gameData.messages.meta.seperator;
            Messages.TileFormat = gameData.messages.meta.tile_format;
            Messages.TransportFormat = gameData.messages.meta.transport_format;
            Messages.ExitThisPlace = gameData.messages.meta.exit_this_place;
            Messages.BackToMap = gameData.messages.meta.back_to_map;
            Messages.LongFullLine = gameData.messages.meta.long_full_line;
            Messages.MetaTerminator = gameData.messages.meta.end_of_meta;

            Messages.NearbyPlayers = gameData.messages.meta.nearby.players_nearby;
            Messages.North = gameData.messages.meta.nearby.north;
            Messages.East = gameData.messages.meta.nearby.east;
            Messages.South = gameData.messages.meta.nearby.south;
            Messages.West = gameData.messages.meta.nearby.west;

            Messages.NoPitchforkMeta = gameData.messages.meta.hay_pile.no_pitchfork;
            Messages.HasPitchforkMeta = gameData.messages.meta.hay_pile.pitchfork;


            // Sec Codes

            Messages.InvalidSecCodeError = gameData.messages.sec_code.invalid_sec_code;
            Messages.YouEarnedAnItemFormat = gameData.messages.sec_code.item_earned;

            // Inventory

            Messages.InventoryHeaderFormat = gameData.messages.meta.inventory.header_format;
            Messages.InventoryItemFormat = gameData.messages.meta.inventory.item_entry;
            Messages.ShopEntryFormat = gameData.messages.meta.inventory.shop_entry;
            Messages.ItemInformationButton = gameData.messages.meta.inventory.item_info_button;
            Messages.ItemInformationByIdButton = gameData.messages.meta.inventory.item_info_itemid_button;

            Messages.ItemDropButton = gameData.messages.meta.inventory.item_drop_button;
            Messages.ItemThrowButton = gameData.messages.meta.inventory.item_throw_button;
            Messages.ItemConsumeButton = gameData.messages.meta.inventory.item_consume_button;
            Messages.ItemUseButton = gameData.messages.meta.inventory.item_use_button;
            Messages.ItemWearButton = gameData.messages.meta.inventory.item_wear_button;
            Messages.ItemReadButton = gameData.messages.meta.inventory.item_read_button;

            Messages.ShopBuyButton = gameData.messages.meta.inventory.buy_button;
            Messages.ShopBuy5Button = gameData.messages.meta.inventory.buy_5_button;
            Messages.ShopBuy25Button = gameData.messages.meta.inventory.buy_25_button;

            Messages.SellButton = gameData.messages.meta.inventory.sell_button;
            Messages.SellAllButton = gameData.messages.meta.inventory.sell_all_button;
            // Npc

            Messages.NpcStartChatFormat = gameData.messages.meta.npc.start_chat_format;
            Messages.NpcChatpointFormat = gameData.messages.meta.npc.chatpoint_format;
            Messages.NpcReplyFormat = gameData.messages.meta.npc.reply_format;
            Messages.NpcTalkButton = gameData.messages.meta.npc.npc_talk_button;
            Messages.NpcInformationButton = gameData.messages.meta.npc.npc_information_button;
            Messages.NpcInformationFormat = gameData.messages.meta.npc.npc_information_format;

            // Map Data

            Map.OverlayTileDepth = gameData.tile_paramaters.overlay_tiles.tile_depth.ToObject<int[]>();

            List<Map.TerrainTile> terrainTiles = new List<Map.TerrainTile>();
            int totalTerrainTiles = gameData.tile_paramaters.terrain_tiles.Count;
            for(int i = 0; i < totalTerrainTiles; i++)
            {
                Map.TerrainTile tile = new Map.TerrainTile();
                tile.Passable = gameData.tile_paramaters.terrain_tiles[i].passable;
                tile.Type = gameData.tile_paramaters.terrain_tiles[i].tile_type;
                Logger.DebugPrint("Registered Tile: " + i + " Passable: " + tile.Passable + " Type: " + tile.Type);
                terrainTiles.Add(tile);
            }
            Map.TerrainTiles = terrainTiles.ToArray();
            
            // Disconnect Reasons

            Messages.BanMessage = gameData.messages.disconnect.banned;
            Messages.IdleKickMessageFormat = gameData.messages.disconnect.client_timeout.kick_message;
            Messages.IdleWarningFormat = gameData.messages.disconnect.client_timeout.warn_message;
            Messages.DuplicateLogin = gameData.messages.disconnect.dupe_login;

            Chat.PrivateMessageSound = gameData.messages.chat.pm_sound;

            GameServer.IdleWarning = gameData.messages.disconnect.client_timeout.warn_after;
            GameServer.IdleTimeout = gameData.messages.disconnect.client_timeout.kick_after;

            // Inventory

            Messages.DefaultInventoryMax = gameData.item.max_carryable;
            Messages.EquipItemFormat = gameData.messages.meta.inventory.equip_format;

            // Click
            Messages.NothingInterestingHere = gameData.messages.click_nothing_message;

            // Swf
            Messages.WagonCutscene = gameData.transport.wagon_cutscene;
            Messages.BoatCutscene = gameData.transport.boat_cutscene;
            Messages.BallonCutscene = gameData.transport.ballon_cutscene;

        }

    }
}
