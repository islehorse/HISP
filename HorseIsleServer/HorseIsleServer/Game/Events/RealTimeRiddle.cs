using HISP.Player;
using HISP.Server;
using System.Collections.Generic;
using System.Threading;

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
            Active = false;
            RealTimeRiddles.Add(this);
        }
        public int RiddleId;
        public string RiddleText;
        public string[] Answers;
        public bool Active;
        public int Reward;
        public static bool LastWon = false;
        private Timer riddleTimeout;
        private const int RIDDLE_TIMEOUT = 5;

        public static RealTimeRiddle GetRandomRiddle()
        {
            int randomRiddleIndex = GameServer.RandomNumberGenerator.Next(0, RealTimeRiddles.Count);
            return RealTimeRiddles[randomRiddleIndex];
        }
        public void StartEvent()
        {
            Active = true;
            riddleTimeout = new Timer(new TimerCallback(riddleTimedOut), null, RIDDLE_TIMEOUT * 60 * 1000, RIDDLE_TIMEOUT * 60 * 1000);

            // Send riddle message to all players
            foreach(GameClient client in GameServer.ConnectedClients)
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
                return;

            LastWon = true;

            Database.CompleteRealTimeRiddle(RiddleId, winner.Id);

            winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.RiddleWin).Count++;

            if (winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.RiddleWin).Count >= 25)
                winner.Awards.AddAward(Award.GetAwardById(33)); // Quick Wit
            if (winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.RiddleWin).Count >= 250)
                winner.Awards.AddAward(Award.GetAwardById(34)); // Riddle Genius


            winner.AddMoney(Reward);
            byte[] riddleWonMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeRiddleWonForOthers(winner.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            byte[] riddleYouWonMessage = PacketBuilder.CreateChat(Messages.FormatEventRealTimeRiddleWonForYou(Reward), PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameServer.ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.Id != winner.Id)
                        client.SendPacket(riddleWonMessage);
                    else
                        client.SendPacket(riddleYouWonMessage);
            }
            EndEvent();
        }
        public void EndEvent()
        {
            Active = false;

            riddleTimeout.Dispose();
            riddleTimeout = null;
        }

        private void riddleTimedOut(object state)
        {
            byte[] riddleTimedOutMessage = PacketBuilder.CreateChat(Messages.EventEndRealTimeRiddle, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameServer.ConnectedClients)
            {
                if (client.LoggedIn)
                    client.SendPacket(riddleTimedOutMessage);
            }
            LastWon = false;
            EndEvent();
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
