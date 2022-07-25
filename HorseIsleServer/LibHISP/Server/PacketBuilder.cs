using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using HISP.Game;
using HISP.Game.SwfModules;

namespace HISP.Server
{
    public class PacketBuilder
    {

        public const byte PACKET_TERMINATOR = 0x00;
        public const byte PACKET_CLIENT_TERMINATOR = 0x0A;

        public const byte PACKET_FLASH_XML_CROSSDOMAIN = 0x3C;

        public const byte PACKET_LOGIN = 0x7F;
        public const byte PACKET_CHAT = 0x14;
        public const byte PACKET_MOVE = 0x15;
        public const byte PACKET_CLICK = 0x77;
        public const byte PACKET_USERINFO = 0x81;
        public const byte PACKET_WORLD = 0x7A;
        public const byte PACKET_BASE_STATS = 0x7B;
        public const byte PACKET_RANCH = 0x23;
        public const byte PACKET_SWF_CUTSCENE = 0x29;
        public const byte PACKET_SWF_MODULE_FORCE = 0x28;
        public const byte PACKET_SWF_MODULE_GENTLE = 0x2A;
        public const byte PACKET_PLACE_INFO = 0x1E;
        public const byte PACKET_BIRDMAP = 0x76;
        public const byte PACKET_HORSE = 0x19;
        public const byte PACKET_AREA_DEFS = 0x79;
        public const byte PACKET_ITEM_INTERACTION = 0x1E;
        public const byte PACKET_ANNOUNCEMENT = 0x7E;
        public const byte PACKET_TILE_FLAGS = 0x75;
        public const byte PACKET_PLAYSOUND = 0x23;
        public const byte PACKET_KEEP_ALIVE = 0x7C;
        public const byte PACKET_DYNAMIC_BUTTON = 0x45;
        public const byte PACKET_DYNAMIC_INPUT = 0x46;
        public const byte PACKET_ARENA_SCORE = 0x2D;
        public const byte PACKET_PLAYER = 0x18;
        public const byte PACKET_INVENTORY = 0x17;
        public const byte PACKET_TRANSPORT = 0x29;
        public const byte PACKET_KICK = 0x80;
        public const byte PACKET_LEAVE = 0x7D;
        public const byte PACKET_NPC = 0x28;
        public const byte PACKET_QUIT = 0x7D;
        public const byte PACKET_PLAYERINFO = 0x16;
        public const byte PACKET_INFORMATION = 0x28;
        public const byte PACKET_WISH = 0x2C;
        public const byte PACKET_SWFMODULE = 0x50;
        public const byte PACKET_AUCTION = 0x24;
        public const byte PACKET_PLAYER_INTERACTION = 0x2A;
        public const byte PACKET_SOCIALS = 0x5A;

        public const byte SOCIALS_MENU = 0x14;
        public const byte SOCIALS_USE = 0x15;

        public const byte PLAYER_INTERACTION_PROFILE = 0x14;
        public const byte PLAYER_INTERACTION_TAG = 0x23;
        public const byte PLAYER_INTERACTION_TRADE = 0x28;
        public const byte PLAYER_INTERACTION_ADD_ITEM = 0x29;
        public const byte PLAYER_INTERACTION_ACCEPT = 0x2A;
        public const byte PLAYER_INTERACTION_TRADE_REJECT = 0x2B;
        public const byte PLAYER_INTERACTION_ADD_BUDDY = 0x1E;
        public const byte PLAYER_INTERACTION_REMOVE_BUDDY = 0x1F;
        public const byte PLAYER_INTERACTION_MUTE = 0x32;
        public const byte PLAYER_INTERACTION_UNMUTE = 0x33;

        public const byte AUCTION_BID_100 = 0x29;
        public const byte AUCTION_BID_1K = 0x2A;
        public const byte AUCTION_BID_10K = 0x2B;
        public const byte AUCTION_BID_100K = 0x2C;
        public const byte AUCTION_BID_1M = 0x2D;
        public const byte AUCTION_BID_10M = 0x2E;
        public const byte AUCITON_BID_100M = 0x2F;

