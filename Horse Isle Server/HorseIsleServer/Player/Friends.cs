using System.Collections.Generic;
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
            catch (KeyNotFoundException) { /* User is ofline, remove from database is sufficent */ };
 

            baseUser.Friends.List.Remove(userid);
        }
        public void AddFriend(User userToFriend)
        {
            bool pendingRequest = Database.IsPendingBuddyRequestExist(baseUser.Id, userToFriend.Id);
            if (pendingRequest)
            {
                Database.AcceptBuddyRequest(baseUser.Id, userToFriend.Id);

                List.Add(userToFriend.Id);
                userToFriend.Friends.List.Add(baseUser.Id);
            }
            else
            {
                Database.AddPendingBuddyRequest(baseUser.Id, userToFriend.Id);
            }
        }

    }
}
