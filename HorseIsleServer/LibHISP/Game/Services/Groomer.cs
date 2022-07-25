using System;
using System.Collections.Generic;

namespace HISP.Game.Services
{
    public class Groomer
    {

        private static List<Groomer> groomers = new List<Groomer>();
        public static Groomer[] Groomers
        {
            get
            {
                return groomers.ToArray();
            }
        }
        public Groomer(int id, double price, int max)
        {
            Id = id;
            PriceMultiplier = price;
            Max = max;
            groomers.Add(this);
        }

        public int Id;
        public double PriceMultiplier;
        public int Max;
        public int CalculatePrice(int groom)
        {
            double price = ((double)Max - (double)groom) * PriceMultiplier;
            return Convert.ToInt32(Math.Round(price));
        }

        public static Groomer GetGroomerById(int id)
        {
            foreach (Groomer groomer in Groomers)
            {
                if (id == groomer.Id)
                    return groomer;
            }
            throw new KeyNotFoundException("Groomer with id: " + id + " Not found.");
        }
    }
}
