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

            // Give ALL players water ballons

            int[] allUsers = Database.GetUsers();
            foreach (int userid in allUsers)
            {
                Logger.DebugPrint("Adding Water Balloon to userid: " + userid.ToString());
                for (int i = 0; i < 6; i++)
                {
                    ItemInstance itm = new ItemInstance(Item.WaterBalloon);

                    if (GameServer.IsUserOnline(userid))
                        GameServer.GetUserById(userid).Inventory.AddWithoutDatabase(itm);

                    Database.AddItemToInventory(userid, itm);
                }
            }
        }

        public void EndEvent()
        {
            ThrownCounter[] winnerCounter = getWinners();
            resetEvent();

            foreach(GameClient connectedClient in GameServer.ConnectedClients)
            {
                if(connectedClient.LoggedIn)
                    if(connectedClient.LoggedinUser.Inventory.HasItemId(Item.WaterBalloon))
                    {
                        InventoryItem invItm = connectedClient.LoggedinUser.Inventory.GetItemByItemId(Item.WaterBalloon);
                        foreach (ItemInstance itm in invItm.ItemInstances)
                            connectedClient.LoggedinUser.Inventory.Remove(itm);
                    }
            }
            DroppedItems.DeleteAllItemOfType(Item.WaterBalloon); // Delete all dropped items
            Database.EradicateItemFromExistance(Item.WaterBalloon); // Delete from offline players


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
                byte[] youWinMsg = PacketBuilder.CreateChat(winMsg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                winner.UserHit.Money += 20000;
                winner.UserHit.LoggedinClient.SendPacket(youWinMsg);
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
