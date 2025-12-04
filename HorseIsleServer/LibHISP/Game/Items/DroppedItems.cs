using HISP.Player;
using HISP.Server;
using HISP.Util;
using System;
using System.Linq;
using System.Threading;

namespace HISP.Game.Items
{
    public class DroppedItems
    {
        private static Mutex dropLock = new Mutex();
        public class DroppedItem
        {
            public DroppedItem(ItemInstance itmInstance)
            {
                if (itmInstance == null) throw new NullReferenceException("How could this happen?");
                Instance = itmInstance;
            }
            public int X;
            public int Y;
            public int DespawnTimer;
            public ItemInstance Instance;
            public int Data;
        }
        private static ThreadSafeList<DroppedItem> droppedItemsList = new ThreadSafeList<DroppedItem>();
        public static DroppedItem[] Items
        {
            get
            {
                lock (dropLock)
                {
                    return droppedItemsList.ToArray();
                }
            }
        }
        public static int GetCountOfItem(Item.ItemInformation item)
        {
            lock (dropLock)
            {
                return droppedItemsList.Count(o => o.Instance.ItemId == item.Id);
            }
        }

        public static DroppedItem[] GetItemsAt(int x, int y)
        {
            lock (dropLock)
            {
                return droppedItemsList.Where(o => (o.X == x && o.Y == y)).ToArray();
            }
        }
        public static void ReadFromDatabase()
        {
            lock (dropLock)
            {
                DroppedItem[] items = Database.GetDroppedItems();
                droppedItemsList.AddRange(items);
            }
        }
        public static void RemoveDroppedItem(DroppedItem item)
        {
            lock(dropLock)
            {
                int randomId = item.Instance.RandomId;
                Database.RemoveDroppedItem(randomId);
                droppedItemsList.Remove(item);
            }

        }

        public static bool IsDroppedItemExist(int randomId)
        {
            lock (dropLock)
            {
                return droppedItemsList.Any(o => o.Instance.RandomId == randomId);
            }
        }
        public static DroppedItem GetDroppedItemById(int randomId)
        {
            lock (dropLock)
            {
                return droppedItemsList.First(o => o.Instance.RandomId == randomId);
            }
        }
        public static void DespawnItems()
        {
            lock (dropLock)
            {
                Database.DecrementDroppedItemDespawnTimer();
                Database.RemoveDespawningItems();

                foreach (DroppedItem item in droppedItemsList) item.DespawnTimer -= 5;
                droppedItemsList.RemoveAll(o => (o.DespawnTimer <= 0 && !User.GetUsersAt(o.X, o.Y, true, true).Any()));
            }
        }

