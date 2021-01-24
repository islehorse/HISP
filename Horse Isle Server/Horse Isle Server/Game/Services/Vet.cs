using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Services
{
    class Vet
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
            double price = ((double)health - 1000.0) * PriceMultiplier;
            return Convert.ToInt32(Math.Floor(price));
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
