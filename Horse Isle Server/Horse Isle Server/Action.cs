using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Action
    {
        public enum ActivationMethod
        {
            MANUAL,
            SHOVEL,
            BINOCULARS,
            MAGNIFYING_GLASS,
            RAKE
        }
        public struct AltActivation
        {
            public ActivationMethod Method;
            public int ActivationX;
            public int ActivationY;
        }

        public struct Requirements
        {
            public bool Track;
            public int MaxCompletions;
            public int MoneyRequired;
            public int[] CompletedRequired;
            public int[] NotCompletedRequired;
            public int RequiredAward;
             
        }
        public struct GameAction
        {
            public int Id;
            public string Notes;
            public string AdventurousTitle;
            public string Difficulty;
            public string Author;
            public int[] ActionIdRequired;

            AltActivation Activation;

        }


    }
}
