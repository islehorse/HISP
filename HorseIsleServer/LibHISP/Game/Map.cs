using System;
using System.IO;
using HISP.Server;

namespace HISP.Game
{
    public class Map
    {

        public const byte BLANK_TILE = 1;
        public const byte RANCH_TILE = 170;
        public const byte POT_OF_GOLD_TILE = 186;
        public const byte BURRIED_TREASURE_TILE = 193;

        public struct TerrainTile
        {
            public bool Passable;
            public string Type;
        }

        public struct TileDepth
        {
            public bool ShowPlayer;
            public bool Passable;
        }

        public static TileDepth[] OverlayTileDepth;
        public static TerrainTile[] TerrainTiles;

        public static int Width;
        public static int Height;

        public static byte[] MapData;
        public static byte[] oMapData;


        public static int NewUserStartX;
        public static int NewUserStartY;

        public static int ModIsleX;
        public static int ModIsleY;

        public static int RulesIsleX;
        public static int RulesIsleY;

        public static int PrisonIsleX;
        public static int PrisonIsleY;


        public static int GetTileId(int x, int y, bool overlay)
        {
            int pos = ((x * Height) + y);

            Logger.DebugPrint("Get tile at pos: " + x + ", " + y + " overlay: " + overlay);

            if (x < 0 || x >= Width)
                return BLANK_TILE;
            if (y < 0 || y >= Height)
                return BLANK_TILE;

            if ((pos <= 0 || pos >= oMapData.Length) && overlay)
                return BLANK_TILE;
            else if ((pos <= 0 || pos >= MapData.Length) && !overlay) 
                return BLANK_TILE;

            else if (overlay && Treasure.IsTileBuiredTreasure(x, y))
                return BURRIED_TREASURE_TILE; // Burried Treasure tile.
            else if (overlay && Treasure.IsTilePotOfGold(x, y))
                return POT_OF_GOLD_TILE; // Pot of Gold tile.
            else if (overlay && Ranch.IsRanchHere(x, y))
            {
                int upgradeLevel = Ranch.GetRanchAt(x, y).UpgradedLevel;
                if (upgradeLevel > 7)
                    upgradeLevel = 7;
                return RANCH_TILE + upgradeLevel; // ranch tile
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
                if (World.InIsle(x, y)) tileset = World.GetIsle(x, y).Tileset;
                otileId = otileId + 64 * tileset;
            }

            bool overlayPassable = OverlayTileDepth[otileId].Passable;

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
            int pos = 8;

            for (int i = 0; i < MapData.Length; i++)
            {
                oMapData[i] = worldMap[pos];
                MapData[i] = worldMap[pos+ 1];
                pos += 2;
            }

            worldMap = null;
            Logger.InfoPrint("Map Data Loaded!");

        }
    }
}
   