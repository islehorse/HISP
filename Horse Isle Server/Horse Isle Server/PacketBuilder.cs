using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class PacketBuilder
    {

        public const byte PACKET_TERMINATOR = 0x00;
        public const byte PACKET_A_TERMINATOR = 0x0A;


        public const byte PACKET_LOGIN = 0x7F;
        public const byte PACKET_CHAT = 0x14;
        public const byte PACKET_MOVE = 0x15;
        public const byte PACKET_USERINFO = 0x81;
        public const byte PACKET_WORLD = 0x7A;

        private const byte CHAT_BOTTOM_LEFT = 0x14;
        private const byte CHAT_BOTTOM_RIGHT = 0x15;

        public const byte LOGIN_INVALID_USER_PASS = 0x15;
        public const byte LOGIN_SUCCESS = 0x14;

        public static byte[] CreateLoginPacket(bool Success)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_LOGIN);
            if (Success)
                ms.WriteByte(LOGIN_SUCCESS);
            else
                ms.WriteByte(LOGIN_INVALID_USER_PASS);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateMovementPacket(int x, int y,int charId, int facing, int direction, bool walk)
        {
            // Header information
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_MOVE);

            ms.WriteByte((byte)((x / 64) + 20)); //1
            ms.WriteByte((byte)((x % 64) + 20)); //2

            ms.WriteByte((byte)((y / 64) + 20)); //3
            ms.WriteByte((byte)((y % 64) + 20)); //4

            ms.WriteByte((byte)(facing + 20)); //5

            ms.WriteByte((byte)((charId / 64) + 20)); //6
            ms.WriteByte((byte)((charId % 64) + 20)); //7

            ms.WriteByte((byte)(direction + 20)); //8

            ms.WriteByte((byte)(Convert.ToInt32(walk) + 20)); //9

            // Map Data

            if (direction >= 20)
            {
                direction -= 20;
            }

            if (direction == 4)
            {
                for(int rely = y - 3; rely < rely + 9; rely++)
                {
                    for (int relx = x - 2; relx < relx + 12; relx++)
                    {
                        int tileId = Map.GetTileId(relx, rely, false);
                        int otileId = Map.GetTileId(relx, rely, true);

                        if (tileId == 290)
                            tileId -= 100;
                        if(otileId == 290)
                            otileId -= 100;

                        ms.WriteByte((byte)tileId);
                        ms.WriteByte((byte)otileId);
                    }
                }

            }


            ms.WriteByte(PACKET_TERMINATOR);
            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
        public static byte[] CreateChat(string formattedText, byte chatWindow)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(formattedText);

            MemoryStream ms = new MemoryStream();

            ms.WriteByte(PACKET_CHAT);
            ms.WriteByte(chatWindow);

            ms.Write(strBytes, 0x00, strBytes.Length);

            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateLoginMessage(string username)
        {
            string formattedStr = Gamedata.NewUserMessage.Replace("%USERNAME%", username);
            
            return CreateChat(formattedStr,CHAT_BOTTOM_RIGHT);
        }

        public static byte[] CreateWorldData(int gameTime, int gameDay, int gameYear, string weather)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(weather);

            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_WORLD);
            
            ms.WriteByte((byte)((gameTime / 64) + 20)); 
            ms.WriteByte((byte)((gameTime % 64) + 20)); 

            ms.WriteByte((byte)((gameDay / 64) + 20)); 
            ms.WriteByte((byte)((gameDay % 64) + 20)); 

            ms.WriteByte((byte)((gameYear / 64) + 20)); 
            ms.WriteByte((byte)((gameYear % 64) + 20));

            ms.Write(strBytes,0x00, strBytes.Length);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
        public static byte[] CreateUserInfo(Client client)
        {
            MemoryStream ms = new MemoryStream();
            if(!client.LoggedIn)         
                throw new Exception("Client is not logged in.");
            User user = client.LoggedinUser;

            byte[] MovementPacket = CreateMovementPacket(user.X, user.Y, user.CharacterId, 2, 4, false);
            ms.Write(MovementPacket, 0x00, MovementPacket.Length);

            byte[] LoginMessage = CreateLoginMessage(user.Username);
            ms.Write(LoginMessage, 0x00, LoginMessage.Length);

            World.Time time = World.GetGameTime();
            int timestamp = time.hours * 60;
            timestamp += time.minutes;

            byte[] WorldData = CreateWorldData(timestamp, time.days, time.year, World.GetWeather());
            ms.Write(WorldData, 0x00, LoginMessage.Length);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
    }
}
