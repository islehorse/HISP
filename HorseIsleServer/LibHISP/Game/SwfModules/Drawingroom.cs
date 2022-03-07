using HISP.Server;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.SwfModules
{
    class Drawingroom
    {
        private static List<Drawingroom> drawingRooms = new List<Drawingroom>();
        public static Drawingroom[] DrawingRooms
        {
            get
            {
                return drawingRooms.ToArray();
            }
        }

        private string drawing;
        public string Drawing
        {
            get
            {
                return drawing;
            }
            set
            {
                if(value.Length < 65535)
                {
                    Database.SetDrawingRoomDrawing(Id, value);
                    drawing = value;
                }
                else
                {
                    throw new DrawingroomFullException();
                }
            }
        }
        public int Id;
        public Drawingroom(int roomId)
        {
            
            if (!Database.DrawingRoomExists(roomId))
            {
                Database.CreateDrawingRoom(roomId);
                Database.SetLastPlayer("D" + roomId.ToString(), -1);
            }
            
            drawing = Database.GetDrawingRoomDrawing(roomId);
            Id = roomId;

            drawingRooms.Add(this);
        }
        public static void LoadAllDrawingRooms()
        {
            // iterate over every special tile
            foreach(World.SpecialTile tile in World.SpecialTiles)
            {
                if(tile.Code != null)
                {
                    if (tile.Code.StartsWith("MULTIROOM-D"))
                    {
                        int roomId = int.Parse(tile.Code.Substring(11));
                        Logger.InfoPrint("Loading Drawing Room ID: " + roomId.ToString());
                        Drawingroom room = new Drawingroom(roomId);
                    }
                }

            }
        }
        public static Drawingroom GetDrawingRoomById(int id)
        {
            foreach(Drawingroom room in DrawingRooms)
            {
                if (room.Id == id)
                    return room;
            }
            throw new KeyNotFoundException("Room with id: " + id + " not found.");
        }

    }
}
