﻿using HISP.Server;
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

        public int OwnerId
        {
            get
            {
                return ownerId;
            }
            set
            {

                ownerId = value;

                if (value == -1)
                {
                    Database.DeleteRanchOwner(this.Id);
                }
                else
                {
                    if(Database.IsRanchOwned(this.Id))
                    {
                        Database.SetRanchOwner(this.Id, ownerId);
                    }
                    else
                    {
                        title = "";
                        description = "";
                        investedMoney = 0;
                        upgradedLevel = 0;
                        for (int i = 0; i < 16; i++)
                            buildings[i] = null;

                        Database.AddRanch(this.Id, OwnerId, "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                }
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
                investedMoney = 0;
                Database.SetRanchUpgradeLevel(Id, value);
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
                title = value;
                Database.SetRanchTitle(Id, value);
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
                description = value;
                Database.SetRanchDescription(Id, value);
            }
        }
        

        private RanchBuilding[] buildings = new RanchBuilding[16];

        public RanchBuilding[] Buildings
        {
            get
            {
                return buildings;
            }
            set
            {
                buildings = value;
                if (buildings[0] != null)
                    Database.SetRanchBuilding1(this.Id, buildings[0].Id);
                else
                    Database.SetRanchBuilding1(this.Id, 0);
                if (buildings[1] != null)
                    Database.SetRanchBuilding2(this.Id, buildings[1].Id);
                else
                    Database.SetRanchBuilding2(this.Id, 0);
                if (buildings[2] != null)
                    Database.SetRanchBuilding3(this.Id, buildings[2].Id);
                else
                    Database.SetRanchBuilding3(this.Id, 0);
                if (buildings[3] != null)
                    Database.SetRanchBuilding4(this.Id, buildings[3].Id);
                else
                    Database.SetRanchBuilding4(this.Id, 0);
                if (buildings[4] != null)
                    Database.SetRanchBuilding5(this.Id, buildings[4].Id);
                else
                    Database.SetRanchBuilding5(this.Id, 0);
                if (buildings[5] != null)
                    Database.SetRanchBuilding6(this.Id, buildings[5].Id);
                else
                    Database.SetRanchBuilding6(this.Id, 0);
                if (buildings[6] != null)
                    Database.SetRanchBuilding7(this.Id, buildings[6].Id);
                else
                    Database.SetRanchBuilding7(this.Id, 0);
                if (buildings[7] != null)
                    Database.SetRanchBuilding8(this.Id, buildings[7].Id);
                else
                    Database.SetRanchBuilding8(this.Id, 0);
                if (buildings[8] != null)
                    Database.SetRanchBuilding9(this.Id, buildings[8].Id);
                else
                    Database.SetRanchBuilding9(this.Id, 0);
                if (buildings[9] != null)
                    Database.SetRanchBuilding10(this.Id, buildings[9].Id);
                else
                    Database.SetRanchBuilding10(this.Id, 0);
                if (buildings[10] != null)
                    Database.SetRanchBuilding11(this.Id, buildings[10].Id);
                else
                    Database.SetRanchBuilding11(this.Id, 0);
                if (buildings[11] != null)
                    Database.SetRanchBuilding12(this.Id, buildings[11].Id);
                else
                    Database.SetRanchBuilding12(this.Id, 0);
                if (buildings[12] != null)
                    Database.SetRanchBuilding13(this.Id, buildings[12].Id);
                else
                    Database.SetRanchBuilding13(this.Id, 0);
                if (buildings[13] != null)
                    Database.SetRanchBuilding14(this.Id, buildings[13].Id);
                else
                    Database.SetRanchBuilding14(this.Id, 0);
                if (buildings[14] != null)
                    Database.SetRanchBuilding15(this.Id, buildings[14].Id);
                else
                    Database.SetRanchBuilding15(this.Id, 0);
                if (buildings[15] != null)
                    Database.SetRanchBuilding16(this.Id, buildings[15].Id);
                else
                    Database.SetRanchBuilding16(this.Id, 0);
            }
        }
        

        public Ranch(int x, int y, int id, int value)
        {
            X = x;
            Y = y;
            Id = id;
            Value = value;
            Title = "";
            Description = "";
            UpgradedLevel = 0;
            OwnerId = -1;
            InvestedMoney = 0;
            for (int i = 0; i < 16; i++)
                buildings[i] = null;

            if (Database.IsRanchOwned(id))
            {
                UpgradedLevel = Database.GetRanchUpgradeLevel(id);
                Title = Database.GetRanchTitle(id);
                Description = Database.GetRanchDescription(id);
                OwnerId = Database.GetRanchOwner(id);
                int b1 = Database.GetRanchBuilding1(id);
                int b2 = Database.GetRanchBuilding2(id);
                int b3 = Database.GetRanchBuilding3(id);
                int b4 = Database.GetRanchBuilding4(id);
                int b5 = Database.GetRanchBuilding5(id);
                int b6 = Database.GetRanchBuilding6(id);
                int b7 = Database.GetRanchBuilding7(id);
                int b8 = Database.GetRanchBuilding8(id);
                int b9 = Database.GetRanchBuilding9(id);
                int b10 = Database.GetRanchBuilding10(id);
                int b11 = Database.GetRanchBuilding11(id);
                int b12 = Database.GetRanchBuilding12(id);
                int b13 = Database.GetRanchBuilding13(id);
                int b14 = Database.GetRanchBuilding14(id);
                int b15 = Database.GetRanchBuilding15(id);
                int b16 = Database.GetRanchBuilding16(id);

                if (RanchBuilding.RanchBuildingExists(b1))
                    buildings[0] = RanchBuilding.GetRanchBuildingById(b1);
                if (RanchBuilding.RanchBuildingExists(b2))
                    buildings[1] = RanchBuilding.GetRanchBuildingById(b2);
                if (RanchBuilding.RanchBuildingExists(b3))
                    buildings[2] = RanchBuilding.GetRanchBuildingById(b3);
                if (RanchBuilding.RanchBuildingExists(b4))
                    buildings[3] = RanchBuilding.GetRanchBuildingById(b4);
                if (RanchBuilding.RanchBuildingExists(b5))
                    buildings[4] = RanchBuilding.GetRanchBuildingById(b5);
                if (RanchBuilding.RanchBuildingExists(b6))
                    buildings[5] = RanchBuilding.GetRanchBuildingById(b6);
                if (RanchBuilding.RanchBuildingExists(b7))
                    buildings[6] = RanchBuilding.GetRanchBuildingById(b7);
                if (RanchBuilding.RanchBuildingExists(b8))
                    buildings[7] = RanchBuilding.GetRanchBuildingById(b8);
                if (RanchBuilding.RanchBuildingExists(b9))
                    buildings[8] = RanchBuilding.GetRanchBuildingById(b9);
                if (RanchBuilding.RanchBuildingExists(b10))
                    buildings[9] = RanchBuilding.GetRanchBuildingById(b10);
                if (RanchBuilding.RanchBuildingExists(b11))
                    buildings[10] = RanchBuilding.GetRanchBuildingById(b11);
                if (RanchBuilding.RanchBuildingExists(b12))
                    buildings[11] = RanchBuilding.GetRanchBuildingById(b12);
                if (RanchBuilding.RanchBuildingExists(b13))
                    buildings[12] = RanchBuilding.GetRanchBuildingById(b13);
                if (RanchBuilding.RanchBuildingExists(b14))
                    buildings[13] = RanchBuilding.GetRanchBuildingById(b14);
                if (RanchBuilding.RanchBuildingExists(b15))
                    buildings[14] = RanchBuilding.GetRanchBuildingById(b15);
                if (RanchBuilding.RanchBuildingExists(b16))
                    buildings[15] = RanchBuilding.GetRanchBuildingById(b16);


                InvestedMoney = Database.GetRanchInvestment(id);
            }
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