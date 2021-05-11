using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using HISP.Game;
using HISP.Game.Chat;
using HISP.Player;
using HISP.Game.Services;
using HISP.Game.SwfModules;
using HISP.Game.Horse;
using HISP.Game.Items;
using System.Globalization;
using HISP.Security;
using System;
using HISP.Game.Events;

namespace HISP.Server
{
    public class GameDataJson
    {

        public static void ReadGamedata()
        {
            if (!File.Exists(ConfigReader.GameDataFile))
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
            for (int i = 0; i < totalIsles; i++)
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

            int totalWaypoints = gameData.places.waypoints.Count;
            for (int i = 0; i < totalWaypoints; i++)
            {
                World.Waypoint waypoint = new World.Waypoint();
                waypoint.Name = gameData.places.waypoints[i].name;
                waypoint.PosX = gameData.places.waypoints[i].pos_x;
                waypoint.PosY = gameData.places.waypoints[i].pos_y;
                waypoint.Type = gameData.places.waypoints[i].type;
                waypoint.Description = gameData.places.waypoints[i].description;
                waypoint.WeatherTypesAvalible = gameData.places.waypoints[i].weather_avalible.ToObject<string[]>();
                Logger.DebugPrint("Registered Waypoint: " + waypoint.PosX.ToString() + ", " + waypoint.PosY.ToString() + " TYPE: " + waypoint.Type);
                World.Waypoints.Add(waypoint);
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
                if (gameData.places.special_tiles[i].exit_x != null)
                    specialTile.ExitX = gameData.places.special_tiles[i].exit_x;
                if (gameData.places.special_tiles[i].exit_x != null)
                    specialTile.ExitY = gameData.places.special_tiles[i].exit_y;
                specialTile.AutoplaySwf = gameData.places.special_tiles[i].autoplay_swf;
                specialTile.TypeFlag = gameData.places.special_tiles[i].type_flag;

                Logger.DebugPrint("Registered Special Tile: " + specialTile.Title + " X " + specialTile.X + " Y: " + specialTile.Y);
                World.SpecialTiles.Add(specialTile);
            }

            // Register Filter Reasons
            int totalReasons = gameData.messages.chat.reason_messages.Count;
            for (int i = 0; i < totalReasons; i++)
            {
                Chat.Reason reason = new Chat.Reason();
                reason.Name = gameData.messages.chat.reason_messages[i].name;
                reason.Message = gameData.messages.chat.reason_messages[i].message;
                Chat.Reasons.Add(reason);

                Logger.DebugPrint("Registered Chat Warning Reason: " + reason.Name + " (Message: " + reason.Message + ")");
            }
            // Register Filters

            int totalFilters = gameData.messages.chat.filter.Count;
            for (int i = 0; i < totalFilters; i++)
            {
                Chat.Filter filter = new Chat.Filter();
                filter.FilteredWord = gameData.messages.chat.filter[i].word;
                filter.MatchAll = gameData.messages.chat.filter[i].match_all;
                filter.Reason = Chat.GetReason((string)gameData.messages.chat.filter[i].reason_type);
                Chat.FilteredWords.Add(filter);

                Logger.DebugPrint("Registered Filtered Word: " + filter.FilteredWord + " With reason: " + filter.Reason.Name + " (Matching all: " + filter.MatchAll + ")");
            }

            // Register Corrections
            int totalCorrections = gameData.messages.chat.correct.Count;
            for (int i = 0; i < totalCorrections; i++)
            {
                Chat.Correction correction = new Chat.Correction();
                correction.FilteredWord = gameData.messages.chat.correct[i].word;
                correction.ReplacedWord = gameData.messages.chat.correct[i].new_word;
                Chat.CorrectedWords.Add(correction);

                Logger.DebugPrint("Registered Word Correction: " + correction.FilteredWord + " to " + correction.ReplacedWord);
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

                Logger.DebugPrint("Registered Transport Location: " + transportPlace.LocationTitle + " To Goto X: " + transportPlace.GotoX + " Y: " + transportPlace.GotoY);
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
                for (int ii = 0; ii < effectsCount; ii++)
                {
                    effectsList[ii] = new Item.Effects();
                    effectsList[ii].EffectsWhat = gameData.item.item_list[i].effects[ii].effect_what;
                    effectsList[ii].EffectAmount = gameData.item.item_list[i].effects[ii].effect_amount;
                }

                item.Effects = effectsList;
                item.SpawnParamaters = new Item.SpawnRules();
                item.SpawnParamaters.SpawnCap = gameData.item.item_list[i].spawn_parameters.spawn_cap;
                item.SpawnParamaters.SpawnInZone = gameData.item.item_list[i].spawn_parameters.spawn_in_area;
                item.SpawnParamaters.SpawnOnTileType = gameData.item.item_list[i].spawn_parameters.spawn_on_tile_type;
                item.SpawnParamaters.SpawnOnSpecialTile = gameData.item.item_list[i].spawn_parameters.spawn_on_special_tile;
                item.SpawnParamaters.SpawnNearSpecialTile = gameData.item.item_list[i].spawn_parameters.spawn_near_special_tile;

                Logger.DebugPrint("Registered Item ID: " + item.Id + " Name: " + item.Name + " spawns on: " + item.SpawnParamaters.SpawnOnTileType);
                Item.Items.Add(item);
            }
            // Register Throwables
            int totalThrowable = gameData.item.throwable.Count;
            for (int i = 0; i < totalThrowable; i++)
            {
                Item.ThrowableItem throwableItem = new Item.ThrowableItem();
                throwableItem.Id = gameData.item.throwable[i].id;
                throwableItem.HitMessage = gameData.item.throwable[i].message_hit;
                throwableItem.ThrowMessage = gameData.item.throwable[i].message_throw;
                throwableItem.HitYourselfMessage = gameData.item.throwable[i].message_hit_yourself;
                Item.ThrowableItems.Add(throwableItem);
            }

            // Register NPCs
            Logger.DebugPrint("Registering NPCS: ");
            int totalNpcs = gameData.npc_list.Count;
            for (int i = 0; i < totalNpcs; i++)
            {
                int id = gameData.npc_list[i].id;
                int x = gameData.npc_list[i].x;
                int y = gameData.npc_list[i].y;
                bool moves = gameData.npc_list[i].moves;

                int udlrStartX = 0;
                int udlrStartY = 0;

                if (gameData.npc_list[i].udlr_start_x != null)
                    udlrStartX = gameData.npc_list[i].udlr_start_x;
                if (gameData.npc_list[i].udlr_start_y != null)
                    udlrStartY = gameData.npc_list[i].udlr_start_y;

                Npc.NpcEntry npcEntry = new Npc.NpcEntry(id, x, y, moves, udlrStartX, udlrStartY);

                npcEntry.Name = gameData.npc_list[i].name;
                npcEntry.AdminDescription = gameData.npc_list[i].admin_description;
                npcEntry.ShortDescription = gameData.npc_list[i].short_description;
                npcEntry.LongDescription = gameData.npc_list[i].long_description;


                if (gameData.npc_list[i].stay_on != null)
                    npcEntry.StayOn = gameData.npc_list[i].stay_on;
                if (gameData.npc_list[i].requires_questid_completed != null)
                    npcEntry.RequiresQuestIdCompleted = gameData.npc_list[i].requires_questid_completed;
                if (gameData.npc_list[i].requires_questid_not_completed != null)
                    npcEntry.RequiresQuestIdNotCompleted = gameData.npc_list[i].requires_questid_not_completed;
                if (gameData.npc_list[i].udlr_script != null)
                    npcEntry.UDLRScript = gameData.npc_list[i].udlr_script;

                npcEntry.AdminOnly = gameData.npc_list[i].admin_only;
                npcEntry.LibarySearchable = gameData.npc_list[i].libary_searchable;
                npcEntry.IconId = gameData.npc_list[i].icon_id;

                Logger.DebugPrint("NPC ID:" + npcEntry.Id.ToString() + " NAME: " + npcEntry.Name);
                List<Npc.NpcChat> chats = new List<Npc.NpcChat>();
                int totalNpcChat = gameData.npc_list[i].chatpoints.Count;
                for (int ii = 0; ii < totalNpcChat; ii++)
                {
                    Npc.NpcChat npcChat = new Npc.NpcChat();
                    npcChat.Id = gameData.npc_list[i].chatpoints[ii].chatpoint_id;
                    npcChat.ChatText = gameData.npc_list[i].chatpoints[ii].chat_text;
                    npcChat.ActivateQuestId = gameData.npc_list[i].chatpoints[ii].activate_questid;

                    Logger.DebugPrint("CHATPOINT ID: " + npcChat.Id.ToString() + " TEXT: " + npcChat.ChatText);
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

                        Logger.DebugPrint("REPLY ID: " + npcReply.Id.ToString() + " TEXT: " + npcReply.ReplyText);
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
            for (int i = 0; i < totalQuests; i++)
            {
                Quest.QuestEntry quest = new Quest.QuestEntry();
                quest.Id = gameData.quest_list[i].id;
                quest.Notes = gameData.quest_list[i].notes;
                if (gameData.quest_list[i].title != null)
                    quest.Title = gameData.quest_list[i].title;
                quest.RequiresQuestIdCompleteStatsMenu = gameData.quest_list[i].requires_questid_statsmenu.ToObject<int[]>();
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
                for (int ii = 0; ii < itemsRequiredCount; ii++)
                {
                    Quest.QuestItemInfo itemInfo = new Quest.QuestItemInfo();
                    itemInfo.ItemId = gameData.quest_list[i].items_required[ii].item_id;
                    itemInfo.Quantity = gameData.quest_list[i].items_required[ii].quantity;
                    itmInfo.Add(itemInfo);
                }
                quest.ItemsRequired = itmInfo.ToArray();
                if (gameData.quest_list[i].fail_npc_chat != null)
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
                quest.SetNpcChatpoint = gameData.quest_list[i].set_npc_chatpoint;
                quest.GotoNpcChatpoint = gameData.quest_list[i].goto_npc_chatpoint;
                if (gameData.quest_list[i].warp_x != null)
                    quest.WarpX = gameData.quest_list[i].warp_x;
                if (gameData.quest_list[i].warp_y != null)
                    quest.WarpY = gameData.quest_list[i].warp_y;
                if (gameData.quest_list[i].success_message != null)
                    quest.SuccessMessage = gameData.quest_list[i].success_message;
                if (gameData.quest_list[i].success_npc_chat != null)
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
                Logger.DebugPrint("Registered Quest: " + quest.Id + " - " + quest.Title);
                Quest.QuestList.Add(quest);
            }

            int totalShops = gameData.shop_list.Count;
            for (int i = 0; i < totalShops; i++)
            {

                Shop shop = new Shop(gameData.shop_list[i].stocks_itemids.ToObject<int[]>());
                shop.Id = gameData.shop_list[i].id;
                shop.BuyPricePercentage = gameData.shop_list[i].buy_percent;
                shop.SellPricePercentage = gameData.shop_list[i].sell_percent;
                shop.BuysItemTypes = gameData.shop_list[i].buys_item_types.ToObject<string[]>();

                Logger.DebugPrint("Registered Shop ID: " + shop.Id + " Selling items at " + shop.SellPricePercentage + "% and buying at " + shop.BuyPricePercentage);
            }

            // Register awards

            int totalAwards = gameData.award_list.Count;
            Award.GlobalAwardList = new Award.AwardEntry[totalAwards];
            for (int i = 0; i < totalAwards; i++)
            {

                Award.AwardEntry award = new Award.AwardEntry();
                award.Id = i + 1;
                award.Sort = gameData.award_list[i].sort_by;
                award.Title = gameData.award_list[i].title;
                award.IconId = gameData.award_list[i].icon_id;
                award.MoneyBonus = gameData.award_list[i].earn_money;
                award.CompletionText = gameData.award_list[i].on_complete_text;
                award.Description = gameData.award_list[i].description;

                Award.GlobalAwardList[i] = award;

                Logger.DebugPrint("Registered Award ID: " + award.Id + " - " + award.Title);
            }

            // Register Abuse Report Reasons

            int totalAbuseReportReasons = gameData.messages.meta.abuse_report.reasons.Count;
            for (int i = 0; i < totalAbuseReportReasons; i++)
            {
                AbuseReport.ReportReason reason = new AbuseReport.ReportReason();
                reason.Id = gameData.messages.meta.abuse_report.reasons[i].id;
                reason.Name = gameData.messages.meta.abuse_report.reasons[i].name;
                reason.Meta = gameData.messages.meta.abuse_report.reasons[i].meta;
                AbuseReport.AddReason(reason);
                Logger.DebugPrint("Registered Abuse Report Reason: " + reason.Name);
            }

            // Map Data

            Map.OverlayTileDepth = gameData.tile_paramaters.overlay_tiles.tile_depth.ToObject<int[]>();

            List<Map.TerrainTile> terrainTiles = new List<Map.TerrainTile>();
            int totalTerrainTiles = gameData.tile_paramaters.terrain_tiles.Count;
            for (int i = 0; i < totalTerrainTiles; i++)
            {
                Map.TerrainTile tile = new Map.TerrainTile();
                tile.Passable = gameData.tile_paramaters.terrain_tiles[i].passable;
                tile.Type = gameData.tile_paramaters.terrain_tiles[i].tile_type;
                Logger.DebugPrint("Registered Tile: " + i + " Passable: " + tile.Passable + " Type: " + tile.Type);
                terrainTiles.Add(tile);
            }
            Map.TerrainTiles = terrainTiles.ToArray();

            // Register Abuse Report Reasons

            int totalInns = gameData.inns.Count;
            for (int i = 0; i < totalInns; i++)
            {
                int id = gameData.inns[i].id;
                int[] restsOffered = gameData.inns[i].rests_offered.ToObject<int[]>();
                int[] mealsOffered = gameData.inns[i].meals_offered.ToObject<int[]>();
                int buyPercent = gameData.inns[i].buy_percent;
                Inn inn = new Inn(id, restsOffered, mealsOffered, buyPercent);

                Logger.DebugPrint("Registered Inn: " + inn.Id + " Buying at: " + inn.BuyPercentage.ToString() + "%!");
            }

            int totalPoets = gameData.poetry.Count;
            for (int i = 0; i < totalPoets; i++)
            {
                Brickpoet.PoetryEntry entry = new Brickpoet.PoetryEntry();
                entry.Id = gameData.poetry[i].id;
                entry.Word = gameData.poetry[i].word;
                entry.Room = gameData.poetry[i].room_id;
                Brickpoet.PoetList.Add(entry);

                Logger.DebugPrint("Registered poet: " + entry.Id.ToString() + " word: " + entry.Word + " in room " + entry.Room.ToString());
            }

            // Register Horse Breeds
            int totalBreeds = gameData.horses.breeds.Count;
            for (int i = 0; i < totalBreeds; i++)
            {
                HorseInfo.Breed horseBreed = new HorseInfo.Breed();

                horseBreed.Id = gameData.horses.breeds[i].id;
                horseBreed.Name = gameData.horses.breeds[i].name;
                horseBreed.Description = gameData.horses.breeds[i].description;

                int speed = gameData.horses.breeds[i].base_stats.speed;
                int strength = gameData.horses.breeds[i].base_stats.strength;
                int conformation = gameData.horses.breeds[i].base_stats.conformation;
                int agility = gameData.horses.breeds[i].base_stats.agility;
                int inteligence = gameData.horses.breeds[i].base_stats.inteligence;
                int endurance = gameData.horses.breeds[i].base_stats.endurance;
                int personality = gameData.horses.breeds[i].base_stats.personality;
                int height = gameData.horses.breeds[i].base_stats.height;
                horseBreed.BaseStats = new HorseInfo.AdvancedStats(null, speed, strength, conformation, agility, inteligence, endurance, personality, height);
                horseBreed.BaseStats.MinHeight = gameData.horses.breeds[i].base_stats.min_height;
                horseBreed.BaseStats.MaxHeight = gameData.horses.breeds[i].base_stats.max_height;

                horseBreed.Colors = gameData.horses.breeds[i].colors.ToObject<string[]>();
                horseBreed.SpawnOn = gameData.horses.breeds[i].spawn_on;
                horseBreed.SpawnInArea = gameData.horses.breeds[i].spawn_area;
                horseBreed.Swf = gameData.horses.breeds[i].swf;
                horseBreed.Type = gameData.horses.breeds[i].type;

                HorseInfo.Breeds.Add(horseBreed);
                Logger.DebugPrint("Registered Horse Breed: #" + horseBreed.Id + ": " + horseBreed.Name);
            }
            // Register Breed Prices @ Pawneer Order
            int totalBreedPrices = gameData.horses.pawneer_base_price.Count;
            for (int i = 0; i < totalBreedPrices; i++)
            {
                int id = gameData.horses.pawneer_base_price[i].breed_id;
                int price = gameData.horses.pawneer_base_price[i].price;
                Pawneer pawneerPricing = new Pawneer(id, price);
                Pawneer.PawneerPriceModels.Add(pawneerPricing);
                Logger.DebugPrint("Registered Pawneer Base Price " + pawneerPricing.BreedId + " for $" + pawneerPricing.BasePrice.ToString("N0", CultureInfo.InvariantCulture));
            }

            int totalCategories = gameData.horses.categorys.Count;
            for (int i = 0; i < totalCategories; i++)
            {
                HorseInfo.Category category = new HorseInfo.Category();
                category.Name = gameData.horses.categorys[i].name;
                category.MetaOthers = gameData.horses.categorys[i].message_others;
                category.Meta = gameData.horses.categorys[i].message;
                HorseInfo.HorseCategories.Add(category);
                Logger.DebugPrint("Registered horse category type: " + category.Name);
            }
            int totalTrackedItems = gameData.messages.meta.misc_stats.tracked_items.Count;
            for (int i = 0; i < totalTrackedItems; i++)
            {
                Tracking.TrackedItemStatsMenu trackedItem = new Tracking.TrackedItemStatsMenu();
                trackedItem.What = gameData.messages.meta.misc_stats.tracked_items[i].id;
                trackedItem.Value = gameData.messages.meta.misc_stats.tracked_items[i].value;
                Tracking.TrackedItemsStatsMenu.Add(trackedItem);
                Logger.DebugPrint("Registered Tracked Item: " + trackedItem.What + " value: " + trackedItem.Value);
            }
            // Register Services

            int totalVets = gameData.services.vet.price_multipliers.Count;
            for (int i = 0; i < totalVets; i++)
            {
                double cost = gameData.services.vet.price_multipliers[i].cost;
                int id = gameData.services.vet.price_multipliers[i].id;
                Vet vet = new Vet(id, cost);
                Logger.DebugPrint("Registered Vet: " + vet.Id + " selling at: " + vet.PriceMultiplier.ToString(CultureInfo.InvariantCulture));
            }

            int totalGroomers = gameData.services.groomer.price_multipliers.Count;
            for (int i = 0; i < totalGroomers; i++)
            {
                double cost = gameData.services.groomer.price_multipliers[i].cost;
                int id = gameData.services.groomer.price_multipliers[i].id;
                int max = gameData.services.groomer.price_multipliers[i].max;
                Groomer groomer = new Groomer(id, cost, max);
                Logger.DebugPrint("Registered Groomer: " + groomer.Id + " selling at: " + groomer.PriceMultiplier.ToString(CultureInfo.InvariantCulture));
            }

            int totalFarriers = gameData.services.farrier.price_multipliers.Count;
            for (int i = 0; i < totalFarriers; i++)
            {
                int id = gameData.services.farrier.price_multipliers[i].id;
                int steel = gameData.services.farrier.price_multipliers[i].steel;
                int steelcost = gameData.services.farrier.price_multipliers[i].steel_cost;
                int iron = gameData.services.farrier.price_multipliers[i].iron;
                int ironcost = gameData.services.farrier.price_multipliers[i].iron_cost;

                Farrier farrier = new Farrier(id, steel, steelcost, iron, ironcost);
                Logger.DebugPrint("Registered Farrier: " + farrier.Id);
            }

            int totalBarns = gameData.services.barn.price_multipliers.Count;
            for (int i = 0; i < totalBarns; i++)
            {
                int id = gameData.services.barn.price_multipliers[i].id;
                double tired_cost = gameData.services.barn.price_multipliers[i].tired_cost;
                double hunger_cost = gameData.services.barn.price_multipliers[i].hunger_cost;
                double thirst_cost = gameData.services.barn.price_multipliers[i].thirst_cost;


                Barn barn = new Barn(id, tired_cost, hunger_cost, thirst_cost);
                Logger.DebugPrint("Registered Barn: " + barn.Id);
            }


            // Register Libary Books
            int totalBooks = gameData.books.Count;
            for (int i = 0; i < totalBooks; i++)
            {
                int id = gameData.books[i].id;
                string author = gameData.books[i].author;
                string title = gameData.books[i].title;
                string text = gameData.books[i].text;
                Book book = new Book(id, title, author, text);
                Logger.DebugPrint("Registered Libary Book: " + book.Id + " " + book.Title + " by " + book.Author);

            }

            // Register Crafts
            int totalWorkshops = gameData.workshop.Count;
            for (int i = 0; i < totalWorkshops; i++)
            {
                Workshop wkShop = new Workshop();
                wkShop.X = gameData.workshop[i].pos_x;
                wkShop.Y = gameData.workshop[i].pos_y;
                int totalCraftableItems = gameData.workshop[i].craftable_items.Count;
                for (int ii = 0; ii < totalCraftableItems; ii++)
                {
                    Workshop.CraftableItem craftableItem = new Workshop.CraftableItem();
                    craftableItem.Id = gameData.workshop[i].craftable_items[ii].id;
                    craftableItem.GiveItemId = gameData.workshop[i].craftable_items[ii].give_item;
                    craftableItem.MoneyCost = gameData.workshop[i].craftable_items[ii].money_cost;
                    int totalItemsRequired = gameData.workshop[i].craftable_items[ii].required_items.Count;
                    for (int iii = 0; iii < totalItemsRequired; iii++)
                    {
                        Workshop.RequiredItem requiredItem = new Workshop.RequiredItem();
                        requiredItem.RequiredItemId = gameData.workshop[i].craftable_items[ii].required_items[iii].req_item;
                        requiredItem.RequiredItemCount = gameData.workshop[i].craftable_items[ii].required_items[iii].req_quantity;
                        craftableItem.RequiredItems.Add(requiredItem);
                    }
                    wkShop.CraftableItems.Add(craftableItem);
                }

                Workshop.Workshops.Add(wkShop);
                Logger.DebugPrint("Registered Workshop at X: " + wkShop.X + " Y: " + wkShop.Y);

            }
            // Register Ranch Buildings
            int totalRanchBuildings = gameData.ranch.ranch_buildings.buildings.Count;
            for (int i = 0; i < totalRanchBuildings; i++)
            {
                int id = gameData.ranch.ranch_buildings.buildings[i].id;
                int cost = gameData.ranch.ranch_buildings.buildings[i].cost;
                string title = gameData.ranch.ranch_buildings.buildings[i].title;
                string description = gameData.ranch.ranch_buildings.buildings[i].description;

                Ranch.RanchBuilding building = new Ranch.RanchBuilding();

                building.Id = id;
                building.Cost = cost;
                building.Title = title;
                building.Description = description;

                Ranch.RanchBuilding.RanchBuildings.Add(building);
                Logger.DebugPrint("Registered Ranch Building: " + building.Title);

            }
            // Register Ranch Upgrades
            int totalRanchUpgrades = gameData.ranch.ranch_buildings.upgrades.Count;
            for (int i = 0; i < totalRanchUpgrades; i++)
            {
                int id = gameData.ranch.ranch_buildings.upgrades[i].id;
                int cost = gameData.ranch.ranch_buildings.upgrades[i].cost;
                string title = gameData.ranch.ranch_buildings.upgrades[i].title;
                string description = gameData.ranch.ranch_buildings.upgrades[i].description;

                Ranch.RanchUpgrade upgrade = new Ranch.RanchUpgrade();

                if (gameData.ranch.ranch_buildings.upgrades[i].limit != null)
                    upgrade.Limit = gameData.ranch.ranch_buildings.upgrades[i].limit;
                upgrade.Id = id;
                upgrade.Cost = cost;
                upgrade.Title = title;
                upgrade.Description = description;

                Ranch.RanchUpgrade.RanchUpgrades.Add(upgrade);
                Logger.DebugPrint("Registered Ranch Upgrade: " + upgrade.Title);

            }
            // Register Ranches
            int totalRanchLocations = gameData.ranch.ranch_locations.Count;
            for (int i = 0; i < totalRanchLocations; i++)
            {
                int x = gameData.ranch.ranch_locations[i].x;
                int y = gameData.ranch.ranch_locations[i].y;
                int id = gameData.ranch.ranch_locations[i].id;
                int value = gameData.ranch.ranch_locations[i].value;
                Ranch ranch = new Ranch(x, y, id, value);
                Ranch.Ranches.Add(ranch);
                Logger.DebugPrint("Registered Ranch id " + id + " at X: " + ranch.X + " Y: " + ranch.Y);

            }
            // Register Riddles
            int totalRiddles = gameData.riddle_room.Count;
            for (int i = 0; i < totalRiddles; i++)
            {
                int id = gameData.riddle_room[i].id;
                string riddle = gameData.riddle_room[i].riddle;
                string[] answers = gameData.riddle_room[i].answers.ToObject<string[]>();
                string reason = gameData.riddle_room[i].reason;
                Riddler riddlerRiddle = new Riddler(id, riddle, answers, reason);
                Logger.DebugPrint("Registered Riddler Riddle: " + riddlerRiddle.Riddle);

            }

            // Register BBCODE
            int totalBBocdes = gameData.bbcode.Count;
            for (int i = 0; i < totalBBocdes; i++)
            {
                string tag = gameData.bbcode[i].tag;
                string meta = gameData.bbcode[i].meta;
                BBCode code = new BBCode(tag, meta);
                Logger.DebugPrint("Registered BBCODE: " + code.Tag + " to " + code.MetaTranslation);
            }

            // Register Training Pens
            int totalTrainingPens = gameData.training_pens.Count;
            for (int i = 0; i < totalTrainingPens; i++)
            {
                Trainer trainer = new Trainer();
                trainer.Id = gameData.training_pens[i].trainer_id;
                trainer.ImprovesStat = gameData.training_pens[i].improves_stat;
                trainer.ImprovesAmount = gameData.training_pens[i].improves_amount;
                trainer.ThirstCost = gameData.training_pens[i].thirst_cost;
                trainer.MoodCost = gameData.training_pens[i].mood_cost;
                trainer.HungerCost = gameData.training_pens[i].hunger_cost;
                trainer.MoneyCost = gameData.training_pens[i].money_cost;
                trainer.ExperienceGained = gameData.training_pens[i].experience;
                Trainer.Trainers.Add(trainer);
                Logger.DebugPrint("Registered Training Pen: " + trainer.Id + " for " + trainer.ImprovesStat);
            }

            // Register Arenas
            int totalArenas = gameData.arena.Count;
            for (int i = 0; i < totalArenas; i++)
            {
                int arenaId = gameData.arena[i].arena_id;
                string arenaType = gameData.arena[i].arena_type;
                int arenaEntryCost = gameData.arena[i].entry_cost;
                int raceEvery = gameData.arena[i].race_every;
                int slots = gameData.arena[i].slots;
                int timeout = gameData.arena[i].timeout;

                Arena arena = new Arena(arenaId, arenaType, arenaEntryCost, raceEvery, slots, timeout);
                Logger.DebugPrint("Registered Arena: " + arena.Id.ToString() + " as " + arena.Type);
            }
            // Register Leaser
            int totalLeasers = gameData.leaser.Count;
            for (int i = 0; i < totalLeasers; i++)
            {
                int breedId = gameData.leaser[i].horse.breed;

                int saddle = -1;
                int saddlePad = -1;
                int bridle = -1;

                if (gameData.leaser[i].horse.tack.saddle != null)
                    saddle = gameData.leaser[i].horse.tack.saddle;

                if (gameData.leaser[i].horse.tack.saddle_pad != null)
                    saddlePad = gameData.leaser[i].horse.tack.saddle_pad;

                if (gameData.leaser[i].horse.tack.bridle != null)
                    bridle = gameData.leaser[i].horse.tack.bridle;

                Leaser leaser = new Leaser(breedId, saddle, saddlePad, bridle);
                leaser.LeaseId = gameData.leaser[i].lease_id;
                leaser.ButtonId = gameData.leaser[i].button_id;
                leaser.Info = gameData.leaser[i].info;
                leaser.OnLeaseText = gameData.leaser[i].on_lease;
                leaser.Price = gameData.leaser[i].price;
                leaser.Minutes = gameData.leaser[i].minutes;

                leaser.Color = gameData.leaser[i].horse.color;
                leaser.Gender = gameData.leaser[i].horse.gender;
                leaser.Height = gameData.leaser[i].horse.hands;
                leaser.Experience = gameData.leaser[i].horse.exp;
                leaser.HorseName = gameData.leaser[i].horse.name;

                leaser.Health = gameData.leaser[i].horse.basic_stats.health;
                leaser.Hunger = gameData.leaser[i].horse.basic_stats.hunger;
                leaser.Thirst = gameData.leaser[i].horse.basic_stats.thirst;
                leaser.Mood = gameData.leaser[i].horse.basic_stats.mood;
                leaser.Tiredness = gameData.leaser[i].horse.basic_stats.energy;
                leaser.Groom = gameData.leaser[i].horse.basic_stats.groom;
                leaser.Shoes = gameData.leaser[i].horse.basic_stats.shoes;

                leaser.Speed = gameData.leaser[i].horse.advanced_stats.speed;
                leaser.Strength = gameData.leaser[i].horse.advanced_stats.strength;
                leaser.Conformation = gameData.leaser[i].horse.advanced_stats.conformation;
                leaser.Agility = gameData.leaser[i].horse.advanced_stats.agility;
                leaser.Endurance = gameData.leaser[i].horse.advanced_stats.endurance;
                leaser.Inteligence = gameData.leaser[i].horse.advanced_stats.inteligence;
                leaser.Personality = gameData.leaser[i].horse.advanced_stats.personality;

                Leaser.HorseLeasers.Add(leaser);
                Logger.DebugPrint("Registered Leaser: " + leaser.LeaseId.ToString() + " For a " + leaser.HorseName);
            }

            // Register Socials
            int totalSocials = gameData.social_types.Count;
            for (int i = 0; i < totalSocials; i++)
            {
                string socialType = gameData.social_types[i].type;
                int totalSocialsOfType = gameData.social_types[i].socials.Count;
                for (int ii = 0; ii < totalSocialsOfType; ii++)
                {
                    SocialType.Social social = new SocialType.Social();

                    social.Id = gameData.social_types[i].socials[ii].social_id;
                    social.ButtonName = gameData.social_types[i].socials[ii].button_name;
                    social.ForSender = gameData.social_types[i].socials[ii].for_sender;
                    social.ForTarget = gameData.social_types[i].socials[ii].for_target;
                    social.ForEveryone = gameData.social_types[i].socials[ii].for_everyone;
                    social.SoundEffect = gameData.social_types[i].socials[ii].sound_effect;

                    SocialType.AddNewSocial(socialType, social);
                    Logger.DebugPrint("Registered Social: " + social.ButtonName);
                }
            }

            // Register Events : Real Time Riddle
            int totalRealTimeRiddles = gameData.events.real_time_riddle.Count;
            for (int i = 0; i < totalRealTimeRiddles; i++)
            {
                int id = gameData.events.real_time_riddle[i].id;
                string riddleText = gameData.events.real_time_riddle[i].text;
                string[] riddleAnswers = gameData.events.real_time_riddle[i].answers.ToObject<string[]>();
                int reward = gameData.events.real_time_riddle[i].money_reward;

                RealTimeRiddle riddle = new RealTimeRiddle(id, riddleText, riddleAnswers, reward);
                
                Logger.DebugPrint("Registered Riddle #" + riddle.RiddleId.ToString());
            }

            // Register Events : Real Time Quiz
            int totalRealTimeQuizCategories = gameData.events.real_time_quiz.Count;
            RealTimeQuiz.Categories = new RealTimeQuiz.QuizCategory[totalRealTimeQuizCategories]; // initalize array
            for (int i = 0; i < totalRealTimeQuizCategories; i++)
            {
                string name = gameData.events.real_time_quiz[i].name;
                int totalQuestions = gameData.events.real_time_quiz[i].questons.Count;

                RealTimeQuiz.QuizCategory quizCategory = new RealTimeQuiz.QuizCategory();
                quizCategory.Name = name;
                quizCategory.Questions = new RealTimeQuiz.QuizQuestion[totalQuestions];

                for(int ii = 0; ii < totalQuestions; ii++)
                {
                    quizCategory.Questions[ii] = new RealTimeQuiz.QuizQuestion(quizCategory);
                    quizCategory.Questions[ii].Question = gameData.events.real_time_quiz[i].questons[ii].question;
                    quizCategory.Questions[ii].Answers = gameData.events.real_time_quiz[i].questons[ii].answers.ToObject<string[]>();
                    Logger.DebugPrint("Registered Real Time Quiz Question: " + quizCategory.Questions[ii].Question);
                }

                RealTimeQuiz.Categories[i] = quizCategory;

                Logger.DebugPrint("Registered Real Time Quiz Category: " + name);
            }


            HorseInfo.HorseNames = gameData.horses.names.ToObject<string[]>();

            Item.Present = gameData.item.special.present;
            Item.MailMessage = gameData.item.special.mail_message;
            Item.DorothyShoes = gameData.item.special.dorothy_shoes;
            Item.PawneerOrder = gameData.item.special.pawneer_order;
            Item.Telescope = gameData.item.special.telescope;
            Item.Pitchfork = gameData.item.special.pitchfork;
            Item.WishingCoin = gameData.item.special.wishing_coin;
            Item.FishingPole = gameData.item.special.fishing_poll;
            Item.Earthworm = gameData.item.special.earthworm;
            Item.BirthdayToken = gameData.item.special.birthday_token;
            Item.WaterBalloon = gameData.item.special.water_balloon;
            Item.ModSplatterball = gameData.item.special.mod_splatterball;
            Item.MagicBean = gameData.item.special.magic_bean;
            Item.MagicDroplet = gameData.item.special.magic_droplet;

            Item.StallionTradingCard = gameData.item.special.stallion_trading_card;
            Item.MareTradingCard = gameData.item.special.mare_trading_card;
            Item.ColtTradingCard = gameData.item.special.colt_trading_card;
            Item.FillyTradingCard = gameData.item.special.filly_trading_card;

            GameServer.IdleWarning = Convert.ToInt32(gameData.messages.disconnect.client_timeout.warn_after);
            GameServer.IdleTimeout = Convert.ToInt32(gameData.messages.disconnect.client_timeout.kick_after);

            Chat.PrivateMessageSound = gameData.messages.chat.pm_sound;

            // New Users

            Messages.NewUserMessage = gameData.messages.new_user.starting_message;
            Map.NewUserStartX = gameData.messages.new_user.starting_x;
            Map.NewUserStartY = gameData.messages.new_user.starting_y;

            // Timed Messages

            Messages.PlaytimeMessageFormat = gameData.messages.timed_messages.playtime_message;
            Messages.RngMessages = gameData.messages.timed_messages.rng_message.ToObject<string[]>();

            // Auto Sell
            Messages.AutoSellNotStandingInSamePlace = gameData.messages.meta.auto_sell.not_standing_sameplace;
            Messages.AutoSellSuccessFormat = gameData.messages.meta.auto_sell.success;
            Messages.AutoSellInsufficentFunds = gameData.messages.meta.auto_sell.insufficent_money;
            Messages.AutoSellTooManyHorses = gameData.messages.meta.auto_sell.toomany_horses;
            Messages.AutoSellYouSoldHorseFormat = gameData.messages.meta.auto_sell.you_sold;
            Messages.AutoSellYouSoldHorseOfflineFormat = gameData.messages.meta.auto_sell.sold_offline;

            // Mute Command
            Messages.NowMutingPlayerFormat = gameData.messages.meta.mute_command.now_ignoring_player;
            Messages.StoppedMutingPlayerFormat = gameData.messages.meta.mute_command.stop_ignoring_player;

            Messages.PlayerIgnoringYourPrivateMessagesFormat = gameData.messages.meta.mute_command.player_ignoring_your_pm;
            Messages.PlayerIgnoringYourBuddyRequests = gameData.messages.meta.mute_command.player_ignoring_your_br;
            Messages.PlayerIgnoringYourSocials = gameData.messages.meta.mute_command.player_ignoring_your_socials;

            Messages.PlayerIgnoringAllPrivateMessagesFormat = gameData.messages.meta.mute_command.player_ignoring_all_pm;
            Messages.PlayerIgnoringAllBuddyRequests = gameData.messages.meta.mute_command.player_ignoring_all_br;
            Messages.PlayerIgnoringAllSocials = gameData.messages.meta.mute_command.player_ignoring_all_socials;

            Messages.CantSendInMutedChannel = gameData.messages.meta.mute_command.cant_send_in_muted_channel;
            Messages.CantSendBuddyRequestWhileMuted = gameData.messages.meta.mute_command.cant_send_br_muted;
            Messages.CantSendPrivateMessageWhileMuted = gameData.messages.meta.mute_command.cant_send_pm_muted;

            Messages.CantSendPrivateMessagePlayerMutedFormat = gameData.messages.meta.mute_command.cant_send_pm_player_muted;

            // Chat Errors
            Messages.CantFindPlayerToPrivateMessage = gameData.messages.chat_errors.cant_find_player;
            Messages.AdsOnlyOncePerMinute = gameData.messages.chat_errors.ads_once_per_minute;
            Messages.GlobalChatLimited = gameData.messages.chat_errors.global_chats_limited;

            // Warp Command

            Messages.SuccessfullyWarpedToPlayer = gameData.messages.commands.warp.player;
            Messages.SuccessfullyWarpedToLocation = gameData.messages.commands.warp.location;
            Messages.OnlyUnicornCanWarp = gameData.messages.commands.warp.only_unicorn;
            Messages.FailedToUnderstandLocation = gameData.messages.commands.warp.location_unknown;

            // Mod
            Messages.ModSplatterballEarnedYouFormat = gameData.messages.mods_revenge.awarded_you;
            Messages.ModSplatterballEarnedOtherFormat = gameData.messages.mods_revenge.awareded_others;
            Messages.ModIsleMessage = gameData.messages.commands.mod_isle.message;
            Map.ModIsleX = gameData.messages.commands.mod_isle.x;
            Map.ModIsleY = gameData.messages.commands.mod_isle.y;

            // Tag

            Messages.TagYourItFormat = gameData.messages.meta.player_interaction.tag.tag_player;
            Messages.TagOtherBuddiesOnlineFormat = gameData.messages.meta.player_interaction.tag.total_buddies;

            // Add Buddy

            Messages.AddBuddyPending = gameData.messages.meta.player_interaction.add_buddy.add_pending;
            Messages.AddBuddyOtherPendingFormat = gameData.messages.meta.player_interaction.add_buddy.other_pending;
            Messages.AddBuddyYourNowBuddiesFormat = gameData.messages.meta.player_interaction.add_buddy.add_confirmed; 
            Messages.AddBuddyDeleteBuddyFormat = gameData.messages.meta.player_interaction.add_buddy.deleted;

            // Socials

            Messages.SocialButton = gameData.messages.meta.player_interaction.socials.socials_button;
            Messages.SocialMessageFormat = gameData.messages.meta.player_interaction.socials.socials_message;
            Messages.SocialTypeFormat = gameData.messages.meta.player_interaction.socials.socials_menu_type;
            Messages.SocialPlayerNoLongerNearby = gameData.messages.meta.player_interaction.socials.no_longer_nearby;

            // Message Queue 
            Messages.MessageQueueHeader = gameData.messages.message_queue;

            // Events : Mods Revenge
            Messages.EventStartModsRevenge = gameData.messages.events.mods_revenge.event_start;
            Messages.EventEndModsRevenge = gameData.messages.events.mods_revenge.event_end;

            // Events : Isle Trading Game
            Messages.EventStartIsleTradingGame = gameData.messages.events.isle_card_trading_game.event_start;
            Messages.EventDisqualifiedIsleTradingGame = gameData.messages.events.isle_card_trading_game.event_disqualified;
            Messages.EventOnlyOneTypeIsleTradingGame = gameData.messages.events.isle_card_trading_game.event_one_type;
            Messages.EventOnlyTwoTypeIsleTradingGame = gameData.messages.events.isle_card_trading_game.event_two_type;
            Messages.EventOnlyThreeTypeIsleTradingGame = gameData.messages.events.isle_card_trading_game.event_three_type;
            Messages.EventNoneIsleTradingGame = gameData.messages.events.isle_card_trading_game.event_no_cards;
            Messages.EventWonIsleTradingGame = gameData.messages.events.isle_card_trading_game.event_win;

            // Events : Water Ballon Game
            Messages.EventStartWaterBallonGame = gameData.messages.events.water_balloon_game.event_start;
            Messages.EventWonWaterBallonGame = gameData.messages.events.water_balloon_game.event_won;
            Messages.EventEndWaterBalloonGame = gameData.messages.events.water_balloon_game.event_end;
            Messages.EventWinnerWaterBalloonGameFormat = gameData.messages.events.water_balloon_game.event_winner;

            // Events : Real Time Quiz

            Messages.EventMetaRealTimeQuizFormat = gameData.messages.events.real_time_quiz.event_meta;
            Messages.EventStartRealTimeQuiz = gameData.messages.events.real_time_quiz.event_start;
            Messages.EventEndRealTimeQuiz = gameData.messages.events.real_time_quiz.event_end;
            Messages.EventBonusRealTimeQuizFormat = gameData.messages.events.real_time_quiz.event_bonus;
            Messages.EventWinBonusRealTimeQuizFormat = gameData.messages.events.real_time_quiz.event_win_bonus;
            Messages.EventWinRealTimeQuizFormat = gameData.messages.events.real_time_quiz.event_win;
            Messages.EventUnavailableRealTimeQuiz = gameData.messages.events.real_time_quiz.event_unavailable;
            Messages.EventEnteredRealTimeQuiz = gameData.messages.events.real_time_quiz.event_entered;
            Messages.EventAlreadyEnteredRealTimeQuiz = gameData.messages.events.real_time_quiz.event_entered_already;
            Messages.EventQuitRealTimeQuiz = gameData.messages.events.real_time_quiz.event_quit;

            // Events : Real Time Riddle

            Messages.EventStartRealTimeRiddleFormat = gameData.messages.events.real_time_riddle.event_start;
            Messages.EventEndRealTimeRiddle = gameData.messages.events.real_time_riddle.event_end;
            Messages.EventWonRealTimeRiddleForOthersFormat = gameData.messages.events.real_time_riddle.event_won_others;
            Messages.EventWonRealTimeRiddleForYouFormat = gameData.messages.events.real_time_riddle.event_won_you;

            // Events : Tack Shop Giveaway

            Messages.EventStartTackShopGiveawayFormat = gameData.messages.events.tack_shop_giveaway.event_start;
            Messages.Event1MinTackShopGiveawayFormat = gameData.messages.events.tack_shop_giveaway.event_1min;
            Messages.EventWonTackShopGiveawayFormat = gameData.messages.events.tack_shop_giveaway.event_won;
            Messages.EventEndTackShopGiveawayFormat = gameData.messages.events.tack_shop_giveaway.event_end;


            // MultiHorses
            Messages.OtherPlayersHere = gameData.messages.meta.multihorses.other_players_here;
            Messages.MultiHorseSelectOneToJoinWith = gameData.messages.meta.multihorses.select_a_horse;
            Messages.MultiHorseFormat = gameData.messages.meta.multihorses.horse_format;

            // 2Player
            Messages.TwoPlayerOtherPlayer = gameData.messages.meta.two_player.other_player;
            Messages.TwoPlayerPlayerFormat = gameData.messages.meta.two_player.player_name;
            Messages.TwoPlayerInviteButton = gameData.messages.meta.two_player.invite_button;
            Messages.TwoPlayerAcceptButton = gameData.messages.meta.two_player.accept_button;
            Messages.TwoPlayerSentInvite = gameData.messages.meta.two_player.sent_invite;
            Messages.TwoPlayerPlayingWithFormat = gameData.messages.meta.two_player.playing_with;

            Messages.TwoPlayerGameInProgressFormat = gameData.messages.meta.two_player.game_in_progress;

            Messages.TwoPlayerYourInvitedFormat = gameData.messages.meta.two_player.your_invited;
            Messages.TwoPlayerInvitedFormat = gameData.messages.meta.two_player.you_invited;
            Messages.TwoPlayerStartingUpGameFormat = gameData.messages.meta.two_player.starting_game;

            Messages.TwoPlayerGameClosed = gameData.messages.meta.two_player.game_closed;
            Messages.TwoPlayerGameClosedOther = gameData.messages.meta.two_player.game_closed_other;

            Messages.TwoPlayerRecordedWinFormat = gameData.messages.meta.two_player.recorded_win;
            Messages.TwoPlayerRecordedLossFormat = gameData.messages.meta.two_player.recorded_loss;

            // Trade

            Messages.TradeWithPlayerFormat = gameData.messages.meta.player_interaction.trade.trading_with;

            Messages.TradeWaitingForOtherDone = gameData.messages.meta.player_interaction.trade.trade_wait_for_done;
            Messages.TradeOtherPlayerIsDone = gameData.messages.meta.player_interaction.trade.other_player_is_done;
            Messages.TradeFinalReview = gameData.messages.meta.player_interaction.trade.final_review;

            Messages.TradeYourOfferingFormat = gameData.messages.meta.player_interaction.trade.you_offering;

            Messages.TradeAddItems = gameData.messages.meta.player_interaction.trade.add_items;
            Messages.TradeOtherOfferingFormat = gameData.messages.meta.player_interaction.trade.other_offering;

            Messages.TradeWhenDoneClick = gameData.messages.meta.player_interaction.trade.when_done_click;
            Messages.TradeCancelAnytime = gameData.messages.meta.player_interaction.trade.cancel_anytime;
            Messages.TradeAcceptTrade = gameData.messages.meta.player_interaction.trade.accept_trade;

            Messages.TradeOfferingNothing = gameData.messages.meta.player_interaction.trade.offering_nothing;
            Messages.TradeOfferingMoneyFormat = gameData.messages.meta.player_interaction.trade.offering_money;
            Messages.TradeOfferingItemFormat = gameData.messages.meta.player_interaction.trade.offering_item;
            Messages.TradeOfferingHorseFormat = gameData.messages.meta.player_interaction.trade.offering_horse;

            // Trading : What to offer

            Messages.TradeWhatToOfferFormat = gameData.messages.meta.player_interaction.trade.what_to_offer;
            Messages.TradeOfferMoney = gameData.messages.meta.player_interaction.trade.offer_money;

            Messages.TradeOfferHorse = gameData.messages.meta.player_interaction.trade.offer_horse;
            Messages.TradeOfferHorseFormat = gameData.messages.meta.player_interaction.trade.offer_horse_format;
            Messages.TradeOfferHorseTacked = gameData.messages.meta.player_interaction.trade.horse_tacked;

            Messages.TradeOfferItem = gameData.messages.meta.player_interaction.trade.offer_object;
            Messages.TradeOfferItemFormat = gameData.messages.meta.player_interaction.trade.offer_object_format;
            Messages.TradeOfferItemOtherPlayerInvFull = gameData.messages.meta.player_interaction.trade.offer_object_inv_full;

            // Trading : Offer Submenu

            Messages.TradeMoneyOfferSubmenuFormat = gameData.messages.meta.player_interaction.trade.money_offer_submenu;
            Messages.TradeItemOfferSubmenuFormat = gameData.messages.meta.player_interaction.trade.object_offer_submenu;

            // Trading : Messges

            Messages.TradeWaitingForOthersToAcceptMessage = gameData.messages.meta.player_interaction.trade.waiting_for_other_to_accept;
            Messages.TradeRequiresBothPlayersMessage = gameData.messages.meta.player_interaction.trade.requires_both_players;

            Messages.TradeItemOfferAtleast1 = gameData.messages.meta.player_interaction.trade.object_offer_atleast_1;
            Messages.TradeItemOfferTooMuchFormat = gameData.messages.meta.player_interaction.trade.object_offer_too_much;
            Messages.TradeMoneyOfferTooMuch = gameData.messages.meta.player_interaction.trade.money_offer_too_much;

            Messages.TradeOtherPlayerHasNegativeMoney = gameData.messages.meta.player_interaction.trade.other_player_has_negative_money;
            Messages.TradeYouHaveNegativeMoney = gameData.messages.meta.player_interaction.trade.you_have_negative_money;


            Messages.TradeAcceptedMessage = gameData.messages.meta.player_interaction.trade.trade_accepted;
            Messages.TradeCanceledByYouMessage = gameData.messages.meta.player_interaction.trade.you_canceled;
            Messages.TradeCanceledByOtherPlayerFormat = gameData.messages.meta.player_interaction.trade.other_canceled;
            Messages.TradeCanceledBecuasePlayerMovedMessage = gameData.messages.meta.player_interaction.trade.trade_canceled_moved;
            Messages.TradeCanceledInterupted = gameData.messages.meta.player_interaction.trade.trade_interupted;

            Messages.TradeRiddenHorse = gameData.messages.meta.player_interaction.trade.trade_ridden_horse;

            Messages.TradeYouCantHandleMoreHorses = gameData.messages.meta.player_interaction.trade.cant_handle_more_horses;
            Messages.TradeOtherPlayerCantHandleMoreHorsesFormat = gameData.messages.meta.player_interaction.trade.other_player_cant_handle_more_horses;

            Messages.TradeOtherCantCarryMoreItems = gameData.messages.meta.player_interaction.trade.other_carry_more;
            Messages.TradeYouCantCarryMoreItems = gameData.messages.meta.player_interaction.trade.you_cant_carry_more;

            Messages.TradeYouSpentMoneyMessageFormat = gameData.messages.meta.player_interaction.trade.trade_spent;
            Messages.TradeYouReceivedMoneyMessageFormat = gameData.messages.meta.player_interaction.trade.trade_received;

            Messages.TradeNotAllowedWhileBidding = gameData.messages.meta.player_interaction.trade.trade_not_allowed_while_bidding;
            Messages.TradeNotAllowedWhileOtherBidding = gameData.messages.meta.player_interaction.trade.trade_not_allowed_while_other_is_bidding;

            // Player Interation

            Messages.PlayerHereMenuFormat = gameData.messages.meta.player_interaction.menu;

            Messages.PlayerHereProfileButton = gameData.messages.meta.player_interaction.profiile_button;
            Messages.PlayerHereSocialButton = gameData.messages.meta.player_interaction.social_button;
            Messages.PlayerHereTradeButton = gameData.messages.meta.player_interaction.trade_button;
            Messages.PlayerHereAddBuddyButton = gameData.messages.meta.player_interaction.buddy_button;
            Messages.PlayerHereTagButton = gameData.messages.meta.player_interaction.tag_button;
            Messages.PlayerHerePmButton = gameData.messages.meta.player_interaction.pm_button;


            // Auction
            Messages.AuctionsRunning = gameData.messages.meta.auction.auctions_running;
            Messages.AuctionPlayersHereFormat = gameData.messages.meta.auction.players_here;
            Messages.AuctionHorseEntryFormat = gameData.messages.meta.auction.auction_horse_entry;
            Messages.AuctionAHorse = gameData.messages.meta.auction.auction_horse;

            Messages.AuctionListHorse = gameData.messages.meta.auction.list_horse;
            Messages.AuctionHorseListEntryFormat = gameData.messages.meta.auction.horse_list_entry;
            Messages.AuctionHorseViewButton = gameData.messages.meta.auction.view_button;
            Messages.AuctionHorseIsTacked = gameData.messages.meta.auction.tacked;

            Messages.AuctionBidMax = gameData.messages.meta.auction.max_bid;
            Messages.AuctionBidRaisedFormat = gameData.messages.meta.auction.bid_raised;
            Messages.AuctionTopBid = gameData.messages.meta.auction.top_bid;
            Messages.AuctionExistingBidHigher = gameData.messages.meta.auction.existing_higher;

            Messages.AuctionYouHaveTooManyHorses = gameData.messages.meta.auction.you_have_too_many_horses;
            Messages.AuctionOnlyOneWinningBidAllowed = gameData.messages.meta.auction.only_one_winning_bid_allowed;

            Messages.AuctionOneHorsePerPlayer = gameData.messages.meta.auction.one_horse_at_a_time;
            Messages.AuctionYouveBeenOutbidFormat = gameData.messages.meta.auction.outbid_by;
            Messages.AuctionCantAffordBid = gameData.messages.meta.auction.cant_afford_bid;
            Messages.AuctionCantAffordAuctionFee = gameData.messages.meta.auction.cant_afford_listing;
            Messages.AuctionNoOtherTransactionAllowed = gameData.messages.meta.auction.no_other_transaction_allowed;

            Messages.AuctionYouBroughtAHorseFormat = gameData.messages.meta.auction.brought_horse;
            Messages.AuctionNoHorseBrought = gameData.messages.meta.auction.no_one_brought;
            Messages.AuctionHorseSoldFormat = gameData.messages.meta.auction.horse_sold;
            
            Messages.AuctionSoldToFormat = gameData.messages.meta.auction.sold_to;
            Messages.AuctionNotSold = gameData.messages.meta.auction.not_sold;
            Messages.AuctionGoingToFormat = gameData.messages.meta.auction.going_to;

            // Hammock Text
            Messages.HammockText = gameData.messages.meta.hammock;

            // Horse Leaser
            Messages.HorseLeaserCantAffordMessage = gameData.messages.horse_leaser.cant_afford;
            Messages.HorseLeaserTemporaryHorseAdded = gameData.messages.horse_leaser.temporary_horse_added;
            Messages.HorseLeaserHorsesFull = gameData.messages.horse_leaser.horses_full;

            Messages.HorseLeaserReturnedToUniterPegasus = gameData.messages.horse_leaser.returned_to_uniter_pegasus;

            Messages.HorseLeaserReturnedToUniterFormat = gameData.messages.horse_leaser.returned_to_uniter;
            Messages.HorseLeaserReturnedToOwnerFormat = gameData.messages.horse_leaser.returned_to_owner;

            // Competitions
            Messages.ArenaResultsMessage = gameData.messages.meta.arena.results;
            Messages.ArenaPlacingFormat = gameData.messages.meta.arena.placing;
            Messages.ArenaAlreadyEntered = gameData.messages.meta.arena.already_entered;

            Messages.ArenaFirstPlace = gameData.messages.meta.arena.first_place;
            Messages.ArenaSecondPlace = gameData.messages.meta.arena.second_place;
            Messages.ArenaThirdPlace = gameData.messages.meta.arena.thirst_place;
            Messages.ArenaFourthPlace = gameData.messages.meta.arena.fourth_place;
            Messages.ArenaFifthPlace = gameData.messages.meta.arena.fifth_place;
            Messages.ArenaSixthPlace = gameData.messages.meta.arena.sixth_place;

            Messages.ArenaEnteredInto = gameData.messages.meta.arena.enter_into;
            Messages.ArenaCantAfford = gameData.messages.meta.arena.cant_afford;

            Messages.ArenaYourScoreFormat = gameData.messages.meta.arena.your_score;

            Messages.ArenaJumpingStartup = gameData.messages.meta.arena.jumping_start_up;
            Messages.ArenaDraftStartup = gameData.messages.meta.arena.draft_start_up;
            Messages.ArenaRacingStartup = gameData.messages.meta.arena.racing_start_up;
            Messages.ArenaConformationStartup = gameData.messages.meta.arena.conformation_start_up;

            Messages.ArenaYouWinFormat = gameData.messages.meta.arena.winner;
            Messages.ArenaOnlyWinnerWins = gameData.messages.meta.arena.only_winner_wins;

            Messages.ArenaTooHungry = gameData.messages.meta.arena.too_hungry;
            Messages.ArenaTooThirsty = gameData.messages.meta.arena.too_thisty;
            Messages.ArenaNeedsFarrier = gameData.messages.meta.arena.farrier;
            Messages.ArenaTooTired = gameData.messages.meta.arena.too_tired;
            Messages.ArenaNeedsVet = gameData.messages.meta.arena.needs_vet;

            Messages.ArenaEventNameFormat = gameData.messages.meta.arena.event_name;
            Messages.ArenaCurrentlyTakingEntriesFormat = gameData.messages.meta.arena.currently_taking_entries;
            Messages.ArenaCompetitionInProgress = gameData.messages.meta.arena.competition_in_progress;
            Messages.ArenaYouHaveHorseEntered = gameData.messages.meta.arena.horse_entered;
            Messages.ArenaCompetitionFull = gameData.messages.meta.arena.competiton_full;

            Messages.ArenaEnterHorseFormat = gameData.messages.meta.arena.enter_horse;
            Messages.ArenaCurrentCompetitors = gameData.messages.meta.arena.current_competitors;
            Messages.ArenaCompetingHorseFormat = gameData.messages.meta.arena.competing_horses;

            // Horse Games
            Messages.HorseGamesSelectHorse = gameData.messages.meta.horse_games.select_a_horse;
            Messages.HorseGamesHorseEntryFormat = gameData.messages.meta.horse_games.horse_entry;

            // City Hall
            Messages.CityHallMenu = gameData.messages.meta.city_hall.menu;
            Messages.CityHallMailSendMeta = gameData.messages.meta.city_hall.mail_send_meta;

            Messages.CityHallSentMessageFormat = gameData.messages.meta.city_hall.sent_mail;
            Messages.CityHallCantAffordPostageMessage = gameData.messages.meta.city_hall.cant_afford_postage;
            Messages.CityHallCantFindPlayerMessageFormat = gameData.messages.meta.city_hall.cant_find_player;

            Messages.CityHallCheapestAutoSells = gameData.messages.meta.city_hall.auto_sell.top_100_cheapest;
            Messages.CityHallCheapestAutoSellHorseEntryFormat = gameData.messages.meta.city_hall.auto_sell.cheap_horse_entry;

            Messages.CityHallMostExpAutoSells = gameData.messages.meta.city_hall.auto_sell.top_50_most_exp;
            Messages.CityHallMostExpAutoSellHorseEntryFormat = gameData.messages.meta.city_hall.auto_sell.exp_horse_entry;

            Messages.CityHallTop25Ranches = gameData.messages.meta.city_hall.ranch_investment.top_25;
            Messages.CityHallRanchEntryFormat = gameData.messages.meta.city_hall.ranch_investment.ranch_entry;

            Messages.CityHallTop25Players = gameData.messages.meta.city_hall.richest_players.top_25;
            Messages.CityHallRichPlayerFormat = gameData.messages.meta.city_hall.richest_players.rich_player_format;

            Messages.CityHallTop100SpoiledHorses = gameData.messages.meta.city_hall.spoiled_horses.top_100;
            Messages.CityHallSpoiledHorseEntryFormat = gameData.messages.meta.city_hall.spoiled_horses.spoiled_horse_entry;

            Messages.CityHallTop25AdventurousPlayers = gameData.messages.meta.city_hall.most_adventurous_players.top_25;
            Messages.CityHallAdventurousPlayerEntryFormat = gameData.messages.meta.city_hall.most_adventurous_players.adventurous_player_entry;

            Messages.CityHallTop25ExperiencedPlayers = gameData.messages.meta.city_hall.most_experinced_players.top_25;
            Messages.CityHallExperiencePlayerEntryFormat = gameData.messages.meta.city_hall.most_experinced_players.experienced_player_entry;

            Messages.CityHallTop25MinigamePlayers = gameData.messages.meta.city_hall.most_active_minigame_players.top_25;
            Messages.CityHallMinigamePlayerEntryFormat = gameData.messages.meta.city_hall.most_active_minigame_players.minigame_player_entry;

            Messages.CityHallTop25ExperiencedHorses = gameData.messages.meta.city_hall.most_experienced_horses.top_25;
            Messages.CityHallExperiencedHorseEntryFormat = gameData.messages.meta.city_hall.most_experienced_horses.experienced_horse_entry;

            // Mail Messages
            Messages.MailReceivedMessage = gameData.messages.meta.mail.mail_received;
            Messages.MailSelectFromFollowing = gameData.messages.meta.mail.mail_select;
            Messages.MailSe = gameData.messages.meta.mail.mail_se;

            Messages.MailReadMetaFormat = gameData.messages.meta.mail.mail_read;
            Messages.MailEntryFormat = gameData.messages.meta.mail.mail_entry;
            Messages.MailRippedMessage = gameData.messages.meta.mail.mail_ripped;

            // Click
            Messages.ClickPlayerHereFormat = gameData.messages.player_here;


            // Ranch
            Messages.RanchUnownedRanchFormat = gameData.messages.meta.ranch.unowned_ranch;
            Messages.RanchYouCouldPurchaseThisRanch = gameData.messages.meta.ranch.you_could_purchase_this;
            Messages.RanchYouAllreadyOwnARanch = gameData.messages.meta.ranch.ranch_already_owned;
            Messages.RanchSubscribersOnly = gameData.messages.meta.ranch.sub_only;
            Messages.RanchDescriptionOthersFormat = gameData.messages.meta.ranch.ranch_desc_others;
            Messages.RanchUnownedRanchClicked = gameData.messages.meta.ranch.unowned_ranch_click;
            Messages.RanchClickMessageFormat = gameData.messages.meta.ranch.click_message;

            Messages.RanchDorothyShoesMessage = gameData.messages.meta.ranch.dorothy_message;
            Messages.RanchDorothyShoesPrisonIsleMessage = gameData.messages.meta.ranch.dorothy_prison_isle;

            Messages.RanchCantAffordRanch = gameData.messages.meta.ranch.ranch_buy_cannot_afford;
            Messages.RanchRanchBroughtMessageFormat = gameData.messages.meta.ranch.ranch_brought;
            Messages.RanchSavedRanchDescripton = gameData.messages.meta.ranch.saved_ranch;
            Messages.RanchDefaultRanchTitle = gameData.messages.meta.ranch.default_title;
            Messages.RanchEditDescriptionMetaFormat = gameData.messages.meta.ranch.edit_description;
            Messages.RanchTitleFormat = gameData.messages.meta.ranch.your_ranch_meta;
            Messages.RanchYourDescriptionFormat = gameData.messages.meta.ranch.view_desc;

            Messages.RanchSellAreYouSure = gameData.messages.meta.ranch.sell_confirm;
            Messages.RanchSoldFormat = gameData.messages.meta.ranch.sell_done;

            // Ranch : Breed

            Messages.RanchCanBuildOneOfTheFollowingInThisSpot = gameData.messages.meta.ranch.build.build_on_this_spot;
            Messages.RanchBuildingEntryFormat = gameData.messages.meta.ranch.build.build_format;
            Messages.RanchCantAffordThisBuilding = gameData.messages.meta.ranch.build.cannot_afford;
            Messages.RanchBuildingInformationFormat = gameData.messages.meta.ranch.build.information;
            Messages.RanchBuildingComplete = gameData.messages.meta.ranch.build.build_complete;
            Messages.RanchBuildingAlreadyHere = gameData.messages.meta.ranch.build.building_allready_placed;
            Messages.RanchTornDownRanchBuildingFormat = gameData.messages.meta.ranch.build.torn_down;
            Messages.RanchViewBuildingFormat = gameData.messages.meta.ranch.build.view_building;
            Messages.RanchBarnHorsesFormat = gameData.messages.meta.ranch.build.barn;

            // Ranch : Upgrades

            Messages.UpgradedMessage = gameData.messages.meta.ranch.upgrade.upgrade_message;
            Messages.UpgradeCannotAfford = gameData.messages.meta.ranch.upgrade.cannot_afford;
            Messages.UpgradeCurrentUpgradeFormat = gameData.messages.meta.ranch.upgrade.upgrade_meta;
            Messages.UpgradeNextUpgradeFormat = gameData.messages.meta.ranch.upgrade.you_could_upgrade;

            // Ranch : Special

            Messages.BuildingRestHere = gameData.messages.meta.ranch.special.rest_here;
            Messages.BuildingGrainSilo = gameData.messages.meta.ranch.special.grain_silo;
            Messages.BuildingBarnFormat = gameData.messages.meta.ranch.special.barn;
            Messages.BuildingBigBarnFormat = gameData.messages.meta.ranch.special.big_barn;
            Messages.BuildingGoldBarnFormat = gameData.messages.meta.ranch.special.gold_barn;
            Messages.BuildingWaterWell = gameData.messages.meta.ranch.special.water_well;
            Messages.BuildingWindmillFormat = gameData.messages.meta.ranch.special.windmills;
            Messages.BuildingWagon = gameData.messages.meta.ranch.special.wagon;
            Messages.BuildingTrainingPen = gameData.messages.meta.ranch.special.training_pen;
            Messages.BuildingVegatableGarden = gameData.messages.meta.ranch.special.vegatable_garden;

            Messages.RanchTrainAllAttempt = gameData.messages.meta.ranch.special.train_all;
            Messages.RanchTrainSuccessFormat = gameData.messages.meta.ranch.special.train_success;
            Messages.RanchTrainCantTrainFormat = gameData.messages.meta.ranch.special.train_cant_train;
            Messages.RanchTrainBadMoodFormat = gameData.messages.meta.ranch.special.train_bad_mood;
            Messages.RanchHorsesFullyRested = gameData.messages.meta.ranch.special.fully_rested;
            Messages.RanchWagonDroppedYouOff = gameData.messages.meta.ranch.special.wagon_used;

            // Treasure
            Messages.PirateTreasureFormat = gameData.messages.treasure.pirate_treasure;
            Messages.PotOfGoldFormat = gameData.messages.treasure.pot_of_gold;

            // Records
            Messages.ProfileSavedMessage = gameData.messages.profile_save;
            Messages.PrivateNotesSavedMessage = gameData.messages.private_notes_save;
            Messages.PrivateNotesMetaFormat = gameData.messages.meta.private_notes_format;


            // Announcements

            Messages.WelcomeFormat = gameData.messages.welcome_format;
            Messages.MotdFormat = gameData.messages.motd_format;
            Messages.LoginMessageFormat = gameData.messages.login_format;
            Messages.LogoutMessageFormat = gameData.messages.logout_format;

            // Pronoun
            Messages.PronounFemaleShe = gameData.messages.meta.stats_page.pronouns.female_she;
            Messages.PronounFemaleHer = gameData.messages.meta.stats_page.pronouns.female_her;

            Messages.PronounMaleHe = gameData.messages.meta.stats_page.pronouns.male_he;
            Messages.PronounMaleHis = gameData.messages.meta.stats_page.pronouns.male_his;

            Messages.PronounYouYour = gameData.messages.meta.stats_page.pronouns.you_your;

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

            Messages.JewelrySlot1Format = gameData.messages.meta.stats_page.jewelry.slot_1;
            Messages.JewelrySlot2Format = gameData.messages.meta.stats_page.jewelry.slot_2;
            Messages.JewelrySlot3Format = gameData.messages.meta.stats_page.jewelry.slot_3;
            Messages.JewelrySlot4Format = gameData.messages.meta.stats_page.jewelry.slot_4;

            Messages.JewelryRemoveSlot1Button = gameData.messages.meta.stats_page.jewelry.remove_slot_1;
            Messages.JewelryRemoveSlot2Button = gameData.messages.meta.stats_page.jewelry.remove_slot_2;
            Messages.JewelryRemoveSlot3Button = gameData.messages.meta.stats_page.jewelry.remove_slot_3;
            Messages.JewelryRemoveSlot4Button = gameData.messages.meta.stats_page.jewelry.remove_slot_4;

            Messages.CompetitionGearHeadFormat = gameData.messages.meta.stats_page.competition_gear.head_format;
            Messages.CompetitionGearBodyFormat = gameData.messages.meta.stats_page.competition_gear.body_format;
            Messages.CompetitionGearLegsFormat = gameData.messages.meta.stats_page.competition_gear.legs_format;
            Messages.CompetitionGearFeetFormat = gameData.messages.meta.stats_page.competition_gear.feet_format;

            Messages.CompetitionGearRemoveHeadButton = gameData.messages.meta.stats_page.competition_gear.remove_head;
            Messages.CompetitionGearRemoveBodyButton = gameData.messages.meta.stats_page.competition_gear.remove_body;
            Messages.CompetitionGearRemoveLegsButton = gameData.messages.meta.stats_page.competition_gear.remove_legs;
            Messages.CompetitionGearRemoveFeetButton = gameData.messages.meta.stats_page.competition_gear.remove_feet;

            Messages.StatsPrivateNotesButton = gameData.messages.meta.stats_page.stats_private_notes;
            Messages.StatsQuestsButton = gameData.messages.meta.stats_page.stats_quests;
            Messages.StatsMinigameRankingButton = gameData.messages.meta.stats_page.stats_minigame_ranking;
            Messages.StatsAwardsButton = gameData.messages.meta.stats_page.stats_awards;
            Messages.StatsMiscButton = gameData.messages.meta.stats_page.stats_misc;

            Messages.JewelrySelected = gameData.messages.meta.stats_page.msg.jewelry_selected;
            Messages.JewelrySelectedOther = gameData.messages.meta.stats_page.msg.jewelry_other;

            Messages.NoJewerlyEquipped = gameData.messages.meta.stats_page.msg.no_jewelry_equipped;
            Messages.NoJewerlyEquippedOther = gameData.messages.meta.stats_page.msg.no_jewelry_other;

            Messages.NoCompetitionGear = gameData.messages.meta.stats_page.msg.no_competition_gear;
            Messages.NoCompetitionGearOther = gameData.messages.meta.stats_page.msg.no_competition_gear_other;

            Messages.CompetitionGearSelected = gameData.messages.meta.stats_page.msg.competition_gear_selected;
            Messages.CompetitionGearSelectedOther = gameData.messages.meta.stats_page.msg.competition_gear_other_selected;

            Messages.StatHunger = gameData.messages.meta.stats_page.hunger_stat_name;
            Messages.StatThirst = gameData.messages.meta.stats_page.thirst_stat_name;
            Messages.StatTired = gameData.messages.meta.stats_page.tired_stat_name;

            Messages.StatsOtherHorses = gameData.messages.meta.stats_page.msg.other_horses;

            Messages.StatPlayerFormats = gameData.messages.meta.stats_page.player_stats.ToObject<string[]>();

            // Misc Stats
            Messages.StatMiscHeader = gameData.messages.meta.misc_stats.header;
            Messages.StatMiscNoneRecorded = gameData.messages.meta.misc_stats.no_stats_recorded;
            Messages.StatMiscEntryFormat = gameData.messages.meta.misc_stats.stat_format;

            // Books (Libary)
            Messages.BooksOfHorseIsle = gameData.messages.meta.libary.books.books_of_horseisle;
            Messages.BookEntryFormat = gameData.messages.meta.libary.books.book_entry;
            Messages.BookReadFormat = gameData.messages.meta.libary.books.book_read;

            // Awards (Libary)
            Messages.AwardsAvalible = gameData.messages.meta.libary.awards.all_earnable_awards;
            Messages.AwardEntryFormat = gameData.messages.meta.libary.awards.award_entry;

            // Locations (Libary)
            Messages.LocationKnownIslands = gameData.messages.meta.libary.locations.known_islands;
            Messages.LocationKnownTowns = gameData.messages.meta.libary.locations.known_towns;
            Messages.LocationIslandFormat = gameData.messages.meta.libary.locations.isle_entry;
            Messages.LocationTownFormat = gameData.messages.meta.libary.locations.town_entry;
            Messages.LocationDescriptionFormat = gameData.messages.meta.libary.locations.location_description;

            // Minigame (Libary)
            Messages.MinigameSingleplayer = gameData.messages.meta.libary.minigames.singleplayer;
            Messages.MinigameTwoplayer = gameData.messages.meta.libary.minigames.twoplayer;
            Messages.MinigameMultiplayer = gameData.messages.meta.libary.minigames.multiplayer;
            Messages.MinigameCompetitions = gameData.messages.meta.libary.minigames.competitions;
            Messages.MinigameEntryFormat = gameData.messages.meta.libary.minigames.minigame_entry;

            // Companion (Libary)
            Messages.CompanionViewFormat = gameData.messages.meta.libary.companion.view_button;
            Messages.CompanionEntryFormat = gameData.messages.meta.libary.companion.entry_format;

            // Tack (Libary)
            Messages.TackViewSetFormat = gameData.messages.meta.libary.tack.view_tack_set;
            Messages.TackSetPeiceFormat = gameData.messages.meta.libary.tack.set_peice_format;

            // Groomer
            Messages.GroomerBestToHisAbilitiesFormat = gameData.messages.meta.groomer.groomed_best_it_can;
            Messages.GroomerCannotAffordMessage = gameData.messages.meta.groomer.cannot_afford_service;
            Messages.GroomerCannotImprove = gameData.messages.meta.groomer.cannot_improve;
            Messages.GroomerBestToHisAbilitiesALL = gameData.messages.meta.groomer.groomed_best_all;
            Messages.GroomerDontNeed = gameData.messages.meta.groomer.dont_need;

            Messages.GroomerHorseCurrentlyAtFormat = gameData.messages.meta.groomer.currently_at;
            Messages.GroomerApplyServiceFormat = gameData.messages.meta.groomer.apply_service;
            Messages.GroomerApplyServiceForAllFormat = gameData.messages.meta.groomer.apply_all;

            // Barn
            Messages.BarnHorseFullyFedFormat = gameData.messages.meta.barn.fully_fed;
            Messages.BarnCantAffordService = gameData.messages.meta.barn.cant_afford;
            Messages.BarnAllHorsesFullyFed = gameData.messages.meta.barn.rested_all;
            Messages.BarnServiceNotNeeded = gameData.messages.meta.barn.not_needed;

            Messages.BarnHorseStatusFormat = gameData.messages.meta.barn.horse_status;
            Messages.BarnHorseMaxed = gameData.messages.meta.barn.horse_maxed;
            Messages.BarnLetHorseRelaxFormat = gameData.messages.meta.barn.let_relax;
            Messages.BarnLetAllHorsesReleaxFormat = gameData.messages.meta.barn.relax_all;

            // Farrier
            Messages.FarrierCurrentShoesFormat = gameData.messages.meta.farrier.current_shoes;
            Messages.FarrierApplyIronShoesFormat = gameData.messages.meta.farrier.apply_iron;
            Messages.FarrierApplySteelShoesFormat = gameData.messages.meta.farrier.apply_steel;
            Messages.FarrierShoeAllFormat = gameData.messages.meta.farrier.shoe_all;

            Messages.FarrierPutOnSteelShoesMessageFormat = gameData.messages.meta.farrier.put_on_steel_shoes;
            Messages.FarrierPutOnIronShoesMessageFormat = gameData.messages.meta.farrier.put_on_iron_shoes;
            Messages.FarrierPutOnSteelShoesAllMesssageFormat = gameData.messages.meta.farrier.put_on_steel_all;
            Messages.FarrierShoesCantAffordMessage = gameData.messages.meta.farrier.cant_afford_farrier;

            // Trainng Pen
            Messages.TrainedInStatFormat = gameData.messages.meta.trainer_pen.train_success;
            Messages.TrainerHeaderFormat = gameData.messages.meta.trainer_pen.train_header;
            Messages.TrainerHorseEntryFormat = gameData.messages.meta.trainer_pen.train_format;
            Messages.TrainerHorseFullyTrainedFormat = gameData.messages.meta.trainer_pen.fully_trained;
            Messages.TrainerCantTrainAgainInFormat = gameData.messages.meta.trainer_pen.train_again_in;
            Messages.TrainerCantAfford = gameData.messages.meta.trainer_pen.cant_afford;

            // Santa
            Messages.SantaHiddenText = gameData.messages.meta.santa.hidden_text;
            Messages.SantaWrapItemFormat = gameData.messages.meta.santa.wrap_format;
            Messages.SantaWrappedObjectMessage = gameData.messages.meta.santa.wrapped_object;
            Messages.SantaCantWrapInvFull = gameData.messages.meta.santa.wrap_fail_inv_full;

            Messages.SantaItemOpenedFormat = gameData.messages.meta.santa.open_format;
            Messages.SantaItemCantOpenInvFull = gameData.messages.meta.santa.open_format;

            // Pawneer
            Messages.PawneerUntackedHorsesICanBuy = gameData.messages.meta.pawneer.untacked_i_can_buy;
            Messages.PawneerHorseFormat = gameData.messages.meta.pawneer.pawn_horse;
            Messages.PawneerOrderMeta = gameData.messages.meta.pawneer.pawneer_order;
            Messages.PawneerHorseConfirmationFormat = gameData.messages.meta.pawneer.are_you_sure;
            Messages.PawneerHorseSoldMessagesFormat = gameData.messages.meta.pawneer.horse_sold;
            Messages.PawneerHorseNotFound = gameData.messages.meta.pawneer.horse_not_found;

            Messages.PawneerOrderSelectBreed = gameData.messages.meta.pawneer.order.select_breed;
            Messages.PawneerOrderBreedEntryFormat = gameData.messages.meta.pawneer.order.breed_entry;

            Messages.PawneerOrderSelectColorFormat = gameData.messages.meta.pawneer.order.select_color;
            Messages.PawneerOrderColorEntryFormat = gameData.messages.meta.pawneer.order.color_entry;

            Messages.PawneerOrderSelectGenderFormat = gameData.messages.meta.pawneer.order.select_gender;
            Messages.PawneerOrderGenderEntryFormat = gameData.messages.meta.pawneer.order.gender_entry;

            Messages.PawneerOrderHorseFoundFormat = gameData.messages.meta.pawneer.order.found;

            // Vet
            Messages.VetServiceHorseFormat = gameData.messages.meta.vet.service_horse;
            Messages.VetSerivcesNotNeeded = gameData.messages.meta.vet.not_needed;
            Messages.VetApplyServicesFormat = gameData.messages.meta.vet.apply;

            Messages.VetApplyServicesForAllFormat = gameData.messages.meta.vet.apply_all;
            Messages.VetFullHealthRecoveredMessageFormat = gameData.messages.meta.vet.now_full_health;
            Messages.VetServicesNotNeededAll = gameData.messages.meta.vet.not_needed_all;
            Messages.VetAllFullHealthRecoveredMessage = gameData.messages.meta.vet.all_full;
            Messages.VetCannotAffordMessage = gameData.messages.meta.vet.cant_afford;

            // Pond
            Messages.PondHeader = gameData.messages.meta.pond.header;
            Messages.PondGoFishing = gameData.messages.meta.pond.go_fishing;
            Messages.PondNoFishingPole = gameData.messages.meta.pond.no_fishing_pole;
            Messages.PondDrinkHereIfSafe = gameData.messages.meta.pond.drink_here;
            Messages.PondHorseDrinkFormat = gameData.messages.meta.pond.horse_drink_format;
            Messages.PondNoEarthWorms = gameData.messages.meta.pond.no_earth_worms;

            Messages.PondDrinkFullFormat = gameData.messages.meta.pond.drank_full;
            Messages.PondCantDrinkHpLowFormat = gameData.messages.meta.pond.cant_drink_hp_low;
            Messages.PondDrinkOhNoesFormat = gameData.messages.meta.pond.drank_something_bad;
            Messages.PondNotThirstyFormat = gameData.messages.meta.pond.not_thirsty;

            // Horse Whisperer
            Messages.WhispererHorseLocateButtonFormat = gameData.messages.meta.whisperer.horse_locate_meta;
            Messages.WhispererServiceCostYouFormat = gameData.messages.meta.whisperer.service_cost;
            Messages.WhispererServiceCannotAfford = gameData.messages.meta.whisperer.cant_afford;
            Messages.WhispererSearchingAmoungHorses = gameData.messages.meta.whisperer.searching_amoung_horses;
            Messages.WhispererNoneFound = gameData.messages.meta.whisperer.none_found_meta;
            Messages.WhispererHorsesFoundFormat = gameData.messages.meta.whisperer.horse_found_meta;

            // Mud Hole
            Messages.MudHoleNoHorses = gameData.messages.meta.mud_hole.no_horses;
            Messages.MudHoleRuinedGroomFormat = gameData.messages.meta.mud_hole.ruined_groom;

            // Movement
            Messages.RandomMovement = gameData.messages.random_movement;

            // Quests Log
            Messages.QuestLogHeader = gameData.messages.meta.quest_log.header_meta;
            Messages.QuestFormat = gameData.messages.meta.quest_log.quest_format;

            Messages.QuestNotCompleted = gameData.messages.meta.quest_log.not_complete;
            Messages.QuestNotAvalible = gameData.messages.meta.quest_log.not_avalible;
            Messages.QuestCompleted = gameData.messages.meta.quest_log.completed;

            Messages.QuestFooterFormat = gameData.messages.meta.quest_log.footer_format;
            // Transport

            Messages.CantAffordTransport = gameData.messages.transport.not_enough_money;
            Messages.WelcomeToAreaFormat = gameData.messages.transport.welcome_to_format;
            Messages.TransportFormat = gameData.messages.meta.transport_format;
            Messages.TransportCostFormat = gameData.messages.meta.transport_cost;
            Messages.TransportWagonFree = gameData.messages.meta.transport_free;

            // Abuse Reports
            Messages.AbuseReportMetaFormat = gameData.messages.meta.abuse_report.options_format;
            Messages.AbuseReportReasonFormat = gameData.messages.meta.abuse_report.report_reason_format;

            Messages.AbuseReportPlayerNotFoundFormat = gameData.messages.abuse_report.player_not_found_format;
            Messages.AbuseReportFiled = gameData.messages.abuse_report.report_filed;
            Messages.AbuseReportProvideValidReason = gameData.messages.abuse_report.valid_reason;

            // Bank
            Messages.BankMadeInIntrestFormat = gameData.messages.meta.bank.made_interest;
            Messages.BankCarryingFormat = gameData.messages.meta.bank.carrying_message;
            Messages.BankWhatToDo = gameData.messages.meta.bank.what_to_do;
            Messages.BankOptionsFormat = gameData.messages.meta.bank.options;


            Messages.BankDepositedMoneyFormat = gameData.messages.bank.deposit_format;
            Messages.BankWithdrewMoneyFormat = gameData.messages.bank.withdraw_format;

            // Riddler
            Messages.RiddlerAnsweredAll = gameData.messages.meta.riddler.riddle_all_complete;
            Messages.RiddlerIncorrectAnswer = gameData.messages.meta.riddler.riddle_incorrect;
            Messages.RiddlerCorrectAnswerFormat = gameData.messages.meta.riddler.riddle_correct;
            Messages.RiddlerEnterAnswerFormat = gameData.messages.meta.riddler.riddle_format;

            // Workshop
            Messages.WorkshopCraftEntryFormat = gameData.messages.meta.workshop.craft_entry;
            Messages.WorkshopRequiresFormat = gameData.messages.meta.workshop.requires;
            Messages.WorkshopRequireEntryFormat = gameData.messages.meta.workshop.require;
            Messages.WorkshopAnd = gameData.messages.meta.workshop.and;

            Messages.WorkshopNoRoomInInventory = gameData.messages.meta.workshop.no_room;
            Messages.WorkshopMissingRequiredItem = gameData.messages.meta.workshop.missing_item;
            Messages.WorkshopCraftingSuccess = gameData.messages.meta.workshop.craft_success;
            Messages.WorkshopCannotAfford = gameData.messages.meta.workshop.no_money;

            // Horses
            Messages.AdvancedStatFormat = gameData.messages.meta.horse.stat_format;
            Messages.BasicStatFormat = gameData.messages.meta.horse.basic_stat_format;
            Messages.HorsesHere = gameData.messages.meta.horse.horses_here;
            Messages.WildHorseFormat = gameData.messages.meta.horse.wild_horse;
            Messages.HorseCaptureTimer = gameData.messages.meta.horse.horse_timer;

            Messages.YouCapturedTheHorse = gameData.messages.meta.horse.horse_caught;
            Messages.HorseEvadedCapture = gameData.messages.meta.horse.horse_escaped;
            Messages.HorseEscapedAnyway = gameData.messages.meta.horse.horse_escaped_anyway;

            Messages.HorsesMenuHeader = gameData.messages.meta.horse.horses_menu;
            Messages.TooManyHorses = gameData.messages.meta.horse.too_many_horses;
            Messages.UpdateHorseCategory = gameData.messages.meta.horse.update_category;
            Messages.HorseEntryFormat = gameData.messages.meta.horse.horse_format;
            Messages.ViewBaiscStats = gameData.messages.meta.horse.view_basic_stats;
            Messages.ViewAdvancedStats = gameData.messages.meta.horse.view_advanced_stats;
            Messages.HorseBuckedYou = gameData.messages.meta.horse.horse_bucked;
            Messages.HorseLlamaBuckedYou = gameData.messages.meta.horse.llama_bucked;
            Messages.HorseCamelBuckedYou = gameData.messages.meta.horse.camel_bucked;

            Messages.HorseRidingMessageFormat = gameData.messages.meta.horse.riding_message;
            Messages.HorseNameYoursFormat = gameData.messages.meta.horse.horse_inventory.your_horse_format;
            Messages.HorseNameOthersFormat = gameData.messages.meta.horse.horse_inventory.horse_others_format;
            Messages.HorseDescriptionFormat = gameData.messages.meta.horse.horse_inventory.description_format;
            Messages.HorseHandsHeightFormat = gameData.messages.meta.horse.horse_inventory.hands_high;
            Messages.HorseExperienceEarnedFormat = gameData.messages.meta.horse.horse_inventory.experience;
            
            Messages.HorseTrainableInFormat = gameData.messages.meta.horse.horse_inventory.trainable_in;
            Messages.HorseIsTrainable = gameData.messages.meta.horse.horse_inventory.currently_trainable;
            Messages.HorseLeasedRemainingTimeFormat = gameData.messages.meta.horse.horse_inventory.leased_horse;

            Messages.HorseCannotMountUntilTackedMessage = gameData.messages.meta.horse.cannot_mount_tacked;
            Messages.HorseDismountedBecauseNotTackedMessageFormat = gameData.messages.meta.horse.dismount_because_tack;
            Messages.HorseMountButtonFormat = gameData.messages.meta.horse.horse_inventory.mount_button;
            Messages.HorseDisMountButtonFormat = gameData.messages.meta.horse.horse_inventory.dismount_button;
            Messages.HorseFeedButtonFormat = gameData.messages.meta.horse.horse_inventory.feed_button;
            Messages.HorseTackButtonFormat = gameData.messages.meta.horse.horse_inventory.tack_button;
            Messages.HorsePetButtonFormat = gameData.messages.meta.horse.horse_inventory.pet_button;
            Messages.HorseProfileButtonFormat = gameData.messages.meta.horse.horse_inventory.profile_button;
            Messages.HorseSavedProfileMessageFormat = gameData.messages.meta.horse.saved_profile;

            Messages.HorseNoAutoSell = gameData.messages.meta.horse.horse_inventory.no_auto_sell;
            Messages.HorseAutoSellPriceFormat = gameData.messages.meta.horse.horse_inventory.auto_sell_format;
            Messages.HorseAutoSellOthersFormat = gameData.messages.meta.horse.horse_inventory.auto_sell_others;
            Messages.HorseAutoSellFormat = gameData.messages.meta.horse.horse_inventory.auto_sell;
            Messages.HorseCantAutoSellTacked = gameData.messages.meta.horse.horse_inventory.cannot_auto_sell_tacked;

            Messages.HorseCurrentlyCategoryFormat = gameData.messages.meta.horse.horse_inventory.marked_as;
            Messages.HorseMarkAsCategory = gameData.messages.meta.horse.horse_inventory.marking_options;
            Messages.HorseStats = gameData.messages.meta.horse.horse_inventory.horse_stats;
            Messages.HorseTacked = gameData.messages.meta.horse.horse_inventory.wearing_tacked;
            Messages.HorseTackFormat = gameData.messages.meta.horse.horse_inventory.tacked_format;

            Messages.HorseCompanion = gameData.messages.meta.horse.horse_inventory.companion;
            Messages.HorseCompanionFormat = gameData.messages.meta.horse.horse_inventory.companion_selected;
            Messages.HorseCompanionChangeButton = gameData.messages.meta.horse.horse_inventory.companion_change_button;
            Messages.HorseNoCompanion = gameData.messages.meta.horse.horse_inventory.no_companion;

            Messages.HorseAdvancedStatsFormat = gameData.messages.meta.horse.horse_inventory.advanced_stats;
            Messages.HorseBreedDetailsFormat = gameData.messages.meta.horse.horse_inventory.breed_details;
            Messages.HorseHeightRangeFormat = gameData.messages.meta.horse.horse_inventory.height_range;
            Messages.HorsePossibleColorsFormat = gameData.messages.meta.horse.horse_inventory.possible_colors;
            Messages.HorseReleaseButton = gameData.messages.meta.horse.horse_inventory.release_horse;
            Messages.HorseOthers = gameData.messages.meta.horse.horse_inventory.other_horses;

            Messages.HorseDescriptionEditFormat = gameData.messages.meta.horse.description_edit;
            Messages.HorseEquipTackMessageFormat = gameData.messages.meta.horse.equip_tack_message;
            Messages.HorseUnEquipTackMessageFormat = gameData.messages.meta.horse.unequip_tack_message;
            Messages.HorseStopRidingMessage = gameData.messages.meta.horse.stop_riding_message;

            Messages.HorsePetMessageFormat = gameData.messages.meta.horse.pet_horse;
            Messages.HorsePetTooHappy = gameData.messages.meta.horse.pet_horse_too_happy;
            Messages.HorsePetTooTired = gameData.messages.meta.horse.pet_horse_too_sleepy;
            Messages.HorseSetNewCategoryMessageFormat = gameData.messages.meta.horse.horse_set_new_category;

            Messages.HorseAutoSellMenuFormat = gameData.messages.meta.horse.auto_sell.auto_sell_meta;
            Messages.HorseIsAutoSell = gameData.messages.meta.horse.auto_sell.is_auto_sell;
            Messages.HorseAutoSellConfirmedFormat = gameData.messages.meta.horse.auto_sell.auto_sell_confirmed;
            Messages.HorseAutoSellRemoved = gameData.messages.meta.horse.auto_sell.auto_sell_remove;

            Messages.HorseSetAutoSell = gameData.messages.meta.horse.horse_inventory.set_auto_sell;
            Messages.HorseChangeAutoSell = gameData.messages.meta.horse.horse_inventory.change_auto_sell;
            Messages.HorseTackFailAutoSell = gameData.messages.meta.horse.tack_fail_autosell;

            Messages.HorseAreYouSureYouWantToReleaseFormat = gameData.messages.meta.horse.horse_release;
            Messages.HorseCantReleaseTheHorseYourRidingOn = gameData.messages.meta.horse.cant_release_currently_riding;
            Messages.HorseReleasedMeta = gameData.messages.meta.horse.released_horse;
            Messages.HorseReleasedBy = gameData.messages.meta.horse.released_by_message;

            // All Stats (basic)

            Messages.HorseAllBasicStats = gameData.messages.meta.horse.allstats_basic.all_baisc_stats;
            Messages.HorseBasicStatEntryFormat = gameData.messages.meta.horse.allstats_basic.horse_entry;

            // All Stats (all)
            Messages.HorseAllStatsHeader = gameData.messages.meta.horse.allstats.all_stats_header;
            Messages.HorseNameEntryFormat = gameData.messages.meta.horse.allstats.horse_name_entry;
            Messages.HorseBasicStatsCompactedFormat = gameData.messages.meta.horse.allstats.basic_stats_compact;
            Messages.HorseAdvancedStatsCompactedFormat = gameData.messages.meta.horse.allstats.advanced_stats_compact;
            Messages.HorseAllStatsLegend = gameData.messages.meta.horse.allstats.legend;


            // Horse companion menu
            Messages.HorseCompanionMenuHeaderFormat = gameData.messages.meta.horse.companion_menu.menu_header;
            Messages.HorseCompnaionMenuCurrentCompanionFormat = gameData.messages.meta.horse.companion_menu.selected_companion;
            Messages.HorseCompanionEntryFormat = gameData.messages.meta.horse.companion_menu.companion_entry;
            Messages.HorseCompanionEquipMessageFormat = gameData.messages.meta.horse.companion_menu.companion_equip_message;
            Messages.HorseCompanionRemoveMessageFormat = gameData.messages.meta.horse.companion_menu.companion_remove_message;
            Messages.HorseCompanionMenuCurrentlyAvalibleCompanions = gameData.messages.meta.horse.companion_menu.companions_avalible;

            // Horse Feed Menu
            Messages.HorseCurrentStatusFormat = gameData.messages.meta.horse.feed_horse.current_status;
            Messages.HorseHoldingHorseFeed = gameData.messages.meta.horse.feed_horse.holding_horse_feed;
            Messages.HorsefeedFormat = gameData.messages.meta.horse.feed_horse.horsefeed_format;
            Messages.HorseNeighsThanks = gameData.messages.meta.horse.feed_horse.horse_neigh;
            Messages.HorseCouldNotFinish = gameData.messages.meta.horse.feed_horse.horse_could_not_finish;

            Messages.HorseFeedPersonalityIncreased = gameData.messages.meta.horse.feed_horse.feed_special_personality;
            Messages.HorseFeedInteligenceIncreased = gameData.messages.meta.horse.feed_horse.feed_special_inteligence;
            Messages.HorseFeedMagicBeanFormat = gameData.messages.meta.horse.feed_horse.feed_special_magic_bean;
            Messages.HorseFeedMagicDropletFormat = gameData.messages.meta.horse.feed_horse.feed_special_magic_droplet;

            // Tack menu (horses)
            Messages.HorseTackedAsFollowsFormat = gameData.messages.meta.horse.tack_menu.tacked_as_follows;
            Messages.HorseUnEquipSaddleFormat = gameData.messages.meta.horse.tack_menu.dequip_saddle;
            Messages.HorseUnEquipSaddlePadFormat = gameData.messages.meta.horse.tack_menu.dequip_saddle_pad;
            Messages.HorseUnEquipBridleFormat = gameData.messages.meta.horse.tack_menu.dequip_bridle;
            Messages.HorseTackInInventory = gameData.messages.meta.horse.tack_menu.you_have_following_tack;
            Messages.HorseLlamaTackInInventory = gameData.messages.meta.horse.tack_menu.you_have_following_llama_tack;
            Messages.HorseCamelTackInInventory = gameData.messages.meta.horse.tack_menu.you_have_following_camel_tack;
            Messages.HorseEquipFormat = gameData.messages.meta.horse.tack_menu.equip_tack;
            Messages.BackToHorse = gameData.messages.meta.horse.back_to_horse;


            // Libary
            Messages.LibaryMainMenu = gameData.messages.meta.libary.main_menu;
            Messages.LibaryFindNpc = gameData.messages.meta.libary.find_npc;
            Messages.LibaryFindNpcSearchResultsHeader = gameData.messages.meta.libary.find_npc_results_header;
            Messages.LibaryFindNpcSearchResultFormat = gameData.messages.meta.libary.find_npc_results_format;
            Messages.LibaryFindNpcSearchNoResults = gameData.messages.meta.libary.find_npc_no_results;
            Messages.LibaryFindNpcLimit5 = gameData.messages.meta.libary.find_npc_limit5;

            Messages.LibaryFindRanch = gameData.messages.meta.libary.find_ranch;
            Messages.LibaryFindRanchResultsHeader = gameData.messages.meta.libary.find_ranch_match_closely;
            Messages.LibaryFindRanchResultFormat = gameData.messages.meta.libary.find_ranch_result;
            Messages.LibaryFindRanchResultsNoResults = gameData.messages.meta.libary.find_ranch_no_results;

            Messages.HorseBreedFormat = gameData.messages.meta.libary.horse_breed_format;
            Messages.HorseRelativeFormat = gameData.messages.meta.libary.horse_relative_format;
            Messages.BreedViewerFormat = gameData.messages.meta.libary.breed_preview_format;
            Messages.BreedViewerMaximumStats = gameData.messages.meta.libary.maximum_stats;

            // Chat

            Messages.ChatViolationMessageFormat = gameData.messages.chat.violation_format;
            Messages.RequiredChatViolations = gameData.messages.chat.violation_points_required;

            Messages.GlobalChatFormatForModerators = gameData.messages.chat.for_others.global_format_moderator;
            Messages.DirectChatFormatForModerators = gameData.messages.chat.for_others.dm_format_moderator;

            Messages.YouWereSentToPrisionIsle = gameData.messages.starved_horse;

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
            Messages.AdsChatFormatForSender = gameData.messages.chat.for_sender.ads_format;
            Messages.AdminChatFormatForSender = gameData.messages.chat.for_sender.admin_format;

            Messages.AdminCommandFormat = gameData.messages.commands.admin_command_completed;
            Messages.PlayerCommandFormat = gameData.messages.commands.player_command_completed;
            Messages.MuteHelp = gameData.messages.commands.mute_help;
            Messages.UnMuteHelp = gameData.messages.commands.unmute_help;


            Messages.PasswordNotice = gameData.messages.chat.password_included;
            Messages.CapsNotice = gameData.messages.chat.caps_notice;

            // Drawing Rooms
            Messages.DrawingLastToDrawFormat = gameData.messages.meta.drawing_rooms.last_draw;
            Messages.DrawingContentsSavedInSlotFormat = gameData.messages.meta.drawing_rooms.saved;
            Messages.DrawingContentsLoadedFromSlotFormat = gameData.messages.meta.drawing_rooms.load;
            Messages.DrawingPlzClearDraw = gameData.messages.meta.drawing_rooms.plz_clear_draw;
            Messages.DrawingPlzClearLoad = gameData.messages.meta.drawing_rooms.plz_clear_load;
            Messages.DrawingNotSentNotSubscribed = gameData.messages.meta.drawing_rooms.not_subscribed_draw;
            Messages.DrawingCannotLoadNotSubscribed = gameData.messages.meta.drawing_rooms.not_subscribed_load;

            // Brickpoet
            Messages.LastPoetFormat = gameData.messages.meta.last_poet;

            // Mutliroom
            Messages.MultiroomParticipentFormat = gameData.messages.meta.multiroom.partcipent_format;
            Messages.MultiroomPlayersParticipating = gameData.messages.meta.multiroom.other_players_participating;

            // Dropped Items

            Messages.NothingMessage = gameData.messages.meta.dropped_items.nothing_message;
            Messages.ItemsOnGroundMessage = gameData.messages.meta.dropped_items.items_message;
            Messages.GrabItemFormat = gameData.messages.meta.dropped_items.item_format;
            Messages.ItemInformationFormat = gameData.messages.meta.dropped_items.item_information_format;
            Messages.GrabAllItemsButton = gameData.messages.meta.dropped_items.grab_all;
            Messages.DroppedAnItemMessage = gameData.messages.dropped_items.dropped_item_message;
            Messages.DroppedItemTileIsFull = gameData.messages.dropped_items.drop_tile_full;
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

            // Player List

            Messages.PlayerListHeader = gameData.messages.meta.player_list.playerlist_header;
            Messages.PlayerListSelectFromFollowing = gameData.messages.meta.player_list.select_from_following;
            Messages.PlayerListOfBuddiesFormat = gameData.messages.meta.player_list.list_of_buddies_format;
            Messages.PlayerListOfNearby = gameData.messages.meta.player_list.list_of_players_nearby;
            Messages.PlayerListOfPlayersFormat = gameData.messages.meta.player_list.list_of_all_players_format;
            Messages.PlayerListOfPlayersAlphabetically = gameData.messages.meta.player_list.list_of_all_players_alphabetically;
            Messages.PlayerListMapAllBuddiesForamt = gameData.messages.meta.player_list.map_all_buddies_format;
            Messages.PlayerListMapAllPlayersFormat = gameData.messages.meta.player_list.map_all_players_format;
            Messages.PlayerListAbuseReport = gameData.messages.meta.player_list.abuse_report;

            Messages.MuteButton = gameData.messages.meta.player_list.mute_button;
            Messages.HearButton = gameData.messages.meta.player_list.hear_button;
            Messages.PmButton = gameData.messages.meta.player_list.pm_button;

            Messages.ThreeMonthSubscripitionIcon = gameData.messages.meta.player_list.icon_subbed_3month;
            Messages.YearSubscriptionIcon = gameData.messages.meta.player_list.icon_subbed_year;
            Messages.NewUserIcon = gameData.messages.meta.player_list.icon_new;
            Messages.MonthSubscriptionIcon = gameData.messages.meta.player_list.icon_subbed_month;
            Messages.AdminIcon = gameData.messages.meta.player_list.icon_admin;
            Messages.ModeratorIcon = gameData.messages.meta.player_list.icon_mod;

            Messages.BuddyListHeader = gameData.messages.meta.player_list.online_buddy_header;
            Messages.BuddyListOnlineBuddyEntryFormat = gameData.messages.meta.player_list.online_buddy_format;
            Messages.BuddyListOfflineBuddys = gameData.messages.meta.player_list.offline_buddys;
            Messages.BuddyListOfflineBuddyEntryFormat = gameData.messages.meta.player_list.offline_buddy_format;

            Messages.NearbyPlayersListHeader = gameData.messages.meta.player_list.nearby_player_header;
            Messages.PlayerListAllAlphabeticalHeader = gameData.messages.meta.player_list.all_players_alphabetical_header;

            Messages.PlayerListEntryFormat = gameData.messages.meta.player_list.player_format;

            Messages.PlayerListIdle = gameData.messages.meta.player_list.idle_text;
            Messages.PlayerListAllHeader = gameData.messages.meta.player_list.all_players_header;
            Messages.PlayerListIconFormat = gameData.messages.meta.player_list.icon_format;
            Messages.PlayerListIconInformation = gameData.messages.meta.player_list.icon_info;
            // Consume

            Messages.ConsumeItemFormat = gameData.messages.consume.consumed_item_format;
            Messages.ConsumedButMaxReached = gameData.messages.consume.consumed_but_max_reached;

            // Meta Format

            Messages.LocationFormat = gameData.messages.meta.location_format;
            Messages.IsleFormat = gameData.messages.meta.isle_format;
            Messages.TownFormat = gameData.messages.meta.town_format;
            Messages.AreaFormat = gameData.messages.meta.area_format;
            Messages.Seperator = gameData.messages.meta.seperator;
            Messages.TileFormat = gameData.messages.meta.tile_format;
            Messages.ExitThisPlace = gameData.messages.meta.exit_this_place;
            Messages.BackToMap = gameData.messages.meta.back_to_map;
            Messages.BackToMapHorse = gameData.messages.meta.back_to_map_horse;
            Messages.LongFullLine = gameData.messages.meta.long_full_line;
            Messages.MetaTerminator = gameData.messages.meta.end_of_meta;

            Messages.PlayersHere = gameData.messages.meta.player_interaction.players_here;
            Messages.NearbyPlayers = gameData.messages.meta.nearby.players_nearby;
            Messages.North = gameData.messages.meta.nearby.north;
            Messages.East = gameData.messages.meta.nearby.east;
            Messages.South = gameData.messages.meta.nearby.south;
            Messages.West = gameData.messages.meta.nearby.west;

            Messages.NoPitchforkMeta = gameData.messages.meta.hay_pile.no_pitchfork;
            Messages.HasPitchforkMeta = gameData.messages.meta.hay_pile.pitchfork;
            Messages.R1 = gameData.messages.meta.r1;
            Messages.PasswordEntry = gameData.messages.meta.password_input;

            // Venus Fly Trap

            Messages.VenusFlyTrapFormat = gameData.messages.meta.venus_flytrap_format;

            // Shortcut
            Messages.NoTelescope = gameData.messages.no_telescope;

            // Inn
            Messages.InnBuyMeal = gameData.messages.meta.inn.buy_meal;
            Messages.InnBuyRest = gameData.messages.meta.inn.buy_rest;
            Messages.InnItemEntryFormat = gameData.messages.meta.inn.inn_entry;
            Messages.InnEnjoyedServiceFormat = gameData.messages.inn.enjoyed_service;
            Messages.InnCannotAffordService = gameData.messages.inn.cant_afford;
            Messages.InnFullyRested = gameData.messages.inn.fully_rested;

            // Password
            Messages.IncorrectPasswordMessage = gameData.messages.incorrect_password;

            // Fountain
            Messages.FountainMeta = gameData.messages.meta.fountain;
            Messages.FountainDrankYourFull = gameData.messages.fountain.drank_your_fill;
            Messages.FountainDroppedMoneyFormat = gameData.messages.fountain.dropped_money;

            // Highscore

            Messages.HighscoreHeaderMeta = gameData.messages.meta.highscores.header_meta;
            Messages.HighscoreFormat = gameData.messages.meta.highscores.highscore_format;
            Messages.BestTimeFormat = gameData.messages.meta.highscores.besttime_format;

            Messages.GameHighScoreHeaderFormat = gameData.messages.meta.highscores.game_highscore_header;
            Messages.GameHighScoreFormat = gameData.messages.meta.highscores.game_highscore_format;

            Messages.GameWinLooseHeaderFormat = gameData.messages.meta.highscores.game_winloose_header;
            Messages.GameWinLooseFormat = gameData.messages.meta.highscores.game_winloose_format;

            Messages.GameBestTimeHeaderFormat = gameData.messages.meta.highscores.game_besttime_header;
            Messages.GameBestTimeFormat = gameData.messages.meta.highscores.game_besttime_format;

            // Awards

            Messages.AwardHeader = gameData.messages.meta.awards_page.awards_header;
            Messages.NoAwards = gameData.messages.meta.awards_page.no_awards;
            Messages.AwardFormat = gameData.messages.meta.awards_page.award_format;

            // World Peace
            Messages.NoWishingCoins = gameData.messages.meta.wishing_well.no_coins;
            Messages.YouHaveWishingCoinsFormat = gameData.messages.meta.wishing_well.wish_coins;
            Messages.WishItemsFormat = gameData.messages.meta.wishing_well.wish_things;
            Messages.WishMoneyFormat = gameData.messages.meta.wishing_well.wish_money;
            Messages.WishWorldPeaceFormat = gameData.messages.meta.wishing_well.wish_worldpeace;

            Messages.TossedCoin = gameData.messages.meta.wishing_well.make_wish;
            Messages.WorldPeaceOnlySoDeep = gameData.messages.meta.wishing_well.world_peace_message;
            Messages.WishingWellMeta = gameData.messages.meta.wishing_well.wish_meta;
            // Sec Codes

            Messages.InvalidSecCodeError = gameData.messages.sec_code.invalid_sec_code;
            Messages.YouEarnedAnItemFormat = gameData.messages.sec_code.item_earned;
            Messages.YouEarnedAnItemButInventoryWasFullFormat = gameData.messages.sec_code.item_earned_full_inv;
            Messages.YouLostAnItemFormat = gameData.messages.sec_code.item_deleted;
            Messages.YouEarnedMoneyFormat = gameData.messages.sec_code.money_earned;
            Messages.BeatHighscoreFormat = gameData.messages.sec_code.highscore_beaten;
            Messages.BeatBestHighscore = gameData.messages.sec_code.best_highscore_beaten;
            Messages.BeatBestTimeFormat = gameData.messages.sec_code.best_time_beaten;

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
            Messages.ItemOpenButton = gameData.messages.meta.inventory.item_open_button;
            Messages.ItemWearButton = gameData.messages.meta.inventory.item_wear_button;
            Messages.ItemReadButton = gameData.messages.meta.inventory.item_read_button;

            Messages.ShopBuyButton = gameData.messages.meta.inventory.buy_button;
            Messages.ShopBuy5Button = gameData.messages.meta.inventory.buy_5_button;
            Messages.ShopBuy25Button = gameData.messages.meta.inventory.buy_25_button;

            Messages.SellButton = gameData.messages.meta.inventory.sell_button;
            Messages.SellAllButton = gameData.messages.meta.inventory.sell_all_button;
            // Npc

            Messages.NpcStartChatFormat = gameData.messages.meta.npc.start_chat_format;
            Messages.NpcNoChatpoints = gameData.messages.meta.npc.no_chatpoints;
            Messages.NpcChatpointFormat = gameData.messages.meta.npc.chatpoint_format;
            Messages.NpcReplyFormat = gameData.messages.meta.npc.reply_format;
            Messages.NpcTalkButton = gameData.messages.meta.npc.npc_talk_button;
            Messages.NpcInformationButton = gameData.messages.meta.npc.npc_information_button;
            Messages.NpcInformationFormat = gameData.messages.meta.npc.npc_information_format;

            // Login Failed Reasons
            Messages.LoginFailedReasonBanned = gameData.messages.login.banned;
            Messages.LoginFailedReasonBannedIpFormat = gameData.messages.login.ip_banned;

            // Disconnect Reasons

            Messages.KickReasonKicked = gameData.messages.disconnect.kicked;
            Messages.KickReasonBanned = gameData.messages.disconnect.banned;
            Messages.KickReasonIdleFormat = gameData.messages.disconnect.client_timeout.kick_message;
            Messages.KickReasonNoTime = gameData.messages.disconnect.no_playtime;
            Messages.IdleWarningFormat = gameData.messages.disconnect.client_timeout.warn_message;
            Messages.KickReasonDuplicateLogin = gameData.messages.disconnect.dupe_login;

            // Competition Gear

            Messages.EquipCompetitionGearFormat = gameData.messages.equips.equip_competition_gear_format;
            Messages.RemoveCompetitionGear = gameData.messages.equips.removed_competition_gear;

            // Jewerly
            Messages.EquipJewelryFormat = gameData.messages.equips.equip_jewelry;
            Messages.MaxJewelryMessage = gameData.messages.equips.max_jewelry;
            Messages.RemoveJewelry = gameData.messages.equips.removed_jewelry;

            // Click
            Messages.NothingInterestingHere = gameData.messages.click_nothing_message;

            // Swf
            Messages.WagonCutscene = gameData.transport.wagon_cutscene;
            Messages.BoatCutscene = gameData.transport.boat_cutscene;
            Messages.BallonCutscene = gameData.transport.ballon_cutscene;

            gameData = null;
            return;
        }

    }
}
