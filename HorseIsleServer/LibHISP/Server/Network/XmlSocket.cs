using HISP.Security;
using HISP.Util;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HISP.Server.Network
{
    public class XmlSocket : Transport
    {
        private List<byte> currentPacket = new List<byte>();
        private static byte[] XMLSOCKET_POLICY_FILE = Encoding.UTF8.GetBytes("<policy-file-request/>");
        public override void ProcessReceivedPackets(int available, byte[] buffer)
        {
            // In XmlSocket Packets are terminates by 0x00 so we have to read until we receive that terminator

            for (int i = 0; i < available; i++)
            {
                if (buffer[i] == 0) // Read until \0...
                {
                    byte[] packet = currentPacket.ToArray();
                    
                    if (Helper.ByteArrayStartsWith(packet, XMLSOCKET_POLICY_FILE) && ConfigReader.EnableSocketPolicyServer) {
                        this.Send(SocketDomainPolicy.GetPolicyFile());
                    }
                    else {
                        Logger.DebugPrint("[WEBSOCKET] [RECV] " + BitConverter.ToString(packet).Replace("-", " "));
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

            Logger.DebugPrint("[XMLSOCKET] [SEND] " + BitConverter.ToString(data).Replace("-", " "));

            // send to the server
            base.Send(data);
        }

    }
}
