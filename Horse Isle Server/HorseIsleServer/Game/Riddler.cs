using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game
{
    public class Riddler
    {
        private static List<Riddler> riddlerRiddles = new List<Riddler>();
        public static Riddler[] Riddles
        {
            get
            {
                return riddlerRiddles.ToArray();
            }
        }
        public Riddler(int id, string riddle, string[] answers, string reason)
        {
            Id = id;
            Riddle = riddle;
            Answers = answers;
            Reason = reason;
            riddlerRiddles.Add(this);
        }
        public int Id;
        public string Riddle;
        public string[] Answers;
        public string Reason;
        public void AnswerSuccess(User user)
        {
            if (!Database.HasPlayerCompletedRiddle(this.Id, user.Id))
                Database.CompleteRiddle(this.Id, user.Id);

            byte[] riddleAnswerCorrectPacket = PacketBuilder.CreateChat(Messages.FormatRiddlerAnswerCorrect(this.Reason), PacketBuilder.CHAT_BOTTOM_RIGHT);
            user.LoggedinClient.SendPacket(riddleAnswerCorrectPacket);
            user.AddMoney(10000);

            if(HasCompletedAllRiddles(user))
                user.Awards.AddAward(Award.GetAwardById(11)); // Riddlers Riddles
        }

        public void AnswerFail(User user)
        {
            byte[] riddleIncorrect = PacketBuilder.CreateChat(Messages.RiddlerIncorrectAnswer, PacketBuilder.CHAT_BOTTOM_RIGHT);
            user.LoggedinClient.SendPacket(riddleIncorrect);
        }

        public bool CheckAnswer(User user, string txt)
        {
            foreach(string Answer in Answers)
            {
                if(Answer.ToLower() == txt.ToLower())
                {
                    AnswerSuccess(user);
                    return true;
                }
            }
            AnswerFail(user);
            return false;
        }

        public static bool HasCompletedAllRiddles(User user)
        {
            if (Database.TotalRiddlesCompletedByPlayer(user.Id) > Riddles.Length)
                return true;
            return false;
        }

        public static Riddler GetRandomRiddle(User user)
        {
            while(true)
            {

                int rng = GameServer.RandomNumberGenerator.Next(0, Riddles.Length);
                if (Database.HasPlayerCompletedRiddle(Riddles[rng].Id, user.Id))
                {
                    continue;
                }
                return Riddles[rng];
            }
        }
    }
}
