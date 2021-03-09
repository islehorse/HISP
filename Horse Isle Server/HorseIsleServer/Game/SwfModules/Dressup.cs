using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.SwfModules
{
    public class Dressup
    {

        public static List<DressupRoom> DressupRooms = new List<DressupRoom>();
        public class DressupRoom
        {
            public DressupRoom(int roomId)
            {
                RoomId = roomId;
                DressupPeices = new List<DressupPeice>();

                DressupPeice[] peices = Database.LoadDressupRoom(this);
                foreach (DressupPeice peice in peices)
                    DressupPeices.Add(peice);

                DressupRooms.Add(this);
            }
            public int RoomId;
            public List<DressupPeice> DressupPeices;

            public DressupPeice GetDressupPeice(int peiceId)
            {
                foreach(DressupPeice peice in DressupPeices)
                {
                    if (peice.PeiceId == peiceId)
                        return peice;
                }
                // Else create peice
                DressupPeice dPeice = new DressupPeice(this, peiceId, 0, 0, false, true);
                DressupPeices.Add(dPeice);
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
