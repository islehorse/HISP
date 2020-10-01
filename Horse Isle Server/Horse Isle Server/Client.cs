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

        private int updateInterval = 60 * 1000;
        private void updateTimerTick(object state)
        {
            Logger.DebugPrint("Sending update packet to "+ LoggedinUser.Username);
            byte[] updatePacket = PacketBuilder.CreateUpdate();
            SendPacket(updatePacket);
        }
        public void Login(int id)
        {
            LoggedinUser = new User(id);
            LoggedIn = true;


            updateTimer = new Timer(new TimerCallback(updateTimerTick), null, updateInterval, updateInterval);
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
                    Logger.ErrorPrint("Socket exception occured: " + e.Message +" and so it was disconnected.");
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

            if (updateTimer != null)
                updateTimer.Change(updateInterval, updateInterval);

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
                    case PacketBuilder.PACKET_UPDATE:
                        Server.OnUpdatePacket(this, Packet);
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
            updateTimer.Dispose();
            LoggedIn = false;
            LoggedinUser = null;
            Server.ConnectedClients.Remove(this);
            ClientSocket.Close();
            ClientSocket.Dispose();
        }

       public void SendPacket(byte[] PacketData)
        {
            try
            {
                ClientSocket.Send(PacketData);
            }
            catch (SocketException e)
            {
                Logger.ErrorPrint("Socket exception occured: " + e.Message + " and so it was disconnected.");
                Disconnect();
            }
        }

        public Client(Socket clientSocket)
        {
            ClientSocket = clientSocket;
            RemoteIp = clientSocket.RemoteEndPoint.ToString();

            Logger.DebugPrint("Client connected @ " + RemoteIp);

            recvPackets = new Thread(() =>
            {
                receivePackets();
            });
            recvPackets.Start();
        }
    }
}
