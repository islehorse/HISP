using HISP.Server;
using System.Collections.Generic;


namespace HISP.Game.SwfModules
{
    public class Brickpoet
    {
        public struct PoetryEntry {
            public int Id;
            public string Word;
            public int Room;

        }

        public class PoetryPeice
        {
            public PoetryPeice(int PoetId, int RoomId, string PeiceWord)
            {
                if(!Database.GetPoetExist(PoetId, RoomId))
                {
                    Id = PoetId;
                    x = GameServer.RandomNumberGenerator.Next(0, 365);
                    y = GameServer.RandomNumberGenerator.Next(0, 280);
                    Word = PeiceWord;
                    roomId = RoomId;
                    Database.AddPoetWord(PoetId, x, y, RoomId);
                }
                else
                {
                    Id = PoetId;
                    roomId = RoomId;
                    Word = PeiceWord;
                    x = Database.GetPoetPositionX(Id, roomId);
                    y = Database.GetPoetPositionY(Id, roomId);
                }
            }
            public int Id;
            public int RoomId
            {
                get
                {
                    return roomId;
                }
            }
            public int X
            {
                get
                {
                    return x;
                }
                set
                {
                    Database.SetPoetPosition(Id, value, y, roomId);
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
                    Database.SetPoetPosition(Id, x, value, roomId);
                    y = value;
                }

            }

            public string Word;

            private int x;
            private int y;

            private int roomId;
        
        }

        public static List<PoetryEntry> PoetList = new List<PoetryEntry>();
        private static List<PoetryPeice[]> poetryRooms = new List<PoetryPeice[]>();
        public static PoetryPeice[][] PoetryRooms
        {
            get
            {
                return poetryRooms.ToArray();
            }
        }

        private static PoetryEntry[] getPoetsInRoom(int roomId)
        {
            List<PoetryEntry> entries = new List<PoetryEntry>();

            foreach(PoetryEntry poet in PoetList.ToArray())
            {
                if(poet.Room == roomId)
                {
                    entries.Add(poet);
                }
            }
            
            return entries.ToArray();
        }
        private static PoetryPeice[] getPoetryRoom(int roomId)
        {
            PoetryEntry[] poetryEntries = getPoetsInRoom(roomId);
            List<PoetryPeice> peices = new List<PoetryPeice>();
            foreach (PoetryEntry poetryEntry in poetryEntries)
            {
                PoetryPeice peice = new PoetryPeice(poetryEntry.Id, roomId, poetryEntry.Word);
                peices.Add(peice);
            }

            return peices.ToArray();
        }

        public static PoetryPeice[] GetPoetryRoom(int roomId)
        {
            foreach(PoetryPeice[] peices in PoetryRooms)
            {
                if (peices[0].RoomId == roomId)
                    return peices;
            }
            throw new KeyNotFoundException("No poetry room of id: " + roomId + " exists.");
        }
        public static PoetryPeice GetPoetryPeice(PoetryPeice[] room, int id)
        {
            foreach(PoetryPeice peice in room)
            {
                if (peice.Id == id)
                    return peice;
            }
            throw new KeyNotFoundException("Peice with id: " + id + " not found in room.");
        }
        public static void LoadPoetryRooms()
        {
            List<int> rooms = new List<int>();
            foreach(PoetryEntry entry in PoetList)
            {
                if (!rooms.Contains(entry.Room))
                    rooms.Add(entry.Room);
            }

            foreach(int room in rooms)
            {
                Logger.InfoPrint("Loading poetry room: " + room.ToString());
                poetryRooms.Add(getPoetryRoom(room));
                if (!Database.LastPlayerExist("P" + room))
                    Database.AddLastPlayer("P" + room, -1);
            }

        }

        




    }
}
