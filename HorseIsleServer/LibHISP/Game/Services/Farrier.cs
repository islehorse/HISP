using System.Collections.Generic;

namespace HISP.Game.Services
{
    public class Farrier
    {
        private static List<Farrier> farriers = new List<Farrier>();
        public static Farrier[] Farriers 
        {
            get
            {
                return farriers.ToArray();
            }
        }

        public int Id;
        public int SteelShoesAmount;
        public int SteelCost;
        public int IronShoesAmount;
        public int IronCost;

        public Farrier(int id, int steelShoesInc, int steelCost, int ironShoesInc, int ironCost)
        {
            this.Id = id;
            this.SteelShoesAmount = steelShoesInc;
            this.SteelCost = steelCost;
            this.IronShoesAmount = ironShoesInc;
            this.IronCost = ironCost;
            farriers.Add(this);
        }

        public static Farrier GetFarrierById(int id)
        {
            foreach (Farrier farrier in Farriers)
                if (farrier.Id == id)
                    return farrier;
            throw new KeyNotFoundException("No farrier with id: " + id + " found.");
        }


    }
}