        public const byte RANCH_BUY = 0x14;
        public const byte RANCH_INFO = 0x16;
        public const byte RANCH_CLICK = 0x17;
        public const byte RANCH_UPGRADE = 0x18;
        public const byte RANCH_SELL = 0x15;
        public const byte RANCH_BUILD = 0x19;
        public const byte RANCH_REMOVE = 0x1A;
        public const byte RANCH_CLICK_NORM = 0x14;
        public const byte RANCH_CLICK_BUILD = 0x15;

        public const byte HORSE_LIST = 0x0A;
        public const byte HORSE_LOOK = 0x14;
        public const byte HORSE_FEED = 0x15;
        public const byte HORSE_ENTER_ARENA = 0x2D;
        public const byte HORSE_PET = 0x18;
        public const byte HORSE_PROFILE = 0x2C;
        public const byte HORSE_PROFILE_EDIT = 0x14;
        public const byte HORSE_TRY_CAPTURE = 0x1C;
        public const byte HORSE_RELEASE = 0x19;
        public const byte HORSE_TACK = 0x16;
        public const byte HORSE_DRINK = 0x2B;
        public const byte HORSE_GIVE_FEED = 0x1B;
        public const byte HORSE_TACK_EQUIP = 0x3C;
        public const byte HORSE_TACK_UNEQUIP = 0x3D;
        public const byte HORSE_VET_SERVICE = 0x2A;
        public const byte HORSE_VET_SERVICE_ALL = 0x2F;
        public const byte HORSE_GROOM_SERVICE_ALL = 0x33;
        public const byte HORSE_GROOM_SERVICE = 0x32;
        public const byte HORSE_BARN_SERVICE = 0x37;
        public const byte HORSE_BARN_SERVICE_ALL = 0x38;
        public const byte HORSE_SHOE_IRON = 0x28;
        public const byte HORSE_SHOE_STEEL = 0x29;
        public const byte HORSE_SHOE_ALL = 0x2E;
        public const byte HORSE_TRAIN = 0x1A;
        public const byte HORSE_MOUNT = 0x46;
        public const byte HORSE_DISMOUNT = 0x47;
        public const byte HORSE_ESCAPE = 0x1E;
        public const byte HORSE_CAUGHT = 0x1D;

        public const byte SWFMODULE_INVITE = 0x14;
        public const byte SWFMODULE_ACCEPT = 0x15;
        public const byte SWFMODULE_CLOSE = 0x16;

        public const byte SWFMODULE_2PLAYER_CLOSED = 0x58;
        public const byte SWFMODULE_2PLAYER = 0x50;
        public const byte SWFMODULE_ARENA = 0x52;
        public const byte SWFMODULE_BRICKPOET = 0x5A;
        public const byte SWFMODULE_DRAWINGROOM = 0x5B;
        public const byte SWFMODULE_DRESSUPROOM = 0x5C;
        public const byte SWFMODULE_BANDHALL = 0x51;

        public const byte DRAWINGROOM_GET_DRAWING = 0x14;
        public const byte DRAWINGROOM_SAVE = 0x15;
        public const byte DRAWINGROOM_LOAD = 0x16;

        public const byte BRICKPOET_LIST_ALL = 0x14;
        public const byte BRICKPOET_MOVE = 0x55;

        public const byte DRESSUPROOM_LIST_ALL = 0x14;

        public const byte WISH_MONEY = 0x31;
        public const byte WISH_ITEMS = 0x32;
        public const byte WISH_WORLDPEACE = 0x33;

        public const byte SECCODE_QUEST = 0x32;
        public const byte SECCODE_GIVE_ITEM = 0x28;
        public const byte SECCODE_DELETE_ITEM = 0x29;
        public const byte SECCODE_SCORE = 0x3D;
        public const byte SECCODE_WINLOOSE = 0x3C;
        public const byte SECCODE_TIME = 0x3E;
        public const byte SECCODE_MONEY = 0x1E;
        public const byte SECCODE_AWARD = 0x33;

