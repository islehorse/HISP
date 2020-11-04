using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game
{
    class Npc
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
        public struct NpcEntry
        {
            public int Id;
            public string Name;
            public string AdminDescription;
            public string ShortDescription;
            public string LongDescription;
            public bool Moves;
            public int X;
            public int Y;
            public string StayOn;
            public int RequiresQuestIdCompleted;
            public int RequiresQuestIdNotCompleted;
            public string UDLRScript;
            public int UDLRStartX;
            public int UDLRStartY;
            public bool AdminOnly;
            public bool LibarySearchable;
            public int IconId;

            public NpcChat[] Chatpoints;
        }

        public static List<NpcEntry> NpcList = new List<NpcEntry>();

        public static NpcReply GetNpcReply(NpcEntry npc, int id)
        {

            foreach (NpcChat chatpoint in npc.Chatpoints)
            {
                foreach (NpcReply reply in chatpoint.Replies)
                {
                    if (reply.Id == id)
                        return reply;
                }
            }
            throw new KeyNotFoundException("Npc reply with " + id + " not found!");
        }

        public static NpcReply GetNpcReply(NpcChat chatpoint, int id)
        {
            foreach(NpcReply reply in chatpoint.Replies)
            {
                if (reply.Id == id)
                    return reply;
            }
            throw new KeyNotFoundException("Npc reply with " + id + " not found!");
        }
        public static NpcChat GetNpcChatpoint(NpcEntry npc, int chatpointId)
        {
            foreach(Npc.NpcChat chatpoint in npc.Chatpoints)
            {
                if(chatpoint.Id == chatpointId)
                {
                    return chatpoint;
                }
            }

            throw new KeyNotFoundException("Npc chatpoint id: " + chatpointId + " not found!");
        }
        public static bool NpcExists(int id)
        {
            try
            {
                GetNpcById(id);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
        public static NpcEntry GetNpcById(int id)
        {
            foreach(NpcEntry npc in NpcList)
            {
                if (npc.Id == id)
                    return npc;
            }
            throw new KeyNotFoundException("Npc id: " + id + " not found!");
        }

        public static NpcEntry[] GetNpcByXAndY(int x, int y)
        {
            List<NpcEntry> npcs = new List<NpcEntry>();

            foreach(NpcEntry npc in NpcList)
            {
                if (npc.X == x && npc.Y == y)
                    npcs.Add(npc);
            }
            return npcs.ToArray();
        }

    }
}
