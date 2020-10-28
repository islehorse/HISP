using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class RandomID
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
