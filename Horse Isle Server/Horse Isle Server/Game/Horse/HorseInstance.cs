
using HISP.Security;
using HISP.Server;

namespace HISP.Game.Horse
{
    class HorseInstance
    {
        public HorseInstance(HorseInfo.Breed breed, int randomId = -1)
        {
            RandomId = RandomID.NextRandomId(randomId);
            Owner = 0;
            if(breed.Type == "camel")
            {
                Name = "Wild Camel";
                if (GameServer.RandomNumberGenerator.Next(0, 100) >= 50)
                {
                    Sex = "cow";
                }
                else
                {
                    Sex = "bull";
                }

            }
            else if(breed.Type == "llama")
            {
                Name = "Jungle Llama";
                if(GameServer.RandomNumberGenerator.Next(0, 100) >= 50)
                {
                    Sex = "male";
                }
                else
                {
                    Sex = "female";
                }
            }
            else if(breed.Type == "zebra")
            {
                Name = "Wild Zebra";
                if (GameServer.RandomNumberGenerator.Next(0, 100) >= 50)
                {
                    Sex = "stallion";
                }
                else
                {
                    Sex = "mare";
                }
            }
            else
            {
                Name = "Wild Horse";
                if (GameServer.RandomNumberGenerator.Next(0,100) >= 50)
                {
                    Sex = "stallion";
                }
                else
                {
                    Sex = "mare";
                }
            }

            Description = "";
            Breed = breed;
            Color = breed.Colors[GameServer.RandomNumberGenerator.Next(0, breed.Colors.Length)];

            BasicStats = new HorseInfo.BasicStats();
            BasicStats.Health = 1000;
            BasicStats.Shoes = 0;
            BasicStats.Hunger = 1000;
            BasicStats.Thirst = 1000;
            BasicStats.Mood = 500;
            BasicStats.Groom = 1000;
            BasicStats.Tiredness = 1000;
            BasicStats.Experience = 0;

            AdvancedStats = new HorseInfo.AdvancedStats();
            AdvancedStats.Speed = 0;
            AdvancedStats.Strength = 0;
            AdvancedStats.Conformation = 0;
            AdvancedStats.Agility = 0;
            AdvancedStats.Endurance = 0;
            AdvancedStats.Inteligence = (GameServer.RandomNumberGenerator.Next(breed.BaseStats.Inteligence, breed.BaseStats.Inteligence * 2)) - breed.BaseStats.Inteligence;
            AdvancedStats.Personality = (GameServer.RandomNumberGenerator.Next(breed.BaseStats.Personality, breed.BaseStats.Personality * 2)) - breed.BaseStats.Personality;
            AdvancedStats.Height = GameServer.RandomNumberGenerator.Next(breed.BaseStats.MinHeight, breed.BaseStats.MaxHeight);

            Equipment = new HorseInfo.HorseEquips();
            AutoSell = 0;
            Category = "KEEPER";
            Spoiled = 0;
            MagicUsed = 0;
            TrainTimer = 0;
            RanchId = 0;
            Leaser = 0;
        }
        public int RanchId;
        public int Leaser;
        public int RandomId;
        public int Owner;
        public string Name;
        public string Description;
        public string Sex;
        public string Color;
        public int TrainTimer;
        public HorseInfo.Breed Breed;
        public HorseInfo.BasicStats BasicStats;
        public HorseInfo.AdvancedStats AdvancedStats;
        public HorseInfo.HorseEquips Equipment;
        public int AutoSell;
        public int Spoiled;
        public int MagicUsed;
        public string Category;
        
    }
}
