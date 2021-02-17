using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HISP.Player;
using HISP.Game;
using HISP.Game.Horse;

namespace HISP.Server
{
    public class GameClient
    {
        public Socket ClientSocket;
        public string RemoteIp;

        public bool LoggedIn = false;
        public User LoggedinUser;

        private Thread recvPackets;
        
        private Timer inactivityTimer;

        private Timer warnTimer;
        private Timer kickTimer;
        private Timer minuteTimer;

        private bool isDisconnecting = false;
        private int keepAliveInterval = 60 * 1000;

        private int totalMinutesElapsed = 0;
        private int oneMinute = 60 * 1000;
        private int warnInterval = GameServer.IdleWarning * 60 * 1000;
        private int kickInterval = GameServer.IdleTimeout * 60 * 1000;

        private bool dcLock = false;

        private void minuteTimerTick(object state)
        {
            dcLock = true;
            totalMinutesElapsed++;
            if (LoggedIn)
            {
                LoggedinUser.FreeMinutes -= 1;
                if (LoggedinUser.FreeMinutes <= 0)
                {
                    LoggedinUser.FreeMinutes = 0;
                    if (!LoggedinUser.Subscribed && !LoggedinUser.Moderator && !LoggedinUser.Administrator)
                    {
                        dcLock = false;
                        Kick(Messages.KickReasonNoTime);
                        return;
                    }

                }

                foreach(HorseInstance horse in LoggedinUser.HorseInventory.HorseList)
                {
                    if (totalMinutesElapsed % 2 == 0)
                    {
                        horse.BasicStats.Thirst--;
                        horse.BasicStats.Hunger--;

                        if (horse.BasicStats.Thirst <= 0)
                            horse.BasicStats.Health -= 5;
                    }

                    if (totalMinutesElapsed % 60 == 0)
                    {
                        horse.BasicStats.Shoes--;
                        horse.BasicStats.Tiredness--;
                    }


                }


                if (totalMinutesElapsed % 2 == 0)
                {
                    LoggedinUser.Thirst--;
                    LoggedinUser.Hunger--;
                }


                if (totalMinutesElapsed % 10 == 0)
                    LoggedinUser.Tiredness--;
            }

            if(!isDisconnecting)
               minuteTimer.Change(oneMinute, oneMinute);
            dcLock = false;
        }
        private void keepAliveTimerTick(object state)
        {
            Logger.DebugPrint("Sending keep-alive packet to "+ LoggedinUser.Username);
            byte[] updatePacket = PacketBuilder.CreateKeepAlive();
            SendPacket(updatePacket);
        }

        private void warnTimerTick(object state)
        {
            Logger.DebugPrint("Sending inactivity warning to: " + RemoteIp);
            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatIdleWarningMessage(), PacketBuilder.CHAT_BOTTOM_RIGHT);
            SendPacket(chatPacket);
            if (LoggedIn)
                LoggedinUser.Idle = true;
            warnTimer.Dispose();
            warnTimer = null;

        }

        private void kickTimerTick(object state)
        {
            Kick(Messages.FormatIdleKickMessage());
        }

        public void Login(int id)
        {
            // Check for duplicate
            foreach(GameClient Client in GameServer.ConnectedClients)
            {
                if(Client.LoggedIn)
                {
                    if (Client.LoggedinUser.Id == id)
                        Client.Kick(Messages.KickReasonDuplicateLogin);
                }
            }
            LoggedinUser = new User(this,id);
            LoggedIn = true;

            Database.SetIpAddress(id, RemoteIp);

            inactivityTimer = new Timer(new TimerCallback(keepAliveTimerTick), null, keepAliveInterval, keepAliveInterval);
        }
        private bool receivePackets()
        {
            // HI1 Packets are terminates by 0x00 so we have to read until we receive that terminator
            

            while(ClientSocket.Connected && !isDisconnecting)
            {
                if(isDisconnecting)
                    break;

                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        if (ClientSocket.Available >= 1)
                        {
                            byte[] buffer = new byte[ClientSocket.Available];
                            ClientSocket.Receive(buffer);

                            foreach (Byte b in buffer)
                            {
                                if (isDisconnecting)
                                    break;

                                ms.WriteByte(b);
                                if (b == 0x00)
                                {
                                    ms.Seek(0x00, SeekOrigin.Begin);
                                    byte[] fullPacket = ms.ToArray();
                                    parsePackets(fullPacket);
                                    ms.Seek(0x00, SeekOrigin.Begin);
                                    ms.SetLength(0);
                                }
                            }
                        }
                    }
                }
                catch(SocketException)
                {
                    Disconnect();
                    break;
                }

            }

            while (dcLock) { }; // Refuse to shut down until dcLock is cleared. (prevents TOCTOU issues.)
            

            // Stop Timers
            if(inactivityTimer != null)
                inactivityTimer.Dispose();
            if(warnTimer != null)
                warnTimer.Dispose();
            if(kickTimer != null)
                kickTimer.Dispose();
            
            // Call OnDisconnect
            GameServer.OnDisconnect(this);
            LoggedIn = false;
            LoggedinUser = null;
            // Close Sockets
            ClientSocket.Close();
            ClientSocket.Dispose();

            return isDisconnecting; // Stop the thread.
        }

        private void parsePackets(byte[] Packet)
        {
            if (Packet.Length < 1)
            {
                Logger.ErrorPrint("Received an invalid packet (size: "+Packet.Length+")");
            }
            byte identifier = Packet[0];

            // Reset timers
           

            if (inactivityTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
            {
                if (LoggedIn)
                    LoggedinUser.Idle = false;
                inactivityTimer.Change(keepAliveInterval, keepAliveInterval);
            }
           
            if (kickTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
                kickTimer = new Timer(new TimerCallback(kickTimerTick), null, kickInterval, kickInterval);

            if (warnTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
                warnTimer = new Timer(new TimerCallback(warnTimerTick), null, warnInterval, warnInterval);

            if (!LoggedIn) // Must be either login or policy-file-request
            {
                if (Encoding.UTF8.GetString(Packet).StartsWith("<policy-file-request/>")) // Policy File Request
                {
                    GameServer.OnCrossdomainPolicyRequest(this);
                }
                switch (identifier)
                {
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
                        GameServer.OnItemInteraction(this,Packet);
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
                    default:
                        Logger.ErrorPrint("Unimplemented Packet: " + BitConverter.ToString(Packet).Replace('-', ' '));
                        break;
                }
            }
        }

       public void Disconnect()
        {
            
            // Cant outright stop threads anymore in .NET core,
            // Lets just let the thread stop gracefully.

            this.isDisconnecting = true;
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
            catch (Exception e)
            {
                Logger.ErrorPrint("Exception occured: " + e.Message);
                Disconnect();
            }
        }

        public GameClient(Socket clientSocket)
        {
            ClientSocket = clientSocket;
            RemoteIp = clientSocket.RemoteEndPoint.ToString();
            
            if(RemoteIp.Contains(":"))
                RemoteIp = RemoteIp.Substring(0, RemoteIp.IndexOf(":"));

            Logger.DebugPrint("Client connected @ " + RemoteIp);

            kickTimer = new Timer(new TimerCallback(kickTimerTick), null, kickInterval, kickInterval);
            warnTimer = new Timer(new TimerCallback(warnTimerTick), null, warnInterval, warnInterval);
            minuteTimer = new Timer(new TimerCallback(minuteTimerTick), null, oneMinute, oneMinute);

            recvPackets = new Thread(() =>
            {
                receivePackets();
            });
            recvPackets.Start();
            
        }
    }
}
