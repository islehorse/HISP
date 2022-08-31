using HISP.Server;
using System.Collections.Generic;

namespace HISP.Player
{
    public class MutedPlayers
    {
        private User baseUser;
        private ThreadSafeList<int> userIds;
        public MutedPlayers(User BaseUser)
        {
            userIds = new ThreadSafeList<int>();
            baseUser = BaseUser;
            int[] userids = Database.GetMutedPlayers(BaseUser.Id);
            
            foreach (int userid in userids)
                userIds.Add(userid);

        }

        public bool IsUserMuted(User user)
        {
            return userIds.Contains(user.Id);
        }

        public void MuteUser(User user)
        {
            userIds.Add(user.Id);
            Database.AddMutedPlayer(baseUser.Id, user.Id);
        }

        public void UnmuteUser(User user)
        {
            userIds.Remove(user.Id);
            Database.DeleteMutedPlayer(baseUser.Id, user.Id);
        }


    }
}
