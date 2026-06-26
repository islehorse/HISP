using System.Threading;

namespace HISP.Game
{
    public class UniqueId
    {
        private static Mutex uniqueIdMutex = new Mutex();
        private static int prevId = 0;
        public static int NextUniqueId(int curUniqueId=-1)
        {
            int uniqueId = 0;
            uniqueIdMutex.WaitOne();
            
            if (curUniqueId == -1)
                uniqueId = prevId+1;
            else
                uniqueId = curUniqueId;
            
            if (uniqueId >= prevId)
                prevId = uniqueId;

            uniqueIdMutex.ReleaseMutex();
            return uniqueId;
        }
    }
}
