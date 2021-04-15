using HISP.Game.Items;
using System.Collections.Generic;


namespace HISP.Game.Horse
{
    public class Leaser
    {
        public static List<Leaser> HorseLeasers = new List<Leaser>();
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
            HorseInstance instance = new HorseInstance(this.Breed, -1, null, "", 0, "LEASED", 0, 0, this.Minutes);
            instance.Name = this.HorseName;
            instance.Color = this.Color;
            instance.Gender = this.Gender;
            instance.Leaser = this.LeaseId;

            instance.BasicStats = new HorseInfo.BasicStats(instance, Health, Shoes, Hunger, Thirst, Mood, Groom, Tiredness, Experience);
            instance.AdvancedStats = new HorseInfo.AdvancedStats(instance, Speed, Strength, Conformation, Agility, Inteligence, Endurance, Personality, Height);

            instance.Equipment.Saddle = this.Saddle;
            instance.Equipment.SaddlePad = this.SaddlePad;
            instance.Equipment.Bridle = this.Bridle;

            return instance;
        }

        public static bool LeaserButtonIdExists(string bid)
        {
            foreach (Leaser leaser in HorseLeasers)
            {
                if (leaser.ButtonId == bid)
                {
                    return true;
                }
            }
            return false; 
        }

        public static Leaser GetLeaserByButtonId(string bid)
        {
            foreach(Leaser leaser in HorseLeasers)
            {
                if(leaser.ButtonId == bid)
                {
                    return leaser;
                }
            }
            throw new KeyNotFoundException("No leaser with button id: " + bid + " found.");
        }

        public static Leaser[] GetLeasersById(int id)
        {
            List<Leaser> leasers = new List<Leaser>();

            foreach (Leaser leaser in HorseLeasers)
            {
                if (leaser.LeaseId == id)
                {
                    leasers.Add(leaser);
                }
            }
            return leasers.ToArray();
        }

    }
}
