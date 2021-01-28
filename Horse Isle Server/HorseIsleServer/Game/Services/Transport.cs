using System.Collections.Generic;

namespace HISP.Game.Services
{
    class Transport
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
            foreach(TransportPoint transportPoint in TransportPoints)
            {
                if (transportPoint.X == x && transportPoint.Y == y)
                {
                    return transportPoint;
                }
            }
            throw new KeyNotFoundException("Cannot find transport point at x:" + x + "y:" + y);
        }

        public static TransportLocation GetTransportLocation(int id)
        {
            foreach (TransportLocation transportLocation in TransportLocations)
            {
                if (transportLocation.Id == id)
                {
                    return transportLocation;
                }
            }
            throw new KeyNotFoundException("Cannot find transport location at Id:" + id);
        }

    }
}
