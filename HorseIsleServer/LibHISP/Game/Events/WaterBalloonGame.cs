using HISP.Game.Items;
using HISP.Player;
using HISP.Server;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Game.Events
{

    public class WaterBalloonGame : IEvent
    {
        public const int WATER_BALLOON_GAME_TIMEOUT = 5;
        public WaterBalloonGame()
        {
            thrownWaterBalloonMemory = new List<ThrownCounter>();
            Active = false;
        }

        private List<ThrownCounter> thrownWaterBalloonMemory;
        public bool Active;
        private Timer gameTimeout;
        public ThrownCounter[] ThrownWaterBalloonMemory
        {
            get
            {
                return thrownWaterBalloonMemory.ToArray();
            }
        }

        public class ThrownCounter
        {
            public ThrownCounter(WaterBalloonGame game, User userHit, int numThrown)
            {
                UserHit = userHit;
                NumThrown = numThrown;
                baseGame = game;

                game.thrownWaterBalloonMemory.Add(this);
            }
            private WaterBalloonGame baseGame;
            public User UserHit;
            public int NumThrown;
        }


        public void StartEvent()
        {
            Active = true;
            gameTimeout = new Timer(new TimerCallback(gameTimedOut), null, WATER_BALLOON_GAME_TIMEOUT * 60 * 1000, WATER_BALLOON_GAME_TIMEOUT * 60 * 1000);

            byte[] gameStartMessage = PacketBuilder.CreateChat(Messages.EventStartWaterBallonGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(gameStartMessage);

            GameServer.AddItemToAllUsersEvenOffline(Item.WaterBalloon, 8);
        }

        public void StopEvent()
        {
            ThrownCounter[] winnerCounter = getWinners();
            resetEvent();

            GameServer.RemoveAllItemsOfIdInTheGame(Item.WaterBalloon);

            // Build event over message
            string winMsg = Messages.EventEndWaterBalloonGame;
            foreach(ThrownCounter winner in winnerCounter)
            {
                winMsg += Messages.FormatWaterBalloonGameWinner(winner.UserHit.Username, winner.NumThrown);
            }
            
            // Send to all online users
            byte[] gameWinnerAnnoucement = PacketBuilder.CreateChat(winMsg, PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(gameWinnerAnnoucement);

            // payout / tell ppl they won.
            foreach (ThrownCounter winner in winnerCounter)
            {
                byte[] youWinMsg = PacketBuilder.CreateChat(Messages.EventWonWaterBallonGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                winner.UserHit.AddMoney(20000);
                winner.UserHit.Client.SendPacket(youWinMsg);
                winner.UserHit.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WaterbaloonGameWin).Count++;
            }


        }
        private void gameTimedOut(object state)
        {
            StopEvent();
        }

        private void resetEvent()
        {
            gameTimeout.Dispose();
            gameTimeout = null;
            thrownWaterBalloonMemory.Clear();
            Active = false;
        }
        private ThrownCounter[] getWinners()
        {
            int maxThrown = 0;
            List<ThrownCounter> winningCounter = new List<ThrownCounter>();

            // Find the highest throw count
            foreach (ThrownCounter throwMemory in ThrownWaterBalloonMemory)
            {
                if (throwMemory == null)
                    continue;

                if(throwMemory.NumThrown >= maxThrown)
                {
                    maxThrown = throwMemory.NumThrown;
                }
            }

            // Find all with that throw count and add to winner list
            foreach (ThrownCounter throwMemory in ThrownWaterBalloonMemory)
            {
                if (throwMemory == null)
                    continue;

                if (throwMemory.NumThrown == maxThrown)
                {
                    winningCounter.Add(throwMemory);
                }
            }

            return winningCounter.ToArray();
        }

        public void LeaveEvent(User userToLeave)
        {
            foreach (ThrownCounter thrownMemory in ThrownWaterBalloonMemory)
            {
                if (thrownMemory == null)
                    continue;

                if (thrownMemory.UserHit.Id == userToLeave.Id)
                    thrownWaterBalloonMemory.Remove(thrownMemory);
            }
        }

        private ThrownCounter getThrownCounter(User userToGet)
        {
            foreach(ThrownCounter thrownMemory in ThrownWaterBalloonMemory)
            {
                if (thrownMemory == null)
                    continue;

                if (thrownMemory.UserHit.Id == userToGet.Id)
                    return thrownMemory;
            }

            return new ThrownCounter(this, userToGet, 0);
        }

        public void AddWaterBallon(User throwAt)
        {
            ThrownCounter throwCounter = getThrownCounter(throwAt);
            throwCounter.NumThrown++;
        }

    }
}
