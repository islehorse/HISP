using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class World
    {

        public static int ServerStartTime;
        public const int MINUTE = 4320;

        public static int GetGameDay()
        {
            int epoch = Database.GetServerCreationTime();
            DateTime serverCreationTime = DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
            DateTime currentTime = DateTime.Now;

            TimeSpan difference = (currentTime.Date - currentTime.Date);

            Int64 totalMilis = Convert.ToInt32(difference.TotalMilliseconds);

            
            Int64 gameMinutes = totalMilis / 4320;


            return;
        }
    }
}
