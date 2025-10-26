using System.Collections.Generic;
using System.Linq;

namespace HISP.Game.Services
{
    public class Transport
    {
        public struct TransportLocation
        {
            public int Id;
            public int Cost;
            public int GotoX;
            public int GotoY;
            public string Type;
            public string LocationTitle;
        }

        public struct TransportPoint
        {
            public int X;
            public int Y;
            public int[] Locations;
        }

        public static List<TransportPoint> TransportPoints = new List<TransportPoint>();
        public static List<TransportLocation> TransportLocations = new List<TransportLocation>();


        public static TransportPoint GetTransportPoint(int x, int y)
        {
            return TransportPoints.First(o => (o.X == x && o.Y == y));
        }

        public static TransportLocation GetTransportLocationById(int id)
        {
            return TransportLocations.First(o => (o.Id == id));
        }

    }
}
