using System;
using System.Net.Sockets;
using System.Threading;
using HISP.Player;
using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Events;
using HISP.Game.Items;
using HISP.Util;
using HISP.Server.Network;
using HISP.Ipc;

namespace HISP.Server
{
    public class GameClient
    {
        private static ThreadSafeList<GameClient> connectedClients = new ThreadSafeList<GameClient>();
        public static GameClient[] ConnectedClients // Done to prevent Enumerator Changed errors.
        {
            get
            {
                return connectedClients.ToArray();
            }
        }

        private ITransport networkTransport;

        private bool loggedIn = false;

        public string RemoteIp
        {
            get
            {
                return networkTransport.Ip;
            }
        }
        public bool LoggedIn 
        {
            get
            {
                bool login = loggedIn;
                if (User == null)
                    return false;
                if (User.Client == null)
                    return false;
                return login;
            }
            set
            {
                loggedIn = value;
            }
        }
        public User User;

        private Timer keepAliveTimer;
        private Timer timeoutTimer;

        private Timer warnTimer;
        private Timer kickTimer;
        private Timer minuteTimer;


        private int timeoutInterval = 95 * 1000;

        private int totalMinutesElapsed = 0;
        private int oneMinute = 60 * 1000;
        private int warnInterval = GameServer.IdleWarning * 60 * 1000; // Time before showing a idle warning
        private int kickInterval = GameServer.IdleTimeout * 60 * 1000; // Time before kicking for inactivity
        

        public GameClient(Socket clientSocket)
        {

            kickTimer = new Timer(new TimerCallback(kickTimerTick), null, kickInterval, kickInterval);
            warnTimer = new Timer(new TimerCallback(warnTimerTick), null, warnInterval, warnInterval);
            minuteTimer = new Timer(new TimerCallback(minuteTimerTick), null, oneMinute, oneMinute);

            networkTransport = new Hybrid();
            networkTransport.Accept(clientSocket, parsePackets, disconnectHandler);
            Logger.DebugPrint(networkTransport.Name + " : Client connected @ " + networkTransport.Ip);


        }

