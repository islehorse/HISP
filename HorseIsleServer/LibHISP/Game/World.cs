using System;
using System.Collections.Generic;
using HISP.Player;
using HISP.Server;

namespace HISP.Game
{

    public class World
    {
        private static Waypoint getWaypoint(string find)
        {
            foreach(Waypoint waypoint in Waypoints)
            {
                if(waypoint.Name == find)
                    return waypoint;
            }
            
            throw new KeyNotFoundException("Waypoint with name "+find+" not found.");
        
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
                    point = GetWaypoint();
                }
                catch (KeyNotFoundException)
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
                    foreach(User user in GameServer.GetUsersInIsle(this,true,true))
                    {
                        GameServer.UpdateWorld(user.Client);
                    }
                }
            }
            
            public Waypoint GetWaypoint()
            {
                return getWaypoint(this.Name);
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
                    point = GetWaypoint();
                }
                catch (KeyNotFoundException)
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
                    foreach (User user in GameServer.GetUsersInTown(this, true, true))
                    {
                        GameServer.UpdateArea(user.Client);
                    }
                }

            }
            public Waypoint GetWaypoint()
            {
                return getWaypoint(this.Name);
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

        public static List<Waypoint> Waypoints = new List<Waypoint>();
        public static List<Isle> Isles = new List<Isle>();
        public static List<Town> Towns = new List<Town>();
        public static List<Area> Areas = new List<Area>();
        public static List<Zone> Zones = new List<Zone>();

        public static List<SpecialTile> SpecialTiles = new List<SpecialTile>();

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
            try
            {
                GetZone(x, y);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
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

        public static bool InSpecialTile(int x, int y)
        {
            foreach (SpecialTile specialTile in SpecialTiles)
            {
                if (specialTile.X == x && specialTile.Y == y)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool InIsle(int x, int y)
        {
            foreach (Isle isle in Isles)
            {
                if (isle.StartX <= x && isle.EndX >= x && isle.StartY <= y && isle.EndY >= y)
                {
                    return true;
                }
            }
            return false;
        }
        public static Zone GetZoneByName(string name)
        {
            foreach(Zone zone in Zones)
            {
                if (zone.Name == name)
                    return zone;
            }
            throw new KeyNotFoundException("Zone not found.");
        }
        public static SpecialTile[] GetSpecialTilesByCode(string code)
        {
            List<SpecialTile> tiles = new List<SpecialTile>();
            foreach (SpecialTile tile in SpecialTiles)
            {
                if (tile.Code == code)
                {
                    tiles.Add(tile);
                }
            }
            return tiles.ToArray();
        }
        public static SpecialTile[] GetSpecialTilesByName(string name)
        {
            List<SpecialTile> tiles = new List<SpecialTile>();
            foreach(SpecialTile tile in SpecialTiles)
            {
                if(tile.Title == name)
                {
                    tiles.Add(tile);
                }
            }
            return tiles.ToArray();
        }
        public static SpecialTile GetSpecialTile(int x, int y)
        {
            foreach(SpecialTile specialTile in SpecialTiles)
            {
                if(specialTile.X == x && specialTile.Y == y)
                {
                    return specialTile;
                }
            }
            throw new KeyNotFoundException("x,y not in a special tile!");
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

        public static Zone GetZone(int x, int y)
        {
            foreach (Zone zone in Zones)
            {

                if (zone.StartX <= x && zone.EndX >= x && zone.StartY <= y && zone.EndY >= y)
                {
                    return zone;
                }
            }
            throw new KeyNotFoundException("x,y not in an zone!");
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
