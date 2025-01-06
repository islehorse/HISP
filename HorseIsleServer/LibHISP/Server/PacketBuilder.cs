using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using HISP.Game;
using HISP.Game.SwfModules;
using HISP.Util;
namespace HISP.Server
{
    public class PacketBuilder
    {
        public const int PACKET_CLIENT_TERMINATOR_LENGTH = 1;
        public const byte PACKET_CLIENT_TERMINATOR = 0x0A;

        // hi1 packets
        public const byte PACKET_LOGIN = 0x7F;
        public const byte PACKET_CHAT = 0x14;
        public const byte PACKET_MOVE = 0x15;
        public const byte PACKET_CLICK = 0x77;
        public const byte PACKET_SEC_CODE = 0x81;
        public const byte PACKET_WORLD = 0x7A;
        public const byte PACKET_BASE_STATS = 0x7B;
        public const byte PACKET_RANCH = 0x23;
        public const byte PACKET_SWF_MODULE_CUTSCENE = 0x29;
        public const byte PACKET_SWF_MODULE_FORCE = 0x28;
        public const byte PACKET_SWF_MODULE_GENTLE = 0x2A;
        public const byte PACKET_META = 0x1E;
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
        public const byte SWFMODULE_OPPONENT = 0x50;
        public const byte SWFMODULE_ARENA = 0x52;
        public const byte SWFMODULE_BRICKPOET = 0x5A;
        public const byte SWFMODULE_DRAWINGROOM = 0x5B;
        public const byte SWFMODULE_DRESSUPROOM = 0x5C;
        public const byte SWFMODULE_BROADCAST = 0x51;

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

