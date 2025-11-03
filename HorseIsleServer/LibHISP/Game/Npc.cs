using HISP.Player;
using HISP.Server;
using System.Collections.Generic;
using System.Linq;


namespace HISP.Game
{
    public class Npc
    {
        public struct NpcReply
        {
            public int Id;
            public string ReplyText;
            public int GotoChatpoint;
            public int RequiresQuestIdCompleted;
            public int RequiresQuestIdNotCompleted;
        }
        public struct NpcChat
        {
            public int Id;
            public string ChatText;
            public int ActivateQuestId;

            public NpcReply[] Replies;
        }
        public class NpcEntry
        {
            public NpcEntry(int npcId, int posX, int posY, bool npcMoves, int udlrStartX=0, int udlrStartY=0)
            {
                id = npcId;
                x = posX;
                y = posY;
                Moves = npcMoves;

                UDLRStartX = udlrStartX;
                UDLRStartY = udlrStartY;

                if(Moves)
                {
                    if(Database.HasNpcPos(id))
                    {
                        udlrScriptPos = Database.GetNpcUdlrPointer(npcId);
                        x = Database.GetNpcPosX(npcId);
                        y = Database.GetNpcPosY(npcId);
                    }
                    else
                    {
                        if(udlrStartX != 0 && udlrStartY != 0)
                        {
                            x = udlrStartX;
                            y = udlrStartY;
                        }
                        udlrScriptPos = 0;
                        Database.AddNpcPos(npcId, x, Y, udlrScriptPos);
                    }
                }
            }
            private int id;
            public string Name;
            public string AdminDescription;
            public string ShortDescription;
            public string LongDescription;
            public bool Moves;
            private int x;
            private int y;
            public int Id
            {
                get
                {
                    return id;
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
                    Database.SetNpcX(id, value);
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
                    Database.SetNpcY(id, value);
                    y = value;
                }
            }
            public string StayOn;
            public int RequiresQuestIdCompleted;
            public int RequiresQuestIdNotCompleted;
            public string UDLRScript;
            public int UDLRStartX;
            public int UDLRStartY;
            public bool AdminOnly;
            public bool LibarySearchable;
            public int IconId;

            private int udlrScriptPos = 0;

            public int UdlrScriptPos
            {
                get
                {
                    return udlrScriptPos;
                }
                set
                {
                    Database.SetNpcUdlrPointer(Id, value);
                    udlrScriptPos = value;
                }
            }
            private bool canNpcBeHere(int x, int y)
            {
                // Horses cannot be in towns.
                if (World.InTown(x, y))
                    return false;
                if (World.InSpecialTile(x, y))
                    return false;

                // Check Tile Type
                int TileID = Map.GetTileId(x, y, false);
                string TileType = Map.TerrainTiles[TileID - 1].Type;

                if (TileType != this.StayOn)
                    return false;

                if (Map.CheckPassable(x, y)) // Can the player stand over here?
                    return true;

                return false;
            }
            public void RandomWander()
            {
                if(Moves)
                {
                    if(UDLRStartX == 0 && UDLRStartY == 0) // not scripted.
                    {
                        if (User.GetUsersAt(this.X, this.Y, true, true).Length > 0)
                            return;

                        int tries = 0;
                        while(true)
                        {
                            int direction = GameServer.RandomNumberGenerator.Next(0, 4);
                            int tryX = this.X;
                            int tryY = this.Y;

                            switch (direction)
                            {
                                case 0:
                                    tryX += 1;
                                    break;
                                case 1:
                                    tryX -= 1;
                                    break;
                                case 2:
                                    tryY += 1;
                                    break;
                                case 3:
                                    tryY -= 1;
                                    break;
                            }

                            if (canNpcBeHere(tryX, tryY))
                            {
                                X = tryX;
                                Y = tryY;
                                break;
                            }
                            tries++;
                            if (tries > 100) // yo stuck lol
                            {
                                Logger.ErrorPrint("NPC: " + this.Name + " is probably stuck (cant move after 100 tries)");
                                break;
                            }
                        }
                        
                    }
                    else // Is Scripted.
                    {
                        if (User.GetUsersAt(this.X, this.Y, true, true).Length > 0)
                            return;

                        if (UdlrScriptPos >= UDLRScript.Length)
                        {
                            X = UDLRStartX;
                            Y = UDLRStartY;
                            UdlrScriptPos = 0;
                        }

                        switch (UDLRScript.ToLower()[UdlrScriptPos])
                        {
                            case 'u':
                                Logger.DebugPrint(this.Name + " Moved up: was " + X + ", " + Y + " now: " + X + ", " + (Y + 1).ToString());
                                Y--;
                                break;
                            case 'd':
                                Logger.DebugPrint(this.Name + " Moved down: was " + X + ", " + Y + " now: " + X + ", " + (Y - 1).ToString());
                                Y++;
                                break;
                            case 'l':
                                Logger.DebugPrint(this.Name + " Moved left: was " + X + ", " + Y + " now: " + (X - 1).ToString() + ", " + Y );
                                X--;
                                break;
                            case 'r':
                                Logger.DebugPrint(this.Name + " Moved right: was " + X + ", " + Y + " now: " + (X + 1).ToString() + ", " + Y);
                                X++;
                                break;
                        }

                        
                        UdlrScriptPos++;

                    }
                }
            }

            public NpcChat[] Chatpoints;
        }

        private static List<NpcEntry> npcList = new List<NpcEntry>();
        public static void AddNpc(NpcEntry npc)
        {
            npcList.Add(npc);
        }
        public static NpcEntry[] NpcList
        {
            get
            {
                return npcList.ToArray();
            }
        }
        public static NpcReply GetNpcReply(NpcEntry npc, int id)
        {
            return npc.Chatpoints.SelectMany(o => o.Replies).First(o => o.Id == id);
        }
        public static NpcReply GetNpcReply(NpcChat chatpoint, int id)
        {
            return chatpoint.Replies.First(o => o.Id == id);
        }
        public static NpcChat GetNpcChatpoint(NpcEntry npc, int chatpointId)
        {
            return npc.Chatpoints.First(o => o.Id == chatpointId);
        }

        public static int GetDefaultChatpoint(User user, NpcEntry npc)
        {
            if (Database.HasNpcStartpointSet(user.Id, npc.Id))
                return Database.GetNpcStartPoint(user.Id, npc.Id);
            else
                return 0;
        }

        public static void SetDefaultChatpoint(User user, NpcEntry npc, int chatpointId)
        {
            if (Database.HasNpcStartpointSet(user.Id, npc.Id))
                Database.SetNpcStartPoint(user.Id, npc.Id, chatpointId);
            else
                Database.AddNpcStartPoint(user.Id, npc.Id, chatpointId);
        }


        public static bool NpcExists(int id)
        {
            return NpcList.Any(o => o.Id == id);
        }
        public static NpcEntry GetNpcById(int id)
        {
            return NpcList.First(o => o.Id == id);
        }

        public static void WanderNpcs()
        {
            Logger.DebugPrint("Making NPC's randomly wander.");
            foreach(NpcEntry npc in NpcList)
            {
                npc.RandomWander();
            }
        }

        public static NpcEntry[] GetNpcsByXAndY(int x, int y)
        {
            return NpcList.Where(o => o.X == x && o.Y == y).ToArray();
        }

    }
}
