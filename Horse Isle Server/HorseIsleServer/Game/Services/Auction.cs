using HISP.Game.Horse;
using HISP.Player;
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

        public class AuctionBid
        {
            public User BidUser;
            public AuctionEntry AuctionItem;
            public int BidAmount;

            public void PlaceBid(int bidAmount)
            {
                string yourBidRaisedTo = Messages.FormatAuctionBidRaised(BidAmount, BidAmount + bidAmount);

                BidAmount += bidAmount;
                if (BidAmount > AuctionItem.HighestBid)
                {
                    AuctionItem.HighestBid += BidAmount;
                    if(AuctionItem.HighestBidder != BidUser.Id)
                    {
                        if (GameServer.IsUserOnline(AuctionItem.HighestBidder))
                        {
                            User oldBidder = GameServer.GetUserById(AuctionItem.HighestBidder);
                            byte[] outbidMessage = PacketBuilder.CreateChat(Messages.FormatAuctionYourOutbidBy(BidUser.Username, AuctionItem.HighestBid), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            oldBidder.LoggedinClient.SendPacket(outbidMessage);
                        }
                    }

                    AuctionItem.HighestBidder = BidUser.Id;
                    yourBidRaisedTo += Messages.AuctionTopBid;

                }
                else
                {
                    yourBidRaisedTo += Messages.AuctionExistingBidHigher;
                }

                byte[] bidPlacedMsg = PacketBuilder.CreateChat(yourBidRaisedTo, PacketBuilder.CHAT_BOTTOM_RIGHT);
                BidUser.LoggedinClient.SendPacket(bidPlacedMsg);
            }
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
        
            public bool Completed 
            {
                get
                {
                    return done;
                }
                set
                {
                    done = value;
                    if(done)
                    {
                        Horse.Owner = HighestBidder;
                        Horse.Hidden = false;

                        if(GameServer.IsUserOnline(highestBidder))
                        {
                            User userWon = GameServer.GetUserById(highestBidder);
                            byte[] wonAuction = PacketBuilder.CreateChat(Messages.FormatAuctionBroughtHorse(highestBid), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            userWon.LoggedinClient.SendPacket(wonAuction);
                            userWon.Money -= highestBid;
                        }
                        else
                        {
                            Database.SetPlayerMoney(Database.GetPlayerMoney(highestBidder) - highestBid, highestBidder);
                        }
                        if(GameServer.IsUserOnline(OwnerId))
                        {
                            User userSold = GameServer.GetUserById(highestBidder);
                            byte[] horseSold = PacketBuilder.CreateChat(Messages.FormatAuctionHorseSold(highestBid), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            userSold.LoggedinClient.SendPacket(horseSold);
                            userSold.Money += highestBid;
                        }
                        else
                        {
                            Database.SetPlayerMoney(Database.GetPlayerMoney(OwnerId) + highestBid, OwnerId);
                        }
                    }
                    Database.SetAuctionDone(RandomId, done);
                }
            }

            public void Bid(User bidder, int bidAmount)
            {
                
                foreach(AuctionBid bid in bidder.Bids)
                {
                    if (bid.AuctionItem.RandomId == this.RandomId)
                    {
                        bid.PlaceBid(bidAmount);
                        return;
                    }
                }

                AuctionBid newBid = new AuctionBid();
                newBid.AuctionItem = this;
                newBid.BidUser = bidder;
                newBid.BidAmount = 0;
                newBid.PlaceBid(bidAmount);
                bidder.Bids.Add(newBid);
            }

            public int RandomId;
            private int timeRemaining;
            private bool done;
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

        public void UpdateAuctionRoom()
        {
            foreach (World.SpecialTile tile in World.SpecialTiles)
            {
                if (tile.Code != null)
                {
                    if (tile.Code.StartsWith("AUCTION-"))
                    {
                        int id = int.Parse(tile.Code.Split('-')[1]);
                        if (id == this.RoomId)
                        {
                            GameServer.UpdateAreaForAll(tile.X, tile.Y);
                        }
                    }
                }
            }
        }
        public void DeleteEntry(AuctionEntry entry)
        {
            Database.DeleteAuctionRoom(entry.RandomId);
            AuctionEntries.Remove(entry);
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
