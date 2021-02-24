using HISP.Game;
using HISP.Game.Horse;
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
