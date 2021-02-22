using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Events
{
    public class RealTimeRiddle
    {
        public static List<RealTimeRiddle> RealTimeRiddles = new List<RealTimeRiddle>();
        public RealTimeRiddle(int riddleId, string riddleText, string[] answers, int reward)
        {
            RiddleId = riddleId;
            RiddleText = riddleText;
            Answers = answers;
            Reward = reward;
            RealTimeRiddles.Add(this);
        }
        public int RiddleId;
        public string RiddleText;
        public string[] Answers;
        public int Reward;


        public static RealTimeRiddle GetRandomRiddle()
        {
            int randomRiddleIndex = GameServer.RandomNumberGenerator.Next(0, RealTimeRiddles.Count);
            return RealTimeRiddles[randomRiddleIndex];
        }
        public bool CheckRiddle(string message)
        {
            string msgCheck = message.ToLower();
            foreach(string answer in Answers)
            {
                if (answer.ToLower().Contains(msgCheck))
                    return true;
            }
            return false;
        }
    }
}
