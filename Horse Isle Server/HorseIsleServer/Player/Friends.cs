using System.Collections.Generic;
using HISP.Game;
using HISP.Server;

namespace HISP.Player
{
    public class Friends
    {
        private User baseUser;
        public List<int> List;

        public int Count
        {
            get
            {
                return List.Count;
            }
        }

        public Friends(User user)
        {
            baseUser = user;
            List = new List<int>();

            int[] friends = Database.GetBuddyList(user.Id);
            foreach(int friendId in friends)
            {
                List.Add(friendId);
            }

        }

        public bool IsFriend(int friendUserId)
        {
            foreach (int userId in List)
                if (userId == friendUserId)
                    return true;
            return false;
        }

        public void RemoveFriend(int userid)
        {
            Database.RemoveBuddy(baseUser.Id, userid);

            // Remove buddy from there list if they are logged in
            try
            {

                User removeFrom = GameServer.GetUserById(userid);
                removeFrom.Friends.List.Remove(baseUser.Id);

            }
            catch (KeyNotFoundException) { /* User is offline, remove from database is sufficent */ };
 

            baseUser.Friends.List.Remove(userid);
        }
        public void AddFriend(User userToFriend)
        {
            if (userToFriend.PendingBuddyRequestTo == baseUser)
            {
                Database.AddBuddy(baseUser.Id, userToFriend.Id);
                List.Add(userToFriend.Id);
                userToFriend.Friends.List.Add(baseUser.Id);

                byte[] nowFriendsMsg = PacketBuilder.CreateChat(Messages.FormatAddBuddyConfirmed(userToFriend.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] nowFriendsOther = PacketBuilder.CreateChat(Messages.FormatAddBuddyConfirmed(baseUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

                userToFriend.LoggedinClient.SendPacket(nowFriendsOther);
                baseUser.LoggedinClient.SendPacket(nowFriendsMsg);

                GameServer.UpdateArea(baseUser.LoggedinClient);
                GameServer.UpdateArea(userToFriend.LoggedinClient);
            }
            else
            {
                baseUser.PendingBuddyRequestTo = userToFriend;
                byte[] pendingMsg = PacketBuilder.CreateChat(Messages.AddBuddyPending, PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] pendingMsgOther = PacketBuilder.CreateChat(Messages.FormatAddBuddyPendingOther(baseUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.LoggedinClient.SendPacket(pendingMsg);
                if(!userToFriend.MuteBuddyRequests && !userToFriend.MuteAll)
                    userToFriend.LoggedinClient.SendPacket(pendingMsgOther);
                
            }
        }

    }
}
