using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Security;
using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<HorseInstance> HorsesOffered = new List<HorseInstance>();
        public List<ItemInstance[]> ItemsOffered = new List<ItemInstance[]>();

        private void endTrade()
        {
            Trader.PendingTradeTo = 0;
            Trader.TradingWith = null;

            OtherTrade.Trader.PendingTradeTo = 0;
            OtherTrade.Trader.TradingWith = null;

            GameServer.UpdateArea(Trader.LoggedinClient);
            GameServer.UpdateArea(OtherTrade.Trader.LoggedinClient);
        }

        public void CompleteTrade()
        {
            bool fail = false;
            // Check hell
            if (OtherTrade.Trader.Money < 0)
            {
                byte[] otherNegativeMoneyNotAllowed = PacketBuilder.CreateChat(Messages.TradeOtherPlayerHasNegativeMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(otherNegativeMoneyNotAllowed);
                fail = true;
            }
            if (Trader.Money < 0)
            {
                byte[] negativeMoneyNotAllowed = PacketBuilder.CreateChat(Messages.TradeYouHaveNegativeMoney, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(negativeMoneyNotAllowed);
                fail = true;
            }
            if (OtherTrade.Trader.Bids.Count > 0)
            {
                byte[] tradeNotAllowedWhileOtherBidding = PacketBuilder.CreateChat(Messages.TradeNotAllowedWhileOtherBidding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeNotAllowedWhileOtherBidding);
                fail = true;
            }
            if (Trader.Bids.Count > 0)
            {
                byte[] tradeNotAllowedWhileBidding = PacketBuilder.CreateChat(Messages.TradeNotAllowedWhileBidding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeNotAllowedWhileBidding);
                fail = true;
            }
            if (OtherTrade.Trader.HorseInventory.HorseList.Length > OtherTrade.Trader.MaxHorses)
            {
                byte[] tradeYouHaveTooManyHorses = PacketBuilder.CreateChat(Messages.TradeYouCantHandleMoreHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeYouHaveTooManyHorses);
                fail = true;
            }
            if (Trader.HorseInventory.HorseList.Length > Trader.MaxHorses)
            {
                byte[] tradeYouHaveTooManyHorses = PacketBuilder.CreateChat(Messages.TradeYouCantHandleMoreHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeYouHaveTooManyHorses);
                fail = true;
            }


            /*
            *      Item Checks
            */

            bool itemYouFail = false;
            foreach (ItemInstance[] inst in OtherTrade.ItemsOffered)
            {
                if (Trader.Inventory.HasItemId(inst[0].ItemId))
                {
                    InventoryItem items = Trader.Inventory.GetItemByItemId(inst[0].ItemId);
                    if (items.ItemInstances.Count + inst.Length >= ConfigReader.MAX_STACK)
                    {
                        itemYouFail = true;
                    }
                }
            }

            if (itemYouFail)
            {
                fail = true;
                byte[] tradeTooManyItems = PacketBuilder.CreateChat(Messages.TradeYouCantCarryMoreItems, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeTooManyItems);
            }

            bool itemOtherFail = false;
            foreach (ItemInstance[] inst in ItemsOffered)
            {
                if (OtherTrade.Trader.Inventory.HasItemId(inst[0].ItemId))
                {
                    InventoryItem items = OtherTrade.Trader.Inventory.GetItemByItemId(inst[0].ItemId);
                    if (items.ItemInstances.Count + inst.Length >= ConfigReader.MAX_STACK)
                    {
                        itemOtherFail = true;
                    }
                }
            }

            if (itemOtherFail)
            {
                fail = true;
                byte[] tradeTooManyItems = PacketBuilder.CreateChat(Messages.TradeOtherCantCarryMoreItems, PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeTooManyItems);
            }

            if (fail)
                goto cancelTrade;
            else
                goto acceptTrade;


            acceptTrade:;
            byte[] tradeAccepted = PacketBuilder.CreateChat(Messages.TradeAcceptedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Trader.LoggedinClient.SendPacket(tradeAccepted);

            if (MoneyOffered > 0) // Transfer Money
            {
                Trader.Money -= MoneyOffered;
                byte[] tradeSpentMoney = PacketBuilder.CreateChat(Messages.FormatTradeYouSpent(MoneyOffered), PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeSpentMoney);
            }

            if(OtherTrade.MoneyOffered > 0)
            {
                Trader.Money += OtherTrade.MoneyOffered;
                byte[] tradeReceivedMoney = PacketBuilder.CreateChat(Messages.FormatTradeYouReceived(OtherTrade.MoneyOffered), PacketBuilder.CHAT_BOTTOM_RIGHT);
                Trader.LoggedinClient.SendPacket(tradeReceivedMoney);
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
                        Trader.LoggedinClient.SendPacket(disMounted);
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
                    OtherTrade.Trader.Inventory.Add(itm);
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
            Trader.LoggedinClient.SendPacket(tradeCanceled);
            endTrade();
        }
        public void AcceptTrade()
        {
            byte[] waitingForAccept = PacketBuilder.CreateChat(Messages.TradeWaitingForOthersToAcceptMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Trader.LoggedinClient.SendPacket(waitingForAccept);

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
            Trader.LoggedinClient.SendPacket(tradeCanceled);

            byte[] tradeCanceledOther = PacketBuilder.CreateChat(Messages.FormatTradeCanceledByPlayer(Trader.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            OtherTrade.Trader.LoggedinClient.SendPacket(tradeCanceledOther);

            endTrade();
        }
        public void CancelTradeMoved()
        {
            byte[] playerMoved = PacketBuilder.CreateChat(Messages.TradeCanceledBecuasePlayerMovedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            Trader.LoggedinClient.SendPacket(playerMoved);
            OtherTrade.Trader.LoggedinClient.SendPacket(playerMoved);

            endTrade();
        }
        
    }
}