        public static void AddItem(ItemInstance item, int x, int y, int despawnTimer=1500)
        {
            lock(dropLock)
            {
                DroppedItem droppedItem = new DroppedItem(item);
                droppedItem.X = x;
                droppedItem.Y = y;
                droppedItem.DespawnTimer = despawnTimer;
                droppedItemsList.Add(droppedItem);
                Database.AddDroppedItem(droppedItem);
            }
        }
        public static void GenerateItems()
        {
            lock (dropLock)
            {
                Logger.DebugPrint("Generating items.");

                int newItems = 0;
                foreach (Item.ItemInformation item in Item.Items)
                {
                    int count = GetCountOfItem(item);

                    if (count < item.SpawnParamaters.SpawnCap)
                    {
                        count++;
                        int despawnTimer = 1440;

                        if (item.SpawnParamaters.SpawnInZone != null)
                        {
                            World.Zone spawnArea = World.GetZoneByName(item.SpawnParamaters.SpawnInZone);

                            while (true)
                            {
                                // Pick a random location inside the zone
                                int tryX = GameServer.RandomNumberGenerator.Next(spawnArea.StartX, spawnArea.EndX);
                                int tryY = GameServer.RandomNumberGenerator.Next(spawnArea.StartY, spawnArea.EndY);


                                if (World.InSpecialTile(tryX, tryY))
                                    continue;


                                if (Map.CheckPassable(tryX, tryY)) // Can the player walk here?
                                {
                                    int TileID = Map.GetTileId(tryX, tryY, false);
                                    string TileType = Map.TerrainTiles[TileID - 1].Type; // Is it the right type?

                                    if (item.SpawnParamaters.SpawnOnTileType == TileType)
                                    {
                                        if (GetItemsAt(tryX, tryY).Length > 25) // Max items in one tile.
                                            continue;

                                        ItemInstance instance = new ItemInstance(item.Id);
                                        AddItem(instance, tryX, tryY, despawnTimer);
                                        Logger.DebugPrint("Created Item ID: " + instance.ItemId + " in ZONE: " + spawnArea.Name + " at: X: " + tryX + " Y: " + tryY);
                                        newItems++;
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }

                        }
                        else if (item.SpawnParamaters.SpawnOnSpecialTile != null)
                        {
                            while (true)
                            {
                                // Pick a random special tile
                                World.SpecialTile[] possileTiles = World.GetSpecialTilesByName(item.SpawnParamaters.SpawnOnSpecialTile);
                                World.SpecialTile spawnOn = possileTiles[GameServer.RandomNumberGenerator.Next(0, possileTiles.Length)];

                                if (Map.CheckPassable(spawnOn.X, spawnOn.Y))
                                {
                                    if (GetItemsAt(spawnOn.X, spawnOn.Y).Length > 25) // Max items in one tile.
                                        continue;

                                    ItemInstance instance = new ItemInstance(item.Id);
                                    AddItem(instance, spawnOn.X, spawnOn.Y, despawnTimer);
                                    Logger.DebugPrint("Created Item ID: " + instance.ItemId + " at: X: " + spawnOn.X + " Y: " + spawnOn.Y);
                                    newItems++;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                        }
                        else if (item.SpawnParamaters.SpawnNearSpecialTile != null)
                        {
                            while (true)
                            {
                                // Pick a random special tile
                                World.SpecialTile[] possileTiles = World.GetSpecialTilesByName(item.SpawnParamaters.SpawnNearSpecialTile);
                                World.SpecialTile spawnNearTile = possileTiles[GameServer.RandomNumberGenerator.Next(0, possileTiles.Length)];

                                // Pick a direction to try spawn in

                                int direction = GameServer.RandomNumberGenerator.Next(0, 4);
                                int tryX = 0;
                                int tryY = 0;
                                if (direction == 0)
                                {
                                    tryX = spawnNearTile.X + 1;
                                    tryY = spawnNearTile.Y;
                                }
                                else if (direction == 1)
                                {
                                    tryX = spawnNearTile.X - 1;
                                    tryY = spawnNearTile.Y;
                                }
                                else if (direction == 3)
                                {
                                    tryX = spawnNearTile.X;
                                    tryY = spawnNearTile.Y + 1;
                                }
                                else if (direction == 4)
                                {
                                    tryX = spawnNearTile.X;
                                    tryY = spawnNearTile.Y - 1;
                                }
                                if (World.InSpecialTile(tryX, tryY))
                                {
                                    World.SpecialTile tile = World.GetSpecialTile(tryX, tryY);
                                    if (tile.Code != null)
                                        continue;
                                }

                                if (Map.CheckPassable(tryX, tryY))
                                {
                                    if (GetItemsAt(tryX, tryY).Length > 25) // Max here
                                        continue;

                                    ItemInstance instance = new ItemInstance(item.Id);
                                    AddItem(instance, tryX, tryY, despawnTimer);
                                    Logger.DebugPrint("Created Item ID: " + instance.ItemId + " at: X: " + tryX + " Y: " + tryY);
                                    newItems++;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                        }
                        else if (item.SpawnParamaters.SpawnOnTileType != null)
                        {

                            while (true)
                            {
                                // Pick a random location:
                                int tryX = GameServer.RandomNumberGenerator.Next(0, Map.Width);
                                int tryY = GameServer.RandomNumberGenerator.Next(0, Map.Height);

                                if (World.InSpecialTile(tryX, tryY))
                                    continue;

                                if (Map.CheckPassable(tryX, tryY)) // Can the player walk here?
                                {
                                    int TileID = Map.GetTileId(tryX, tryY, false);
                                    string TileType = Map.TerrainTiles[TileID - 1].Type; // Is it the right type?

                                    if (item.SpawnParamaters.SpawnOnTileType == TileType)
                                    {
                                        if (GetItemsAt(tryX, tryY).Length > 25) // Max here
                                            continue;

                                        ItemInstance instance = new ItemInstance(item.Id);
                                        AddItem(instance, tryX, tryY, despawnTimer);
                                        Logger.DebugPrint("Created Item ID: " + instance.ItemId + " at: X: " + tryX + " Y: " + tryY);
                                        newItems++;
                                        break;

                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }

                }
            }
        }

        public static void DeleteAllItemsWithId(int itemId)
        {
            lock(dropLock)
            {
                Database.DeleteAllDroppedItemsWithId(itemId);
                droppedItemsList.RemoveAll(o => o.Instance.ItemId == itemId);
            }
        }

        public static void Init()
        {
            lock(dropLock)
            {
                ReadFromDatabase();
                GenerateItems();
            }
        }

    }
}
