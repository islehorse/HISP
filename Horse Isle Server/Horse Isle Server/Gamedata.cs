using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Horse_Isle_Server
{
    class Gamedata
    {
        
        public static void ReadGamedata()
        {
            if(!File.Exists(ConfigReader.GameDataFile))
            {
                Logger.ErrorPrint("Game Data JSON File: " + ConfigReader.GameDataFile + " Does not exist!");
                return;
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

            // Register Areas
            int totalAreas = gameData.places.towns.Count;
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
                    effectsList[ii].EffectsWhat = gameData.item.item_list[i].effects[ii].effect_amount;
                }

                item.Effects = effectsList;
                item.SpawnParamaters = new Item.SpawnRules();
                item.SpawnParamaters.SpawnCap = gameData.item.item_list[i].spawn_parameters.spawn_cap;
                item.SpawnParamaters.SpawnInArea = gameData.item.item_list[i].spawn_parameters.spawn_in_area;
                item.SpawnParamaters.SpawnOnTileType = gameData.item.item_list[i].spawn_parameters.spawn_on_tile_type;
                item.SpawnParamaters.SpawnOnSpecialTile = gameData.item.item_list[i].spawn_parameters.spawn_on_special_tile;
                item.SpawnParamaters.SpawnNearSpecialTile = gameData.item.item_list[i].spawn_parameters.spawn_near_special_tile;

                Logger.DebugPrint("Registered Item ID: " + item.Id + " Name: " + item.Name + " spawns on: "+item.SpawnParamaters.SpawnOnTileType);
                Item.Items.Add(item);
            }


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

            // Meta Format

            Messages.LocationFormat = gameData.messages.meta.location_format;
            Messages.IsleFormat = gameData.messages.meta.isle_format;
            Messages.TownFormat = gameData.messages.meta.town_format;
            Messages.AreaFormat = gameData.messages.meta.area_format;
            Messages.Seperator = gameData.messages.meta.seperator;
            Messages.TileFormat = gameData.messages.meta.tile_format;
            Messages.TransportFormat = gameData.messages.meta.transport_format;
            Messages.InventoryFormat = gameData.messages.meta.inventory_format;
            Messages.ExitThisPlace = gameData.messages.meta.exit_this_place;
            Messages.BackToMap = gameData.messages.meta.back_to_map;
            Messages.LongFullLine = gameData.messages.meta.long_full_line;
            Messages.MetaTerminator = gameData.messages.meta.end_of_meta;

            Messages.NothingMessage = gameData.messages.meta.dropped_items.nothing_message;
            Messages.ItemsOnGroundMessage = gameData.messages.meta.dropped_items.items_message;
            Messages.GrabItemFormat = gameData.messages.meta.dropped_items.item_format;
            Messages.GrabbedItemMessage = gameData.messages.grab_message;
            Messages.GrabAllItemsMessage = gameData.messages.grab_all_message;

            Messages.NearbyPlayers = gameData.messages.meta.nearby.players_nearby;
            Messages.North = gameData.messages.meta.nearby.north;
            Messages.East = gameData.messages.meta.nearby.east;
            Messages.South = gameData.messages.meta.nearby.south;
            Messages.West = gameData.messages.meta.nearby.west;

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

            Chat.PrivateMessageSound = gameData.messages.chat.pm_sound;

            Server.IdleWarning = gameData.messages.disconnect.client_timeout.warn_after;
            Server.IdleTimeout = gameData.messages.disconnect.client_timeout.kick_after;

            // Inventory

            Messages.DefaultInventoryMax = gameData.item.max_carryable;

            // Swf
            Messages.WagonCutscene = gameData.transport.wagon_cutscene;
            Messages.BoatCutscene = gameData.transport.boat_cutscene;
            Messages.BallonCutscene = gameData.transport.ballon_cutscene;

        }

    }
}