        // Helper function for packets that return map data, (eg CreateMovement or CreateBirdMap)
        // To encode tile data and add it to a given packet represented as a List<Byte>. 
        private static void encodeTileDataAndAddToPacket(List<byte> packet, int tileId, int otileId)
        {
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

        // Creates a byte array of a packet to inform the client that Player 2 in the current 2Player game
        // has actually left / quit the game.
        public static byte[] Create2PlayerClose()
        {
            byte[] packet = new byte[2];
            packet[0] = PACKET_SWFMODULE;
            packet[1] = SWFMODULE_2PLAYER_CLOSED;

            return packet;
        }

        // Creates a byte array of a packet to inform the client that a peice in a dressup room
        // was moved to another location.
        public static byte[] CreateDressupRoomPeiceMove(int peiceId, double x, double y, bool active)
        {
            string peiceMoveStr = "";
            peiceMoveStr += peiceId.ToString() + "|";
            if (active)
            {
                peiceMoveStr += x.ToString() + "|";
                peiceMoveStr += y.ToString() + "|";
            }
            else
            {
                peiceMoveStr += "D|D|";
            }
            peiceMoveStr += "^";
            byte[] peiceMoveBytes = Encoding.UTF8.GetBytes(peiceMoveStr);

            byte[] packet = new byte[1 + peiceMoveBytes.Length];

            packet[0] = PACKET_SWFMODULE;
            Array.Copy(peiceMoveBytes, 0, packet, 1, peiceMoveBytes.Length);
            
            return packet;
        }
        // Creates a byte array of a packet to inform the client of all the peices
        // in a given dressup room.
        public static byte[] CreateDressupRoomPeiceLoad(Dressup.DressupPeice[] dressupPeices)
        {
            string peiceLoadStr = "";
            foreach(Dressup.DressupPeice peice in dressupPeices)
            {
                if (!peice.Active)
                    continue;

                peiceLoadStr += peice.PeiceId.ToString() + "|";
                peiceLoadStr += peice.X.ToString() + "|";
                peiceLoadStr += peice.Y.ToString() + "|";
                peiceLoadStr += "^";
            }

            byte[] peiceLoadBytes = Encoding.UTF8.GetBytes(peiceLoadStr);
            byte[] packet = new byte[1 + peiceLoadBytes.Length];

            packet[0] = PACKET_SWFMODULE;
            Array.Copy(peiceLoadBytes, 0, packet, 1, peiceLoadBytes.Length);

            return packet;
        }
        // Creates a byte array that contains the contents of a request byte array
        // as the response, it basically just forwards it onwards
        // this is used for *most* SwfModule
        public static byte[] CreateForwardedSwfModule(byte[] request)
        {
            byte[] packet = new byte[1 + (request.Length-1)];
            packet[0] = PACKET_SWFMODULE;
            Array.Copy(request, 0, packet, 1, (request.Length-2));
            return packet;
        }
        // Creates a byte array that contains "Bird Map" data
        // From a given X/Y Position, this is primarily used to handle
        // using the telescope item in game 
        public static byte[] CreateBirdMap(int X, int Y)
        {
            // The size is always fixed in this case, but i still have to use a List<byte> because
            // encodeTileDataAndAddToPacket expects packet as a List<byte>.
            List<byte> packet = new List<byte>();

            // Calculate top left corner of BirdMap viewport
            // from given X/Y position.
            int startX = X - 24;
            int startY = Y - 15;

            packet.Add(PACKET_BIRDMAP);

            for (int rely = 0; rely <= 30; rely++)
            {
                for (int relx = 0; relx <= 48; relx++)
                {
                    int tileId = Map.GetTileId(startX + relx, startY + rely, false);
                    int otileId = Map.GetTileId(startX + relx, startY + rely, true);
                    encodeTileDataAndAddToPacket(packet, tileId, otileId);
                }
            }

            return packet.ToArray();
        }
        // Creates a byte array for a packet to inform the client that the image in a drawing room has changed.
        public static byte[] CreateDrawingUpdate(string Drawing)
        {
            byte[] drawingBytes = Encoding.UTF8.GetBytes(Drawing);
            byte[] packet = new byte[1 + drawingBytes.Length];

            packet[0] = PACKET_SWFMODULE;
            Array.Copy(drawingBytes, 0, packet, 1, drawingBytes.Length);

            return packet;
        }
        // Creates a byte array for a packet to inform the client that a poetry peice in a brick poet room has moved.
        public static byte[] CreateBrickPoetMove(Brickpoet.PoetryPeice peice)
        {
            string peiceUpdateStr = "|";
            peiceUpdateStr += peice.Id + "|";
            peiceUpdateStr += peice.X + "|";
            peiceUpdateStr += peice.Y + "|";
            peiceUpdateStr += "^";

            byte[] infoBytes = Encoding.UTF8.GetBytes(peiceUpdateStr);
            byte[] packet = new byte[(1 * 2) + infoBytes.Length];

            packet[0] = PACKET_SWFMODULE;
            packet[1] = BRICKPOET_MOVE;

            Array.Copy(infoBytes, 0, packet, 2, infoBytes.Length);


            return packet;
        }
        // Creates a byte array for a packet to inform the client of all all Poetry Peices in a Brick Poet room
        public static byte[] CreateBrickPoetList(Brickpoet.PoetryPeice[] room)
        {
            string peicesStr = "";
            foreach(Brickpoet.PoetryPeice peice in room)
            {
                peicesStr += "A";
                peicesStr += "|";
                peicesStr += peice.Id;
                peicesStr += "|";
                peicesStr += peice.Word.ToUpper();
                peicesStr += "|";
                peicesStr += peice.X;
                peicesStr += "|";
                peicesStr += peice.Y;
                peicesStr += "|";
                peicesStr += "^";
            }
            byte[] peicesBytes = Encoding.UTF8.GetBytes(peicesStr);
            byte[] packet = new byte[1 + peicesBytes.Length];

            packet[0] = PACKET_SWFMODULE;

            Array.Copy(peicesBytes, 0, packet, 1, peicesBytes.Length);

            return packet;
        }
        // Creates a byte array of a packet requesting the client to play a sound effect.
        public static byte[] CreatePlaySound(string sound)
        {
            byte[] soundBytes = Encoding.UTF8.GetBytes(sound);
            byte[] packet = new byte[1 + soundBytes.Length];

            packet[0] = PACKET_PLAYSOUND;

            Array.Copy(soundBytes, 0, packet, 1, soundBytes.Length);


            return packet;
        }
        // Creates a byte array of a packet informing the client that a given user has left the game
        // So they can be removed from the chat list
        public static byte[] CreatePlayerLeave(string username)
        {
            byte[] userBytes = Encoding.UTF8.GetBytes(username);
            byte[] packet = new byte[(1 * 2) + userBytes.Length];

            packet[0] = PACKET_PLAYERINFO;
            packet[1] = PLAYERINFO_LEAVE;

            Array.Copy(userBytes, 0, packet, 2, userBytes.Length);

            return packet;
        }
        // Creates a byte array of a packet informing the client that a given player has changed position,
        // changed direction, or changed character sprites
        public static byte[] CreatePlayerInfoUpdateOrCreate(int x, int y, int facing, int charId, string username)
        {
            byte[] userBytes = Encoding.UTF8.GetBytes(username);
            byte[] packet = new byte[(1 * 9) + userBytes.Length];

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


            return packet;
        }
        // Creates a byte array of a packet to inform the client
        // if a given Login Attempt was successful or not
        public static byte[] CreateLogin(bool Success, string ErrorMessage="")
        {
            byte[] loginFailMessage = Encoding.UTF8.GetBytes(ErrorMessage);
            byte[] packet = new byte[(1 * 2) + loginFailMessage.Length];

            packet[0] = PACKET_LOGIN;
            if (ErrorMessage != "")
                packet[1] = LOGIN_CUSTOM_MESSAGE;
            else if (Success)
                packet[1] = LOGIN_SUCCESS;
            else
                packet[1] = LOGIN_INVALID_USER_PASS;

            Array.Copy(loginFailMessage, 0, packet, 2, loginFailMessage.Length);

            return packet;
        }
        // Creates a byte array of a packet to inform the client of
        // the users current profile page, or "about me"
        // This is for the the "Profile" button 
        public static byte[] CreateProfilePage(string userProfile)
        {
            byte[] profileBytes = Encoding.UTF8.GetBytes(userProfile);
            byte[] packet = new byte[1 + profileBytes.Length];

            packet[0] = PACKET_PLAYER;
            Array.Copy(profileBytes, 0, packet, 1, profileBytes.Length);
            
            return packet;
        }
        // Creates a byte array of a packet to inform the client of the players
        // new X/Y position, there character id, facing direction, and Tile Data for their position in the map.
        public static byte[] CreateMovement(int x, int y, int charId, int facing, int direction, bool walk)
        {

            /* Packet HEADER */

            // Packet size varries too much and so using a dynamically sized list of bytes instead of a byte[]
            List<byte> packet = new List<byte>();

            packet.Add(PACKET_MOVE); // 0x0

            packet.Add((byte)(((x-4) / 64) + 20)); //0x1
            packet.Add((byte)(((x-4) % 64) + 20)); //0x2

            packet.Add((byte)(((y-1) / 64) + 20)); //0x3
            packet.Add((byte)(((y-1) % 64) + 20)); //0x4

            packet.Add((byte)(facing + 20)); //0x5

            packet.Add((byte)((charId / 64) + 20)); //0x6
            packet.Add((byte)((charId % 64) + 20)); //0x7
            packet.Add((byte)(direction + 20)); //0x8
            packet.Add((byte)(Convert.ToInt32(walk) + 20)); //0x9

            /* Packet PAYLOAD */
            bool moveTwo = false;
            if(direction >= 20) // is the player riding a horse?
            {
                direction -= 20;
                moveTwo = true;
            }
            

            // Calculate start of the client's viewport start offset from top-left origin
            int startY = y - 4;
            int startX = x - 6;
            int endX = startX + 12;
            int endY = startY + 9;

            // This giant if case logic essentially
            // Pulls the missing tile data portion from map file
            // And encodes it into packet data
            if (direction == DIRECTION_UP)
            {
                int totalY = 0;
                if (moveTwo)
                {
                    startY++;
                    totalY = 1;
                }
                
                for (int curY = startY; curY >= startY - totalY; curY--)
                {
                    for (int curX = startX; curX <= endX; curX++)
                    {
                        int tileId = Map.GetTileId(curX, curY, false);
                        int otileId = Map.GetTileId(curX, curY, true);
                        encodeTileDataAndAddToPacket(packet, tileId, otileId);
                    }
                }
            }
            else if (direction == DIRECTION_LEFT)
            {
                int totalX = 0;
                if (moveTwo)
                {
                    startX++;
                    totalX = 1;
                }

                for (int curX = startX; curX >= startX - totalX; curX--)
                {
                    for (int curY = startY; curY <= endY; curY++)
                    {
                        int tileId = Map.GetTileId(curX, curY, false);
                        int otileId = Map.GetTileId(curX, curY, true);
                        encodeTileDataAndAddToPacket(packet, tileId, otileId);
                    }
                }
            }
            else if (direction == DIRECTION_RIGHT)
            {
                int totalX = 0;
                if (moveTwo)
                {
                    endX--;
                    totalX = 1;
                }

                for (int curX = endX; curX <= endX + totalX; curX++)
                {

                    for (int curY = startY; curY <= endY; curY++)
                    {
                        int tileId = Map.GetTileId(curX, curY, false);
                        int otileId = Map.GetTileId(curX, curY, true);
                        encodeTileDataAndAddToPacket(packet, tileId, otileId);

                    }
                }
            }
            else if (direction == DIRECTION_DOWN)
            {
                int totalY = 0;
                if (moveTwo)
                {
                    endY--;
                    totalY = 1;
                }

                for (int curY = endY; curY <= endY + totalY; curY++)
                {

                    for (int curX = startX; curX <= endX; curX++)
                    {
                        int tileId = Map.GetTileId(curX, curY, false);
                        int otileId = Map.GetTileId(curX, curY, true);
                        encodeTileDataAndAddToPacket(packet, tileId, otileId);
                    }
                }

            }
            else if (direction == DIRECTION_TELEPORT)
            {
                for(int rely = 0; rely <= 9; rely++)
                {
                    for (int relx = 0; relx <= 12; relx++)
                    {
                        int tileId = Map.GetTileId(startX + relx, startY + rely, false);
                        int otileId = Map.GetTileId(startX + relx, startY + rely, true);
                        encodeTileDataAndAddToPacket(packet, tileId, otileId);
                    }
                }

            }

            return packet.ToArray();
        }
        // Creates a byte array of a packet containing Information about a specific tile
        // used when you click on a tile in the client, it gives you some extra info about it.
        public static byte[] CreateTileClickInfo(string text)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(text);
            byte[] packet = new byte[1 + strBytes.Length];
            packet[0] = PACKET_CLICK;
            Array.Copy(strBytes, 0, packet, 1, strBytes.Length);
            return packet;
        }

