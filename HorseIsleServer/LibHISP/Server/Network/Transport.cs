using HISP.Util;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace HISP.Server.Network
{
    public abstract class Transport : ITransport
    {
        internal Socket socket;
        internal string remoteIp;

        internal Action<byte[]> onReceiveCallback;
        internal Action onDisconnectCallback;

        internal byte[] workBuffer = new byte[0xFFFF];

        internal bool isDisconnecting = false;

        public abstract void ProcessReceivedPackets(int available, byte[] buffer);
        public abstract string Name { get; }

        internal virtual bool checkForError(SocketAsyncEventArgs e)
        {
            if (isDisconnecting || socket == null || e.BytesTransferred <= 0 || !socket.Connected || e.SocketError != SocketError.Success)
            {
                Disconnect();
                return true;
            }
            else
            {
                return false;
            }
        }
        internal virtual void receivePackets(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                do
                {
                    if (checkForError(e)) break;
                    ProcessReceivedPackets(e.BytesTransferred, e.Buffer);
                    if (checkForError(e)) break;

                } while (!socket.ReceiveAsync(e));
            }
            catch (Exception ex) { 
                Logger.ErrorPrint(ex.StackTrace); 
                try { this.Disconnect(); } catch (Exception) { }; 
            };

        }

        public virtual string Ip
        {
            get
            {
                return this.remoteIp;
            }
        }

        public virtual bool Disconnected
        {
            get
            {
                return this.isDisconnecting;
            }
        }

        internal virtual void passObjects(Socket socket, Action<byte[]> onReceive, Action onDisconnect)
        {
            socket.SendTimeout = 10 * 1000; // 10sec
            socket.ReceiveTimeout = 10 * 1000; // 10sec

            this.socket = socket;
            this.onReceiveCallback = onReceive;
            this.onDisconnectCallback = onDisconnect;
            this.remoteIp = Helper.GetIp(socket.RemoteEndPoint);
        }

        public virtual void Accept(Socket socket, Action<byte[]> onReceive, Action onDisconnect)
        {
            passObjects(socket, onReceive, onDisconnect);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += receivePackets;
            e.SetBuffer(workBuffer, 0, workBuffer.Length);
            if (!socket.ReceiveAsync(e))
                receivePackets(null, e);
        }

        public virtual void Disconnect()
        {
            if (this.isDisconnecting)
                return;
            this.isDisconnecting = true;

            // Close Socket
            if (socket != null)
            {
                try
                {
                    socket.Disconnect(false);
                    socket.Dispose();
                    socket = null;

                }
                catch (SocketException) { }
                catch (ObjectDisposedException) { };
            }

            onDisconnectCallback();
        }

        public virtual void Send(byte[] data)
        {
            if (Disconnected) return;
            if (data == null) return;

            try
            {
                socket.Send(data);
            }
            catch (ObjectDisposedException)
            {
                if (!Disconnected)
                    Disconnect();
            }
        }
    }
}

