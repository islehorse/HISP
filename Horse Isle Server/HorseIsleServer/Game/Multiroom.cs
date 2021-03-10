using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game
{
    public class Multiroom
    {
        public static List<Multiroom> Multirooms = new List<Multiroom>();
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
                    if (tile.Code.StartsWith("MULTIROOM"))
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

            Multirooms.Add(this);
        }

        public int x;
        public int y;

        public List<User> JoinedUsers = new List<User>();

        public void Join(User user)
        {
            if (!JoinedUsers.Contains(user))
            {
                Logger.DebugPrint(user.Username + " Joined multiroom @ " + x.ToString() + "," + y.ToString());
                JoinedUsers.Add(user);

                foreach (User joinedUser in JoinedUsers)
                    if (joinedUser.Id != user.Id)
                        GameServer.UpdateArea(joinedUser.LoggedinClient);
            }
            
        }

        public void Leave(User user)
        {

            if(JoinedUsers.Contains(user))
            {
                Logger.DebugPrint(user.Username + " Left multiroom @ " + x.ToString() + "," + y.ToString());
                JoinedUsers.Remove(user);
            }

            foreach (User joinedUser in JoinedUsers)
                GameServer.UpdateArea(joinedUser.LoggedinClient);

        }
    }
}
