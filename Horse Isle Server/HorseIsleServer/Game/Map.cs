using System;
using System.IO;
using HISP.Server;

namespace HISP.Game
{
    public class Map
    {
        public struct TerrainTile
        {
            public bool Passable;
            public string Type;
        }

        public static int[] OverlayTileDepth;

        public static int Width;
        public static int Height;

        public static byte[] MapData;
        public static byte[] oMapData;

        public static TerrainTile[] TerrainTiles;
        

        public static int NewUserStartX;
        public static int NewUserStartY;

        public static int ModIsleX;
        public static int ModIsleY;

        public static int RulesIsleX;
        public static int RulesIsleY;

        public static int GetTileId(int x, int y, bool overlay)
        {
            int pos = ((x * Height) + y);

            if ((pos <= 0 || pos >= oMapData.Length) && overlay)
                return 1;
            else if ((pos <= 0 || pos >= MapData.Length) && !overlay) 
                return 1;
            else if (overlay && Treasure.IsTileBuiredTreasure(x, y))
                return 193; // Burried Treasure tile.
            else if (overlay && Treasure.IsTilePotOfGold(x, y))
                return 186; // Pot of Gold tile.
            else if (overlay && Ranch.IsRanchHere(x, y))
            {
                int upgradeLevel = Ranch.GetRanchAt(x, y).UpgradedLevel;
                if (upgradeLevel > 7)
                    upgradeLevel = 7;
                return 170 + upgradeLevel;
            }
            else if (overlay)
                return oMapData[pos];
            else if (!overlay)
                return MapData[pos];
            else // Not sure how you could even get here.
                return 1;
        }
        public static bool CheckPassable(int x, int y)
        {
            int tileId = GetTileId(x, y, false) - 1;
            int otileId = GetTileId(x, y, true) - 1;

            bool terrainPassable = TerrainTiles[tileId].Passable;
            int tileset = 0;


            if (otileId > 192)
            {
                if (World.InIsle(x, y))
                    tileset = World.GetIsle(x, y).Tileset;
                otileId = otileId + 64 * tileset;
            }
            

            int tileDepth = OverlayTileDepth[otileId];
            bool overlayPassable = false;
            if (tileDepth == 0)
                overlayPassable = false;
            if (tileDepth == 1)
                overlayPassable = false;
            if (tileDepth == 2)
                overlayPassable = true;
            if (tileDepth == 3)
                overlayPassable = true;

            if ((!terrainPassable && overlayPassable) && otileId == 0)
                return false;

            bool passable = false;
            if (!overlayPassable)
                passable = false;
            if (!terrainPassable)
                passable = false;

            if (!passable && overlayPassable)
                passable = true;

            return passable;
        }

        public static void OpenMap()
        {
            if(!File.Exists(ConfigReader.MapFile))
            {
                Logger.ErrorPrint("Map file not found.");
                return;
            }
            Logger.InfoPrint("Loading Map Data (" + ConfigReader.MapFile + ")");

            byte[] worldMap = File.ReadAllBytes(ConfigReader.MapFile);

            Width = BitConverter.ToInt32(worldMap, 0);
            Height = BitConverter.ToInt32(worldMap, 4);
            
            MapData = new byte[Width * Height];
            oMapData = new byte[Width * Height];
            int ii = 8;

            for (int i = 0; i < MapData.Length; i++)
            {
                oMapData[i] = worldMap[ii];
                MapData[i] = worldMap[ii+ 1];
                ii += 2;
            }

            worldMap = null;
            Logger.InfoPrint("Map Data Loaded!");

        }
    }
}
   