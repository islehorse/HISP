using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game
{
    class Horse
    {
        public struct Stats
        {
            public int Speed;
            public int Strength;
            public int Conformation;
            public int Agility;
            public int Endurance;
            public int Inteligence;
            public int Personality;
            public int Height;
            public int MinHeight;
            public int MaxHeight;
        }

        public struct Breed
        {
            public int Id;
            public string Name;
            public string Description;
            public Stats BaseStats;
            public string[] Colors;
            public string SpawnOn;
            public string SpawnInArea;
            public string Swf;
            public string Type;
        }

        public static List<Breed> Breeds = new List<Breed>();

    }
}
