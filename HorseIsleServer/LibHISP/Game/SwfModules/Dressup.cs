using HISP.Server;
using HISP.Util;
using System.Collections.Generic;

namespace HISP.Game.SwfModules
{
    public class Dressup
    {

        private static List<DressupRoom> dressupRooms = new List<DressupRoom>();
        public static DressupRoom[] DressupRooms 
        {
            get
            {
                return dressupRooms.ToArray();
            }
        }
        public class DressupRoom
        {
            public int RoomId;
            private ThreadSafeList<DressupPeice> dressupPeices;
            public DressupPeice[] DressupPeices
            {
                get
                {
                    return dressupPeices.ToArray();
                }
            }
            public DressupRoom(int roomId)
            {
                RoomId = roomId;
                dressupPeices = new ThreadSafeList<DressupPeice>();

                DressupPeice[] peices = Database.LoadDressupRoom(this);
                foreach (DressupPeice peice in peices)
                    dressupPeices.Add(peice);

                dressupRooms.Add(this);
            }
            
            public DressupPeice GetDressupPeice(int peiceId)
            {
                foreach(DressupPeice peice in DressupPeices)
                {
                    if (peice.PeiceId == peiceId)
                        return peice;
                }
                // Else create peice
                DressupPeice dPeice = new DressupPeice(this, peiceId, 0, 0, false, true);
                dressupPeices.Add(dPeice);
                return dPeice;
            }
        }


        public class DressupPeice
        {
            public DressupPeice(DressupRoom room,int peiceId, int x, int y, bool active, bool createNew)
            {
                this.baseRoom = room;
                this.PeiceId = peiceId;
                this.x = x;
                this.y = y;
                this.active = active;


                if (createNew)
                    Database.CreateDressupRoomPeice(room.RoomId, peiceId, active, x, y);

            }
            public DressupRoom baseRoom;
            public int PeiceId;
            public int X
            {
                get
                {
                    return x;
                }
                set
                {
                    Database.SetDressupRoomPeiceX(baseRoom.RoomId, PeiceId, value);
                    x = value;
                }
            }
            public int Y
            {
                get
                {
                    return y;
                }
                set
                {
                    Database.SetDressupRoomPeiceY(baseRoom.RoomId, PeiceId, value);
                    y = value;
                }
            }
            public bool Active
            {
                get
                {
                    return active;
                }
                set
                {
                    Database.SetDressupRoomPeiceActive(baseRoom.RoomId, PeiceId, value);
                    active = value;
                }
            }

            private int x;
            private int y;
            private bool active;

        }

        public static DressupRoom GetDressupRoom(int roomId)
        {
            foreach(DressupRoom sRoom in DressupRooms)
            {
                if (sRoom.RoomId == roomId)
                    return sRoom;
            }
            
            // Else create room

            DressupRoom room = new DressupRoom(roomId);
            return room;
        }
    }
}
