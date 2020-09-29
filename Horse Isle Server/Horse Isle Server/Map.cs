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
        public static Bitmap MapData;
        public static Bitmap oMapData;
        public static int GetTileId(int x, int y, bool overlay)
        {
            if ((x > MapData.Width || x < 0) || (y > MapData.Height || y < 0)) // Outside map?
                return 0x01;

            if(overlay)
                return oMapData.GetPixel(x,y).ToArgb() & Convert.ToInt32(0xFF000000);
            else
                return MapData.GetPixel(x, y).ToArgb() & Convert.ToInt32(0xFF000000);
        }

        public static void OpenMap()
        {
            if(!File.Exists(ConfigReader.MapFile) || !File.Exists(ConfigReader.OverlayMapFile))
            {
                Logger.ErrorPrint("Map file not found.");
                return;
            }

            MapData = new Bitmap(ConfigReader.MapFile);
            oMapData = new Bitmap(ConfigReader.MapFile);
        }
    }
}
