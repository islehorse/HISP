using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using HISP.Player;
using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Events;
using HISP.Game.Items;
using HISP.Util;

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

        public Socket ClientSocket;
        public string RemoteIp;
        private bool loggedIn = false;
        public bool LoggedIn 
        {
            get
            {
                bool login = loggedIn;
                if (LoggedinUser == null)
                    return false;
                if (LoggedinUser.LoggedinClient == null)
                    return false;
                return login;
            }
            set
            {
                loggedIn = value;
            }
        }
        public User LoggedinUser;

        private Timer keepAliveTimer;
        private Timer timeoutTimer;

        private Timer warnTimer;
        private Timer kickTimer;
        private Timer minuteTimer;

        private bool isDisconnecting = false;
        private int timeoutInterval = 95 * 1000;

        private int totalMinutesElapsed = 0;
        private int oneMinute = 60 * 1000;
        private int warnInterval = GameServer.IdleWarning * 60 * 1000; // Time before showing a idle warning
        private int kickInterval = GameServer.IdleTimeout * 60 * 1000; // Time before kicking for inactivity
        
        private List<byte> currentPacket = new List<byte>();
        private byte[] workBuffer = new byte[0x8000];

        public GameClient(Socket clientSocket)
        {
            clientSocket.SendTimeout = 10 * 1000; // 10sec
            clientSocket.ReceiveTimeout = 10 * 1000; // 10sec

            ClientSocket = clientSocket;
            RemoteIp = clientSocket.RemoteEndPoint.ToString();

            if (RemoteIp.Contains(":"))
                RemoteIp = RemoteIp.Substring(0, RemoteIp.IndexOf(":"));

            Logger.DebugPrint("Client connected @ " + RemoteIp);

            kickTimer = new Timer(new TimerCallback(kickTimerTick), null, kickInterval, kickInterval);
            warnTimer = new Timer(new TimerCallback(warnTimerTick), null, warnInterval, warnInterval);
            minuteTimer = new Timer(new TimerCallback(minuteTimerTick), null, oneMinute, oneMinute);

            connectedClients.Add(this);

            SocketAsyncEventArgs evt = new SocketAsyncEventArgs();
            evt.Completed += receivePackets;
            evt.SetBuffer(workBuffer, 0, workBuffer.Length);
            if (!clientSocket.ReceiveAsync(evt))
                receivePackets(null, evt);
        }

        public static void OnShutdown()
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
                            client.LoggedinUser.Inventory.AddIgnoringFull(rubyItem);
                        }

                        client.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.GameUpdates).Count++;
                        Logger.DebugPrint("Kicking: " + client.LoggedinUser.Username);
                    }
                    client.Kick("Server shutdown.");

                }
            }
            catch (Exception) { };
        }
        public static void CreateClient(object sender, SocketAsyncEventArgs e)
        {
            try{
		        do
		        {
		            Socket eSocket = e.AcceptSocket;
                    if (eSocket == null)
                        return;
                    if (eSocket.RemoteEndPoint == null)
                        return;
			        
                    new GameClient(eSocket);
		            e.AcceptSocket = null;
		    
		            if(GameServer.ServerSocket == null)
		    	        return;
		        } while (!GameServer.ServerSocket.AcceptAsync(e));
            }catch(ObjectDisposedException) {} // server shutdown
        }
        private void timeoutTimerTick(object state)
        {
            if (this.LoggedIn)
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (this.isDisconnecting)
                return;
            this.isDisconnecting = true;
            
            // Close Socket
            if (ClientSocket != null)
            {
                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
                ClientSocket = null;
            }


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


        private void receivePackets(object sender, SocketAsyncEventArgs e)
        {
            do
            {
                // HI1 Packets are terminates by 0x00 so we have to read until we receive that terminator

                if (isDisconnecting ||
                    ClientSocket == null ||
                    e.BytesTransferred <= 0 ||
                    !ClientSocket.Connected || 
                    e.SocketError != SocketError.Success)
                {
                    Disconnect();
                    return;
                }

                int availble = e.BytesTransferred;
                if (availble >= 1) // More than 1 byte transfered..
                {

                    for (int i = 0; i < availble; i++)
                    {
                        currentPacket.Add(e.Buffer[i]);
                        if (e.Buffer[i] == PacketBuilder.PACKET_TERMINATOR) // Read until \0...
                        {
                            parsePackets(currentPacket.ToArray());
                            currentPacket.Clear();
                        }
                    }
                }

                if (availble == 0)
                    Disconnect();

                if (isDisconnecting || ClientSocket == null)
                    return;


            } while (!ClientSocket.ReceiveAsync(e));

        }

        private void keepAliveTick(object state)
        {
            Logger.DebugPrint("Sending keep-alive packet to " + LoggedinUser.Username);
            byte[] updatePacket = PacketBuilder.CreateKeepAlive();
            SendPacket(updatePacket);
            if(!isDisconnecting && keepAliveTimer != null) // wtf how is this still a problem?
               keepAliveTimer.Change(oneMinute, oneMinute);
        }
        private void minuteTimerTick(object state)
        {
            totalMinutesElapsed++;
            if (LoggedIn)
            {

                GameServer.UpdatePlayer(this);

                LoggedinUser.CanUseAdsChat = true;
                LoggedinUser.FreeMinutes -= 1;

                GameServer.DoItemPurchases(this);

                if (totalMinutesElapsed % 2 == 0)
                {
                    LoggedinUser.TotalGlobalChatMessages++;
                }

                if (LoggedinUser.FreeMinutes <= 0)
                {
                    LoggedinUser.FreeMinutes = 0;
                    if (!LoggedinUser.Subscribed && !LoggedinUser.Moderator && !LoggedinUser.Administrator)
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
                    RandomEvent.ExecuteRandomEvent(LoggedinUser);
                }

                bool gotoPrision = false;
                foreach(HorseInstance horse in LoggedinUser.HorseInventory.HorseList)
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
                                LoggedinUser.Teleport(tpX, tpY);


                            if (LoggedinUser.CurrentlyRidingHorse != null)
                            {
                                if(LoggedinUser.CurrentlyRidingHorse.RandomId == horse.RandomId)
                                {
                                    GameServer.StopRidingHorse(this);
                                }
                                
                             }

                            if(LoggedinUser.LastViewedHorse != null)
                            {
                                if(LoggedinUser.LastViewedHorse.RandomId == horse.RandomId)
                                {
                                    LoggedinUser.LastViewedHorse = null;
                                }
                            }    


                             LoggedinUser.HorseInventory.DeleteHorse(horse);
                        }

                        
                    }

                }
                if(gotoPrision)
                {
                    byte[] sendToPrision = PacketBuilder.CreateChat(Messages.YouWereSentToPrisionIsle, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    SendPacket(sendToPrision);
                    LoggedinUser.Teleport(45, 35);
                }


                if (totalMinutesElapsed % 5 == 0)
                    LoggedinUser.Thirst--;
                
                if (totalMinutesElapsed % 15 == 0)
                    LoggedinUser.Hunger--;

                if (totalMinutesElapsed % 15 == 0)
                    LoggedinUser.Tiredness--;
            }




            if (!isDisconnecting)
               minuteTimer.Change(oneMinute, oneMinute);

        }

        private void warnTimerTick(object state)
        {
            Logger.DebugPrint("Sending inactivity warning to: " + RemoteIp);
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatIdleWarningMessage(), PacketBuilder.CHAT_BOTTOM_RIGHT);
            SendPacket(chatPacket);
            if (LoggedIn)
                LoggedinUser.Idle = true;
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
                    if (Client.LoggedinUser.Id == id)
                        Client.Kick(Messages.KickReasonDuplicateLogin);
                }
            }

            LoggedinUser = new User(this, id);
            LoggedIn = true;

            Database.SetIpAddress(id, RemoteIp);
            Database.SetLoginCount(id, Database.GetLoginCount(id) + 1);

            keepAliveTimer = new Timer(new TimerCallback(keepAliveTick), null, oneMinute, oneMinute);
            timeoutTimer = new Timer(new TimerCallback(timeoutTimerTick), null, timeoutInterval, timeoutInterval);
        }

        private void parsePackets(byte[] Packet)
        {
            if (Packet.Length < 1)
            {
                Logger.ErrorPrint("Received an invalid packet (size: "+Packet.Length+")");
            }
            byte identifier = Packet[0];

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
                            LoggedinUser.Idle = false;
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
            catch (ObjectDisposedException) 
            {
                return;
            }


            /*
             *  Put packet handling in a try/catch
             *  this prevents the entire server from crashing
             *  if theres an error in handling a particular packet.
             */
#if (!DEBUG)
            try
            {
#endif
                if (!LoggedIn) // Must be either login or policy-file-request
                {
                    switch (identifier)
                    {
                        case PacketBuilder.PACKET_FLASH_XML_CROSSDOMAIN:
                            GameServer.OnCrossdomainPolicyRequest(this, Packet);
                            break;
                        case PacketBuilder.PACKET_LOGIN:
                            GameServer.OnLoginRequest(this, Packet);
                            break;
                    }
                }
                else
                {
                    switch (identifier)
                    {
                        case PacketBuilder.PACKET_LOGIN:
                            GameServer.OnUserInfoRequest(this, Packet);
                            break;
                        case PacketBuilder.PACKET_MOVE:
                            GameServer.OnMovementPacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_PLAYERINFO:
                            GameServer.OnPlayerInfoPacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_PLAYER:
                            GameServer.OnProfilePacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_CHAT:
                            GameServer.OnChatPacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_CLICK:
                            GameServer.OnClickPacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_KEEP_ALIVE:
                            GameServer.OnKeepAlive(this, Packet);
                            break;
                        case PacketBuilder.PACKET_TRANSPORT:
                            GameServer.OnTransportUsed(this, Packet);
                            break;
                        case PacketBuilder.PACKET_INVENTORY:
                            GameServer.OnInventoryRequested(this, Packet);
                            break;
                        case PacketBuilder.PACKET_DYNAMIC_BUTTON:
                            GameServer.OnDynamicButtonPressed(this, Packet);
                            break;
                        case PacketBuilder.PACKET_DYNAMIC_INPUT:
                            GameServer.OnDynamicInputReceived(this, Packet);
                            break;
                        case PacketBuilder.PACKET_ITEM_INTERACTION:
                            GameServer.OnItemInteraction(this, Packet);
                            break;
                        case PacketBuilder.PACKET_ARENA_SCORE:
                            GameServer.OnArenaScored(this, Packet);
                            break;
                        case PacketBuilder.PACKET_QUIT:
                            GameServer.OnQuitPacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_NPC:
                            GameServer.OnNpcInteraction(this, Packet);
                            break;
                        case PacketBuilder.PACKET_BIRDMAP:
                            GameServer.OnBirdMapRequested(this, Packet);
                            break;
                        case PacketBuilder.PACKET_SWFMODULE:
                            GameServer.OnSwfModuleCommunication(this, Packet);
                            break;
                        case PacketBuilder.PACKET_HORSE:
                            GameServer.OnHorseInteraction(this, Packet);
                            break;
                        case PacketBuilder.PACKET_WISH:
                            GameServer.OnWish(this, Packet);
                            break;
                        case PacketBuilder.PACKET_RANCH:
                            GameServer.OnRanchPacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_AUCTION:
                            GameServer.OnAuctionPacket(this, Packet);
                            break;
                        case PacketBuilder.PACKET_PLAYER_INTERACTION:
                            GameServer.OnPlayerInteration(this, Packet);
                            break;
                        case PacketBuilder.PACKET_SOCIALS:
                            GameServer.OnSocialPacket(this, Packet);
                            break;
                        default:
                            Logger.ErrorPrint("Unimplemented Packet: " + BitConverter.ToString(Packet).Replace('-', ' '));
                            break;
                    }
                }
#if (!DEBUG)
            }
            catch(Exception e)
            {
                Logger.ErrorPrint("Unhandled Exception: " + e.ToString() + "\n" + e.Message + "\n" + e.StackTrace);
                Kick("Unhandled Exception: " + e.ToString());
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

       public void SendPacket(byte[] PacketData)
        {
            try
            {
                ClientSocket.Send(PacketData);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

    }


}
