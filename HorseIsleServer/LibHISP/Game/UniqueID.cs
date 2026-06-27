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
                int newUniqueId = 0;

                if (uniqueId == -1)
                    newUniqueId = prevId + 1;
                else
                    newUniqueId = uniqueId;

                if (newUniqueId >= prevId)
                    prevId = newUniqueId;

                return newUniqueId;
            }
        }
    }
}
