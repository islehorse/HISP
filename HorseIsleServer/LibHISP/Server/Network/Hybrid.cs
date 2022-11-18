using System;
using System.Net.Sockets;

namespace HISP.Server.Network
{
    public class Hybrid : Transport
    {
        Transport actualTransport = null;

       
        public override string Name
        {
            get
            {
                if(actualTransport == null)
                    return "TransportDeterminer";
                else
                    return actualTransport.Name;
            }
        }


        public override bool Disconnected
        {
            get
            {
                if (actualTransport == null)
                    return base.Disconnected;
                else
                    return actualTransport.Disconnected;
            }
        }

        public override string Ip
        {
            get
            {
                if (actualTransport == null)
                    return base.Ip;
                else
                    return actualTransport.Ip;
            }
        }


        public override void Disconnect()
        {
            if (actualTransport == null)
            {
                base.Disconnect();
            }
            else
            {
                actualTransport.Disconnect();
            }
        }

        public override void ProcessReceivedPackets(int available, byte[] buffer)
        {

            if (ConfigReader.EnableWebSocket && WebSocket.IsStartOfHandshake(buffer))
            {
                Logger.InfoPrint(this.Ip + " Switching to WebSocket");
                actualTransport = new WebSocket();

                actualTransport.passObjects(this.socket, this.onReceiveCallback, this.onDisconnectCallback);
                actualTransport.ProcessReceivedPackets(available, buffer);
                actualTransport.Accept(base.socket, base.onReceiveCallback, base.onDisconnectCallback);
            }
            else
            {
                Logger.InfoPrint(this.Ip + " Switching to XmlSocket");
                actualTransport = new XmlSocket();

                actualTransport.passObjects(this.socket, this.onReceiveCallback, this.onDisconnectCallback);
                actualTransport.ProcessReceivedPackets(available, buffer);
                actualTransport.Accept(base.socket, base.onReceiveCallback, base.onDisconnectCallback);
            }
        }

        internal override void receivePackets(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if(base.checkForError(e)) return;
                ProcessReceivedPackets(e.BytesTransferred, e.Buffer);
                if (base.checkForError(e)) return;
            }
            catch (Exception ex)
            {
                Logger.ErrorPrint(ex.StackTrace);
                try { this.Disconnect(); } catch (Exception) { };
            };
        }

        public override void Send(byte[] data)
        {
            if(actualTransport == null)
                base.Send(data);
            else
                actualTransport.Send(data);
        }
    }
}
