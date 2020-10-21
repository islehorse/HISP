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

            bool tilePassable = false;
            if (terrainPassable || overlayPassable)
                tilePassable = true;
            if (!overlayPassable && (tileId != 0 && otileId != 0))
                tilePassable = false;


           
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
   