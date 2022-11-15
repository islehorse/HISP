using HISP.Security;
using HISP.Util;
using System.Text;

namespace HISP.Server.Network
{
    public class XmlSocket : Transport
    {
        public override void ProcessReceivedPackets(int available, byte[] buffer)
        {
            // In XmlSocket Packets are terminates by 0x00 so we have to read until we receive that terminator
            for (int i = 0; i < available; i++)
            {
                currentPacket.Add(buffer[i]);
                if (buffer[i] == PacketBuilder.PACKET_TERMINATOR) // Read until \0...
                {
                    onReceiveCallback(currentPacket.ToArray());
                    currentPacket.Clear();
                }
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

    }
}
