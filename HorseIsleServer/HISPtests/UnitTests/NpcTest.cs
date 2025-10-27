using HISP.Game;
using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Tests.UnitTests
{
    public class NpcTest
    {
        public static bool Test(string testName, object value, object valueComp, bool needsEqual = true)
        {
            bool result = value.Equals(valueComp);

            if (result == needsEqual)
                ResultLogger.LogTestStatus(true, "NPC_TEST " + testName, "Success.");
            else
                ResultLogger.LogTestResult(false, "NPC_TEST " + testName, value.ToString(), valueComp.ToString());

            return result;
        }
        public static bool RunNpcTest()
        {
            List<bool> results = new List<bool>();

            Npc.NpcEntry wanderingNpc = Npc.NpcList.First(o => o.Moves);
            int oldX = wanderingNpc.X;

            for(int i = 0; i < 30; i++)
                Npc.WanderNpcs();
            
            results.Add(Test("WanderNPC", oldX, wanderingNpc.X, false));

            Npc.NpcEntry npc = Npc.GetNpcById(430);
            results.Add(Test("GetNpcById", npc.Id, 430));

            Npc.NpcChat chat = npc.Chatpoints.First();
            results.Add(Test("GetNpcChatpoint", Npc.GetNpcChatpoint(npc, chat.Id), chat));

            Npc.NpcReply reply = chat.Replies.First();
            results.Add(Test("GetNpcReply", Npc.GetNpcReply(npc, reply.Id).Id, reply.Id));
            results.Add(Test("GetNpcReply2", Npc.GetNpcReply(chat, reply.Id).Id, reply.Id));
            
            results.Add(Test("GetNpcsByXAndY", Npc.GetNpcsByXAndY(npc.X, npc.Y).First().Id, npc.Id));

            foreach (bool result in results)
            {
                if (result == false)
                    return false;
            }
            return true;
        }
    }
}
