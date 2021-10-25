using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

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
            foreach (Trainer trainer in Trainers)
                if (trainer.Id == id)
                    return trainer;

            throw new KeyNotFoundException("Trainer " + id + " not found");
        }
    }
}
