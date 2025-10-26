using HISP.Game.Items;
using HISP.Player;
using HISP.Server;
using HISP.Util;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Game.Events
{
    public class ModsRevenge
    {
        public class ThrowTracker
        {
            public ThrowTracker(User thrower)
            {
                Thrower = thrower;
                thrownAt = new ThreadSafeList<User>();   
            }

            public void AddThrownAt(User user)
            {
                thrownAt.Add(user);
            }
            public User Thrower;
            private ThreadSafeList<User> thrownAt;
            public User[] ThrownAt
            {
                get
                {
                    return thrownAt.ToArray();
                }
            }
        }

        public bool Active = false;
        public const int REVENGE_TIMEOUT = 10;
        private ThreadSafeList<ThrowTracker> trackedThrows;
        private Timer revengeTimeout;
        public ThrowTracker[] TrackedThrows
        {
            get
            {
                return trackedThrows.ToArray();
            }
        }

        public ModsRevenge()
        {
            trackedThrows = new ThreadSafeList<ThrowTracker>();
            Active = false;
        }

        public void StartEvent()
        {
            revengeTimeout = new Timer(new TimerCallback(revengeTimedOut), null, REVENGE_TIMEOUT * 60 * 1000, REVENGE_TIMEOUT * 60 * 1000);

            int TOTAL_SPLATTERBALLS = 50; // Thanks MayDay!

            // Give Splatterballs
            int[] allUsers = Database.GetModeratorUsers();
            foreach (int userid in allUsers)
            {
                for (int i = 0; i < TOTAL_SPLATTERBALLS; i++)
                {
                    ItemInstance itm = new ItemInstance(Item.ModSplatterball);

                    if (User.IsUserOnline(userid))
                        User.GetUserById(userid).Inventory.AddWithoutDatabase(itm);

                    Database.AddItemToInventory(userid, itm);
                }
            }

            byte[] annoucePacket = PacketBuilder.CreateChat(Messages.EventStartModsRevenge, PacketBuilder.CHAT_BOTTOM_RIGHT);

            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(annoucePacket);
        }

        public void EndEvent()
        {
            GameServer.RemoveAllItemsOfIdInTheGame(Item.ModSplatterball);

            byte[] annoucePacket = PacketBuilder.CreateChat(Messages.EventEndModsRevenge, PacketBuilder.CHAT_BOTTOM_RIGHT);

            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(annoucePacket);
        }
        private void revengeTimedOut(object state)
        {
            resetEvent();
            EndEvent();
        }

        private void resetEvent()
        {
            revengeTimeout.Dispose();
            trackedThrows.Clear();
            Active = false;
        }
        private ThrowTracker getUserThrowTracker(User thrower)
        {
            foreach (ThrowTracker throwTracker in TrackedThrows)
            {
                if (throwTracker.Thrower.Id == thrower.Id)
                    return throwTracker;
            }

            ThrowTracker tracker = new ThrowTracker(thrower);
            trackedThrows.Add(tracker);
            return tracker;
        }

        private bool checkUserThrownAtAlready(ThrowTracker tracker, User thrownAt)
        {
            foreach (User user in tracker.ThrownAt)
            {
                if (user.Id == thrownAt.Id)
                    return true;
            }

            return false;

        }


        public void LeaveEvent(User userToLeave)
        {
            foreach (ThrowTracker thrownMemory in TrackedThrows)
            {
                if (thrownMemory.Thrower.Id == userToLeave.Id)
                    trackedThrows.Remove(thrownMemory);
            }
        }


        public void Payout(User thrower, User throwAt)
        {
            ThrowTracker throwCounter = getUserThrowTracker(thrower);
            if(!checkUserThrownAtAlready(throwCounter, throwAt))
            {

                byte[] otherEarned = PacketBuilder.CreateChat(Messages.FormatModSplatterBallAwardedOther(thrower.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] youEarned = PacketBuilder.CreateChat(Messages.FormatModSplatterBallAwardedYou(throwAt.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

                thrower.AddMoney(50);
                throwAt.AddMoney(500);

                thrower.Client.SendPacket(youEarned);
                throwAt.Client.SendPacket(otherEarned);

                throwCounter.AddThrownAt(throwAt);
            }
        }


    }
}
