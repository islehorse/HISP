using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Horse_Isle_Server
{

    internal class Isle
    {
        public int StartX = 0;
        public int EndX = 0;
        public int StartY = 0;
        public int EndY = 0;
        public int Tileset = 0;
        public string Name;
        public Isle(int startx, int starty, int endx, int endy, int tileset, string name)
        {
            StartX = startx;
            StartY = starty;
            EndX = endx;
            EndY = endy;
            Tileset = tileset;
            Name = name;
        }
    }

    class Gamedata
    {
        public static List<Isle> Isles = new List<Isle>();
        
        public static int NewUserStartX;
        public static int NewUserStartY;
        public static string NewUserMessage;
        
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

                int startx = gameData.isles[i].start_x;
                int starty = gameData.isles[i].start_y;
                int endx = gameData.isles[i].end_x;
                int endy = gameData.isles[i].end_y;
                int tileset = gameData.isles[i].tileset;
                string name = gameData.isles[i].name;

                Logger.DebugPrint("Registered Isle: " + name + " X " + startx + "-" + endx + " Y " + starty + "-" + endy + " tileset: " + tileset);
                Isles.Add(new Isle(startx, starty, endx, endy, tileset, name));
            }

            NewUserMessage = gameData.new_user.starting_message;
            Logger.DebugPrint("New User Message: " + NewUserMessage);
            NewUserStartX = gameData.new_user.starting_x;
            Logger.DebugPrint("New User Start X: " + NewUserStartX);
            NewUserStartY = gameData.new_user.starting_y;
            Logger.DebugPrint("New User Start Y: " + NewUserStartY);


        }

    }
}
