using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Player
{
    public class Trade
    {
        public Trade(User me)
        {
            UniqueId = UniqueID.NextUniqueId();
            Tradee = me;
        }

        public int UniqueId;
        public User Tradee;
        public Trade OtherTrade;

        public bool Fail = false;
        public string Stage = "OPEN";
        public int MoneyOffered = 0;
        private List<HorseInstance> horsesOffered = new List<HorseInstance>();
        private List<ItemInstance[]> itemsOffered = new List<ItemInstance[]>();
        public ItemInstance[][] ItemsOffered
        {
            get
            {
                return itemsOffered.ToArray();
            }
        }
        public HorseInstance[] HorsesOffered
        {
            get
            {
                return horsesOffered.ToArray();
            }
        }
        public void RemoveOfferedHorse(HorseInstance horse)
        {
            horsesOffered.Remove(horse);
        }
        public void OfferHorse(HorseInstance horse)
        {
            horsesOffered.Add(horse);
        }

        public void OfferItems(ItemInstance[] items)
        {
            itemsOffered.Add(items);
        }
        public void RemoveOfferedItems(ItemInstance[] items)
        {
            itemsOffered.Remove(items);
        }
        private void endTrade()
        {
            Tradee.PendingTradeTo = 0;
            Tradee.CurrentTrade = null;

            OtherTrade.Tradee.PendingTradeTo = 0;
            OtherTrade.Tradee.CurrentTrade = null;

            GameServer.UpdateArea(Tradee.Client);
            GameServer.UpdateArea(OtherTrade.Tradee.Client);
        }

        public void CompleteTrade()
        {
            
            /*
             *  Money Checks
             */

            // Check if other player has no money
            if (MoneyOffered > 0 && OtherTrade.Tradee.Money < 0)
            {
                byte[] otherNegativeMoneyNotAllowed = PacketBuilder.CreateChat(Messages.TradeOtherPlayerHasNegativeMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(otherNegativeMoneyNotAllowed);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if current player has no money
            if (OtherTrade.MoneyOffered > 0 && Tradee.Money < 0)
            {
                byte[] negativeMoneyNotAllowed = PacketBuilder.CreateChat(Messages.TradeYouHaveNegativeMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(negativeMoneyNotAllowed);
                Fail = true;
                OtherTrade.Fail = true;
            }
            
            // Check if other player has any active bids
            if (OtherTrade.Tradee.Bids.Length > 0)
            {
                byte[] tradeNotAllowedWhileOtherBidding = PacketBuilder.CreateChat(Messages.TradeNotAllowedWhileOtherBidding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeNotAllowedWhileOtherBidding);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // check if current player has active bids
            if (Tradee.Bids.Length > 0)
            {
                byte[] tradeNotAllowedWhileBidding = PacketBuilder.CreateChat(Messages.TradeNotAllowedWhileBidding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeNotAllowedWhileBidding);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if other player has max money
            if (MoneyOffered > 0 && OtherTrade.Tradee.Money + MoneyOffered > 2100000000)
            {
                byte[] tradeOtherHasTooMuchMoney = PacketBuilder.CreateChat(Messages.TradeWillGiveOtherTooMuchMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeOtherHasTooMuchMoney);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if you have no money
            if (OtherTrade.MoneyOffered > 0 && Tradee.Money + OtherTrade.MoneyOffered > 2100000000)
            {
                byte[] tradeYouHasTooMuchMoney = PacketBuilder.CreateChat(Messages.TradeWillGiveYouTooMuchMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeYouHasTooMuchMoney);
                Fail = true;
                OtherTrade.Fail = true;
            }


            /*
             *  Horse Checks
             */

            // Check if other player has max horses
            if (HorsesOffered.Length > 0 && OtherTrade.Tradee.HorseInventory.HorseList.Length + HorsesOffered.Length > OtherTrade.Tradee.MaxHorses)
            {
                byte[] tradeYouHaveTooManyHorses = PacketBuilder.CreateChat(Messages.TradeYouCantHandleMoreHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeYouHaveTooManyHorses);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if current player has max horses
            if (OtherTrade.HorsesOffered.Length > 0 && Tradee.HorseInventory.HorseList.Length + OtherTrade.HorsesOffered.Length > Tradee.MaxHorses)
            {
                byte[] tradeYouHaveTooManyHorses = PacketBuilder.CreateChat(Messages.TradeYouCantHandleMoreHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeYouHaveTooManyHorses);
                Fail = true;
                OtherTrade.Fail = true;
            }

            /*
            *      Item Checks
            */

            // fail if you would get more than the maximum amount of an item.
            if (OtherTrade.ItemsOffered.Length > 0)
            {
                foreach (ItemInstance[] inst in OtherTrade.ItemsOffered)
                {
                    if (Tradee.Inventory.HasItemId(inst[0].ItemId))
                    {
                        InventoryItem items = Tradee.Inventory.GetItemByItemId(inst[0].ItemId);
                        if (items.ItemInstances.Length + inst.Length > Item.MAX_STACK)
                        {
                            byte[] tradeTooManyItems = PacketBuilder.CreateChat(Messages.TradeYouCantCarryMoreItems, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            Tradee.Client.SendPacket(tradeTooManyItems);
                            Fail = true;
                            OtherTrade.Fail = true;
                        }
                    }
                }
            }

            // fail if other player would get more than the maximum amount of an item.
            if (ItemsOffered.Length > 0)
            {
                foreach (ItemInstance[] inst in ItemsOffered)
                {
                    if (OtherTrade.Tradee.Inventory.HasItemId(inst[0].ItemId))
                    {
                        InventoryItem items = OtherTrade.Tradee.Inventory.GetItemByItemId(inst[0].ItemId);
                        if (items.ItemInstances.Length + inst.Length > Item.MAX_STACK)
                        {
                            byte[] tradeTooManyItems = PacketBuilder.CreateChat(Messages.TradeOtherCantCarryMoreItems, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            Tradee.Client.SendPacket(tradeTooManyItems);
                            Fail = true;
                            OtherTrade.Fail = true;
                        }
                    }
                }
            }


            if (this.Fail)
                goto cancelTrade;
            else
                goto acceptTrade;

            
            acceptTrade:
            byte[] tradeAccepted = PacketBuilder.CreateChat(Messages.TradeAcceptedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Tradee.Client.SendPacket(tradeAccepted);

            // Transfer Money
            

            // if you are offering money to the other player, delete it from your balance
            if (MoneyOffered > 0)
            {
                Tradee.TakeMoney(MoneyOffered);
                byte[] tradeSpentMoney = PacketBuilder.CreateChat(Messages.FormatTradeYouSpent(MoneyOffered), PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeSpentMoney);
            }

            // if you are receiving money, add it to your balance.
            if (OtherTrade.MoneyOffered > 0)
            {
                Tradee.AddMoney(OtherTrade.MoneyOffered);
                byte[] tradeReceivedMoney = PacketBuilder.CreateChat(Messages.FormatTradeYouReceived(OtherTrade.MoneyOffered), PacketBuilder.CHAT_BOTTOM_RIGHT);
                Tradee.Client.SendPacket(tradeReceivedMoney);
            }

            foreach (HorseInstance inst in HorsesOffered) // Transfer Horses
            {
                inst.Owner = OtherTrade.Tradee.Id;

                // Dismount horse if its traded
                if (Tradee.CurrentlyRidingHorse != null)
                {
                    if (Tradee.CurrentlyRidingHorse.UniqueId == inst.UniqueId)
                    {
                        byte[] dismountedPacket = PacketBuilder.CreateChat(Messages.TradeRiddenHorse, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        Tradee.Facing %= 5;
                        Tradee.CurrentlyRidingHorse = null;
                        Tradee.Client.SendPacket(dismountedPacket);
                    }
                }

                Tradee.HorseInventory.DeleteHorse(inst, false);
                OtherTrade.Tradee.HorseInventory.AddHorse(inst, false);
            }

            foreach (ItemInstance[] item in ItemsOffered) // Transfer Items
            {
                foreach (ItemInstance itm in item)
                {
                    Tradee.Inventory.Remove(itm);
                    OtherTrade.Tradee.Inventory.AddIgnoringFull(itm);
                }
            }


            Tradee.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Trading).Count++;


            if (Tradee.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Trading).Count >= 100)
                Tradee.Awards.Add(Award.GetAwardById(Award.AWARD_TRADER)); // Trader
            if (Tradee.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Trading).Count >= 1000)
                Tradee.Awards.Add(Award.GetAwardById(Award.AWARD_PRO_TRADER)); // Pro Trader

            endTrade();
            return;

            cancelTrade:
            InteruptTrade();
            return;

        }
        public void InteruptTrade()
        {
            byte[] tradeCanceled = PacketBuilder.CreateChat(Messages.TradeCanceledInterupted, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Tradee.Client.SendPacket(tradeCanceled);
            endTrade();
        }
        public void AcceptTrade()
        {
            byte[] waitingForAccept = PacketBuilder.CreateChat(Messages.TradeWaitingForOthersToAcceptMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Tradee.Client.SendPacket(waitingForAccept);

            if (OtherTrade.Stage == "ACCEPTED")
            {
                CompleteTrade();
                OtherTrade.CompleteTrade();
            }

            Stage = "ACCEPTED";
        }
        public void CancelTrade()
        {
            byte[] tradeCanceled = PacketBuilder.CreateChat(Messages.TradeCanceledByYouMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Tradee.Client.SendPacket(tradeCanceled);

            byte[] tradeCanceledOther = PacketBuilder.CreateChat(Messages.FormatTradeCanceledByPlayer(Tradee.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            OtherTrade.Tradee.Client.SendPacket(tradeCanceledOther);

            endTrade();
        }
        public void CancelTradeMoved()
        {
            byte[] playerMoved = PacketBuilder.CreateChat(Messages.TradeCanceledBecuasePlayerMovedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Tradee.Client.SendPacket(playerMoved);
            OtherTrade.Tradee.Client.SendPacket(playerMoved);

            endTrade();
        }
        
    }
}
