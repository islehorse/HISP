using System.Threading;

namespace HISP.Game
{
    public class RandomID
    {
        private static Mutex rndmIdMutex = new Mutex();
        private static int prevId = 0;
        public static int NextRandomId(int randomId=-1)
        {
            lock(rndmIdMutex)
            {
                int rndmId = 0;

                if (randomId == -1)
                    rndmId = prevId + 1;
                else
                    rndmId = randomId;

                if (rndmId >= prevId)
                    prevId = rndmId;


                return rndmId;
            }
        }
    }
}
