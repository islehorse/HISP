
using HISP.Security;
using HISP.Server;

namespace HISP.Game.Horse
{
    public class HorseInstance
    {
        public HorseInstance(HorseInfo.Breed breed, int randomId = -1, string loadName=null, string loadDescription = "", int loadSpoiled=0, string loadCategory="KEEPER", int loadMagicUsed=0, int loadAutoSell=0, int leaseTimer=0, bool loadHidden=false, int loadOwner=0)
        {
            RandomId = RandomID.NextRandomId(randomId);
            owner = loadOwner;
            if(loadName == null)
            {

                if (breed.Type == "camel")
                {
                    name = "Wild Camel";

                }
                else if (breed.Type == "llama")
                {
                    name = "Jungle Llama";
                }
                else if (breed.Type == "zebra")
                {
                    name = "Wild Zebra";
                }
                else
                {
                    name = "Wild Horse";
                }
            }
            else
            {
                name = loadName;
            }
            if(GameServer.RandomNumberGenerator.Next(0, 100) > 50)
                Gender = breed.GenderTypes()[1];
            else
                Gender = breed.GenderTypes()[0];

            description = loadDescription;
            Breed = breed;
            Color = breed.Colors[GameServer.RandomNumberGenerator.Next(0, breed.Colors.Length)];

            BasicStats = new HorseInfo.BasicStats(this, 1000, 0, 1000, 1000, 500, 1000, 1000, 0);
            int inteligence = (GameServer.RandomNumberGenerator.Next(breed.BaseStats.Inteligence, (breed.BaseStats.Inteligence * 2)-1) - breed.BaseStats.Inteligence);
            int personality = (GameServer.RandomNumberGenerator.Next(breed.BaseStats.Personality, (breed.BaseStats.Personality * 2)-1) - breed.BaseStats.Personality);
            int height = GameServer.RandomNumberGenerator.Next(breed.BaseStats.MinHeight, breed.BaseStats.MaxHeight);
            AdvancedStats = new HorseInfo.AdvancedStats(this, 0, 0, 0, 0, inteligence, 0, personality, height);

            Equipment = new HorseInfo.HorseEquips();
            autosell = loadAutoSell;
            category = loadCategory;
            spoiled = loadSpoiled;
            magicUsed = loadMagicUsed;
            leaseTime = leaseTimer;
            hidden = loadHidden;
            Leaser = 0;
        }
        public int Leaser;
        public int RandomId;
        public int owner;
        public int Owner 
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
                Database.SetHorseOwner(RandomId, owner);
            }
        }

        public bool Hidden
        {
            get
            {
                return hidden;
            }
            set
            {
                hidden = value;
                Database.SetHorseHidden(RandomId, value);
            }
        }
        public int LeaseTime
        {
            get
            {
                return leaseTime;
            }
            set
            {
                leaseTime = value;
                Database.SetLeaseTime(this.RandomId, leaseTime);
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value.Trim();
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
                description = value.Trim();
                Database.SetHorseDescription(this.RandomId, description);
            }
        }
        public string Gender;
        public string Color;
        public int TrainTimer
        {
            get
            {
                int timeout = Database.GetHorseTrainTimeout(this.RandomId);
                if (timeout < 0)
                    return 0;
                else
                    return timeout;
            }
            set
            {
                Database.SetHorseTrainTimeout(this.RandomId, value);
            }
        }
        public HorseInfo.Breed Breed;
        public HorseInfo.BasicStats BasicStats;
        public HorseInfo.AdvancedStats AdvancedStats;
        public HorseInfo.HorseEquips Equipment;
        public int AutoSell
        {
            get
            {
                return autosell;
            }
            set
            {
                Database.SetHorseAutoSell(RandomId, value);
                autosell = value;
            }
        }
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
        public int MagicUsed
        {
            get
            {
                return magicUsed;
            }
            set
            {
                Database.SetHorseMagicUsed(RandomId, value);
                magicUsed = value;
            }
        }
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                Database.SetHorseCategory(RandomId, value);
                category = value;
            }
        }

        private string name;
        private string description;
        private int spoiled;
        private int leaseTime;
        private bool hidden;
        private int magicUsed;
        private int autosell;
        private string category;

        public void ChangeNameWithoutUpdatingDatabase(string newName)
        {
            name = newName;
        }

        
    }
}
