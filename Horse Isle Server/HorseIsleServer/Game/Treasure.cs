using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game
{
    public class Treasure
    {
        private static List<Treasure> treasures = new List<Treasure>();
        public static Treasure[] Treasures
        {
            get
            {
                return treasures.ToArray();
            }
        }

        private int value;

        public int RandomId;
        public int X;
        public int Y;
        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                Database.SetTreasureValue(RandomId, value);
            }
        }

        public string Type;
        public Treasure(int x, int y, string type, int randomId = -1,  int moneyValue=-1)
        {
            RandomId = Security.RandomID.NextRandomId(randomId);

            if(type == "BURIED")
            {
                if(moneyValue == -1)
                    value = GameServer.RandomNumberGenerator.Next(100,1500);
            }
            else if(type == "RAINBOW")
            {
                if (moneyValue == -1)
                    value = GameServer.RandomNumberGenerator.Next(10000, 30000);
            }

            if (moneyValue != -1)
                value = moneyValue;

            X = x;
            Y = y;
            Type = type;
        }

        public static int NumberOfPirates()
        {
            int count = 0;
            foreach (Treasure treasure in Treasures)
            {
                if (treasure.Type == "BURIED")
                    count++;
            }
            return count;
        }

        public static int NumberOfRainbows()
        {
            int count = 0;
            foreach(Treasure treasure in Treasures)
            {
                if (treasure.Type == "RAINBOW")
                    count++;
            }
            return count;
        }

        public static bool IsTileTreasure(int x, int y)
        {
            foreach (Treasure treasure in Treasures)
            {
                if (treasure.X == x && treasure.Y == y)
                    return true;
                
            }
            return false;
        }
        public static bool IsTileBuiredTreasure(int x, int y)
        {
            foreach (Treasure treasure in Treasures)
            {
                if (treasure.Type == "BURIED")
                {
                    if (treasure.X == x && treasure.Y == y)
                        return true;
                }
            }
            return false;
        }

        public static bool IsTilePotOfGold(int x, int y)
        {
            foreach(Treasure treasure in Treasures)
            {
                if(treasure.Type == "RAINBOW")
                {
                    if (treasure.X == x && treasure.Y == y)
                        return true;
                }
            }
            return false;
        }

        public static Treasure GetTreasureAt(int x, int y)
        {
            foreach (Treasure treasure in Treasures)
            {
                if (treasure.X == x && treasure.Y == y)
                    return treasure;
            }
            throw new KeyNotFoundException("NO Treasure at " + x + "," + y);
        }

        public static void AddValue()
        {
            foreach(Treasure treasure in treasures)
            {
                treasure.Value += 1;
            }
        }
        public void CollectTreasure(User user)
        {

            treasures.Remove(this);
            Database.DeleteTreasure(this.RandomId);
            GenerateTreasure();

            byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
            user.LoggedinClient.SendPacket(MovementPacket);

            user.AddMoney(Value);

            if(this.Type == "BURIED")
            {
                byte[] treasureReceivedPacket = PacketBuilder.CreateChat(Messages.FormatPirateTreasure(this.Value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.LoggedinClient.SendPacket(treasureReceivedPacket);
                user.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PirateTreasure).Count++;
                
                if(user.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PirateTreasure).Count >= 10)
                    user.Awards.AddAward(Award.GetAwardById(18)); // Pirate Tracker

                if (user.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PirateTreasure).Count >= 100)
                    user.Awards.AddAward(Award.GetAwardById(19)); // Pirate Stalker
            }
            else if(this.Type == "RAINBOW")
            {
                byte[] treasureReceivedPacket = PacketBuilder.CreateChat(Messages.FormatPotOfGold(this.Value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.LoggedinClient.SendPacket(treasureReceivedPacket);
                
                user.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PotOfGold).Count++;

                if (user.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PotOfGold).Count >= 3)
                    user.Awards.AddAward(Award.GetAwardById(20)); // Leprechaun

                if (user.TrackedItems.GetTrackedItem(Tracking.TrackableItem.PirateTreasure).Count >= 20)
                    user.Awards.AddAward(Award.GetAwardById(21)); // Lucky Leprechaun
            }

        }

        public static void GenerateTreasure()
        {
            while(NumberOfPirates() < 5)
            {
                // Pick x/y
                int tryX = GameServer.RandomNumberGenerator.Next(0, Map.Width);
                int tryY = GameServer.RandomNumberGenerator.Next(0, Map.Height);

                if (!Map.CheckPassable(tryX, tryY)) // can the player walk here?
                    continue;

                if (World.InTown(tryX, tryY)) // in a town?
                    continue;

                if (Map.GetTileId(tryX, tryY, true) != 1) // is there allready an overlay here?
                    continue;

                if (Map.TerrainTiles[Map.GetTileId(tryX, tryY, false) - 1].Type != "BEACH")
                    continue;

                // Create Treasure
                Treasure treasure = new Treasure(tryX, tryY, "BURIED");
                treasures.Add(treasure);
                Database.AddTreasure(treasure.RandomId, treasure.X, treasure.Y, treasure.Value, treasure.Type);

                Logger.DebugPrint("Created Pirate Treasure at " + treasure.X + "," + treasure.Y + " with value: " + treasure.Value);

            }

            while (NumberOfRainbows() < 1)
            {
                // Pick x/y
                int tryX = GameServer.RandomNumberGenerator.Next(0, Map.Width);
                int tryY = GameServer.RandomNumberGenerator.Next(0, Map.Height);

                if (!Map.CheckPassable(tryX, tryY)) // can the player walk here?
                    continue;

                if (World.InTown(tryX, tryY)) // in a town?
                    continue;

                if (Map.GetTileId(tryX, tryY, true) != 1) // is there allready an overlay here?
                    continue;

                if (Map.TerrainTiles[Map.GetTileId(tryX, tryY, false) - 1].Type != "GRASS" && Map.TerrainTiles[Map.GetTileId(tryX, tryY, false) - 1].Type != "BEACH") // Grass and BEACH tiles only.
                    continue;

                // Create Treasure
                Treasure treasure = new Treasure(tryX, tryY, "RAINBOW");
                treasures.Add(treasure);
                Database.AddTreasure(treasure.RandomId, treasure.X, treasure.Y, treasure.Value, treasure.Type);

                Logger.DebugPrint("Created Pot of Gold at " + treasure.X + "," + treasure.Y + " with value: " + treasure.Value);

            }

        }
        public static void Init()
        {
            Treasure[] treasuresLst = Database.GetTreasures();
            foreach (Treasure treasure in treasuresLst)
            {
                treasures.Add(treasure);
            }

            GenerateTreasure();
        }
    }
}
