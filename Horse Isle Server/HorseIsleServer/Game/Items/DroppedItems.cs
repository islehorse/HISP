using System;
using System.Collections.Generic;
using HISP.Server;
using HISP.Game;

namespace HISP.Game.Items
{
    public class DroppedItems
    {
        public struct DroppedItem
        {
            public int X;
            public int Y;
            public int DespawnTimer;
            public ItemInstance instance;
        }
        private static int epoch = 0;
        private static List<DroppedItem> droppedItemsList = new List<DroppedItem>();
        public static int GetCountOfItem(Item.ItemInformation item)
        {

            DroppedItem[] dropedItems = droppedItemsList.ToArray();
            int count = 0;
            foreach(DroppedItem droppedItem in dropedItems)
            {
                if (droppedItem.instance == null)
                {
                    RemoveDroppedItem(droppedItem);
                    continue;
                }

                if(droppedItem.instance.ItemId == item.Id)
                {
                    count++;
                }
            }
            return count;
        }

        public static DroppedItem[] GetItemsAt(int x, int y)
        {

            DroppedItem[] dropedItems = droppedItemsList.ToArray();
            List<DroppedItem> items = new List<DroppedItem>();
            foreach(DroppedItem droppedItem in dropedItems)
            {
                if(droppedItem.X == x && droppedItem.Y == y)
                {
                    items.Add(droppedItem);
                }
            }
            return items.ToArray();
        }
        public static void ReadFromDatabase()
        {
            DroppedItem[] items = Database.GetDroppedItems();
            foreach (DroppedItem droppedItem in items)
                droppedItemsList.Add(droppedItem); 
        }
        public static void Update()
        {
            int epoch_new = (Int32)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            DespawnItems(epoch, epoch_new);

            GenerateItems();
        }
        public static void RemoveDroppedItem(DroppedItem item)
        {
            int randomId = item.instance.RandomId;
            Database.RemoveDroppedItem(randomId);
            droppedItemsList.Remove(item);
            
        }