        // Creates a byte array of a packet containing information to be displayed in the "Meta" window
        // (Thats the one on the top-right corner of the screent hat contains buttons and other widgets)
        public static byte[] CreateMeta(string formattedText)
        {
            byte[] formattedBytes = Encoding.UTF8.GetBytes(formattedText);
            byte[] packet = new byte[1 + formattedBytes.Length];

            packet[0] = PACKET_META;
            Array.Copy(formattedBytes, 0, packet, 1, formattedBytes.Length);

            return packet;
        }
        // Creates a byte array of a packet informing the client to display a chat message
        // And which of the two chat windows to include it in.
        public static byte[] CreateChat(string formattedText, byte chatWindow)
        {
            byte[] formattedBytes = Encoding.UTF8.GetBytes(formattedText);
            byte[] packet = new byte[(1 * 2) + formattedBytes.Length];
            packet[0] = PACKET_CHAT;
            packet[1] = chatWindow;
            Array.Copy(formattedBytes, 0, packet, 2, formattedBytes.Length);
            return packet;
        }

        // Creates a byte array of a packet informing the client to change the current Weather Effect.
        public static byte[] CreateWeatherUpdate(string newWeather)
        {
            byte[] weatherBytes = Encoding.UTF8.GetBytes(newWeather);
            byte[] packet = new byte[(1 * 2) + weatherBytes.Length];
            packet[0] = PACKET_WORLD;
            packet[1] = WEATHER_UPDATE;
            Array.Copy(weatherBytes, 0, packet, 2, weatherBytes.Length);
            return packet;
        }
        // Creates a byte array of a packet informing the client of the current game time, and weather effect.
        public static byte[] CreateTimeAndWeatherUpdate(int gameTime, int gameDay, int gameYear, string weather)
        {
            byte[] weatherBytes = Encoding.UTF8.GetBytes(weather);

            byte[] packet = new byte[(1 * 7) + weatherBytes.Length];

            packet[0] = PACKET_WORLD;
            // Encode current time
            packet[1] = (byte)((gameTime / 64) + 20);
            packet[2] = (byte)((gameTime % 64) + 20);
            // Encode current day
            packet[3] = (byte)((gameDay / 64) + 20);
            packet[4] = (byte)((gameDay % 64) + 20);
            // Encode current year
            packet[5] = (byte)((gameYear / 64) + 20);
            packet[6] = (byte)((gameYear % 64) + 20);

            // Copy weather information to packet
            Array.Copy(weatherBytes, 0, packet, 7, weatherBytes.Length);
            
            return packet;
        }
        // Creates a byte array of a "keep alive" packet, to check if the client is still connected
        // and to inform the client the server is still here too and has not crashed / disconnected the client.
        public static byte[] CreateKeepAlive()
        {
            byte[] packet = new byte[1];
            packet[0] = PACKET_KEEP_ALIVE;
            return packet;
        }
        // Creates a byte array of a packet to inform the client of all "Places" that exist in the world
        // (as defined by gamedata json) This is used in the map view when you hover over certain areas
        public static byte[] CreatePlaceData(World.Isle[] isles, World.Town[] towns, World.Area[] areas)
        {
            // As this information is defined by gamedata.json file
            // the size of it can vary alot, so im using a List<byte> instead of a byte[] here.
            List<byte> packet = new List<byte>();
            packet.Add(PACKET_AREA_DEFS);

            // Encode Towns

            foreach (World.Town town in towns)
            {
                byte[] townBytes = Encoding.UTF8.GetBytes(town.Name);

                packet.Add(AREA_SEPERATOR);
                packet.Add(AREA_TOWN);
                
                packet.Add((byte)(((town.StartX - 4) / 64) + 20));
                packet.Add((byte)(((town.StartX - 4) % 64) + 20));
                
                packet.Add((byte)(((town.EndX - 4) / 64) + 20));
                packet.Add((byte)(((town.EndX - 4) % 64) + 20));
                
                packet.Add((byte)(((town.StartY - 1) / 64) + 20));
                packet.Add((byte)(((town.StartY - 1) % 64) + 20));
                
                packet.Add((byte)(((town.EndY - 1) / 64) + 20));
                packet.Add((byte)(((town.EndY - 1) % 64) + 20));


                Helper.ByteArrayToByteList(townBytes, packet);
            }

            // Encode Areas

            foreach (World.Area area in areas)
            {
                byte[] areaBytes = Encoding.UTF8.GetBytes(area.Name);

                packet.Add(AREA_SEPERATOR);
                packet.Add(AREA_AREA);

                packet.Add((byte)(((area.StartX - 4) / 64) + 20));
                packet.Add((byte)(((area.StartX - 4) % 64) + 20));

                packet.Add((byte)(((area.EndX - 4) / 64) + 20));
                packet.Add((byte)(((area.EndX - 4) % 64) + 20));
                
                packet.Add((byte)(((area.StartY - 1) / 64) + 20));
                packet.Add((byte)(((area.StartY - 1) % 64) + 20));
                
                packet.Add((byte)(((area.EndY - 1) / 64) + 20));
                packet.Add((byte)(((area.EndY - 1) % 64) + 20));


                Helper.ByteArrayToByteList(areaBytes, packet);
            }

            // Encode Isles

            foreach (World.Isle isle in isles)
            {
                byte[] isleBytes = Encoding.UTF8.GetBytes(isle.Name);

                packet.Add(AREA_SEPERATOR);
                packet.Add(AREA_ISLE);

                packet.Add((byte)(((isle.StartX - 4) / 64) + 20));
                packet.Add((byte)(((isle.StartX - 4) % 64) + 20));

                packet.Add((byte)(((isle.EndX - 4) / 64) + 20));
                packet.Add((byte)(((isle.EndX - 4) % 64) + 20));

                packet.Add((byte)(((isle.StartY - 1) / 64) + 20));
                packet.Add((byte)(((isle.StartY - 1) % 64) + 20));

                packet.Add((byte)(((isle.EndY - 1) / 64) + 20));
                packet.Add((byte)(((isle.EndY - 1) % 64) + 20));

                packet.Add((byte)isle.Tileset.ToString()[0]);
                
                Helper.ByteArrayToByteList(isleBytes, packet);
            }

            return packet.ToArray();
        }
        // Creates a byte array of a packet informing the client of the players money, total player count and,
        // how many mail messages they have.
        public static byte[] CreateMoneyPlayerCountAndMail(int money, int playerCount, int mail)
        {
            byte[] playerDataBytes = Encoding.UTF8.GetBytes(money.ToString("N0", CultureInfo.InvariantCulture) + "|" + 
                                                       playerCount.ToString("N0", CultureInfo.InvariantCulture) + "|" + 
                                                       mail.ToString("N0", CultureInfo.InvariantCulture) + "|");
            byte[] packet = new byte[1 + playerDataBytes.Length];
            packet[0] = PACKET_BASE_STATS;
            Array.Copy(playerDataBytes, 0, packet, 1, playerDataBytes.Length);
            return packet;
        }
        // Creates a byte array of a packet informing the client of Tile Overlay flags
        // these tell the client what tiles are and are not passable, which ones the player
        // should appear ontop of or under, and stuff like that.
        public static byte[] CreateTileOverlayFlags(Map.TileDepth[] tileDepthFlags)
        {
            byte[] packet = new byte[1 + tileDepthFlags.Length];
            packet[0] = PACKET_TILE_FLAGS;

            for(int i = 0; i < tileDepthFlags.Length; i++)
            {
                int flag;

                if (!tileDepthFlags[i].ShowPlayer && !tileDepthFlags[i].Passable)
                    flag = 0;
                else if (tileDepthFlags[i].ShowPlayer && !tileDepthFlags[i].Passable)
                    flag = 1;
                else if (!tileDepthFlags[i].ShowPlayer && tileDepthFlags[i].Passable)
                    flag = 2;
                else if (tileDepthFlags[i].ShowPlayer && tileDepthFlags[i].Passable)
                    flag = 3;
                else
                    throw new Exception("Somehow, showplayers was not true or false, and passable was not true or false, this should be impossible");

                packet[1 + i] = Convert.ToByte(flag.ToString()[0]);
            }

            return packet;
        }
        // Creates a byte array of a packet informing the client of its current Sec Code seed and Inc values,
        // Some client packets (eg minigame rewards) require this special Message Authentication Code to validate them
        // Its not at all secure, you can easily just forge these packets by just implementing sec codes, but i didnt make it --
        public static byte[] CreateSecCode(byte[] SecCodeSeed, int SecCodeInc, bool Admin, bool Moderator)
        {

            char userType = 'N'; // Normal?
            
            if (Moderator)
                userType = 'M'; // Moderator

            if (Admin)
                userType = 'A'; // Admin

            byte[] packet = new byte[6];

            packet[0] = PACKET_SEC_CODE;

            packet[1] = (byte)(SecCodeSeed[0] + '!');
            packet[2] = (byte)(SecCodeSeed[1] + '!');
            packet[3] = (byte)(SecCodeSeed[2] + '!');
            packet[4] = (byte)(SecCodeInc     + '!');


            packet[5] = (byte)userType;

            return packet;
        }
        // Creates a byte array of a packet to tell the client to please GET
        // a certain SWF in the mod/ directory on web, and then load it as a MovieClip
        // into the actual game client.
        public static byte[] CreateSwfModule(string swf,byte headerByte)
        {
            byte[] swfBytes = Encoding.UTF8.GetBytes(swf);
            byte[] packet = new byte[1 + swfBytes.Length];
            
            packet[0] = headerByte;
            Array.Copy(swfBytes, 0, packet, 1, swfBytes.Length);
            
            return packet;
        }
        // Creates a byte array of a packet to show the client an "Annoucement" message
        // This has the exact same effect as CreateChat with CHAT_BOTTOM_RIGHT but for some reason
        // the header byte is different,
        // This is basically only used for MOTD.
        public static byte[] CreateMotd(string announcement)
        {
            byte[] annouceBytes = Encoding.UTF8.GetBytes(announcement);
            byte[] packet = new byte[1 + annouceBytes.Length];

            packet[0] = PACKET_ANNOUNCEMENT;
            Array.Copy(annouceBytes, 0, packet, 1, annouceBytes.Length);

            return packet;
        }
        // Creates a byte array of a packet informing the clent that they have been kicked from the server
        // and includes a reason for them being kicked,
        public static byte[] CreateKickMessage(string reason)
        {
            byte[] kickMsgBytes = Encoding.UTF8.GetBytes(reason);
            byte[] packet = new byte[1 + kickMsgBytes.Length];

            packet[0] = PACKET_KICK;
            Array.Copy(kickMsgBytes, 0, packet, 1, kickMsgBytes.Length);

            return packet;
        }

    }
}
