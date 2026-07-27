using System;
using System.Collections.Generic;
using System.Linq;
using HISP.Game;
using HISP.Server;
using HISP.Util;

namespace HISP.Player
{
    public class Friends
    {
        private User baseUser;
        private ThreadSafeList<int> list;
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
            list = new ThreadSafeList<int>();

            int[] friends = Database.GetBuddyList(user.Id);
            foreach(int friendId in friends)
            {
                list.Add(friendId);
            }

        }

        public bool IsFriend(int friendUserId)
        {
            return List.Any(userId => userId == friendUserId);
        }

        public void RemoveFriend(int userid)
        {
            Database.RemoveBuddy(baseUser.Id, userid);

            // Remove buddy from there list if they are logged in
            if(User.IsUserOnline(userid))
            {
                User removeFrom = User.GetUserById(userid);
                removeFrom.Friends.RemoveFromLocalList(baseUser.Id);
            }
            

            baseUser.Friends.RemoveFromLocalList(userid);
        }
        public void AddFriend(User userToFriend)
        {
            if(baseUser.MuteBuddy)
            {
                byte[] cantFriend = PacketBuilder.CreateChat(Messages.CantSendBuddyRequestWhileMuted, PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.Client.SendPacket(cantFriend);
                return;
            }
            else if(userToFriend.MuteBuddyRequests)
            {
                byte[] cantFriend = PacketBuilder.CreateChat(Messages.PlayerIgnoringAllBuddyRequests, PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.Client.SendPacket(cantFriend);
                return;
            }
            else if(userToFriend.MutePlayer.IsUserMuted(userToFriend))
            {
                byte[] cantFriend = PacketBuilder.CreateChat(Messages.PlayerIgnoringYourBuddyRequests, PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.Client.SendPacket(cantFriend);
                return;
            }

            if (userToFriend.PendingBuddyRequestTo == baseUser)
            {
                Database.AddBuddy(baseUser.Id, userToFriend.Id);
                list.Add(userToFriend.Id);
                userToFriend.Friends.AddToLocalList(baseUser.Id);

                byte[] nowFriendsMsg = PacketBuilder.CreateChat(Messages.FormatAddBuddyConfirmed(userToFriend.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] nowFriendsOther = PacketBuilder.CreateChat(Messages.FormatAddBuddyConfirmed(baseUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

                userToFriend.Client.SendPacket(nowFriendsOther);
                baseUser.Client.SendPacket(nowFriendsMsg);

                if(!baseUser.MajorPriority)
                    GameServer.UpdateArea(baseUser.Client);

                if (!userToFriend.MajorPriority)
                    GameServer.UpdateArea(userToFriend.Client);
            }
            else
            {
                baseUser.PendingBuddyRequestTo = userToFriend;
                byte[] pendingMsg = PacketBuilder.CreateChat(Messages.AddBuddyPending, PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] pendingMsgOther = PacketBuilder.CreateChat(Messages.FormatAddBuddyPendingOther(baseUser.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                baseUser.Client.SendPacket(pendingMsg);
                if(!userToFriend.MuteBuddyRequests && !userToFriend.MuteAll)
                    userToFriend.Client.SendPacket(pendingMsgOther);
                
            }
        }

    }
}
