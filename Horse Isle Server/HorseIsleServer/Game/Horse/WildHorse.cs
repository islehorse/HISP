using HISP.Player;
using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Horse
{

    public class WildHorse
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
            else
            {
                x = MapX;
                y = MapY;
            }
            wildHorses.Add(this);
            if(addToDatabase)
                Database.AddWildHorse(this);
        }


        public void RandomWander()
        {
            while(true)
            {
                int direction = GameServer.RandomNumberGenerator.Next(0, 3);
                int tryX = this.X;
                int tryY = this.Y;

                switch (direction)
                {
                    case 0:
                        tryX += 1;
                        break;
                    case 1:
                        tryX -= 1;
                        break;
                    case 2:
                        tryY += 1;
                        break;
                    case 3:
                        tryY -= 1;
                        break;


                }
                // Horses cannot be in towns.
                if (World.InTown(tryX, tryY))
                    continue;
                if (World.InSpecialTile(tryX, tryY))
                    continue;

                if (Map.CheckPassable(tryX, tryY)) // Can the player stand here?
                {
                    Logger.DebugPrint(this.Instance.Breed.Name + " Randomly wandered to: " + tryX.ToString() + ", " + tryY.ToString());
                    X = tryX;
                    Y = tryY;
                    break;
                }
            }
        }

        public void Escape()
        {
            while(true)
            {
                int tryX = X + GameServer.RandomNumberGenerator.Next(-15, 15);
                int tryY = Y + GameServer.RandomNumberGenerator.Next(-15, 15);

                // Horses cannot be in towns.
                if (World.InTown(tryX, tryY))
                    continue;
                if (World.InSpecialTile(tryX, tryY))
                    continue;

                if (Map.CheckPassable(tryX, tryY))
                {
                    X = tryX;
                    Y = tryY;
                    break;
                }
            }

        }

        public void Capture(User forUser)
        {
            forUser.HorseInventory.AddHorse(this.Instance);
            Despawn(this);
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
                if (horseBreed.Swf == "")
                    continue;
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

        public static WildHorse[] GetHorsesAt(int x, int y)
        {
            List<WildHorse> horses = new List<WildHorse>();
            foreach (WildHorse wildHorse in WildHorses)
            {
                if (wildHorse.X == x && wildHorse.Y == y)
                    horses.Add(wildHorse);
            }
            return horses.ToArray();
        }
        
        public static bool DoesHorseExist(int randomId)
        {
            foreach (WildHorse wildHorse in WildHorses)
            {
                if (wildHorse.Instance.RandomId == randomId)
                    return true;
            }
            return false;
        }
        public static WildHorse GetHorseById(int randomId)
        {
            foreach(WildHorse wildHorse in WildHorses)
            {
                if (wildHorse.Instance.RandomId == randomId)
                    return wildHorse;
            }
            throw new KeyNotFoundException("No horse with id: " + randomId + " was found.");
        }

        public static void Despawn(WildHorse horse)
        {
            Database.RemoveWildHorse(horse.Instance.RandomId);
            wildHorses.Remove(horse);
        }

        public static void Update()
        {
            foreach(WildHorse wildHorse in WildHorses)
            {
                wildHorse.Timeout -= 1;

                if (GameServer.GetUsersAt(wildHorse.X, wildHorse.Y, true, true).Length > 0)
                    continue;

                if (wildHorse.Timeout <= 0)
                    Despawn(wildHorse);

                if (GameServer.RandomNumberGenerator.Next(0, 100) >= 50)
                    wildHorse.RandomWander();
            }
            if(WildHorses.Length < 40)
            {
                GenerateHorses();
            }    
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
                Database.SetWildHorseX(this.Instance.RandomId, value);
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
                Database.SetWildHorseY(this.Instance.RandomId, value);
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
                Database.SetWildHorseTimeout(this.Instance.RandomId, value);
                timeout = value;
            }
        }


        private int x;
        private int y;
        private int timeout;

    }
}
