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
            public Participent(User user)
            {
                UserInstance = user;
                Won = false;
                Quit = false;
                CorrectAnswers = 0;
                MistakenAnswers = 0;

            }

            public void NextQuestion()
            {
                QuizCategory chosenCategory = Categories[GameServer.RandomNumberGenerator.Next(0, Categories.Length)];
                OnQuestion = chosenCategory.Questions[GameServer.RandomNumberGenerator.Next(0, chosenCategory.Questions.Length)];
            }

            public void UpdateParticipent()
            {
                if (this.Quit)
                    return;

                byte[] realTimeQuizQuestion = PacketBuilder.CreateMetaPacket(Meta.BuildRealTimeQuiz(this));
                this.UserInstance.LoggedinClient.SendPacket(realTimeQuizQuestion);
            }

            public User UserInstance;
            public int CorrectAnswers;
            public int MistakenAnswers;
            public bool Quit;
            public bool Won;

            public QuizQuestion OnQuestion;
        }

        public static QuizCategory[] Categories;
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

            Participent newParticipent = new Participent(user);
            user.InRealTimeQuiz = true;
            participents.Add(newParticipent);


            return newParticipent;
        }

        public void LeaveEvent(User user, bool sendData=true)
        {
            try
            {
                Participent partcipent = getParticipent(user.Id);
                partcipent.Quit = true;
                user.InRealTimeQuiz = false;

                if(sendData)
                    if (user.LoggedinClient != null)
                        if(GameServer.IsUserOnline(user.Id))
                            GameServer.UpdateArea(user.LoggedinClient);
            }
            catch (KeyNotFoundException) { };
        }

        public void StartEvent()
        {
            quizTimer = new Timer(new TimerCallback(quizTimesUp), null, QUIZ_TIMEOUT * 60 * 1000, QUIZ_TIMEOUT * 60 * 1000);

            byte[] quizStartMessage = PacketBuilder.CreateChat(Messages.EventStartRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameServer.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(quizStartMessage);

            Active = true;

            GameServer.QuizEvent = this;
        }

        public void EndEvent()
        {
            byte[] eventEndMessage = PacketBuilder.CreateChat(Messages.EventEndRealTimeQuiz, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameServer.ConnectedClients)
                if(client.LoggedIn)
                    client.SendPacket(eventEndMessage);
            
            stopEvent();
        }
        private void stopEvent()
        {
            foreach(Participent participent in Participents)
            {
                if (participent.UserInstance.LoggedinClient != null)
                {
                    if (GameServer.IsUserOnline(participent.UserInstance.Id))
                    {
                        if (participent.Quit)
                            continue;

                        if (participent.UserInstance.InRealTimeQuiz)
                            GameServer.UpdateArea(participent.UserInstance.LoggedinClient);

                        participent.UserInstance.InRealTimeQuiz = false;

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

                        participent.UserInstance.Money += money;
                    }
                }
            }

            participents.Clear();
            participents = null;
            quizTimer.Dispose();

            Active = false;
            GameServer.TackShopGiveawayEvent = null;
        }

        private void quizTimesUp(object state)
        {
            EndEvent();
        }
    }
}