        public const byte WINLOOSE_WIN = 0x14;
        public const byte WINLOOSE_LOOSE = 0x15;

        public const byte NPC_START_CHAT = 0x14;
        public const byte NPC_CONTINUE_CHAT = 0x15;

        public const byte PLAYERINFO_LEAVE = 0x16;
        public const byte PLAYERINFO_UPDATE_OR_CREATE = 0x15;
        public const byte PLAYERINFO_PLAYER_LIST = 0x14;

        public const byte PROFILE_WINLOOSE_LIST = 0x50;
        public const byte PROFILE_HIGHSCORES_LIST = 0x51;
        public const byte PROFILE_BESTTIMES_LIST = 0x52;

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
        public const byte ITEM_BUY_AND_CONSUME = 0x34;
        public const byte ITEM_BUY_5 = 0x35;
        public const byte ITEM_BUY_25 = 0x37;
        public const byte ITEM_SELL = 0x3C;
        public const byte ITEM_WRAP = 0x17;
        public const byte ITEM_SELL_ALL = 0x3D;
        public const byte ITEM_VIEW = 0x2A;
        public const byte ITEM_LOOK = 0x4C;
        public const byte ITEM_READ = 0x52;
        public const byte ITEM_RIP = 0x2B;
        public const byte ITEM_OPEN = 0x16;
        public const byte ITEM_WEAR = 0x46;
        public const byte ITEM_REMOVE = 0x47;
        public const byte ITEM_CONSUME = 0x51;
        public const byte ITEM_DRINK = 0x52;
        public const byte ITEM_BINOCULARS = 0x5C;
        public const byte ITEM_THROW = 0x1F;
        public const byte ITEM_MAGNIFYING = 0x5D;
        public const byte ITEM_CRAFT = 0x64;
        public const byte ITEM_USE = 0x5F;
        public const byte ITEM_RAKE = 0x5B;
        public const byte ITEM_SHOVEL = 0x5A;
        
        public const byte LOGIN_INVALID_USER_PASS = 0x15;
        public const byte LOGIN_CUSTOM_MESSAGE = 0x16;
        public const byte LOGIN_SUCCESS = 0x14;

        public const byte WEATHER_UPDATE = 0x13;

        public const byte DIRECTION_UP = 0;
        public const byte DIRECTION_RIGHT = 1;
        public const byte DIRECTION_DOWN = 2;
        public const byte DIRECTION_LEFT = 3;
        public const byte DIRECTION_TELEPORT = 4;
        public const byte DIRECTION_NONE = 10;

        public static byte[] Create2PlayerClose()
        {
            byte[] packet = new byte[3];
            packet[0] = PACKET_SWFMODULE;
            packet[1] = SWFMODULE_2PLAYER_CLOSED;
            packet[2] = PACKET_TERMINATOR;

            return packet;
        }
        public static byte[] CreateDressupRoomPeiceMove(int peiceId, double x, double y, bool active)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_SWFMODULE);
            string packetStr = "";
            packetStr += peiceId.ToString() + "|";
            if (active)
            {
                packetStr += x.ToString() + "|";
                packetStr += y.ToString() + "|";
            }
            else
            {
                packetStr += "D|D|";
            }
            packetStr += "^";

