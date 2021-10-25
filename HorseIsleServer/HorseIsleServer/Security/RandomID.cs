namespace HISP.Security
{
    public class RandomID
    {
        private static int prevId = 0;
        public static int NextRandomId(int randomId=-1)
        {
            int rndmId = 0;

            if (randomId == -1)
                rndmId = prevId+1;
            else
                rndmId = randomId;

            if (rndmId >= prevId)
                prevId = rndmId;

            return rndmId;
        }
    }
}
