using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Horse
{

    class WildHorse
    {

        public WildHorse(HorseInstance horse, int MapX = -1, int MapY = -1, int despawnTimeout=60, bool addToDatabase = true)
        {
            Instance = horse;
            timeout = despawnTimeout;

            if(MapX == -1 && MapY == -1)
            {
                while (true)
                {
                    if (horse.Breed.SpawnInArea == null)
                    {

                        // Pick a random isle.
                        int isleId = GameServer.RandomNumberGenerator.Next(0, World.Isles.Count);
                        World.Isle isle = World.Isles[isleId];

                        // Pick x/y in isle.
                        int tryX = GameServer.RandomNumberGenerator.Next(isle.StartX, isle.EndX);
                        int tryY = GameServer.RandomNumberGenerator.Next(isle.StartY, isle.EndY);

                        // Horses cannot be in towns.
                        if (World.InTown(tryX, tryY))
                            continue;
                        if (World.InSpecialTile(tryX, tryY))
                            continue;

                        // Check Tile Type
                        int TileID = Map.GetTileId(tryX, tryY, false);
                        string TileType = Map.TerrainTiles[TileID - 1].Type;
                        if (TileType == horse.Breed.SpawnOn)
                        {
                            if (Map.CheckPassable(tryX, tryY)) // Can the player stand over here?
                            {
                                x = tryX;
                                y = tryY;
                                break;
                            }
                        }
                    }
                    else
                    {
                        World.Zone zone = World.GetZoneByName(horse.Breed.SpawnInArea);
                        // Pick x/y in zone.
                        int tryX = GameServer.RandomNumberGenerator.Next(zone.StartX, zone.EndX);
                        int tryY = GameServer.RandomNumberGenerator.Next(zone.StartY, zone.EndY);

                        // Horses cannot be in towns.
                        if (World.InTown(tryX, tryY))
                            continue;
                        if (World.InSpecialTile(tryX, tryY))
                            continue;

                        // Check Tile Type
                        int TileID = Map.GetTileId(tryX, tryY, false);
                        string TileType = Map.TerrainTiles[TileID - 1].Type;
                        if (TileType == horse.Breed.SpawnOn)
                        {
                            if (Map.CheckPassable(tryX, tryY)) // Can the player stand over here?
                            {
                                x = tryX;
                                y = tryY;
                                break;
                            }
                        }

                    }
                }
            }
            wildHorses.Add(this);
            if(addToDatabase)
                Database.AddWildHorse(this);
        }

        private static List<WildHorse> wildHorses = new List<WildHorse>();
        public static WildHorse[] WildHorses
        {
            get
            {
                return wildHorses.ToArray();
            }
        }

        public static void GenerateHorses()
        {
            Logger.InfoPrint("Generating horses.");
            while(wildHorses.Count < 40)
            {
                HorseInfo.Breed horseBreed = HorseInfo.Breeds[GameServer.RandomNumberGenerator.Next(0, HorseInfo.Breeds.Count)];

                if (horseBreed.SpawnInArea == "none") // no unipegs >_>
                    continue;

                HorseInstance horseInst = new HorseInstance(horseBreed);
                WildHorse wildHorse = new WildHorse(horseInst);

                Logger.DebugPrint("Created " + horseBreed.Name + " at X:" + wildHorse.X + ", Y:" + wildHorse.Y);
            }
        }

        public static void Init()
        {
            Database.LoadWildHorses();
            GenerateHorses();
        }
        public HorseInstance Instance;
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }


        private int x;
        private int y;
        private int timeout;

    }
}
