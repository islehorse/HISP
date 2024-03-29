﻿using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Security;
using HISP.Server;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace HISP.Player
{
    public class Trade
    {
        public Trade(User me)
        {
            RandomId = RandomID.NextRandomId();
            Trader = me;
        }

        public int RandomId;
        public User Trader;
        public Trade OtherTrade;

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
            Trader.PendingTradeTo = 0;
            Trader.TradingWith = null;

            OtherTrade.Trader.PendingTradeTo = 0;
            OtherTrade.Trader.TradingWith = null;

            GameServer.UpdateArea(Trader.Client);
            GameServer.UpdateArea(OtherTrade.Trader.Client);
        }

        public bool Fail = false;
        public void CompleteTrade()
        {
            
            /*
             *  Money Checks
             */

            // Check if other player has no money
            if (MoneyOffered > 0 && OtherTrade.Trader.Money < 0)
            {
                byte[] otherNegativeMoneyNotAllowed = PacketBuilder.CreateChat(Messages.TradeOtherPlayerHasNegativeMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(otherNegativeMoneyNotAllowed);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if current player has no money
            if (OtherTrade.MoneyOffered > 0 && Trader.Money < 0)
            {
                byte[] negativeMoneyNotAllowed = PacketBuilder.CreateChat(Messages.TradeYouHaveNegativeMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(negativeMoneyNotAllowed);
                Fail = true;
                OtherTrade.Fail = true;
            }
            
            // Check if other player has any bids
            if (OtherTrade.Trader.Bids.Length > 0)
            {
                byte[] tradeNotAllowedWhileOtherBidding = PacketBuilder.CreateChat(Messages.TradeNotAllowedWhileOtherBidding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeNotAllowedWhileOtherBidding);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // check if current player has bids'
            if (Trader.Bids.Length > 0)
            {
                byte[] tradeNotAllowedWhileBidding = PacketBuilder.CreateChat(Messages.TradeNotAllowedWhileBidding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeNotAllowedWhileBidding);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if other player has max money
            if (MoneyOffered > 0 && OtherTrade.Trader.Money + MoneyOffered > 2100000000)
            {
                byte[] tradeOtherHasTooMuchMoney = PacketBuilder.CreateChat(Messages.TradeWillGiveOtherTooMuchMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeOtherHasTooMuchMoney);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if you have no money
            if (OtherTrade.MoneyOffered > 0 && Trader.Money + OtherTrade.MoneyOffered > 2100000000)
            {
                byte[] tradeYouHasTooMuchMoney = PacketBuilder.CreateChat(Messages.TradeWillGiveYouTooMuchMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeYouHasTooMuchMoney);
                Fail = true;
                OtherTrade.Fail = true;
            }


            /*
             *  Horse Checks
             */

            // Check if other player has max horses
            if (HorsesOffered.Length > 0 && OtherTrade.Trader.HorseInventory.HorseList.Length + HorsesOffered.Length > OtherTrade.Trader.MaxHorses)
            {
                byte[] tradeYouHaveTooManyHorses = PacketBuilder.CreateChat(Messages.TradeYouCantHandleMoreHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeYouHaveTooManyHorses);
                Fail = true;
                OtherTrade.Fail = true;
            }

            // Check if current player has max horses
            if (OtherTrade.HorsesOffered.Length > 0 && Trader.HorseInventory.HorseList.Length + OtherTrade.HorsesOffered.Length > Trader.MaxHorses)
            {
                byte[] tradeYouHaveTooManyHorses = PacketBuilder.CreateChat(Messages.TradeYouCantHandleMoreHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeYouHaveTooManyHorses);
                Fail = true;
                OtherTrade.Fail = true;
            }

            /*
            *      Item Checks
            */

            if (OtherTrade.ItemsOffered.Length > 0)
            {
                foreach (ItemInstance[] inst in OtherTrade.ItemsOffered)
                {
                    if (Trader.Inventory.HasItemId(inst[0].ItemId))
                    {
                        InventoryItem items = Trader.Inventory.GetItemByItemId(inst[0].ItemId);
                        if (items.ItemInstances.Length + inst.Length > Item.MAX_STACK)
                        {
                            byte[] tradeTooManyItems = PacketBuilder.CreateChat(Messages.TradeYouCantCarryMoreItems, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            Trader.Client.SendPacket(tradeTooManyItems);
                            Fail = true;
                            OtherTrade.Fail = true;
                        }
                    }
                }
            }

            if (ItemsOffered.Length > 0)
            {
                foreach (ItemInstance[] inst in ItemsOffered)
                {
                    if (OtherTrade.Trader.Inventory.HasItemId(inst[0].ItemId))
                    {
                        InventoryItem items = OtherTrade.Trader.Inventory.GetItemByItemId(inst[0].ItemId);
                        if (items.ItemInstances.Length + inst.Length > Item.MAX_STACK)
                        {
                            byte[] tradeTooManyItems = PacketBuilder.CreateChat(Messages.TradeOtherCantCarryMoreItems, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            Trader.Client.SendPacket(tradeTooManyItems);
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

            
            acceptTrade:;
            byte[] tradeAccepted = PacketBuilder.CreateChat(Messages.TradeAcceptedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Trader.Client.SendPacket(tradeAccepted);

            // Transfer Money

            if (MoneyOffered > 0) 
            {
                Trader.TakeMoney(MoneyOffered);
                byte[] tradeSpentMoney = PacketBuilder.CreateChat(Messages.FormatTradeYouSpent(MoneyOffered), PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeSpentMoney);
            }

            if(OtherTrade.MoneyOffered > 0)
            {
                Trader.AddMoney(OtherTrade.MoneyOffered);
                byte[] tradeReceivedMoney = PacketBuilder.CreateChat(Messages.FormatTradeYouReceived(OtherTrade.MoneyOffered), PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.Client.SendPacket(tradeReceivedMoney);
            }

            foreach (HorseInstance inst in HorsesOffered) // Transfer Horses
            {
                inst.Owner = OtherTrade.Trader.Id;

                // Dismount horse if its traded
                if (Trader.CurrentlyRidingHorse != null)
                {
                    if (Trader.CurrentlyRidingHorse.RandomId == inst.RandomId)
                    {
                        byte[] disMounted = PacketBuilder.CreateChat(Messages.TradeRiddenHorse, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        Trader.Facing %= 5;
                        Trader.CurrentlyRidingHorse = null;
                        Trader.Client.SendPacket(disMounted);
                    }
                }

                Trader.HorseInventory.DeleteHorse(inst, false);
                OtherTrade.Trader.HorseInventory.AddHorse(inst, false);
            }

            foreach (ItemInstance[] item in ItemsOffered) // Transfer Items
            {
                foreach (ItemInstance itm in item)
                {
                    Trader.Inventory.Remove(itm);
                    OtherTrade.Trader.Inventory.AddIgnoringFull(itm);
                }
            }


            Trader.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Trading).Count++;


            if (Trader.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Trading).Count >= 100)
                Trader.Awards.AddAward(Award.GetAwardById(29)); // Trader
            if (Trader.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Trading).Count >= 1000)
                Trader.Awards.AddAward(Award.GetAwardById(50)); // Pro Trader

            endTrade();
            return;

            cancelTrade:;
            InteruptTrade();
            return;

        }
        public void InteruptTrade()
        {
            byte[] tradeCanceled = PacketBuilder.CreateChat(Messages.TradeCanceledInterupted, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Trader.Client.SendPacket(tradeCanceled);
            endTrade();
        }
        public void AcceptTrade()
        {
            byte[] waitingForAccept = PacketBuilder.CreateChat(Messages.TradeWaitingForOthersToAcceptMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Trader.Client.SendPacket(waitingForAccept);

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
            Trader.Client.SendPacket(tradeCanceled);

            byte[] tradeCanceledOther = PacketBuilder.CreateChat(Messages.FormatTradeCanceledByPlayer(Trader.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            OtherTrade.Trader.Client.SendPacket(tradeCanceledOther);

            endTrade();
        }
        public void CancelTradeMoved()
        {
            byte[] playerMoved = PacketBuilder.CreateChat(Messages.TradeCanceledBecuasePlayerMovedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Trader.Client.SendPacket(playerMoved);
            OtherTrade.Trader.Client.SendPacket(playerMoved);

            endTrade();
        }
        
    }
}
