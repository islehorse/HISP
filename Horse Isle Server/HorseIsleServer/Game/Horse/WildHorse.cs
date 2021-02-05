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

        public bool CanHorseBeHere(int x, int y, bool checkSpawnLocationValid=true)
        {

            // Horses cannot be in towns.
            if (World.InTown(x, y))
                return false;
            if (World.InSpecialTile(x, y))
                return false;

            // Check area
            if(checkSpawnLocationValid)
            {
                if (this.Instance.Breed.SpawnInArea != null)
                {
                    if (World.InArea(x, y))
                    {
                        if (World.GetArea(x, y).Name != this.Instance.Breed.SpawnInArea)
                            return false;
                    }
                }
            }
            


            // Check Tile Type
            int TileID = Map.GetTileId(x, y, false);
            string TileType = Map.TerrainTiles[TileID - 1].Type;
            if(checkSpawnLocationValid)
            {
                if (TileType != this.Instance.Breed.SpawnOn)
                    return false;
            }


            if (Map.CheckPassable(x, y)) // Can the player stand over here?
                return true;

            return false;
        }
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


                        // Pick x/y
                        int tryX = GameServer.RandomNumberGenerator.Next(0, Map.Width);
                        int tryY = GameServer.RandomNumberGenerator.Next(0, Map.Height);

                        if (CanHorseBeHere(tryX, tryY))
                        {
                            x = tryX;
                            y = tryY;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        World.Zone zone = World.GetZoneByName(horse.Breed.SpawnInArea);
                        // Pick x/y in zone.
                        int tryX = GameServer.RandomNumberGenerator.Next(zone.StartX, zone.EndX);
                        int tryY = GameServer.RandomNumberGenerator.Next(zone.StartY, zone.EndY);

                        if (CanHorseBeHere(tryX, tryY))
                        {
                            x = tryX;
                            y = tryY;
                            break;
                        }
                        else
                        {
                            continue;
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
            if (GameServer.GetUsersAt(this.X, this.Y, true, true).Length > 0)
                return;

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
            bool check = CanHorseBeHere(this.X, this.Y); // if the horse is allready in an invalid position..

            if (CanHorseBeHere(tryX, tryY, check))
            {
                x = tryX;
                y = tryY;
            }
            
        }

        public void Escape()
        {
            while(true)
            {
                int tryX = X + GameServer.RandomNumberGenerator.Next(-15, 15);
                int tryY = Y + GameServer.RandomNumberGenerator.Next(-15, 15);

                bool check = CanHorseBeHere(this.X, this.Y); // if the horse is allready in an invalid position..

                if (CanHorseBeHere(tryX, tryY, check))
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
            Logger.DebugPrint("Making horses wander.");
            foreach (WildHorse wildHorse in WildHorses)
            {
                wildHorse.Timeout -= 1;

                if (GameServer.GetUsersAt(wildHorse.X, wildHorse.Y, true, true).Length > 0)
                    continue;

                if (wildHorse.Timeout <= 0)
                    Despawn(wildHorse);

                if (GameServer.RandomNumberGenerator.Next(0, 100) >= 25)
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
