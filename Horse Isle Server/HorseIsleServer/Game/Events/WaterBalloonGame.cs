using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Player;
using HISP.Server;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Game.Events
{

    public class WaterBalloonGame
    {
        public WaterBalloonGame()
        {
            ThrownWaterBalloonMemory = new List<ThrownCounter>();
            Active = false;
        }


        public List<ThrownCounter> ThrownWaterBalloonMemory;
        public bool Active;
        private Timer gameTimeout;
        private const int WATER_BALLOON_GAME_TIMEOUT = 5;
        public class ThrownCounter
        {
            public ThrownCounter(WaterBalloonGame game, User userHit, int numThrown)
            {
                UserHit = userHit;
                NumThrown = numThrown;
                baseGame = game;

                game.ThrownWaterBalloonMemory.Add(this);
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
            foreach (GameClient client in GameServer.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(gameStartMessage);

            GameServer.AddItemToAllUsersEvenOffline(Item.WaterBalloon, 8);
        }

        public void EndEvent()
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
            foreach (GameClient client in GameServer.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(gameWinnerAnnoucement);

            // payout / tell ppl they won.
            foreach (ThrownCounter winner in winnerCounter)
            {
                byte[] youWinMsg = PacketBuilder.CreateChat(Messages.EventWonWaterBallonGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                winner.UserHit.Money += 20000;
                winner.UserHit.LoggedinClient.SendPacket(youWinMsg);
                winner.UserHit.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WaterbaloonGameWin).Count++;
            }


        }
        private void gameTimedOut(object state)
        {
            EndEvent();
        }

        private void resetEvent()
        {
            gameTimeout.Dispose();
            gameTimeout = null;
            ThrownWaterBalloonMemory.Clear();
            Active = false;
        }
        private ThrownCounter[] getWinners()
        {
            int maxThrown = 0;
            ThrownCounter[] thrownWaterBalloonMemory = ThrownWaterBalloonMemory.ToArray();
            List<ThrownCounter> winningCounter = new List<ThrownCounter>();

            // Find the highest throw count
            foreach(ThrownCounter throwMemory in thrownWaterBalloonMemory)
            {
                if(throwMemory.NumThrown >= maxThrown)
                {
                    maxThrown = throwMemory.NumThrown;
                }
            }

            // Find all with that throw count and add to winner list
            foreach (ThrownCounter throwMemory in thrownWaterBalloonMemory)
            {
                if (throwMemory.NumThrown == maxThrown)
                {
                    winningCounter.Add(throwMemory);
                }
            }

            return winningCounter.ToArray();
        }

        public void LeaveEvent(User userToLeave)
        {
            foreach (ThrownCounter thrownMemory in ThrownWaterBalloonMemory.ToArray())
            {
                if (thrownMemory.UserHit.Id == userToLeave.Id)
                    ThrownWaterBalloonMemory.Remove(thrownMemory);
            }
        }

        private ThrownCounter getThrownCounter(User userToGet)
        {
            foreach(ThrownCounter thrownMemory in ThrownWaterBalloonMemory.ToArray())
            {
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
