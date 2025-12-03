using HISP.Player;
using HISP.Server;
using HISP.Util;
using System;
using System.Linq;

namespace HISP.Game.Items
{
    public class DroppedItems
    {
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
                return droppedItemsList.ToArray();
            }
        }
        public static int GetCountOfItem(Item.ItemInformation item)
        {
            return droppedItemsList.Count(o => o.Instance.ItemId == item.Id);
        }

        public static DroppedItem[] GetItemsAt(int x, int y)
        {
            return droppedItemsList.Where(o => (o.X == x && o.Y == y)).ToArray();
        }
        public static void ReadFromDatabase()
        {
            DroppedItem[] items = Database.GetDroppedItems();
            foreach (DroppedItem droppedItem in items) droppedItemsList.Add(droppedItem); 
        }
        public static void RemoveDroppedItem(DroppedItem item)
        {
            int randomId = item.Instance.RandomId;
            Database.RemoveDroppedItem(randomId);
            droppedItemsList.Remove(item);
            
        }

        public static bool IsDroppedItemExist(int randomId)
        {
            return droppedItemsList.Any(o => o.Instance.RandomId == randomId);
        }
        public static DroppedItem GetDroppedItemById(int randomId)
        {
            return droppedItemsList.First(o => o.Instance.RandomId == randomId);
        }
        public static void DespawnItems()
        {
            Database.DecrementDroppedItemDespawnTimer();
            Database.RemoveDespawningItems(); // GO-GO-GO-GOGOGOGO GOTTA GO FAST!!!
            for (int i = 0; i < droppedItemsList.Count; i++)
            {
                if (droppedItemsList[i] == null) // Item removed in another thread.
                    continue;

                droppedItemsList[i].DespawnTimer -= 5;

                if(droppedItemsList[i].DespawnTimer <= 0)
                {
                    if (User.GetUsersAt(droppedItemsList[i].X, droppedItemsList[i].Y, true, true).Length > 0) // Dont despawn items players are standing on
                        continue;

                    Logger.DebugPrint("Despawned Item at " + droppedItemsList[i].X + ", " + droppedItemsList[i].Y);
                    droppedItemsList.Remove(droppedItemsList[i]);
                    
                }
            }
        }

        public static void AddItem(ItemInstance item, int x, int y, int despawnTimer=1500)
        {
            DroppedItem droppedItem = new DroppedItem(item);
            droppedItem.X = x;
            droppedItem.Y = y;
            droppedItem.DespawnTimer = despawnTimer;
            droppedItemsList.Add(droppedItem);
            Database.AddDroppedItem(droppedItem);
        }
        public static void GenerateItems()
        {
            Logger.DebugPrint("Generating items.");

            int newItems = 0;
            foreach (Item.ItemInformation item in Item.Items)
            {
                int count = GetCountOfItem(item);
#if !OS_DEBUG
                do
                {
#endif
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
#if !OS_DEBUG
                } while (count < item.SpawnParamaters.SpawnCap);
#endif

            }

        }

        public static void DeleteAllItemsWithId(int itemId)
        {
            Database.DeleteAllDroppedItemsWithId(itemId);
            droppedItemsList.RemoveAll(o => o.Instance.ItemId == itemId);
        }

        public static void Init()
        {
            ReadFromDatabase();
            GenerateItems();
        }

    }
}
