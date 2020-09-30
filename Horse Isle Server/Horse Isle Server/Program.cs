using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace Horse_Isle_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Bitmap mbmp = new Bitmap("MapData.bmp");
            Bitmap bmp = new Bitmap("oMapData.bmp");
            Console.WriteLine(bmp.PixelFormat);
            Bitmap bbmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            for(int x = 0; x < bmp.Width; x++)
            {
                for(int y = 0; y < bmp.Height; y++)
                {
                    Color col = mbmp.GetPixel(x, y);
                    Color col2 = bmp.GetPixel(x, y);

                    bbmp.SetPixel(x,y,Color.FromArgb(col2.B, 0, col.B));
                    
                }
            }
            bbmp.Save("MapData2.bmp", ImageFormat.Bmp);
            Console.WriteLine("image checked");
            */



            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            ConfigReader.OpenConfig();
            CrossDomainPolicy.GetPolicy();
            Database.OpenDatabase();
            Map.OpenMap();
            Gamedata.ReadGamedata();
            Server.StartServer();

        }
    }
}
