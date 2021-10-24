using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Services
{
    public class Barn
    {
        public Barn(int id, double tiredCost, double hungerCost, double thirstCost)
        {
            this.Id = id;
            this.TiredCost = tiredCost;
            this.HungerCost = hungerCost;
            this.ThirstCost = thirstCost;
            barns.Add(this);
        }
        private static List<Barn> barns = new List<Barn>();
        public static Barn[] Barns
        {
            get
            {
                return barns.ToArray();
            }
        }

        public int Id;
        public double TiredCost;
        public double HungerCost;
        public double ThirstCost;
        public int CalculatePrice(int tiredness, int hunger, int thirst)
        {
            double tiredPrice = (1000.0 - (double)tiredness) * TiredCost;
            double hungerPrice = (1000.0 - (double)hunger) * HungerCost;
            double thirstPrice = (1000.0 - (double)thirst) * ThirstCost;
            return Convert.ToInt32(Math.Round(tiredPrice + hungerPrice + thirstPrice));
        }
        public static Barn GetBarnById(int id)
        {
            foreach (Barn barn in Barns)
                if (barn.Id == id)
                    return barn;
            throw new KeyNotFoundException("Barn id: " + id.ToString() + " Not found!");
        }

    }
}
