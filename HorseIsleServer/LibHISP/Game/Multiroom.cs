using HISP.Player;
using HISP.Server;
using System.Collections.Generic;
using System.Linq;

namespace HISP.Game
{
    public class Multiroom
    {
        private static List<Multiroom> multirooms = new List<Multiroom>();
        private List<User> joinedUsers = new List<User>();

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
            foreach (Multiroom multiroom in Multirooms)
                if(multiroom.x == x && multiroom.y == y)
                   return multiroom;

            throw new KeyNotFoundException();
        }
        public static bool IsMultiRoomAt(int x, int y)
        {
            foreach (Multiroom multiroom in Multirooms)
                if (multiroom.x == x && multiroom.y == y)
                    return true;

            return false;
        }

        public static void LeaveAllMultirooms(User user)
        {
            foreach (Multiroom room in Multirooms)
                room.Leave(user);
        }

        public static void CreateMultirooms()
        {
            Logger.InfoPrint("Creating Multirooms...");
            foreach(World.SpecialTile tile in World.SpecialTiles)
            {
                if (tile.Code != null)
                {
                    if (tile.Code.StartsWith("MULTIROOM") || tile.Code.StartsWith("MULTIHORSES") || tile.Code.StartsWith("2PLAYER") || tile.Code.StartsWith("AUCTION"))
                    {
                        Logger.DebugPrint("Created Multiroom @ " + tile.X.ToString() + "," + tile.Y.ToString());
                        new Multiroom(tile.X, tile.Y);
                    }
                }   
            }
        }
        public Multiroom(int x, int y)
        {
            this.x = x;
            this.y = y;

            multirooms.Add(this);
        }

        public void Join(User user)
        {
            if (!JoinedUsers.Contains(user))
            {
                Logger.DebugPrint(user.Username + " Joined multiroom @ " + x.ToString() + "," + y.ToString());
                joinedUsers.Add(user);

                foreach (User joinedUser in JoinedUsers)
                    if (joinedUser.Id != user.Id)
                        if(!TwoPlayer.IsPlayerInGame(joinedUser))
                            if(!joinedUser.MajorPriority)
                                GameServer.UpdateArea(joinedUser.LoggedinClient);
            }
            
        }

        public void Leave(User user)
        {

            if(JoinedUsers.Contains(user))
            {
                Logger.DebugPrint(user.Username + " Left multiroom @ " + x.ToString() + "," + y.ToString());
                joinedUsers.Remove(user);


                foreach (User joinedUser in JoinedUsers)
                    if (!TwoPlayer.IsPlayerInGame(joinedUser))
                        if (!joinedUser.MajorPriority)
                            GameServer.UpdateArea(joinedUser.LoggedinClient);
            }

        }
    }
}
