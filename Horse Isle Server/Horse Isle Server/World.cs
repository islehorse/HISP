using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{

    class World
    {
        public struct Isle
        {
            public int StartX;
            public int EndX;
            public int StartY;
            public int EndY;
            public int Tileset;
            public string Name;
        }
        public struct Town
        {
            public int StartX;
            public int EndX;
            public int StartY;
            public int EndY;
            public string Name;
        }
        public struct Area
        {
            public int StartX;
            public int EndX;
            public int StartY;
            public int EndY;
            public string Name;
        }


        public struct Time
        {
            public int minutes;
            public int hours;
            public int days;
            public int year;
        }
        public const int MINUTE = 4320;

        public static List<Isle> Isles = new List<Isle>();
        public static List<Town> Towns = new List<Town>();
        public static List<Area> Areas = new List<Area>();
        public static Time GetGameTime()
        {
            int epoch = Database.GetServerCreationTime();
            DateTime serverCreationTime = DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
            DateTime currentTime = DateTime.Now;

            TimeSpan difference = (currentTime.Date - currentTime.Date);

            int totalMilis = Convert.ToInt32(difference.TotalMilliseconds);

            
            int gameMinutes = totalMilis / MINUTE;
            int gameHours = (totalMilis / MINUTE * 600);
            int gameDays = (totalMilis / (MINUTE * 60) * 24);
            int gameYears = ((totalMilis / (MINUTE * 60) * 24)*365);

            Time time = new Time();
            time.days = gameDays;
            time.year = gameYears;
            time.minutes = gameMinutes;
            time.hours = gameHours;

            return time;
        }

        public static bool InArea(int x, int y)
        {
            try
            {
                GetArea(x, y);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public static bool InTown(int x, int y)
        {
            try
            {
                GetTown(x, y);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }


        public static bool InIsle(int x, int y)
        {
            try
            {
                GetIsle(x, y);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }
        public static Isle GetIsle(int x, int y)
        {
            foreach(Isle isle in Isles)
            {

                if (isle.StartX <= x && isle.EndX >= x && isle.StartY <= y && isle.EndY >= y)
                {
                    return isle;
                }
            }
            throw new KeyNotFoundException("x,y not in an isle!");
        }

        public static Area GetArea(int x, int y)
        {
            foreach (Area area in Areas)
            {

                if (area.StartX <= x && area.EndX >= x && area.StartY <= y && area.EndY >= y)
                {
                    return area;
                }
            }
            throw new KeyNotFoundException("x,y not in an area!");
        }

        public static Town GetTown(int x, int y)
        {
            foreach (Town town in Towns)
            {

                if (town.StartX <= x && town.EndX >= x && town.StartY <= y && town.EndY >= y)
                {
                    return town;
                }
            }
            throw new KeyNotFoundException("x,y not in a town!");
        }

        public static int[] GetDroppedItems(int x, int y)
        {
            return new int[] { }; // Not implemented yet.
        }
        public static string GetWeather()
        {
            return Database.GetWorldWeather();
        }
    }
}
