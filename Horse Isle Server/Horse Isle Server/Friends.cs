using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Friends
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
        public void AddFriend(User userToFriend)
        {
            bool pendingRequest = Database.IsPendingBuddyRequestExist(baseUser.Id, userToFriend.Id);
            if (pendingRequest)
            {
                Database.AcceptBuddyRequest(baseUser.Id, userToFriend.Id);
                List.Add(userToFriend.Id);
            }
            else
            {
                Database.AddPendingBuddyRequest(baseUser.Id, userToFriend.Id);
            }
        }

    }
}
