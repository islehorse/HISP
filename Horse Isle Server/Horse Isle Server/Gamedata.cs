using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Horse_Isle_Server
{



    class Gamedata
    {
        public static string TileFlags;
        
        public static int NewUserStartX;
        public static int NewUserStartY;
        // Messages
        public static string NewUserMessage;
        public static string AreaMessage;
        public static string NothingMessage;
        public static string LoginMessage;
        
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

            NewUserMessage = gameData.new_user.starting_message;
            NewUserStartX = gameData.new_user.starting_x;
            NewUserStartY = gameData.new_user.starting_y;

            LoginMessage = gameData.messages.login_format;
            AreaMessage = gameData.messages.area_format;
            NothingMessage = gameData.messages.nothing_message;

            TileFlags = gameData.map_flags;
        }

    }
}
