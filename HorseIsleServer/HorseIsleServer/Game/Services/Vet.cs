using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Services
{
    public class Vet
    {

        public static List<Vet> Vets = new List<Vet>();

        public Vet(int id, double price)
        {
            Id = id;
            PriceMultiplier = price;
            Vets.Add(this);
        }

        public int Id;
        public double PriceMultiplier; 
        public int CalculatePrice(int health)
        {
            double price = (1000.0 - (double)health) * PriceMultiplier;
            return Convert.ToInt32(Math.Round(price));
        }
        
        public static Vet GetVetById(int id)
        {
            foreach(Vet vet in Vets)
            {
                if (id == vet.Id)
                    return vet;
            }
            throw new KeyNotFoundException("Vet with id: " + id + " Not found.");
        }
    }
}
