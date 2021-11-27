using System.Collections.Generic;
using HISP.Game;
using HISP.Server;

namespace HISP.Player
{
    public class Friends
    {
        private User baseUser;
        private List<int> list;
        public int[] List
        {
            get
            {
                return list.ToArray();
            }
        }

        public int Count
        {
            get
            {
                return List.Length;
            }
        }

        public void RemoveFromLocalList(int value)
        {
            list.Remove(value);
        }

        public void AddToLocalList(int value)
        {
            list.Add(value);
        }
        public Friends(User user)
        {
            baseUser = user;
            list = new List<int>();

            int[] friends = Database.GetBuddyList(user.Id);
            foreach(int friendId in friends)
            {
                list.Add(friendId);
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
                removeFrom.Friends.RemoveFromLocalList(baseUser.Id);

            }
            catch (KeyNotFoundException) { /* User is offline, remove from database is sufficent */ };
 

            baseUser.Friends.RemoveFromLocalList(userid);
        }
        public void AddFriend(User userToFriend)
        {
            if(baseUser.MuteBuddy)
            {
                byte[] cantFriend = PacketBuilder.CreateChat(Messages.CantSendBuddyRequestWhileMuted, PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.LoggedinClient.SendPacket(cantFriend);
                return;
            }
            else if(userToFriend.MuteBuddyRequests)
            {
                byte[] cantFriend = PacketBuilder.CreateChat(Messages.PlayerIgnoringAllBuddyRequests, PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.LoggedinClient.SendPacket(cantFriend);
                return;
            }
            else if(userToFriend.MutePlayer.IsUserMuted(userToFriend))
            {
                byte[] cantFriend = PacketBuilder.CreateChat(Messages.PlayerIgnoringYourBuddyRequests, PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.LoggedinClient.SendPacket(cantFriend);
                return;
            }

            if (userToFriend.PendingBuddyRequestTo == baseUser)
            {
                Database.AddBuddy(baseUser.Id, userToFriend.Id);
                list.Add(userToFriend.Id);
                userToFriend.Friends.AddToLocalList(baseUser.Id);

                byte[] nowFriendsMsg = PacketBuilder.CreateChat(Messages.FormatAddBuddyConfirmed(userToFriend.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] nowFriendsOther = PacketBuilder.CreateChat(Messages.FormatAddBuddyConfirmed(baseUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

                userToFriend.LoggedinClient.SendPacket(nowFriendsOther);
                baseUser.LoggedinClient.SendPacket(nowFriendsMsg);

                if(!baseUser.MajorPriority)
                    GameServer.UpdateArea(baseUser.LoggedinClient);

                if (!userToFriend.MajorPriority)
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
