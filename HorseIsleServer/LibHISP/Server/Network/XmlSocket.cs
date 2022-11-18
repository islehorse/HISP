using HISP.Security;
using HISP.Util;
using System;
using System.Text;

namespace HISP.Server.Network
{
    public class XmlSocket : Transport
    {
        private const byte XMLSOCKET_PACKET_TERMINATOR = 0x00;
        public override void ProcessReceivedPackets(int available, byte[] buffer)
        {
            // In XmlSocket Packets are terminates by 0x00 so we have to read until we receive that terminator
            for (int i = 0; i < available; i++)
            {
                if (buffer[i] == XMLSOCKET_PACKET_TERMINATOR) // Read until \0...
                {
                    onReceiveCallback(currentPacket.ToArray());
                    currentPacket.Clear();
                }
                currentPacket.Add(buffer[i]);
            }

            // Handle XMLSocket Policy File
            if (Helper.ByteArrayStartsWith(buffer, Encoding.UTF8.GetBytes("<policy-file-request/>")))
            {
                this.Send(CrossDomainPolicy.GetPolicyFile());
            }
        }

        public override string Name
        {
            get
            {
                return "XmlSocket";
            }
        }

        public override void Send(byte[] data)
        {
            // add \0 to the end of the buffer
            byte[] buffer = new byte[data.Length + 1];
            Array.Copy(data, buffer, data.Length); // copy packet to buffer
            buffer[buffer.Length - 1] = XMLSOCKET_PACKET_TERMINATOR;

            // send to the server
            base.Send(data);
        }

    }
}
