using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            Messages.NothingMessage = gameData.messages.meta.nothing_message;

            // Map Data

            Map.OverlayTileDepth = gameData.tile_paramaters.overlay_tiles.tile_depth.ToObject<int[]>();
            Map.OverlayTilesetPassibility = gameData.tile_paramaters.overlay_tiles.passibility.ToObject<bool[][]>();
            Map.TerrainTilePassibility = gameData.tile_paramaters.terrain_tiles.passibility.ToObject<bool[]>();

            // Disconnect Reasons

            Messages.BanMessage = gameData.messages.disconnect.banned;
            Messages.IdleKickMessageFormat = gameData.messages.disconnect.client_timeout.kick_message;
            Messages.IdleWarningFormat = gameData.messages.disconnect.client_timeout.warn_message;


            Server.IdleWarning = gameData.messages.disconnect.client_timeout.warn_after;
            Server.IdleTimeout = gameData.messages.disconnect.client_timeout.kick_after;

        }

    }
}
