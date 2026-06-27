using System.Threading;

namespace HISP.Game
{
    public class UniqueID
    {
        private static Mutex uniqueIdMutex = new Mutex();
        private static int prevId = 0;
        public static int NextUniqueId(int uniqueId=-1)
        {
            lock(uniqueIdMutex)
            {
                int rndmId = 0;

                if (uniqueId == -1)
                    rndmId = prevId + 1;
                else
                    rndmId = uniqueId;

                if (rndmId >= prevId)
                    prevId = rndmId;

                return rndmId;
            }
        }
    }
}
