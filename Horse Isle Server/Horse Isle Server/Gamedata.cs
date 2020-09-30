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

            int totalIsles = gameData.isles.Count;
            for(int i = 0; i < totalIsles; i++)
            {

                World.Isle isle = new World.Isle();
                isle.StartX = gameData.isles[i].start_x;
                isle.StartY = gameData.isles[i].start_y;
                isle.EndX = gameData.isles[i].end_x;
                isle.EndY = gameData.isles[i].end_y;
                isle.Tileset = gameData.isles[i].tileset;
                isle.Name = gameData.isles[i].name;

                Logger.DebugPrint("Registered Isle: " + isle.Name + " X " + isle.StartX + "-" + isle.EndX + " Y " + isle.StartY + "-" + isle.EndY + " tileset: " + isle.Tileset);
                World.Isles.Add(isle);
            }

            Messages.NewUserMessage = gameData.new_user.starting_message;
            Map.NewUserStartX = gameData.new_user.starting_x;
            Map.NewUserStartY = gameData.new_user.starting_y;

            Messages.LoginFormat = gameData.messages.login_format;
            Messages.AreaMessage = gameData.messages.area_format;
            Messages.NothingMessage = gameData.messages.nothing_message;
            Messages.MotdFormat = gameData.messages.motd_format;

            JArray overlayTileDepth = gameData.tile_paramaters.overlay_tiles.tile_depth;
            JArray terrainTilePassibility = gameData.tile_paramaters.terrain_tiles.passibility;
            Map.OverlayTileDepth = overlayTileDepth.ToObject<int[]>();
            Map.TerrainTilePassibility = terrainTilePassibility.ToObject<bool[]>();
        }

    }
}
