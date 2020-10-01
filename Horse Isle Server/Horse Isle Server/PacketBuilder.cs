using System;
using System.IO;
using System.Text;

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
        public const byte PACKET_BASE_STATS = 0x7B;
        public const byte PACKET_PLACE_INFO = 0x1E;
        public const byte PACKET_AREA_DEFS = 0x79;
        public const byte PACKET_ANNOUNCEMENT = 0x7E;
        public const byte PACKET_TILE_FLAGS = 0x75;
        public const byte PACKET_UPDATE = 0x7C;
        public const byte PACKET_PROFILE = 0x18;

        public const byte VIEW_PROFILE = 0x14;
        public const byte SAVE_PROFILE = 0x15;

        public const byte AREA_SEPERATOR = 0x5E;
        public const byte AREA_TOWN = 0x54;
        public const byte AREA_AREA = 0x41;
        public const byte AREA_ISLE = 0x49;

        public const byte MOVE_UP = 0x14;
        public const byte MOVE_DOWN = 0x15;
        public const byte MOVE_RIGHT = 0x16;
        public const byte MOVE_LEFT = 0x17;
        public const byte MOVE_EXIT = 0x10;

        public const byte CHAT_BOTTOM_LEFT = 0x14;
        public const byte CHAT_BOTTOM_RIGHT = 0x15;


        public const byte LOGIN_INVALID_USER_PASS = 0x15;
        public const byte LOGIN_SUCCESS = 0x14;

        public const byte DIRECTION_UP = 0;
        public const byte DIRECTION_RIGHT = 1;
        public const byte DIRECTION_DOWN = 2;
        public const byte DIRECTION_LEFT = 3;
        public const byte DIRECTION_LOGIN = 4;
        public const byte DIRECTION_NONE = 10;



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

        public static byte[] CreateProfilePacket(string userProfile)
        {
            MemoryStream ms = new MemoryStream();

            ms.WriteByte(PACKET_PROFILE);

            byte[] strBytes = Encoding.UTF8.GetBytes(userProfile);
            ms.Write(strBytes, 0x00, strBytes.Length);

            ms.WriteByte(PACKET_TERMINATOR);
            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateMovementPacket(int x, int y,int charId,int facing, int direction, bool walk)
        {
            // Header information
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_MOVE);

            ms.WriteByte((byte)(((x-4) / 64) + 20)); //1
            ms.WriteByte((byte)(((x-4) % 64) + 20)); //2

            ms.WriteByte((byte)(((y-1) / 64) + 20)); //3
            ms.WriteByte((byte)(((y-1) % 64) + 20)); //4

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

            int ystart = y - 4;
            int xstart = x - 6;

            if (direction == DIRECTION_UP)
            {
                for (int relx = 0; relx <= 12; relx++)
                {
                    int tileId = Map.GetTileId(xstart + relx, ystart, false);
                    int otileId = Map.GetTileId(xstart + relx, ystart, true);

                    if (tileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        tileId -= 100;
                    }
                    ms.WriteByte((byte)tileId);

                    if (otileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        otileId -= 100;
                    }
                    ms.WriteByte((byte)otileId);
                }
            }

            if (direction == DIRECTION_LEFT)
            {
                for (int rely = 0; rely <= 9; rely++)
                {
                    int tileId = Map.GetTileId(xstart, ystart + rely, false);
                    int otileId = Map.GetTileId(xstart, ystart + rely, true);



                    if (tileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        tileId -= 100;
                    }
                    ms.WriteByte((byte)tileId);

                    if (otileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        otileId -= 100;
                    }
                    ms.WriteByte((byte)otileId);
                }
            }


            if (direction == DIRECTION_RIGHT)
            {
                for (int rely = 0; rely <= 9; rely++)
                {
                    int tileId = Map.GetTileId(xstart + 12, ystart + rely, false);
                    int otileId = Map.GetTileId(xstart + 12, ystart + rely, true);


                    if (tileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        tileId -= 100;
                    }
                    ms.WriteByte((byte)tileId);

                    if (otileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        otileId -= 100;
                    }
                    ms.WriteByte((byte)otileId);
                }
            }

            if (direction == DIRECTION_DOWN)
            {
                for (int relx = 0; relx <= 12; relx++)
                {
                    int tileId = Map.GetTileId(xstart + relx, ystart + 9, false);
                    int otileId = Map.GetTileId(xstart + relx, ystart + 9, true);


                    if (tileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        tileId -= 100;
                    }
                    ms.WriteByte((byte)tileId);

                    if (otileId >= 190)
                    {
                        ms.WriteByte((byte)190);
                        otileId -= 100;
                    }
                    ms.WriteByte((byte)otileId);
                }
            }
            if (direction == DIRECTION_LOGIN)
            {
                for(int rely = 0; rely <= 9; rely++)
                {
                    for (int relx = 0; relx <= 12; relx++)
                    {
                        int tileId = Map.GetTileId(xstart + relx, ystart + rely, false);
                        int otileId = Map.GetTileId(xstart + relx, ystart + rely, true);

                        if(tileId >= 190)
                        {
                            ms.WriteByte((byte)190);
                            tileId -= 100;
                        }
                        ms.WriteByte((byte)tileId);

                        if (otileId >= 190)
                        {
                            ms.WriteByte((byte)190);
                            otileId -= 100;
                        }
                        ms.WriteByte((byte)otileId);

                    }
                }

            }


            ms.WriteByte(PACKET_TERMINATOR);
            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            //Logger.DebugPrint(BitConverter.ToString(Packet).Replace("-", " "));
            return Packet;
        }

        public static byte[] CreatePlaceInfo(string formattedText)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(formattedText);

            MemoryStream ms = new MemoryStream();

            ms.WriteByte(PACKET_PLACE_INFO);

            ms.Write(strBytes, 0x00, strBytes.Length);

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

        public static byte[] CreateUpdate()
        {
            MemoryStream ms = new MemoryStream();

            ms.WriteByte(PACKET_UPDATE);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
        public static byte[] CreatePlaceData(World.Isle[] isles, World.Town[] towns, World.Area[] areas)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_AREA_DEFS);

            // Write Towns

            foreach (World.Town town in towns)
            {
                byte[] strBytes = Encoding.UTF8.GetBytes(town.Name);

                ms.WriteByte(AREA_SEPERATOR);
                ms.WriteByte(AREA_TOWN);

                ms.WriteByte((byte)(((town.StartX - 4) / 64) + 20));
                ms.WriteByte((byte)(((town.StartX - 4) % 64) + 20));

                ms.WriteByte((byte)(((town.EndX - 4) / 64) + 20));
                ms.WriteByte((byte)(((town.EndX - 4) % 64) + 20));

                ms.WriteByte((byte)(((town.StartY - 1) / 64) + 20));
                ms.WriteByte((byte)(((town.StartY - 1) % 64) + 20));

                ms.WriteByte((byte)(((town.EndY - 1) / 64) + 20));
                ms.WriteByte((byte)(((town.EndY - 1) % 64) + 20));


                ms.Write(strBytes, 0x00, strBytes.Length);
            }

            // Write Areas

            foreach (World.Area area in areas)
            {
                byte[] strBytes = Encoding.UTF8.GetBytes(area.Name);

                ms.WriteByte(AREA_SEPERATOR);
                ms.WriteByte(AREA_AREA);

                ms.WriteByte((byte)(((area.StartX - 4) / 64) + 20));
                ms.WriteByte((byte)(((area.StartX - 4) % 64) + 20));

                ms.WriteByte((byte)(((area.EndX - 4) / 64) + 20));
                ms.WriteByte((byte)(((area.EndX - 4) % 64) + 20));

                ms.WriteByte((byte)(((area.StartY - 1) / 64) + 20));
                ms.WriteByte((byte)(((area.StartY - 1) % 64) + 20));

                ms.WriteByte((byte)(((area.EndY - 1) / 64) + 20));
                ms.WriteByte((byte)(((area.EndY - 1) % 64) + 20));


                ms.Write(strBytes, 0x00, strBytes.Length);
            }

            // Write Isles

            foreach (World.Isle isle in isles)
            {
                byte[] strBytes = Encoding.UTF8.GetBytes(isle.Name);

                ms.WriteByte(AREA_SEPERATOR);
                ms.WriteByte(AREA_ISLE);

                ms.WriteByte((byte)(((isle.StartX - 4) / 64) + 20));
                ms.WriteByte((byte)(((isle.StartX - 4) % 64) + 20));

                ms.WriteByte((byte)(((isle.EndX - 4) / 64) + 20));
                ms.WriteByte((byte)(((isle.EndX - 4) % 64) + 20));

                ms.WriteByte((byte)(((isle.StartY - 1) / 64) + 20));
                ms.WriteByte((byte)(((isle.StartY - 1) % 64) + 20));

                ms.WriteByte((byte)(((isle.EndY - 1) / 64) + 20));
                ms.WriteByte((byte)(((isle.EndY - 1) % 64) + 20));

                ms.WriteByte((byte)isle.Tileset.ToString()[0]);

                ms.Write(strBytes, 0x00, strBytes.Length);
            }
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateBaseStats(int money, int playerCount, int mail)
        {
            byte[] moneyStrBytes = Encoding.UTF8.GetBytes(money.ToString());
            byte[] playerStrBytes = Encoding.UTF8.GetBytes(playerCount.ToString());
            byte[] mailStrBytes = Encoding.UTF8.GetBytes(mail.ToString());

            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_BASE_STATS);
            ms.Write(moneyStrBytes, 0x00, moneyStrBytes.Length);
            ms.WriteByte((byte)'|');
            ms.Write(playerStrBytes, 0x00, playerStrBytes.Length);
            ms.WriteByte((byte)'|');
            ms.Write(mailStrBytes, 0x00, mailStrBytes.Length);
            ms.WriteByte((byte)'|');
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateTileOverlayFlags(int[] tileDepthFlags)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_TILE_FLAGS);

            foreach(int tileDepthFlag in tileDepthFlags)
            {
                ms.WriteByte((byte)tileDepthFlag.ToString()[0]);
            }

            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
        public static byte[] CreateSecCode(byte[] SecCodeSeed, int SecCodeInc, bool Admin, bool Moderator)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_USERINFO);

            ms.WriteByte((byte)(SecCodeSeed[0] + 33));
            ms.WriteByte((byte)(SecCodeSeed[1] + 33));
            ms.WriteByte((byte)(SecCodeSeed[2] + 33));
            ms.WriteByte((byte)(SecCodeInc + 33));

            char userType = 'N'; // Normal?
            if (Moderator)
                userType = 'M';
            if (Admin)
                userType = 'A';

            ms.WriteByte((byte)userType);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
        public static byte[] CreateAnnouncement(string announcement)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_ANNOUNCEMENT);
            byte[] strBytes = Encoding.UTF8.GetBytes(announcement);
            ms.Write(strBytes, 0x00, strBytes.Length);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateAreaMessage(int x, int y)
        {
            string locationStr = Messages.FormatLocationData(x, y);
            return CreatePlaceInfo(locationStr);
        }
        public static byte[] CreateMotd()
        {
            string formattedMotd = Messages.FormatMOTD();
            return CreateAnnouncement(formattedMotd);
        }
        public static byte[] CreateLoginMessage(string username)
        {
            string formattedStr = Messages.FormatLoginMessage(username);
            return CreateChat(formattedStr, CHAT_BOTTOM_RIGHT);
        }

    }
}
