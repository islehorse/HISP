using HISP.Player;
using HISP.Server;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Game.Events
{
    public class RealTimeQuiz
    {
        public class QuizQuestion
        {
            public QuizQuestion(QuizCategory category)
            {
                BaseCategory = category;
            }
            public QuizCategory BaseCategory;
            public string Question;
            public string[] Answers;
        }
        public class QuizCategory
        {
            public string Name;
            public QuizQuestion[] Questions;
        }

        public class Participent
        {
            public Participent(User user, RealTimeQuiz Quiz)
            {
                UserInstance = user;
                Won = false;
                Quit = false;
                CorrectAnswers = 0;
                MistakenAnswers = 0;
                baseQuiz = Quiz;
                NextQuestion();
            }

            public void NextQuestion()
            {
                OnQuestion = baseQuiz.Questions[CorrectAnswers++];
            }

            public void UpdateParticipent()
            {
                if (this.Quit)
                    return;

                byte[] realTimeQuizQuestion = PacketBuilder.CreateMetaPacket(Meta.BuildRealTimeQuiz(this));
                this.UserInstance.LoggedinClient.SendPacket(realTimeQuizQuestion);
            }

            public void CheckAnswer(string answer)
            {
                foreach (string correctAnswer in OnQuestion.Answers)
                {
                    if(answer.ToLower().Trim() == correctAnswer.ToLower().Trim())
                    {
                        if(CorrectAnswers >= baseQuiz.Questions.Length)
                        {
                            baseQuiz.WinEvent(UserInstance);
                            return;
                        }

                        NextQuestion();
                        UpdateParticipent();
                        return;
                    }
                    if (answer.ToLower().Trim() == "quit")
                    {
                        baseQuiz.QuitEvent(UserInstance);
                        return;
                    }
                }
                MistakenAnswers++;
                UpdateParticipent();
            }

            private RealTimeQuiz baseQuiz;
            public User UserInstance;
            public int CorrectAnswers;
            public int MistakenAnswers;
            public bool Quit;
            public bool Won;
            public QuizQuestion OnQuestion;
        }

        public static QuizCategory[] Categories;
        public QuizQuestion[] Questions;
        public bool Active;
        public const int QUIZ_TIMEOUT = 5;
        private Timer quizTimer;
        public Participent[] Participents
        {
            get
            {
                return participents.ToArray();
            }
        }
        private List<Participent> participents;

        public RealTimeQuiz()
        {
            participents = new List<Participent>();
            Questions = new QuizQuestion[8];
            for(int i = 0; i < 8; i++)
            {
                QuizCategory chosenCategory = Categories[GameServer.RandomNumberGenerator.Next(0, Categories.Length)];
                Questions[i] = chosenCategory.Questions[GameServer.RandomNumberGenerator.Next(0, chosenCategory.Questions.Length)];
            }
            Active = false;

        }

        
        private Participent getParticipent(int id)
        {
            foreach (Participent participent in Participents)
            {
                if (participent.UserInstance.Id == id)
                {
                    return participent;
                }
            }
            throw new KeyNotFoundException("No participent found.");
        }
        public Participent JoinEvent(User user)
        {
            try
            {
                return getParticipent(user.Id);
            }
            catch (KeyNotFoundException) { };

            Participent newParticipent = new Participent(user, this);
            user.InRealTimeQuiz = true;
            participents.Add(newParticipent);


            return newParticipent;
        }

        public void LeaveEvent(User user)
        {
            try
            {
                Participent partcipent = getParticipent(user.Id);
                user.InRealTimeQuiz = false;
                participents.Remove(partcipent);
                partcipent = null;
            }
            catch (KeyNotFoundException) { };
        }

        public void QuitEvent(User user)
        {
            try
            {
                Participent partcipent = getParticipent(user.Id);
                partcipent.Quit = true;
                user.InRealTimeQuiz = false;
                GameServer.UpdateArea(user.LoggedinClient);
            }
            catch (KeyNotFoundException) { };
        }

        public void StartEvent()
        {
            quizTimer = new Timer(new TimerCallback(quizTimesUp), null, QUIZ_TIMEOUT * 60 * 1000, QUIZ_TIMEOUT * 60 * 1000);

            byte[] quizStartMessage = PacketBuilder.CreateChat(Messages.EventStartRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(quizStartMessage);

            Active = true;

            GameServer.QuizEvent = this;
        }

        public void WinEvent(User winner)
        {
            byte[] eventWinMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeQuizWin(winner.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(eventWinMessage);

            getParticipent(winner.Id).Won = true;

            winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.QuizWin).Count++;
            if (winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.QuizWin).Count >= 15)
                winner.Awards.AddAward(Award.GetAwardById(54)); // Quiz Genius
            if (winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.QuizWin).Count >= 25)
                winner.Awards.AddAward(Award.GetAwardById(33)); // Quick Wit

            stopEvent();
        }

        public void EndEvent()
        {
            byte[] eventEndMessage = PacketBuilder.CreateChat(Messages.EventEndRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
                if(client.LoggedIn)
                    client.SendPacket(eventEndMessage);
            
            stopEvent();
        }
        private void stopEvent()
        {
            foreach(Participent participent in Participents)
            {
                
                if (participent.Quit)
                    continue;


                participent.UserInstance.InRealTimeQuiz = false;
                GameServer.UpdateArea(participent.UserInstance.LoggedinClient);

                int money = 0;

                if (participent.Won)
                    money += 2500;
                else
                    money += 250;

                money += (participent.CorrectAnswers * 500);
                money -= (participent.MistakenAnswers * 100);

                if (money < 250)
                    money = 250;


                if (participent.Won)
                {
                    byte[] wonBonusMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeQuizWinBonus(money), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    participent.UserInstance.LoggedinClient.SendPacket(wonBonusMessage);
                }
                else 
                {
                    byte[] bonusMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeQuizBonus(money), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    participent.UserInstance.LoggedinClient.SendPacket(bonusMessage);
                }

                participent.UserInstance.AddMoney(money);
              
            }

            participents.Clear();
            participents = null;
            quizTimer.Dispose();

            Active = false;
            GameServer.QuizEvent = null;
        }

        private void quizTimesUp(object state)
        {
            EndEvent();
        }
    }
}
