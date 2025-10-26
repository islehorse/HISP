using System.Collections.Generic;
using System.Linq;

namespace HISP.Game.Services
{
    public class Trainer
    {
        public static List<Trainer> Trainers = new List<Trainer>();

        public int Id;
        public string ImprovesStat;
        public int ImprovesAmount;
        public int ThirstCost;
        public int MoodCost;
        public int HungerCost;
        public int MoneyCost;
        public int ExperienceGained;

        public static Trainer GetTrainerById(int id)
        {
            return Trainers.First(o => o.Id == id);
        }
    }
}
