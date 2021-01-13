
using HISP.Security;
using HISP.Server;

namespace HISP.Game.Horse
{
    class HorseInstance
    {
        public HorseInstance(HorseInfo.Breed breed, int randomId = -1, string loadName=null, string loadDescription = "", int loadSpoiled=0)
        {
            RandomId = RandomID.NextRandomId(randomId);
            Owner = 0;
            if(loadName == null)
            {

                if (breed.Type == "camel")
                {
                    name = "Wild Camel";
                    if (GameServer.RandomNumberGenerator.Next(0, 100) >= 50)
                    {
                        Sex = "cow";
                    }
                    else
                    {
                        Sex = "bull";
                    }

                }
                else if (breed.Type == "llama")
                {
                    name = "Jungle Llama";
                    if (GameServer.RandomNumberGenerator.Next(0, 100) >= 50)
                    {
                        Sex = "male";
                    }
                    else
                    {
                        Sex = "female";
                    }
                }
                else if (breed.Type == "zebra")
                {
                    name = "Wild Zebra";
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
                    name = "Wild Horse";
                    if (GameServer.RandomNumberGenerator.Next(0, 100) >= 50)
                    {
                        Sex = "stallion";
                    }
                    else
                    {
                        Sex = "mare";
                    }
                }
            }
            else
            {
                name = loadName;
            }

            description = loadDescription;
            Breed = breed;
            Color = breed.Colors[GameServer.RandomNumberGenerator.Next(0, breed.Colors.Length)];

            BasicStats = new HorseInfo.BasicStats(this, 1000, 0, 1000, 1000, 500, 1000, 1000, 0);
            int inteligence = (GameServer.RandomNumberGenerator.Next(breed.BaseStats.Inteligence, breed.BaseStats.Inteligence * 2)) - breed.BaseStats.Inteligence;
            int personality = (GameServer.RandomNumberGenerator.Next(breed.BaseStats.Personality, breed.BaseStats.Personality * 2)) - breed.BaseStats.Personality;
            int height = GameServer.RandomNumberGenerator.Next(breed.BaseStats.MinHeight, breed.BaseStats.MaxHeight);
            AdvancedStats = new HorseInfo.AdvancedStats(this, 0, 0, 0, 0, inteligence, 0, personality, height);

            Equipment = new HorseInfo.HorseEquips();
            AutoSell = 0;
            Category = "KEEPER";
            spoiled = loadSpoiled;
            MagicUsed = 0;
            TrainTimer = 0;
            RanchId = 0;
            Leaser = 0;
        }
        public int RanchId;
        public int Leaser;
        public int RandomId;
        public int Owner;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                Database.SetHorseName(this.RandomId, name);
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
                Database.SetHorseDescription(this.RandomId, value);
            }
        }
        public string Sex;
        public string Color;
        public int TrainTimer;
        public HorseInfo.Breed Breed;
        public HorseInfo.BasicStats BasicStats;
        public HorseInfo.AdvancedStats AdvancedStats;
        public HorseInfo.HorseEquips Equipment;
        public int AutoSell;
        public int Spoiled
        {
            get
            {
                return spoiled;
            }
            set
            {
                Database.SetHorseSpoiled(RandomId, value);
                spoiled = value;
            }
        }
        public int MagicUsed;
        public string Category;

        private string name;
        private string description;
        private int spoiled;


        
    }
}
