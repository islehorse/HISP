using HISP.Game.Items;
using System.Collections.Generic;
using System.Linq;


namespace HISP.Game.Horse
{
    public class Leaser
    {
        private static List<Leaser> horseLeasers = new List<Leaser>();
        public static void AddHorseLeaser(Leaser leaser)
        {
            horseLeasers.Add(leaser);
        }

        public static Leaser[] HorseLeasers
        {
            get
            {
                return horseLeasers.ToArray();
            }
        }
        public Leaser(int breedId, int saddle, int saddlePad, int bridle)
        {
            Breed = HorseInfo.GetBreedById(breedId);

            if (saddle != -1)
                Saddle = Item.GetItemById(saddle);
            if (saddlePad != -1)
                SaddlePad = Item.GetItemById(saddlePad);
            if (bridle != -1)
                Bridle = Item.GetItemById(bridle);


        }


        public int LeaseId;
        public string ButtonId;
        public string Info;
        public string OnLeaseText;
        public int Price;
        public int Minutes;

        // Horse

        public HorseInfo.Breed Breed;
        public string HorseName;
        public string Color;
        public string Gender;

        public int Health;
        public int Shoes;
        public int Hunger;
        public int Thirst;
        public int Mood;
        public int Groom;
        public int Tiredness;
        public int Experience;

        public Item.ItemInformation Saddle = null;
        public Item.ItemInformation SaddlePad = null;
        public Item.ItemInformation Bridle = null;

        public int Speed;
        public int Strength;
        public int Conformation;
        public int Agility;
        public int Inteligence;
        public int Endurance;
        public int Personality;
        public int Height;

        public HorseInstance GenerateLeaseHorse()
        {
            HorseInstance instance = new HorseInstance(this.Breed, loadColor: this.Color, loadCategory: "LEASED", leaseTimer: this.Minutes);
            instance.Name = this.HorseName;
            instance.Gender = this.Gender;
            instance.Leaser = this.LeaseId;

            instance.BasicStats = new HorseInfo.BasicStats(instance, Health, Shoes, Hunger, Thirst, Mood, Groom, Tiredness, Experience);
            instance.AdvancedStats = new HorseInfo.AdvancedStats(instance, Speed, Strength, Conformation, Agility, Inteligence, Endurance, Personality, Height);

            instance.Equipment.Saddle = this.Saddle;
            instance.Equipment.SaddlePad = this.SaddlePad;
            instance.Equipment.Bridle = this.Bridle;

            return instance;
        }

        public static bool LeaserButtonIdExists(string buttonId)
        {
            return HorseLeasers.Any(o => o.ButtonId == buttonId);
        }

        public static Leaser GetLeaserByButtonId(string buttonId)
        {
            return HorseLeasers.First(o => o.ButtonId == buttonId);
        }

        public static Leaser[] GetLeasersById(int id)
        {
            return HorseLeasers.Where(o => o.LeaseId == id).ToArray();
        }

    }
}
