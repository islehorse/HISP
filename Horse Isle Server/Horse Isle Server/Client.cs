using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Client
    {
        public Socket ClientSocket;
        public string RemoteIp;

        public bool LoggedIn = false;
        public User LoggedinUser;

        private Thread recvPackets;

        private const byte PACKET_LOGIN = 0x7F;
        private const byte PACKET_CHAT = 0x14;
        private const byte PACKET_MOVE = 0x15;
        private const byte PACKET_USERINFO = 0x81;

        public void Login(int id)
        {
            LoggedinUser = new User(id);
            LoggedIn = true;
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
            if(!LoggedIn) // Must be either login or policy-file-request
            {
                if(Encoding.UTF8.GetString(Packet).StartsWith("<policy-file-request/>")) // Policy File Request
                {
                    Server.OnCrossdomainPolicyRequest(this);
                }
                switch(identifier)
                {
                    case PACKET_LOGIN:
                        Server.OnLoginRequest(this,Packet);
                        break;
                }
            }    
        }

       public void Disconnect()
        {
            Server.ConnectedClients.Remove(this);
            ClientSocket.Dispose();
        }

       public void SendPacket(byte[] PacketData)
        {
            ClientSocket.Send(PacketData);
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
