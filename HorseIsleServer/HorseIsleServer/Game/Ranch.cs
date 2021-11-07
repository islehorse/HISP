using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Player;
using HISP.Server;
using System;
using System.Collections.Generic;

namespace HISP.Game
{
    public class Ranch
    {
        public class RanchUpgrade
        {
            public static List<RanchUpgrade> RanchUpgrades = new List<RanchUpgrade>();
            public int Id;
            public int Cost;
            public string Title;
            public string Description;
            public int Limit;

            public static bool RanchUpgradeExists(int id)
            {
                foreach (RanchUpgrade rachUpgrade in RanchUpgrades)
                {
                    if (rachUpgrade.Id == id)
                        return true;
                }
                return false;
            }
            public static RanchUpgrade GetRanchUpgradeById(int id)
            {
                foreach (RanchUpgrade rachUpgrade in RanchUpgrades)
                {
                    if (rachUpgrade.Id == id)
                        return rachUpgrade;
                }
                throw new KeyNotFoundException("No ranch found.");
            }
        }
        public class RanchBuilding
        {
            public static List<RanchBuilding> RanchBuildings = new List<RanchBuilding>();
            public int Id;
            public int Cost;
            public string Title;
            public string Description;

            public static bool RanchBuildingExists(int id)
            {
                foreach (RanchBuilding ranchBuilding in RanchBuildings)
                {
                    if (ranchBuilding.Id == id)
                        return true;
                }
                return false;
            }
            public static RanchBuilding GetRanchBuildingById(int id)
            {
                foreach(RanchBuilding ranchBuilding in RanchBuildings)
                {
                    if (ranchBuilding.Id == id)
                        return ranchBuilding;
                }
                throw new KeyNotFoundException("No ranch found.");
            }

            public int GetTeardownPrice()
            {
                return (int)Math.Round((float)this.Cost / (100 / 35.0));
            }
        }
        public static List<Ranch> Ranches = new List<Ranch>();

        public int X;
        public int Y;
        public int Id;
        public int Value;

        private int ownerId;        
        private int upgradedLevel;
        private int investedMoney;
        private string title;
        private string description;

        public int GetSellPrice()
        {
            return (int)Math.Round((float)this.InvestedMoney / (100 / 75.0));
        }
        private void removeDorothyShoes(int Id)
        {
            if (Id == -1)
                return;
            
            if(GameServer.IsUserOnline(Id))
            {
                User user = GameServer.GetUserById(Id);
                user.OwnedRanch = null;
                InventoryItem items = user.Inventory.GetItemByItemId(Item.DorothyShoes);
                foreach (ItemInstance itm in items.ItemInstances)
                {
                    user.Inventory.Remove(itm);
                }
            }
            else
            {
                Database.RemoveAllItemTypesFromPlayerInventory(this.Id, Item.DorothyShoes);
            }
            
        }
        
        private void deleteRanch()
        {
            Database.DeleteRanchOwner(this.Id);
            removeDorothyShoes(this.ownerId);
            resetRanch();
        }
        private void resetRanch()
        {
            title = "";
            description = "";
            investedMoney = 0;
            upgradedLevel = 0;
            ownerId = -1;
            for (int i = 0; i < 16; i++)
                buildings[i] = null;
        }
        public int OwnerId
        {
            get
            {
                if(ownerId != -1)
                {
                    if (ConfigReader.AllUsersSubbed || Database.IsUserAdmin(ownerId))
                        return ownerId;

                    int subExp = Database.GetUserSubscriptionExpireDate(ownerId);
                    DateTime expTime = Util.UnixTimeStampToDateTime(subExp);
                    if ((DateTime.UtcNow.Date - expTime.Date).Days >= 30)
                    {
                        int price = GetSellPrice();
                        try
                        {
                            checked
                            {
                                Database.SetPlayerMoney(Database.GetPlayerMoney(ownerId) + price, ownerId);    
                            }
                        }
                        catch (OverflowException)
                        {
                            Database.SetPlayerMoney(2147483647, ownerId);;
                        }

                        Database.AddMessageToQueue(ownerId, Messages.FormatRanchForcefullySoldMessage(price));
                        deleteRanch();
                        return -1;
                    }

                }
                return ownerId;
                
            }
            set
            {
                if (value == -1)
                {
                    deleteRanch();
                }
                else
                {
                    if(Database.IsRanchOwned(this.Id))
                    {
                        Database.SetRanchOwner(this.Id, ownerId);
                        removeDorothyShoes(ownerId);
                    }
                    else
                    {
                        resetRanch();
                        Database.AddRanch(this.Id, value, "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                }

                ownerId = value;
            }
        }

        public int UpgradedLevel
        {
            get
            {
                return upgradedLevel;
            }
            set
            {
                upgradedLevel = value;
                Database.SetRanchUpgradeLevel(Id, value);
            }
        }
        public int InvestedMoney
        {
            get
            {
                return investedMoney;
            }
            set
            {
                investedMoney = value;
                Database.SetRanchInvestment(Id, value);
            }
        }
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value.Trim();
                Database.SetRanchTitle(Id, title);
            }
        }
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value.Trim();
                Database.SetRanchDescription(Id, value);
            }
        }
        

