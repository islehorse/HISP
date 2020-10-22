using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Horse_Isle_Server
{
    class Client
    {
        public Socket ClientSocket;
        public string RemoteIp;

        public bool LoggedIn = false;
        public User LoggedinUser;

        private Thread recvPackets;
        
        private Timer updateTimer;
        private Timer inactivityTimer;

        private Timer warnTimer;
        private Timer kickTimer;


        private int keepAliveInterval = 60 * 1000;
        private int updateInterval = 60 * 1000;

        private int warnInterval = Server.IdleWarning * 60 * 1000;
        private int kickInterval = Server.IdleTimeout * 60 * 1000;

        
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
        }

        private void kickTimerTick(object state)
        {
            Kick(Messages.FormatIdleKickMessage());
        }
        private void updateTimerTick(object state)
        {
            Server.UpdateWorld(this);
            Server.UpdatePlayer(this);
        }
        public void Login(int id)
        {
            LoggedinUser = new User(this,id);
            LoggedIn = true;

            updateTimer = new Timer(new TimerCallback(updateTimerTick), null, updateInterval, updateInterval);
            inactivityTimer = new Timer(new TimerCallback(keepAliveTimerTick), null, keepAliveInterval, keepAliveInterval);
        }
        private void receivePackets()
        {
            // HI1 Packets are terminates by 0x00 so we have to read until we receive that terminator
            MemoryStream ms = new MemoryStream();

            while(ClientSocket.Connected)
            {
                try
                {
                    if (ClientSocket.Available >= 1)
                    {
                        byte[] buffer = new byte[ClientSocket.Available];
                        ClientSocket.Receive(buffer);


                        foreach (Byte b in buffer)
                        {
                            ms.WriteByte(b);
                            if (b == 0x00)
                            {
                                ms.Seek(0x00, SeekOrigin.Begin);
                                byte[] fullPacket = ms.ToArray();
                                parsePackets(fullPacket);

                                ms.Close();
                                ms = new MemoryStream();
                            }
                        }

                    }
                }
                catch(SocketException e)
                {
                    Logger.ErrorPrint("Socket exception occured: " + e.Message);
                    Disconnect();
                    break;
                }

            }

                
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
                inactivityTimer.Change(keepAliveInterval, keepAliveInterval);

           
            if (kickTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
                kickTimer.Change(kickInterval, kickInterval);

            if (warnTimer != null && identifier != PacketBuilder.PACKET_KEEP_ALIVE)
                warnTimer.Change(warnInterval, warnInterval);

            if (!LoggedIn) // Must be either login or policy-file-request
            {
                if (Encoding.UTF8.GetString(Packet).StartsWith("<policy-file-request/>")) // Policy File Request
                {
                    Server.OnCrossdomainPolicyRequest(this);
                }
                switch (identifier)
                {
                    case PacketBuilder.PACKET_LOGIN:
                        Server.OnLoginRequest(this, Packet);
                        break;
                }
            }
            else
            {
                switch (identifier)
                {
                    case PacketBuilder.PACKET_LOGIN:
                        Server.OnUserInfoRequest(this, Packet);
                        break;
                    case PacketBuilder.PACKET_MOVE:
                        Server.OnMovementPacket(this, Packet);
                        break;
                    case PacketBuilder.PACKET_PROFILE:
                        Server.OnProfilePacket(this, Packet);
                        break;
                    case PacketBuilder.PACKET_CHAT:
                        Server.OnChatPacket(this, Packet);
                        break;
                    case PacketBuilder.PACKET_KEEP_ALIVE:
                        Server.OnKeepAlive(this, Packet);
                        break;
                    case PacketBuilder.PACKET_TRANSPORT:
                        Server.OnTransportUsed(this, Packet);
                        break;
                    case PacketBuilder.PACKET_INVENTORY:
                        Server.OnInventoryRequested(this, Packet);
                        break;
                    case PacketBuilder.PACKET_ITEM_INTERACTION:
                        Server.OnItemInteraction(this,Packet);
                        break;
                    default:
                        Logger.ErrorPrint("Unimplemented Packet: " + BitConverter.ToString(Packet).Replace('-', ' '));
                        break;
                }
            }
        }

       public void Disconnect()
        {
            Logger.DebugPrint(ClientSocket.RemoteEndPoint + " has Disconnected.");
            recvPackets.Abort();
            if(updateTimer != null)
                updateTimer.Dispose();
            if(inactivityTimer != null)
                inactivityTimer.Dispose();
            if(warnTimer != null)
                warnTimer.Dispose();
            if(kickTimer != null)
                kickTimer.Dispose();
            
            Server.OnDisconnect(this);
            LoggedIn = false;
            LoggedinUser = null;
            ClientSocket.Close();
            ClientSocket.Dispose();
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

        public Client(Socket clientSocket)
        {
            ClientSocket = clientSocket;
            RemoteIp = clientSocket.RemoteEndPoint.ToString();

            Logger.DebugPrint("Client connected @ " + RemoteIp);

            kickTimer = new Timer(new TimerCallback(kickTimerTick), null, kickInterval, kickInterval);
            warnTimer = new Timer(new TimerCallback(warnTimerTick), null, warnInterval, warnInterval);

            recvPackets = new Thread(() =>
            {
                receivePackets();
            });
            recvPackets.Start();
        }
    }
}
