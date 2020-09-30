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
        public static int[] OverlayTileDepth;
        public static bool[] TerrainTilePassibility;

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
            bool passable = TerrainTilePassibility[tileId-1];
            Logger.DebugPrint("Checking tile passibility for tileid: " + tileId + " at " + x + "," + y+" passable: " +passable);
            return passable;
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
   