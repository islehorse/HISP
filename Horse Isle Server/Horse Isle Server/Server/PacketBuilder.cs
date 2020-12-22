using System;
using System.IO;
using System.Text;
using HISP.Game;
namespace HISP.Server
{
    class PacketBuilder
    {

        public const byte PACKET_TERMINATOR = 0x00;
        public const byte PACKET_CLIENT_TERMINATOR = 0x0A;
        

        public const byte PACKET_LOGIN = 0x7F;
        public const byte PACKET_CHAT = 0x14;
        public const byte PACKET_MOVE = 0x15;
        public const byte PACKET_CLICK = 0x77;
        public const byte PACKET_USERINFO = 0x81;
        public const byte PACKET_WORLD = 0x7A;
        public const byte PACKET_BASE_STATS = 0x7B;
        public const byte PACKET_SWF_CUTSCENE = 0x29;
        public const byte PACKET_SWF_MODULE_GENTLE = 0x2A;
        public const byte PACKET_PLACE_INFO = 0x1E;
        public const byte PACKET_AREA_DEFS = 0x79;
        public const byte PACKET_ITEM_INTERACTION = 0x1E;
        public const byte PACKET_ANNOUNCEMENT = 0x7E;
        public const byte PACKET_TILE_FLAGS = 0x75;
        public const byte PACKET_PLAYSOUND = 0x23;
        public const byte PACKET_KEEP_ALIVE = 0x7C;
        public const byte PACKET_PLAYER = 0x18;
        public const byte PACKET_INVENTORY = 0x17;
        public const byte PACKET_TRANSPORT = 0x29;
        public const byte PACKET_KICK = 0x80;
        public const byte PACKET_LEAVE = 0x7D;
        public const byte PACKET_NPC = 0x28;
        public const byte PACKET_QUIT = 0x7D;
        public const byte PACKET_PLAYERINFO = 0x16;
        public const byte PACKET_INFORMATION = 0x28;

        public const byte SECCODE_QUEST = 0x32;
        public const byte SECCODE_ITEM = 0x28;

        public const byte NPC_START_CHAT = 0x14;
        public const byte NPC_CONTINUE_CHAT = 0x15;

        public const byte PLAYERINFO_LEAVE = 0x16;
        public const byte PLAYERINFO_UPDATE_OR_CREATE = 0x15;
        
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
        public const byte MOVE_ESCAPE = 0x18;
        public const byte MOVE_UPDATE = 0x0A;

        public const byte CHAT_BOTTOM_LEFT = 0x14;
        public const byte CHAT_BOTTOM_RIGHT = 0x15;
        public const byte CHAT_DM_RIGHT = 0x16;

        public const byte ITEM_INFORMATON = 0x14;
        public const byte ITEM_INFORMATON_ID = 0x15;
        public const byte NPC_INFORMATION = 0x16;

        public const byte ITEM_DROP = 0x1E; 
        public const byte ITEM_PICKUP = 0x14;
        public const byte ITEM_PICKUP_ALL = 0x15;
        public const byte ITEM_BUY = 0x33;
        public const byte ITEM_BUY_5 = 0x35;
        public const byte ITEM_BUY_25 = 0x37;
        public const byte ITEM_SELL = 0x3C;
        public const byte ITEM_SELL_ALL = 0x3D;
        public const byte ITEM_BINOCULARS = 0x5C;
        public const byte ITEM_MAGNIFYING = 0x5D;
        public const byte ITEM_RAKE = 0x5B;
        public const byte ITEM_SHOVEL = 0x5A;

        public const byte LOGIN_INVALID_USER_PASS = 0x15;
        public const byte LOGIN_SUCCESS = 0x14;

        public const byte DIRECTION_UP = 0;
        public const byte DIRECTION_RIGHT = 1;
        public const byte DIRECTION_DOWN = 2;
        public const byte DIRECTION_LEFT = 3;
        public const byte DIRECTION_TELEPORT = 4;
        public const byte DIRECTION_NONE = 10;


        public static byte[] CreatePlaysoundPacket(string sound)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_PLAYSOUND);

            byte[] strBytes = Encoding.UTF8.GetBytes(sound);
            ms.Write(strBytes, 0x00, strBytes.Length);

            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
        public static byte[] CreatePlayerLeavePacket(string username)
        {
            MemoryStream ms = new MemoryStream();

            ms.WriteByte(PACKET_PLAYERINFO);
            ms.WriteByte(PLAYERINFO_LEAVE);

            byte[] strBytes = Encoding.UTF8.GetBytes(username);
            ms.Write(strBytes, 0x00, strBytes.Length);

            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }
        public static byte[] CreatePlayerInfoUpdateOrCreate(int x, int y, int facing, int charId, string username)
        {
            MemoryStream ms = new MemoryStream();

            ms.WriteByte(PACKET_PLAYERINFO);
            ms.WriteByte(PLAYERINFO_UPDATE_OR_CREATE);

            ms.WriteByte((byte)(((x - 4) / 64) + 20)); 
            ms.WriteByte((byte)(((x - 4) % 64) + 20));

            ms.WriteByte((byte)(((y - 1) / 64) + 20)); 
            ms.WriteByte((byte)(((y - 1) % 64) + 20));

            ms.WriteByte((byte)(facing + 20));

            ms.WriteByte((byte)((charId / 64) + 20)); //6
            ms.WriteByte((byte)((charId % 64) + 20)); //7


            byte[] strBytes = Encoding.UTF8.GetBytes(username);
            ms.Write(strBytes, 0x00, strBytes.Length);

            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

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

            ms.WriteByte(PACKET_PLAYER);

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
            if (direction == DIRECTION_TELEPORT)
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

        public static byte[] CreateClickTileInfoPacket(string text)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_CLICK);
            ms.Write(strBytes, 0x00, strBytes.Length);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateMetaPacket(string formattedText)
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

        public static byte[] CreateKeepAlive()
        {
            MemoryStream ms = new MemoryStream();

            ms.WriteByte(PACKET_KEEP_ALIVE);
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

        public static byte[] CreatePlayerData(int money, int playerCount, int mail)
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
        public static byte[] CreateSwfModulePacket(string swf,byte type)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(type);
            byte[] strBytes = Encoding.UTF8.GetBytes(swf);
            ms.Write(strBytes, 0x00, strBytes.Length);
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

        public static byte[] CreateKickMessage(string reason)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_KICK);
            byte[] strBytes = Encoding.UTF8.GetBytes(reason);
            ms.Write(strBytes, 0x00, strBytes.Length);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            return Packet;
        }

        public static byte[] CreateMotd()
        {
            string formattedMotd = Messages.FormatMOTD();
            return CreateAnnouncement(formattedMotd);
        }
        public static byte[] CreateWelcomeMessage(string username)
        {
            string formattedStr = Messages.FormatWelcomeMessage(username);
            return CreateChat(formattedStr, CHAT_BOTTOM_RIGHT);
        }

    }
}
