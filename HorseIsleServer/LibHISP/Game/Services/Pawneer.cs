using HISP.Game.Horse;
using System;
using System.Collections.Generic;


namespace HISP.Game.Services
{
    public class Pawneer
    {
        public Pawneer(int breedId, int basePrice)
        {
            BreedId = breedId;
            BasePrice = basePrice;
        }
        private static List<Pawneer> pawneerPriceModels = new List<Pawneer>();
        private static Pawneer[] PawneerPriceModels
        {
            get
            {
                return pawneerPriceModels.ToArray();
            }
        }
        public static void AddPawneerPriceModel(Pawneer pawneerPrice)
        {
            pawneerPriceModels.Add(pawneerPrice);
        }
        public int BreedId;
        public int BasePrice;


        public static int GetPawneerBasePriceForHorse(HorseInfo.Breed breed)
        {
            foreach (Pawneer ppm in PawneerPriceModels)
            {
                if (ppm.BreedId == breed.Id)
                {
                    return ppm.BasePrice;
                }
            }

            throw new Exception("No pawneeer base price found >_> for breed #" + breed.Id + " " + breed.Name);
        }
        public static int CalculateTotalPrice(HorseInstance horse)
        {
            HorseInfo.StatCalculator speedStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.SPEED);
            HorseInfo.StatCalculator strengthStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.STRENGTH);
            HorseInfo.StatCalculator conformationStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.CONFORMATION);
            HorseInfo.StatCalculator agilityStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.AGILITY);
            HorseInfo.StatCalculator enduranceStat = new HorseInfo.StatCalculator(horse, HorseInfo.StatType.ENDURANCE);

            int basePrice = GetPawneerBasePriceForHorse(horse.Breed);

            int additionalPrice = speedStat.BreedOffset * 350;
            additionalPrice += strengthStat.BreedOffset * 350;
            additionalPrice += conformationStat.BreedOffset * 350;
            additionalPrice += agilityStat.BreedOffset * 350;
            additionalPrice += enduranceStat.BreedOffset * 350;

            additionalPrice += horse.BasicStats.Health * 40;
            additionalPrice += horse.BasicStats.Shoes * 20;

            int price = basePrice + additionalPrice;
            return price;
        }
    }
}
