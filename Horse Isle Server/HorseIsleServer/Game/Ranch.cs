using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game
{
    public class Ranch
    {
        public class RanchBuilding
        {
            public static List<RanchBuilding> RanchBuildings = new List<RanchBuilding>();
            public int Id;
            public string Type;
            public int Cost;
            public string Title;
            public string Description;
            public int Limit;
        }
        public static List<Ranch> Ranches = new List<Ranch>();

        public int X;
        public int Y;
        public int Id;
        public int Value;

        public int OwnerId;
        public int UpgradedLevel;
        public int InvestedMoney;
        public string Title;
        public string Description;

        public RanchBuilding[] Buildings = new RanchBuilding[16];
        public Ranch(int x, int y, int id, int value)
        {
            X = x;
            Y = y;
            Id = id;
            Value = value;
            UpgradedLevel = 0;
            OwnerId = -1;
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
    }
}
