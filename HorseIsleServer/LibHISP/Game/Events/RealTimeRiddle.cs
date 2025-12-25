using HISP.Player;
using HISP.Server;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Game.Events
{
    public class RealTimeRiddle : IEvent
    {
        private const int RIDDLE_TIMEOUT = 5;

        public int RiddleId;
        public string RiddleText;
        public string[] Answers;
        public bool Active;
        public int Reward;

        public static bool LastRiddleWon = false;
        private Timer riddleTimeout;
        private static List<RealTimeRiddle> realTimeRiddles = new List<RealTimeRiddle>();
        public RealTimeRiddle(int riddleId, string riddleText, string[] answers, int reward)
        {
            RiddleId = riddleId;
            RiddleText = riddleText;
            Answers = answers;
            Reward = reward;
            Active = false;
            realTimeRiddles.Add(this);
        }

        public static RealTimeRiddle GetRandomRiddle()
        {
            int randomRiddleIndex = GameServer.RandomNumberGenerator.Next(0, realTimeRiddles.Count);
            return realTimeRiddles[randomRiddleIndex];
        }
        public void StartEvent()
        {
            Active = true;
            riddleTimeout = new Timer(new TimerCallback(riddleTimedOut), null, RIDDLE_TIMEOUT * 60 * 1000, RIDDLE_TIMEOUT * 60 * 1000);

            // Send riddle message to all players
            foreach(GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    ShowStartMessage(client);
            }
        }

        public void ShowStartMessage(GameClient client)
        {
            if (!Active)
                return;
            byte[] riddleStartMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeRiddleStart(RiddleText), PacketBuilder.CHAT_BOTTOM_RIGHT);
            client.SendPacket(riddleStartMessage);
        }
        public void Win(User winner)
        {
            if (!Active)
                return;

            if (Database.HasPlayerCompletedRealTimeRiddle(RiddleId, winner.Id))
            {
                byte[] alreadyWonRiddleMessage = PacketBuilder.CreateChat(Messages.EventAlreadySovledRealTimeRiddle, PacketBuilder.CHAT_BOTTOM_RIGHT);
                winner.Client.SendPacket(alreadyWonRiddleMessage);
                return;
            }

            LastRiddleWon = true;

            Database.CompleteRealTimeRiddle(RiddleId, winner.Id);

            winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.RiddleWin).Count++;

            if (winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.RiddleWin).Count >= 25)
                winner.Awards.AddAward(Award.GetAwardById(33)); // Quick Wit
            if (winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.RiddleWin).Count >= 250)
                winner.Awards.AddAward(Award.GetAwardById(34)); // Riddle Genius


            winner.AddMoney(Reward);
            byte[] riddleWonMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeRiddleWonForOthers(winner.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            byte[] riddleYouWonMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeRiddleWonForYou(Reward), PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.User.Id != winner.Id)
                        client.SendPacket(riddleWonMessage);
                    else
                        client.SendPacket(riddleYouWonMessage);
            }
            StopEvent();
        }
        public void StopEvent()
        {
            if(this.Active)
            {
                this.Active = false;

                riddleTimeout.Dispose();
                riddleTimeout = null;
            }
        }

        private void riddleTimedOut(object state)
        {
            byte[] riddleTimedOutMessage = PacketBuilder.CreateChat(Messages.EventEndRealTimeRiddle, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
            {
                if (client.LoggedIn)
                    client.SendPacket(riddleTimedOutMessage);
            }
            LastRiddleWon = false;
            StopEvent();
        }

        public bool CheckRiddle(string message)
        {
            string msgCheck = message.ToLower();
            foreach(string answer in Answers)
            {
                if (msgCheck.Contains(answer.ToLower()))
                    return true;
            }
            return false;
        }
    }
}
