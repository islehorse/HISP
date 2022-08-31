using HISP.Game.Horse;
using HISP.Player;
using HISP.Security;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game.Services
{
    public class Auction
    {
        private static ThreadSafeList<Auction> auctionRooms = new ThreadSafeList<Auction>();
        public static Auction[] AuctionRooms
        {
            get
            {
                return auctionRooms.ToArray();
            }
        }

        public Auction(int id)
        {
            RoomId = id;
            auctionEntries = new ThreadSafeList<AuctionEntry>();
            Database.LoadAuctionRoom(this, RoomId);
        }

        public class AuctionBid
        {
            public const int MAX_BID = 2000000000;
            public User BidUser;
            public AuctionEntry AuctionItem;
            public int BidAmount;


            public void PlaceBid(int bidAmount)
            {
                if(BidUser.HorseInventory.HorseList.Length >= BidUser.MaxHorses)
                {
                    byte[] tooManyHorses = PacketBuilder.CreateChat(Messages.AuctionYouHaveTooManyHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    BidUser.LoggedinClient.SendPacket(tooManyHorses);
                    return;
                }

                string yourBidRaisedTo = Messages.FormatAuctionBidRaised(BidAmount, BidAmount + bidAmount);

                if(BidAmount >= MAX_BID)
                {
                    byte[] maxBidReached = PacketBuilder.CreateChat(Messages.AuctionBidMax, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    BidUser.LoggedinClient.SendPacket(maxBidReached);
                    return;
                }


                if (BidAmount + bidAmount > BidUser.Money && (AuctionItem.OwnerId != BidUser.Id))
                {

                    byte[] cantAffordBid = PacketBuilder.CreateChat(Messages.AuctionCantAffordBid, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    BidUser.LoggedinClient.SendPacket(cantAffordBid);
                    return;
                }


                BidAmount += bidAmount;
                if(BidAmount > MAX_BID) // no u
                {
                    yourBidRaisedTo = Messages.FormatAuctionBidRaised(BidAmount, MAX_BID);
                    BidAmount = MAX_BID;
                }
                
                if (BidAmount > AuctionItem.HighestBid)
                {

                    foreach(Auction room in AuctionRooms)
                    {
                        foreach(Auction.AuctionEntry entry in room.AuctionEntries)
                        {
                            if(entry.RandomId != AuctionItem.RandomId && entry.HighestBidder == BidUser.Id)
                            {
                                byte[] cantWinTooMuch = PacketBuilder.CreateChat(Messages.AuctionOnlyOneWinningBidAllowed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                BidUser.LoggedinClient.SendPacket(cantWinTooMuch);
                                return;
                            }
                        }
                    }

                    int oldBid = AuctionItem.HighestBid;
                    AuctionItem.HighestBid = BidAmount;
                    if(AuctionItem.HighestBidder != BidUser.Id && oldBid > 0)
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
            public AuctionEntry(int timeRemaining, int highestBid, int highestBidder, int randomId = -1)
            {
                RandomId = RandomID.NextRandomId(randomId);
                this.timeRemaining = timeRemaining;
                this.highestBid = highestBid;
                this.highestBidder = highestBidder;
            }

            public HorseInstance Horse;
            public int OwnerId;
            private ThreadSafeList<AuctionBid> bidders = new ThreadSafeList<AuctionBid>();
            public AuctionBid[] Bidders
            {
                get
                {
                    return bidders.ToArray();
                }
            }
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
                        Horse.Owner = highestBidder;
                        Horse.Hidden = false;

                        if(OwnerId == highestBidder)
                        {
                            if(GameServer.IsUserOnline(OwnerId))
                            {
                                User auctionRunner = GameServer.GetUserById(highestBidder);
                                auctionRunner.HorseInventory.UnHide(Horse.RandomId);
                                byte[] notSold = PacketBuilder.CreateChat(Messages.AuctionNoHorseBrought, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                auctionRunner.LoggedinClient.SendPacket(notSold);
                            }
                            return;
                        }


                        if(GameServer.IsUserOnline(highestBidder))
                        {
                            User userWon = GameServer.GetUserById(highestBidder);
                            byte[] wonAuction = PacketBuilder.CreateChat(Messages.FormatAuctionBroughtHorse(highestBid), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            userWon.LoggedinClient.SendPacket(wonAuction);
                            userWon.TakeMoney(highestBid);
                            userWon.HorseInventory.AddHorse(Horse, false);
                        }
                        else
                        {
                            Database.SetPlayerMoney(Database.GetPlayerMoney(highestBidder) - highestBid, highestBidder);
                        }

                        if(GameServer.IsUserOnline(OwnerId))
                        {
                            User userSold = GameServer.GetUserById(OwnerId);
                            byte[] horseSold = PacketBuilder.CreateChat(Messages.FormatAuctionHorseSold(highestBid), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            userSold.LoggedinClient.SendPacket(horseSold);
                            userSold.AddMoney(highestBid);
                            userSold.HorseInventory.DeleteHorse(Horse, false);
                        }
                        else
                        {
                            Database.SetPlayerMoney(Database.GetPlayerMoney(OwnerId) + highestBid, OwnerId);
                        }
                    }
                    Database.SetAuctionDone(RandomId, done);

                    foreach(AuctionBid bid in Bidders) // Cleanup some stuffs
                    {
                        if(bid.BidUser != null)
                            bid.BidUser.RemoveBid(bid);
                    }
                    bidders.Clear();
                }
            }
            public void AddBid(AuctionBid bid)
            {
                bidders.Add(bid);
            }
            public void Bid(User bidder, int bidAmount)
            {
                
                foreach(AuctionBid bid in bidder.Bids)
                {
                    if (bid.AuctionItem.RandomId == this.RandomId)
                    {
                        bid.PlaceBid(bidAmount);
                        auctionRoomPlacedIn.UpdateAuctionRoom();
                        return;
                    }
                }

                AuctionBid newBid = new AuctionBid();
                newBid.AuctionItem = this;
                newBid.BidUser = bidder;
                if (HighestBidder == bidder.Id)
                    newBid.BidAmount = HighestBid;
                else
                    newBid.BidAmount = 0;
                newBid.PlaceBid(bidAmount);
                bidder.AddBid(newBid);
                bidders.Add(newBid);
                auctionRoomPlacedIn.UpdateAuctionRoom();
            }

            public Auction auctionRoomPlacedIn;
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
            World.SpecialTile[] tiles = World.GetSpecialTilesByName("AUCTION-" + this.RoomId.ToString());
            foreach (World.SpecialTile tile in tiles)
            {
                GameServer.UpdateAreaForAll(tile.X, tile.Y, true);
            }
        }
        public void DeleteEntry(AuctionEntry entry)
        {
            Database.DeleteAuctionRoom(entry.RandomId);
            auctionEntries.Remove(entry);
        }

        public void AddExistingEntry(AuctionEntry entry)
        {
            auctionEntries.Add(entry);
        }
        public void AddEntry(AuctionEntry entry)
        {
            entry.auctionRoomPlacedIn = this;
            Database.AddAuctionRoom(entry, this.RoomId);
            auctionEntries.Add(entry);
        }

        private ThreadSafeList<AuctionEntry> auctionEntries;
        public int RoomId;

        public AuctionEntry[] AuctionEntries
        {
            get
            {
                return auctionEntries.ToArray();
            }
        }
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

        public bool HasUserPlacedAuctionAllready(User user)
        {
            foreach(AuctionEntry entry in AuctionEntries)
            {
                if (entry.OwnerId == user.Id)
                    return true; 
            }
            return false;
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
                        auctionRooms.Add(loadAuctionRoom);
                        Logger.InfoPrint("Loading Auction Room: " + code.ToString());
                    }
                }
            }
        }


    }
}
