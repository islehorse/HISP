using HISP.Game.Horse;
using HISP.Security;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game.Services
{
    public class Auction
    {
        public static List<Auction> AuctionRooms = new List<Auction>();
        public Auction(int id)
        {
            RoomId = id;
            AuctionEntries = new List<AuctionEntry>();
            Database.LoadAuctionRoom(this, RoomId);
        }
        public class AuctionEntry
        {
            public AuctionEntry(int timeRemaining, int highestBid, int highestBidder, int randomId=-1)
            {
                RandomId = RandomID.NextRandomId(randomId);
                this.timeRemaining = timeRemaining;
                this.highestBid = highestBid;
                this.highestBidder = highestBidder;
            }

            public HorseInstance Horse;
            public int OwnerId;

            public int RandomId;
            private int timeRemaining;

            private int highestBid;
            private int highestBidder;

            public int TimeRemaining
            {
                get
                {
                    return timeRemaining;
                }
                set
                {
                    timeRemaining = value;
                    Database.SetAuctionTimeout(RandomId, value);
                }
            }
            public int HighestBid
            {
                get
                {
                    return highestBid;
                }
                set
                {
                    highestBid = value;
                    Database.SetAuctionHighestBid(RandomId, value);
                }
            }

            public int HighestBidder
            {
                get
                {
                    return highestBidder;
                }
                set
                {
                    highestBidder = value;
                    Database.SetAuctionHighestBidder(RandomId, value);
                }
            }


        }

        public void AddEntry(AuctionEntry entry)
        {
            Database.AddAuctionRoom(entry, this.RoomId);
            AuctionEntries.Add(entry);
        }

        public List<AuctionEntry> AuctionEntries;
        public int RoomId;


        public bool HasAuctionEntry(int randomId)
        {
            foreach (AuctionEntry entry in AuctionEntries)
            {
                if (entry.RandomId == randomId)
                {
                    return true;
                }
            }
            return false;
        }
        public AuctionEntry GetAuctionEntry(int randomId)
        {
            foreach(AuctionEntry entry in AuctionEntries)
            {
                if(entry.RandomId == randomId)
                {
                    return entry;
                }
            }
            throw new KeyNotFoundException("Auction Entry with RandomID: " + randomId + " NOT FOUND");
        }

        public static Auction GetAuctionRoomById(int roomId)
        {
            foreach(Auction auction in AuctionRooms)
            {
                if(auction.RoomId == roomId)
                {
                    return auction;
                }
            }
            throw new KeyNotFoundException("Auction with roomID " + roomId + " NOT FOUND!");
        }

        public static void LoadAllAuctionRooms()
        {
            foreach(World.SpecialTile tile in World.SpecialTiles)
            {
                if(tile.Code != null)
                {
                    if(tile.Code.StartsWith("AUCTION-"))
                    {
                        int code = int.Parse(tile.Code.Split('-')[1]);
                        Auction loadAuctionRoom = new Auction(code);
                        AuctionRooms.Add(loadAuctionRoom);
                        Logger.InfoPrint("Loading Auction Room: " + code.ToString());
                    }
                }
            }
        }


    }
}
