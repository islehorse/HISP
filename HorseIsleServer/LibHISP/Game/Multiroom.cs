using HISP.Player;
using HISP.Server;
using HISP.Util;
using System.Linq;

namespace HISP.Game
{
    public class Multiroom
    {
        private static ThreadSafeList<Multiroom> multirooms = new ThreadSafeList<Multiroom>();
        private ThreadSafeList<User> joinedUsers = new ThreadSafeList<User>();

        public int x;
        public int y;
        public User[] JoinedUsers
        {
            get
            {
                return joinedUsers.ToArray();
            }
        }
        public static Multiroom[] Multirooms
        { 
            get
            {
                return multirooms.ToArray();
            }
        }
        public static Multiroom GetMultiroom(int x, int y)
        {
            return Multirooms.First(o => (o.x == x && o.y == y));
        }
        public static bool IsMultiRoomAt(int x, int y)
        {
            return Multirooms.Any(o => (o.x == x && o.y == y));
        }

        public static void LeaveAllMultirooms(User user)
        {
            foreach (Multiroom room in Multirooms)
                room.Leave(user);
        }

        public static void CreateMultirooms()
        {
            Logger.InfoPrint("Creating Multirooms...");
            foreach(World.SpecialTile tile in World.SpecialTiles.Where(
                spTile => 
                spTile.Code != null &&
                (spTile.Code.StartsWith("MULTIROOM") || 
                spTile.Code.StartsWith("MULTIHORSES") ||
                spTile.Code.StartsWith("2PLAYER") || 
                spTile.Code.StartsWith("AUCTION")))
            ) {
                Logger.DebugPrint("Created Multiroom @ " + tile.X.ToString() + "," + tile.Y.ToString());
                new Multiroom(tile.X, tile.Y);
            }
        }
        public Multiroom(int x, int y)
        {
            this.x = x;
            this.y = y;

            multirooms.Add(this);
        }

        public void Join(User userToJoin)
        {
            if (!JoinedUsers.Contains(userToJoin))
            {
                Logger.DebugPrint(userToJoin.Username + " Joined multiroom @ " + x.ToString() + "," + y.ToString());
                joinedUsers.Add(userToJoin);

                foreach (User joinedUser in JoinedUsers.Where(userInGame => 
                    userInGame.Id != userToJoin.Id && 
                    !TwoPlayer.IsPlayerInGame(userInGame) &&
                    !userInGame.MajorPriority)
                ) GameServer.UpdateArea(joinedUser.Client);
            }
            
        }

        public void Leave(User userToLeave)
        {

            if(JoinedUsers.Contains(userToLeave))
            {
                Logger.DebugPrint(userToLeave.Username + " Left multiroom @ " + x.ToString() + "," + y.ToString());
                joinedUsers.Remove(userToLeave);

                foreach (User joinedUser in JoinedUsers.Where(
                    userInGame => !TwoPlayer.IsPlayerInGame(userInGame) && 
                    !userInGame.MajorPriority)
                ) GameServer.UpdateArea(joinedUser.Client);
            }

        }
    }
}
