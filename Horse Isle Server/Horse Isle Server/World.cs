using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{

    class World
    {

        public struct Time
        {
            public int minutes;
            public int hours;
            public int days;
            public int year;
        }
        public const int MINUTE = 4320;

        public static Time GetGameTime()
        {
            int epoch = Database.GetServerCreationTime();
            DateTime serverCreationTime = DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
            DateTime currentTime = DateTime.Now;

            TimeSpan difference = (currentTime.Date - currentTime.Date);

            int totalMilis = Convert.ToInt32(difference.TotalMilliseconds);

            
            int gameMinutes = totalMilis / MINUTE;
            int gameHours = (totalMilis / MINUTE * 600);
            int gameDays = (totalMilis / (MINUTE * 60) * 24);
            int gameYears = ((totalMilis / (MINUTE * 60) * 24)*365);

            Time time = new Time();
            time.days = gameDays;
            time.year = gameYears;
            time.minutes = gameMinutes;
            time.hours = gameHours;

            return time;
        }

        public static string GetWeather()
        {
            return Database.GetWorldWeather();
        }
    }
}