            byte[] packetBytes = Encoding.UTF8.GetBytes(packetStr);
            ms.Write(packetBytes, 0x00, packetBytes.Length);
            ms.WriteByte(PACKET_TERMINATOR);
            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] response = ms.ToArray();
            ms.Dispose();
            return response;
        }
        public static byte[] CreateDressupRoomPeiceResponse(Dressup.DressupPeice[] dressupPeices)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_SWFMODULE);
            string packetStr = "";
            foreach(Dressup.DressupPeice peice in dressupPeices)
            {
                if (!peice.Active)
                    continue;

                packetStr += peice.PeiceId.ToString() + "|";
                packetStr += peice.X.ToString() + "|";
                packetStr += peice.Y.ToString() + "|";
                packetStr += "^";
            }

            byte[] packetBytes = Encoding.UTF8.GetBytes(packetStr);
            ms.Write(packetBytes, 0x00, packetBytes.Length);
            ms.WriteByte(PACKET_TERMINATOR);
            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] response = ms.ToArray();
            ms.Dispose();
            return response;
        }

        public static byte[] CreateForwardedSwfRequest(byte[] request)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_SWFMODULE);
            ms.Write(request, 0x2, request.Length - 0x4);
            ms.WriteByte(PACKET_TERMINATOR);
            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] response = ms.ToArray();
            ms.Dispose();
            return response;
        }

        public static byte[] CreateBirdMap(int playerX, int playerY)
        {
            MemoryStream ms = new MemoryStream();

            int xstart = playerX - 24;
            int ystart = playerY - 15;

            ms.WriteByte(PACKET_BIRDMAP);

            for (int rely = 0; rely <= 30; rely++)
            {
                for (int relx = 0; relx <= 48; relx++)
                {
                    int tileId = Map.GetTileId(xstart + relx, ystart + rely, false);
                    int otileId = Map.GetTileId(xstart + relx, ystart + rely, true);

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

            ms.WriteByte(PACKET_TERMINATOR);
            ms.Seek(0x00, SeekOrigin.Begin);
            return ms.ToArray();
        }
        public static byte[] CreateDrawingUpdatePacket(string Drawing)
        {
            byte[] drawingBytes = Encoding.UTF8.GetBytes(Drawing);
            byte[] packet = new byte[(1 * 2) + drawingBytes.Length];

            packet[0] = PACKET_SWFMODULE;
            Array.Copy(drawingBytes, 0, packet, 1, drawingBytes.Length);
            packet[packet.Length-1] = PACKET_TERMINATOR;

            return packet;
        }
        public static byte[] CreateBrickPoetMovePacket(Brickpoet.PoetryPeice peice)
        {
            string packetStr = "|";
            packetStr += peice.Id + "|";
            packetStr += peice.X + "|";
            packetStr += peice.Y + "|";
            packetStr += "^";

            byte[] infoBytes = Encoding.UTF8.GetBytes(packetStr);
            byte[] packet = new byte[(1 * 3) + infoBytes.Length];

            packet[0] = PACKET_SWFMODULE;
            packet[1] = BRICKPOET_MOVE;

            Array.Copy(infoBytes, 0, packet, 2, infoBytes.Length);

            packet[packet.Length-1] = PACKET_TERMINATOR;

            return packet;
        }
        public static byte[] CreateBrickPoetListPacket(Brickpoet.PoetryPeice[] room)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_SWFMODULE);
            string packetStr = "";
            foreach(Brickpoet.PoetryPeice peice in room)
            {
                packetStr += "A";
                packetStr += "|";
                packetStr += peice.Id;
                packetStr += "|";
                packetStr += peice.Word.ToUpper();
                packetStr += "|";
                packetStr += peice.X;
                packetStr += "|";
                packetStr += peice.Y;
                packetStr += "|";
                packetStr += "^";
            }
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetStr);
            ms.Write(packetBytes, 0x00, packetBytes.Length);
            ms.WriteByte(PACKET_TERMINATOR);

            ms.Seek(0x00, SeekOrigin.Begin);
            return ms.ToArray();
        }
        public static byte[] CreatePlaysoundPacket(string sound)
        {
            byte[] soundBytes = Encoding.UTF8.GetBytes(sound);
            byte[] packet = new byte[(1 * 2) + soundBytes.Length];

            packet[0] = PACKET_PLAYSOUND;

            Array.Copy(soundBytes, 0, packet, 1, soundBytes.Length);

            packet[packet.Length - 1] = PACKET_TERMINATOR;

            return packet;
        }
        public static byte[] CreatePlayerLeavePacket(string username)
        {
            byte[] userBytes = Encoding.UTF8.GetBytes(username);
            byte[] packet = new byte[(1 * 3) + userBytes.Length];

            packet[0] = PACKET_PLAYERINFO;
            packet[1] = PLAYERINFO_LEAVE;

            Array.Copy(userBytes, 0, packet, 2, userBytes.Length);

            packet[packet.Length - 1] = PACKET_TERMINATOR;

            return packet;
        }
        public static byte[] CreatePlayerInfoUpdateOrCreate(int x, int y, int facing, int charId, string username)
        {
            byte[] userBytes = Encoding.UTF8.GetBytes(username);
            byte[] packet = new byte[(1 * 10) + userBytes.Length];

            packet[0] = PACKET_PLAYERINFO;
            packet[1] = PLAYERINFO_UPDATE_OR_CREATE;

            packet[2] = (byte)(((x - 4) / 64) + 20);
            packet[3] = (byte)(((x - 4) % 64) + 20);

            packet[4] = (byte)(((y - 1) / 64) + 20);
            packet[5] = (byte)(((y - 1) % 64) + 20);

            packet[6] = (byte)(facing + 20);

            packet[7] = (byte)((charId / 64) + 20);
            packet[8] = (byte)((charId % 64) + 20);

            Array.Copy(userBytes, 0, packet, 9, userBytes.Length);

            packet[packet.Length-1] = PACKET_TERMINATOR;

            return packet;
        }

        public static byte[] CreateLoginPacket(bool Success, string message="")
        {
            byte[] loginFailMessage = Encoding.UTF8.GetBytes(message);
            byte[] packet = new byte[(1 * 3) + loginFailMessage.Length];

            packet[0] = PACKET_LOGIN;
            if (message != "")
                packet[1] = LOGIN_CUSTOM_MESSAGE;
            else if (Success)
                packet[1] = LOGIN_SUCCESS;
            else
                packet[1] = LOGIN_INVALID_USER_PASS;

            Array.Copy(loginFailMessage, 0, packet, 2, loginFailMessage.Length);

            packet[packet.Length-1] = PACKET_TERMINATOR;

            return packet;
        }

        public static byte[] CreateProfilePacket(string userProfile)
        {
            byte[] profileBytes = Encoding.UTF8.GetBytes(userProfile);
            byte[] packet = new byte[(1 * 2) + profileBytes.Length];

            packet[0] = PACKET_PLAYER;
            Array.Copy(profileBytes, 0, packet, 1, profileBytes.Length);
            packet[packet.Length-1] = PACKET_TERMINATOR;

            return packet;
        }

        public static byte[] CreateMovementPacket(int x, int y, int charId, int facing, int direction, bool walk)
        {
            // Header information
            List<byte> packet = new List<byte>();

            packet.Add(PACKET_MOVE);

            packet.Add((byte)(((x-4) / 64) + 20)); //1
            packet.Add((byte)(((x-4) % 64) + 20)); //2

            packet.Add((byte)(((y-1) / 64) + 20)); //3
            packet.Add((byte)(((y-1) % 64) + 20)); //4

            packet.Add((byte)(facing + 20)); //5

            packet.Add((byte)((charId / 64) + 20)); //6
            packet.Add((byte)((charId % 64) + 20)); //7
            packet.Add((byte)(direction + 20)); //8
            packet.Add((byte)(Convert.ToInt32(walk) + 20)); //9


            // Map Data
            bool moveTwo = false;
            if(direction >= 20)
            {
                direction -= 20;
                moveTwo = true;
            }
            
            int ystart = y - 4;
            int xstart = x - 6;
            int xend = xstart + 12;
            int yend = ystart + 9;

            if (direction == DIRECTION_UP)
            {
                int totalY = 0;
                if (moveTwo)
                {
                    ystart++;
                    totalY = 1;
                }

                for (int yy = ystart; yy >= ystart - totalY; yy--)
                {
                    for (int xx = xstart; xx <= xend; xx++)
                    {
                        int tileId = Map.GetTileId(xx, yy, false);
                        int otileId = Map.GetTileId(xx, yy, true);

                        if (tileId >= 190)
                        {
                            packet.Add((byte)190);
                            tileId -= 100;
                        }
                        packet.Add((byte)tileId);

                        if (otileId >= 190)
                        {
                            packet.Add((byte)190);
                            otileId -= 100;
                        }
                        packet.Add((byte)otileId);
                    }
                }
            }

            if (direction == DIRECTION_LEFT)
            {
                int totalX = 0;
                if (moveTwo)
                {
                    xstart++;
                    totalX = 1;
                }

                for (int xx = xstart; xx >= xstart - totalX; xx--)
                {
                    for (int yy = ystart; yy <= yend; yy++)
                    {
                        int tileId = Map.GetTileId(xx, yy, false);
                        int otileId = Map.GetTileId(xx, yy, true);



                        if (tileId >= 190)
                        {
                            packet.Add((byte)190);
                            tileId -= 100;
                        }
                        packet.Add((byte)tileId);

                        if (otileId >= 190)
                        {
                            packet.Add((byte)190);
                            otileId -= 100;
                        }
                        packet.Add((byte)otileId);
                    }
                }
            }


            if (direction == DIRECTION_RIGHT)
            {
                int totalX = 0;
                if (moveTwo)
                {
                    xend--;
                    totalX = 1;
                }

                for (int xx = xend; xx <= xend + totalX; xx++)
                {

                    for (int yy = ystart; yy <= yend; yy++)
                    {
                        int tileId = Map.GetTileId(xx, yy, false);
                        int otileId = Map.GetTileId(xx, yy, true);


                        if (tileId >= 190)
                        {
                            packet.Add((byte)190);
                            tileId -= 100;
                        }
                        packet.Add((byte)tileId);

                        if (otileId >= 190)
                        {
                            packet.Add((byte)190);
                            otileId -= 100;
                        }
                        packet.Add((byte)otileId);

                    }
                }
            }

            if (direction == DIRECTION_DOWN)
            {
                int totalY = 0;
                if (moveTwo)
                {
                    yend--;
                    totalY = 1;
                }

                for (int yy = yend; yy <= yend + totalY; yy++)
                {

                    for (int xx = xstart; xx <= xend; xx++)
                    {
                        int tileId = Map.GetTileId(xx, yy, false);
                        int otileId = Map.GetTileId(xx, yy, true);


                        if (tileId >= 190)
                        {
                            packet.Add((byte)190);
                            tileId -= 100;
                        }
                        packet.Add((byte)tileId);

                        if (otileId >= 190)
                        {
                            packet.Add((byte)190);
                            otileId -= 100;
                        }
                        packet.Add((byte)otileId);

                    }
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
                            packet.Add((byte)190);
                            tileId -= 100;
                        }
                        packet.Add((byte)tileId);

                        if (otileId >= 190)
                        {
                            packet.Add((byte)190);
                            otileId -= 100;
                        }
                        packet.Add((byte)otileId);

                    }
                }

            }
            packet.Add(PACKET_TERMINATOR);

            return packet.ToArray();
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

        public static byte[] CreateWeatherUpdatePacket(string newWeather)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(newWeather);

            MemoryStream ms = new MemoryStream();
            ms.WriteByte(PACKET_WORLD);
            ms.WriteByte(WEATHER_UPDATE);
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
            byte[] moneyStrBytes = Encoding.UTF8.GetBytes(money.ToString("N0", CultureInfo.InvariantCulture));
            byte[] playerStrBytes = Encoding.UTF8.GetBytes(playerCount.ToString("N0", CultureInfo.InvariantCulture));
            byte[] mailStrBytes = Encoding.UTF8.GetBytes(mail.ToString("N0", CultureInfo.InvariantCulture));

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
