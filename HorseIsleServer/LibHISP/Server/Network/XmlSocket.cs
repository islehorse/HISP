using HISP.Security;
using HISP.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HISP.Server.Network
{
    public class XmlSocket : Transport
    {
        private List<byte> currentPacket = new List<byte>();
        private const byte XMLSOCKET_PACKET_TERMINATOR = 0x00;
        private static byte[] XMLSOCKET_POLICY_FILE = Encoding.UTF8.GetBytes("<policy-file-request/>");
        public override void ProcessReceivedPackets(int available, byte[] buffer)
        {
            // In XmlSocket Packets are terminates by 0x00 so we have to read until we receive that terminator

            for (int i = 0; i < available; i++)
            {
                if (buffer[i] == XMLSOCKET_PACKET_TERMINATOR) // Read until \0...
                {
                    byte[] packet = currentPacket.ToArray();
                    
                    if (Helper.ByteArrayStartsWith(packet, XMLSOCKET_POLICY_FILE) && ConfigReader.EnableSocketPolicyServer) {
                        this.Send(SocketDomainPolicy.GetPolicyFile());
                    }
                    else {
                        onReceiveCallback(packet);
                    }
                    
                    currentPacket.Clear();
                    continue;
                }
                currentPacket.Add(buffer[i]);
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
            int oldLength = data.Length;

            // Resize the array to be 1 extra byte in size;
            Array.Resize(ref data, oldLength + 1);

            // add \0 to the end of the buffer
            data[oldLength] = XMLSOCKET_PACKET_TERMINATOR;

            // send to the server
            base.Send(data);
        }

    }
}