        public static void OnShutdown(string reason)
        {
            try
            {
                foreach (GameClient client in ConnectedClients)
                {

                    if (client.LoggedIn)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            ItemInstance rubyItem = new ItemInstance(Item.Ruby);
                            client.User.Inventory.AddIgnoringFull(rubyItem);
                        }

                        client.User.TrackedItems.GetTrackedItem(Tracking.TrackableItem.GameUpdates).Count++;
                        Logger.DebugPrint("Kicking: " + client.User.Username);
                    }
                    client.Kick("Server shutdown: "+reason);
                }
            }
            catch (Exception) { };
        }

        private static bool acceptConnections(SocketAsyncEventArgs e)
        {
            try
            {
                if (e == null) return false;
                if (GameServer.ServerSocket == null) return false;
                return !GameServer.ServerSocket.AcceptAsync(e);
            }
            catch (Exception) { return false; }
        }
        public static void CreateClient(object sender, SocketAsyncEventArgs e)
        {
            do
            {
                Socket clientSocket = e.AcceptSocket;

                if (clientSocket == null)
                    continue;
                if (clientSocket.RemoteEndPoint == null)
                    continue;

                connectedClients.Add(new GameClient(clientSocket));
                e.AcceptSocket = null;

            } while (acceptConnections(e));
        }
        private void timeoutTimerTick(object state)
        {
            if (this.LoggedIn)
            {
                Disconnect();
            }
        }

        private void disconnectHandler()
        {

            // Stop Timers
            if (timeoutTimer != null)
            {
                timeoutTimer.Dispose();
                timeoutTimer = null;
            }
            if (keepAliveTimer != null)
            {
                keepAliveTimer.Dispose();
                keepAliveTimer = null;
            }
            if (warnTimer != null)
            {
                warnTimer.Dispose();
                warnTimer = null;
            }
            if (kickTimer != null)
            {
                kickTimer.Dispose();
                kickTimer = null;
            }

            // Call OnDisconnect

            connectedClients.Remove(this);
            GameServer.OnDisconnect(this);
            LoggedIn = false;
        }
        public void Disconnect()
        {
            if(!networkTransport.Disconnected)
                networkTransport.Disconnect();
        }

        private void keepAliveTick(object state)
        {
            Logger.DebugPrint("Sending keep-alive packet to " + User.Username);
            byte[] updatePacket = PacketBuilder.CreateKeepAlive();
            SendPacket(updatePacket);
            keepAliveTimer.Change(oneMinute, oneMinute);
        }
        private void minuteTimerTick(object state)
        {
            totalMinutesElapsed++;
            if (LoggedIn)
            {

                GameServer.UpdatePlayer(this);

                User.CanUseAdsChat = true;
                User.FreeMinutes -= 1;

                GameServer.DoItemPurchases(this);

                if (totalMinutesElapsed % 2 == 0)
                {
                    User.TotalGlobalChatMessages++;
                }

                if (User.FreeMinutes <= 0)
                {
                    User.FreeMinutes = 0;
                    if (!User.Subscribed && !User.Moderator && !User.Administrator)
                    {
                        Kick(Messages.KickReasonNoTime);
                        return;
                    }

                }

                // Those fun messages when u have been playing for awhile.
                if (totalMinutesElapsed % (2 * 60) == 0)
                {
                    string ptMessage = Messages.RngMessages[GameServer.RandomNumberGenerator.Next(0, Messages.RngMessages.Length)];
                    byte[] playTimeMessage = PacketBuilder.CreateChat(Messages.FormatPlaytimeMessage(totalMinutesElapsed / 60) + ptMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    SendPacket(playTimeMessage);
                }


                if (GameServer.RandomNumberGenerator.Next(0, 100) == 59) // RANDOM EVENT HAS OCCURED!
                {
                    RandomEvent.ExecuteRandomEvent(User);
                }

                bool gotoPrision = false;
                foreach(HorseInstance horse in User.HorseInventory.HorseList)
                {
                    if (totalMinutesElapsed % 2 == 0)
                    {
                        horse.BasicStats.Thirst--;
                        horse.BasicStats.Hunger--;

                        if (horse.BasicStats.Thirst <= 0 && horse.BasicStats.Hunger <= 0)
                        {
                            horse.BasicStats.Health -= 5;
                            if(horse.BasicStats.Hunger <= 0)
                            {
                                gotoPrision = true; // Goto jail, go directly to jail, do not pass go, do not collect 200$

                                horse.BasicStats.Health = 10;
                                horse.BasicStats.Hunger = 500;
                                horse.BasicStats.Thirst = 500;
                            }
                        }
                    }

                  
                    if(horse.Leaser > 0)
                    {
                        horse.LeaseTime--;

                        if (horse.LeaseTime <= 0)
                        {
                            int tpX = 0;
                            int tpY = 0;
                            if(horse.Breed.Type == "unicorn" || horse.Breed.Type == "pegasus")
                            {
                                foreach (World.SpecialTile tile in World.SpecialTiles)
                                {
                                    if (tile.Code == null)
                                        continue;

                                    if (tile.Code.StartsWith("HORSELEASER-"))
                                    {
                                        int id = int.Parse(tile.Code.Split("-")[1]);
                                        if (horse.Leaser == id)
                                        {
                                            string msg = Messages.FormatHorseReturnedToUniter(horse.Breed.Name);
                                            if (horse.Breed.Type == "pegasus")
                                                msg = Messages.HorseLeaserReturnedToUniterPegasus;

                                            byte[] youWereTeleportedToUniter = PacketBuilder.CreateChat(msg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                            SendPacket(youWereTeleportedToUniter);

                                            tpX = tile.X;
                                            tpY = tile.Y;

                                            if(tile.ExitX != 0 && tile.ExitY != 0)
                                            {
                                                tpX = tile.ExitX;
                                                tpY = tile.ExitY;
                                            }
                                            else
                                            {
                                                tpY++;
                                            }

                                        }
                                    }
                                }
                                
                            }

                            byte[] horseReturned = PacketBuilder.CreateChat(Messages.FormatHorseReturnedToOwner(horse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            SendPacket(horseReturned);

                            if(tpX != 0 && tpY != 0)
                                User.Teleport(tpX, tpY);


                            if (User.CurrentlyRidingHorse != null)
                            {
                                if(User.CurrentlyRidingHorse.RandomId == horse.RandomId)
                                {
                                    GameServer.StopRidingHorse(this);
                                }
                                
                             }

                            if(User.LastViewedHorse != null)
                            {
                                if(User.LastViewedHorse.RandomId == horse.RandomId)
                                {
                                    User.LastViewedHorse = null;
                                }
                            }    


                             User.HorseInventory.DeleteHorse(horse);
                        }

                        
                    }

                }
                if(gotoPrision)
                {
                    byte[] sendToPrision = PacketBuilder.CreateChat(Messages.YouWereSentToPrisionIsle, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    SendPacket(sendToPrision);
                    User.Teleport(45, 35);
                }


                if (totalMinutesElapsed % 5 == 0)
                    User.Thirst--;
                
                if (totalMinutesElapsed % 15 == 0)
                    User.Hunger--;

                if (totalMinutesElapsed % 15 == 0)
                    User.Tiredness--;
            }

            minuteTimer.Change(oneMinute, oneMinute);
        }

        private void warnTimerTick(object state)
        {
            Logger.DebugPrint("Sending inactivity warning to: " + RemoteIp);
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatIdleWarningMessage(), PacketBuilder.CHAT_BOTTOM_RIGHT);
            SendPacket(chatPacket);
            if (LoggedIn)
                User.Idle = true;
        }

        private void kickTimerTick(object state)
        {
            Kick(Messages.FormatIdleKickMessage());
        }

        public void Login(int id)
        {
            /*
             *  Check for duplicate user
             *  and disconnect them.
             */
            foreach (GameClient Client in GameClient.ConnectedClients)
            {
                if (Client.LoggedIn)
                {
                    if (Client.User.Id == id)
                        Client.Kick(Messages.KickReasonDuplicateLogin);
                }
            }

            User = new User(this, id);
            LoggedIn = true;

            Database.SetIpAddress(id, RemoteIp);
            Database.SetLoginCount(id, Database.GetLoginCount(id) + 1);

            keepAliveTimer = new Timer(new TimerCallback(keepAliveTick), null, oneMinute, oneMinute);
            timeoutTimer = new Timer(new TimerCallback(timeoutTimerTick), null, timeoutInterval, timeoutInterval);
        }

        private void parsePackets(byte[] packet)
        {
            if (packet.Length < 1)
            {
                Logger.ErrorPrint("Received an invalid packet (size: "+packet.Length+")");
                return;
            }
            byte identifier = packet[0];

            /*
             *  Every time ive tried to fix this properly by just checking if its null or something
             *  it keeps happening, so now im just going to catch the exception
             *  and hope it works.
             */
            try
            {
                if (LoggedIn)
                {
                    if (timeoutTimer != null)
                        timeoutTimer.Change(timeoutInterval, timeoutInterval); // Reset time before timing out
                    else
                        return;

                    if (keepAliveTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
                    {
                        if (LoggedIn)
                            User.Idle = false;
                        keepAliveTimer.Change(oneMinute, oneMinute);
                    }
                    else
                    {
                        return;
                    }
                }

                if (kickTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
                    kickTimer.Change(kickInterval, kickInterval);
                else
                    return;

                if (warnTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
                    warnTimer.Change(warnInterval, warnInterval);
                else
                    return;

            }
            catch (ObjectDisposedException e) 
            {
                Logger.DebugPrint("Unhandled exception: " + e.Message);
                return;
            }


            /*
             *  Put packet handling in a try/catch
             *  this prevents the entire server from crashing
             *  if theres an error in handling a particular packet.
             */
#if !OS_DEBUG
            try
            {
#endif
                if (!LoggedIn)
                {
                    switch (identifier)
                    {
                        case PacketBuilder.PACKET_LOGIN:
                            GameServer.OnUserLogin(this, packet);
                            break;
#if ENABLE_IPC
                        case IpcPacket.PACKET_IPC:
                            IpcPacket.OnIpcReceived(this, packet);
                            break;
#endif
                    }
                }
                else
                {
                    switch (identifier)
                    {
                        case PacketBuilder.PACKET_LOGIN:
                            GameServer.OnUserInfoRequest(this, packet);
                            break;
                        case PacketBuilder.PACKET_MOVE:
                            GameServer.OnMovementPacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_PLAYERINFO:
                            GameServer.OnPlayerInfoPacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_PLAYER:
                            GameServer.OnProfilePacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_CHAT:
                            GameServer.OnChatPacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_CLICK:
                            GameServer.OnClickPacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_KEEP_ALIVE:
                            GameServer.OnKeepAlive(this, packet);
                            break;
                        case PacketBuilder.PACKET_TRANSPORT:
                            GameServer.OnTransportUsed(this, packet);
                            break;
                        case PacketBuilder.PACKET_INVENTORY:
                            GameServer.OnInventoryRequested(this, packet);
                            break;
                        case PacketBuilder.PACKET_DYNAMIC_BUTTON:
                            GameServer.OnDynamicButtonPressed(this, packet);
                            break;
                        case PacketBuilder.PACKET_DYNAMIC_INPUT:
                            GameServer.OnDynamicInputReceived(this, packet);
                            break;
                        case PacketBuilder.PACKET_ITEM_INTERACTION:
                            GameServer.OnItemInteraction(this, packet);
                            break;
                        case PacketBuilder.PACKET_ARENA_SCORE:
                            GameServer.OnArenaScored(this, packet);
                            break;
                        case PacketBuilder.PACKET_QUIT:
                            GameServer.OnQuitPacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_NPC:
                            GameServer.OnNpcInteraction(this, packet);
                            break;
                        case PacketBuilder.PACKET_BIRDMAP:
                            GameServer.OnBirdMapRequested(this, packet);
                            break;
                        case PacketBuilder.PACKET_SWFMODULE:
                            GameServer.OnSwfModuleCommunication(this, packet);
                            break;
                        case PacketBuilder.PACKET_HORSE:
                            GameServer.OnHorseInteraction(this, packet);
                            break;
                        case PacketBuilder.PACKET_WISH:
                            GameServer.OnWish(this, packet);
                            break;
                        case PacketBuilder.PACKET_RANCH:
                            GameServer.OnRanchPacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_AUCTION:
                            GameServer.OnAuctionPacket(this, packet);
                            break;
                        case PacketBuilder.PACKET_PLAYER_INTERACTION:
                            GameServer.OnPlayerInteration(this, packet);
                            break;
                        case PacketBuilder.PACKET_SOCIALS:
                            GameServer.OnSocialPacket(this, packet);
                            break;
                        default:
                            Logger.ErrorPrint("Unimplemented packet: " + BitConverter.ToString(packet).Replace('-', ' '));
                            break;
                    }
                }
#if !OS_DEBUG
            }
            catch (Exception e)
            {
                Logger.ErrorPrint("Unhandled Exception: " + e.Message + "\n" + e.StackTrace);
                Kick("Unhandled Exception: " + e.Message + "\n" + e.StackTrace);
            }
#endif
        }

        public void Kick(string Reason)
        {
            byte[] kickPacket = PacketBuilder.CreateKickMessage(Reason);
            SendPacket(kickPacket);
            Disconnect();

            Logger.InfoPrint("CLIENT: "+RemoteIp+" KICKED for: "+Reason);
        }

       public void SendPacket(byte[] packetData)
        {
            if(!networkTransport.Disconnected)
                networkTransport.Send(packetData);
        }

    }


}