        public static bool IsDroppedItemExist(int randomId)
        {
            try
            {
                GetDroppedItemById(randomId);
                return true;

            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }
        public static DroppedItem GetDroppedItemById(int randomId)
        {

            DroppedItem[] dropedItems = droppedItemsList.ToArray();

            foreach (DroppedItem item in dropedItems)
            {
                if(item.instance.RandomId == randomId)
                {
                    return item;
                }
            }

            throw new KeyNotFoundException("Random id: " + randomId.ToString() + " not found");
            
        }
        public static void DespawnItems(int old_epoch, int new_epoch)
        {
            int removedCount = 0;
            DroppedItem[] items = droppedItemsList.ToArray();
            foreach (DroppedItem item in items)
            {
                if(new_epoch + item.DespawnTimer < old_epoch)
                {
                    if(GameServer.GetUsersAt(item.X, item.Y,true,true).Length == 0)
                    {
                        Logger.DebugPrint("Despawned Item at " + item.X + ", " + item.Y);
                        RemoveDroppedItem(item);
                        removedCount++;
                    }
                }
            }
            if(removedCount > 0)
                epoch = new_epoch;
        }

        public static void AddItem(ItemInstance item, int x, int y)
        {
            DroppedItem droppedItem = new DroppedItem();
            droppedItem.X = x;
            droppedItem.Y = y;
            droppedItem.DespawnTimer = 1500;
            droppedItem.instance = item;
            droppedItemsList.Add(droppedItem);
        }
        public static void GenerateItems()
        {

            Logger.InfoPrint("Generating items, (this may take awhile on a fresh database!)");

            int newItems = 0;
            foreach (Item.ItemInformation item in Item.Items)
            {
                int count = GetCountOfItem(item);
                while (count < item.SpawnParamaters.SpawnCap)
                {

                    count++;

                    int despawnTimer = GameServer.RandomNumberGenerator.Next(900, 1500);

                    if (item.SpawnParamaters.SpawnInZone != null)
                    {
                        World.Zone spawnArea = World.GetZoneByName(item.SpawnParamaters.SpawnInZone);

                        while(true)
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
                                    if (GetItemsAt(tryX, tryY).Length > 26) // Max here
                                        continue;

                                    ItemInstance instance = new ItemInstance(item.Id);
                                    DroppedItem droppedItem = new DroppedItem();
                                    droppedItem.X = tryX;
                                    droppedItem.Y = tryY;
                                    droppedItem.DespawnTimer = despawnTimer;
                                    droppedItem.instance = instance;
                                    droppedItemsList.Add(droppedItem);
                                    Database.AddDroppedItem(droppedItem);
                                    Logger.DebugPrint("Created Item ID: " + instance.ItemId + " in ZONE: " + spawnArea.Name + " at: X: " + droppedItem.X + " Y: " + droppedItem.Y);
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
                        while(true)
                        {
                            // Pick a random special tile
                            World.SpecialTile[] possileTiles = World.GetSpecialTilesByName(item.SpawnParamaters.SpawnOnSpecialTile);
                            World.SpecialTile spawnOn = possileTiles[GameServer.RandomNumberGenerator.Next(0, possileTiles.Length)];

                            if (Map.CheckPassable(spawnOn.X, spawnOn.Y))
                            {
                                if (GetItemsAt(spawnOn.X, spawnOn.Y).Length > 26) // Max here
                                    continue;

                                ItemInstance instance = new ItemInstance(item.Id);
                                DroppedItem droppedItem = new DroppedItem();
                                droppedItem.X = spawnOn.X;
                                droppedItem.Y = spawnOn.Y;
                                droppedItem.DespawnTimer = despawnTimer;
                                droppedItem.instance = instance;
                                droppedItemsList.Add(droppedItem);
                                Database.AddDroppedItem(droppedItem);
                                Logger.DebugPrint("Created Item ID: " + instance.ItemId + " at: X: " + droppedItem.X + " Y: " + droppedItem.Y);
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
                            else if(direction == 1)
                            {
                                tryX = spawnNearTile.X - 1;
                                tryY = spawnNearTile.Y;
                            }
                            else if(direction == 3)
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
                                if (GetItemsAt(tryX, tryY).Length > 26) // Max here
                                    continue;

                                ItemInstance instance = new ItemInstance(item.Id);
                                DroppedItem droppedItem = new DroppedItem();
                                droppedItem.X = tryX;
                                droppedItem.Y = tryY;
                                droppedItem.DespawnTimer = despawnTimer;
                                droppedItem.instance = instance;
                                droppedItemsList.Add(droppedItem);
                                Database.AddDroppedItem(droppedItem);
                                Logger.DebugPrint("Created Item ID: " + instance.ItemId + " at: X: " + droppedItem.X + " Y: " + droppedItem.Y);
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
                            // Pick a random isle:
                            //int isleId = GameServer.RandomNumberGenerator.Next(0, World.Isles.Count);
                            //World.Isle isle = World.Isles[isleId];

                            // Pick a random location inside the isle
                            //int tryX = GameServer.RandomNumberGenerator.Next(isle.StartX, isle.EndX);
                            //int tryY = GameServer.RandomNumberGenerator.Next(isle.StartY, isle.EndY);
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
                                    if (GetItemsAt(tryX, tryY).Length > 26) // Max here
                                        continue;

                                    ItemInstance instance = new ItemInstance(item.Id);
                                    DroppedItem droppedItem = new DroppedItem();
                                    droppedItem.X = tryX;
                                    droppedItem.Y = tryY;
                                    droppedItem.DespawnTimer = despawnTimer;
                                    droppedItem.instance = instance;
                                    droppedItemsList.Add(droppedItem);
                                    Database.AddDroppedItem(droppedItem);
                                    Logger.DebugPrint("Created Item ID: " + instance.ItemId + " at: X: " + droppedItem.X + " Y: " + droppedItem.Y);
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

        public static void Init()
        {
            ReadFromDatabase();
            GenerateItems();
        }

    }
}
