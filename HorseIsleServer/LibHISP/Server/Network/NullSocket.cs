// A transport that does absolutely nothing.
// only use this for testing.

using System;
using System.Net;
using System.Net.Sockets;

namespace HISP.Server.Network
{
    public class NullSocket : ITransport
    {
        private bool disconnected = false;
        public string Name 
        {
            get 
            { 
                return "NullSocket"; 
            }
        }
        public bool Disconnected
        {
            get
            {
                return disconnected;
            }
            set
            {
                disconnected = value;
            }
        }

        public string Ip
        {
            get
            {
                return IPAddress.Loopback.MapToIPv4().ToString();
            }
        }
        public void Accept(Socket socket, Action<byte[]> onReceive, Action onDisconnect)
        {
            return;
        }

        public void Disconnect()
        {
            disconnected = true;
            return;
        }

        public void Send(byte[] data)
        {
            return;
        }
    }
}
