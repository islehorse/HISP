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

        public static T[] AsArray<T>(this JsonNode jnode)
        {
            JsonArray jarr = jnode.AsArray();

            T[] array = new T[jarr.Count];

            for (int i = 0; i < jarr.Count; i++)
                array[i] = jarr[i].AsValue().GetValue<T>();

            return array;
        }

    }
    public class GameDataJson
    {
       
        private static JsonNode jdata = JsonNode.Parse("{}");

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
                    jdata.Merge(node);

                }
            }
            else if (File.Exists(ConfigReader.GameData))
            {

                Logger.DebugPrint("Found GAMEDATA FILE ... ");
                string jsonData = File.ReadAllText(ConfigReader.GameData);
                jdata = JsonNode.Parse(jsonData);
                
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
            int totalTowns = jdata["places"]["towns"].AsArray().Count;
            for (int i = 0; i < totalTowns; i++)
            {

                World.Town town = new World.Town();
                town.StartX = ((int)jdata["places"]["towns"].AsArray()[i]["start_x"]);
                town.StartY = ((int)jdata["places"]["towns"].AsArray()[i]["start_y"]);
                town.EndX =   ((int)jdata["places"]["towns"].AsArray()[i]["end_x"]);
                town.EndY =   ((int)jdata["places"]["towns"].AsArray()[i]["end_y"]);
                town.Name =   ((string)jdata["places"]["towns"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Town: " + town.Name + " X " + town.StartX + "," + town.EndX + " Y " + town.StartY + "," + town.EndY);
                World.Towns.Add(town);
            }
        }

        private static void registerZones()
        {
            int totalZones = jdata["places"]["zones"].AsArray().Count;
            for (int i = 0; i < totalZones; i++)
            {

                World.Zone zone = new World.Zone();
                zone.StartX = ((int)jdata["places"]["zones"].AsArray()[i]["start_x"]);
                zone.StartY = ((int)jdata["places"]["zones"].AsArray()[i]["start_y"]);
                zone.EndX =   ((int)jdata["places"]["zones"].AsArray()[i]["end_x"]);
                zone.EndY =   ((int)jdata["places"]["zones"].AsArray()[i]["end_y"]);
                zone.Name =   ((string)jdata["places"]["zones"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Zone: " + zone.Name + " X " + zone.StartX + "," + zone.EndX + " Y " + zone.StartY + "," + zone.EndY);
                World.Zones.Add(zone);
            }

        }
        private static void registerAreas()
        {
            int totalAreas = jdata["places"]["areas"].AsArray().Count;
            for (int i = 0; i < totalAreas; i++)
            {

                World.Area area = new World.Area();
                area.StartX = ((int)jdata["places"]["areas"].AsArray()[i]["start_x"]);
                area.StartY = ((int)jdata["places"]["areas"].AsArray()[i]["start_y"]);
                area.EndX   = ((int)jdata["places"]["areas"].AsArray()[i]["end_x"]);
                area.EndY   = ((int)jdata["places"]["areas"].AsArray()[i]["end_y"]);
                area.Name   = ((string)jdata["places"]["areas"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Area: " + area.Name + " X " + area.StartX + "," + area.EndX + " Y " + area.StartY + "," + area.EndY);
                World.Areas.Add(area);
            }
        }
        private static void registerIsles()
        {

            int totalIsles = jdata["places"]["isles"].AsArray().Count;
            for (int i = 0; i < totalIsles; i++)
            {
                World.Isle isle = new World.Isle();
                isle.StartX = ((int)jdata["places"]["isles"].AsArray()[i]["start_x"]);
                isle.StartY = ((int)jdata["places"]["isles"].AsArray()[i]["start_y"]);
                isle.EndX = ((int)jdata["places"]["isles"].AsArray()[i]["end_x"]);
                isle.EndY = ((int)jdata["places"]["isles"].AsArray()[i]["end_y"]);
                isle.Tileset = ((int)jdata["places"]["isles"].AsArray()[i]["tileset"]);
                isle.Name = ((string)jdata["places"]["isles"].AsArray()[i]["name"]);

                Logger.DebugPrint("Registered Isle: " + isle.Name + " X " + isle.StartX + "," + isle.EndX + " Y " + isle.StartY + "," + isle.EndY + " tileset: " + isle.Tileset);
                World.Isles.Add(isle);
            }
        }

        private static void registerWaypoints()
        {
            int totalWaypoints = jdata["places"]["waypoints"].AsArray().Count;
            for (int i = 0; i < totalWaypoints; i++)
            {
                World.Waypoint waypoint = new World.Waypoint();
                waypoint.Name = ((string)jdata["places"]["waypoints"].AsArray()[i]["name"]);
                waypoint.PosX = ((int)jdata["places"]["waypoints"].AsArray()[i]["pos_x"]);
                waypoint.PosY = ((int)jdata["places"]["waypoints"].AsArray()[i]["pos_y"]);
                waypoint.Type = ((string)jdata["places"]["waypoints"].AsArray()[i]["type"]);
                waypoint.Description = ((string)jdata["places"]["waypoints"].AsArray()[i]["description"]);
                waypoint.WeatherTypesAvalible = jdata["places"]["waypoints"].AsArray()[i]["weather_avalible"].AsArray<string>();
                Logger.DebugPrint("Registered Waypoint: " + waypoint.PosX.ToString() + ", " + waypoint.PosY.ToString() + " TYPE: " + waypoint.Type);
                World.Waypoints.Add(waypoint);
            }
        }
        private static void registerSpecialTiles()
        {
            int totalSpecialTiles = jdata["places"]["special_tiles"].AsArray().Count;
            for (int i = 0; i < totalSpecialTiles; i++)
            {

                World.SpecialTile specialTile = new World.SpecialTile();
                specialTile.X = ((int)jdata["places"]["special_tiles"].AsArray()[i]["x"]);
                specialTile.Y = ((int)jdata["places"]["special_tiles"].AsArray()[i]["y"]);
                specialTile.Title = ((string)jdata["places"]["special_tiles"].AsArray()[i]["title"]);
                specialTile.Description = ((string)jdata["places"]["special_tiles"].AsArray()[i]["description"]);
                specialTile.Code = ((string)jdata["places"]["special_tiles"].AsArray()[i]["code"]);
                if (jdata["places"]["special_tiles"].AsArray()[i]["exit_x"] != null)
                    specialTile.ExitX = ((int)jdata["places"]["special_tiles"].AsArray()[i]["exit_x"]);
                if (jdata["places"]["special_tiles"].AsArray()[i]["exit_y"] != null)
                    specialTile.ExitY = ((int)jdata["places"]["special_tiles"].AsArray()[i]["exit_y"]);
                specialTile.AutoplaySwf = ((string)jdata["places"]["special_tiles"].AsArray()[i]["autoplay_swf"]);
                specialTile.TypeFlag = ((string)jdata["places"]["special_tiles"].AsArray()[i]["type_flag"]);

                Logger.DebugPrint("Registered Special Tile: " + specialTile.Title + " X " + specialTile.X + " Y: " + specialTile.Y);
                World.SpecialTiles.Add(specialTile);
            }
        }
        private static void registerChatWarningReasons()
        {
            int totalReasons = jdata["messages"]["chat"]["reason_messages"].AsArray().Count;
            for (int i = 0; i < totalReasons; i++)
            {
                ChatMsg.Reason reason = new ChatMsg.Reason();
                reason.Name = ((string)jdata["messages"]["chat"]["reason_messages"].AsArray()[i]["name"]);
                reason.Message = ((string)jdata["messages"]["chat"]["reason_messages"].AsArray()[i]["message"]);
                ChatMsg.AddReason(reason);

                Logger.DebugPrint("Registered Chat Warning Reason: " + reason.Name + " (Message: " + reason.Message + ")");
            }

        }
        private static void registerFilteredWords()
        {
            int totalFilters = jdata["messages"]["chat"]["filter"].AsArray().Count;
            for (int i = 0; i < totalFilters; i++)
            {
                ChatMsg.Filter msgFilter = new ChatMsg.Filter();
                msgFilter.FilteredWord = ((string)jdata["messages"]["chat"]["filter"].AsArray()[i]["word"]);
                msgFilter.MatchAll = ((bool)jdata["messages"]["chat"]["filter"].AsArray()[i]["match_all"]);
                msgFilter.Reason = ChatMsg.GetReason((string)jdata["messages"]["chat"]["filter"].AsArray()[i]["reason_type"]);
                ChatMsg.AddFilter(msgFilter);

                Logger.DebugPrint("Registered Filtered Word: " + msgFilter.FilteredWord + " With reason: " + msgFilter.Reason.Name + " (Matching all: " + msgFilter.MatchAll + ")");
            }
        }
        private static void registerWordCorrections()
        {
            int totalCorrections = jdata["messages"]["chat"]["correct"].AsArray().Count;
            for (int i = 0; i < totalCorrections; i++)
            {
                ChatMsg.Correction correction = new ChatMsg.Correction();
                correction.FilteredWord = ((string)jdata["messages"]["chat"]["correct"].AsArray()[i]["word"]);
                correction.ReplacedWord = ((string)jdata["messages"]["chat"]["correct"].AsArray()[i]["new_word"]);
                ChatMsg.AddCorrection(correction);

                Logger.DebugPrint("Registered Word Correction: " + correction.FilteredWord + " to " + correction.ReplacedWord);
            }
        }
        private static void registerTransportPoints()
        {
            int totalTransportPoints = jdata["transport"]["transport_points"].AsArray().Count;
            for (int i = 0; i < totalTransportPoints; i++)
            {
                Transport.TransportPoint transportPoint = new Transport.TransportPoint();
                transportPoint.X = ((int)jdata["transport"]["transport_points"].AsArray()[i]["x"]);
                transportPoint.Y = ((int)jdata["transport"]["transport_points"].AsArray()[i]["y"]);
                transportPoint.Locations = jdata["transport"]["transport_points"].AsArray()[i]["places"].AsArray<int>();
                Transport.TransportPoints.Add(transportPoint);

                Logger.DebugPrint("Registered Transport Point: At X: " + transportPoint.X + " Y: " + transportPoint.Y);
            }
        }

        private static void registerTransportLocations()
        {
            int totalTransportPlaces = jdata["transport"]["transport_places"].AsArray().Count;
            for (int i = 0; i < totalTransportPlaces; i++)
            {
                Transport.TransportLocation transportPlace = new Transport.TransportLocation();
                transportPlace.Id = ((int)jdata["transport"]["transport_places"].AsArray()[i]["id"]);
                transportPlace.Cost = ((int)jdata["transport"]["transport_places"].AsArray()[i]["cost"]);
                transportPlace.GotoX = ((int)jdata["transport"]["transport_places"].AsArray()[i]["goto_x"]);
                transportPlace.GotoY = ((int)jdata["transport"]["transport_places"].AsArray()[i]["goto_y"]);
                transportPlace.Type = ((string)jdata["transport"]["transport_places"].AsArray()[i]["type"]);
                transportPlace.LocationTitle = ((string)jdata["transport"]["transport_places"].AsArray()[i]["place_title"]);
                Transport.TransportLocations.Add(transportPlace);

                Logger.DebugPrint("Registered Transport Location: " + transportPlace.LocationTitle + " To Goto X: " + transportPlace.GotoX + " Y: " + transportPlace.GotoY);
            }
        }
        private static void registerItems()
        {
            int totalItems = jdata["item"]["item_list"].AsArray().Count;
            for (int i = 0; i < totalItems; i++)
            {
                Item.ItemInformation iteminfo = new Item.ItemInformation();
                iteminfo.Id = ((int)jdata["item"]["item_list"].AsArray()[i]["id"]);
                iteminfo.Name = ((string)jdata["item"]["item_list"].AsArray()[i]["name"]);
                iteminfo.PluralName = ((string)jdata["item"]["item_list"].AsArray()[i]["plural_name"]);
                iteminfo.Description = ((string)jdata["item"]["item_list"].AsArray()[i]["description"]);
                iteminfo.IconId = ((int)jdata["item"]["item_list"].AsArray()[i]["icon_id"]);
                iteminfo.SortBy = ((int)jdata["item"]["item_list"].AsArray()[i]["sort_by"]);
                iteminfo.SellPrice = ((int)jdata["item"]["item_list"].AsArray()[i]["sell_price"]);
                iteminfo.EmbedSwf = ((string)jdata["item"]["item_list"].AsArray()[i]["embed_swf"]);
                iteminfo.WishingWell = ((bool)jdata["item"]["item_list"].AsArray()[i]["wishing_well"]);
                iteminfo.Type = ((string)jdata["item"]["item_list"].AsArray()[i]["type"]);
                iteminfo.MiscFlags = jdata["item"]["item_list"].AsArray()[i]["misc_flags"].AsArray<int>();
                int effectsCount = jdata["item"]["item_list"].AsArray()[i]["effects"].AsArray().Count;

                Item.Effects[] effectsList = new Item.Effects[effectsCount];
                for (int ii = 0; ii < effectsCount; ii++)
                {
                    effectsList[ii] = new Item.Effects();
                    effectsList[ii].EffectsWhat = ((string)jdata["item"]["item_list"].AsArray()[i]["effects"].AsArray()[ii]["effect_what"]);
                    effectsList[ii].EffectAmount = ((int)jdata["item"]["item_list"].AsArray()[i]["effects"].AsArray()[ii]["effect_amount"]);
                }

                iteminfo.Effects = effectsList;
                iteminfo.SpawnParamaters = new Item.SpawnRules();
                iteminfo.SpawnParamaters.SpawnCap = ((int)jdata["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_cap"]);
                iteminfo.SpawnParamaters.SpawnInZone = ((string)jdata["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_in_area"]);
                iteminfo.SpawnParamaters.SpawnOnTileType = ((string)jdata["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_on_tile_type"]);
                iteminfo.SpawnParamaters.SpawnOnSpecialTile = ((string)jdata["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_on_special_tile"]);
                iteminfo.SpawnParamaters.SpawnNearSpecialTile = ((string)jdata["item"]["item_list"].AsArray()[i]["spawn_parameters"]["spawn_near_special_tile"]);

                Logger.DebugPrint("Registered Item ID: " + iteminfo.Id + " Name: " + iteminfo.Name + " spawns on: " + iteminfo.SpawnParamaters.SpawnOnTileType);
                Item.AddItemInfo(iteminfo);
            }
        }
        private static void registerThrowables()
        {
            int totalThrowable = jdata["item"]["throwable"].AsArray().Count;
            for (int i = 0; i < totalThrowable; i++)
            {
                Item.ThrowableItem throwableItem = new Item.ThrowableItem();
                throwableItem.Id = ((int)jdata["item"]["throwable"].AsArray()[i]["id"]);
                throwableItem.HitMessage = ((string)jdata["item"]["throwable"].AsArray()[i]["message_hit"]);
                throwableItem.ThrowMessage = ((string)jdata["item"]["throwable"].AsArray()[i]["message_throw"]);
                throwableItem.HitYourselfMessage = ((string)jdata["item"]["throwable"].AsArray()[i]["message_hit_yourself"]);
                Item.AddThrowableItem(throwableItem);
            }
        }

        private static void registerNpcs()
        {
            Logger.DebugPrint("Registering NPCS: ");
            int totalNpcs = jdata["npc_list"].AsArray().Count;
            for (int i = 0; i < totalNpcs; i++)
            {
                int id = (int)jdata["npc_list"].AsArray()[i]["id"];
                int x = (int)jdata["npc_list"].AsArray()[i]["x"];
                int y = (int)jdata["npc_list"].AsArray()[i]["y"];
                bool moves = (bool)jdata["npc_list"].AsArray()[i]["moves"];

                int udlrStartX = 0;
                int udlrStartY = 0;

                if (jdata["npc_list"].AsArray()[i]["udlr_start_x"] != null)
                    udlrStartX = (int)jdata["npc_list"].AsArray()[i]["udlr_start_x"];
                if (jdata["npc_list"].AsArray()[i]["udlr_start_y"] != null)
                    udlrStartY = (int)jdata["npc_list"].AsArray()[i]["udlr_start_y"];

                Npc.NpcEntry npcEntry = new Npc.NpcEntry(id, x, y, moves, udlrStartX, udlrStartY);

                npcEntry.Name = (string)jdata["npc_list"].AsArray()[i]["name"];
                npcEntry.AdminDescription = (string)jdata["npc_list"].AsArray()[i]["admin_description"];
                npcEntry.ShortDescription = (string)jdata["npc_list"].AsArray()[i]["short_description"];
                npcEntry.LongDescription = (string)jdata["npc_list"].AsArray()[i]["long_description"];


                if (jdata["npc_list"].AsArray()[i]["stay_on"] != null)
                    npcEntry.StayOn = (string)jdata["npc_list"].AsArray()[i]["stay_on"];
                if (jdata["npc_list"].AsArray()[i]["requires_questid_completed"] != null)
                    npcEntry.RequiresQuestIdCompleted = (int)jdata["npc_list"].AsArray()[i]["requires_questid_completed"];
                if (jdata["npc_list"].AsArray()[i]["requires_questid_not_completed"] != null)
                    npcEntry.RequiresQuestIdNotCompleted = (int)jdata["npc_list"].AsArray()[i]["requires_questid_not_completed"];
                if (jdata["npc_list"].AsArray()[i]["udlr_script"] != null)
                    npcEntry.UDLRScript = (string)jdata["npc_list"].AsArray()[i]["udlr_script"];

                npcEntry.AdminOnly = (bool)jdata["npc_list"].AsArray()[i]["admin_only"];
                npcEntry.LibarySearchable = (bool)jdata["npc_list"].AsArray()[i]["libary_searchable"];
                npcEntry.IconId = (int)jdata["npc_list"].AsArray()[i]["icon_id"];

                Logger.DebugPrint("NPC ID:" + npcEntry.Id.ToString() + " NAME: " + npcEntry.Name);
                List<Npc.NpcChat> chats = new List<Npc.NpcChat>();
                int totalNpcChat = jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray().Count;
                for (int ii = 0; ii < totalNpcChat; ii++)
                {
                    Npc.NpcChat npcChat = new Npc.NpcChat();
                    npcChat.Id = (int)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["chatpoint_id"];
                    npcChat.ChatText = (string)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["chat_text"];
                    npcChat.ActivateQuestId = (int)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["activate_questid"];

                    Logger.DebugPrint("CHATPOINT ID: " + npcChat.Id.ToString() + " TEXT: " + npcChat.ChatText);
                    int totalNpcReply = jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray().Count;
                    List<Npc.NpcReply> replys = new List<Npc.NpcReply>();
                    for (int iii = 0; iii < totalNpcReply; iii++)
                    {
                        Npc.NpcReply npcReply = new Npc.NpcReply();
                        npcReply.Id = (int)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["reply_id"];
                        npcReply.ReplyText = (string)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["reply_text"];
                        npcReply.GotoChatpoint = (int)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["goto_chatpoint"];

                        if (jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_completed"] != null)
                            npcReply.RequiresQuestIdCompleted = (int)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_completed"];

                        if (jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_not_completed"] != null)
                            npcReply.RequiresQuestIdNotCompleted = (int)jdata["npc_list"].AsArray()[i]["chatpoints"].AsArray()[ii]["replies"].AsArray()[iii]["requires_questid_not_completed"];

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
            int totalQuests = jdata["quest_list"].AsArray().Count;
            for (int i = 0; i < totalQuests; i++)
            {
                Quest.QuestEntry quest = new Quest.QuestEntry();
                quest.Id = (int)jdata["quest_list"].AsArray()[i]["id"];
                quest.Notes = (string)jdata["quest_list"].AsArray()[i]["notes"];
                if (jdata["quest_list"].AsArray()[i]["title"] != null)
                    quest.Title = (string)jdata["quest_list"].AsArray()[i]["title"];
                quest.RequiresQuestIdCompleteStatsMenu = jdata["quest_list"].AsArray()[i]["requires_questid_statsmenu"].AsArray<int>();
                if (jdata["quest_list"].AsArray()[i]["alt_activation"] != null)
                {
                    quest.AltActivation = new Quest.QuestAltActivation();
                    quest.AltActivation.Type = (string)jdata["quest_list"].AsArray()[i]["alt_activation"]["type"];
                    quest.AltActivation.ActivateX = (int)jdata["quest_list"].AsArray()[i]["alt_activation"]["x"];
                    quest.AltActivation.ActivateY = (int)jdata["quest_list"].AsArray()[i]["alt_activation"]["y"];
                }
                quest.Tracked = (bool)jdata["quest_list"].AsArray()[i]["tracked"];
                quest.MaxRepeats = (int)jdata["quest_list"].AsArray()[i]["max_repeats"];
                quest.MoneyCost = (int)jdata["quest_list"].AsArray()[i]["money_cost"];
                int itemsRequiredCount = jdata["quest_list"].AsArray()[i]["items_required"].AsArray().Count;

                List<Quest.QuestItemInfo> itmInfo = new List<Quest.QuestItemInfo>();
                for (int ii = 0; ii < itemsRequiredCount; ii++)
                {
                    Quest.QuestItemInfo itemInfo = new Quest.QuestItemInfo();
                    itemInfo.ItemId = (int)jdata["quest_list"].AsArray()[i]["items_required"].AsArray()[ii]["item_id"];
                    itemInfo.Quantity = (int)jdata["quest_list"].AsArray()[i]["items_required"].AsArray()[ii]["quantity"];
                    itmInfo.Add(itemInfo);
                }
                quest.ItemsRequired = itmInfo.ToArray();
                if (jdata["quest_list"].AsArray()[i]["fail_npc_chat"] != null)
                    quest.FailNpcChat = (string)jdata["quest_list"].AsArray()[i]["fail_npc_chat"];
                quest.MoneyEarned = (int)jdata["quest_list"].AsArray()[i]["money_gained"];

                int itemsGainedCount = jdata["quest_list"].AsArray()[i]["items_gained"].AsArray().Count;
                itmInfo = new List<Quest.QuestItemInfo>();
                for (int ii = 0; ii < itemsGainedCount; ii++)
                {
                    Quest.QuestItemInfo itemInfo = new Quest.QuestItemInfo();
                    itemInfo.ItemId = (int)jdata["quest_list"].AsArray()[i]["items_gained"].AsArray()[ii]["item_id"];
                    itemInfo.Quantity = (int)jdata["quest_list"].AsArray()[i]["items_gained"].AsArray()[ii]["quantity"];
                    itmInfo.Add(itemInfo);
                }
                quest.ItemsEarned = itmInfo.ToArray();

                quest.QuestPointsEarned = (int)jdata["quest_list"].AsArray()[i]["quest_points"];
                quest.SetNpcChatpoint = (int)jdata["quest_list"].AsArray()[i]["set_npc_chatpoint"];
                quest.GotoNpcChatpoint = (int)jdata["quest_list"].AsArray()[i]["goto_npc_chatpoint"];
                if (jdata["quest_list"].AsArray()[i]["warp_x"] != null)
                    quest.WarpX = (int)jdata["quest_list"].AsArray()[i]["warp_x"];
                if (jdata["quest_list"].AsArray()[i]["warp_y"] != null)
                    quest.WarpY = (int)jdata["quest_list"].AsArray()[i]["warp_y"];
                if (jdata["quest_list"].AsArray()[i]["success_message"] != null)
                    quest.SuccessMessage = (string)jdata["quest_list"].AsArray()[i]["success_message"];
                if (jdata["quest_list"].AsArray()[i]["success_npc_chat"] != null)
                    quest.SuccessNpcChat = (string)jdata["quest_list"].AsArray()[i]["success_npc_chat"];
                if (jdata["quest_list"].AsArray()[i]["requires_awardid"] != null)
                    quest.AwardRequired = (int)jdata["quest_list"].AsArray()[i]["requires_awardid"];
                quest.RequiresQuestIdCompleted = jdata["quest_list"].AsArray()[i]["requires_questid_completed"].AsArray<int>();
                quest.RequiresQuestIdNotCompleted = jdata["quest_list"].AsArray()[i]["requires_questid_not_completed"].AsArray<int>();
                quest.HideReplyOnFail = (bool)jdata["quest_list"].AsArray()[i]["hide_reply_on_fail"];
                if (jdata["quest_list"].AsArray()[i]["difficulty"] != null)
                    quest.Difficulty = (string)jdata["quest_list"].AsArray()[i]["difficulty"];
                if (jdata["quest_list"].AsArray()[i]["author"] != null)
                    quest.Author = (string)jdata["quest_list"].AsArray()[i]["author"];
                if (jdata["quest_list"].AsArray()[i]["chained_questid"] != null)
                    quest.ChainedQuestId = (int)jdata["quest_list"].AsArray()[i]["chained_questid"];
                quest.Minigame = (bool)jdata["quest_list"].AsArray()[i]["minigame"];

                Logger.DebugPrint("Registered Quest: " + quest.Id);
                Quest.AddQuestEntry(quest);
            }
        }

        private static void registerShops()
        {

            int totalShops = jdata["shop_list"].AsArray().Count;
            for (int i = 0; i < totalShops; i++)
            {
                int id = (int)jdata["shop_list"].AsArray()[i]["id"];
                int[] item_list = jdata["shop_list"].AsArray()[i]["stocks_itemids"].AsArray<int>();
                Shop shop = new Shop(item_list, id);
                shop.BuyPricePercentage = (int)jdata["shop_list"].AsArray()[i]["buy_percent"];
                shop.SellPricePercentage = (int)jdata["shop_list"].AsArray()[i]["sell_percent"];
                shop.BuysItemTypes = jdata["shop_list"].AsArray()[i]["buys_item_types"].AsArray<string>();

                Logger.DebugPrint("Registered Shop ID: " + shop.Id + " Selling items at " + shop.SellPricePercentage + "% and buying at " + shop.BuyPricePercentage);
            }
        }

        private static void registerAwards()
        {
            int totalAwards = jdata["award_list"].AsArray().Count;
            Award.GlobalAwardList = new Award.AwardEntry[totalAwards];
            for (int i = 0; i < totalAwards; i++)
            {

                Award.AwardEntry award = new Award.AwardEntry();
                award.Id = (int)jdata["award_list"].AsArray()[i]["id"];
                award.Sort = (int)jdata["award_list"].AsArray()[i]["sort_by"];
                award.Title = (string)jdata["award_list"].AsArray()[i]["title"];
                award.IconId = (int)jdata["award_list"].AsArray()[i]["icon_id"];
                award.MoneyBonus = (int)jdata["award_list"].AsArray()[i]["earn_money"];
                award.CompletionText = (string)jdata["award_list"].AsArray()[i]["on_complete_text"];
                award.Description = (string)jdata["award_list"].AsArray()[i]["description"];

                Award.GlobalAwardList[i] = award;

                Logger.DebugPrint("Registered Award ID: " + award.Id + " - " + award.Title);
            }
        }
        private static void registerAbuseReportReasons()
        {
            int totalAbuseReportReasons = jdata["messages"]["meta"]["abuse_report"]["reasons"].AsArray().Count;
            for (int i = 0; i < totalAbuseReportReasons; i++)
            {
                AbuseReport.ReportReason reason = new AbuseReport.ReportReason();
                reason.Id = (string)jdata["messages"]["meta"]["abuse_report"]["reasons"].AsArray()[i]["id"];
                reason.Name = (string)jdata["messages"]["meta"]["abuse_report"]["reasons"].AsArray()[i]["name"];
                reason.Meta = (string)jdata["messages"]["meta"]["abuse_report"]["reasons"].AsArray()[i]["meta"];
                AbuseReport.AddReason(reason);
                Logger.DebugPrint("Registered Abuse Report Reason: " + reason.Name);
            }
        }
        private static void registerOverlayTileDepth()
        {
            List<Map.TileDepth> overlayTilesDepth = new List<Map.TileDepth>();
            int totalOverlayTileDepth = jdata["tile_parameters"]["overlay_tiles"].AsArray().Count;
            for (int i = 0; i < totalOverlayTileDepth; i++)
            {
                Map.TileDepth tileDepth = new Map.TileDepth();
                tileDepth.Passable = (bool)jdata["tile_parameters"]["overlay_tiles"].AsArray()[i]["passable"];
                tileDepth.ShowPlayer = (bool)jdata["tile_parameters"]["overlay_tiles"].AsArray()[i]["show_player"];
                Logger.DebugPrint("Registered Overlay Tile: " + i + " Depth; Passable: " + tileDepth.Passable + " ShowPlayer: " + tileDepth.ShowPlayer);
                overlayTilesDepth.Add(tileDepth);
            }
            Map.OverlayTileDepth = overlayTilesDepth.ToArray();
        }
        private static void registerTerrianTileTypes()
        {
            List<Map.TerrainTile> terrainTiles = new List<Map.TerrainTile>();
            int totalTerrainTiles = jdata["tile_parameters"]["terrain_tiles"].AsArray().Count;
            for (int i = 0; i < totalTerrainTiles; i++)
            {
                Map.TerrainTile tile = new Map.TerrainTile();
                tile.Passable = (bool)jdata["tile_parameters"]["terrain_tiles"].AsArray()[i]["passable"];
                tile.Type = (string)jdata["tile_parameters"]["terrain_tiles"].AsArray()[i]["tile_type"];
                Logger.DebugPrint("Registered Tile Information: " + i + " Passable: " + tile.Passable + " Type: " + tile.Type);
                terrainTiles.Add(tile);
            }
            Map.TerrainTiles = terrainTiles.ToArray();
        }
        private static void registerInns()
        {
            int totalInns = jdata["inns"].AsArray().Count;
            for (int i = 0; i < totalInns; i++)
            {
                int id = (int)jdata["inns"].AsArray()[i]["id"];
                int[] restsOffered = jdata["inns"].AsArray()[i]["rests_offered"].AsArray<int>();
                int[] mealsOffered = jdata["inns"].AsArray()[i]["meals_offered"].AsArray<int>();
                int buyPercent = (int)jdata["inns"].AsArray()[i]["buy_percent"];
                Inn inn = new Inn(id, restsOffered, mealsOffered, buyPercent);

                Logger.DebugPrint("Registered Inn: " + inn.Id + " Buying at: " + inn.BuyPercentage.ToString() + "%!");
            }
        }

        private static void registerPoets()
        {
            int totalPoets = jdata["poetry"].AsArray().Count;
            for (int i = 0; i < totalPoets; i++)
            {
                Brickpoet.PoetryEntry entry = new Brickpoet.PoetryEntry();
                entry.Id = (int)jdata["poetry"].AsArray()[i]["id"];
                entry.Word = (string)jdata["poetry"].AsArray()[i]["word"];
                entry.Room = (int)jdata["poetry"].AsArray()[i]["room_id"];
                Brickpoet.AddPoetEntry(entry);

                Logger.DebugPrint("Registered poet: " + entry.Id.ToString() + " word: " + entry.Word + " in room " + entry.Room.ToString());
            }
        }

        private static void registerBreeds()
        {
            int totalBreeds = jdata["horses"]["breeds"].AsArray().Count;
            for (int i = 0; i < totalBreeds; i++)
            {
                HorseInfo.Breed horseBreed = new HorseInfo.Breed();

                horseBreed.Id = (int)jdata["horses"]["breeds"].AsArray()[i]["id"];
                horseBreed.Name = (string)jdata["horses"]["breeds"].AsArray()[i]["name"];
                horseBreed.Description = (string)jdata["horses"]["breeds"].AsArray()[i]["description"];

                int speed = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["speed"];
                int strength = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["strength"];
                int conformation = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["conformation"];
                int agility = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["agility"];
                int inteligence = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["inteligence"];
                int endurance = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["endurance"];
                int personality = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["personality"];
                int height = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["height"];
                horseBreed.BaseStats = new HorseInfo.AdvancedStats(null, speed, strength, conformation, agility, inteligence, endurance, personality, height);
                horseBreed.BaseStats.MinHeight = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["min_height"];
                horseBreed.BaseStats.MaxHeight = (int)jdata["horses"]["breeds"].AsArray()[i]["base_stats"]["max_height"];

                horseBreed.Colors = jdata["horses"]["breeds"].AsArray()[i]["colors"].AsArray<string>();
                horseBreed.SpawnOn = (string)jdata["horses"]["breeds"].AsArray()[i]["spawn_on"];
                horseBreed.SpawnInArea = (string)jdata["horses"]["breeds"].AsArray()[i]["spawn_area"];
                horseBreed.Swf = (string)jdata["horses"]["breeds"].AsArray()[i]["swf"];
                horseBreed.Type = (string)jdata["horses"]["breeds"].AsArray()[i]["type"];

                HorseInfo.AddBreed(horseBreed);
                Logger.DebugPrint("Registered Horse Breed: #" + horseBreed.Id + ": " + horseBreed.Name);
            }
        }

        private static void registerBreedPricesPawneerOrder()
        {
            int totalBreedPrices = jdata["horses"]["pawneer_base_price"].AsArray().Count;
            for (int i = 0; i < totalBreedPrices; i++)
            {
                int id = (int)jdata["horses"]["pawneer_base_price"].AsArray()[i]["breed_id"];
                int price = (int)jdata["horses"]["pawneer_base_price"].AsArray()[i]["price"];
                Pawneer pawneerPricing = new Pawneer(id, price);
                Pawneer.AddPawneerPriceModel(pawneerPricing);
                Logger.DebugPrint("Registered Pawneer Base Price " + pawneerPricing.BreedId + " for $" + pawneerPricing.BasePrice.ToString("N0", CultureInfo.InvariantCulture));
            }
        }

        private static void registerHorseCategorys()
        {
            int totalCategories = jdata["horses"]["categorys"].AsArray().Count;
            for (int i = 0; i < totalCategories; i++)
            {
                HorseInfo.Category category = new HorseInfo.Category();
                category.Name = (string)jdata["horses"]["categorys"].AsArray()[i]["name"];
                category.MetaOthers = (string)jdata["horses"]["categorys"].AsArray()[i]["message_others"];
                category.Meta = (string)jdata["horses"]["categorys"].AsArray()[i]["message"];
                HorseInfo.AddHorseCategory(category);
                Logger.DebugPrint("Registered horse category type: " + category.Name);
            }
        }

        private static void registerTrackedItems()
        {

            int totalTrackedItems = jdata["messages"]["meta"]["misc_stats"]["tracked_items"].AsArray().Count;
            for (int i = 0; i < totalTrackedItems; i++)
            {
                Tracking.TrackedItemStatsMenu trackedItem = new Tracking.TrackedItemStatsMenu();
                trackedItem.What = (string)jdata["messages"]["meta"]["misc_stats"]["tracked_items"].AsArray()[i]["id"];
                trackedItem.Value = (string)jdata["messages"]["meta"]["misc_stats"]["tracked_items"].AsArray()[i]["value"];
                Tracking.TrackedItemsStatsMenu.Add(trackedItem);
                Logger.DebugPrint("Registered Tracked Item: " + trackedItem.What + " value: " + trackedItem.Value);
            }
        }
        private static void registerVets()
        {
            int totalVets = jdata["services"]["vet"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalVets; i++)
            {
                double cost = (double)jdata["services"]["vet"]["price_multipliers"].AsArray()[i]["cost"];
                int id = (int)jdata["services"]["vet"]["price_multipliers"].AsArray()[i]["id"];
                Vet veternarian = new Vet(id, cost);
                Logger.DebugPrint("Registered Vet: " + veternarian.Id + " selling at: " + veternarian.PriceMultiplier.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void registerGroomers()
        {
            int totalGroomers = jdata["services"]["groomer"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalGroomers; i++)
            {
                double cost = (double)jdata["services"]["groomer"]["price_multipliers"].AsArray()[i]["cost"];
                int id = (int)jdata["services"]["groomer"]["price_multipliers"].AsArray()[i]["id"];
                int max = (int)jdata["services"]["groomer"]["price_multipliers"].AsArray()[i]["max"];
                Groomer groomer = new Groomer(id, cost, max);
                Logger.DebugPrint("Registered Groomer: " + groomer.Id + " selling at: " + groomer.PriceMultiplier.ToString(CultureInfo.InvariantCulture));
            }

        }

        private static void registerFarriers()
        {
            int totalFarriers = jdata["services"]["farrier"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalFarriers; i++)
            {
                int id = (int)jdata["services"]["farrier"]["price_multipliers"].AsArray()[i]["id"];
                int steel = (int)jdata["services"]["farrier"]["price_multipliers"].AsArray()[i]["steel"];
                int steelcost = (int)jdata["services"]["farrier"]["price_multipliers"].AsArray()[i]["steel_cost"];
                int iron = (int)jdata["services"]["farrier"]["price_multipliers"].AsArray()[i]["iron"];
                int ironcost = (int)jdata["services"]["farrier"]["price_multipliers"].AsArray()[i]["iron_cost"];

                Farrier farrier = new Farrier(id, steel, steelcost, iron, ironcost);
                Logger.DebugPrint("Registered Farrier: " + farrier.Id);
            }
        }

        private static void registerBarns()
        {
            int totalBarns = jdata["services"]["barn"]["price_multipliers"].AsArray().Count;
            for (int i = 0; i < totalBarns; i++)
            {
                int id = (int)jdata["services"]["barn"]["price_multipliers"].AsArray()[i]["id"];
                double tired_cost = (double)jdata["services"]["barn"]["price_multipliers"].AsArray()[i]["tired_cost"];
                double hunger_cost = (double)jdata["services"]["barn"]["price_multipliers"].AsArray()[i]["hunger_cost"];
                double thirst_cost = (double)jdata["services"]["barn"]["price_multipliers"].AsArray()[i]["thirst_cost"];


                Barn barn = new Barn(id, tired_cost, hunger_cost, thirst_cost);
                Logger.DebugPrint("Registered Barn: " + barn.Id);
            }
        }

        private static void registerLibaryBooks()
        {
            int totalBooks = jdata["books"].AsArray().Count;
            for (int i = 0; i < totalBooks; i++)
            {
                int id = (int)jdata["books"][i]["id"];
                string author = (string)jdata["books"][i]["author"];
                string title = (string)jdata["books"][i]["title"];
                string text = (string)jdata["books"][i]["text"];
                Book book = new Book(id, title, author, text);
                Logger.DebugPrint("Registered Library Book: " + book.Id + " " + book.Title + " by " + book.Author);
            }
        }

        private static void registerCrafts()
        {
            int totalWorkshops = jdata["workshop"].AsArray().Count;
            for (int i = 0; i < totalWorkshops; i++)
            {
                Workshop wkShop = new Workshop();
                wkShop.X = (int)jdata["workshop"].AsArray()[i]["pos_x"];
                wkShop.Y = (int)jdata["workshop"].AsArray()[i]["pos_y"];
                int totalCraftableItems = jdata["workshop"].AsArray()[i]["craftable_items"].AsArray().Count;
                for (int ii = 0; ii < totalCraftableItems; ii++)
                {
                    Workshop.CraftableItem craftableItem = new Workshop.CraftableItem();
                    craftableItem.Id = (int)jdata["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["id"];
                    craftableItem.GiveItemId = (int)jdata["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["give_item"];
                    craftableItem.MoneyCost = (int)jdata["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["money_cost"];
                    int totalItemsRequired = jdata["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["required_items"].AsArray().Count;
                    for (int iii = 0; iii < totalItemsRequired; iii++)
                    {
                        Workshop.RequiredItem requiredItem = new Workshop.RequiredItem();
                        requiredItem.RequiredItemId = (int)jdata["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["required_items"].AsArray()[iii]["req_item"];
                        requiredItem.RequiredItemCount = (int)jdata["workshop"].AsArray()[i]["craftable_items"].AsArray()[ii]["required_items"].AsArray()[iii]["req_quantity"];
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
            int totalRanchBuildings = jdata["ranch"]["ranch_buildings"]["buildings"].AsArray().Count;
            for (int i = 0; i < totalRanchBuildings; i++)
            {
                int id = (int)jdata["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["id"];
                int cost = (int)jdata["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["cost"];
                string title = (string)jdata["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["title"];
                string description = (string)jdata["ranch"]["ranch_buildings"]["buildings"].AsArray()[i]["description"];

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
            int totalRanchUpgrades = jdata["ranch"]["ranch_buildings"]["upgrades"].AsArray().Count;
            for (int i = 0; i < totalRanchUpgrades; i++)
            {
                int id = (int)jdata["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["id"];
                int cost = (int)jdata["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["cost"];
                string title = (string)jdata["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["title"];
                string description = (string)jdata["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["description"];

                Ranch.RanchUpgrade ranchUpgrade = new Ranch.RanchUpgrade();

                if (jdata["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["limit"] != null)
                    ranchUpgrade.Limit = (int)jdata["ranch"]["ranch_buildings"]["upgrades"].AsArray()[i]["limit"];
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
            int totalRanchLocations = jdata["ranch"]["ranch_locations"].AsArray().Count;
            for (int i = 0; i < totalRanchLocations; i++)
            {
                int x = (int)jdata["ranch"]["ranch_locations"].AsArray()[i]["x"];
                int y = (int)jdata["ranch"]["ranch_locations"].AsArray()[i]["y"];
                int id = (int)jdata["ranch"]["ranch_locations"].AsArray()[i]["id"];
                int value = (int)jdata["ranch"]["ranch_locations"].AsArray()[i]["value"];
                Ranch ranchEntry = new Ranch(x, y, id, value);
                Ranch.Ranches.Add(ranchEntry);
                Logger.DebugPrint("Registered Ranch id " + id + " at X: " + ranchEntry.X + " Y: " + ranchEntry.Y);

            }
        }

        private static void registerRiddlerRiddles()
        {
            int totalRiddles = jdata["riddle_room"].AsArray().Count;
            for (int i = 0; i < totalRiddles; i++)
            {
                int id = (int)jdata["riddle_room"].AsArray()[i]["id"];
                string riddle = (string)jdata["riddle_room"].AsArray()[i]["riddle"];
                string[] answers = jdata["riddle_room"].AsArray()[i]["answers"].AsArray<string>();
                string reason = (string)jdata["riddle_room"].AsArray()[i]["reason"];
                Riddler riddlerRiddle = new Riddler(id, riddle, answers, reason);
                Logger.DebugPrint("Registered Riddler Riddle: " + riddlerRiddle.Riddle);

            }
        }

        private static void registerBBCodes()
        {
            int totalBBocdes = jdata["bbcode"].AsArray().Count;
            for (int i = 0; i < totalBBocdes; i++)
            {
                string tag = (string)jdata["bbcode"].AsArray()[i]["tag"];
                string meta = (string)jdata["bbcode"].AsArray()[i]["meta"];
                BBCode code = new BBCode(tag, meta);
                Logger.DebugPrint("Registered BBCODE: " + code.Tag + " to " + code.MetaTranslation);
            }
        }
        private static void registerTrainingPens()
        {
            int totalTrainingPens = jdata["training_pens"].AsArray().Count;
            for (int i = 0; i < totalTrainingPens; i++)
            {
                Trainer trainer = new Trainer();
                trainer.Id = (int)jdata["training_pens"].AsArray()[i]["trainer_id"];
                trainer.ImprovesStat = (string)jdata["training_pens"].AsArray()[i]["improves_stat"];
                trainer.ImprovesAmount = (int)jdata["training_pens"].AsArray()[i]["improves_amount"];
                trainer.ThirstCost = (int)jdata["training_pens"].AsArray()[i]["thirst_cost"];
                trainer.MoodCost = (int)jdata["training_pens"].AsArray()[i]["mood_cost"];
                trainer.HungerCost = (int)jdata["training_pens"].AsArray()[i]["hunger_cost"];
                trainer.MoneyCost = (int)jdata["training_pens"].AsArray()[i]["money_cost"];
                trainer.ExperienceGained = (int)jdata["training_pens"].AsArray()[i]["experience"];
                Trainer.Trainers.Add(trainer);
                Logger.DebugPrint("Registered Training Pen: " + trainer.Id + " for " + trainer.ImprovesStat);
            }

        }

        private static void registerArenas()
        {
            int totalArenas = jdata["arena"]["arena_list"].AsArray().Count;
            for (int i = 0; i < totalArenas; i++)
            {
                int arenaId = (int)jdata["arena"]["arena_list"].AsArray()[i]["arena_id"];
                string arenaType = (string)jdata["arena"]["arena_list"].AsArray()[i]["arena_type"];
                int arenaEntryCost = (int)jdata["arena"]["arena_list"].AsArray()[i]["entry_cost"];
                int raceEvery = (int)jdata["arena"]["arena_list"].AsArray()[i]["race_every"];
                int slots = (int)jdata["arena"]["arena_list"].AsArray()[i]["slots"];
                int timeout = (int)jdata["arena"]["arena_list"].AsArray()[i]["timeout"];

                Arena arena = new Arena(arenaId, arenaType, arenaEntryCost, raceEvery, slots, timeout);
                Logger.DebugPrint("Registered Arena: " + arena.Id.ToString() + " as " + arena.Type);
            }
            Arena.ExpRewards = jdata["arena"]["arena_exp"].AsArray<int>();
        }

        private static void registerLeasers()
        {
            int totalLeasers = jdata["leaser"].AsArray().Count;
            for (int i = 0; i < totalLeasers; i++)
            {
                int breedId = (int)jdata["leaser"].AsArray()[i]["horse"]["breed"];

                int saddle = -1;
                int saddlePad = -1;
                int bridle = -1;

                if (jdata["leaser"].AsArray()[i]["horse"]["tack"]["saddle"] != null)
                    saddle = (int)jdata["leaser"].AsArray()[i]["horse"]["tack"]["saddle"];

                if (jdata["leaser"].AsArray()[i]["horse"]["tack"]["saddle_pad"] != null)
                    saddlePad = (int)jdata["leaser"].AsArray()[i]["horse"]["tack"]["saddle_pad"];

                if (jdata["leaser"].AsArray()[i]["horse"]["tack"]["bridle"] != null)
                    bridle = (int)jdata["leaser"].AsArray()[i]["horse"]["tack"]["bridle"];

                Leaser leaser = new Leaser(breedId, saddle, saddlePad, bridle);
                leaser.LeaseId = (int)jdata["leaser"].AsArray()[i]["lease_id"];
                leaser.ButtonId = (string)jdata["leaser"].AsArray()[i]["button_id"];
                leaser.Info = (string)jdata["leaser"].AsArray()[i]["info"];
                leaser.OnLeaseText = (string)jdata["leaser"].AsArray()[i]["on_lease"];
                leaser.Price = (int)jdata["leaser"].AsArray()[i]["price"];
                leaser.Minutes = (int)jdata["leaser"].AsArray()[i]["minutes"];

                leaser.Color = (string)jdata["leaser"].AsArray()[i]["horse"]["color"];
                leaser.Gender = (string)jdata["leaser"].AsArray()[i]["horse"]["gender"];
                leaser.Height = (int)jdata["leaser"].AsArray()[i]["horse"]["hands"];
                leaser.Experience = (int)jdata["leaser"].AsArray()[i]["horse"]["exp"];
                leaser.HorseName = (string)jdata["leaser"].AsArray()[i]["horse"]["name"];

                leaser.Health = (int)jdata["leaser"].AsArray()[i]["horse"]["basic_stats"]["health"];
                leaser.Hunger = (int)jdata["leaser"].AsArray()[i]["horse"]["basic_stats"]["hunger"];
                leaser.Thirst = (int)jdata["leaser"].AsArray()[i]["horse"]["basic_stats"]["thirst"];
                leaser.Mood = (int)jdata["leaser"].AsArray()[i]["horse"]["basic_stats"]["mood"];
                leaser.Tiredness = (int)jdata["leaser"].AsArray()[i]["horse"]["basic_stats"]["energy"];
                leaser.Groom = (int)jdata["leaser"].AsArray()[i]["horse"]["basic_stats"]["groom"];
                leaser.Shoes = (int)jdata["leaser"].AsArray()[i]["horse"]["basic_stats"]["shoes"];

                leaser.Speed = (int)jdata["leaser"].AsArray()[i]["horse"]["advanced_stats"]["speed"];
                leaser.Strength = (int)jdata["leaser"].AsArray()[i]["horse"]["advanced_stats"]["strength"];
                leaser.Conformation = (int)jdata["leaser"].AsArray()[i]["horse"]["advanced_stats"]["conformation"];
                leaser.Agility = (int)jdata["leaser"].AsArray()[i]["horse"]["advanced_stats"]["agility"];
                leaser.Endurance = (int)jdata["leaser"].AsArray()[i]["horse"]["advanced_stats"]["endurance"];
                leaser.Inteligence = (int)jdata["leaser"].AsArray()[i]["horse"]["advanced_stats"]["inteligence"];
                leaser.Personality = (int)jdata["leaser"].AsArray()[i]["horse"]["advanced_stats"]["personality"];

                Leaser.AddHorseLeaser(leaser);
                Logger.DebugPrint("Registered Leaser: " + leaser.LeaseId.ToString() + " For a " + leaser.HorseName);
            }
        }

        private static void registerSocials()
        {
            int totalSocials = jdata["social_types"].AsArray().Count;
            for (int i = 0; i < totalSocials; i++)
            {
                string socialType = (string)jdata["social_types"].AsArray()[i]["type"];
                int totalSocialsOfType = jdata["social_types"].AsArray()[i]["socials"].AsArray().Count;
                for (int ii = 0; ii < totalSocialsOfType; ii++)
                {
                    SocialType.Social social = new SocialType.Social();

                    social.Id = (int)jdata["social_types"].AsArray()[i]["socials"].AsArray()[ii]["social_id"];
                    social.ButtonName = (string)jdata["social_types"].AsArray()[i]["socials"].AsArray()[ii]["button_name"];
                    social.ForSender = (string)jdata["social_types"].AsArray()[i]["socials"].AsArray()[ii]["for_sender"];
                    social.ForTarget = (string)jdata["social_types"].AsArray()[i]["socials"].AsArray()[ii]["for_target"];
                    social.ForEveryone = (string)jdata["social_types"].AsArray()[i]["socials"].AsArray()[ii]["for_everyone"];
                    social.SoundEffect = (string)jdata["social_types"].AsArray()[i]["socials"].AsArray()[ii]["sound_effect"];

                    SocialType.AddNewSocial(socialType, social);
                    Logger.DebugPrint("Registered Social: " + social.ButtonName);
                }
            }
        }

        private static void registerRealTimeRiddleEvents()
        {
            int totalRealTimeRiddles = jdata["events"]["real_time_riddle"].AsArray().Count;
            for (int i = 0; i < totalRealTimeRiddles; i++)
            {
                int id = (int)jdata["events"]["real_time_riddle"].AsArray()[i]["id"];
                string riddleText = (string)jdata["events"]["real_time_riddle"].AsArray()[i]["text"];
                string[] riddleAnswers = jdata["events"]["real_time_riddle"].AsArray()[i]["answers"].AsArray<string>();
                int reward = (int)jdata["events"]["real_time_riddle"].AsArray()[i]["money_reward"];

                RealTimeRiddle riddle = new RealTimeRiddle(id, riddleText, riddleAnswers, reward);

                Logger.DebugPrint("Registered Riddle #" + riddle.RiddleId.ToString());
            }
        }

        private static void registerRealTimeQuizEvents()
        {
            int totalRealTimeQuizCategories = jdata["events"]["real_time_quiz"].AsArray().Count;
            RealTimeQuiz.Categories = new RealTimeQuiz.QuizCategory[totalRealTimeQuizCategories]; // initalize array
            for (int i = 0; i < totalRealTimeQuizCategories; i++)
            {
                string name = (string)jdata["events"]["real_time_quiz"].AsArray()[i]["name"];
                int totalQuestions = jdata["events"]["real_time_quiz"].AsArray()[i]["questons"].AsArray().Count;

                RealTimeQuiz.QuizCategory quizCategory = new RealTimeQuiz.QuizCategory();
                quizCategory.Name = name;
                quizCategory.Questions = new RealTimeQuiz.QuizQuestion[totalQuestions];

                for (int ii = 0; ii < totalQuestions; ii++)
                {
                    quizCategory.Questions[ii] = new RealTimeQuiz.QuizQuestion(quizCategory);
                    quizCategory.Questions[ii].Question = (string)jdata["events"]["real_time_quiz"].AsArray()[i]["questons"].AsArray()[ii]["question"];
                    quizCategory.Questions[ii].Answers = jdata["events"]["real_time_quiz"].AsArray()[i]["questons"].AsArray()[ii]["answers"].AsArray<string>();
                    Logger.DebugPrint("Registered Real Time Quiz Question: " + quizCategory.Questions[ii].Question);
                }

                RealTimeQuiz.Categories[i] = quizCategory;

                Logger.DebugPrint("Registered Real Time Quiz Category: " + name);
            }
        }
        private static void registerRandomEvents()
        {
            int totalRandomEvent = jdata["events"]["random_events"].AsArray().Count;
            for (int i = 0; i < totalRandomEvent; i++)
            {
                int minmoney = 0;
                int maxmoney = 0;
                int lowerHorseHealth = 0;
                int giveObj = 0;

                int id = (int)jdata["events"]["random_events"].AsArray()[i]["id"];
                string txt = (string)jdata["events"]["random_events"].AsArray()[i]["text"];

                if (jdata["events"]["random_events"].AsArray()[i]["min_money"] != null)
                    minmoney = (int)jdata["events"]["random_events"].AsArray()[i]["min_money"];
                if (jdata["events"]["random_events"].AsArray()[i]["max_money"] != null)
                    maxmoney = (int)jdata["events"]["random_events"].AsArray()[i]["max_money"];
                if (jdata["events"]["random_events"].AsArray()[i]["lower_horse_health"] != null)
                    lowerHorseHealth = (int)jdata["events"]["random_events"].AsArray()[i]["lower_horse_health"];
                if (jdata["events"]["random_events"].AsArray()[i]["give_object"] != null)
                    giveObj = (int)jdata["events"]["random_events"].AsArray()[i]["give_object"];

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

            HorseInfo.HorseNames = jdata["horses"]["names"].AsArray<string>(); 

            Item.Present = (int)jdata["item"]["special"]["present"];
            Item.MailMessage = (int)jdata["item"]["special"]["mail_message"];
            Item.DorothyShoes = (int)jdata["item"]["special"]["dorothy_shoes"];
            Item.PawneerOrder = (int)jdata["item"]["special"]["pawneer_order"];
            Item.Telescope = (int)jdata["item"]["special"]["telescope"];
            Item.Pitchfork = (int)jdata["item"]["special"]["pitchfork"];
            Item.WishingCoin = (int)jdata["item"]["special"]["wishing_coin"];
            Item.FishingPole = (int)jdata["item"]["special"]["fishing_poll"];
            Item.Earthworm = (int)jdata["item"]["special"]["earthworm"];
            Item.BirthdayToken = (int)jdata["item"]["special"]["birthday_token"];
            Item.WaterBalloon = (int)jdata["item"]["special"]["water_balloon"];
            Item.ModSplatterball = (int)jdata["item"]["special"]["mod_splatterball"];
            Item.MagicBean = (int)jdata["item"]["special"]["magic_bean"];
            Item.MagicDroplet = (int)jdata["item"]["special"]["magic_droplet"];
            Item.Ruby = (int)jdata["item"]["special"]["ruby"];

            Item.StallionTradingCard = (int)jdata["item"]["special"]["stallion_trading_card"];
            Item.MareTradingCard = (int)jdata["item"]["special"]["mare_trading_card"];
            Item.ColtTradingCard = (int)jdata["item"]["special"]["colt_trading_card"];
            Item.FillyTradingCard = (int)jdata["item"]["special"]["filly_trading_card"];

            GameServer.IdleWarning = (int)jdata["messages"]["disconnect"]["client_timeout"]["warn_after"];
            GameServer.IdleTimeout = (int)jdata["messages"]["disconnect"]["client_timeout"]["kick_after"];

            ChatMsg.PrivateMessageSound = (string)jdata["messages"]["chat"]["pm_sound"];

            // HISP Specific ...
            Messages.HISPHelpCommandUsageFormat = (string)jdata["hisp_specific"]["HISP_help_command_usage_format"];
            
            // New Users

            Messages.NewUserMessage = (string)jdata["messages"]["new_user"]["starting_message"];
            Map.NewUserStartX = (int)jdata["messages"]["new_user"]["starting_x"];
            Map.NewUserStartY = (int)jdata["messages"]["new_user"]["starting_y"];

            // Timed Messages

            Messages.PlaytimeMessageFormat = (string)jdata["messages"]["timed_messages"]["playtime_message"];
            Messages.RngMessages = jdata["messages"]["timed_messages"]["rng_message"].AsArray<string>();

            // Auto Sell
            Messages.AutoSellNotStandingInSamePlace = (string)jdata["messages"]["meta"]["auto_sell"]["not_standing_sameplace"];
            Messages.AutoSellSuccessFormat = (string)jdata["messages"]["meta"]["auto_sell"]["success"];
            Messages.AutoSellInsufficentFunds = (string)jdata["messages"]["meta"]["auto_sell"]["insufficent_money"];
            Messages.AutoSellTooManyHorses = (string)jdata["messages"]["meta"]["auto_sell"]["toomany_horses"];
            Messages.AutoSellYouSoldHorseFormat = (string)jdata["messages"]["meta"]["auto_sell"]["you_sold"];
            Messages.AutoSellYouSoldHorseOfflineFormat = (string)jdata["messages"]["meta"]["auto_sell"]["sold_offline"];

            // Mute Command
            Messages.NowMutingPlayerFormat = (string)jdata["messages"]["meta"]["mute_command"]["now_ignoring_player"];
            Messages.StoppedMutingPlayerFormat = (string)jdata["messages"]["meta"]["mute_command"]["stop_ignoring_player"];

            Messages.PlayerIgnoringYourPrivateMessagesFormat = (string)jdata["messages"]["meta"]["mute_command"]["player_ignoring_your_pm"];
            Messages.PlayerIgnoringYourBuddyRequests = (string)jdata["messages"]["meta"]["mute_command"]["player_ignoring_your_br"];
            Messages.PlayerIgnoringYourSocials = (string)jdata["messages"]["meta"]["mute_command"]["player_ignoring_your_socials"];

            Messages.PlayerIgnoringAllPrivateMessagesFormat = (string)jdata["messages"]["meta"]["mute_command"]["player_ignoring_all_pm"];
            Messages.PlayerIgnoringAllBuddyRequests = (string)jdata["messages"]["meta"]["mute_command"]["player_ignoring_all_br"];
            Messages.PlayerIgnoringAllSocials = (string)jdata["messages"]["meta"]["mute_command"]["player_ignoring_all_socials"];

            Messages.CantSendInMutedChannel = (string)jdata["messages"]["meta"]["mute_command"]["cant_send_in_muted_channel"];
            Messages.CantSendBuddyRequestWhileMuted = (string)jdata["messages"]["meta"]["mute_command"]["cant_send_br_muted"];
            Messages.CantSendPrivateMessageWhileMuted = (string)jdata["messages"]["meta"]["mute_command"]["cant_send_pm_muted"];

            Messages.CantSendPrivateMessagePlayerMutedFormat = (string)jdata["messages"]["meta"]["mute_command"]["cant_send_pm_player_muted"];

            // Chat Errors
            Messages.CantFindPlayerToPrivateMessage = (string)jdata["messages"]["chat_errors"]["cant_find_player"];
            Messages.AdsOnlyOncePerMinute = (string)jdata["messages"]["chat_errors"]["ads_once_per_minute"];
            Messages.GlobalChatLimited = (string)jdata["messages"]["chat_errors"]["global_chats_limited"];
            Messages.GlobalChatTooLong = (string)jdata["messages"]["chat_errors"]["global_too_long"];
            Messages.AdsChatTooLong = (string)jdata["messages"]["chat_errors"]["ads_too_long"];

            // Warp Command

            Messages.SuccessfullyWarpedToPlayer = (string)jdata["messages"]["commands"]["warp"]["player"];
            Messages.SuccessfullyWarpedToLocation = (string)jdata["messages"]["commands"]["warp"]["location"];
            Messages.OnlyUnicornCanWarp = (string)jdata["messages"]["commands"]["warp"]["only_unicorn"];
            Messages.FailedToUnderstandLocation = (string)jdata["messages"]["commands"]["warp"]["location_unknown"];

            // Mod Isle
            Messages.ModSplatterballEarnedYouFormat = (string)jdata["messages"]["mods_revenge"]["awarded_you"];
            Messages.ModSplatterballEarnedOtherFormat = (string)jdata["messages"]["mods_revenge"]["awareded_others"];
            Messages.ModIsleMessage = (string)jdata["messages"]["commands"]["mod_isle"]["message"];
            Map.ModIsleX = (int)jdata["messages"]["commands"]["mod_isle"]["x"];
            Map.ModIsleY = (int)jdata["messages"]["commands"]["mod_isle"]["y"];

            // Rules Isle
            Map.RulesIsleX = (int)jdata["messages"]["commands"]["rules_isle"]["x"];
            Map.RulesIsleY = (int)jdata["messages"]["commands"]["rules_isle"]["y"];
            Messages.RulesIsleSentMessage = (string)jdata["messages"]["commands"]["rules_isle"]["message"];
            Messages.RulesIsleCommandMessageFormat = (string)jdata["messages"]["commands"]["rules_isle"]["command_msg"];

            // Prison Isle
            Map.PrisonIsleX = (int)jdata["messages"]["commands"]["prison_isle"]["x"];
            Map.PrisonIsleY = (int)jdata["messages"]["commands"]["prison_isle"]["y"];
            Messages.PrisonIsleSentMessage = (string)jdata["messages"]["commands"]["prison_isle"]["message"];
            Messages.PrisonIsleCommandMessageFormat = (string)jdata["messages"]["commands"]["prison_isle"]["command_msg"];


            // Tag

            Messages.TagYourItFormat = (string)jdata["messages"]["meta"]["player_interaction"]["tag.tag_player"];
            Messages.TagOtherBuddiesOnlineFormat = (string)jdata["messages"]["meta"]["player_interaction"]["tag.total_buddies"];

            // Add Buddy

            Messages.AddBuddyPending = (string)jdata["messages"]["meta"]["player_interaction"]["add_buddy.add_pending"];
            Messages.AddBuddyOtherPendingFormat = (string)jdata["messages"]["meta"]["player_interaction"]["add_buddy.other_pending"];
            Messages.AddBuddyYourNowBuddiesFormat = (string)jdata["messages"]["meta"]["player_interaction"]["add_buddy.add_confirmed"];
            Messages.AddBuddyDeleteBuddyFormat = (string)jdata["messages"]["meta"]["player_interaction"]["add_buddy.deleted"];

            // Socials

            Messages.SocialButton = (string)jdata["messages"]["meta"]["player_interaction"]["socials"]["socials_button"];
            Messages.SocialMessageFormat = (string)jdata["messages"]["meta"]["player_interaction"]["socials"]["socials_message"];
            Messages.SocialTypeFormat = (string)jdata["messages"]["meta"]["player_interaction"]["socials"]["socials_menu_type"];
            Messages.SocialPlayerNoLongerNearby = (string)jdata["messages"]["meta"]["player_interaction"]["socials"]["no_longer_nearby"];

            // Message Queue 
            Messages.MessageQueueHeader = (string)jdata["messages"]["message_queue"];

            // Random Event
            Messages.RandomEventPrefix = (string)jdata["messages"]["random_event_prefix"];

            // Events : Mods Revenge
            Messages.EventStartModsRevenge = (string)jdata["messages"]["events"]["mods_revenge"]["event_start"];
            Messages.EventEndModsRevenge = (string)jdata["messages"]["events"]["mods_revenge"]["event_end"];

            // Events : Isle Trading Game
            Messages.EventStartIsleTradingGame = (string)jdata["messages"]["events"]["isle_card_trading_game"]["event_start"];
            Messages.EventDisqualifiedIsleTradingGame = (string)jdata["messages"]["events"]["isle_card_trading_game"]["event_disqualified"];
            Messages.EventOnlyOneTypeIsleTradingGame = (string)jdata["messages"]["events"]["isle_card_trading_game"]["event_one_type"];
            Messages.EventOnlyTwoTypeIsleTradingGame = (string)jdata["messages"]["events"]["isle_card_trading_game"]["event_two_type"];
            Messages.EventOnlyThreeTypeIsleTradingGame = (string)jdata["messages"]["events"]["isle_card_trading_game"]["event_three_type"];
            Messages.EventNoneIsleTradingGame = (string)jdata["messages"]["events"]["isle_card_trading_game"]["event_no_cards"];
            Messages.EventWonIsleTradingGame = (string)jdata["messages"]["events"]["isle_card_trading_game"]["event_win"];

            // Events : Water Ballon Game
            Messages.EventStartWaterBallonGame = (string)jdata["messages"]["events"]["water_balloon_game"]["event_start"];
            Messages.EventWonWaterBallonGame = (string)jdata["messages"]["events"]["water_balloon_game"]["event_won"];
            Messages.EventEndWaterBalloonGame = (string)jdata["messages"]["events"]["water_balloon_game"]["event_end"];
            Messages.EventWinnerWaterBalloonGameFormat = (string)jdata["messages"]["events"]["water_balloon_game"]["event_winner"];

            // Events : Real Time Quiz

            Messages.EventMetaRealTimeQuizFormat = (string)jdata["messages"]["events"]["real_time_quiz"]["event_meta"];
            Messages.EventStartRealTimeQuiz = (string)jdata["messages"]["events"]["real_time_quiz"]["event_start"];
            Messages.EventEndRealTimeQuiz = (string)jdata["messages"]["events"]["real_time_quiz"]["event_end"];
            Messages.EventBonusRealTimeQuizFormat = (string)jdata["messages"]["events"]["real_time_quiz"]["event_bonus"];
            Messages.EventWinBonusRealTimeQuizFormat = (string)jdata["messages"]["events"]["real_time_quiz"]["event_win_bonus"];
            Messages.EventWinRealTimeQuizFormat = (string)jdata["messages"]["events"]["real_time_quiz"]["event_win"];
            Messages.EventUnavailableRealTimeQuiz = (string)jdata["messages"]["events"]["real_time_quiz"]["event_unavailable"];
            Messages.EventEnteredRealTimeQuiz = (string)jdata["messages"]["events"]["real_time_quiz"]["event_entered"];
            Messages.EventAlreadyEnteredRealTimeQuiz = (string)jdata["messages"]["events"]["real_time_quiz"]["event_entered_already"];
            Messages.EventQuitRealTimeQuiz = (string)jdata["messages"]["events"]["real_time_quiz"]["event_quit"];

            // Events : Real Time Riddle

            Messages.EventStartRealTimeRiddleFormat = (string)jdata["messages"]["events"]["real_time_riddle"]["event_start"];
            Messages.EventEndRealTimeRiddle = (string)jdata["messages"]["events"]["real_time_riddle"]["event_end"];
            Messages.EventWonRealTimeRiddleForOthersFormat = (string)jdata["messages"]["events"]["real_time_riddle"]["event_won_others"];
            Messages.EventWonRealTimeRiddleForYouFormat = (string)jdata["messages"]["events"]["real_time_riddle"]["event_won_you"];
            Messages.EventAlreadySovledRealTimeRiddle = (string)jdata["messages"]["events"]["real_time_riddle"]["event_solved_already"];

            // Events : Tack Shop Giveaway

            Messages.EventStartTackShopGiveawayFormat = (string)jdata["messages"]["events"]["tack_shop_giveaway"]["event_start"];
            Messages.Event1MinTackShopGiveawayFormat = (string)jdata["messages"]["events"]["tack_shop_giveaway"]["event_1min"];
            Messages.EventWonTackShopGiveawayFormat = (string)jdata["messages"]["events"]["tack_shop_giveaway"]["event_won"];
            Messages.EventEndTackShopGiveawayFormat = (string)jdata["messages"]["events"]["tack_shop_giveaway"]["event_end"];


            // MultiHorses
            Messages.OtherPlayersHere = (string)jdata["messages"]["meta"]["multihorses"]["other_players_here"];
            Messages.MultiHorseSelectOneToJoinWith = (string)jdata["messages"]["meta"]["multihorses"]["select_a_horse"];
            Messages.MultiHorseFormat = (string)jdata["messages"]["meta"]["multihorses"]["horse_format"];

            // 2Player
            Messages.TwoPlayerOtherPlayer = (string)jdata["messages"]["meta"]["two_player"]["other_player"];
            Messages.TwoPlayerPlayerFormat = (string)jdata["messages"]["meta"]["two_player"]["player_name"];
            Messages.TwoPlayerInviteButton = (string)jdata["messages"]["meta"]["two_player"]["invite_button"];
            Messages.TwoPlayerAcceptButton = (string)jdata["messages"]["meta"]["two_player"]["accept_button"];
            Messages.TwoPlayerSentInvite = (string)jdata["messages"]["meta"]["two_player"]["sent_invite"];
            Messages.TwoPlayerPlayingWithFormat = (string)jdata["messages"]["meta"]["two_player"]["playing_with"];

            Messages.TwoPlayerGameInProgressFormat = (string)jdata["messages"]["meta"]["two_player"]["game_in_progress"];

            Messages.TwoPlayerYourInvitedFormat = (string)jdata["messages"]["meta"]["two_player"]["your_invited"];
            Messages.TwoPlayerInvitedFormat = (string)jdata["messages"]["meta"]["two_player"]["you_invited"];
            Messages.TwoPlayerStartingUpGameFormat = (string)jdata["messages"]["meta"]["two_player"]["starting_game"];

            Messages.TwoPlayerGameClosed = (string)jdata["messages"]["meta"]["two_player"]["game_closed"];
            Messages.TwoPlayerGameClosedOther = (string)jdata["messages"]["meta"]["two_player"]["game_closed_other"];

            Messages.TwoPlayerRecordedWinFormat = (string)jdata["messages"]["meta"]["two_player"]["recorded_win"];
            Messages.TwoPlayerRecordedLossFormat = (string)jdata["messages"]["meta"]["two_player"]["recorded_loss"];

            // Trade

            Messages.TradeWithPlayerFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trading_with"];

            Messages.TradeWaitingForOtherDone = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_wait_for_done"];
            Messages.TradeOtherPlayerIsDone = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["other_player_is_done"];
            Messages.TradeFinalReview = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["final_review"];

            Messages.TradeYourOfferingFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["you_offering"];

            Messages.TradeAddItems = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["add_items"];
            Messages.TradeOtherOfferingFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["other_offering"];

            Messages.TradeWhenDoneClick = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["when_done_click"];
            Messages.TradeCancelAnytime = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["cancel_anytime"];
            Messages.TradeAcceptTrade = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["accept_trade"];

            Messages.TradeOfferingNothing = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offering_nothing"];
            Messages.TradeOfferingMoneyFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offering_money"];
            Messages.TradeOfferingItemFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offering_item"];
            Messages.TradeOfferingHorseFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offering_horse"];

            // Trading : What to offer

            Messages.TradeWhatToOfferFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["what_to_offer"];
            Messages.TradeOfferMoney = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offer_money"];

            Messages.TradeOfferHorse = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offer_horse"];
            Messages.TradeOfferHorseFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offer_horse_format"];
            Messages.TradeOfferHorseTacked = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["horse_tacked"];

            Messages.TradeOfferItem = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offer_object"];
            Messages.TradeOfferItemFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offer_object_format"];
            Messages.TradeOfferItemOtherPlayerInvFull = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["offer_object_inv_full"];

            // Trading : Offer Submenu

            Messages.TradeMoneyOfferSubmenuFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["money_offer_submenu"];
            Messages.TradeItemOfferSubmenuFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["object_offer_submenu"];

            // Trading : Messges

            Messages.TradeWaitingForOthersToAcceptMessage = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["waiting_for_other_to_accept"];
            Messages.TradeRequiresBothPlayersMessage = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["requires_both_players"];

            Messages.TradeItemOfferAtleast1 = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["object_offer_atleast_1"];
            Messages.TradeItemOfferTooMuchFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["object_offer_too_much"];
            Messages.TradeMoneyOfferTooMuch = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["money_offer_too_much"];

            Messages.TradeOtherPlayerHasNegativeMoney = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["other_player_has_negative_money"];
            Messages.TradeYouHaveNegativeMoney = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["you_have_negative_money"];


            Messages.TradeAcceptedMessage = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_accepted"];
            Messages.TradeCanceledByYouMessage = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["you_canceled"];
            Messages.TradeCanceledByOtherPlayerFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["other_canceled"];
            Messages.TradeCanceledBecuasePlayerMovedMessage = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_canceled_moved"];
            Messages.TradeCanceledInterupted = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_interupted"];

            Messages.TradeRiddenHorse = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_ridden_horse"];

            Messages.TradeYouCantHandleMoreHorses = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["cant_handle_more_horses"];
            Messages.TradeOtherPlayerCantHandleMoreHorsesFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["other_player_cant_handle_more_horses"];

            Messages.TradeOtherCantCarryMoreItems = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["other_carry_more"];
            Messages.TradeYouCantCarryMoreItems = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["you_cant_carry_more"];

            Messages.TradeYouSpentMoneyMessageFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_spent"];
            Messages.TradeYouReceivedMoneyMessageFormat = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_received"];

            Messages.TradeNotAllowedWhileBidding = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_not_allowed_while_bidding"];
            Messages.TradeNotAllowedWhileOtherBidding = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_not_allowed_while_other_is_bidding"];

            Messages.TradeWillGiveYouTooMuchMoney = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_other_cannot_carry_that_much"];
            Messages.TradeWillGiveOtherTooMuchMoney = (string)jdata["messages"]["meta"]["player_interaction"]["trade"]["trade_you_cannot_carry_that_much"];

            // Player Interation

            Messages.PlayerHereMenuFormat = (string)jdata["messages"]["meta"]["player_interaction"]["menu"];

            Messages.PlayerHereProfileButton = (string)jdata["messages"]["meta"]["player_interaction"]["profiile_button"];
            Messages.PlayerHereSocialButton = (string)jdata["messages"]["meta"]["player_interaction"]["social_button"];
            Messages.PlayerHereTradeButton = (string)jdata["messages"]["meta"]["player_interaction"]["trade_button"];
            Messages.PlayerHereAddBuddyButton = (string)jdata["messages"]["meta"]["player_interaction"]["buddy_button"];
            Messages.PlayerHereTagButton = (string)jdata["messages"]["meta"]["player_interaction"]["tag_button"];
            Messages.PmButton = (string)jdata["messages"]["meta"]["player_interaction"]["pm_button"];


            // Auction
            Messages.AuctionsRunning = (string)jdata["messages"]["meta"]["auction"]["auctions_running"];
            Messages.AuctionPlayersHereFormat = (string)jdata["messages"]["meta"]["auction"]["players_here"];
            Messages.AuctionHorseEntryFormat = (string)jdata["messages"]["meta"]["auction"]["auction_horse_entry"];
            Messages.AuctionAHorse = (string)jdata["messages"]["meta"]["auction"]["auction_horse"];

            Messages.AuctionListHorse = (string)jdata["messages"]["meta"]["auction"]["list_horse"];
            Messages.AuctionHorseListEntryFormat = (string)jdata["messages"]["meta"]["auction"]["horse_list_entry"];
            Messages.AuctionHorseViewButton = (string)jdata["messages"]["meta"]["auction"]["view_button"];
            Messages.AuctionHorseIsTacked = (string)jdata["messages"]["meta"]["auction"]["tacked"];

            Messages.AuctionBidMax = (string)jdata["messages"]["meta"]["auction"]["max_bid"];
            Messages.AuctionBidRaisedFormat = (string)jdata["messages"]["meta"]["auction"]["bid_raised"];
            Messages.AuctionTopBid = (string)jdata["messages"]["meta"]["auction"]["top_bid"];
            Messages.AuctionExistingBidHigher = (string)jdata["messages"]["meta"]["auction"]["existing_higher"];

            Messages.AuctionYouHaveTooManyHorses = (string)jdata["messages"]["meta"]["auction"]["you_have_too_many_horses"];
            Messages.AuctionOnlyOneWinningBidAllowed = (string)jdata["messages"]["meta"]["auction"]["only_one_winning_bid_allowed"];

            Messages.AuctionOneHorsePerPlayer = (string)jdata["messages"]["meta"]["auction"]["one_horse_at_a_time"];
            Messages.AuctionYouveBeenOutbidFormat = (string)jdata["messages"]["meta"]["auction"]["outbid_by"];
            Messages.AuctionCantAffordBid = (string)jdata["messages"]["meta"]["auction"]["cant_afford_bid"];
            Messages.AuctionCantAffordAuctionFee = (string)jdata["messages"]["meta"]["auction"]["cant_afford_listing"];
            Messages.AuctionNoOtherTransactionAllowed = (string)jdata["messages"]["meta"]["auction"]["no_other_transaction_allowed"];

            Messages.AuctionYouBroughtAHorseFormat = (string)jdata["messages"]["meta"]["auction"]["brought_horse"];
            Messages.AuctionNoHorseBrought = (string)jdata["messages"]["meta"]["auction"]["no_one_brought"];
            Messages.AuctionHorseSoldFormat = (string)jdata["messages"]["meta"]["auction"]["horse_sold"];

            Messages.AuctionSoldToFormat = (string)jdata["messages"]["meta"]["auction"]["sold_to"];
            Messages.AuctionNotSold = (string)jdata["messages"]["meta"]["auction"]["not_sold"];
            Messages.AuctionGoingToFormat = (string)jdata["messages"]["meta"]["auction"]["going_to"];

            // Hammock Text
            Messages.HammockText = (string)jdata["messages"]["meta"]["hammock"];

            // Horse Leaser
            Messages.HorseLeaserCantAffordMessage = (string)jdata["messages"]["horse_leaser"]["cant_afford"];
            Messages.HorseLeaserTemporaryHorseAdded = (string)jdata["messages"]["horse_leaser"]["temporary_horse_added"];
            Messages.HorseLeaserHorsesFull = (string)jdata["messages"]["horse_leaser"]["horses_full"];

            Messages.HorseLeaserReturnedToUniterPegasus = (string)jdata["messages"]["horse_leaser"]["returned_to_uniter_pegasus"];

            Messages.HorseLeaserReturnedToUniterFormat = (string)jdata["messages"]["horse_leaser"]["returned_to_uniter"];
            Messages.HorseLeaserReturnedToOwnerFormat = (string)jdata["messages"]["horse_leaser"]["returned_to_owner"];

            // Competitions
            Messages.ArenaResultsMessage = (string)jdata["messages"]["meta"]["arena"]["results"];
            Messages.ArenaPlacingFormat = (string)jdata["messages"]["meta"]["arena"]["placing"];
            Messages.ArenaAlreadyEntered = (string)jdata["messages"]["meta"]["arena"]["already_entered"];

            Messages.ArenaFirstPlace = (string)jdata["messages"]["meta"]["arena"]["first_place"];
            Messages.ArenaSecondPlace = (string)jdata["messages"]["meta"]["arena"]["second_place"];
            Messages.ArenaThirdPlace = (string)jdata["messages"]["meta"]["arena"]["third_place"];
            Messages.ArenaFourthPlace = (string)jdata["messages"]["meta"]["arena"]["fourth_place"];
            Messages.ArenaFifthPlace = (string)jdata["messages"]["meta"]["arena"]["fifth_place"];
            Messages.ArenaSixthPlace = (string)jdata["messages"]["meta"]["arena"]["sixth_place"];

            Messages.ArenaEnteredInto = (string)jdata["messages"]["meta"]["arena"]["enter_into"];
            Messages.ArenaCantAfford = (string)jdata["messages"]["meta"]["arena"]["cant_afford"];

            Messages.ArenaYourScoreFormat = (string)jdata["messages"]["meta"]["arena"]["your_score"];

            Messages.ArenaJumpingStartup = (string)jdata["messages"]["meta"]["arena"]["jumping_start_up"];
            Messages.ArenaDraftStartup = (string)jdata["messages"]["meta"]["arena"]["draft_start_up"];
            Messages.ArenaRacingStartup = (string)jdata["messages"]["meta"]["arena"]["racing_start_up"];
            Messages.ArenaConformationStartup = (string)jdata["messages"]["meta"]["arena"]["conformation_start_up"];

            Messages.ArenaYouWinFormat = (string)jdata["messages"]["meta"]["arena"]["winner"];
            Messages.ArenaOnlyWinnerWins = (string)jdata["messages"]["meta"]["arena"]["only_winner_wins"];

            Messages.ArenaTooHungry = (string)jdata["messages"]["meta"]["arena"]["too_hungry"];
            Messages.ArenaTooThirsty = (string)jdata["messages"]["meta"]["arena"]["too_thisty"];
            Messages.ArenaNeedsFarrier = (string)jdata["messages"]["meta"]["arena"]["farrier"];
            Messages.ArenaTooTired = (string)jdata["messages"]["meta"]["arena"]["too_tired"];
            Messages.ArenaNeedsVet = (string)jdata["messages"]["meta"]["arena"]["needs_vet"];

            Messages.ArenaEventNameFormat = (string)jdata["messages"]["meta"]["arena"]["event_name"];
            Messages.ArenaCurrentlyTakingEntriesFormat = (string)jdata["messages"]["meta"]["arena"]["currently_taking_entries"];
            Messages.ArenaCompetitionInProgress = (string)jdata["messages"]["meta"]["arena"]["competition_in_progress"];
            Messages.ArenaYouHaveHorseEntered = (string)jdata["messages"]["meta"]["arena"]["horse_entered"];
            Messages.ArenaCompetitionFull = (string)jdata["messages"]["meta"]["arena"]["competiton_full"];

            Messages.ArenaFullErrorMessage = (string)jdata["messages"]["meta"]["arena"]["arena_join_fail_full"];

            Messages.ArenaEnterHorseFormat = (string)jdata["messages"]["meta"]["arena"]["enter_horse"];
            Messages.ArenaCurrentCompetitors = (string)jdata["messages"]["meta"]["arena"]["current_competitors"];
            Messages.ArenaCompetingHorseFormat = (string)jdata["messages"]["meta"]["arena"]["competing_horses"];

            // Horse Games
            Messages.HorseGamesSelectHorse = (string)jdata["messages"]["meta"]["horse_games"]["select_a_horse"];
            Messages.HorseGamesHorseEntryFormat = (string)jdata["messages"]["meta"]["horse_games"]["horse_entry"];

            // City Hall
            Messages.CityHallMenu = (string)jdata["messages"]["meta"]["city_hall"]["menu"];
            Messages.CityHallMailSendMeta = (string)jdata["messages"]["meta"]["city_hall"]["mail_send_meta"];

            Messages.CityHallSentMessageFormat = (string)jdata["messages"]["meta"]["city_hall"]["sent_mail"];
            Messages.CityHallCantAffordPostageMessage = (string)jdata["messages"]["meta"]["city_hall"]["cant_afford_postage"];
            Messages.CityHallCantFindPlayerMessageFormat = (string)jdata["messages"]["meta"]["city_hall"]["cant_find_player"];

            Messages.CityHallCheapestAutoSells = (string)jdata["messages"]["meta"]["city_hall"]["auto_sell"]["top_100_cheapest"];
            Messages.CityHallCheapestAutoSellHorseEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["auto_sell"]["cheap_horse_entry"];

            Messages.CityHallMostExpAutoSells = (string)jdata["messages"]["meta"]["city_hall"]["auto_sell"]["top_50_most_exp"];
            Messages.CityHallMostExpAutoSellHorseEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["auto_sell"]["exp_horse_entry"];

            Messages.CityHallTop25Ranches = (string)jdata["messages"]["meta"]["city_hall"]["ranch_investment"]["top_25"];
            Messages.CityHallRanchEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["ranch_investment"]["ranch_entry"];

            Messages.CityHallTop25Players = (string)jdata["messages"]["meta"]["city_hall"]["richest_players"]["top_25"];
            Messages.CityHallRichPlayerFormat = (string)jdata["messages"]["meta"]["city_hall"]["richest_players"]["rich_player_format"];

            Messages.CityHallTop100SpoiledHorses = (string)jdata["messages"]["meta"]["city_hall"]["spoiled_horses"]["top_100"];
            Messages.CityHallSpoiledHorseEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["spoiled_horses"]["spoiled_horse_entry"];

            Messages.CityHallTop25AdventurousPlayers = (string)jdata["messages"]["meta"]["city_hall"]["most_adventurous_players"]["top_25"];
            Messages.CityHallAdventurousPlayerEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["most_adventurous_players"]["adventurous_player_entry"];

            Messages.CityHallTop25ExperiencedPlayers = (string)jdata["messages"]["meta"]["city_hall"]["most_experinced_players"]["top_25"];
            Messages.CityHallExperiencePlayerEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["most_experinced_players"]["experienced_player_entry"];

            Messages.CityHallTop25MinigamePlayers = (string)jdata["messages"]["meta"]["city_hall"]["most_active_minigame_players"]["top_25"];
            Messages.CityHallMinigamePlayerEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["most_active_minigame_players"]["minigame_player_entry"];

            Messages.CityHallTop25ExperiencedHorses = (string)jdata["messages"]["meta"]["city_hall"]["most_experienced_horses"]["top_25"];
            Messages.CityHallExperiencedHorseEntryFormat = (string)jdata["messages"]["meta"]["city_hall"]["most_experienced_horses"]["experienced_horse_entry"];

            // Mail Messages
            Messages.MailReceivedMessage = (string)jdata["messages"]["meta"]["mail"]["mail_received"];
            Messages.MailSelectFromFollowing = (string)jdata["messages"]["meta"]["mail"]["mail_select"];
            Messages.MailSe = (string)jdata["messages"]["meta"]["mail"]["mail_se"];

            Messages.MailReadMetaFormat = (string)jdata["messages"]["meta"]["mail"]["mail_read"];
            Messages.MailEntryFormat = (string)jdata["messages"]["meta"]["mail"]["mail_entry"];
            Messages.MailRippedMessage = (string)jdata["messages"]["meta"]["mail"]["mail_ripped"];

            // Click
            Messages.ClickPlayerHereFormat = (string)jdata["messages"]["player_here"];


            // Ranch
            Messages.RanchUnownedRanchFormat = (string)jdata["messages"]["meta"]["ranch"]["unowned_ranch"];
            Messages.RanchYouCouldPurchaseThisRanch = (string)jdata["messages"]["meta"]["ranch"]["you_could_purchase_this"];
            Messages.RanchYouAllreadyOwnARanch = (string)jdata["messages"]["meta"]["ranch"]["ranch_already_owned"];
            Messages.RanchSubscribersOnly = (string)jdata["messages"]["meta"]["ranch"]["sub_only"];
            Messages.RanchDescriptionOthersFormat = (string)jdata["messages"]["meta"]["ranch"]["ranch_desc_others"];
            Messages.RanchUnownedRanchClicked = (string)jdata["messages"]["meta"]["ranch"]["unowned_ranch_click"];
            Messages.RanchClickMessageFormat = (string)jdata["messages"]["meta"]["ranch"]["click_message"];

            Messages.RanchNoDorothyShoesMessage = (string)jdata["messages"]["meta"]["ranch"]["no_dorothy_shoes"];
            Messages.RanchDorothyShoesMessage = (string)jdata["messages"]["meta"]["ranch"]["dorothy_message"];
            Messages.RanchDorothyShoesPrisonIsleMessage = (string)jdata["messages"]["meta"]["ranch"]["dorothy_prison_isle"];
            Messages.RanchForcefullySoldFormat = (string)jdata["messages"]["meta"]["ranch"]["forcefully_sold"];

            Messages.RanchCantAffordRanch = (string)jdata["messages"]["meta"]["ranch"]["ranch_buy_cannot_afford"];
            Messages.RanchRanchBroughtMessageFormat = (string)jdata["messages"]["meta"]["ranch"]["ranch_brought"];

            Messages.RanchSavedRanchDescripton = (string)jdata["messages"]["meta"]["ranch"]["ranch_info"]["saved"];
            Messages.RanchSavedTitleTooLongError = (string)jdata["messages"]["meta"]["ranch"]["ranch_info"]["title_too_long"];
            Messages.RanchSavedDescrptionTooLongError = (string)jdata["messages"]["meta"]["ranch"]["ranch_info"]["description_too_long"];
            Messages.RanchSavedTitleViolationsError = (string)jdata["messages"]["meta"]["ranch"]["ranch_info"]["title_contains_violations"];
            Messages.RanchSavedDescrptionViolationsErrorFormat = (string)jdata["messages"]["meta"]["ranch"]["ranch_info"]["desc_contains_violations"];


            Messages.RanchDefaultRanchTitle = (string)jdata["messages"]["meta"]["ranch"]["default_title"];
            Messages.RanchEditDescriptionMetaFormat = (string)jdata["messages"]["meta"]["ranch"]["edit_description"];
            Messages.RanchTitleFormat = (string)jdata["messages"]["meta"]["ranch"]["your_ranch_meta"];
            Messages.RanchYourDescriptionFormat = (string)jdata["messages"]["meta"]["ranch"]["view_desc"];

            Messages.RanchSellAreYouSure = (string)jdata["messages"]["meta"]["ranch"]["sell_confirm"];
            Messages.RanchSoldFormat = (string)jdata["messages"]["meta"]["ranch"]["sell_done"];

            // Ranch : Breed

            Messages.RanchCanBuildOneOfTheFollowingInThisSpot = (string)jdata["messages"]["meta"]["ranch"]["build"]["build_on_this_spot"];
            Messages.RanchBuildingEntryFormat = (string)jdata["messages"]["meta"]["ranch"]["build"]["build_format"];
            Messages.RanchCantAffordThisBuilding = (string)jdata["messages"]["meta"]["ranch"]["build"]["cannot_afford"];
            Messages.RanchBuildingInformationFormat = (string)jdata["messages"]["meta"]["ranch"]["build"]["information"];
            Messages.RanchBuildingComplete = (string)jdata["messages"]["meta"]["ranch"]["build"]["build_complete"];
            Messages.RanchBuildingAlreadyHere = (string)jdata["messages"]["meta"]["ranch"]["build"]["building_allready_placed"];
            Messages.RanchTornDownRanchBuildingFormat = (string)jdata["messages"]["meta"]["ranch"]["build"]["torn_down"];
            Messages.RanchViewBuildingFormat = (string)jdata["messages"]["meta"]["ranch"]["build"]["view_building"];
            Messages.RanchBarnHorsesFormat = (string)jdata["messages"]["meta"]["ranch"]["build"]["barn"];

            // Ranch : Upgrades

            Messages.UpgradedMessage = (string)jdata["messages"]["meta"]["ranch"]["upgrade"]["upgrade_message"];
            Messages.UpgradeCannotAfford = (string)jdata["messages"]["meta"]["ranch"]["upgrade"]["cannot_afford"];
            Messages.UpgradeCurrentUpgradeFormat = (string)jdata["messages"]["meta"]["ranch"]["upgrade"]["upgrade_meta"];
            Messages.UpgradeNextUpgradeFormat = (string)jdata["messages"]["meta"]["ranch"]["upgrade"]["you_could_upgrade"];

            // Ranch : Special

            Messages.BuildingRestHere = (string)jdata["messages"]["meta"]["ranch"]["special"]["rest_here"];
            Messages.BuildingGrainSilo = (string)jdata["messages"]["meta"]["ranch"]["special"]["grain_silo"];
            Messages.BuildingBarnFormat = (string)jdata["messages"]["meta"]["ranch"]["special"]["barn"];
            Messages.BuildingBigBarnFormat = (string)jdata["messages"]["meta"]["ranch"]["special"]["big_barn"];
            Messages.BuildingGoldBarnFormat = (string)jdata["messages"]["meta"]["ranch"]["special"]["gold_barn"];
            Messages.BuildingWaterWell = (string)jdata["messages"]["meta"]["ranch"]["special"]["water_well"];
            Messages.BuildingWindmillFormat = (string)jdata["messages"]["meta"]["ranch"]["special"]["windmills"];
            Messages.BuildingWagon = (string)jdata["messages"]["meta"]["ranch"]["special"]["wagon"];
            Messages.BuildingTrainingPen = (string)jdata["messages"]["meta"]["ranch"]["special"]["training_pen"];
            Messages.BuildingVegatableGarden = (string)jdata["messages"]["meta"]["ranch"]["special"]["vegatable_garden"];

            Messages.RanchTrainAllAttempt = (string)jdata["messages"]["meta"]["ranch"]["special"]["train_all"];
            Messages.RanchTrainSuccessFormat = (string)jdata["messages"]["meta"]["ranch"]["special"]["train_success"];
            Messages.RanchTrainCantTrainFormat = (string)jdata["messages"]["meta"]["ranch"]["special"]["train_cant_train"];
            Messages.RanchTrainBadMoodFormat = (string)jdata["messages"]["meta"]["ranch"]["special"]["train_bad_mood"];
            Messages.RanchHorsesFullyRested = (string)jdata["messages"]["meta"]["ranch"]["special"]["fully_rested"];
            Messages.RanchWagonDroppedYouOff = (string)jdata["messages"]["meta"]["ranch"]["special"]["wagon_used"];

            // Treasure
            Messages.PirateTreasureFormat = (string)jdata["messages"]["treasure"]["pirate_treasure"];
            Messages.PotOfGoldFormat = (string)jdata["messages"]["treasure"]["pot_of_gold"];

            // Records
            Messages.PrivateNotesSavedMessage = (string)jdata["messages"]["private_notes_save"];
            Messages.PrivateNotesMetaFormat = (string)jdata["messages"]["meta"]["private_notes_format"];

            // Profile
            Messages.ProfileSavedMessage = (string)jdata["messages"]["profile"]["save"];
            Messages.ProfileTooLongMessage = (string)jdata["messages"]["profile"]["too_long"];
            Messages.ProfileViolationFormat = (string)jdata["messages"]["profile"]["blocked"];

            // Announcements

            Messages.WelcomeFormat = (string)jdata["messages"]["welcome_format"];
            Messages.MotdFormat = (string)jdata["messages"]["motd_format"];
            Messages.LoginMessageFormat = (string)jdata["messages"]["login_format"];
            Messages.LogoutMessageFormat = (string)jdata["messages"]["logout_format"];

            // Pronoun
            Messages.PronounFemaleShe = (string)jdata["messages"]["meta"]["stats_page"]["pronouns"]["female_she"];
            Messages.PronounFemaleHer = (string)jdata["messages"]["meta"]["stats_page"]["pronouns"]["female_her"];

            Messages.PronounMaleHe = (string)jdata["messages"]["meta"]["stats_page"]["pronouns"]["male_he"];
            Messages.PronounMaleHis = (string)jdata["messages"]["meta"]["stats_page"]["pronouns"]["male_his"];

            Messages.PronounNeutralYour = (string)jdata["messages"]["meta"]["stats_page"]["pronouns"]["neutral_your"];

            Messages.PronounNeutralThey = (string)jdata["messages"]["meta"]["stats_page"]["pronouns"]["neutral_they"];
            Messages.PronounNeutralTheir = (string)jdata["messages"]["meta"]["stats_page"]["pronouns"]["neutral_their"];

            // Stats
            Messages.StatsBarFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_bar_format"];
            Messages.StatsAreaFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_area_format"];
            Messages.StatsMoneyFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_money_format"];
            Messages.StatsFreeTimeFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_freetime_format"];
            Messages.StatsDescriptionFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_description_format"];
            Messages.StatsExpFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_experience"];
            Messages.StatsQuestpointsFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_questpoints"];
            Messages.StatsHungerFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_hunger"];
            Messages.StatsThirstFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_thisrt"];
            Messages.StatsTiredFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_tiredness"];
            Messages.StatsGenderFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_gender"];
            Messages.StatsJewelFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_equipped"];
            Messages.StatsCompetitionGearFormat = (string)jdata["messages"]["meta"]["stats_page"]["stats_competion_gear"];

            Messages.JewelrySlot1Format = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["slot_1"];
            Messages.JewelrySlot2Format = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["slot_2"];
            Messages.JewelrySlot3Format = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["slot_3"];
            Messages.JewelrySlot4Format = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["slot_4"];

            Messages.JewelryRemoveSlot1Button = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_1"];
            Messages.JewelryRemoveSlot2Button = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_2"];
            Messages.JewelryRemoveSlot3Button = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_3"];
            Messages.JewelryRemoveSlot4Button = (string)jdata["messages"]["meta"]["stats_page"]["jewelry"]["remove_slot_4"];

            Messages.CompetitionGearHeadFormat = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["head_format"];
            Messages.CompetitionGearBodyFormat = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["body_format"];
            Messages.CompetitionGearLegsFormat = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["legs_format"];
            Messages.CompetitionGearFeetFormat = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["feet_format"];

            Messages.CompetitionGearRemoveHeadButton = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["remove_head"];
            Messages.CompetitionGearRemoveBodyButton = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["remove_body"];
            Messages.CompetitionGearRemoveLegsButton = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["remove_legs"];
            Messages.CompetitionGearRemoveFeetButton = (string)jdata["messages"]["meta"]["stats_page"]["competition_gear"]["remove_feet"];

            Messages.StatsPrivateNotesButton = (string)jdata["messages"]["meta"]["stats_page"]["stats_private_notes"];
            Messages.StatsQuestsButton = (string)jdata["messages"]["meta"]["stats_page"]["stats_quests"];
            Messages.StatsMinigameRankingButton = (string)jdata["messages"]["meta"]["stats_page"]["stats_minigame_ranking"];
            Messages.StatsAwardsButton = (string)jdata["messages"]["meta"]["stats_page"]["stats_awards"];
            Messages.StatsMiscButton = (string)jdata["messages"]["meta"]["stats_page"]["stats_misc"];

            Messages.JewelrySelected = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["jewelry_selected"];
            Messages.JewelrySelectedOther = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["jewelry_other"];

            Messages.NoJewerlyEquipped = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["no_jewelry_equipped"];
            Messages.NoJewerlyEquippedOther = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["no_jewelry_other"];

            Messages.NoCompetitionGear = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["no_competition_gear"];
            Messages.NoCompetitionGearOther = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["no_competition_gear_other"];

            Messages.CompetitionGearSelected = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["competition_gear_selected"];
            Messages.CompetitionGearSelectedOther = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["competition_gear_other_selected"];

            Messages.StatHunger = (string)jdata["messages"]["meta"]["stats_page"]["hunger_stat_name"];
            Messages.StatThirst = (string)jdata["messages"]["meta"]["stats_page"]["thirst_stat_name"];
            Messages.StatTired = (string)jdata["messages"]["meta"]["stats_page"]["tired_stat_name"];

            Messages.StatsOtherHorses = (string)jdata["messages"]["meta"]["stats_page"]["msg"]["other_horses"];
            Messages.StatPlayerFormats = jdata["messages"]["meta"]["stats_page"]["player_stats"].AsArray<string>();

            Messages.StatThirstDizzy = (string)jdata["messages"]["movement_key"]["thirsty"];
            Messages.StatHungerStumble = (string)jdata["messages"]["movement_key"]["hungery"];

            // Misc Stats
            Messages.StatMiscHeader = (string)jdata["messages"]["meta"]["misc_stats"]["header"];
            Messages.StatMiscNoneRecorded = (string)jdata["messages"]["meta"]["misc_stats"]["no_stats_recorded"];
            Messages.StatMiscEntryFormat = (string)jdata["messages"]["meta"]["misc_stats"]["stat_format"];

            // Books (Libary)
            Messages.BooksOfHorseIsle = (string)jdata["messages"]["meta"]["libary"]["books"]["books_of_horseisle"];
            Messages.BookEntryFormat = (string)jdata["messages"]["meta"]["libary"]["books"]["book_entry"];
            Messages.BookReadFormat = (string)jdata["messages"]["meta"]["libary"]["books"]["book_read"];

            // Awards (Libary)
            Messages.AwardsAvalible = (string)jdata["messages"]["meta"]["libary"]["awards"]["all_earnable_awards"];
            Messages.AwardEntryFormat = (string)jdata["messages"]["meta"]["libary"]["awards"]["award_entry"];

            // Locations (Libary)
            Messages.LocationKnownIslands = (string)jdata["messages"]["meta"]["libary"]["locations"]["known_islands"];
            Messages.LocationKnownTowns = (string)jdata["messages"]["meta"]["libary"]["locations"]["known_towns"];
            Messages.LocationIslandFormat = (string)jdata["messages"]["meta"]["libary"]["locations"]["isle_entry"];
            Messages.LocationTownFormat = (string)jdata["messages"]["meta"]["libary"]["locations"]["town_entry"];
            Messages.LocationDescriptionFormat = (string)jdata["messages"]["meta"]["libary"]["locations"]["location_description"];

            // Minigame (Libary)
            Messages.MinigameSingleplayer = (string)jdata["messages"]["meta"]["libary"]["minigames"]["singleplayer"];
            Messages.MinigameTwoplayer = (string)jdata["messages"]["meta"]["libary"]["minigames"]["twoplayer"];
            Messages.MinigameMultiplayer = (string)jdata["messages"]["meta"]["libary"]["minigames"]["multiplayer"];
            Messages.MinigameCompetitions = (string)jdata["messages"]["meta"]["libary"]["minigames"]["competitions"];
            Messages.MinigameEntryFormat = (string)jdata["messages"]["meta"]["libary"]["minigames"]["minigame_entry"];

            // Companion (Libary)
            Messages.CompanionViewFormat = (string)jdata["messages"]["meta"]["libary"]["companion"]["view_button"];
            Messages.CompanionEntryFormat = (string)jdata["messages"]["meta"]["libary"]["companion"]["entry_format"];

            // Tack (Libary)
            Messages.TackViewSetFormat = (string)jdata["messages"]["meta"]["libary"]["tack"]["view_tack_set"];
            Messages.TackSetPeiceFormat = (string)jdata["messages"]["meta"]["libary"]["tack"]["set_peice_format"];

            // Groomer
            Messages.GroomerBestToHisAbilitiesFormat = (string)jdata["messages"]["meta"]["groomer"]["groomed_best_it_can"];
            Messages.GroomerCannotAffordMessage = (string)jdata["messages"]["meta"]["groomer"]["cannot_afford_service"];
            Messages.GroomerCannotImprove = (string)jdata["messages"]["meta"]["groomer"]["cannot_improve"];
            Messages.GroomerBestToHisAbilitiesALL = (string)jdata["messages"]["meta"]["groomer"]["groomed_best_all"];
            Messages.GroomerDontNeed = (string)jdata["messages"]["meta"]["groomer"]["dont_need"];

            Messages.GroomerHorseCurrentlyAtFormat = (string)jdata["messages"]["meta"]["groomer"]["currently_at"];
            Messages.GroomerApplyServiceFormat = (string)jdata["messages"]["meta"]["groomer"]["apply_service"];
            Messages.GroomerApplyServiceForAllFormat = (string)jdata["messages"]["meta"]["groomer"]["apply_all"];

            // Barn
            Messages.BarnHorseFullyFedFormat = (string)jdata["messages"]["meta"]["barn"]["fully_fed"];
            Messages.BarnCantAffordService = (string)jdata["messages"]["meta"]["barn"]["cant_afford"];
            Messages.BarnAllHorsesFullyFed = (string)jdata["messages"]["meta"]["barn"]["rested_all"];
            Messages.BarnServiceNotNeeded = (string)jdata["messages"]["meta"]["barn"]["not_needed"];

            Messages.BarnHorseStatusFormat = (string)jdata["messages"]["meta"]["barn"]["horse_status"];
            Messages.BarnHorseMaxed = (string)jdata["messages"]["meta"]["barn"]["horse_maxed"];
            Messages.BarnLetHorseRelaxFormat = (string)jdata["messages"]["meta"]["barn"]["let_relax"];
            Messages.BarnLetAllHorsesReleaxFormat = (string)jdata["messages"]["meta"]["barn"]["relax_all"];

            // Farrier
            Messages.FarrierCurrentShoesFormat = (string)jdata["messages"]["meta"]["farrier"]["current_shoes"];
            Messages.FarrierApplyIronShoesFormat = (string)jdata["messages"]["meta"]["farrier"]["apply_iron"];
            Messages.FarrierApplySteelShoesFormat = (string)jdata["messages"]["meta"]["farrier"]["apply_steel"];
            Messages.FarrierShoeAllFormat = (string)jdata["messages"]["meta"]["farrier"]["shoe_all"];

            Messages.FarrierPutOnSteelShoesMessageFormat = (string)jdata["messages"]["meta"]["farrier"]["put_on_steel_shoes"];
            Messages.FarrierPutOnIronShoesMessageFormat = (string)jdata["messages"]["meta"]["farrier"]["put_on_iron_shoes"];
            Messages.FarrierPutOnSteelShoesAllMesssageFormat = (string)jdata["messages"]["meta"]["farrier"]["put_on_steel_all"];
            Messages.FarrierShoesCantAffordMessage = (string)jdata["messages"]["meta"]["farrier"]["cant_afford_farrier"];

            // Trainng Pen
            Messages.TrainedInStatFormat = (string)jdata["messages"]["meta"]["trainer_pen"]["train_success"];
            Messages.TrainerHeaderFormat = (string)jdata["messages"]["meta"]["trainer_pen"]["train_header"];
            Messages.TrainerHorseEntryFormat = (string)jdata["messages"]["meta"]["trainer_pen"]["train_format"];
            Messages.TrainerHorseFullyTrainedFormat = (string)jdata["messages"]["meta"]["trainer_pen"]["fully_trained"];
            Messages.TrainerCantTrainAgainInFormat = (string)jdata["messages"]["meta"]["trainer_pen"]["train_again_in"];
            Messages.TrainerCantAfford = (string)jdata["messages"]["meta"]["trainer_pen"]["cant_afford"];

            // Santa
            Messages.SantaHiddenText = (string)jdata["messages"]["meta"]["santa"]["hidden_text"];
            Messages.SantaWrapItemFormat = (string)jdata["messages"]["meta"]["santa"]["wrap_format"];
            Messages.SantaWrappedObjectMessage = (string)jdata["messages"]["meta"]["santa"]["wrapped_object"];
            Messages.SantaCantWrapInvFull = (string)jdata["messages"]["meta"]["santa"]["wrap_fail_inv_full"];
            Messages.SantaCantOpenNothingInside = (string)jdata["messages"]["meta"]["santa"]["open_fail_empty"];
            Messages.SantaItemOpenedFormat = (string)jdata["messages"]["meta"]["santa"]["open_format"];
            Messages.SantaItemCantOpenInvFull = (string)jdata["messages"]["meta"]["santa"]["open_fail_inv_full"];

            // Pawneer
            Messages.PawneerUntackedHorsesICanBuy = (string)jdata["messages"]["meta"]["pawneer"]["untacked_i_can_buy"];
            Messages.PawneerHorseFormat = (string)jdata["messages"]["meta"]["pawneer"]["pawn_horse"];
            Messages.PawneerOrderMeta = (string)jdata["messages"]["meta"]["pawneer"]["pawneer_order"];
            Messages.PawneerHorseConfirmationFormat = (string)jdata["messages"]["meta"]["pawneer"]["are_you_sure"];
            Messages.PawneerHorseSoldMessagesFormat = (string)jdata["messages"]["meta"]["pawneer"]["horse_sold"];
            Messages.PawneerHorseNotFound = (string)jdata["messages"]["meta"]["pawneer"]["horse_not_found"];

            Messages.PawneerOrderSelectBreed = (string)jdata["messages"]["meta"]["pawneer"]["order"]["select_breed"];
            Messages.PawneerOrderBreedEntryFormat = (string)jdata["messages"]["meta"]["pawneer"]["order"]["breed_entry"];

            Messages.PawneerOrderSelectColorFormat = (string)jdata["messages"]["meta"]["pawneer"]["order"]["select_color"];
            Messages.PawneerOrderColorEntryFormat = (string)jdata["messages"]["meta"]["pawneer"]["order"]["color_entry"];

            Messages.PawneerOrderSelectGenderFormat = (string)jdata["messages"]["meta"]["pawneer"]["order"]["select_gender"];
            Messages.PawneerOrderGenderEntryFormat = (string)jdata["messages"]["meta"]["pawneer"]["order"]["gender_entry"];

            Messages.PawneerOrderHorseFoundFormat = (string)jdata["messages"]["meta"]["pawneer"]["order"]["found"];

            // Vet
            Messages.VetServiceHorseFormat = (string)jdata["messages"]["meta"]["vet"]["service_horse"];
            Messages.VetSerivcesNotNeeded = (string)jdata["messages"]["meta"]["vet"]["not_needed"];
            Messages.VetApplyServicesFormat = (string)jdata["messages"]["meta"]["vet"]["apply"];

            Messages.VetApplyServicesForAllFormat = (string)jdata["messages"]["meta"]["vet"]["apply_all"];
            Messages.VetFullHealthRecoveredMessageFormat = (string)jdata["messages"]["meta"]["vet"]["now_full_health"];
            Messages.VetServicesNotNeededAll = (string)jdata["messages"]["meta"]["vet"]["not_needed_all"];
            Messages.VetAllFullHealthRecoveredMessage = (string)jdata["messages"]["meta"]["vet"]["all_full"];
            Messages.VetCannotAffordMessage = (string)jdata["messages"]["meta"]["vet"]["cant_afford"];

            // Pond
            Messages.PondHeader = (string)jdata["messages"]["meta"]["pond"]["header"];
            Messages.PondGoFishing = (string)jdata["messages"]["meta"]["pond"]["go_fishing"];
            Messages.PondNoFishingPole = (string)jdata["messages"]["meta"]["pond"]["no_fishing_pole"];
            Messages.PondDrinkHereIfSafe = (string)jdata["messages"]["meta"]["pond"]["drink_here"];
            Messages.PondHorseDrinkFormat = (string)jdata["messages"]["meta"]["pond"]["horse_drink_format"];
            Messages.PondNoEarthWorms = (string)jdata["messages"]["meta"]["pond"]["no_earth_worms"];

            Messages.PondDrinkFullFormat = (string)jdata["messages"]["meta"]["pond"]["drank_full"];
            Messages.PondCantDrinkHpLowFormat = (string)jdata["messages"]["meta"]["pond"]["cant_drink_hp_low"];
            Messages.PondDrinkOhNoesFormat = (string)jdata["messages"]["meta"]["pond"]["drank_something_bad"];
            Messages.PondNotThirstyFormat = (string)jdata["messages"]["meta"]["pond"]["not_thirsty"];

            // Horse Whisperer
            Messages.WhispererHorseLocateButtonFormat = (string)jdata["messages"]["meta"]["whisperer"]["horse_locate_meta"];
            Messages.WhispererServiceCostYouFormat = (string)jdata["messages"]["meta"]["whisperer"]["service_cost"];
            Messages.WhispererServiceCannotAfford = (string)jdata["messages"]["meta"]["whisperer"]["cant_afford"];
            Messages.WhispererSearchingAmoungHorses = (string)jdata["messages"]["meta"]["whisperer"]["searching_amoung_horses"];
            Messages.WhispererNoneFound = (string)jdata["messages"]["meta"]["whisperer"]["none_found_meta"];
            Messages.WhispererHorsesFoundFormat = (string)jdata["messages"]["meta"]["whisperer"]["horse_found_meta"];

            // Mud Hole
            Messages.MudHoleNoHorses = (string)jdata["messages"]["meta"]["mud_hole"]["no_horses"];
            Messages.MudHoleRuinedGroomFormat = (string)jdata["messages"]["meta"]["mud_hole"]["ruined_groom"];

            // Movement
            Messages.RandomMovement = (string)jdata["messages"]["random_movement"];

            // Quests Log
            Messages.QuestLogHeader = (string)jdata["messages"]["meta"]["quest_log"]["header_meta"];
            Messages.QuestFormat = (string)jdata["messages"]["meta"]["quest_log"]["quest_format"];

            Messages.QuestNotCompleted = (string)jdata["messages"]["meta"]["quest_log"]["not_complete"];
            Messages.QuestNotAvalible = (string)jdata["messages"]["meta"]["quest_log"]["not_avalible"];
            Messages.QuestCompleted = (string)jdata["messages"]["meta"]["quest_log"]["completed"];

            Messages.QuestFooterFormat = (string)jdata["messages"]["meta"]["quest_log"]["footer_format"];
            // Transport

            Messages.CantAffordTransport = (string)jdata["messages"]["transport"]["not_enough_money"];
            Messages.WelcomeToAreaFormat = (string)jdata["messages"]["transport"]["welcome_to_format"];
            Messages.TransportFormat = (string)jdata["messages"]["meta"]["transport_format"];
            Messages.TransportCostFormat = (string)jdata["messages"]["meta"]["transport_cost"];
            Messages.TransportWagonFree = (string)jdata["messages"]["meta"]["transport_free"];

            // Abuse Reports
            Messages.AbuseReportMetaFormat = (string)jdata["messages"]["meta"]["abuse_report"]["options_format"];
            Messages.AbuseReportReasonFormat = (string)jdata["messages"]["meta"]["abuse_report"]["report_reason_format"];

            Messages.AbuseReportPlayerNotFoundFormat = (string)jdata["messages"]["abuse_report"]["player_not_found_format"];
            Messages.AbuseReportFiled = (string)jdata["messages"]["abuse_report"]["report_filed"];
            Messages.AbuseReportProvideValidReason = (string)jdata["messages"]["abuse_report"]["valid_reason"];

            // Bank
            Messages.BankMadeInIntrestFormat = (string)jdata["messages"]["meta"]["bank"]["made_interest"];
            Messages.BankCarryingFormat = (string)jdata["messages"]["meta"]["bank"]["carrying_message"];
            Messages.BankWhatToDo = (string)jdata["messages"]["meta"]["bank"]["what_to_do"];
            Messages.BankOptionsFormat = (string)jdata["messages"]["meta"]["bank"]["options"];


            Messages.BankDepositedMoneyFormat = (string)jdata["messages"]["bank"]["deposit_format"];
            Messages.BankWithdrewMoneyFormat = (string)jdata["messages"]["bank"]["withdraw_format"];

            Messages.BankCantHoldThisMuch = (string)jdata["messages"]["bank"]["cant_hold_that_much"];
            Messages.BankYouCantHoldThisMuch = (string)jdata["messages"]["bank"]["cant_withdraw_that_much"];

            // Riddler
            Messages.RiddlerAnsweredAll = (string)jdata["messages"]["meta"]["riddler"]["riddle_all_complete"];
            Messages.RiddlerIncorrectAnswer = (string)jdata["messages"]["meta"]["riddler"]["riddle_incorrect"];
            Messages.RiddlerCorrectAnswerFormat = (string)jdata["messages"]["meta"]["riddler"]["riddle_correct"];
            Messages.RiddlerEnterAnswerFormat = (string)jdata["messages"]["meta"]["riddler"]["riddle_format"];

            // Workshop
            Messages.WorkshopCraftEntryFormat = (string)jdata["messages"]["meta"]["workshop"]["craft_entry"];
            Messages.WorkshopRequiresFormat = (string)jdata["messages"]["meta"]["workshop"]["requires"];
            Messages.WorkshopRequireEntryFormat = (string)jdata["messages"]["meta"]["workshop"]["require"];
            Messages.WorkshopAnd = (string)jdata["messages"]["meta"]["workshop"]["and"];

            Messages.WorkshopNoRoomInInventory = (string)jdata["messages"]["meta"]["workshop"]["no_room"];
            Messages.WorkshopMissingRequiredItem = (string)jdata["messages"]["meta"]["workshop"]["missing_item"];
            Messages.WorkshopCraftingSuccess = (string)jdata["messages"]["meta"]["workshop"]["craft_success"];
            Messages.WorkshopCannotAfford = (string)jdata["messages"]["meta"]["workshop"]["no_money"];

            // Horses
            Messages.AdvancedStatFormat = (string)jdata["messages"]["meta"]["horse"]["stat_format"];
            Messages.BasicStatFormat = (string)jdata["messages"]["meta"]["horse"]["basic_stat_format"];
            Messages.HorsesHere = (string)jdata["messages"]["meta"]["horse"]["horses_here"];
            Messages.WildHorseFormat = (string)jdata["messages"]["meta"]["horse"]["wild_horse"];
            Messages.HorseCaptureTimer = (string)jdata["messages"]["meta"]["horse"]["horse_timer"];

            Messages.YouCapturedTheHorse = (string)jdata["messages"]["meta"]["horse"]["horse_caught"];
            Messages.HorseEvadedCapture = (string)jdata["messages"]["meta"]["horse"]["horse_escaped"];
            Messages.HorseEscapedAnyway = (string)jdata["messages"]["meta"]["horse"]["horse_escaped_anyway"];

            Messages.HorsesMenuHeader = (string)jdata["messages"]["meta"]["horse"]["horses_menu"];
            Messages.TooManyHorses = (string)jdata["messages"]["meta"]["horse"]["too_many_horses"];
            Messages.UpdateHorseCategory = (string)jdata["messages"]["meta"]["horse"]["update_category"];
            Messages.HorseEntryFormat = (string)jdata["messages"]["meta"]["horse"]["horse_format"];
            Messages.ViewBaiscStats = (string)jdata["messages"]["meta"]["horse"]["view_basic_stats"];
            Messages.ViewAdvancedStats = (string)jdata["messages"]["meta"]["horse"]["view_advanced_stats"];
            Messages.HorseBuckedYou = (string)jdata["messages"]["meta"]["horse"]["horse_bucked"];
            Messages.HorseLlamaBuckedYou = (string)jdata["messages"]["meta"]["horse"]["llama_bucked"];
            Messages.HorseCamelBuckedYou = (string)jdata["messages"]["meta"]["horse"]["camel_bucked"];

            Messages.HorseRidingMessageFormat = (string)jdata["messages"]["meta"]["horse"]["riding_message"];
            Messages.HorseNameYoursFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["your_horse_format"];
            Messages.HorseNameOthersFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["horse_others_format"];
            Messages.HorseDescriptionFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["description_format"];
            Messages.HorseHandsHeightFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["hands_high"];
            Messages.HorseExperienceEarnedFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["experience"];

            Messages.HorseTrainableInFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["trainable_in"];
            Messages.HorseIsTrainable = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["currently_trainable"];
            Messages.HorseLeasedRemainingTimeFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["leased_horse"];

            Messages.HorseCannotMountUntilTackedMessage = (string)jdata["messages"]["meta"]["horse"]["cannot_mount_tacked"];
            Messages.HorseDismountedBecauseNotTackedMessageFormat = (string)jdata["messages"]["meta"]["horse"]["dismount_because_tack"];
            Messages.HorseMountButtonFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["mount_button"];
            Messages.HorseDisMountButtonFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["dismount_button"];
            Messages.HorseFeedButtonFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["feed_button"];
            Messages.HorseTackButtonFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["tack_button"];
            Messages.HorsePetButtonFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["pet_button"];
            Messages.HorseProfileButtonFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["profile_button"];

            Messages.HorseSavedProfileMessageFormat = (string)jdata["messages"]["meta"]["horse"]["profile"]["saved"];
            Messages.HorseProfileMessageTooLongError = (string)jdata["messages"]["meta"]["horse"]["profile"]["desc_too_long"];
            Messages.HorseNameTooLongError = (string)jdata["messages"]["meta"]["horse"]["profile"]["name_too_long"];
            Messages.HorseNameViolationsError = (string)jdata["messages"]["meta"]["horse"]["profile"]["name_profanity_detected"];
            Messages.HorseProfileMessageProfileError = (string)jdata["messages"]["meta"]["horse"]["profile"]["profile_profanity_detected"];

            Messages.HorseCatchTooManyHorsesMessage = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["horse_cannot_catch_max"];
            Messages.HorseNoAutoSell = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["no_auto_sell"];
            Messages.HorseAutoSellPriceFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["auto_sell_format"];
            Messages.HorseAutoSellOthersFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["auto_sell_others"];
            Messages.HorseAutoSellFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["auto_sell"];
            Messages.HorseCantAutoSellTacked = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["cannot_auto_sell_tacked"];

            Messages.HorseCurrentlyCategoryFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["marked_as"];
            Messages.HorseMarkAsCategory = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["marking_options"];
            Messages.HorseStats = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["horse_stats"];
            Messages.HorseTacked = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["wearing_tacked"];
            Messages.HorseTackFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["tacked_format"];

            Messages.HorseCompanion = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["companion"];
            Messages.HorseCompanionFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["companion_selected"];
            Messages.HorseCompanionChangeButton = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["companion_change_button"];
            Messages.HorseNoCompanion = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["no_companion"];

            Messages.HorseAdvancedStatsFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["advanced_stats"];
            Messages.HorseBreedDetailsFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["breed_details"];
            Messages.HorseHeightRangeFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["height_range"];
            Messages.HorsePossibleColorsFormat = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["possible_colors"];
            Messages.HorseReleaseButton = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["release_horse"];
            Messages.HorseOthers = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["other_horses"];

            Messages.HorseDescriptionEditFormat = (string)jdata["messages"]["meta"]["horse"]["description_edit"];
            Messages.HorseEquipTackMessageFormat = (string)jdata["messages"]["meta"]["horse"]["equip_tack_message"];
            Messages.HorseUnEquipTackMessageFormat = (string)jdata["messages"]["meta"]["horse"]["unequip_tack_message"];
            Messages.HorseStopRidingMessage = (string)jdata["messages"]["meta"]["horse"]["stop_riding_message"];

            Messages.HorsePetMessageFormat = (string)jdata["messages"]["meta"]["horse"]["pet_horse"];
            Messages.HorsePetTooHappy = (string)jdata["messages"]["meta"]["horse"]["pet_horse_too_happy"];
            Messages.HorsePetTooTired = (string)jdata["messages"]["meta"]["horse"]["pet_horse_too_sleepy"];
            Messages.HorseSetNewCategoryMessageFormat = (string)jdata["messages"]["meta"]["horse"]["horse_set_new_category"];

            Messages.HorseAutoSellMenuFormat = (string)jdata["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_meta"];
            Messages.HorseIsAutoSell = (string)jdata["messages"]["meta"]["horse"]["auto_sell"]["is_auto_sell"];
            Messages.HorseAutoSellConfirmedFormat = (string)jdata["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_confirmed"];
            Messages.HorseAutoSellValueTooHigh = (string)jdata["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_to_high"];
            Messages.HorseAutoSellRemoved = (string)jdata["messages"]["meta"]["horse"]["auto_sell"]["auto_sell_remove"];

            Messages.HorseSetAutoSell = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["set_auto_sell"];
            Messages.HorseChangeAutoSell = (string)jdata["messages"]["meta"]["horse"]["horse_inventory"]["change_auto_sell"];
            Messages.HorseTackFailAutoSell = (string)jdata["messages"]["meta"]["horse"]["tack_fail_autosell"];

            Messages.HorseAreYouSureYouWantToReleaseFormat = (string)jdata["messages"]["meta"]["horse"]["horse_release"];
            Messages.HorseCantReleaseTheHorseYourRidingOn = (string)jdata["messages"]["meta"]["horse"]["cant_release_currently_riding"];
            Messages.HorseReleasedMeta = (string)jdata["messages"]["meta"]["horse"]["released_horse"];
            Messages.HorseReleasedBy = (string)jdata["messages"]["meta"]["horse"]["released_by_message"];

            // All Stats (basic)

            Messages.HorseAllBasicStats = (string)jdata["messages"]["meta"]["horse"]["allstats_basic"]["all_baisc_stats"];
            Messages.HorseBasicStatEntryFormat = (string)jdata["messages"]["meta"]["horse"]["allstats_basic"]["horse_entry"];

            // All Stats (all)
            Messages.HorseAllStatsHeader = (string)jdata["messages"]["meta"]["horse"]["allstats"]["all_stats_header"];
            Messages.HorseNameEntryFormat = (string)jdata["messages"]["meta"]["horse"]["allstats"]["horse_name_entry"];
            Messages.HorseBasicStatsCompactedFormat = (string)jdata["messages"]["meta"]["horse"]["allstats"]["basic_stats_compact"];
            Messages.HorseAdvancedStatsCompactedFormat = (string)jdata["messages"]["meta"]["horse"]["allstats"]["advanced_stats_compact"];
            Messages.HorseAllStatsLegend = (string)jdata["messages"]["meta"]["horse"]["allstats"]["legend"];


            // Horse companion menu
            Messages.HorseCompanionMenuHeaderFormat = (string)jdata["messages"]["meta"]["horse"]["companion_menu"]["menu_header"];
            Messages.HorseCompnaionMenuCurrentCompanionFormat = (string)jdata["messages"]["meta"]["horse"]["companion_menu"]["selected_companion"];
            Messages.HorseCompanionEntryFormat = (string)jdata["messages"]["meta"]["horse"]["companion_menu"]["companion_entry"];
            Messages.HorseCompanionEquipMessageFormat = (string)jdata["messages"]["meta"]["horse"]["companion_menu"]["companion_equip_message"];
            Messages.HorseCompanionRemoveMessageFormat = (string)jdata["messages"]["meta"]["horse"]["companion_menu"]["companion_remove_message"];
            Messages.HorseCompanionMenuCurrentlyAvalibleCompanions = (string)jdata["messages"]["meta"]["horse"]["companion_menu"]["companions_avalible"];

            // Horse Feed Menu
            Messages.HorseCurrentStatusFormat = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["current_status"];
            Messages.HorseHoldingHorseFeed = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["holding_horse_feed"];
            Messages.HorsefeedFormat = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["horsefeed_format"];
            Messages.HorseNeighsThanks = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["horse_neigh"];
            Messages.HorseCouldNotFinish = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["horse_could_not_finish"];

            Messages.HorseFeedPersonalityIncreased = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["feed_special_personality"];
            Messages.HorseFeedInteligenceIncreased = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["feed_special_inteligence"];
            Messages.HorseFeedMagicBeanFormat = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["feed_special_magic_bean"];
            Messages.HorseFeedMagicDropletFormat = (string)jdata["messages"]["meta"]["horse"]["feed_horse"]["feed_special_magic_droplet"];

            // Tack menu (horses)
            Messages.HorseTackedAsFollowsFormat = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["tacked_as_follows"];
            Messages.HorseUnEquipSaddleFormat = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["dequip_saddle"];
            Messages.HorseUnEquipSaddlePadFormat = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["dequip_saddle_pad"];
            Messages.HorseUnEquipBridleFormat = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["dequip_bridle"];
            Messages.HorseTackInInventory = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["you_have_following_tack"];
            Messages.HorseLlamaTackInInventory = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["you_have_following_llama_tack"];
            Messages.HorseCamelTackInInventory = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["you_have_following_camel_tack"];
            Messages.HorseEquipFormat = (string)jdata["messages"]["meta"]["horse"]["tack_menu"]["equip_tack"];
            Messages.BackToHorse = (string)jdata["messages"]["meta"]["horse"]["back_to_horse"];


            // Libary
            Messages.LibaryMainMenu = (string)jdata["messages"]["meta"]["libary"]["main_menu"];
            Messages.LibaryFindNpc = (string)jdata["messages"]["meta"]["libary"]["find_npc"];
            Messages.LibaryFindNpcSearchResultsHeader = (string)jdata["messages"]["meta"]["libary"]["find_npc_results_header"];
            Messages.LibaryFindNpcSearchResultFormat = (string)jdata["messages"]["meta"]["libary"]["find_npc_results_format"];
            Messages.LibaryFindNpcSearchNoResults = (string)jdata["messages"]["meta"]["libary"]["find_npc_no_results"];
            Messages.LibaryFindNpcLimit5 = (string)jdata["messages"]["meta"]["libary"]["find_npc_limit5"];

            Messages.LibaryFindRanch = (string)jdata["messages"]["meta"]["libary"]["find_ranch"];
            Messages.LibaryFindRanchResultsHeader = (string)jdata["messages"]["meta"]["libary"]["find_ranch_match_closely"];
            Messages.LibaryFindRanchResultFormat = (string)jdata["messages"]["meta"]["libary"]["find_ranch_result"];
            Messages.LibaryFindRanchResultsNoResults = (string)jdata["messages"]["meta"]["libary"]["find_ranch_no_results"];

            Messages.HorseBreedFormat = (string)jdata["messages"]["meta"]["libary"]["horse_breed_format"];
            Messages.HorseRelativeFormat = (string)jdata["messages"]["meta"]["libary"]["horse_relative_format"];
            Messages.BreedViewerFormat = (string)jdata["messages"]["meta"]["libary"]["breed_preview_format"];
            Messages.BreedViewerMaximumStats = (string)jdata["messages"]["meta"]["libary"]["maximum_stats"];

            // Chat

            Messages.ChatViolationMessageFormat = (string)jdata["messages"]["chat"]["violation_format"];
            Messages.RequiredChatViolations = (int)jdata["messages"]["chat"]["violation_points_required"];

            Messages.GlobalChatFormatForModerators = (string)jdata["messages"]["chat"]["for_others"]["global_format_moderator"];
            Messages.DirectChatFormatForModerators = (string)jdata["messages"]["chat"]["for_others"]["dm_format_moderator"];

            Messages.YouWereSentToPrisionIsle = (string)jdata["messages"]["starved_horse"];

            Messages.HereChatFormat = (string)jdata["messages"]["chat"]["for_others"]["here_format"];
            Messages.IsleChatFormat = (string)jdata["messages"]["chat"]["for_others"]["isle_format"];
            Messages.NearChatFormat = (string)jdata["messages"]["chat"]["for_others"]["near_format"];
            Messages.GlobalChatFormat = (string)jdata["messages"]["chat"]["for_others"]["global_format"];
            Messages.AdsChatFormat = (string)jdata["messages"]["chat"]["for_others"]["ads_format"];
            Messages.DirectChatFormat = (string)jdata["messages"]["chat"]["for_others"]["dm_format"];
            Messages.BuddyChatFormat = (string)jdata["messages"]["chat"]["for_others"]["friend_format"];
            Messages.ModChatFormat = (string)jdata["messages"]["chat"]["for_others"]["mod_format"];
            Messages.AdminChatFormat = (string)jdata["messages"]["chat"]["for_others"]["admin_format"];

            Messages.HereChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["here_format"];
            Messages.IsleChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["isle_format"];
            Messages.NearChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["near_format"];
            Messages.BuddyChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["friend_format"];
            Messages.DirectChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["dm_format"];
            Messages.ModChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["mod_format"];
            Messages.AdsChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["ads_format"];
            Messages.AdminChatFormatForSender = (string)jdata["messages"]["chat"]["for_sender"]["admin_format"];

            Messages.ServerAnnoucementFormat = (string)jdata["messages"]["chat"]["server_annoucement"];

            Messages.DmModBadge = (string)jdata["messages"]["chat"]["dm_moderator"];
            Messages.DmAutoResponse = (string)jdata["messages"]["chat"]["dm_autoreply"];

            Messages.AdminCommandFormat = (string)jdata["messages"]["commands"]["admin_command_completed"];
            Messages.PlayerCommandFormat = (string)jdata["messages"]["commands"]["player_command_completed"];
            Messages.MuteHelp = (string)jdata["messages"]["commands"]["mute_help"];
            Messages.UnMuteHelp = (string)jdata["messages"]["commands"]["unmute_help"];

            Messages.PasswordNotice = (string)jdata["messages"]["chat"]["password_included"];
            Messages.CapsNotice = (string)jdata["messages"]["chat"]["caps_notice"];

            // AutoReply
            Messages.AutoReplyTooLong = (string)jdata["messages"]["auto_reply.too_long"];
            Messages.AutoReplyHasViolations = (string)jdata["messages"]["auto_reply.contains_violations"];

            // Drawing Rooms
            Messages.DrawingLastToDrawFormat = (string)jdata["messages"]["meta"]["drawing_rooms"]["last_draw"];
            Messages.DrawingContentsSavedInSlotFormat = (string)jdata["messages"]["meta"]["drawing_rooms"]["saved"];
            Messages.DrawingContentsLoadedFromSlotFormat = (string)jdata["messages"]["meta"]["drawing_rooms"]["load"];
            Messages.DrawingPlzClearDraw = (string)jdata["messages"]["meta"]["drawing_rooms"]["plz_clear_draw"];
            Messages.DrawingPlzClearLoad = (string)jdata["messages"]["meta"]["drawing_rooms"]["plz_clear_load"];
            Messages.DrawingNotSentNotSubscribed = (string)jdata["messages"]["meta"]["drawing_rooms"]["not_subscribed_draw"];
            Messages.DrawingCannotLoadNotSubscribed = (string)jdata["messages"]["meta"]["drawing_rooms"]["not_subscribed_load"];

            // Brickpoet
            Messages.LastPoetFormat = (string)jdata["messages"]["meta"]["last_poet"];

            // Mutliroom
            Messages.MultiroomParticipentFormat = (string)jdata["messages"]["meta"]["multiroom"]["partcipent_format"];
            Messages.MultiroomPlayersParticipating = (string)jdata["messages"]["meta"]["multiroom"]["other_players_participating"];

            // Dropped Items
            Messages.NothingMessage = (string)jdata["messages"]["meta"]["dropped_items"]["nothing_message"];
            Messages.ItemsOnGroundMessage = (string)jdata["messages"]["meta"]["dropped_items"]["items_message"];
            Messages.GrabItemFormat = (string)jdata["messages"]["meta"]["dropped_items"]["item_format"];
            Messages.ItemInformationFormat = (string)jdata["messages"]["meta"]["dropped_items"]["item_information_format"];
            Messages.GrabAllItemsButton = (string)jdata["messages"]["meta"]["dropped_items"]["grab_all"];
            Messages.DroppedAnItemMessage = (string)jdata["messages"]["dropped_items"]["dropped_item_message"];
            Messages.DroppedItemTileIsFull = (string)jdata["messages"]["dropped_items"]["drop_tile_full"];
            Messages.DroppedItemCouldntPickup = (string)jdata["messages"]["dropped_items"]["other_picked_up"];
            Messages.GrabbedAllItemsMessage = (string)jdata["messages"]["dropped_items"]["grab_all_message"];
            Messages.GrabbedItemMessage = (string)jdata["messages"]["dropped_items"]["grab_message"];
            Messages.GrabAllItemsMessage = (string)jdata["messages"]["dropped_items"]["grab_all_message"];

            Messages.GrabbedAllItemsButInventoryFull = (string)jdata["messages"]["dropped_items"]["grab_all_but_inv_full"];
            Messages.GrabbedItemButInventoryFull = (string)jdata["messages"]["dropped_items"]["grab_but_inv_full"];

            // Tools
            Messages.BinocularsNothing = (string)jdata["messages"]["tools"]["binoculars"];
            Messages.MagnifyNothing = (string)jdata["messages"]["tools"]["magnify"];
            Messages.RakeNothing = (string)jdata["messages"]["tools"]["rake"];
            Messages.ShovelNothing = (string)jdata["messages"]["tools"]["shovel"];

            // Shop
            Messages.ThingsIAmSelling = (string)jdata["messages"]["meta"]["shop"]["selling"];
            Messages.ThingsYouSellMe = (string)jdata["messages"]["meta"]["shop"]["sell_me"];
            Messages.InfinitySign = (string)jdata["messages"]["meta"]["shop"]["infinity"];

            Messages.CantAfford1 = (string)jdata["messages"]["shop"]["cant_afford_1"];
            Messages.CantAfford5 = (string)jdata["messages"]["shop"]["cant_afford_5"];
            Messages.CantAfford25 = (string)jdata["messages"]["shop"]["cant_afford_25"];
            Messages.Brought1Format = (string)jdata["messages"]["shop"]["brought_1"];
            Messages.Brought5Format = (string)jdata["messages"]["shop"]["brought_5"];
            Messages.Brought25Format = (string)jdata["messages"]["shop"]["brought_25"];
            Messages.Sold1Format = (string)jdata["messages"]["shop"]["sold_1"];
            Messages.SoldAllFormat = (string)jdata["messages"]["shop"]["sold_all"];
            Messages.CannotSellYoudGetTooMuchMoney = (string)jdata["messages"]["shop"]["cant_hold_extra_money"];

            Messages.Brought1ButInventoryFull = (string)jdata["messages"]["shop"]["brought_1_but_inv_full"];
            Messages.Brought5ButInventoryFull = (string)jdata["messages"]["shop"]["brought_5_but_inv_full"];
            Messages.Brought25ButInventoryFull = (string)jdata["messages"]["shop"]["brought_25_but_inv_full"];

            // Player List
            Messages.PlayerListHeader = (string)jdata["messages"]["meta"]["player_list"]["playerlist_header"];
            Messages.PlayerListSelectFromFollowing = (string)jdata["messages"]["meta"]["player_list"]["select_from_following"];
            Messages.PlayerListOfBuddiesFormat = (string)jdata["messages"]["meta"]["player_list"]["list_of_buddies_format"];
            Messages.PlayerListOfNearby = (string)jdata["messages"]["meta"]["player_list"]["list_of_players_nearby"];
            Messages.PlayerListOfPlayersFormat = (string)jdata["messages"]["meta"]["player_list"]["list_of_all_players_format"];
            Messages.PlayerListOfPlayersAlphabetically = (string)jdata["messages"]["meta"]["player_list"]["list_of_all_players_alphabetically"];
            Messages.PlayerListMapAllBuddiesForamt = (string)jdata["messages"]["meta"]["player_list"]["map_all_buddies_format"];
            Messages.PlayerListMapAllPlayersFormat = (string)jdata["messages"]["meta"]["player_list"]["map_all_players_format"];
            Messages.PlayerListAbuseReport = (string)jdata["messages"]["meta"]["player_list"]["abuse_report"];

            Messages.MuteButton = (string)jdata["messages"]["meta"]["player_list"]["mute_button"];
            Messages.HearButton = (string)jdata["messages"]["meta"]["player_list"]["hear_button"];

            Messages.ThreeMonthSubscripitionIcon = (int)jdata["messages"]["meta"]["player_list"]["icon_subbed_3month"];
            Messages.YearSubscriptionIcon = (int)jdata["messages"]["meta"]["player_list"]["icon_subbed_year"];
            Messages.NewUserIcon = (int)jdata["messages"]["meta"]["player_list"]["icon_new"];
            Messages.MonthSubscriptionIcon = (int)jdata["messages"]["meta"]["player_list"]["icon_subbed_month"];
            Messages.AdminIcon = (int)jdata["messages"]["meta"]["player_list"]["icon_admin"];
            Messages.ModeratorIcon = (int)jdata["messages"]["meta"]["player_list"]["icon_mod"];

            Messages.BuddyListHeader = (string)jdata["messages"]["meta"]["player_list"]["online_buddy_header"];
            Messages.BuddyListOnlineBuddyEntryFormat = (string)jdata["messages"]["meta"]["player_list"]["online_buddy_format"];
            Messages.BuddyListOfflineBuddys = (string)jdata["messages"]["meta"]["player_list"]["offline_buddys"];
            Messages.BuddyListOfflineBuddyEntryFormat = (string)jdata["messages"]["meta"]["player_list"]["offline_buddy_format"];

            Messages.NearbyPlayersListHeader = (string)jdata["messages"]["meta"]["player_list"]["nearby_player_header"];
            Messages.PlayerListAllAlphabeticalHeader = (string)jdata["messages"]["meta"]["player_list"]["all_players_alphabetical_header"];

            Messages.PlayerListEntryFormat = (string)jdata["messages"]["meta"]["player_list"]["player_format"];

            Messages.PlayerListIdle = (string)jdata["messages"]["meta"]["player_list"]["idle_text"];
            Messages.PlayerListAllHeader = (string)jdata["messages"]["meta"]["player_list"]["all_players_header"];
            Messages.PlayerListIconFormat = (string)jdata["messages"]["meta"]["player_list"]["icon_format"];
            Messages.PlayerListIconInformation = (string)jdata["messages"]["meta"]["player_list"]["icon_info"];

            // Consume
            Messages.ConsumeItemFormat = (string)jdata["messages"]["consume"]["consumed_item_format"];
            Messages.ConsumedButMaxReached = (string)jdata["messages"]["consume"]["consumed_but_max_reached"];

            // Meta Format
            Messages.LocationFormat = (string)jdata["messages"]["meta"]["location_format"];
            Messages.IsleFormat = (string)jdata["messages"]["meta"]["isle_format"];
            Messages.TownFormat = (string)jdata["messages"]["meta"]["town_format"];
            Messages.AreaFormat = (string)jdata["messages"]["meta"]["area_format"];
            Messages.Seperator = (string)jdata["messages"]["meta"]["seperator"];
            Messages.TileFormat = (string)jdata["messages"]["meta"]["tile_format"];
            Messages.ExitThisPlace = (string)jdata["messages"]["meta"]["exit_this_place"];
            Messages.BackToMap = (string)jdata["messages"]["meta"]["back_to_map"];
            Messages.BackToMapHorse = (string)jdata["messages"]["meta"]["back_to_map_horse"];
            Messages.LongFullLine = (string)jdata["messages"]["meta"]["long_full_line"];
            Messages.MetaTerminator = (string)jdata["messages"]["meta"]["end_of_meta"];

            Messages.PlayersHere = (string)jdata["messages"]["meta"]["player_interaction"]["players_here"];
            Messages.NearbyPlayers = (string)jdata["messages"]["meta"]["nearby"]["players_nearby"];
            Messages.North = (string)jdata["messages"]["meta"]["nearby"]["north"];
            Messages.East = (string)jdata["messages"]["meta"]["nearby"]["east"];
            Messages.South = (string)jdata["messages"]["meta"]["nearby"]["south"];
            Messages.West = (string)jdata["messages"]["meta"]["nearby"]["west"];

            Messages.NoPitchforkMeta = (string)jdata["messages"]["meta"]["hay_pile"]["no_pitchfork"];
            Messages.HasPitchforkMeta = (string)jdata["messages"]["meta"]["hay_pile"]["pitchfork"];
            Messages.R1 = (string)jdata["messages"]["meta"]["r1"];
            Messages.PasswordEntry = (string)jdata["messages"]["meta"]["password_input"];

            // Venus Fly Trap
            Messages.VenusFlyTrapFormat = (string)jdata["messages"]["meta"]["venus_flytrap_format"];

            // Shortcut
            Messages.NoTelescope = (string)jdata["messages"]["no_telescope"];

            // Inn
            Messages.InnBuyMeal = (string)jdata["messages"]["meta"]["inn"]["buy_meal"];
            Messages.InnBuyRest = (string)jdata["messages"]["meta"]["inn"]["buy_rest"];
            Messages.InnItemEntryFormat = (string)jdata["messages"]["meta"]["inn"]["inn_entry"];
            Messages.InnEnjoyedServiceFormat = (string)jdata["messages"]["inn"]["enjoyed_service"];
            Messages.InnCannotAffordService = (string)jdata["messages"]["inn"]["cant_afford"];
            Messages.InnFullyRested = (string)jdata["messages"]["inn"]["fully_rested"];

            // Password
            Messages.IncorrectPasswordMessage = (string)jdata["messages"]["incorrect_password"];

            // Fountain
            Messages.FountainMeta = (string)jdata["messages"]["meta"]["fountain"];
            Messages.FountainDrankYourFull = (string)jdata["messages"]["fountain"]["drank_your_fill"];
            Messages.FountainDroppedMoneyFormat = (string)jdata["messages"]["fountain"]["dropped_money"];

            // Highscore
            Messages.HighscoreHeaderMeta = (string)jdata["messages"]["meta"]["highscores"]["header_meta"];
            Messages.HighscoreFormat = (string)jdata["messages"]["meta"]["highscores"]["highscore_format"];
            Messages.BestTimeFormat = (string)jdata["messages"]["meta"]["highscores"]["besttime_format"];

            Messages.GameHighScoreHeaderFormat = (string)jdata["messages"]["meta"]["highscores"]["game_highscore_header"];
            Messages.GameHighScoreFormat = (string)jdata["messages"]["meta"]["highscores"]["game_highscore_format"];

            Messages.GameWinLooseHeaderFormat = (string)jdata["messages"]["meta"]["highscores"]["game_winloose_header"];
            Messages.GameWinLooseFormat = (string)jdata["messages"]["meta"]["highscores"]["game_winloose_format"];

            Messages.GameBestTimeHeaderFormat = (string)jdata["messages"]["meta"]["highscores"]["game_besttime_header"];
            Messages.GameBestTimeFormat = (string)jdata["messages"]["meta"]["highscores"]["game_besttime_format"];

            // Awards
            Messages.AwardHeader = (string)jdata["messages"]["meta"]["awards_page"]["awards_header"];
            Messages.AwardOthersFormat = (string)jdata["messages"]["meta"]["awards_page"]["awards_others_header"];
            Messages.NoAwards = (string)jdata["messages"]["meta"]["awards_page"]["no_awards"];
            Messages.AwardFormat = (string)jdata["messages"]["meta"]["awards_page"]["award_format"];

            // World Peace
            Messages.NoWishingCoins = (string)jdata["messages"]["meta"]["wishing_well"]["no_coins"];
            Messages.YouHaveWishingCoinsFormat = (string)jdata["messages"]["meta"]["wishing_well"]["wish_coins"];
            Messages.WishItemsFormat = (string)jdata["messages"]["meta"]["wishing_well"]["wish_things"];
            Messages.WishMoneyFormat = (string)jdata["messages"]["meta"]["wishing_well"]["wish_money"];
            Messages.WishWorldPeaceFormat = (string)jdata["messages"]["meta"]["wishing_well"]["wish_worldpeace"];

            Messages.TossedCoin = (string)jdata["messages"]["meta"]["wishing_well"]["make_wish"];
            Messages.WorldPeaceOnlySoDeep = (string)jdata["messages"]["meta"]["wishing_well"]["world_peace_message"];
            Messages.WishingWellMeta = (string)jdata["messages"]["meta"]["wishing_well"]["wish_meta"];
            // Sec Codes

            Messages.InvalidSecCodeError = (string)jdata["messages"]["sec_code"]["invalid_sec_code"];
            Messages.YouEarnedAnItemFormat = (string)jdata["messages"]["sec_code"]["item_earned"];
            Messages.YouEarnedAnItemButInventoryWasFullFormat = (string)jdata["messages"]["sec_code"]["item_earned_full_inv"];
            Messages.YouLostAnItemFormat = (string)jdata["messages"]["sec_code"]["item_deleted"];
            Messages.YouEarnedMoneyFormat = (string)jdata["messages"]["sec_code"]["money_earned"];
            Messages.BeatHighscoreFormat = (string)jdata["messages"]["sec_code"]["highscore_beaten"];
            Messages.BeatBestHighscore = (string)jdata["messages"]["sec_code"]["best_highscore_beaten"];
            Messages.BeatBestTimeFormat = (string)jdata["messages"]["sec_code"]["best_time_beaten"];

            // Inventory

            Messages.InventoryHeaderFormat = (string)jdata["messages"]["meta"]["inventory"]["header_format"];
            Messages.InventoryItemFormat = (string)jdata["messages"]["meta"]["inventory"]["item_entry"];
            Messages.ShopEntryFormat = (string)jdata["messages"]["meta"]["inventory"]["shop_entry"];
            Messages.ItemInformationButton = (string)jdata["messages"]["meta"]["inventory"]["item_info_button"];
            Messages.ItemInformationByIdButton = (string)jdata["messages"]["meta"]["inventory"]["item_info_itemid_button"];

            Messages.ItemDropButton = (string)jdata["messages"]["meta"]["inventory"]["item_drop_button"];
            Messages.ItemThrowButton = (string)jdata["messages"]["meta"]["inventory"]["item_throw_button"];
            Messages.ItemConsumeButton = (string)jdata["messages"]["meta"]["inventory"]["item_consume_button"];
            Messages.ItemUseButton = (string)jdata["messages"]["meta"]["inventory"]["item_use_button"];
            Messages.ItemOpenButton = (string)jdata["messages"]["meta"]["inventory"]["item_open_button"];
            Messages.ItemWearButton = (string)jdata["messages"]["meta"]["inventory"]["item_wear_button"];
            Messages.ItemReadButton = (string)jdata["messages"]["meta"]["inventory"]["item_read_button"];

            Messages.ShopBuyButton = (string)jdata["messages"]["meta"]["inventory"]["buy_button"];
            Messages.ShopBuy5Button = (string)jdata["messages"]["meta"]["inventory"]["buy_5_button"];
            Messages.ShopBuy25Button = (string)jdata["messages"]["meta"]["inventory"]["buy_25_button"];

            Messages.SellButton = (string)jdata["messages"]["meta"]["inventory"]["sell_button"];
            Messages.SellAllButton = (string)jdata["messages"]["meta"]["inventory"]["sell_all_button"];
            // Npc

            Messages.NpcStartChatFormat = (string)jdata["messages"]["meta"]["npc"]["start_chat_format"];
            Messages.NpcNoChatpoints = (string)jdata["messages"]["meta"]["npc"]["no_chatpoints"];
            Messages.NpcChatpointFormat = (string)jdata["messages"]["meta"]["npc"]["chatpoint_format"];
            Messages.NpcReplyFormat = (string)jdata["messages"]["meta"]["npc"]["reply_format"];
            Messages.NpcTalkButton = (string)jdata["messages"]["meta"]["npc"]["npc_talk_button"];
            Messages.NpcInformationButton = (string)jdata["messages"]["meta"]["npc"]["npc_information_button"];
            Messages.NpcInformationFormat = (string)jdata["messages"]["meta"]["npc"]["npc_information_format"];

            // Login Failed Reasons
            Messages.LoginFailedReasonBanned = (string)jdata["messages"]["login"]["banned"];
            Messages.LoginFailedReasonBannedIpFormat = (string)jdata["messages"]["login"]["ip_banned"];

            // Disconnect Reasons

            Messages.KickReasonKicked = (string)jdata["messages"]["disconnect"]["kicked"];
            Messages.KickReasonBanned = (string)jdata["messages"]["disconnect"]["banned"];
            Messages.KickReasonIdleFormat = (string)jdata["messages"]["disconnect"]["client_timeout"]["kick_message"];
            Messages.KickReasonNoTime = (string)jdata["messages"]["disconnect"]["no_playtime"];
            Messages.IdleWarningFormat = (string)jdata["messages"]["disconnect"]["client_timeout"]["warn_message"];
            Messages.KickReasonDuplicateLogin = (string)jdata["messages"]["disconnect"]["dupe_login"];

            // Competition Gear

            Messages.EquipCompetitionGearFormat = (string)jdata["messages"]["equips"]["equip_competition_gear_format"];
            Messages.RemoveCompetitionGear = (string)jdata["messages"]["equips"]["removed_competition_gear"];

            // Jewerly
            Messages.EquipJewelryFormat = (string)jdata["messages"]["equips"]["equip_jewelry"];
            Messages.MaxJewelryMessage = (string)jdata["messages"]["equips"]["max_jewelry"];
            Messages.RemoveJewelry = (string)jdata["messages"]["equips"]["removed_jewelry"];

            // Click
            Messages.NothingInterestingHere = (string)jdata["messages"]["click_nothing_message"];

            // Swf files
            Messages.WagonCutscene = (string)jdata["transport"]["wagon_cutscene"];
            Messages.BoatCutscene = (string)jdata["transport"]["boat_cutscene"];
            Messages.BallonCutscene = (string)jdata["transport"]["ballon_cutscene"];

            // free json data
            jdata = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return;
        }

    }
}
