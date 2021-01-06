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

        public struct Category
        {
            public string Name;
            public string Meta;
        }

        public static List<Category> HorseCategories = new List<Category>();
        public static List<Breed> Breeds = new List<Breed>();
        public static double CalculateHands(int height)
        {
            return ((double)height / 4.0);
        }
        public static string BreedViewerSwf(HorseInstance horse, string terrainTileType)
        {
            double hands = CalculateHands(horse.AdvancedStats.Height);

            string swf = "breedviewer.swf?terrain=" + terrainTileType + "&breed=" + horse.Breed.Swf + "&color=" + horse.Color + "&hands=" + hands.ToString();
            if (horse.Equipment.Saddle != null)
                swf += "&saddle=" + horse.Equipment.Saddle.EmbedSwf;
            if (horse.Equipment.SaddlePad != null)
                swf += "&saddlepad=" + horse.Equipment.SaddlePad.EmbedSwf;
            if (horse.Equipment.Bridle != null)
                swf += "&bridle=" + horse.Equipment.Bridle.EmbedSwf;
            if (horse.Equipment.Companion != null)
                swf += "&companion=" + horse.Equipment.Companion.EmbedSwf;
            swf += "&junk=";

            return swf;
        }
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
