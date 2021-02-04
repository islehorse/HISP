using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.SwfModules
{
    class Drawingroom
    {
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
                Database.CreateDrawingRoom(roomId);
            drawing = Database.GetDrawingRoomDrawing(roomId);
            Id = roomId;

        }

    }
}
