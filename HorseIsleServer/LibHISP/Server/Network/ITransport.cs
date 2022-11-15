using System;
using System.Net.Sockets;

namespace HISP.Server.Network
{
    public interface ITransport
    {
        public string Name { get; }
        public bool Disconnected { get; }
        public string Ip { get; }

        public void Accept(Socket socket, Action<byte[]> onReceive, Action onDisconnect);
        public void Send(byte[] data);
        public void Disconnect();
    }
}
