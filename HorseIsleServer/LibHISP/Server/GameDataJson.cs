using HISP.Game;
using HISP.Game.Chat;
using HISP.Game.Events;
using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Game.Services;
using HISP.Game.SwfModules;
using HISP.Player;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HISP.Server
{
    public static class SystemTextJsonMergeExtensions
    {
        /// <summary>
        /// Merges the specified Json Node into the base JsonNode for which this method is called.
        /// It is null safe and can be easily used with null-check & null coalesce operators for fluent calls.
        /// NOTE: JsonNodes are context aware and track their parent relationships therefore to merge the values both JsonNode objects
        ///         specified are mutated. The Base is mutated with new data while the source is mutated to remove reverences to all
        ///         fields so that they can be added to the base.
        ///
        /// Source taken directly from the open-source Gist here:
        /// https://gist.github.com/cajuncoding/bf78bdcf790782090d231590cbc2438f
        ///
        /// </summary>
        /// <param name="jsonBase"></param>
        /// <param name="jsonMerge"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static JsonNode Merge(this JsonNode jsonBase, JsonNode jsonMerge)
        {
            if (jsonBase == null || jsonMerge == null)
                return jsonBase;

            switch (jsonBase)
            {
                case JsonObject jsonBaseObj when jsonMerge is JsonObject jsonMergeObj:
                    {
                        //NOTE: We must materialize the set (e.g. to an Array), and then clear the merge array so the node can then be 
                        //      re-assigned to the target/base Json; clearing the Object seems to be the most efficient approach...
                        var mergeNodesArray = jsonMergeObj.ToArray();
                        jsonMergeObj.Clear();

                        foreach (var prop in mergeNodesArray)
                        {
                            jsonBaseObj[prop.Key] = jsonBaseObj[prop.Key] switch
                            {
                                JsonObject jsonBaseChildObj when prop.Value is JsonObject jsonMergeChildObj => jsonBaseChildObj.Merge(jsonMergeChildObj),
                                JsonArray jsonBaseChildArray when prop.Value is JsonArray jsonMergeChildArray => jsonBaseChildArray.Merge(jsonMergeChildArray),
                                _ => prop.Value
                            };
                        }
                        break;
                    }
                case JsonArray jsonBaseArray when jsonMerge is JsonArray jsonMergeArray:
                    {
                        //NOTE: We must materialize the set (e.g. to an Array), and then clear the merge array,
                        //      so they can then be re-assigned to the target/base Json...
                        var mergeNodesArray = jsonMergeArray.ToArray();
                        jsonMergeArray.Clear();
                        foreach (var mergeNode in mergeNodesArray) jsonBaseArray.Add(mergeNode);
                        break;
                    }
                default:
                    throw new ArgumentException($"The JsonNode type [{jsonBase.GetType().Name}] is incompatible for merging with the target/base " +
                                                        $"type [{jsonMerge.GetType().Name}]; merge requires the types to be the same.");

            }

            return jsonBase;
        }

        /// <summary>
        /// Merges the specified Dictionary of values into the base JsonNode for which this method is called.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="jsonBase"></param>
        /// <param name="dictionary"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonNode MergeDictionary<TKey, TValue>(this JsonNode jsonBase, IDictionary<TKey, TValue> dictionary, JsonSerializerOptions options = null)
            => jsonBase.Merge(JsonSerializer.SerializeToNode(dictionary, options));
    }
    public class GameDataJson
    {
       
        private static JsonNode gameData = JsonNode.Parse("{}");

        private static void readGamedataFiles()
        {
            Logger.DebugPrint("Reading GAMEDATA");
            if (Directory.Exists(ConfigReader.GameData))
            {
                Logger.DebugPrint("Found GAMEDATA DIR ... ");
                
                string[] files = Directory.GetFiles(ConfigReader.GameData);
                foreach (string file in files)
                {
                    Logger.DebugPrint("Reading: " + file);
                    string jsonData = File.ReadAllText(file);
                    JsonNode node = JsonNode.Parse(jsonData);
                    gameData.Merge(node);

                }
            }
            else if (File.Exists(ConfigReader.GameData))
            {

                Logger.DebugPrint("Found GAMEDATA FILE ... ");
                string jsonData = File.ReadAllText(ConfigReader.GameData);
                gameData = JsonNode.Parse(jsonData);
                
            }
            else
            {
                Logger.ErrorPrint("Could not find GAMEDATA, configured as; " + ConfigReader.GameData + " But no file or directory exists!");
                GameServer.ShutdownServer("Unable to find GAMEDATA");
                return;
            }
        }

        private static void registerTowns()
        {
            int totalTowns = gameData["places"]["towns"].AsArray().Count;
            for (int i = 0; i < totalTowns; i++)
            {

                World.Town town = new World.Town();
                town.StartX = ((int)gameData["places"]["towns"].AsArray()[i]["start_x"]);
                town.StartY = ((int)gameData["places"]["towns"].AsArray()[i]["start_y"]);
                town.EndX =   ((int)gameData["places"]["towns"].AsArray()[i]["end_x"]);
                town.EndY =   ((int)gameData["places"]["towns"].AsArray()[i]["end_y"]);
                town.Name =   ((string)gameData["places"]["towns"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Town: " + town.Name + " X " + town.StartX + "," + town.EndX + " Y " + town.StartY + "," + town.EndY);
                World.Towns.Add(town);
            }
        }

        private static void registerZones()
        {
            int totalZones = gameData["places"]["zones"].AsArray().Count;
            for (int i = 0; i < totalZones; i++)
            {

                World.Zone zone = new World.Zone();
                zone.StartX = ((int)gameData["places"]["zones"].AsArray()[i]["start_x"]);
                zone.StartY = ((int)gameData["places"]["zones"].AsArray()[i]["start_y"]);
                zone.EndX =   ((int)gameData["places"]["zones"].AsArray()[i]["end_x"]);
                zone.EndY =   ((int)gameData["places"]["zones"].AsArray()[i]["end_y"]);
                zone.Name =   ((string)gameData["places"]["zones"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Zone: " + zone.Name + " X " + zone.StartX + "," + zone.EndX + " Y " + zone.StartY + "," + zone.EndY);
                World.Zones.Add(zone);
            }

        }
        private static void registerAreas()
        {
            int totalAreas = gameData["places"]["areas"].AsArray().Count;
            for (int i = 0; i < totalAreas; i++)
            {

                World.Area area = new World.Area();
                area.StartX = ((int)gameData["places"]["areas"].AsArray()[i]["start_x"]);
                area.StartY = ((int)gameData["places"]["areas"].AsArray()[i]["start_y"]);
                area.EndX   = ((int)gameData["places"]["areas"].AsArray()[i]["end_x"]);
                area.EndY   = ((int)gameData["places"]["areas"].AsArray()[i]["end_y"]);
                area.Name   = ((string)gameData["places"]["areas"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Area: " + area.Name + " X " + area.StartX + "-" + area.EndX + " Y " + area.StartY + "-" + area.EndY);
                World.Areas.Add(area);
            }
        }
        private static void registerIsles()
        {

            int totalIsles = gameData["places"]["isles"].AsArray().Count;
            for (int i = 0; i < totalIsles; i++)
            {
                World.Isle isle = new World.Isle();
                isle.StartX = ((int)gameData["places"]["isles"].AsArray()[i]["start_x"]);
                isle.StartY = ((int)gameData["places"]["isles"].AsArray()[i]["start_y"]);
                isle.EndX = ((int)gameData["places"]["isles"].AsArray()[i]["end_x"]);
                isle.EndY = ((int)gameData["places"]["isles"].AsArray()[i]["end_y"]);
                isle.Tileset = ((int)gameData["places"]["isles"].AsArray()[i]["tileset"]);
                isle.Name = ((string)gameData["places"]["isles"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Isle: " + isle.Name + " X " + isle.StartX + "," + isle.EndX + " Y " + isle.StartY + "," + isle.EndY + " tileset: " + isle.Tileset);
                World.Isles.Add(isle);
            }
        }

        private static void registerWaypoints()
        {
            int totalWaypoints = gameData["places"]["waypoints"].AsArray().Count;
            for (int i = 0; i < totalWaypoints; i++)
            {
                World.Waypoint waypoint = new World.Waypoint();
                waypoint.Name = ((string)gameData["places"]["waypoints"].AsArray()[i]["name"]);
                waypoint.PosX = ((int)gameData["places"]["waypoints"].AsArray()[i]["pos_x"]);
                waypoint.PosY = ((int)gameData["places"]["waypoints"].AsArray()[i]["pos_y"]);
                waypoint.Type = ((string)gameData["places"]["waypoints"].AsArray()[i]["type"]);
                waypoint.Description = ((string)gameData["places"]["waypoints"].AsArray()[i]["description"]);
                waypoint.WeatherTypesAvalible = gameData["places"]["waypoints"].AsArray()[i]["weather_avalible"].Deserialize<string[]>();
                Logger.DebugPrint("Registered Waypoint: " + waypoint.PosX.ToString() + ", " + waypoint.PosY.ToString() + " TYPE: " + waypoint.Type);
                World.Waypoints.Add(waypoint);
            }
        }
        private static void registerSpecialTiles()
        {
            int totalSpecialTiles = gameData["places"]["special_tiles"].AsArray().Count;
            for (int i = 0; i < totalSpecialTiles; i++)
            {

                World.SpecialTile specialTile = new World.SpecialTile();
                specialTile.X = ((int)gameData["places"]["special_tiles"].AsArray()[i]["x"]);
                specialTile.Y = ((int)gameData["places"]["special_tiles"].AsArray()[i]["y"]);
                specialTile.Title = ((string)gameData["places"]["special_tiles"].AsArray()[i]["title"]);
                specialTile.Description = ((string)gameData["places"]["special_tiles"].AsArray()[i]["description"]);
                specialTile.Code = ((string)gameData["places"]["special_tiles"].AsArray()[i]["code"]);
                if (gameData["places"]["special_tiles"].AsArray()[i]["exit_x"] != null)
                    specialTile.ExitX = ((int)gameData["places"]["special_tiles"].AsArray()[i]["exit_x"]);
                if (gameData["places"]["special_tiles"].AsArray()[i]["exit_y"] != null)
                    specialTile.ExitY = ((int)gameData["places"]["special_tiles"].AsArray()[i]["exit_y"]);
                specialTile.AutoplaySwf = ((string)gameData["places"]["special_tiles"].AsArray()[i]["autoplay_swf"]);
                specialTile.TypeFlag = ((string)gameData["places"]["special_tiles"].AsArray()[i]["type_flag"]);

                Logger.DebugPrint("Registered Special Tile: " + specialTile.Title + " X " + specialTile.X + " Y: " + specialTile.Y);
                World.SpecialTiles.Add(specialTile);
            }
        }
        private static void registerChatWarningReasons()
        {
            int totalReasons = gameData["messages"]["chat"]["reason_messages"].AsArray().Count;
            for (int i = 0; i < totalReasons; i++)
            {
                ChatMsg.Reason reason = new ChatMsg.Reason();
                reason.Name = ((string)gameData["messages"]["chat"]["reason_messages"].AsArray()[i]["name"]);
                reason.Message = ((string)gameData["messages"]["chat"]["reason_messages"].AsArray()[i]["message"]);
                ChatMsg.AddReason(reason);

                Logger.DebugPrint("Registered Chat Warning Reason: " + reason.Name + " (Message: " + reason.Message + ")");
            }

        }
        private static void registerFilteredWords()
        {
            int totalFilters = gameData["messages"]["chat"]["filter"].AsArray().Count;
            for (int i = 0; i < totalFilters; i++)
            {
                ChatMsg.Filter msgFilter = new ChatMsg.Filter();
                msgFilter.FilteredWord = ((string)gameData["messages"]["chat"]["filter"].AsArray()[i]["word"]);
                msgFilter.MatchAll = ((bool)gameData["messages"]["chat"]["filter"].AsArray()[i]["match_all"]);
                msgFilter.Reason = ChatMsg.GetReason((string)gameData["messages"]["chat"]["filter"].AsArray()[i]["reason_type"]);
                ChatMsg.AddFilter(msgFilter);

                Logger.DebugPrint("Registered Filtered Word: " + msgFilter.FilteredWord + " With reason: " + msgFilter.Reason.Name + " (Matching all: " + msgFilter.MatchAll + ")");
            }
        }
        private static void registerWordCorrections()
        {
            int totalCorrections = gameData["messages"]["chat"]["correct"].AsArray().Count;
            for (int i = 0; i < totalCorrections; i++)
            {
                ChatMsg.Correction correction = new ChatMsg.Correction();
                correction.FilteredWord = ((string)gameData["messages"]["chat"]["correct"].AsArray()[i]["word"]);
                correction.ReplacedWord = ((string)gameData["messages"]["chat"]["correct"].AsArray()[i]["new_word"]);
                ChatMsg.AddCorrection(correction);

                Logger.DebugPrint("Registered Word Correction: " + correction.FilteredWord + " to " + correction.ReplacedWord);
            }
        }
        private static void registerTransportPoints()
        {
            int totalTransportPoints = gameData["transport"]["transport_points"].AsArray().Count;
            for (int i = 0; i < totalTransportPoints; i++)
            {
                Transport.TransportPoint transportPoint = new Transport.TransportPoint();
                transportPoint.X = ((int)gameData["transport"]["transport_points"].AsArray()[i]["x"]);
                transportPoint.Y = ((int)gameData["transport"]["transport_points"].AsArray()[i]["y"]);
                transportPoint.Locations = gameData["transport"]["transport_points"].AsArray()[i]["places"].Deserialize<int[]>();
                Transport.TransportPoints.Add(transportPoint);

                Logger.DebugPrint("Registered Transport Point: At X: " + transportPoint.X + " Y: " + transportPoint.Y);
            }
        }

        private static void registerTransportLocations()
        {
            int totalTransportPlaces = gameData["transport"]["transport_places"].AsArray().Count;
            for (int i = 0; i < totalTransportPlaces; i++)
            {
                Transport.TransportLocation transportPlace = new Transport.TransportLocation();
                transportPlace.Id = ((int)gameData["transport"]["transport_places"].AsArray()[i]["id"]);
                transportPlace.Cost = ((int)gameData["transport"]["transport_places"].AsArray()[i]["cost"]);
                transportPlace.GotoX = ((int)gameData["transport"]["transport_places"].AsArray()[i]["goto_x"]);
                transportPlace.GotoY = ((int)gameData["transport"]["transport_places"].AsArray()[i]["goto_y"]);
                transportPlace.Type = ((string)gameData["transport"]["transport_places"].AsArray()[i]["type"]);
                transportPlace.LocationTitle = ((string)gameData["transport"]["transport_places"].AsArray()[i]["place_title"]);
                Transport.TransportLocations.Add(transportPlace);

                Logger.DebugPrint("Registered Transport Location: " + transportPlace.LocationTitle + " To Goto X: " + transportPlace.GotoX + " Y: " + transportPlace.GotoY);
            }
        }
        private static void registerItems()
        {
            int totalItems = gameData["item"]["item_list"].AsArray().Count;
            for (int i = 0; i < totalItems; i++)
            {
                Item.ItemInformation iteminfo = new Item.ItemInformation();
                iteminfo.Id = ((int)gameData["item"]["item_list"].AsArray()[i]["id"]);
                iteminfo.Name = ((string)gameData["item"]["item_list"].AsArray()[i]["name"]);
                iteminfo.PluralName = ((string)gameData["item"]["item_list"].AsArray()[i]["plural_name"]);
                iteminfo.Description = ((string)gameData["item"]["item_list"].AsArray()[i]["description"]);
                iteminfo.IconId = ((int)gameData["item"]["item_list"].AsArray()[i]["icon_id"]);
                iteminfo.SortBy = ((int)gameData["item"]["item_list"].AsArray()[i]["sort_by"]);
                iteminfo.SellPrice = ((int)gameData["item"]["item_list"].AsArray()[i]["sell_price"]);
                iteminfo.EmbedSwf = ((string)gameData["item"]["item_list"].AsArray()[i]["embed_swf"]);
                iteminfo.WishingWell = ((bool)gameData["item"]["item_list"].AsArray()[i]["wishing_well"]);
                iteminfo.Type = ((string)gameData["item"]["item_list"].AsArray()[i]["type"]);
                iteminfo.MiscFlags = gameData["item"]["item_list"].AsArray()[i]["misc_flags"].Deserialize<int[]>();
                int effectsCount = gameData["item"]["item_list"].AsArray()[i]["effects"].AsArray().Count;

                Item.Effects[] effectsList = new Item.Effects[effectsCount];
                for (int ii = 0; ii < effectsCount; ii++)
                {
                    effectsList[ii] = new Item.Effects();
                    effectsList[ii].EffectsWhat = ((string)gameData["item"]["item_list"].AsArray()[i]["effects"].AsArray()[ii]["effect_what"]);
                    effectsList[ii].EffectAmount = ((int)gameData["item"]["item_list"].AsArray()[i]["effects"].AsArray()[ii]["effect_amount"]);
                }

                iteminfo.Effects = effectsList;
                iteminfo.SpawnParamaters = new Item.SpawnRules();
                iteminfo.SpawnParamaters.SpawnCap = ((int)gameData["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_cap"]);
                iteminfo.SpawnParamaters.SpawnInZone = ((string)gameData["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_in_area"]);
                iteminfo.SpawnParamaters.SpawnOnTileType = ((string)gameData["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_on_tile_type"]);
                iteminfo.SpawnParamaters.SpawnOnSpecialTile = ((string)gameData["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_on_special_tile"]);
                iteminfo.SpawnParamaters.SpawnNearSpecialTile = ((string)gameData["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_near_special_tile"]);

                Logger.DebugPrint("Registered Item ID: " + iteminfo.Id + " Name: " + iteminfo.Name + " spawns on: " + iteminfo.SpawnParamaters.SpawnOnTileType);
                Item.AddItemInfo(iteminfo);
            }
        }
        private static void registerThrowables()
        {
            int totalThrowable = gameData["item"]["throwable"].AsArray().Count;
            for (int i = 0; i < totalThrowable; i++)
            {
                Item.ThrowableItem throwableItem = new Item.ThrowableItem();
                throwableItem.Id = ((int)gameData["item"]["throwable"].AsArray()[i]["id"]);
                throwableItem.HitMessage = ((string)gameData["item"]["throwable"].AsArray()[i]["message_hit"]);
                throwableItem.ThrowMessage = ((string)gameData["item"]["throwable"].AsArray()[i]["message_throw"]);
                throwableItem.HitYourselfMessage = ((string)gameData["item"]["throwable"].AsArray()[i]["message_hit_yourself"]);
                Item.AddThrowableItem(throwableItem);
            }
        }

        private static void registerNpcs()
        {
            Logger.DebugPrint("Registering NPCS: ");
            int totalNpcs = gameData["npc_list"].AsArray().Count;
            for (int i = 0; i < totalNpcs; i++)
            {
                int id = (int)gameData["npc_list"].AsArray()[i]["id"];
                int x = (int)gameData["npc_list"].AsArray()[i]["x"];
                int y = (int)gameData["npc_list"].AsArray()[i]["y"];
                bool moves = (bool)gameData["npc_list"].AsArray()[i]["moves"];

                int udlrStartX = 0;
                int udlrStartY = 0;

                if (gameData["npc_list"].AsArray()[i]["udlr_start_x"] != null)
                    udlrStartX = (int)gameData["npc_list"].AsArray()[i]["udlr_start_x"];
                if (gameData["npc_list"].AsArray()[i]["udlr_start_y"] != null)
                    udlrStartY = (int)gameData["npc_list"].AsArray()[i]["udlr_start_y"];

                Npc.NpcEntry npcEntry = new Npc.NpcEntry(id, x, y, moves, udlrStartX, udlrStartY);

                npcEntry.Name = (string)gameData["npc_list"].AsArray()[i]["name"];
                npcEntry.AdminDescription = (string)gameData["npc_list"].AsArray()[i]["admin_description"];
                npcEntry.ShortDescription = (string)gameData["npc_list"].AsArray()[i]["short_description"];
                npcEntry.LongDescription = (string)gameData["npc_list"].AsArray()[i]["long_description"];


                if (gameData["npc_list"].AsArray()[i]["stay_on"] != null)
                    npcEntry.StayOn = (string)gameData["npc_list"].AsArray()[i]["stay_on"];
                if (gameData["npc_list"].AsArray()[i]["requires_questid_completed"] != null)
                    npcEntry.RequiresQuestIdCompleted = (int)gameData["npc_list"].AsArray()[i]["requires_questid_completed"];
                if (gameData["npc_list"].AsArray()[i]["requires_questid_not_completed"] != null)
                    npcEntry.RequiresQuestIdNotCompleted = (int)gameData["npc_list"].AsArray()[i]["requires_questid_not_completed"];
                if (gameData["npc_list"].AsArray()[i]["udlr_script"] != null)
                    npcEntry.UDLRScript = (string)gameData["npc_list"].AsArray()[i]["udlr_script"];

                npcEntry.AdminOnly = (bool)gameData["npc_list"].AsArray()[i]["admin_only"];
                npcEntry.LibarySearchable = (bool)gameData["npc_list"].AsArray()[i]["libary_searchable"];
                npcEntry.IconId = (int)gameData["npc_list"].AsArray()[i]["icon_id"];

                Logger.DebugPrint("NPC ID:" + npcEntry.Id.ToString() + " NAME: " + npcEntry.Name);
                List<Npc.NpcChat> chats = new List<Npc.NpcChat>();
                int totalNpcChat = gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray().Count;
                for (int ii = 0; ii < totalNpcChat; ii++)
                {
                    Npc.NpcChat npcChat = new Npc.NpcChat();
                    npcChat.Id = (int)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["chatpoint_id"];
                    npcChat.ChatText = (string)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["chat_text"];
                    npcChat.ActivateQuestId = (int)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["activate_questid"];

                    Logger.DebugPrint("CHATPOINT ID: " + npcChat.Id.ToString() + " TEXT: " + npcChat.ChatText);
                    int totalNpcReply = gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray().Count;
                    List<Npc.NpcReply> replys = new List<Npc.NpcReply>();
                    for (int iii = 0; iii < totalNpcReply; iii++)
                    {
                        Npc.NpcReply npcReply = new Npc.NpcReply();
                        npcReply.Id = (int)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["reply_id"];
                        npcReply.ReplyText = (string)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["reply_text"];
                        npcReply.GotoChatpoint = (int)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["goto_chatpoint"];

                        if (gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_completed"] != null)
                            npcReply.RequiresQuestIdCompleted = (int)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_completed"];

                        if (gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_not_completed"] != null)
                            npcReply.RequiresQuestIdNotCompleted = (int)gameData["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_not_completed"];

                        Logger.DebugPrint("REPLY ID: " + npcReply.Id.ToString() + " TEXT: " + npcReply.ReplyText);
                        replys.Add(npcReply);

                    }
                    npcChat.Replies = replys.ToArray();
                    chats.Add(npcChat);
                }
                npcEntry.Chatpoints = chats.ToArray();
                Npc.AddNpc(npcEntry);
            }
        }

        private static void registerQuests()
        {
            Logger.DebugPrint("Registering Quests: ");
            int totalQuests = gameData["quest_list"].AsArray().Count;
            for (int i = 0; i < totalQuests; i++)
            {
                Quest.QuestEntry quest = new Quest.QuestEntry();
                quest.Id = (int)gameData["quest_list"].AsArray()[i]["id"];
                quest.Notes = (string)gameData["quest_list"].AsArray()[i]["notes"];
                if (gameData["quest_list"].AsArray()[i]["title"] != null)
                    quest.Title = (string)gameData["quest_list"].AsArray()[i]["title"];
                quest.RequiresQuestIdCompleteStatsMenu = gameData["quest_list"].AsArray()[i]["requires_questid_statsmenu"].Deserialize<int[]>();
                if (gameData["quest_list"].AsArray()[i]["alt_activation"] != null)
                {
                    quest.AltActivation = new Quest.QuestAltActivation();
                    quest.AltActivation.Type = (string)gameData["quest_list"].AsArray()[i]["alt_activation"]["type"];
                    quest.AltActivation.ActivateX = (int)gameData["quest_list"].AsArray()[i]["alt_activation"]["x"];
                    quest.AltActivation.ActivateY = (int)gameData["quest_list"].AsArray()[i]["alt_activation"]["y"];
                }
                quest.Tracked = (bool)gameData["quest_list"].AsArray()[i]["tracked"];
                quest.MaxRepeats = (int)gameData["quest_list"].AsArray()[i]["max_repeats"];
                quest.MoneyCost = (int)gameData["quest_list"].AsArray()[i]["money_cost"];
                int itemsRequiredCount = gameData["quest_list"].AsArray()[i]["items_required"].AsArray().Count;

                List<Quest.QuestItemInfo> itmInfo = new List<Quest.QuestItemInfo>();
                for (int ii = 0; ii < itemsRequiredCount; ii++)
                {
                    Quest.QuestItemInfo itemInfo = new Quest.QuestItemInfo();
                    itemInfo.ItemId = (int)gameData["quest_list"].AsArray()[i]["items_required"].AsArray()[ii]["item_id"];
                    itemInfo.Quantity = (int)gameData["quest_list"].AsArray()[i]["items_required"].AsArray()[ii]["quantity"];
                    itmInfo.Add(itemInfo);
                }
                quest.ItemsRequired = itmInfo.ToArray();
                if (gameData["quest_list"].AsArray()[i]["fail_npc_chat"] != null)
                    quest.FailNpcChat = (string)gameData["quest_list"].AsArray()[i]["fail_npc_chat"];
                quest.MoneyEarned = (int)gameData["quest_list"].AsArray()[i]["money_gained"];

                int itemsGainedCount = gameData["quest_list"].AsArray()[i]["items_gained"].AsArray().Count;
                itmInfo = new List<Quest.QuestItemInfo>();
                for (int ii = 0; ii < itemsGainedCount; ii++)
                {
                    Quest.QuestItemInfo itemInfo = new Quest.QuestItemInfo();
                    itemInfo.ItemId = (int)gameData["quest_list"].AsArray()[i]["items_gained"].AsArray()[ii]["item_id"];
                    itemInfo.Quantity = (int)gameData["quest_list"].AsArray()[i]["items_gained"].AsArray()[ii]["quantity"];
                    itmInfo.Add(itemInfo);
                }
                quest.ItemsEarned = itmInfo.ToArray();

                quest.QuestPointsEarned = (int)gameData["quest_list"].AsArray()[i]["quest_points"];
                quest.SetNpcChatpoint = (int)gameData["quest_list"].AsArray()[i]["set_npc_chatpoint"];
                quest.GotoNpcChatpoint = (int)gameData["quest_list"].AsArray()[i]["goto_npc_chatpoint"];
                if (gameData["quest_list"].AsArray()[i]["warp_x"] != null)
                    quest.WarpX = (int)gameData["quest_list"].AsArray()[i]["warp_x"];
                if (gameData["quest_list"].AsArray()[i]["warp_y"] != null)
                    quest.WarpY = (int)gameData["quest_list"].AsArray()[i]["warp_y"];
                if (gameData["quest_list"].AsArray()[i]["success_message"] != null)
                    quest.SuccessMessage = (string)gameData["quest_list"].AsArray()[i]["success_message"];
                if (gameData["quest_list"].AsArray()[i]["success_npc_chat"] != null)
                    quest.SuccessNpcChat = (string)gameData["quest_list"].AsArray()[i]["success_npc_chat"];
                if (gameData["quest_list"].AsArray()[i]["requires_awardid"] != null)
                    quest.AwardRequired = (int)gameData["quest_list"].AsArray()[i]["requires_awardid"];
                quest.RequiresQuestIdCompleted = gameData["quest_list"].AsArray()[i]["requires_questid_completed"].Deserialize<int[]>();
                quest.RequiresQuestIdNotCompleted = gameData["quest_list"].AsArray()[i]["requires_questid_not_completed"].Deserialize<int[]>();
                quest.HideReplyOnFail = (bool)gameData["quest_list"].AsArray()[i]["hide_reply_on_fail"];
                if (gameData["quest_list"].AsArray()[i]["difficulty"] != null)
                    quest.Difficulty = (string)gameData["quest_list"].AsArray()[i]["difficulty"];
                if (gameData["quest_list"].AsArray()[i]["author"] != null)
                    quest.Author = (string)gameData["quest_list"].AsArray()[i]["author"];
                if (gameData["quest_list"].AsArray()[i]["chained_questid"] != null)
                    quest.ChainedQuestId = (int)gameData["quest_list"].AsArray()[i]["chained_questid"];
                quest.Minigame = (bool)gameData["quest_list"].AsArray()[i]["minigame"];

                Logger.DebugPrint("Registered Quest: " + quest.Id);
                Quest.AddQuestEntry(quest);
            }
        }

        private static void registerShops()
        {

            int totalShops = gameData["shop_list"].AsArray().Count;
            for (int i = 0; i < totalShops; i++)
            {
                int id = (int)gameData["shop_list"].AsArray()[i]["id"];
                int[] item_list = gameData["shop_list"].AsArray()[i]["stocks_itemids"].Deserialize<int[]>();
                Shop shop = new Shop(item_list, id);
                shop.BuyPricePercentage = (int)gameData["shop_list"].AsArray()[i]["buy_percent"];
                shop.SellPricePercentage = (int)gameData["shop_list"].AsArray()[i]["sell_percent"];
                shop.BuysItemTypes = gameData["shop_list"].AsArray()[i]["buys_item_types"].Deserialize<string[]>();

                Logger.DebugPrint("Registered Shop ID: " + shop.Id + " Selling items at " + shop.SellPricePercentage + "% and buying at " + shop.BuyPricePercentage);
            }
        }

        private static void registerAwards()
        {
            int totalAwards = gameData["award_list"].AsArray().Count;
            Award.GlobalAwardList = new Award.AwardEntry[totalAwards];
            for (int i = 0; i < totalAwards; i++)
            {

                Award.AwardEntry award = new Award.AwardEntry();
                award.Id = (int)gameData["award_list"].AsArray()[i]["id"];
                award.Sort = (int)gameData["award_list"].AsArray()[i]["sort_by"];
                award.Title = (string)gameData["award_list"].AsArray()[i]["title"];
                award.IconId = (int)gameData["award_list"].AsArray()[i]["icon_id"];
                award.MoneyBonus = (int)gameData["award_list"].AsArray()[i]["earn_money"];
                award.CompletionText = (string)gameData["award_list"].AsArray()[i]["on_complete_text"];
                award.Description = (string)gameData["award_list"].AsArray()[i]["description"];

                Award.GlobalAwardList[i] = award;

                Logger.DebugPrint("Registered Award ID: " + award.Id + " - " + award.Title);
            }
        }
        private static void registerAbuseReportReasons()
        {
            int totalAbuseReportReasons = gameData["messages"]["meta"]["abuse_report"]["reasons"].AsArray().Count;
            for (int i = 0; i < totalAbuseReportReasons; i++)
            {
                AbuseReport.ReportReason reason = new AbuseReport.ReportReason();
                reason.Id = (string)gameData["messages"]["meta"]["abuse_report"]["reasons"].AsArray()[i]["id"];
                reason.Name = (string)gameData["messages"]["meta"]["abuse_report"]["reasons"].AsArray()[i]["name"];
                reason.Meta = (string)gameData["messages"]["meta"]["abuse_report"]["reasons"].AsArray()[i]["meta"];
                AbuseReport.AddReason(reason);
                Logger.DebugPrint("Registered Abuse Report Reason: " + reason.Name);
            }
        }
        private static void registerOverlayTileDepth()
        {
            List<Map.TileDepth> overlayTilesDepth = new List<Map.TileDepth>();
            int totalOverlayTileDepth = gameData["tile_paramaters"]["overlay_tiles"].AsArray().Count;
            for (int i = 0; i < totalOverlayTileDepth; i++)
            {
                Map.TileDepth tileDepth = new Map.TileDepth();
                tileDepth.Passable = (bool)gameData["tile_paramaters"]["overlay_tiles"].AsArray()[i]["passable"];
                tileDepth.ShowPlayer = (bool)gameData["tile_paramaters"]["overlay_tiles"].AsArray()[i]["show_player"];
                Logger.DebugPrint("Registered Overlay Tile: " + i + " Depth; Passable: " + tileDepth.Passable + " ShowPlayer: " + tileDepth.ShowPlayer);
                overlayTilesDepth.Add(tileDepth);
            }
            Map.OverlayTileDepth = overlayTilesDepth.ToArray();
        }
        private static void registerTerrianTileTypes()
        {
            List<Map.TerrainTile> terrainTiles = new List<Map.TerrainTile>();
            int totalTerrainTiles = gameData["tile_paramaters"]["terrain_tiles"].AsArray().Count;
            for (int i = 0; i < totalTerrainTiles; i++)
            {
                Map.TerrainTile tile = new Map.TerrainTile();
                tile.Passable = (bool)gameData["tile_paramaters"]["terrain_tiles"].AsArray()[i]["passable"];
                tile.Type = (string)gameData["tile_paramaters"]["terrain_tiles"].AsArray()[i]["tile_type"];
                Logger.DebugPrint("Registered Tile Information: " + i + " Passable: " + tile.Passable + " Type: " + tile.Type);
                terrainTiles.Add(tile);
            }
            Map.TerrainTiles = terrainTiles.ToArray();
        }
        private static void registerInns()
        {
            int totalInns = gameData["inns"].AsArray().Count;
            for (int i = 0; i < totalInns; i++)
            {
                int id = (int)gameData["inns"].AsArray()[i]["id"];
                int[] restsOffered = gameData["inns"].AsArray()[i]["rests_offered"].Deserialize<int[]>();
                int[] mealsOffered = gameData["inns"].AsArray()[i]["meals_offered"].Deserialize<int[]>();
                int buyPercent = (int)gameData["inns"].AsArray()[i]["buy_percent"];
                Inn inn = new Inn(id, restsOffered, mealsOffered, buyPercent);

                Logger.DebugPrint("Registered Inn: " + inn.Id + " Buying at: " + inn.BuyPercentage.ToString() + "%!");
            }
        }

        private static void registerPoets()
        {
            int totalPoets = gameData["poetry"].AsArray().Count;
            for (int i = 0; i < totalPoets; i++)
            {
                Brickpoet.PoetryEntry entry = new Brickpoet.PoetryEntry();
                entry.Id = (int)gameData["poetry"].AsArray()[i]["id"];
                entry.Word = (string)gameData["poetry"].AsArray()[i]["word"];
                entry.Room = (int)gameData["poetry"].AsArray()[i]["room_id"];
                Brickpoet.AddPoetEntry(entry);

                Logger.DebugPrint("Registered poet: " + entry.Id.ToString() + " word: " + entry.Word + " in room " + entry.Room.ToString());
            }
        }

        private static void registerBreeds()
        {
            int totalBreeds = gameData["horses"]["breeds"].AsArray().Count;
            for (int i = 0; i < totalBreeds; i++)
            {
                HorseInfo.Breed horseBreed = new HorseInfo.Breed();

                horseBreed.Id = (int)gameData["horses"]["breeds"].AsArray()[i]["id"];
                horseBreed.Name = (string)gameData["horses"]["breeds"].AsArray()[i]["name"];
                horseBreed.Description = (string)gameData["horses"]["breeds"].AsArray()[i]["description"];

                int speed = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["speed"];
                int strength = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["strength"];
                int conformation = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["conformation"];
                int agility = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["agility"];
                int inteligence = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["inteligence"];
                int endurance = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["endurance"];
                int personality = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["personality"];
                int height = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["height"];
                horseBreed.BaseStats = new HorseInfo.AdvancedStats(null, speed, strength, conformation, agility, inteligence, endurance, personality, height);
                horseBreed.BaseStats.MinHeight = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["min_height"];
                horseBreed.BaseStats.MaxHeight = (int)gameData["horses"]["breeds"].AsArray()[i]["base_stats"]["max_height"];

                horseBreed.Colors = gameData["horses"]["breeds"].AsArray()[i]["colors"].Deserialize<string[]>();
                horseBreed.SpawnOn = (string)gameData["horses"]["breeds"].AsArray()[i]["spawn_on"];
                horseBreed.SpawnInArea = (string)gameData["horses"]["breeds"].AsArray()[i]["spawn_area"];
                horseBreed.Swf = (string)gameData["horses"]["breeds"].AsArray()[i]["swf"];
                horseBreed.Type = (string)gameData["horses"]["breeds"].AsArray()[i]["type"];

                HorseInfo.AddBreed(horseBreed);
                Logger.DebugPrint("Registered Horse Breed: #" + horseBreed.Id + ": " + horseBreed.Name);
            }
        }

        private static void registerBreedPricesPawneerOrder()
        {
            int totalBreedPrices = gameData["horses"]["pawneer_base_price"].AsArray().Count;
            for (int i = 0; i < totalBreedPrices; i++)
            {
                int id = (int)gameData["horses"]["pawneer_base_price"].AsArray()[i]["breed_id"];
                int price = (int)gameData["horses"]["pawneer_base_price"].AsArray()[i]["price"];
                Pawneer pawneerPricing = new Pawneer(id, price);
                Pawneer.AddPawneerPriceModel(pawneerPricing);
                Logger.DebugPrint("Registered Pawneer Base Price " + pawneerPricing.BreedId + " for $" + pawneerPricing.BasePrice.ToString("N0", CultureInfo.InvariantCulture));
            }
        }

        private static void registerHorseCategorys()
        {
            int totalCategories = gameData["horses"]["categorys"].AsArray().Count;
            for (int i = 0; i < totalCategories; i++)
            {
                HorseInfo.Category category = new HorseInfo.Category();
                category.Name = (string)gameData["horses"]["categorys"].AsArray()[i]["name"];
                category.MetaOthers = (string)gameData["horses"]["categorys"].AsArray()[i]["message_others"];
                category.Meta = (string)gameData["horses"]["categorys"].AsArray()[i]["message"];
                HorseInfo.AddHorseCategory(category);
                Logger.DebugPrint("Registered horse category type: " + category.Name);
            }
        }

        private static void registerTrackedItems()
        {

            int totalTrackedItems = gameData["messages"]["meta"]["misc_stats"]["tracked_items"].AsArray().Count;
            for (int i = 0; i < totalTrackedItems; i++)
            {
                Tracking.TrackedItemStatsMenu trackedItem = new Tracking.TrackedItemStatsMenu();
                trackedItem.What = (string)gameData["messages"]["meta"]["misc_stats"]["tracked_items"].AsArray()[i]["id"];
                trackedItem.Value = (string)gameData["messages"]["meta"]["misc_stats"]["tracked_items"].AsArray()[i]["value"];
                Tracking.TrackedItemsStatsMenu.Add(trackedItem);
                Logger.DebugPrint("Registered Tracked Item: " + trackedItem.What + " value: " + trackedItem.Value);
            }
        }
        private static void registerVets()
        {
            int totalVets = gameData["services"]["vet"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalVets; i++)
            {
                double cost = (double)gameData["services"]["vet"]["price_multipliers"].AsArray()[i]["cost"];
                int id = (int)gameData["services"]["vet"]["price_multipliers"].AsArray()[i]["id"];
                Vet veternarian = new Vet(id, cost);
                Logger.DebugPrint("Registered Vet: " + veternarian.Id + " selling at: " + veternarian.PriceMultiplier.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void registerGroomers()
        {
            int totalGroomers = gameData["services"]["groomer"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalGroomers; i++)
            {
                double cost = (double)gameData["services"]["groomer"]["price_multipliers"].AsArray()[i]["cost"];
                int id = (int)gameData["services"]["groomer"]["price_multipliers"].AsArray()[i]["id"];
                int max = (int)gameData["services"]["groomer"]["price_multipliers"].AsArray()[i]["max"];
                Groomer groomer = new Groomer(id, cost, max);
                Logger.DebugPrint("Registered Groomer: " + groomer.Id + " selling at: " + groomer.PriceMultiplier.ToString(CultureInfo.InvariantCulture));
            }

        }

        private static void registerFarriers()
        {
            int totalFarriers = gameData["services"]["farrier"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalFarriers; i++)
            {
                int id = (int)gameData["services"]["farrier"]["price_multipliers"].AsArray()[i]["id"];
                int steel = (int)gameData["services"]["farrier"]["price_multipliers"].AsArray()[i]["steel"];
                int steelcost = (int)gameData["services"]["farrier"]["price_multipliers"].AsArray()[i]["steel_cost"];
                int iron = (int)gameData["services"]["farrier"]["price_multipliers"].AsArray()[i]["iron"];
                int ironcost = (int)gameData["services"]["farrier"]["price_multipliers"].AsArray()[i]["iron_cost"];

                Farrier farrier = new Farrier(id, steel, steelcost, iron, ironcost);
                Logger.DebugPrint("Registered Farrier: " + farrier.Id);
            }
        }

        private static void registerBarns()
        {
            int totalBarns = gameData["services"]["barn"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalBarns; i++)
            {
                int id = (int)gameData["services"]["barn"]["price_multipliers"].AsArray()[i]["id"];
                double tired_cost = (double)gameData["services"]["barn"]["price_multipliers"].AsArray()[i]["tired_cost"];
                double hunger_cost = (double)gameData["services"]["barn"]["price_multipliers"].AsArray()[i]["hunger_cost"];
                double thirst_cost = (double)gameData["services"]["barn"]["price_multipliers"].AsArray()[i]["thirst_cost"];


                Barn barn = new Barn(id, tired_cost, hunger_cost, thirst_cost);
                Logger.DebugPrint("Registered Barn: " + barn.Id);
            }
        }

        private static void registerLibaryBooks()
        {
            int totalBooks = gameData["books"].AsArray().Count;
            for (int i = 0; i < totalBooks; i++)
            {
                int id = (int)gameData["books"][i]["id"];
                string author = (string)gameData["books"][i]["author"];
                string title = (string)gameData["books"][i]["title"];
                string text = (string)gameData["books"][i]["text"];
                Book book = new Book(id, title, author, text);
                Logger.DebugPrint("Registered Library Book: " + book.Id + " " + book.Title + " by " + book.Author);
            }
        }

        private static void registerCrafts()
        {
            int totalWorkshops = gameData["workshop"].AsArray().Count;
            for (int i = 0; i < totalWorkshops; i++)
            {
                Workshop wkShop = new Workshop();
                wkShop.X = (int)gameData["workshop"].AsArray()[i]["pos_x"];
                wkShop.Y = (int)gameData["workshop"].AsArray()[i]["pos_y"];
                int totalCraftableItems = gameData["workshop"].AsArray()[i]["craftable_items"].AsArray().Count;
                for (int ii = 0; ii < totalCraftableItems; ii++)
                {
                    Workshop.CraftableItem craftableItem = new Workshop.CraftableItem();
                    craftableItem.Id = (int)gameData["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["id"];
                    craftableItem.GiveItemId = (int)gameData["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["give_item"];
                    craftableItem.MoneyCost = (int)gameData["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["money_cost"];
                    int totalItemsRequired = gameData["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["required_items"].AsArray().Count;
                    for (int iii = 0; iii < totalItemsRequired; iii++)
                    {
                        Workshop.RequiredItem requiredItem = new Workshop.RequiredItem();
                        requiredItem.RequiredItemId = (int)gameData["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["required_items"].AsArray()[iii]["req_item"];
                        requiredItem.RequiredItemCount = (int)gameData["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["required_items"].AsArray()[iii]["req_quantity"];
                        craftableItem.AddRequiredItem(requiredItem);
                    }
                    wkShop.AddCraftableItem(craftableItem);
                }

                Workshop.AddWorkshop(wkShop);
                Logger.DebugPrint("Registered Workshop at X: " + wkShop.X + " Y: " + wkShop.Y);

            }
        }

        private static void registerRanchBuildings()
        {
            int totalRanchBuildings = gameData["ranch"]["ranch_buildings"]["buildings"].AsArray().Count;
            for (int i = 0; i < totalRanchBuildings; i++)
            {
                int id = (int)gameData["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["id"];
                int cost = (int)gameData["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["cost"];
                string title = (string)gameData["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["title"];
                string description = (string)gameData["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["description"];

                Ranch.RanchBuilding ranchBuilding = new Ranch.RanchBuilding();

                ranchBuilding.Id = id;
                ranchBuilding.Cost = cost;
                ranchBuilding.Title = title;
                ranchBuilding.Description = description;

                Ranch.RanchBuilding.RanchBuildings.Add(ranchBuilding);
                Logger.DebugPrint("Registered Ranch Building: " + ranchBuilding.Title);

            }
        }

        private static void registerRanchUpgrades()
        {
            int totalRanchUpgrades = gameData["ranch"]["ranch_buildings"]["upgrades"].AsArray().Count;
            for (int i = 0; i < totalRanchUpgrades; i++)
            {
                int id = (int)gameData["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["id"];
                int cost = (int)gameData["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["cost"];
                string title = (string)gameData["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["title"];
                string description = (string)gameData["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["description"];

                Ranch.RanchUpgrade ranchUpgrade = new Ranch.RanchUpgrade();

                if (gameData["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["limit"] != null)
                    ranchUpgrade.Limit = (int)gameData["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["limit"];
                ranchUpgrade.Id = id;
                ranchUpgrade.Cost = cost;
                ranchUpgrade.Title = title;
                ranchUpgrade.Description = description;

                Ranch.RanchUpgrade.RanchUpgrades.Add(ranchUpgrade);
                Logger.DebugPrint("Registered Ranch Upgrade: " + ranchUpgrade.Title);

            }
        }

        private static void registerRanchs()
        {
            int totalRanchLocations = gameData["ranch"]["ranch_locations"].AsArray().Count;
            for (int i = 0; i < totalRanchLocations; i++)
            {
                int x = (int)gameData["ranch"]["ranch_locations"].AsArray()[i]["x"];
                int y = (int)gameData["ranch"]["ranch_locations"].AsArray()[i]["y"];
                int id = (int)gameData["ranch"]["ranch_locations"].AsArray()[i]["id"];
                int value = (int)gameData["ranch"]["ranch_locations"].AsArray()[i]["value"];
                Ranch ranchEntry = new Ranch(x, y, id, value);
                Ranch.Ranches.Add(ranchEntry);
                Logger.DebugPrint("Registered Ranch id " + id + " at X: " + ranchEntry.X + " Y: " + ranchEntry.Y);

            }
        }

        private static void registerRiddlerRiddles()
        {
            int totalRiddles = gameData["riddle_room"].AsArray().Count;
            for (int i = 0; i < totalRiddles; i++)
            {
                int id = (int)gameData["riddle_room"].AsArray()[i]["id"];
                string riddle = (string)gameData["riddle_room"].AsArray()[i]["riddle"];
                string[] answers = gameData["riddle_room"].AsArray()[i]["answers"].Deserialize<string[]>();
                string reason = (string)gameData["riddle_room"].AsArray()[i]["reason"];
                Riddler riddlerRiddle = new Riddler(id, riddle, answers, reason);
                Logger.DebugPrint("Registered Riddler Riddle: " + riddlerRiddle.Riddle);

            }
        }

        private static void registerBBCodes()
        {
            int totalBBocdes = gameData["bbcode"].AsArray().Count;
            for (int i = 0; i < totalBBocdes; i++)
            {
                string tag = (string)gameData["bbcode"].AsArray()[i]["tag"];
                string meta = (string)gameData["bbcode"].AsArray()[i]["meta"];
                BBCode code = new BBCode(tag, meta);
                Logger.DebugPrint("Registered BBCODE: " + code.Tag + " to " + code.MetaTranslation);
            }
        }
        private static void registerTrainingPens()
        {
            int totalTrainingPens = gameData["training_pens"].AsArray().Count;
            for (int i = 0; i < totalTrainingPens; i++)
            {
                Trainer trainer = new Trainer();
                trainer.Id = (int)gameData["training_pens"].AsArray()[i]["trainer_id"];
                trainer.ImprovesStat = (string)gameData["training_pens"].AsArray()[i]["improves_stat"];
                trainer.ImprovesAmount = (int)gameData["training_pens"].AsArray()[i]["improves_amount"];
                trainer.ThirstCost = (int)gameData["training_pens"].AsArray()[i]["thirst_cost"];
                trainer.MoodCost = (int)gameData["training_pens"].AsArray()[i]["mood_cost"];
                trainer.HungerCost = (int)gameData["training_pens"].AsArray()[i]["hunger_cost"];
                trainer.MoneyCost = (int)gameData["training_pens"].AsArray()[i]["money_cost"];
                trainer.ExperienceGained = (int)gameData["training_pens"].AsArray()[i]["experience"];
                Trainer.Trainers.Add(trainer);
                Logger.DebugPrint("Registered Training Pen: " + trainer.Id + " for " + trainer.ImprovesStat);
            }

        }

        private static void registerArenas()
        {
            int totalArenas = gameData["arena"]["arena_list"].AsArray().Count;
            for (int i = 0; i < totalArenas; i++)
            {
                int arenaId = (int)gameData["arena"]["arena_list"].AsArray()[i]["arena_id"];
                string arenaType = (string)gameData["arena"]["arena_list"].AsArray()[i]["arena_type"];
                int arenaEntryCost = (int)gameData["arena"]["arena_list"].AsArray()[i]["entry_cost"];
                int raceEvery = (int)gameData["arena"]["arena_list"].AsArray()[i]["race_every"];
                int slots = (int)gameData["arena"]["arena_list"].AsArray()[i]["slots"];
                int timeout = (int)gameData["arena"]["arena_list"].AsArray()[i]["timeout"];

                Arena arena = new Arena(arenaId, arenaType, arenaEntryCost, raceEvery, slots, timeout);
                Logger.DebugPrint("Registered Arena: " + arena.Id.ToString() + " as " + arena.Type);
            }
            Arena.ExpRewards = gameData["arena"]["arena_exp"].Deserialize<int[]>();
        }

        private static void registerLeasers()
        {
            int totalLeasers = gameData["leaser"].AsArray().Count;
            for (int i = 0; i < totalLeasers; i++)
            {
                int breedId = (int)gameData["leaser"].AsArray()[i]["horse"]["breed"];

                int saddle = -1;
                int saddlePad = -1;
                int bridle = -1;

                if (gameData["leaser"].AsArray()[i]["horse"]["tack"]["saddle"] != null)
                    saddle = (int)gameData["leaser"].AsArray()[i]["horse"]["tack"]["saddle"];

                if (gameData["leaser"].AsArray()[i]["horse"]["tack"]["saddle_pad"] != null)
                    saddlePad = (int)gameData["leaser"].AsArray()[i]["horse"]["tack"]["saddle_pad"];

                if (gameData["leaser"].AsArray()[i]["horse"]["tack"]["bridle"] != null)
                    bridle = (int)gameData["leaser"].AsArray()[i]["horse"]["tack"]["bridle"];

                Leaser leaser = new Leaser(breedId, saddle, saddlePad, bridle);
                leaser.LeaseId = (int)gameData["leaser"].AsArray()[i]["lease_id"];
                leaser.ButtonId = (string)gameData["leaser"].AsArray()[i]["button_id"];
                leaser.Info = (string)gameData["leaser"].AsArray()[i]["info"];
                leaser.OnLeaseText = (string)gameData["leaser"].AsArray()[i]["on_lease"];
                leaser.Price = (int)gameData["leaser"].AsArray()[i]["price"];
                leaser.Minutes = (int)gameData["leaser"].AsArray()[i]["minutes"];

                leaser.Color = (string)gameData["leaser"].AsArray()[i]["horse"]["color"];
                leaser.Gender = (string)gameData["leaser"].AsArray()[i]["horse"]["gender"];
                leaser.Height = (int)gameData["leaser"].AsArray()[i]["horse"]["hands"];
                leaser.Experience = (int)gameData["leaser"].AsArray()[i]["horse"]["exp"];
                leaser.HorseName = (string)gameData["leaser"].AsArray()[i]["horse"]["name"];

                leaser.Health = (int)gameData["leaser"].AsArray()[i]["horse"]["basic_stats"]["health"];
                leaser.Hunger = (int)gameData["leaser"].AsArray()[i]["horse"]["basic_stats"]["hunger"];
                leaser.Thirst = (int)gameData["leaser"].AsArray()[i]["horse"]["basic_stats"]["thirst"];
                leaser.Mood = (int)gameData["leaser"].AsArray()[i]["horse"]["basic_stats"]["mood"];
                leaser.Tiredness = (int)gameData["leaser"].AsArray()[i]["horse"]["basic_stats"]["energy"];
                leaser.Groom = (int)gameData["leaser"].AsArray()[i]["horse"]["basic_stats"]["groom"];
                leaser.Shoes = (int)gameData["leaser"].AsArray()[i]["horse"]["basic_stats"]["shoes"];

                leaser.Speed = (int)gameData["leaser"].AsArray()[i]["horse"]["advanced_stats"]["speed"];
                leaser.Strength = (int)gameData["leaser"].AsArray()[i]["horse"]["advanced_stats"]["strength"];
                leaser.Conformation = (int)gameData["leaser"].AsArray()[i]["horse"]["advanced_stats"]["conformation"];
                leaser.Agility = (int)gameData["leaser"].AsArray()[i]["horse"]["advanced_stats"]["agility"];
                leaser.Endurance = (int)gameData["leaser"].AsArray()[i]["horse"]["advanced_stats"]["endurance"];
                leaser.Inteligence = (int)gameData["leaser"].AsArray()[i]["horse"]["advanced_stats"]["inteligence"];
                leaser.Personality = (int)gameData["leaser"].AsArray()[i]["horse"]["advanced_stats"]["personality"];

                Leaser.AddHorseLeaser(leaser);
                Logger.DebugPrint("Registered Leaser: " + leaser.LeaseId.ToString() + " For a " + leaser.HorseName);
            }
        }

        private static void registerSocials()
        {
            int totalSocials = gameData["social_types"].AsArray().Count;
            for (int i = 0; i < totalSocials; i++)
            {
                string socialType = (string)gameData["social_types"].AsArray()[i]["type"];
                int totalSocialsOfType = gameData["social_types"].AsArray()[i]["socials"].AsArray().Count;
                for (int ii = 0; ii < totalSocialsOfType; ii++)
                {
                    SocialType.Social social = new SocialType.Social();

                    social.Id = (int)gameData["social_types"].AsArray()[i]["socials"].AsArray()[ii]["social_id"];
                    social.ButtonName = (string)gameData["social_types"].AsArray()[i]["socials"].AsArray()[ii]["button_name"];
                    social.ForSender = (string)gameData["social_types"].AsArray()[i]["socials"].AsArray()[ii]["for_sender"];
                    social.ForTarget = (string)gameData["social_types"].AsArray()[i]["socials"].AsArray()[ii]["for_target"];
                    social.ForEveryone = (string)gameData["social_types"].AsArray()[i]["socials"].AsArray()[ii]["for_everyone"];
                    social.SoundEffect = (string)gameData["social_types"].AsArray()[i]["socials"].AsArray()[ii]["sound_effect"];

                    SocialType.AddNewSocial(socialType, social);
                    Logger.DebugPrint("Registered Social: " + social.ButtonName);
                }
            }
        }

        private static void registerRealTimeRiddleEvents()
        {
            int totalRealTimeRiddles = gameData["events"]["real_time_riddle"].AsArray().Count;
            for (int i = 0; i < totalRealTimeRiddles; i++)
            {
                int id = (int)gameData["events"]["real_time_riddle"].AsArray()[i]["id"];
                string riddleText = (string)gameData["events"]["real_time_riddle"].AsArray()[i]["text"];
                string[] riddleAnswers = gameData["events"]["real_time_riddle"].AsArray()[i]["answers"].Deserialize<string[]>();
                int reward = (int)gameData["events"]["real_time_riddle"].AsArray()[i]["money_reward"];

                RealTimeRiddle riddle = new RealTimeRiddle(id, riddleText, riddleAnswers, reward);

                Logger.DebugPrint("Registered Riddle #" + riddle.RiddleId.ToString());
            }
        }

        private static void registerRealTimeQuizEvents()
        {
            int totalRealTimeQuizCategories = gameData["events"]["real_time_quiz"].AsArray().Count;
            RealTimeQuiz.Categories = new RealTimeQuiz.QuizCategory[totalRealTimeQuizCategories]; // initalize array
            for (int i = 0; i < totalRealTimeQuizCategories; i++)
            {
                string name = (string)gameData["events"]["real_time_quiz"].AsArray()[i]["name"];
                int totalQuestions = gameData["events"]["real_time_quiz"].AsArray()[i]["questons"].AsArray().Count;

                RealTimeQuiz.QuizCategory quizCategory = new RealTimeQuiz.QuizCategory();
                quizCategory.Name = name;
                quizCategory.Questions = new RealTimeQuiz.QuizQuestion[totalQuestions];

                for (int ii = 0; ii < totalQuestions; ii++)
                {
                    quizCategory.Questions[ii] = new RealTimeQuiz.QuizQuestion(quizCategory);
                    quizCategory.Questions[ii].Question = (string)gameData["events"]["real_time_quiz"].AsArray()[i]["questons"].AsArray()[ii]["question"];
                    quizCategory.Questions[ii].Answers = gameData["events"]["real_time_quiz"].AsArray()[i]["questons"].AsArray()[ii]["answers"].Deserialize<string[]>();
                    Logger.DebugPrint("Registered Real Time Quiz Question: " + quizCategory.Questions[ii].Question);
                }

                RealTimeQuiz.Categories[i] = quizCategory;

                Logger.DebugPrint("Registered Real Time Quiz Category: " + name);
            }
        }
        private static void registerRandomEvents()
        {
            int totalRandomEvent = gameData["events"]["random_events"].AsArray().Count;
            for (int i = 0; i < totalRandomEvent; i++)
            {
                int minmoney = 0;
                int maxmoney = 0;
                int lowerHorseHealth = 0;
                int giveObj = 0;

                int id = (int)gameData["events"]["random_events"].AsArray()[i]["id"];
                string txt = (string)gameData["events"]["random_events"].AsArray()[i]["text"];

                if (gameData["events"]["random_events"].AsArray()[i]["min_money"] != null)
                    minmoney = (int)gameData["events"]["random_events"].AsArray()[i]["min_money"];
                if (gameData["events"]["random_events"].AsArray()[i]["max_money"] != null)
                    maxmoney = (int)gameData["events"]["random_events"].AsArray()[i]["max_money"];
                if (gameData["events"]["random_events"].AsArray()[i]["lower_horse_health"] != null)
                    lowerHorseHealth = (int)gameData["events"]["random_events"].AsArray()[i]["lower_horse_health"];
                if (gameData["events"]["random_events"].AsArray()[i]["give_object"] != null)
                    giveObj = (int)gameData["events"]["random_events"].AsArray()[i]["give_object"];

                new RandomEvent(id, txt, minmoney, maxmoney, lowerHorseHealth, giveObj);
                Logger.DebugPrint("Registered Random Event: " + txt);
            }
        }
        public static void ReadGamedata()
        {
            readGamedataFiles();
            registerTowns();
            registerZones();
            registerAreas();
            registerIsles();
            registerWaypoints();
            registerSpecialTiles();
            registerChatWarningReasons();
            registerFilteredWords();
            registerWordCorrections();
            registerTransportPoints();
            registerTransportLocations();
            registerItems();
            registerThrowables();
            registerNpcs();
            registerQuests();
            registerShops();
            registerAwards();
            registerAbuseReportReasons();
            registerOverlayTileDepth();
            registerTerrianTileTypes();
            registerInns();
            registerPoets();
            registerBreeds();
            registerBreedPricesPawneerOrder();
            registerHorseCategorys();
            registerTrackedItems();
            registerVets();
            registerGroomers();
            registerFarriers();
            registerBarns();
            registerLibaryBooks();
            registerCrafts();
            registerRanchBuildings();
            registerRanchUpgrades();
            registerRanchs();
            registerRiddlerRiddles();
            registerBBCodes();
            registerTrainingPens();
            registerArenas();
            registerLeasers();
            registerSocials();
            registerRealTimeRiddleEvents();
            registerRealTimeQuizEvents();
            registerRandomEvents();

            // the rest is easier;

            HorseInfo.HorseNames = gameData["horses"]["names"].Deserialize<string[]>(); 

            Item.Present = (int)gameData["item"]["special"]["present"];
            Item.MailMessage = (int)gameData["item"]["special"]["mail_message"];
            Item.DorothyShoes = (int)gameData["item"]["special"]["dorothy_shoes"];
            Item.PawneerOrder = (int)gameData["item"]["special"]["pawneer_order"];
            Item.Telescope = (int)gameData["item"]["special"]["telescope"];
            Item.Pitchfork = (int)gameData["item"]["special"]["pitchfork"];
            Item.WishingCoin = (int)gameData["item"]["special"]["wishing_coin"];
            Item.FishingPole = (int)gameData["item"]["special"]["fishing_poll"];
            Item.Earthworm = (int)gameData["item"]["special"]["earthworm"];
            Item.BirthdayToken = (int)gameData["item"]["special"]["birthday_token"];
            Item.WaterBalloon = (int)gameData["item"]["special"]["water_balloon"];
            Item.ModSplatterball = (int)gameData["item"]["special"]["mod_splatterball"];
            Item.MagicBean = (int)gameData["item"]["special"]["magic_bean"];
            Item.MagicDroplet = (int)gameData["item"]["special"]["magic_droplet"];
            Item.Ruby = (int)gameData["item"]["special"]["ruby"];

            Item.StallionTradingCard = (int)gameData["item"]["special"]["stallion_trading_card"];
            Item.MareTradingCard = (int)gameData["item"]["special"]["mare_trading_card"];
            Item.ColtTradingCard = (int)gameData["item"]["special"]["colt_trading_card"];
            Item.FillyTradingCard = (int)gameData["item"]["special"]["filly_trading_card"];

            GameServer.IdleWarning = (int)gameData["messages"]["disconnect"]["client_timeout"]["warn_after"];
            GameServer.IdleTimeout = (int)gameData["messages"]["disconnect"]["client_timeout"]["kick_after"];

            ChatMsg.PrivateMessageSound = (string)gameData["messages"]["chat"]["pm_sound"];

            // HISP Specific ...
            Messages.HISPHelpCommandUsageFormat = (string)gameData["hisp_specific"]["HISP_help_command_usage_format"];

            // New Users

            Messages.NewUserMessage = (string)gameData["messages"]["new_user"]["starting_message"];
            Map.NewUserStartX = (int)gameData["messages"]["new_user"]["starting_x"];
            Map.NewUserStartY = (int)gameData["messages"]["new_user"]["starting_y"];

            // Timed Messages

            Messages.PlaytimeMessageFormat = (string)gameData["messages"]["timed_messages"]["playtime_message"];
            Messages.RngMessages = gameData["messages"]["timed_messages"]["rng_message"].Deserialize<string[]>();

            // Auto Sell
            Messages.AutoSellNotStandingInSamePlace = (string)gameData["messages"]["meta"]["auto_sell"]["not_standing_sameplace"];
            Messages.AutoSellSuccessFormat = (string)gameData["messages"]["meta"]["auto_sell"]["success"];
            Messages.AutoSellInsufficentFunds = (string)gameData["messages"]["meta"]["auto_sell"]["insufficent_money"];
            Messages.AutoSellTooManyHorses = (string)gameData["messages"]["meta"]["auto_sell"]["toomany_horses"];
            Messages.AutoSellYouSoldHorseFormat = (string)gameData["messages"]["meta"]["auto_sell"]["you_sold"];
            Messages.AutoSellYouSoldHorseOfflineFormat = (string)gameData["messages"]["meta"]["auto_sell"]["sold_offline"];

            // Mute Command
            Messages.NowMutingPlayerFormat = (string)gameData["messages"]["meta"]["mute_command"]["now_ignoring_player"];
            Messages.StoppedMutingPlayerFormat = (string)gameData["messages"]["meta"]["mute_command"]["stop_ignoring_player"];

            Messages.PlayerIgnoringYourPrivateMessagesFormat = (string)gameData["messages"]["meta"]["mute_command"]["player_ignoring_your_pm"];
            Messages.PlayerIgnoringYourBuddyRequests = (string)gameData["messages"]["meta"]["mute_command"]["player_ignoring_your_br"];
            Messages.PlayerIgnoringYourSocials = (string)gameData["messages"]["meta"]["mute_command"]["player_ignoring_your_socials"];

            Messages.PlayerIgnoringAllPrivateMessagesFormat = (string)gameData["messages"]["meta"]["mute_command"]["player_ignoring_all_pm"];
            Messages.PlayerIgnoringAllBuddyRequests = (string)gameData["messages"]["meta"]["mute_command"]["player_ignoring_all_br"];
            Messages.PlayerIgnoringAllSocials = (string)gameData["messages"]["meta"]["mute_command"]["player_ignoring_all_socials"];

            Messages.CantSendInMutedChannel = (string)gameData["messages"]["meta"]["mute_command"]["cant_send_in_muted_channel"];
            Messages.CantSendBuddyRequestWhileMuted = (string)gameData["messages"]["meta"]["mute_command"]["cant_send_br_muted"];
            Messages.CantSendPrivateMessageWhileMuted = (string)gameData["messages"]["meta"]["mute_command"]["cant_send_pm_muted"];

            Messages.CantSendPrivateMessagePlayerMutedFormat = (string)gameData["messages"]["meta"]["mute_command"]["cant_send_pm_player_muted"];

            // Chat Errors
            Messages.CantFindPlayerToPrivateMessage = (string)gameData["messages"]["chat_errors"]["cant_find_player"];
            Messages.AdsOnlyOncePerMinute = (string)gameData["messages"]["chat_errors"]["ads_once_per_minute"];
            Messages.GlobalChatLimited = (string)gameData["messages"]["chat_errors"]["global_chats_limited"];
            Messages.GlobalChatTooLong = (string)gameData["messages"]["chat_errors"]["global_too_long"];
            Messages.AdsChatTooLong = (string)gameData["messages"]["chat_errors"]["ads_too_long"];

            // Warp Command

            Messages.SuccessfullyWarpedToPlayer = (string)gameData["messages"]["commands"]["warp"]["player"];
            Messages.SuccessfullyWarpedToLocation = (string)gameData["messages"]["commands"]["warp"]["location"];
            Messages.OnlyUnicornCanWarp = (string)gameData["messages"]["commands"]["warp"]["only_unicorn"];
            Messages.FailedToUnderstandLocation = (string)gameData["messages"]["commands"]["warp"]["location_unknown"];

            // Mod Isle
            Messages.ModSplatterballEarnedYouFormat = (string)gameData["messages"]["mods_revenge"]["awarded_you"];
            Messages.ModSplatterballEarnedOtherFormat = (string)gameData["messages"]["mods_revenge"]["awareded_others"];
            Messages.ModIsleMessage = (string)gameData["messages"]["commands"]["mod_isle"]["message"];
            Map.ModIsleX = (int)gameData["messages"]["commands"]["mod_isle"]["x"];
            Map.ModIsleY = (int)gameData["messages"]["commands"]["mod_isle"]["y"];

            // Rules Isle
            Map.RulesIsleX = (int)gameData["messages"]["commands"]["rules_isle"]["x"];
            Map.RulesIsleY = (int)gameData["messages"]["commands"]["rules_isle"]["y"];
            Messages.RulesIsleSentMessage = (string)gameData["messages"]["commands"]["rules_isle"]["message"];
            Messages.RulesIsleCommandMessageFormat = (string)gameData["messages"]["commands"]["rules_isle"]["command_msg"];

            // Prison Isle
            Map.PrisonIsleX = (int)gameData["messages"]["commands"]["prison_isle"]["x"];
            Map.PrisonIsleY = (int)gameData["messages"]["commands"]["prison_isle"]["y"];
            Messages.PrisonIsleSentMessage = (string)gameData["messages"]["commands"]["prison_isle"]["message"];
            Messages.PrisonIsleCommandMessageFormat = (string)gameData["messages"]["commands"]["prison_isle"]["command_msg"];


            // Tag

            Messages.TagYourItFormat = (string)gameData["messages"]["meta"]["player_interaction"]["tag.tag_player"];
            Messages.TagOtherBuddiesOnlineFormat = (string)gameData["messages"]["meta"]["player_interaction"]["tag.total_buddies"];

            // Add Buddy

            Messages.AddBuddyPending = (string)gameData["messages"]["meta"]["player_interaction"]["add_buddy.add_pending"];
            Messages.AddBuddyOtherPendingFormat = (string)gameData["messages"]["meta"]["player_interaction"]["add_buddy.other_pending"];
            Messages.AddBuddyYourNowBuddiesFormat = (string)gameData["messages"]["meta"]["player_interaction"]["add_buddy.add_confirmed"];
            Messages.AddBuddyDeleteBuddyFormat = (string)gameData["messages"]["meta"]["player_interaction"]["add_buddy.deleted"];

            // Socials

            Messages.SocialButton = (string)gameData["messages"]["meta"]["player_interaction"]["socials"]["socials_button"];
            Messages.SocialMessageFormat = (string)gameData["messages"]["meta"]["player_interaction"]["socials"]["socials_message"];
            Messages.SocialTypeFormat = (string)gameData["messages"]["meta"]["player_interaction"]["socials"]["socials_menu_type"];
            Messages.SocialPlayerNoLongerNearby = (string)gameData["messages"]["meta"]["player_interaction"]["socials"]["no_longer_nearby"];

            // Message Queue 
            Messages.MessageQueueHeader = (string)gameData["messages"]["message_queue"];

            // Random Event
            Messages.RandomEventPrefix = (string)gameData["messages"]["random_event_prefix"];

            // Events : Mods Revenge
            Messages.EventStartModsRevenge = (string)gameData["messages"]["events"]["mods_revenge"]["event_start"];
            Messages.EventEndModsRevenge = (string)gameData["messages"]["events"]["mods_revenge"]["event_end"];

            // Events : Isle Trading Game
            Messages.EventStartIsleTradingGame = (string)gameData["messages"]["events"]["isle_card_trading_game"]["event_start"];
            Messages.EventDisqualifiedIsleTradingGame = (string)gameData["messages"]["events"]["isle_card_trading_game"]["event_disqualified"];
            Messages.EventOnlyOneTypeIsleTradingGame = (string)gameData["messages"]["events"]["isle_card_trading_game"]["event_one_type"];
            Messages.EventOnlyTwoTypeIsleTradingGame = (string)gameData["messages"]["events"]["isle_card_trading_game"]["event_two_type"];
            Messages.EventOnlyThreeTypeIsleTradingGame = (string)gameData["messages"]["events"]["isle_card_trading_game"]["event_three_type"];
            Messages.EventNoneIsleTradingGame = (string)gameData["messages"]["events"]["isle_card_trading_game"]["event_no_cards"];
            Messages.EventWonIsleTradingGame = (string)gameData["messages"]["events"]["isle_card_trading_game"]["event_win"];

            // Events : Water Ballon Game
            Messages.EventStartWaterBallonGame = (string)gameData["messages"]["events"]["water_balloon_game"]["event_start"];
            Messages.EventWonWaterBallonGame = (string)gameData["messages"]["events"]["water_balloon_game"]["event_won"];
            Messages.EventEndWaterBalloonGame = (string)gameData["messages"]["events"]["water_balloon_game"]["event_end"];
            Messages.EventWinnerWaterBalloonGameFormat = (string)gameData["messages"]["events"]["water_balloon_game"]["event_winner"];

            // Events : Real Time Quiz

            Messages.EventMetaRealTimeQuizFormat = (string)gameData["messages"]["events"]["real_time_quiz"]["event_meta"];
            Messages.EventStartRealTimeQuiz = (string)gameData["messages"]["events"]["real_time_quiz"]["event_start"];
            Messages.EventEndRealTimeQuiz = (string)gameData["messages"]["events"]["real_time_quiz"]["event_end"];
            Messages.EventBonusRealTimeQuizFormat = (string)gameData["messages"]["events"]["real_time_quiz"]["event_bonus"];
            Messages.EventWinBonusRealTimeQuizFormat = (string)gameData["messages"]["events"]["real_time_quiz"]["event_win_bonus"];
            Messages.EventWinRealTimeQuizFormat = (string)gameData["messages"]["events"]["real_time_quiz"]["event_win"];
            Messages.EventUnavailableRealTimeQuiz = (string)gameData["messages"]["events"]["real_time_quiz"]["event_unavailable"];
            Messages.EventEnteredRealTimeQuiz = (string)gameData["messages"]["events"]["real_time_quiz"]["event_entered"];
            Messages.EventAlreadyEnteredRealTimeQuiz = (string)gameData["messages"]["events"]["real_time_quiz"]["event_entered_already"];
            Messages.EventQuitRealTimeQuiz = (string)gameData["messages"]["events"]["real_time_quiz"]["event_quit"];

            // Events : Real Time Riddle

            Messages.EventStartRealTimeRiddleFormat = (string)gameData["messages"]["events"]["real_time_riddle"]["event_start"];
            Messages.EventEndRealTimeRiddle = (string)gameData["messages"]["events"]["real_time_riddle"]["event_end"];
            Messages.EventWonRealTimeRiddleForOthersFormat = (string)gameData["messages"]["events"]["real_time_riddle"]["event_won_others"];
            Messages.EventWonRealTimeRiddleForYouFormat = (string)gameData["messages"]["events"]["real_time_riddle"]["event_won_you"];
            Messages.EventAlreadySovledRealTimeRiddle = (string)gameData["messages"]["events"]["real_time_riddle"]["event_solved_already"];

            // Events : Tack Shop Giveaway

            Messages.EventStartTackShopGiveawayFormat = (string)gameData["messages"]["events"]["tack_shop_giveaway"]["event_start"];
            Messages.Event1MinTackShopGiveawayFormat = (string)gameData["messages"]["events"]["tack_shop_giveaway"]["event_1min"];
            Messages.EventWonTackShopGiveawayFormat = (string)gameData["messages"]["events"]["tack_shop_giveaway"]["event_won"];
            Messages.EventEndTackShopGiveawayFormat = (string)gameData["messages"]["events"]["tack_shop_giveaway"]["event_end"];


            // MultiHorses
            Messages.OtherPlayersHere = (string)gameData["messages"]["meta"]["multihorses"]["other_players_here"];
            Messages.MultiHorseSelectOneToJoinWith = (string)gameData["messages"]["meta"]["multihorses"]["select_a_horse"];
            Messages.MultiHorseFormat = (string)gameData["messages"]["meta"]["multihorses"]["horse_format"];

            // 2Player
            Messages.TwoPlayerOtherPlayer = (string)gameData["messages"]["meta"]["two_player"]["other_player"];
            Messages.TwoPlayerPlayerFormat = (string)gameData["messages"]["meta"]["two_player"]["player_name"];
            Messages.TwoPlayerInviteButton = (string)gameData["messages"]["meta"]["two_player"]["invite_button"];
            Messages.TwoPlayerAcceptButton = (string)gameData["messages"]["meta"]["two_player"]["accept_button"];
            Messages.TwoPlayerSentInvite = (string)gameData["messages"]["meta"]["two_player"]["sent_invite"];
            Messages.TwoPlayerPlayingWithFormat = (string)gameData["messages"]["meta"]["two_player"]["playing_with"];

            Messages.TwoPlayerGameInProgressFormat = (string)gameData["messages"]["meta"]["two_player"]["game_in_progress"];

            Messages.TwoPlayerYourInvitedFormat = (string)gameData["messages"]["meta"]["two_player"]["your_invited"];
            Messages.TwoPlayerInvitedFormat = (string)gameData["messages"]["meta"]["two_player"]["you_invited"];
            Messages.TwoPlayerStartingUpGameFormat = (string)gameData["messages"]["meta"]["two_player"]["starting_game"];

            Messages.TwoPlayerGameClosed = (string)gameData["messages"]["meta"]["two_player"]["game_closed"];
            Messages.TwoPlayerGameClosedOther = (string)gameData["messages"]["meta"]["two_player"]["game_closed_other"];

            Messages.TwoPlayerRecordedWinFormat = (string)gameData["messages"]["meta"]["two_player"]["recorded_win"];
            Messages.TwoPlayerRecordedLossFormat = (string)gameData["messages"]["meta"]["two_player"]["recorded_loss"];

            // Trade

            Messages.TradeWithPlayerFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trading_with"];

            Messages.TradeWaitingForOtherDone = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_wait_for_done"];
            Messages.TradeOtherPlayerIsDone = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["other_player_is_done"];
            Messages.TradeFinalReview = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["final_review"];

            Messages.TradeYourOfferingFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["you_offering"];

            Messages.TradeAddItems = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["add_items"];
            Messages.TradeOtherOfferingFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["other_offering"];

            Messages.TradeWhenDoneClick = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["when_done_click"];
            Messages.TradeCancelAnytime = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["cancel_anytime"];
            Messages.TradeAcceptTrade = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["accept_trade"];

            Messages.TradeOfferingNothing = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offering_nothing"];
            Messages.TradeOfferingMoneyFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offering_money"];
            Messages.TradeOfferingItemFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offering_item"];
            Messages.TradeOfferingHorseFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offering_horse"];

            // Trading : What to offer

            Messages.TradeWhatToOfferFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["what_to_offer"];
            Messages.TradeOfferMoney = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offer_money"];

            Messages.TradeOfferHorse = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offer_horse"];
            Messages.TradeOfferHorseFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offer_horse_format"];
            Messages.TradeOfferHorseTacked = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["horse_tacked"];

            Messages.TradeOfferItem = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offer_object"];
            Messages.TradeOfferItemFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offer_object_format"];
            Messages.TradeOfferItemOtherPlayerInvFull = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["offer_object_inv_full"];

            // Trading : Offer Submenu

            Messages.TradeMoneyOfferSubmenuFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["money_offer_submenu"];
            Messages.TradeItemOfferSubmenuFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["object_offer_submenu"];

            // Trading : Messges

            Messages.TradeWaitingForOthersToAcceptMessage = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["waiting_for_other_to_accept"];
            Messages.TradeRequiresBothPlayersMessage = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["requires_both_players"];

            Messages.TradeItemOfferAtleast1 = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["object_offer_atleast_1"];
            Messages.TradeItemOfferTooMuchFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["object_offer_too_much"];
            Messages.TradeMoneyOfferTooMuch = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["money_offer_too_much"];

            Messages.TradeOtherPlayerHasNegativeMoney = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["other_player_has_negative_money"];
            Messages.TradeYouHaveNegativeMoney = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["you_have_negative_money"];


            Messages.TradeAcceptedMessage = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_accepted"];
            Messages.TradeCanceledByYouMessage = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["you_canceled"];
            Messages.TradeCanceledByOtherPlayerFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["other_canceled"];
            Messages.TradeCanceledBecuasePlayerMovedMessage = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_canceled_moved"];
            Messages.TradeCanceledInterupted = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_interupted"];

            Messages.TradeRiddenHorse = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_ridden_horse"];

            Messages.TradeYouCantHandleMoreHorses = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["cant_handle_more_horses"];
            Messages.TradeOtherPlayerCantHandleMoreHorsesFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["other_player_cant_handle_more_horses"];

            Messages.TradeOtherCantCarryMoreItems = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["other_carry_more"];
            Messages.TradeYouCantCarryMoreItems = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["you_cant_carry_more"];

            Messages.TradeYouSpentMoneyMessageFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_spent"];
            Messages.TradeYouReceivedMoneyMessageFormat = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_received"];

            Messages.TradeNotAllowedWhileBidding = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_not_allowed_while_bidding"];
            Messages.TradeNotAllowedWhileOtherBidding = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_not_allowed_while_other_is_bidding"];

            Messages.TradeWillGiveYouTooMuchMoney = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_other_cannot_carry_that_much"];
            Messages.TradeWillGiveOtherTooMuchMoney = (string)gameData["messages"]["meta"]["player_interaction"]["trade"]["trade_you_cannot_carry_that_much"];

            // Player Interation

            Messages.PlayerHereMenuFormat = (string)gameData["messages"]["meta"]["player_interaction"]["menu"];

            Messages.PlayerHereProfileButton = (string)gameData["messages"]["meta"]["player_interaction"]["profiile_button"];
            Messages.PlayerHereSocialButton = (string)gameData["messages"]["meta"]["player_interaction"]["social_button"];
            Messages.PlayerHereTradeButton = (string)gameData["messages"]["meta"]["player_interaction"]["trade_button"];
            Messages.PlayerHereAddBuddyButton = (string)gameData["messages"]["meta"]["player_interaction"]["buddy_button"];
            Messages.PlayerHereTagButton = (string)gameData["messages"]["meta"]["player_interaction"]["tag_button"];
            Messages.PmButton = (string)gameData["messages"]["meta"]["player_interaction"]["pm_button"];


            // Auction
            Messages.AuctionsRunning = (string)gameData["messages"]["meta"]["auction"]["auctions_running"];
            Messages.AuctionPlayersHereFormat = (string)gameData["messages"]["meta"]["auction"]["players_here"];
            Messages.AuctionHorseEntryFormat = (string)gameData["messages"]["meta"]["auction"]["auction_horse_entry"];
            Messages.AuctionAHorse = (string)gameData["messages"]["meta"]["auction"]["auction_horse"];

            Messages.AuctionListHorse = (string)gameData["messages"]["meta"]["auction"]["list_horse"];
            Messages.AuctionHorseListEntryFormat = (string)gameData["messages"]["meta"]["auction"]["horse_list_entry"];
            Messages.AuctionHorseViewButton = (string)gameData["messages"]["meta"]["auction"]["view_button"];
            Messages.AuctionHorseIsTacked = (string)gameData["messages"]["meta"]["auction"]["tacked"];

            Messages.AuctionBidMax = (string)gameData["messages"]["meta"]["auction"]["max_bid"];
            Messages.AuctionBidRaisedFormat = (string)gameData["messages"]["meta"]["auction"]["bid_raised"];
            Messages.AuctionTopBid = (string)gameData["messages"]["meta"]["auction"]["top_bid"];
            Messages.AuctionExistingBidHigher = (string)gameData["messages"]["meta"]["auction"]["existing_higher"];

            Messages.AuctionYouHaveTooManyHorses = (string)gameData["messages"]["meta"]["auction"]["you_have_too_many_horses"];
            Messages.AuctionOnlyOneWinningBidAllowed = (string)gameData["messages"]["meta"]["auction"]["only_one_winning_bid_allowed"];

            Messages.AuctionOneHorsePerPlayer = (string)gameData["messages"]["meta"]["auction"]["one_horse_at_a_time"];
            Messages.AuctionYouveBeenOutbidFormat = (string)gameData["messages"]["meta"]["auction"]["outbid_by"];
            Messages.AuctionCantAffordBid = (string)gameData["messages"]["meta"]["auction"]["cant_afford_bid"];
            Messages.AuctionCantAffordAuctionFee = (string)gameData["messages"]["meta"]["auction"]["cant_afford_listing"];
            Messages.AuctionNoOtherTransactionAllowed = (string)gameData["messages"]["meta"]["auction"]["no_other_transaction_allowed"];

            Messages.AuctionYouBroughtAHorseFormat = (string)gameData["messages"]["meta"]["auction"]["brought_horse"];
            Messages.AuctionNoHorseBrought = (string)gameData["messages"]["meta"]["auction"]["no_one_brought"];
            Messages.AuctionHorseSoldFormat = (string)gameData["messages"]["meta"]["auction"]["horse_sold"];

            Messages.AuctionSoldToFormat = (string)gameData["messages"]["meta"]["auction"]["sold_to"];
            Messages.AuctionNotSold = (string)gameData["messages"]["meta"]["auction"]["not_sold"];
            Messages.AuctionGoingToFormat = (string)gameData["messages"]["meta"]["auction"]["going_to"];

            // Hammock Text
            Messages.HammockText = (string)gameData["messages"]["meta"]["hammock"];

            // Horse Leaser
            Messages.HorseLeaserCantAffordMessage = (string)gameData["messages"]["horse_leaser"]["cant_afford"];
            Messages.HorseLeaserTemporaryHorseAdded = (string)gameData["messages"]["horse_leaser"]["temporary_horse_added"];
            Messages.HorseLeaserHorsesFull = (string)gameData["messages"]["horse_leaser"]["horses_full"];

            Messages.HorseLeaserReturnedToUniterPegasus = (string)gameData["messages"]["horse_leaser"]["returned_to_uniter_pegasus"];

            Messages.HorseLeaserReturnedToUniterFormat = (string)gameData["messages"]["horse_leaser"]["returned_to_uniter"];
            Messages.HorseLeaserReturnedToOwnerFormat = (string)gameData["messages"]["horse_leaser"]["returned_to_owner"];

            // Competitions
            Messages.ArenaResultsMessage = (string)gameData["messages"]["meta"]["arena"]["results"];
            Messages.ArenaPlacingFormat = (string)gameData["messages"]["meta"]["arena"]["placing"];
            Messages.ArenaAlreadyEntered = (string)gameData["messages"]["meta"]["arena"]["already_entered"];

            Messages.ArenaFirstPlace = (string)gameData["messages"]["meta"]["arena"]["first_place"];
            Messages.ArenaSecondPlace = (string)gameData["messages"]["meta"]["arena"]["second_place"];
            Messages.ArenaThirdPlace = (string)gameData["messages"]["meta"]["arena"]["third_place"];
            Messages.ArenaFourthPlace = (string)gameData["messages"]["meta"]["arena"]["fourth_place"];
            Messages.ArenaFifthPlace = (string)gameData["messages"]["meta"]["arena"]["fifth_place"];
            Messages.ArenaSixthPlace = (string)gameData["messages"]["meta"]["arena"]["sixth_place"];

            Messages.ArenaEnteredInto = (string)gameData["messages"]["meta"]["arena"]["enter_into"];
            Messages.ArenaCantAfford = (string)gameData["messages"]["meta"]["arena"]["cant_afford"];

            Messages.ArenaYourScoreFormat = (string)gameData["messages"]["meta"]["arena"]["your_score"];

            Messages.ArenaJumpingStartup = (string)gameData["messages"]["meta"]["arena"]["jumping_start_up"];
            Messages.ArenaDraftStartup = (string)gameData["messages"]["meta"]["arena"]["draft_start_up"];
            Messages.ArenaRacingStartup = (string)gameData["messages"]["meta"]["arena"]["racing_start_up"];
            Messages.ArenaConformationStartup = (string)gameData["messages"]["meta"]["arena"]["conformation_start_up"];

            Messages.ArenaYouWinFormat = (string)gameData["messages"]["meta"]["arena"]["winner"];
            Messages.ArenaOnlyWinnerWins = (string)gameData["messages"]["meta"]["arena"]["only_winner_wins"];

            Messages.ArenaTooHungry = (string)gameData["messages"]["meta"]["arena"]["too_hungry"];
            Messages.ArenaTooThirsty = (string)gameData["messages"]["meta"]["arena"]["too_thisty"];
            Messages.ArenaNeedsFarrier = (string)gameData["messages"]["meta"]["arena"]["farrier"];
            Messages.ArenaTooTired = (string)gameData["messages"]["meta"]["arena"]["too_tired"];
            Messages.ArenaNeedsVet = (string)gameData["messages"]["meta"]["arena"]["needs_vet"];

            Messages.ArenaEventNameFormat = (string)gameData["messages"]["meta"]["arena"]["event_name"];
            Messages.ArenaCurrentlyTakingEntriesFormat = (string)gameData["messages"]["meta"]["arena"]["currently_taking_entries"];
            Messages.ArenaCompetitionInProgress = (string)gameData["messages"]["meta"]["arena"]["competition_in_progress"];
            Messages.ArenaYouHaveHorseEntered = (string)gameData["messages"]["meta"]["arena"]["horse_entered"];
            Messages.ArenaCompetitionFull = (string)gameData["messages"]["meta"]["arena"]["competiton_full"];

            Messages.ArenaFullErrorMessage = (string)gameData["messages"]["meta"]["arena"]["arena_join_fail_full"];

            Messages.ArenaEnterHorseFormat = (string)gameData["messages"]["meta"]["arena"]["enter_horse"];
            Messages.ArenaCurrentCompetitors = (string)gameData["messages"]["meta"]["arena"]["current_competitors"];
            Messages.ArenaCompetingHorseFormat = (string)gameData["messages"]["meta"]["arena"]["competing_horses"];

            // Horse Games
            Messages.HorseGamesSelectHorse = (string)gameData["messages"]["meta"]["horse_games"]["select_a_horse"];
            Messages.HorseGamesHorseEntryFormat = (string)gameData["messages"]["meta"]["horse_games"]["horse_entry"];

            // City Hall
            Messages.CityHallMenu = (string)gameData["messages"]["meta"]["city_hall"]["menu"];
            Messages.CityHallMailSendMeta = (string)gameData["messages"]["meta"]["city_hall"]["mail_send_meta"];

            Messages.CityHallSentMessageFormat = (string)gameData["messages"]["meta"]["city_hall"]["sent_mail"];
            Messages.CityHallCantAffordPostageMessage = (string)gameData["messages"]["meta"]["city_hall"]["cant_afford_postage"];
            Messages.CityHallCantFindPlayerMessageFormat = (string)gameData["messages"]["meta"]["city_hall"]["cant_find_player"];

            Messages.CityHallCheapestAutoSells = (string)gameData["messages"]["meta"]["city_hall"]["auto_sell"]["top_100_cheapest"];
            Messages.CityHallCheapestAutoSellHorseEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["auto_sell"]["cheap_horse_entry"];

            Messages.CityHallMostExpAutoSells = (string)gameData["messages"]["meta"]["city_hall"]["auto_sell"]["top_50_most_exp"];
            Messages.CityHallMostExpAutoSellHorseEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["auto_sell"]["exp_horse_entry"];

            Messages.CityHallTop25Ranches = (string)gameData["messages"]["meta"]["city_hall"]["ranch_investment"]["top_25"];
            Messages.CityHallRanchEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["ranch_investment"]["ranch_entry"];

            Messages.CityHallTop25Players = (string)gameData["messages"]["meta"]["city_hall"]["richest_players"]["top_25"];
            Messages.CityHallRichPlayerFormat = (string)gameData["messages"]["meta"]["city_hall"]["richest_players"]["rich_player_format"];

            Messages.CityHallTop100SpoiledHorses = (string)gameData["messages"]["meta"]["city_hall"]["spoiled_horses"]["top_100"];
            Messages.CityHallSpoiledHorseEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["spoiled_horses"]["spoiled_horse_entry"];

            Messages.CityHallTop25AdventurousPlayers = (string)gameData["messages"]["meta"]["city_hall"]["most_adventurous_players"]["top_25"];
            Messages.CityHallAdventurousPlayerEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["most_adventurous_players"]["adventurous_player_entry"];

            Messages.CityHallTop25ExperiencedPlayers = (string)gameData["messages"]["meta"]["city_hall"]["most_experinced_players"]["top_25"];
            Messages.CityHallExperiencePlayerEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["most_experinced_players"]["experienced_player_entry"];

            Messages.CityHallTop25MinigamePlayers = (string)gameData["messages"]["meta"]["city_hall"]["most_active_minigame_players"]["top_25"];
            Messages.CityHallMinigamePlayerEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["most_active_minigame_players"]["minigame_player_entry"];

            Messages.CityHallTop25ExperiencedHorses = (string)gameData["messages"]["meta"]["city_hall"]["most_experienced_horses"]["top_25"];
            Messages.CityHallExperiencedHorseEntryFormat = (string)gameData["messages"]["meta"]["city_hall"]["most_experienced_horses"]["experienced_horse_entry"];

            // Mail Messages
            Messages.MailReceivedMessage = (string)gameData["messages"]["meta"]["mail"]["mail_received"];
            Messages.MailSelectFromFollowing = (string)gameData["messages"]["meta"]["mail"]["mail_select"];
            Messages.MailSe = (string)gameData["messages"]["meta"]["mail"]["mail_se"];

            Messages.MailReadMetaFormat = (string)gameData["messages"]["meta"]["mail"]["mail_read"];
            Messages.MailEntryFormat = (string)gameData["messages"]["meta"]["mail"]["mail_entry"];
            Messages.MailRippedMessage = (string)gameData["messages"]["meta"]["mail"]["mail_ripped"];

            // Click
            Messages.ClickPlayerHereFormat = (string)gameData["messages"]["player_here"];


            // Ranch
            Messages.RanchUnownedRanchFormat = (string)gameData["messages"]["meta"]["ranch"]["unowned_ranch"];
            Messages.RanchYouCouldPurchaseThisRanch = (string)gameData["messages"]["meta"]["ranch"]["you_could_purchase_this"];
            Messages.RanchYouAllreadyOwnARanch = (string)gameData["messages"]["meta"]["ranch"]["ranch_already_owned"];
            Messages.RanchSubscribersOnly = (string)gameData["messages"]["meta"]["ranch"]["sub_only"];
            Messages.RanchDescriptionOthersFormat = (string)gameData["messages"]["meta"]["ranch"]["ranch_desc_others"];
            Messages.RanchUnownedRanchClicked = (string)gameData["messages"]["meta"]["ranch"]["unowned_ranch_click"];
            Messages.RanchClickMessageFormat = (string)gameData["messages"]["meta"]["ranch"]["click_message"];

            Messages.RanchNoDorothyShoesMessage = (string)gameData["messages"]["meta"]["ranch"]["no_dorothy_shoes"];
            Messages.RanchDorothyShoesMessage = (string)gameData["messages"]["meta"]["ranch"]["dorothy_message"];
            Messages.RanchDorothyShoesPrisonIsleMessage = (string)gameData["messages"]["meta"]["ranch"]["dorothy_prison_isle"];
            Messages.RanchForcefullySoldFormat = (string)gameData["messages"]["meta"]["ranch"]["forcefully_sold"];

            Messages.RanchCantAffordRanch = (string)gameData["messages"]["meta"]["ranch"]["ranch_buy_cannot_afford"];
            Messages.RanchRanchBroughtMessageFormat = (string)gameData["messages"]["meta"]["ranch"]["ranch_brought"];

            Messages.RanchSavedRanchDescripton = (string)gameData["messages"]["meta"]["ranch"]["ranch_info"]["saved"];
            Messages.RanchSavedTitleTooLongError = (string)gameData["messages"]["meta"]["ranch"]["ranch_info"]["title_too_long"];
            Messages.RanchSavedDescrptionTooLongError = (string)gameData["messages"]["meta"]["ranch"]["ranch_info"]["description_too_long"];
            Messages.RanchSavedTitleViolationsError = (string)gameData["messages"]["meta"]["ranch"]["ranch_info"]["title_contains_violations"];
            Messages.RanchSavedDescrptionViolationsErrorFormat = (string)gameData["messages"]["meta"]["ranch"]["ranch_info"]["desc_contains_violations"];


            Messages.RanchDefaultRanchTitle = (string)gameData["messages"]["meta"]["ranch"]["default_title"];
            Messages.RanchEditDescriptionMetaFormat = (string)gameData["messages"]["meta"]["ranch"]["edit_description"];
            Messages.RanchTitleFormat = (string)gameData["messages"]["meta"]["ranch"]["your_ranch_meta"];
            Messages.RanchYourDescriptionFormat = (string)gameData["messages"]["meta"]["ranch"]["view_desc"];

            Messages.RanchSellAreYouSure = (string)gameData["messages"]["meta"]["ranch"]["sell_confirm"];
            Messages.RanchSoldFormat = (string)gameData["messages"]["meta"]["ranch"]["sell_done"];

            // Ranch : Breed

            Messages.RanchCanBuildOneOfTheFollowingInThisSpot = (string)gameData["messages"]["meta"]["ranch"]["build"]["build_on_this_spot"];
            Messages.RanchBuildingEntryFormat = (string)gameData["messages"]["meta"]["ranch"]["build"]["build_format"];
            Messages.RanchCantAffordThisBuilding = (string)gameData["messages"]["meta"]["ranch"]["build"]["cannot_afford"];
            Messages.RanchBuildingInformationFormat = (string)gameData["messages"]["meta"]["ranch"]["build"]["information"];
            Messages.RanchBuildingComplete = (string)gameData["messages"]["meta"]["ranch"]["build"]["build_complete"];
            Messages.RanchBuildingAlreadyHere = (string)gameData["messages"]["meta"]["ranch"]["build"]["building_allready_placed"];
            Messages.RanchTornDownRanchBuildingFormat = (string)gameData["messages"]["meta"]["ranch"]["build"]["torn_down"];
            Messages.RanchViewBuildingFormat = (string)gameData["messages"]["meta"]["ranch"]["build"]["view_building"];
            Messages.RanchBarnHorsesFormat = (string)gameData["messages"]["meta"]["ranch"]["build"]["barn"];

            // Ranch : Upgrades

            Messages.UpgradedMessage = (string)gameData["messages"]["meta"]["ranch"]["upgrade"]["upgrade_message"];
            Messages.UpgradeCannotAfford = (string)gameData["messages"]["meta"]["ranch"]["upgrade"]["cannot_afford"];
            Messages.UpgradeCurrentUpgradeFormat = (string)gameData["messages"]["meta"]["ranch"]["upgrade"]["upgrade_meta"];
            Messages.UpgradeNextUpgradeFormat = (string)gameData["messages"]["meta"]["ranch"]["upgrade"]["you_could_upgrade"];

            // Ranch : Special

            Messages.BuildingRestHere = (string)gameData["messages"]["meta"]["ranch"]["special"]["rest_here"];
            Messages.BuildingGrainSilo = (string)gameData["messages"]["meta"]["ranch"]["special"]["grain_silo"];
            Messages.BuildingBarnFormat = (string)gameData["messages"]["meta"]["ranch"]["special"]["barn"];
            Messages.BuildingBigBarnFormat = (string)gameData["messages"]["meta"]["ranch"]["special"]["big_barn"];
            Messages.BuildingGoldBarnFormat = (string)gameData["messages"]["meta"]["ranch"]["special"]["gold_barn"];
            Messages.BuildingWaterWell = (string)gameData["messages"]["meta"]["ranch"]["special"]["water_well"];
            Messages.BuildingWindmillFormat = (string)gameData["messages"]["meta"]["ranch"]["special"]["windmills"];
            Messages.BuildingWagon = (string)gameData["messages"]["meta"]["ranch"]["special"]["wagon"];
            Messages.BuildingTrainingPen = (string)gameData["messages"]["meta"]["ranch"]["special"]["training_pen"];
            Messages.BuildingVegatableGarden = (string)gameData["messages"]["meta"]["ranch"]["special"]["vegatable_garden"];

            Messages.RanchTrainAllAttempt = (string)gameData["messages"]["meta"]["ranch"]["special"]["train_all"];
            Messages.RanchTrainSuccessFormat = (string)gameData["messages"]["meta"]["ranch"]["special"]["train_success"];
            Messages.RanchTrainCantTrainFormat = (string)gameData["messages"]["meta"]["ranch"]["special"]["train_cant_train"];
            Messages.RanchTrainBadMoodFormat = (string)gameData["messages"]["meta"]["ranch"]["special"]["train_bad_mood"];
            Messages.RanchHorsesFullyRested = (string)gameData["messages"]["meta"]["ranch"]["special"]["fully_rested"];
            Messages.RanchWagonDroppedYouOff = (string)gameData["messages"]["meta"]["ranch"]["special"]["wagon_used"];

            // Treasure
            Messages.PirateTreasureFormat = (string)gameData["messages"]["treasure"]["pirate_treasure"];
            Messages.PotOfGoldFormat = (string)gameData["messages"]["treasure"]["pot_of_gold"];

            // Records
            Messages.PrivateNotesSavedMessage = (string)gameData["messages"]["private_notes_save"];
            Messages.PrivateNotesMetaFormat = (string)gameData["messages"]["meta"]["private_notes_format"];

            // Profile
            Messages.ProfileSavedMessage = (string)gameData["messages"]["profile"]["save"];
            Messages.ProfileTooLongMessage = (string)gameData["messages"]["profile"]["too_long"];
            Messages.ProfileViolationFormat = (string)gameData["messages"]["profile"]["blocked"];

            // Announcements

            Messages.WelcomeFormat = (string)gameData["messages"]["welcome_format"];
            Messages.MotdFormat = (string)gameData["messages"]["motd_format"];
            Messages.LoginMessageFormat = (string)gameData["messages"]["login_format"];
            Messages.LogoutMessageFormat = (string)gameData["messages"]["logout_format"];

            // Pronoun
            Messages.PronounFemaleShe = (string)gameData["messages"]["meta"]["stats_page"]["pronouns"]["female_she"];
            Messages.PronounFemaleHer = (string)gameData["messages"]["meta"]["stats_page"]["pronouns"]["female_her"];

            Messages.PronounMaleHe = (string)gameData["messages"]["meta"]["stats_page"]["pronouns"]["male_he"];
            Messages.PronounMaleHis = (string)gameData["messages"]["meta"]["stats_page"]["pronouns"]["male_his"];

            Messages.PronounNeutralYour = (string)gameData["messages"]["meta"]["stats_page"]["pronouns"]["neutral_your"];

            Messages.PronounNeutralThey = (string)gameData["messages"]["meta"]["stats_page"]["pronouns"]["neutral_they"];
            Messages.PronounNeutralTheir = (string)gameData["messages"]["meta"]["stats_page"]["pronouns"]["neutral_their"];

            // Stats
            Messages.StatsBarFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_bar_format"];
            Messages.StatsAreaFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_area_format"];
            Messages.StatsMoneyFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_money_format"];
            Messages.StatsFreeTimeFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_freetime_format"];
            Messages.StatsDescriptionFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_description_format"];
            Messages.StatsExpFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_experience"];
            Messages.StatsQuestpointsFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_questpoints"];
            Messages.StatsHungerFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_hunger"];
            Messages.StatsThirstFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_thisrt"];
            Messages.StatsTiredFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_tiredness"];
            Messages.StatsGenderFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_gender"];
            Messages.StatsJewelFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_equipped"];
            Messages.StatsCompetitionGearFormat = (string)gameData["messages"]["meta"]["stats_page"]["stats_competion_gear"];

            Messages.JewelrySlot1Format = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["slot_1"];
            Messages.JewelrySlot2Format = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["slot_2"];
            Messages.JewelrySlot3Format = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["slot_3"];
            Messages.JewelrySlot4Format = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["slot_4"];

            Messages.JewelryRemoveSlot1Button = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_1"];
            Messages.JewelryRemoveSlot2Button = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_2"];
            Messages.JewelryRemoveSlot3Button = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_3"];
            Messages.JewelryRemoveSlot4Button = (string)gameData["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_4"];

            Messages.CompetitionGearHeadFormat = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["head_format"];
            Messages.CompetitionGearBodyFormat = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["body_format"];
            Messages.CompetitionGearLegsFormat = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["legs_format"];
            Messages.CompetitionGearFeetFormat = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["feet_format"];

            Messages.CompetitionGearRemoveHeadButton = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["remove_head"];
            Messages.CompetitionGearRemoveBodyButton = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["remove_body"];
            Messages.CompetitionGearRemoveLegsButton = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["remove_legs"];
            Messages.CompetitionGearRemoveFeetButton = (string)gameData["messages"]["meta"]["stats_page"]["competition_gear"]["remove_feet"];

            Messages.StatsPrivateNotesButton = (string)gameData["messages"]["meta"]["stats_page"]["stats_private_notes"];
            Messages.StatsQuestsButton = (string)gameData["messages"]["meta"]["stats_page"]["stats_quests"];
            Messages.StatsMinigameRankingButton = (string)gameData["messages"]["meta"]["stats_page"]["stats_minigame_ranking"];
            Messages.StatsAwardsButton = (string)gameData["messages"]["meta"]["stats_page"]["stats_awards"];
            Messages.StatsMiscButton = (string)gameData["messages"]["meta"]["stats_page"]["stats_misc"];

            Messages.JewelrySelected = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["jewelry_selected"];
            Messages.JewelrySelectedOther = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["jewelry_other"];

            Messages.NoJewerlyEquipped = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["no_jewelry_equipped"];
            Messages.NoJewerlyEquippedOther = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["no_jewelry_other"];

            Messages.NoCompetitionGear = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["no_competition_gear"];
            Messages.NoCompetitionGearOther = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["no_competition_gear_other"];

            Messages.CompetitionGearSelected = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["competition_gear_selected"];
            Messages.CompetitionGearSelectedOther = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["competition_gear_other_selected"];

            Messages.StatHunger = (string)gameData["messages"]["meta"]["stats_page"]["hunger_stat_name"];
            Messages.StatThirst = (string)gameData["messages"]["meta"]["stats_page"]["thirst_stat_name"];
            Messages.StatTired = (string)gameData["messages"]["meta"]["stats_page"]["tired_stat_name"];

            Messages.StatsOtherHorses = (string)gameData["messages"]["meta"]["stats_page"]["msg"]["other_horses"];
            Messages.StatPlayerFormats = gameData["messages"]["meta"]["stats_page"]["player_stats"].Deserialize<string[]>();

            Messages.StatThirstDizzy = (string)gameData["messages"]["movement_key"]["thirsty"];
            Messages.StatHungerStumble = (string)gameData["messages"]["movement_key"]["hungery"];

            // Misc Stats
            Messages.StatMiscHeader = (string)gameData["messages"]["meta"]["misc_stats"]["header"];
            Messages.StatMiscNoneRecorded = (string)gameData["messages"]["meta"]["misc_stats"]["no_stats_recorded"];
            Messages.StatMiscEntryFormat = (string)gameData["messages"]["meta"]["misc_stats"]["stat_format"];

            // Books (Libary)
            Messages.BooksOfHorseIsle = (string)gameData["messages"]["meta"]["libary"]["books"]["books_of_horseisle"];
            Messages.BookEntryFormat = (string)gameData["messages"]["meta"]["libary"]["books"]["book_entry"];
            Messages.BookReadFormat = (string)gameData["messages"]["meta"]["libary"]["books"]["book_read"];

            // Awards (Libary)
            Messages.AwardsAvalible = (string)gameData["messages"]["meta"]["libary"]["awards"]["all_earnable_awards"];
            Messages.AwardEntryFormat = (string)gameData["messages"]["meta"]["libary"]["awards"]["award_entry"];

            // Locations (Libary)
            Messages.LocationKnownIslands = (string)gameData["messages"]["meta"]["libary"]["locations"]["known_islands"];
            Messages.LocationKnownTowns = (string)gameData["messages"]["meta"]["libary"]["locations"]["known_towns"];
            Messages.LocationIslandFormat = (string)gameData["messages"]["meta"]["libary"]["locations"]["isle_entry"];
            Messages.LocationTownFormat = (string)gameData["messages"]["meta"]["libary"]["locations"]["town_entry"];
            Messages.LocationDescriptionFormat = (string)gameData["messages"]["meta"]["libary"]["locations"]["location_description"];

            // Minigame (Libary)
            Messages.MinigameSingleplayer = (string)gameData["messages"]["meta"]["libary"]["minigames"]["singleplayer"];
            Messages.MinigameTwoplayer = (string)gameData["messages"]["meta"]["libary"]["minigames"]["twoplayer"];
            Messages.MinigameMultiplayer = (string)gameData["messages"]["meta"]["libary"]["minigames"]["multiplayer"];
            Messages.MinigameCompetitions = (string)gameData["messages"]["meta"]["libary"]["minigames"]["competitions"];
            Messages.MinigameEntryFormat = (string)gameData["messages"]["meta"]["libary"]["minigames"]["minigame_entry"];

            // Companion (Libary)
            Messages.CompanionViewFormat = (string)gameData["messages"]["meta"]["libary"]["companion"]["view_button"];
            Messages.CompanionEntryFormat = (string)gameData["messages"]["meta"]["libary"]["companion"]["entry_format"];

            // Tack (Libary)
            Messages.TackViewSetFormat = (string)gameData["messages"]["meta"]["libary"]["tack"]["view_tack_set"];
            Messages.TackSetPeiceFormat = (string)gameData["messages"]["meta"]["libary"]["tack"]["set_peice_format"];

            // Groomer
            Messages.GroomerBestToHisAbilitiesFormat = (string)gameData["messages"]["meta"]["groomer"]["groomed_best_it_can"];
            Messages.GroomerCannotAffordMessage = (string)gameData["messages"]["meta"]["groomer"]["cannot_afford_service"];
            Messages.GroomerCannotImprove = (string)gameData["messages"]["meta"]["groomer"]["cannot_improve"];
            Messages.GroomerBestToHisAbilitiesALL = (string)gameData["messages"]["meta"]["groomer"]["groomed_best_all"];
            Messages.GroomerDontNeed = (string)gameData["messages"]["meta"]["groomer"]["dont_need"];

            Messages.GroomerHorseCurrentlyAtFormat = (string)gameData["messages"]["meta"]["groomer"]["currently_at"];
            Messages.GroomerApplyServiceFormat = (string)gameData["messages"]["meta"]["groomer"]["apply_service"];
            Messages.GroomerApplyServiceForAllFormat = (string)gameData["messages"]["meta"]["groomer"]["apply_all"];

            // Barn
            Messages.BarnHorseFullyFedFormat = (string)gameData["messages"]["meta"]["barn"]["fully_fed"];
            Messages.BarnCantAffordService = (string)gameData["messages"]["meta"]["barn"]["cant_afford"];
            Messages.BarnAllHorsesFullyFed = (string)gameData["messages"]["meta"]["barn"]["rested_all"];
            Messages.BarnServiceNotNeeded = (string)gameData["messages"]["meta"]["barn"]["not_needed"];

            Messages.BarnHorseStatusFormat = (string)gameData["messages"]["meta"]["barn"]["horse_status"];
            Messages.BarnHorseMaxed = (string)gameData["messages"]["meta"]["barn"]["horse_maxed"];
            Messages.BarnLetHorseRelaxFormat = (string)gameData["messages"]["meta"]["barn"]["let_relax"];
            Messages.BarnLetAllHorsesReleaxFormat = (string)gameData["messages"]["meta"]["barn"]["relax_all"];

            // Farrier
            Messages.FarrierCurrentShoesFormat = (string)gameData["messages"]["meta"]["farrier"]["current_shoes"];
            Messages.FarrierApplyIronShoesFormat = (string)gameData["messages"]["meta"]["farrier"]["apply_iron"];
            Messages.FarrierApplySteelShoesFormat = (string)gameData["messages"]["meta"]["farrier"]["apply_steel"];
            Messages.FarrierShoeAllFormat = (string)gameData["messages"]["meta"]["farrier"]["shoe_all"];

            Messages.FarrierPutOnSteelShoesMessageFormat = (string)gameData["messages"]["meta"]["farrier"]["put_on_steel_shoes"];
            Messages.FarrierPutOnIronShoesMessageFormat = (string)gameData["messages"]["meta"]["farrier"]["put_on_iron_shoes"];
            Messages.FarrierPutOnSteelShoesAllMesssageFormat = (string)gameData["messages"]["meta"]["farrier"]["put_on_steel_all"];
            Messages.FarrierShoesCantAffordMessage = (string)gameData["messages"]["meta"]["farrier"]["cant_afford_farrier"];

            // Trainng Pen
            Messages.TrainedInStatFormat = (string)gameData["messages"]["meta"]["trainer_pen"]["train_success"];
            Messages.TrainerHeaderFormat = (string)gameData["messages"]["meta"]["trainer_pen"]["train_header"];
            Messages.TrainerHorseEntryFormat = (string)gameData["messages"]["meta"]["trainer_pen"]["train_format"];
            Messages.TrainerHorseFullyTrainedFormat = (string)gameData["messages"]["meta"]["trainer_pen"]["fully_trained"];
            Messages.TrainerCantTrainAgainInFormat = (string)gameData["messages"]["meta"]["trainer_pen"]["train_again_in"];
            Messages.TrainerCantAfford = (string)gameData["messages"]["meta"]["trainer_pen"]["cant_afford"];

            // Santa
            Messages.SantaHiddenText = (string)gameData["messages"]["meta"]["santa"]["hidden_text"];
            Messages.SantaWrapItemFormat = (string)gameData["messages"]["meta"]["santa"]["wrap_format"];
            Messages.SantaWrappedObjectMessage = (string)gameData["messages"]["meta"]["santa"]["wrapped_object"];
            Messages.SantaCantWrapInvFull = (string)gameData["messages"]["meta"]["santa"]["wrap_fail_inv_full"];
            Messages.SantaCantOpenNothingInside = (string)gameData["messages"]["meta"]["santa"]["open_fail_empty"];
            Messages.SantaItemOpenedFormat = (string)gameData["messages"]["meta"]["santa"]["open_format"];
            Messages.SantaItemCantOpenInvFull = (string)gameData["messages"]["meta"]["santa"]["open_fail_inv_full"];

            // Pawneer
            Messages.PawneerUntackedHorsesICanBuy = (string)gameData["messages"]["meta"]["pawneer"]["untacked_i_can_buy"];
            Messages.PawneerHorseFormat = (string)gameData["messages"]["meta"]["pawneer"]["pawn_horse"];
            Messages.PawneerOrderMeta = (string)gameData["messages"]["meta"]["pawneer"]["pawneer_order"];
            Messages.PawneerHorseConfirmationFormat = (string)gameData["messages"]["meta"]["pawneer"]["are_you_sure"];
            Messages.PawneerHorseSoldMessagesFormat = (string)gameData["messages"]["meta"]["pawneer"]["horse_sold"];
            Messages.PawneerHorseNotFound = (string)gameData["messages"]["meta"]["pawneer"]["horse_not_found"];

            Messages.PawneerOrderSelectBreed = (string)gameData["messages"]["meta"]["pawneer"]["order"]["select_breed"];
            Messages.PawneerOrderBreedEntryFormat = (string)gameData["messages"]["meta"]["pawneer"]["order"]["breed_entry"];

            Messages.PawneerOrderSelectColorFormat = (string)gameData["messages"]["meta"]["pawneer"]["order"]["select_color"];
            Messages.PawneerOrderColorEntryFormat = (string)gameData["messages"]["meta"]["pawneer"]["order"]["color_entry"];

            Messages.PawneerOrderSelectGenderFormat = (string)gameData["messages"]["meta"]["pawneer"]["order"]["select_gender"];
            Messages.PawneerOrderGenderEntryFormat = (string)gameData["messages"]["meta"]["pawneer"]["order"]["gender_entry"];

            Messages.PawneerOrderHorseFoundFormat = (string)gameData["messages"]["meta"]["pawneer"]["order"]["found"];

            // Vet
            Messages.VetServiceHorseFormat = (string)gameData["messages"]["meta"]["vet"]["service_horse"];
            Messages.VetSerivcesNotNeeded = (string)gameData["messages"]["meta"]["vet"]["not_needed"];
            Messages.VetApplyServicesFormat = (string)gameData["messages"]["meta"]["vet"]["apply"];

            Messages.VetApplyServicesForAllFormat = (string)gameData["messages"]["meta"]["vet"]["apply_all"];
            Messages.VetFullHealthRecoveredMessageFormat = (string)gameData["messages"]["meta"]["vet"]["now_full_health"];
            Messages.VetServicesNotNeededAll = (string)gameData["messages"]["meta"]["vet"]["not_needed_all"];
            Messages.VetAllFullHealthRecoveredMessage = (string)gameData["messages"]["meta"]["vet"]["all_full"];
            Messages.VetCannotAffordMessage = (string)gameData["messages"]["meta"]["vet"]["cant_afford"];

            // Pond
            Messages.PondHeader = (string)gameData["messages"]["meta"]["pond"]["header"];
            Messages.PondGoFishing = (string)gameData["messages"]["meta"]["pond"]["go_fishing"];
            Messages.PondNoFishingPole = (string)gameData["messages"]["meta"]["pond"]["no_fishing_pole"];
            Messages.PondDrinkHereIfSafe = (string)gameData["messages"]["meta"]["pond"]["drink_here"];
            Messages.PondHorseDrinkFormat = (string)gameData["messages"]["meta"]["pond"]["horse_drink_format"];
            Messages.PondNoEarthWorms = (string)gameData["messages"]["meta"]["pond"]["no_earth_worms"];

            Messages.PondDrinkFullFormat = (string)gameData["messages"]["meta"]["pond"]["drank_full"];
            Messages.PondCantDrinkHpLowFormat = (string)gameData["messages"]["meta"]["pond"]["cant_drink_hp_low"];
            Messages.PondDrinkOhNoesFormat = (string)gameData["messages"]["meta"]["pond"]["drank_something_bad"];
            Messages.PondNotThirstyFormat = (string)gameData["messages"]["meta"]["pond"]["not_thirsty"];

            // Horse Whisperer
            Messages.WhispererHorseLocateButtonFormat = (string)gameData["messages"]["meta"]["whisperer"]["horse_locate_meta"];
            Messages.WhispererServiceCostYouFormat = (string)gameData["messages"]["meta"]["whisperer"]["service_cost"];
            Messages.WhispererServiceCannotAfford = (string)gameData["messages"]["meta"]["whisperer"]["cant_afford"];
            Messages.WhispererSearchingAmoungHorses = (string)gameData["messages"]["meta"]["whisperer"]["searching_amoung_horses"];
            Messages.WhispererNoneFound = (string)gameData["messages"]["meta"]["whisperer"]["none_found_meta"];
            Messages.WhispererHorsesFoundFormat = (string)gameData["messages"]["meta"]["whisperer"]["horse_found_meta"];

            // Mud Hole
            Messages.MudHoleNoHorses = (string)gameData["messages"]["meta"]["mud_hole"]["no_horses"];
            Messages.MudHoleRuinedGroomFormat = (string)gameData["messages"]["meta"]["mud_hole"]["ruined_groom"];

            // Movement
            Messages.RandomMovement = (string)gameData["messages"]["random_movement"];

            // Quests Log
            Messages.QuestLogHeader = (string)gameData["messages"]["meta"]["quest_log"]["header_meta"];
            Messages.QuestFormat = (string)gameData["messages"]["meta"]["quest_log"]["quest_format"];

            Messages.QuestNotCompleted = (string)gameData["messages"]["meta"]["quest_log"]["not_complete"];
            Messages.QuestNotAvalible = (string)gameData["messages"]["meta"]["quest_log"]["not_avalible"];
            Messages.QuestCompleted = (string)gameData["messages"]["meta"]["quest_log"]["completed"];

            Messages.QuestFooterFormat = (string)gameData["messages"]["meta"]["quest_log"]["footer_format"];
            // Transport

            Messages.CantAffordTransport = (string)gameData["messages"]["transport"]["not_enough_money"];
            Messages.WelcomeToAreaFormat = (string)gameData["messages"]["transport"]["welcome_to_format"];
            Messages.TransportFormat = (string)gameData["messages"]["meta"]["transport_format"];
            Messages.TransportCostFormat = (string)gameData["messages"]["meta"]["transport_cost"];
            Messages.TransportWagonFree = (string)gameData["messages"]["meta"]["transport_free"];

            // Abuse Reports
            Messages.AbuseReportMetaFormat = (string)gameData["messages"]["meta"]["abuse_report"]["options_format"];
            Messages.AbuseReportReasonFormat = (string)gameData["messages"]["meta"]["abuse_report"]["report_reason_format"];

            Messages.AbuseReportPlayerNotFoundFormat = (string)gameData["messages"]["abuse_report"]["player_not_found_format"];
            Messages.AbuseReportFiled = (string)gameData["messages"]["abuse_report"]["report_filed"];
            Messages.AbuseReportProvideValidReason = (string)gameData["messages"]["abuse_report"]["valid_reason"];

            // Bank
            Messages.BankMadeInIntrestFormat = (string)gameData["messages"]["meta"]["bank"]["made_interest"];
            Messages.BankCarryingFormat = (string)gameData["messages"]["meta"]["bank"]["carrying_message"];
            Messages.BankWhatToDo = (string)gameData["messages"]["meta"]["bank"]["what_to_do"];
            Messages.BankOptionsFormat = (string)gameData["messages"]["meta"]["bank"]["options"];


            Messages.BankDepositedMoneyFormat = (string)gameData["messages"]["bank"]["deposit_format"];
            Messages.BankWithdrewMoneyFormat = (string)gameData["messages"]["bank"]["withdraw_format"];

            Messages.BankCantHoldThisMuch = (string)gameData["messages"]["bank"]["cant_hold_that_much"];
            Messages.BankYouCantHoldThisMuch = (string)gameData["messages"]["bank"]["cant_withdraw_that_much"];

            // Riddler
            Messages.RiddlerAnsweredAll = (string)gameData["messages"]["meta"]["riddler.riddle_all_complete"];
            Messages.RiddlerIncorrectAnswer = (string)gameData["messages"]["meta"]["riddler.riddle_incorrect"];
            Messages.RiddlerCorrectAnswerFormat = (string)gameData["messages"]["meta"]["riddler.riddle_correct"];
            Messages.RiddlerEnterAnswerFormat = (string)gameData["messages"]["meta"]["riddler.riddle_format"];

            // Workshop
            Messages.WorkshopCraftEntryFormat = (string)gameData["messages"]["meta"]["workshop.craft_entry"];
            Messages.WorkshopRequiresFormat = (string)gameData["messages"]["meta"]["workshop.requires"];
            Messages.WorkshopRequireEntryFormat = (string)gameData["messages"]["meta"]["workshop.require"];
            Messages.WorkshopAnd = (string)gameData["messages"]["meta"]["workshop.and"];

            Messages.WorkshopNoRoomInInventory = (string)gameData["messages"]["meta"]["workshop.no_room"];
            Messages.WorkshopMissingRequiredItem = (string)gameData["messages"]["meta"]["workshop.missing_item"];
            Messages.WorkshopCraftingSuccess = (string)gameData["messages"]["meta"]["workshop.craft_success"];
            Messages.WorkshopCannotAfford = (string)gameData["messages"]["meta"]["workshop.no_money"];

            // Horses
            Messages.AdvancedStatFormat = (string)gameData["messages"]["meta"]["horse"]["stat_format"];
            Messages.BasicStatFormat = (string)gameData["messages"]["meta"]["horse"]["basic_stat_format"];
            Messages.HorsesHere = (string)gameData["messages"]["meta"]["horse"]["horses_here"];
            Messages.WildHorseFormat = (string)gameData["messages"]["meta"]["horse"]["wild_horse"];
            Messages.HorseCaptureTimer = (string)gameData["messages"]["meta"]["horse"]["horse_timer"];

            Messages.YouCapturedTheHorse = (string)gameData["messages"]["meta"]["horse"]["horse_caught"];
            Messages.HorseEvadedCapture = (string)gameData["messages"]["meta"]["horse"]["horse_escaped"];
            Messages.HorseEscapedAnyway = (string)gameData["messages"]["meta"]["horse"]["horse_escaped_anyway"];

            Messages.HorsesMenuHeader = (string)gameData["messages"]["meta"]["horse"]["horses_menu"];
            Messages.TooManyHorses = (string)gameData["messages"]["meta"]["horse"]["too_many_horses"];
            Messages.UpdateHorseCategory = (string)gameData["messages"]["meta"]["horse"]["update_category"];
            Messages.HorseEntryFormat = (string)gameData["messages"]["meta"]["horse"]["horse_format"];
            Messages.ViewBaiscStats = (string)gameData["messages"]["meta"]["horse"]["view_basic_stats"];
            Messages.ViewAdvancedStats = (string)gameData["messages"]["meta"]["horse"]["view_advanced_stats"];
            Messages.HorseBuckedYou = (string)gameData["messages"]["meta"]["horse"]["horse_bucked"];
            Messages.HorseLlamaBuckedYou = (string)gameData["messages"]["meta"]["horse"]["llama_bucked"];
            Messages.HorseCamelBuckedYou = (string)gameData["messages"]["meta"]["horse"]["camel_bucked"];

            Messages.HorseRidingMessageFormat = (string)gameData["messages"]["meta"]["horse"]["riding_message"];
            Messages.HorseNameYoursFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["your_horse_format"];
            Messages.HorseNameOthersFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["horse_others_format"];
            Messages.HorseDescriptionFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["description_format"];
            Messages.HorseHandsHeightFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["hands_high"];
            Messages.HorseExperienceEarnedFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["experience"];

            Messages.HorseTrainableInFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["trainable_in"];
            Messages.HorseIsTrainable = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["currently_trainable"];
            Messages.HorseLeasedRemainingTimeFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["leased_horse"];

            Messages.HorseCannotMountUntilTackedMessage = (string)gameData["messages"]["meta"]["horse"]["cannot_mount_tacked"];
            Messages.HorseDismountedBecauseNotTackedMessageFormat = (string)gameData["messages"]["meta"]["horse"]["dismount_because_tack"];
            Messages.HorseMountButtonFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["mount_button"];
            Messages.HorseDisMountButtonFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["dismount_button"];
            Messages.HorseFeedButtonFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["feed_button"];
            Messages.HorseTackButtonFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["tack_button"];
            Messages.HorsePetButtonFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["pet_button"];
            Messages.HorseProfileButtonFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["profile_button"];

            Messages.HorseSavedProfileMessageFormat = (string)gameData["messages"]["meta"]["horse"]["profile"]["saved"];
            Messages.HorseProfileMessageTooLongError = (string)gameData["messages"]["meta"]["horse"]["profile"]["desc_too_long"];
            Messages.HorseNameTooLongError = (string)gameData["messages"]["meta"]["horse"]["profile"]["name_too_long"];
            Messages.HorseNameViolationsError = (string)gameData["messages"]["meta"]["horse"]["profile"]["name_profanity_detected"];
            Messages.HorseProfileMessageProfileError = (string)gameData["messages"]["meta"]["horse"]["profile"]["profile_profanity_detected"];

            Messages.HorseCatchTooManyHorsesMessage = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["horse_cannot_catch_max"];
            Messages.HorseNoAutoSell = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["no_auto_sell"];
            Messages.HorseAutoSellPriceFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["auto_sell_format"];
            Messages.HorseAutoSellOthersFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["auto_sell_others"];
            Messages.HorseAutoSellFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["auto_sell"];
            Messages.HorseCantAutoSellTacked = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["cannot_auto_sell_tacked"];

            Messages.HorseCurrentlyCategoryFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["marked_as"];
            Messages.HorseMarkAsCategory = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["marking_options"];
            Messages.HorseStats = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["horse_stats"];
            Messages.HorseTacked = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["wearing_tacked"];
            Messages.HorseTackFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["tacked_format"];

            Messages.HorseCompanion = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["companion"];
            Messages.HorseCompanionFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["companion_selected"];
            Messages.HorseCompanionChangeButton = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["companion_change_button"];
            Messages.HorseNoCompanion = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["no_companion"];

            Messages.HorseAdvancedStatsFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["advanced_stats"];
            Messages.HorseBreedDetailsFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["breed_details"];
            Messages.HorseHeightRangeFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["height_range"];
            Messages.HorsePossibleColorsFormat = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["possible_colors"];
            Messages.HorseReleaseButton = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["release_horse"];
            Messages.HorseOthers = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["other_horses"];

            Messages.HorseDescriptionEditFormat = (string)gameData["messages"]["meta"]["horse"]["description_edit"];
            Messages.HorseEquipTackMessageFormat = (string)gameData["messages"]["meta"]["horse"]["equip_tack_message"];
            Messages.HorseUnEquipTackMessageFormat = (string)gameData["messages"]["meta"]["horse"]["unequip_tack_message"];
            Messages.HorseStopRidingMessage = (string)gameData["messages"]["meta"]["horse"]["stop_riding_message"];

            Messages.HorsePetMessageFormat = (string)gameData["messages"]["meta"]["horse"]["pet_horse"];
            Messages.HorsePetTooHappy = (string)gameData["messages"]["meta"]["horse"]["pet_horse_too_happy"];
            Messages.HorsePetTooTired = (string)gameData["messages"]["meta"]["horse"]["pet_horse_too_sleepy"];
            Messages.HorseSetNewCategoryMessageFormat = (string)gameData["messages"]["meta"]["horse"]["horse_set_new_category"];

            Messages.HorseAutoSellMenuFormat = (string)gameData["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_meta"];
            Messages.HorseIsAutoSell = (string)gameData["messages"]["meta"]["horse"]["auto_sell"]["is_auto_sell"];
            Messages.HorseAutoSellConfirmedFormat = (string)gameData["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_confirmed"];
            Messages.HorseAutoSellValueTooHigh = (string)gameData["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_to_high"];
            Messages.HorseAutoSellRemoved = (string)gameData["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_remove"];

            Messages.HorseSetAutoSell = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["set_auto_sell"];
            Messages.HorseChangeAutoSell = (string)gameData["messages"]["meta"]["horse"]["horse_inventory"]["change_auto_sell"];
            Messages.HorseTackFailAutoSell = (string)gameData["messages"]["meta"]["horse"]["tack_fail_autosell"];

            Messages.HorseAreYouSureYouWantToReleaseFormat = (string)gameData["messages"]["meta"]["horse"]["horse_release"];
            Messages.HorseCantReleaseTheHorseYourRidingOn = (string)gameData["messages"]["meta"]["horse"]["cant_release_currently_riding"];
            Messages.HorseReleasedMeta = (string)gameData["messages"]["meta"]["horse"]["released_horse"];
            Messages.HorseReleasedBy = (string)gameData["messages"]["meta"]["horse"]["released_by_message"];

            // All Stats (basic)

            Messages.HorseAllBasicStats = (string)gameData["messages"]["meta"]["horse"]["allstats_basic"]["all_baisc_stats"];
            Messages.HorseBasicStatEntryFormat = (string)gameData["messages"]["meta"]["horse"]["allstats_basic"]["horse_entry"];

            // All Stats (all)
            Messages.HorseAllStatsHeader = (string)gameData["messages"]["meta"]["horse"]["allstats"]["all_stats_header"];
            Messages.HorseNameEntryFormat = (string)gameData["messages"]["meta"]["horse"]["allstats"]["horse_name_entry"];
            Messages.HorseBasicStatsCompactedFormat = (string)gameData["messages"]["meta"]["horse"]["allstats"]["basic_stats_compact"];
            Messages.HorseAdvancedStatsCompactedFormat = (string)gameData["messages"]["meta"]["horse"]["allstats"]["advanced_stats_compact"];
            Messages.HorseAllStatsLegend = (string)gameData["messages"]["meta"]["horse"]["allstats"]["legend"];


            // Horse companion menu
            Messages.HorseCompanionMenuHeaderFormat = (string)gameData["messages"]["meta"]["horse"]["companion_menu"]["menu_header"];
            Messages.HorseCompnaionMenuCurrentCompanionFormat = (string)gameData["messages"]["meta"]["horse"]["companion_menu"]["selected_companion"];
            Messages.HorseCompanionEntryFormat = (string)gameData["messages"]["meta"]["horse"]["companion_menu"]["companion_entry"];
            Messages.HorseCompanionEquipMessageFormat = (string)gameData["messages"]["meta"]["horse"]["companion_menu"]["companion_equip_message"];
            Messages.HorseCompanionRemoveMessageFormat = (string)gameData["messages"]["meta"]["horse"]["companion_menu"]["companion_remove_message"];
            Messages.HorseCompanionMenuCurrentlyAvalibleCompanions = (string)gameData["messages"]["meta"]["horse"]["companion_menu"]["companions_avalible"];

            // Horse Feed Menu
            Messages.HorseCurrentStatusFormat = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["current_status"];
            Messages.HorseHoldingHorseFeed = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["holding_horse_feed"];
            Messages.HorsefeedFormat = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["horsefeed_format"];
            Messages.HorseNeighsThanks = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["horse_neigh"];
            Messages.HorseCouldNotFinish = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["horse_could_not_finish"];

            Messages.HorseFeedPersonalityIncreased = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["feed_special_personality"];
            Messages.HorseFeedInteligenceIncreased = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["feed_special_inteligence"];
            Messages.HorseFeedMagicBeanFormat = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["feed_special_magic_bean"];
            Messages.HorseFeedMagicDropletFormat = (string)gameData["messages"]["meta"]["horse"]["feed_horse"]["feed_special_magic_droplet"];

            // Tack menu (horses)
            Messages.HorseTackedAsFollowsFormat = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["tacked_as_follows"];
            Messages.HorseUnEquipSaddleFormat = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["dequip_saddle"];
            Messages.HorseUnEquipSaddlePadFormat = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["dequip_saddle_pad"];
            Messages.HorseUnEquipBridleFormat = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["dequip_bridle"];
            Messages.HorseTackInInventory = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["you_have_following_tack"];
            Messages.HorseLlamaTackInInventory = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["you_have_following_llama_tack"];
            Messages.HorseCamelTackInInventory = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["you_have_following_camel_tack"];
            Messages.HorseEquipFormat = (string)gameData["messages"]["meta"]["horse"]["tack_menu"]["equip_tack"];
            Messages.BackToHorse = (string)gameData["messages"]["meta"]["horse"]["back_to_horse"];


            // Libary
            Messages.LibaryMainMenu = (string)gameData["messages"]["meta"]["libary"]["main_menu"];
            Messages.LibaryFindNpc = (string)gameData["messages"]["meta"]["libary"]["find_npc"];
            Messages.LibaryFindNpcSearchResultsHeader = (string)gameData["messages"]["meta"]["libary"]["find_npc_results_header"];
            Messages.LibaryFindNpcSearchResultFormat = (string)gameData["messages"]["meta"]["libary"]["find_npc_results_format"];
            Messages.LibaryFindNpcSearchNoResults = (string)gameData["messages"]["meta"]["libary"]["find_npc_no_results"];
            Messages.LibaryFindNpcLimit5 = (string)gameData["messages"]["meta"]["libary"]["find_npc_limit5"];

            Messages.LibaryFindRanch = (string)gameData["messages"]["meta"]["libary"]["find_ranch"];
            Messages.LibaryFindRanchResultsHeader = (string)gameData["messages"]["meta"]["libary"]["find_ranch_match_closely"];
            Messages.LibaryFindRanchResultFormat = (string)gameData["messages"]["meta"]["libary"]["find_ranch_result"];
            Messages.LibaryFindRanchResultsNoResults = (string)gameData["messages"]["meta"]["libary"]["find_ranch_no_results"];

            Messages.HorseBreedFormat = (string)gameData["messages"]["meta"]["libary"]["horse_breed_format"];
            Messages.HorseRelativeFormat = (string)gameData["messages"]["meta"]["libary"]["horse_relative_format"];
            Messages.BreedViewerFormat = (string)gameData["messages"]["meta"]["libary"]["breed_preview_format"];
            Messages.BreedViewerMaximumStats = (string)gameData["messages"]["meta"]["libary"]["maximum_stats"];

            // Chat

            Messages.ChatViolationMessageFormat = (string)gameData["messages"]["chat"]["violation_format"];
            Messages.RequiredChatViolations = (int)gameData["messages"]["chat"]["violation_points_required"];

            Messages.GlobalChatFormatForModerators = (string)gameData["messages"]["chat"]["for_others"]["global_format_moderator"];
            Messages.DirectChatFormatForModerators = (string)gameData["messages"]["chat"]["for_others"]["dm_format_moderator"];

            Messages.YouWereSentToPrisionIsle = (string)gameData["messages"]["starved_horse"];

            Messages.HereChatFormat = (string)gameData["messages"]["chat"]["for_others"]["here_format"];
            Messages.IsleChatFormat = (string)gameData["messages"]["chat"]["for_others"]["isle_format"];
            Messages.NearChatFormat = (string)gameData["messages"]["chat"]["for_others"]["near_format"];
            Messages.GlobalChatFormat = (string)gameData["messages"]["chat"]["for_others"]["global_format"];
            Messages.AdsChatFormat = (string)gameData["messages"]["chat"]["for_others"]["ads_format"];
            Messages.DirectChatFormat = (string)gameData["messages"]["chat"]["for_others"]["dm_format"];
            Messages.BuddyChatFormat = (string)gameData["messages"]["chat"]["for_others"]["friend_format"];
            Messages.ModChatFormat = (string)gameData["messages"]["chat"]["for_others"]["mod_format"];
            Messages.AdminChatFormat = (string)gameData["messages"]["chat"]["for_others"]["admin_format"];

            Messages.HereChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["here_format"];
            Messages.IsleChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["isle_format"];
            Messages.NearChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["near_format"];
            Messages.BuddyChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["friend_format"];
            Messages.DirectChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["dm_format"];
            Messages.ModChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["mod_format"];
            Messages.AdsChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["ads_format"];
            Messages.AdminChatFormatForSender = (string)gameData["messages"]["chat"]["for_sender"]["admin_format"];

            Messages.ServerAnnoucementFormat = (string)gameData["messages"]["chat"]["server_annoucement"];

            Messages.DmModBadge = (string)gameData["messages"]["chat"]["dm_moderator"];
            Messages.DmAutoResponse = (string)gameData["messages"]["chat"]["dm_autoreply"];

            Messages.AdminCommandFormat = (string)gameData["messages"]["commands"]["admin_command_completed"];
            Messages.PlayerCommandFormat = (string)gameData["messages"]["commands"]["player_command_completed"];
            Messages.MuteHelp = (string)gameData["messages"]["commands"]["mute_help"];
            Messages.UnMuteHelp = (string)gameData["messages"]["commands"]["unmute_help"];

            Messages.PasswordNotice = (string)gameData["messages"]["chat"]["password_included"];
            Messages.CapsNotice = (string)gameData["messages"]["chat"]["caps_notice"];

            // AutoReply
            Messages.AutoReplyTooLong = (string)gameData["messages"]["auto_reply.too_long"];
            Messages.AutoReplyHasViolations = (string)gameData["messages"]["auto_reply.contains_violations"];

            // Drawing Rooms
            Messages.DrawingLastToDrawFormat = (string)gameData["messages"]["meta"]["drawing_rooms"]["last_draw"];
            Messages.DrawingContentsSavedInSlotFormat = (string)gameData["messages"]["meta"]["drawing_rooms"]["saved"];
            Messages.DrawingContentsLoadedFromSlotFormat = (string)gameData["messages"]["meta"]["drawing_rooms"]["load"];
            Messages.DrawingPlzClearDraw = (string)gameData["messages"]["meta"]["drawing_rooms"]["plz_clear_draw"];
            Messages.DrawingPlzClearLoad = (string)gameData["messages"]["meta"]["drawing_rooms"]["plz_clear_load"];
            Messages.DrawingNotSentNotSubscribed = (string)gameData["messages"]["meta"]["drawing_rooms"]["not_subscribed_draw"];
            Messages.DrawingCannotLoadNotSubscribed = (string)gameData["messages"]["meta"]["drawing_rooms"]["not_subscribed_load"];

            // Brickpoet
            Messages.LastPoetFormat = (string)gameData["messages"]["meta"]["last_poet"];

            // Mutliroom
            Messages.MultiroomParticipentFormat = (string)gameData["messages"]["meta"]["multiroom"]["partcipent_format"];
            Messages.MultiroomPlayersParticipating = (string)gameData["messages"]["meta"]["multiroom"]["other_players_participating"];

            // Dropped Items
            Messages.NothingMessage = (string)gameData["messages"]["meta"]["dropped_items"]["nothing_message"];
            Messages.ItemsOnGroundMessage = (string)gameData["messages"]["meta"]["dropped_items"]["items_message"];
            Messages.GrabItemFormat = (string)gameData["messages"]["meta"]["dropped_items"]["item_format"];
            Messages.ItemInformationFormat = (string)gameData["messages"]["meta"]["dropped_items"]["item_information_format"];
            Messages.GrabAllItemsButton = (string)gameData["messages"]["meta"]["dropped_items"]["grab_all"];
            Messages.DroppedAnItemMessage = (string)gameData["messages"]["dropped_items"]["dropped_item_message"];
            Messages.DroppedItemTileIsFull = (string)gameData["messages"]["dropped_items"]["drop_tile_full"];
            Messages.DroppedItemCouldntPickup = (string)gameData["messages"]["dropped_items"]["other_picked_up"];
            Messages.GrabbedAllItemsMessage = (string)gameData["messages"]["dropped_items"]["grab_all_message"];
            Messages.GrabbedItemMessage = (string)gameData["messages"]["dropped_items"]["grab_message"];
            Messages.GrabAllItemsMessage = (string)gameData["messages"]["dropped_items"]["grab_all_message"];

            Messages.GrabbedAllItemsButInventoryFull = (string)gameData["messages"]["dropped_items"]["grab_all_but_inv_full"];
            Messages.GrabbedItemButInventoryFull = (string)gameData["messages"]["dropped_items"]["grab_but_inv_full"];

            // Tools
            Messages.BinocularsNothing = (string)gameData["messages"]["tools"]["binoculars"];
            Messages.MagnifyNothing = (string)gameData["messages"]["tools"]["magnify"];
            Messages.RakeNothing = (string)gameData["messages"]["tools"]["rake"];
            Messages.ShovelNothing = (string)gameData["messages"]["tools"]["shovel"];

            // Shop
            Messages.ThingsIAmSelling = (string)gameData["messages"]["meta"]["shop"]["selling"];
            Messages.ThingsYouSellMe = (string)gameData["messages"]["meta"]["shop"]["sell_me"];
            Messages.InfinitySign = (string)gameData["messages"]["meta"]["shop"]["infinity"];

            Messages.CantAfford1 = (string)gameData["messages"]["shop"]["cant_afford_1"];
            Messages.CantAfford5 = (string)gameData["messages"]["shop"]["cant_afford_5"];
            Messages.CantAfford25 = (string)gameData["messages"]["shop"]["cant_afford_25"];
            Messages.Brought1Format = (string)gameData["messages"]["shop"]["brought_1"];
            Messages.Brought5Format = (string)gameData["messages"]["shop"]["brought_5"];
            Messages.Brought25Format = (string)gameData["messages"]["shop"]["brought_25"];
            Messages.Sold1Format = (string)gameData["messages"]["shop"]["sold_1"];
            Messages.SoldAllFormat = (string)gameData["messages"]["shop"]["sold_all"];
            Messages.CannotSellYoudGetTooMuchMoney = (string)gameData["messages"]["shop"]["cant_hold_extra_money"];

            Messages.Brought1ButInventoryFull = (string)gameData["messages"]["shop"]["brought_1_but_inv_full"];
            Messages.Brought5ButInventoryFull = (string)gameData["messages"]["shop"]["brought_5_but_inv_full"];
            Messages.Brought25ButInventoryFull = (string)gameData["messages"]["shop"]["brought_25_but_inv_full"];

            // Player List
            Messages.PlayerListHeader = (string)gameData["messages"]["meta"]["player_list"]["playerlist_header"];
            Messages.PlayerListSelectFromFollowing = (string)gameData["messages"]["meta"]["player_list"]["select_from_following"];
            Messages.PlayerListOfBuddiesFormat = (string)gameData["messages"]["meta"]["player_list"]["list_of_buddies_format"];
            Messages.PlayerListOfNearby = (string)gameData["messages"]["meta"]["player_list"]["list_of_players_nearby"];
            Messages.PlayerListOfPlayersFormat = (string)gameData["messages"]["meta"]["player_list"]["list_of_all_players_format"];
            Messages.PlayerListOfPlayersAlphabetically = (string)gameData["messages"]["meta"]["player_list"]["list_of_all_players_alphabetically"];
            Messages.PlayerListMapAllBuddiesForamt = (string)gameData["messages"]["meta"]["player_list"]["map_all_buddies_format"];
            Messages.PlayerListMapAllPlayersFormat = (string)gameData["messages"]["meta"]["player_list"]["map_all_players_format"];
            Messages.PlayerListAbuseReport = (string)gameData["messages"]["meta"]["player_list"]["abuse_report"];

            Messages.MuteButton = (string)gameData["messages"]["meta"]["player_list"]["mute_button"];
            Messages.HearButton = (string)gameData["messages"]["meta"]["player_list"]["hear_button"];

            Messages.ThreeMonthSubscripitionIcon = (int)gameData["messages"]["meta"]["player_list"]["icon_subbed_3month"];
            Messages.YearSubscriptionIcon = (int)gameData["messages"]["meta"]["player_list"]["icon_subbed_year"];
            Messages.NewUserIcon = (int)gameData["messages"]["meta"]["player_list"]["icon_new"];
            Messages.MonthSubscriptionIcon = (int)gameData["messages"]["meta"]["player_list"]["icon_subbed_month"];
            Messages.AdminIcon = (int)gameData["messages"]["meta"]["player_list"]["icon_admin"];
            Messages.ModeratorIcon = (int)gameData["messages"]["meta"]["player_list"]["icon_mod"];

            Messages.BuddyListHeader = (string)gameData["messages"]["meta"]["player_list"]["online_buddy_header"];
            Messages.BuddyListOnlineBuddyEntryFormat = (string)gameData["messages"]["meta"]["player_list"]["online_buddy_format"];
            Messages.BuddyListOfflineBuddys = (string)gameData["messages"]["meta"]["player_list"]["offline_buddys"];
            Messages.BuddyListOfflineBuddyEntryFormat = (string)gameData["messages"]["meta"]["player_list"]["offline_buddy_format"];

            Messages.NearbyPlayersListHeader = (string)gameData["messages"]["meta"]["player_list"]["nearby_player_header"];
            Messages.PlayerListAllAlphabeticalHeader = (string)gameData["messages"]["meta"]["player_list"]["all_players_alphabetical_header"];

            Messages.PlayerListEntryFormat = (string)gameData["messages"]["meta"]["player_list"]["player_format"];

            Messages.PlayerListIdle = (string)gameData["messages"]["meta"]["player_list"]["idle_text"];
            Messages.PlayerListAllHeader = (string)gameData["messages"]["meta"]["player_list"]["all_players_header"];
            Messages.PlayerListIconFormat = (string)gameData["messages"]["meta"]["player_list"]["icon_format"];
            Messages.PlayerListIconInformation = (string)gameData["messages"]["meta"]["player_list"]["icon_info"];

            // Consume
            Messages.ConsumeItemFormat = (string)gameData["messages"]["consume"]["consumed_item_format"];
            Messages.ConsumedButMaxReached = (string)gameData["messages"]["consume"]["consumed_but_max_reached"];

            // Meta Format
            Messages.LocationFormat = (string)gameData["messages"]["meta"]["location_format"];
            Messages.IsleFormat = (string)gameData["messages"]["meta"]["isle_format"];
            Messages.TownFormat = (string)gameData["messages"]["meta"]["town_format"];
            Messages.AreaFormat = (string)gameData["messages"]["meta"]["area_format"];
            Messages.Seperator = (string)gameData["messages"]["meta"]["seperator"];
            Messages.TileFormat = (string)gameData["messages"]["meta"]["tile_format"];
            Messages.ExitThisPlace = (string)gameData["messages"]["meta"]["exit_this_place"];
            Messages.BackToMap = (string)gameData["messages"]["meta"]["back_to_map"];
            Messages.BackToMapHorse = (string)gameData["messages"]["meta"]["back_to_map_horse"];
            Messages.LongFullLine = (string)gameData["messages"]["meta"]["long_full_line"];
            Messages.MetaTerminator = (string)gameData["messages"]["meta"]["end_of_meta"];

            Messages.PlayersHere = (string)gameData["messages"]["meta"]["player_interaction"]["players_here"];
            Messages.NearbyPlayers = (string)gameData["messages"]["meta"]["nearby"]["players_nearby"];
            Messages.North = (string)gameData["messages"]["meta"]["nearby"]["north"];
            Messages.East = (string)gameData["messages"]["meta"]["nearby"]["east"];
            Messages.South = (string)gameData["messages"]["meta"]["nearby"]["south"];
            Messages.West = (string)gameData["messages"]["meta"]["nearby"]["west"];

            Messages.NoPitchforkMeta = (string)gameData["messages"]["meta"]["hay_pile"]["no_pitchfork"];
            Messages.HasPitchforkMeta = (string)gameData["messages"]["meta"]["hay_pile"]["pitchfork"];
            Messages.R1 = (string)gameData["messages"]["meta"]["r1"];
            Messages.PasswordEntry = (string)gameData["messages"]["meta"]["password_input"];

            // Venus Fly Trap
            Messages.VenusFlyTrapFormat = (string)gameData["messages"]["meta"]["venus_flytrap_format"];

            // Shortcut
            Messages.NoTelescope = (string)gameData["messages"]["no_telescope"];

            // Inn
            Messages.InnBuyMeal = (string)gameData["messages"]["meta"]["inn"]["buy_meal"];
            Messages.InnBuyRest = (string)gameData["messages"]["meta"]["inn"]["buy_rest"];
            Messages.InnItemEntryFormat = (string)gameData["messages"]["meta"]["inn"]["inn_entry"];
            Messages.InnEnjoyedServiceFormat = (string)gameData["messages"]["inn"]["enjoyed_service"];
            Messages.InnCannotAffordService = (string)gameData["messages"]["inn"]["cant_afford"];
            Messages.InnFullyRested = (string)gameData["messages"]["inn"]["fully_rested"];

            // Password
            Messages.IncorrectPasswordMessage = (string)gameData["messages"]["incorrect_password"];

            // Fountain
            Messages.FountainMeta = (string)gameData["messages"]["meta"]["fountain"];
            Messages.FountainDrankYourFull = (string)gameData["messages"]["fountain"]["drank_your_fill"];
            Messages.FountainDroppedMoneyFormat = (string)gameData["messages"]["fountain"]["dropped_money"];

            // Highscore
            Messages.HighscoreHeaderMeta = (string)gameData["messages"]["meta"]["highscores"]["header_meta"];
            Messages.HighscoreFormat = (string)gameData["messages"]["meta"]["highscores"]["highscore_format"];
            Messages.BestTimeFormat = (string)gameData["messages"]["meta"]["highscores"]["besttime_format"];

            Messages.GameHighScoreHeaderFormat = (string)gameData["messages"]["meta"]["highscores"]["game_highscore_header"];
            Messages.GameHighScoreFormat = (string)gameData["messages"]["meta"]["highscores"]["game_highscore_format"];

            Messages.GameWinLooseHeaderFormat = (string)gameData["messages"]["meta"]["highscores"]["game_winloose_header"];
            Messages.GameWinLooseFormat = (string)gameData["messages"]["meta"]["highscores"]["game_winloose_format"];

            Messages.GameBestTimeHeaderFormat = (string)gameData["messages"]["meta"]["highscores"]["game_besttime_header"];
            Messages.GameBestTimeFormat = (string)gameData["messages"]["meta"]["highscores"]["game_besttime_format"];

            // Awards
            Messages.AwardHeader = (string)gameData["messages"]["meta"]["awards_page"]["awards_header"];
            Messages.AwardOthersFormat = (string)gameData["messages"]["meta"]["awards_page"]["awards_others_header"];
            Messages.NoAwards = (string)gameData["messages"]["meta"]["awards_page"]["no_awards"];
            Messages.AwardFormat = (string)gameData["messages"]["meta"]["awards_page"]["award_format"];

            // World Peace
            Messages.NoWishingCoins = (string)gameData["messages"]["meta"]["wishing_well"]["no_coins"];
            Messages.YouHaveWishingCoinsFormat = (string)gameData["messages"]["meta"]["wishing_well"]["wish_coins"];
            Messages.WishItemsFormat = (string)gameData["messages"]["meta"]["wishing_well"]["wish_things"];
            Messages.WishMoneyFormat = (string)gameData["messages"]["meta"]["wishing_well"]["wish_money"];
            Messages.WishWorldPeaceFormat = (string)gameData["messages"]["meta"]["wishing_well"]["wish_worldpeace"];

            Messages.TossedCoin = (string)gameData["messages"]["meta"]["wishing_well"]["make_wish"];
            Messages.WorldPeaceOnlySoDeep = (string)gameData["messages"]["meta"]["wishing_well"]["world_peace_message"];
            Messages.WishingWellMeta = (string)gameData["messages"]["meta"]["wishing_well"]["wish_meta"];
            // Sec Codes

            Messages.InvalidSecCodeError = (string)gameData["messages"]["sec_code"]["invalid_sec_code"];
            Messages.YouEarnedAnItemFormat = (string)gameData["messages"]["sec_code"]["item_earned"];
            Messages.YouEarnedAnItemButInventoryWasFullFormat = (string)gameData["messages"]["sec_code"]["item_earned_full_inv"];
            Messages.YouLostAnItemFormat = (string)gameData["messages"]["sec_code"]["item_deleted"];
            Messages.YouEarnedMoneyFormat = (string)gameData["messages"]["sec_code"]["money_earned"];
            Messages.BeatHighscoreFormat = (string)gameData["messages"]["sec_code"]["highscore_beaten"];
            Messages.BeatBestHighscore = (string)gameData["messages"]["sec_code"]["best_highscore_beaten"];
            Messages.BeatBestTimeFormat = (string)gameData["messages"]["sec_code"]["best_time_beaten"];

            // Inventory

            Messages.InventoryHeaderFormat = (string)gameData["messages"]["meta"]["inventory"]["header_format"];
            Messages.InventoryItemFormat = (string)gameData["messages"]["meta"]["inventory"]["item_entry"];
            Messages.ShopEntryFormat = (string)gameData["messages"]["meta"]["inventory"]["shop_entry"];
            Messages.ItemInformationButton = (string)gameData["messages"]["meta"]["inventory"]["item_info_button"];
            Messages.ItemInformationByIdButton = (string)gameData["messages"]["meta"]["inventory"]["item_info_itemid_button"];

            Messages.ItemDropButton = (string)gameData["messages"]["meta"]["inventory"]["item_drop_button"];
            Messages.ItemThrowButton = (string)gameData["messages"]["meta"]["inventory"]["item_throw_button"];
            Messages.ItemConsumeButton = (string)gameData["messages"]["meta"]["inventory"]["item_consume_button"];
            Messages.ItemUseButton = (string)gameData["messages"]["meta"]["inventory"]["item_use_button"];
            Messages.ItemOpenButton = (string)gameData["messages"]["meta"]["inventory"]["item_open_button"];
            Messages.ItemWearButton = (string)gameData["messages"]["meta"]["inventory"]["item_wear_button"];
            Messages.ItemReadButton = (string)gameData["messages"]["meta"]["inventory"]["item_read_button"];

            Messages.ShopBuyButton = (string)gameData["messages"]["meta"]["inventory"]["buy_button"];
            Messages.ShopBuy5Button = (string)gameData["messages"]["meta"]["inventory"]["buy_5_button"];
            Messages.ShopBuy25Button = (string)gameData["messages"]["meta"]["inventory"]["buy_25_button"];

            Messages.SellButton = (string)gameData["messages"]["meta"]["inventory"]["sell_button"];
            Messages.SellAllButton = (string)gameData["messages"]["meta"]["inventory"]["sell_all_button"];
            // Npc

            Messages.NpcStartChatFormat = (string)gameData["messages"]["meta"]["npc"]["start_chat_format"];
            Messages.NpcNoChatpoints = (string)gameData["messages"]["meta"]["npc"]["no_chatpoints"];
            Messages.NpcChatpointFormat = (string)gameData["messages"]["meta"]["npc"]["chatpoint_format"];
            Messages.NpcReplyFormat = (string)gameData["messages"]["meta"]["npc"]["reply_format"];
            Messages.NpcTalkButton = (string)gameData["messages"]["meta"]["npc"]["npc_talk_button"];
            Messages.NpcInformationButton = (string)gameData["messages"]["meta"]["npc"]["npc_information_button"];
            Messages.NpcInformationFormat = (string)gameData["messages"]["meta"]["npc"]["npc_information_format"];

            // Login Failed Reasons
            Messages.LoginFailedReasonBanned = (string)gameData["messages"]["login"]["banned"];
            Messages.LoginFailedReasonBannedIpFormat = (string)gameData["messages"]["login"]["ip_banned"];

            // Disconnect Reasons

            Messages.KickReasonKicked = (string)gameData["messages"]["disconnect"]["kicked"];
            Messages.KickReasonBanned = (string)gameData["messages"]["disconnect"]["banned"];
            Messages.KickReasonIdleFormat = (string)gameData["messages"]["disconnect"]["client_timeout"]["kick_message"];
            Messages.KickReasonNoTime = (string)gameData["messages"]["disconnect"]["no_playtime"];
            Messages.IdleWarningFormat = (string)gameData["messages"]["disconnect"]["client_timeout"]["warn_message"];
            Messages.KickReasonDuplicateLogin = (string)gameData["messages"]["disconnect"]["dupe_login"];

            // Competition Gear

            Messages.EquipCompetitionGearFormat = (string)gameData["messages"]["equips"]["equip_competition_gear_format"];
            Messages.RemoveCompetitionGear = (string)gameData["messages"]["equips"]["removed_competition_gear"];

            // Jewerly
            Messages.EquipJewelryFormat = (string)gameData["messages"]["equips"]["equip_jewelry"];
            Messages.MaxJewelryMessage = (string)gameData["messages"]["equips"]["max_jewelry"];
            Messages.RemoveJewelry = (string)gameData["messages"]["equips"]["removed_jewelry"];

            // Click
            Messages.NothingInterestingHere = (string)gameData["messages"]["click_nothing_message"];

            // Swf
            Messages.WagonCutscene = (string)gameData["transport"]["wagon_cutscene"];
            Messages.BoatCutscene = (string)gameData["transport"]["boat_cutscene"];
            Messages.BallonCutscene = (string)gameData["transport"]["ballon_cutscene"];

            gameData = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return;
        }

    }
}
