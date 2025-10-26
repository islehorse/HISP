using HISP.Player;
using HISP.Server;
using HISP.Util;
using System;
using System.Linq;

namespace HISP.Game
{

    public class World
    {
        public static bool IsPointOnScreen(int screenX, int screenY, int pointX, int pointY)
        {
            int startX = screenX - 9;
            int endX = screenX + 9;
            int startY = screenY - 8;
            int endY = screenY + 9;
            if (startX <= pointX && endX >= pointX && startY <= pointY && endY >= pointY)
                return true;
            else
                return false;
        }
        public static Waypoint GetWaypoint(string find)
        {
            return Waypoints.First(o => o.Name.Equals(find, StringComparison.InvariantCultureIgnoreCase));        
        }
        public static Waypoint GetWaypointStartsWith(string find)
        {
            return Waypoints.Where(o => o.Name.StartsWith(find, StringComparison.InvariantCultureIgnoreCase)).First();
        }

        public struct Waypoint
        {
            public string Name;
            public int PosX;
            public int PosY;
            public string Type;
            public string Description;
            public string[] WeatherTypesAvalible;

        }
        public class Isle 
        {
            
            public int StartX;
            public int EndX;
            public int StartY;
            public int EndY;
            public int Tileset;
            public string Name;
            public string SelectRandomWeather()
            {
                Waypoint point;
                try
                {
                    point = this.Waypoint;
                }
                catch (InvalidOperationException)
                {
                    return "SUNNY";
                }
                int intWeatherType = GameServer.RandomNumberGenerator.Next(0, point.WeatherTypesAvalible.Length);
                string weather = point.WeatherTypesAvalible[intWeatherType];
                return weather;
            }

            public string Weather
            {
                get
                {
                    if (!Database.WeatherExists(Name))
                    {
                        string weather = SelectRandomWeather();
                        Database.InsertWeather(Name, weather);
                        return weather;
                    }
                    else
                    {
                        return Database.GetWeather(Name);
                    }
                }
                set
                {
                    Database.SetWeather(Name, value);
                    foreach(User user in User.GetUsersInIsle(this,true,true))
                    {
                        GameServer.UpdateWorld(user.Client);
                    }
                }
            }
            
            public Waypoint Waypoint
            {
                get
                {
                    return GetWaypoint(this.Name);
                }
            }

        }
        public class Town
        {
            public int StartX;
            public int EndX;
            public int StartY;
            public int EndY;
            public string Name;
            public string SelectRandomWeather()
            {
                Waypoint point;
                try
                {
                    point = this.Waypoint;
                }
                catch (InvalidOperationException)
                {
                    return "SUNNY";
                }
                int intWeatherType = GameServer.RandomNumberGenerator.Next(0, point.WeatherTypesAvalible.Length);
                string weather = point.WeatherTypesAvalible[intWeatherType];
                return weather;
            }
            public string Weather
            {
                get
                {
                    if (!Database.WeatherExists(Name))
                    {
                        string weather = SelectRandomWeather();
                        Database.InsertWeather(Name, weather);
                        return weather;
                    }
                    else
                    {
                        return Database.GetWeather(Name);
                    }
                }
                set
                {
                    Database.SetWeather(Name, value);
                    foreach (User user in User.GetUsersInTown(this, true, true))
                    {
                        GameServer.UpdateArea(user.Client);
                    }
                }

            }
            public Waypoint Waypoint
            {
                get
                {
                    return GetWaypoint(this.Name);
                }
            }
         }
        public struct Area
        {
            public int StartX;
            public int EndX;
            public int StartY;
            public int EndY;
            public string Name;
        }

        public struct Zone
        {
            public int StartX;
            public int EndX;
            public int StartY;
            public int EndY;
            public string Name;
        }


        public class Time
        {
            public int Minutes;
            public int Days;
            public int Years;
        }

        public struct SpecialTile
        {
            public int X;
            public int Y;
            
            public string Title;
            public string Description;

            public string Code;
            public int ExitX;
            public int ExitY;
            
            public string AutoplaySwf;
            public string TypeFlag;
        }
        public static Time ServerTime = new Time();
        public static int StartDate;

        public static ThreadSafeList<Waypoint> Waypoints = new ThreadSafeList<Waypoint>();
        public static ThreadSafeList<Isle> Isles = new ThreadSafeList<Isle>();
        public static ThreadSafeList<Town> Towns = new ThreadSafeList<Town>();
        public static ThreadSafeList<Area> Areas = new ThreadSafeList<Area>();
        public static ThreadSafeList<Zone> Zones = new ThreadSafeList<Zone>();

        public static ThreadSafeList<SpecialTile> SpecialTiles = new ThreadSafeList<SpecialTile>();

        public static void TickWorldClock() 
        {
            ServerTime.Minutes++;


            if (ServerTime.Minutes > 1440) // 1 day
            {
                ServerTime.Days += 1;
                ServerTime.Minutes = 0;
                
                Database.DoIntrestPayments(ConfigReader.IntrestRate);
            }

            if (ServerTime.Days > 365)  // 1 year!
            {
                ServerTime.Days = 0;
                ServerTime.Years += 1;
            }
        }

        public static void ReadWorldData()
        {
            Logger.DebugPrint("Reading time from database...");
            ServerTime.Minutes = Database.GetServerTime();
            ServerTime.Days = Database.GetServerDay();
            ServerTime.Years = Database.GetServerYear();
            StartDate = Database.GetServerStartTime();
            Logger.InfoPrint("It is " + ServerTime.Minutes / 60 + ":" + ServerTime.Minutes % 60 + " on Day " + ServerTime.Days + " in Year " + ServerTime.Years + "!!!");

        }

        public static bool InZone(int x, int y)
        {
            return Zones.Any(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }
        public static bool InArea(int x, int y)
        {
            return Areas.Any(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }

        public static bool InTown(int x, int y)
        {
            return Towns.Any(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }

        public static bool InSpecialTile(int x, int y)
        {
            return SpecialTiles.Any(o => (o.X == x && o.Y == y));
        }

        public static bool InIsle(int x, int y)
        {
            return Isles.Any(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }
        public static Zone GetZoneByName(string name)
        {
            return Zones.First(o => o.Name == name);
        }
        public static SpecialTile[] GetSpecialTilesByCode(string code)
        {
            return SpecialTiles.Where(o => o.Code == code).ToArray();
        }
        public static SpecialTile[] GetSpecialTilesByName(string name)
        {
            return SpecialTiles.Where(o => o.Title == name).ToArray();
        }
        public static SpecialTile GetSpecialTile(int x, int y)
        {
            return SpecialTiles.First(o => (o.X == x && o.Y == y));
        }
        public static Isle GetIsle(int x, int y)
        {
            return Isles.First(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }

        public static Zone GetZone(int x, int y)
        {
            return Zones.First(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }
        public static Area GetArea(int x, int y)
        {
            return Areas.First(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }

        public static Town GetTown(int x, int y)
        {
            return Towns.First(o => (o.StartX <= x && o.EndX >= x && o.StartY <= y && o.EndY >= y));
        }

        public static bool CanDropItems(int x, int y)
        {
            if (World.InSpecialTile(x, y))
            {
                World.SpecialTile tile = World.GetSpecialTile(x, y);
                if (tile.Code != null)
                    return false;
            }
            return true;
        }

    }
}
