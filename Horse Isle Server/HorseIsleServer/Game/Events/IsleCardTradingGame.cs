using HISP.Game.Items;
using HISP.Server;
using System.Threading;

namespace HISP.Game.Events
{
    public class IsleCardTradingGame
    {
        public bool Active;
        private Timer tradingTimeout;
        private const int TRADING_TIMEOUT = 5;



        public void StartEvent()
        {
            Active = true;
            tradingTimeout = new Timer(new TimerCallback(tradeTimedOut), null, TRADING_TIMEOUT * 60 * 1000, TRADING_TIMEOUT * 60 * 1000);
            byte[] msg = PacketBuilder.CreateChat(Messages.EventStartIsleTradingGame, PacketBuilder.CHAT_BOTTOM_RIGHT);

            foreach (GameClient client in GameServer.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(msg);

            // Give Trading Cards
            
            int[] allUsers = Database.GetUsers();
            foreach (int userid in allUsers)
            {
                int tradingCardId = Item.TradingCards[GameServer.RandomNumberGenerator.Next(0, Item.TradingCards.Length)];
                for (int i = 0; i < 4; i++)
                {
                    ItemInstance itm = new ItemInstance(tradingCardId);

                    if (GameServer.IsUserOnline(userid))
                        GameServer.GetUserById(userid).Inventory.AddWithoutDatabase(itm);

                    Database.AddItemToInventory(userid, itm);
                }
            }


        }

        public void EndEvent()
        {
            Active = false; 

            foreach(GameClient client in GameServer.ConnectedClients)
            {
                if(client.LoggedIn)
                {
                    int totalCards = 0;
                    int totalTypes = 0;

                    foreach (int itemId in Item.TradingCards)
                        if (client.LoggedinUser.Inventory.HasItemId(itemId))
                            totalCards += client.LoggedinUser.Inventory.GetItemByItemId(itemId).ItemInstances.Count;

                    if (client.LoggedinUser.Inventory.HasItemId(Item.ColtTradingCard))
                        totalTypes++;
                    if (client.LoggedinUser.Inventory.HasItemId(Item.FillyTradingCard))
                        totalTypes++;
                    if (client.LoggedinUser.Inventory.HasItemId(Item.MareTradingCard))
                        totalTypes++;
                    if (client.LoggedinUser.Inventory.HasItemId(Item.StallionTradingCard))
                        totalTypes++;

                    if(totalCards > 4)
                    {
                        byte[] disqualifiedTooManyCards = PacketBuilder.CreateChat(Messages.EventDisqualifiedIsleTradingGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        client.SendPacket(disqualifiedTooManyCards);
                    }
                    else if(totalTypes == 0)
                    {
                        byte[] noCardsMessage = PacketBuilder.CreateChat(Messages.EventNoneIsleTradingGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        client.SendPacket(noCardsMessage);
                    }
                    else if(totalTypes == 1) 
                    {
                        byte[] onlyOneTypeOfCardMesage = PacketBuilder.CreateChat(Messages.EventOnlyOneTypeIsleTradingGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        client.SendPacket(onlyOneTypeOfCardMesage);
                    }
                    else if (totalTypes == 2)
                    {
                        byte[] onlyTwoTypeOfCardMesage = PacketBuilder.CreateChat(Messages.EventOnlyTwoTypeIsleTradingGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        client.SendPacket(onlyTwoTypeOfCardMesage);
                    }
                    else if (totalTypes == 3)
                    {
                        byte[] onlyThreeTypeOfCardMesage = PacketBuilder.CreateChat(Messages.EventOnlyThreeTypeIsleTradingGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        client.SendPacket(onlyThreeTypeOfCardMesage);
                    }
                    else if (totalTypes == 4) 
                    {
                        client.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.IsleCardsGameWin).Count++;

                        byte[] wonIsleCardGame = PacketBuilder.CreateChat(Messages.EventWonIsleTradingGame, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        client.SendPacket(wonIsleCardGame);

                        client.LoggedinUser.Money += 25000;

                    }


                }
            }

            // Remove all trading cards from the game
            foreach (int itemId in Item.TradingCards)
                GameServer.RemoveAllItemsOfIdInTheGame(itemId);

            tradingTimeout.Dispose();
            tradingTimeout = null;
        }

        private void tradeTimedOut(object state)
        {
            EndEvent();
        }
    }

}
