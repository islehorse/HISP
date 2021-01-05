using System.Collections.Generic;

namespace HISP.Game.Horse
{
    class HorseInfo
    {
        public struct AdvancedStats
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
        public struct BasicStats
        {
            public int Health;
            public int Shoes;
            public int Hunger;
            public int Thirst;
            public int Mood;
            public int Groom;
            public int Tiredness;
            public int Experience;
        }

        public struct Breed
        {
            public int Id;
            public string Name;
            public string Description;
            public AdvancedStats BaseStats;
            public string[] Colors;
            public string SpawnOn;
            public string SpawnInArea;
            public string Swf;
            public string Type;
        }

        public struct HorseEquips
        {
            public Item.ItemInformation Saddle;
            public Item.ItemInformation SaddlePad;
            public Item.ItemInformation Bridle;
            public Item.ItemInformation Companion;
        }



        public static List<Breed> Breeds = new List<Breed>();

        public static Breed GetBreedById(int id)
        {
            foreach(Breed breed in Breeds)
            {
                if (breed.Id == id)
                    return breed;
            }
            throw new KeyNotFoundException("No horse breed with id " + id);
        }
    }
}