        private RanchBuilding[] buildings = new RanchBuilding[16];
        public int GetBuildingCount(int buildingId)
        {
            int count = 0;
            foreach(RanchBuilding building in buildings)
            {
                if(building != null)
                    if (building.Id == buildingId)
                        count++;
            }
            return count;
        }
        private void updateBuildings()
        {
            for(int i = 0; i < buildings.Length; i++)
            {
                if (buildings[i] != null)
                    Database.SetRanchBuilding(Id, i + 1, buildings[i].Id);
                else
                    Database.SetRanchBuilding(Id, i + 1, 0);
            }
        }
        public RanchBuilding GetBuilding(int buildingId)
        {
            return buildings[buildingId];
        }
        public void SetBuilding(int buildingId, RanchBuilding value)
        {
            buildings[buildingId] = value;
            updateBuildings();
        }


        public string GetSwf(bool mine)
        {
            string swf  = "ranchviewer.swf?H=" + (upgradedLevel+1).ToString();
            for(int i = 0; i < buildings.Length; i++)
            {
                swf += "&B" + (i+1).ToString() + "=";
                if (buildings[i] != null)
                {
                    swf += buildings[i].Id.ToString();
                }
            }
            if (mine)
                swf += "&MINE=1";
            return swf;
        }
        

        public Ranch(int x, int y, int id, int value)
        {
            X = x;
            Y = y;
            Id = id;
            Value = value;
            title = "";
            description = "";
            upgradedLevel = 0;
            ownerId = -1;
            investedMoney = 0;
            for (int i = 0; i < 16; i++)
                buildings[i] = null;
            bool owned = Database.IsRanchOwned(id);
            if (owned)
            {
                upgradedLevel = Database.GetRanchUpgradeLevel(id);
                title = Database.GetRanchTitle(id);
                description = Database.GetRanchDescription(id);
                ownerId = Database.GetRanchOwner(id);

                for (int i = 1; i <= 16; i++)
                {
                    int bid = Database.GetRanchBuilding(id, i);
                    if(RanchBuilding.RanchBuildingExists(bid))
                        buildings[i - 1] = RanchBuilding.GetRanchBuildingById(bid);
                }

                InvestedMoney = Database.GetRanchInvestment(id);
            }
        }

        public RanchUpgrade GetRanchUpgrade()
        {
            return RanchUpgrade.GetRanchUpgradeById(this.upgradedLevel + 1);
        }
        public static bool IsRanchHere(int x, int y)
        {
            foreach (Ranch ranch in Ranches)
            {
                if (ranch.X == x && ranch.Y == y)
                    return true;
            }
            return false;
        }
        public static bool RanchExists(int ranchId)
        {
            foreach (Ranch ranch in Ranches)
            {
                if (ranch.Id == ranchId)
                    return true;
            }
            return false;
        }
        public static Ranch GetRanchById(int ranchId)
        {
            foreach (Ranch ranch in Ranches)
            {
                if (ranch.Id == ranchId)
                    return ranch;
            }
            throw new KeyNotFoundException("No Ranch with id " + ranchId);
        }
        public static Ranch GetRanchAt(int x, int y)
        {
            foreach(Ranch ranch in Ranches)
            {
                if (ranch.X == x && ranch.Y == y)
                    return ranch;
            }
            throw new KeyNotFoundException("No Ranch found at x" + x + " y" + y);
        }

        public static bool IsRanchOwned(int playerId)
        {
            foreach (Ranch ranch in Ranches)
            {
                if (ranch.OwnerId == playerId)
                {
                    return true;
                }
            }
            return false;
        }
        public static Ranch GetRanchOwnedBy(int playerId)
        {
            foreach(Ranch ranch in Ranches)
            {
                if(ranch.OwnerId == playerId)
                {
                    return ranch;
                }
            }
            throw new KeyNotFoundException("Player " + playerId + " does not own a ranch.");
        }
    }
}
