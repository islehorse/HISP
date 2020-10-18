using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Map
    {
        public struct TerrainTile
        {
            public bool Passable;
            public string Type;
        }

        public static int[] OverlayTileDepth;

        public static TerrainTile[] TerrainTiles;
        public static bool[] OverlayTilesetPassibility;

        public static Bitmap MapData;
        

        public static int NewUserStartX;
        public static int NewUserStartY;
        public static int GetTileId(int x, int y, bool overlay)
        {
            if ((x > MapData.Width || x < 0) || (y > MapData.Height || y < 0)) // Outside map?
                return 0x1;
                

            if (overlay)
                return MapData.GetPixel(x, y).R;
            else
                return MapData.GetPixel(x, y).B;
        }
        public static bool CheckPassable(int x, int y)
        {
            int tileId = GetTileId(x, y, false);
            int otileId = GetTileId(x, y, true);

            bool terrainPassable = TerrainTiles[tileId - 1].Passable;
            int tileset = 0;

            if (otileId > 190)
            {
                otileId -= 192;
                if (World.InIsle(x, y))
                    tileset = World.GetIsle(x, y).Tileset+1;
            }
            
            otileId = (otileId - 1) * (tileset+1);

            bool overlayPassable = OverlayTilesetPassibility[otileId];

            bool tilePassable = false;
            if (terrainPassable || overlayPassable)
                tilePassable = true;
            if (!overlayPassable && otileId != 1)
                tilePassable = false;


            Logger.DebugPrint("Checking tile passibility for tileid: " + tileId + " and overlay tileid " + otileId + " at " + x + "," + y);
            return tilePassable;
        }

        public static void OpenMap()
        {
            if(!File.Exists(ConfigReader.MapFile))
            {
                Logger.ErrorPrint("Map file not found.");
                return;
            }

            MapData = new Bitmap(ConfigReader.MapFile);
        }
    }
}
   