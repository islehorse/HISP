#define WEBSOCKET_DEBUG

using HISP.Security;
using HISP.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HISP.Server.Network
{
    public class WebSocket : Transport
    {
        private const string WEBSOCKET_SEED = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private const byte WEBSOCKET_CONTINUE = 0x0;
        private const byte WEBSOCKET_TEXT = 0x1;
        private const byte WEBSOCKET_BINARY = 0x2;

        private const byte WEBSOCKET_LENGTH_INT16 = 0x7E;
        private const byte WEBSOCKET_LENGTH_INT64 = 0x7F;

        private List<byte> currentMessage = new List<byte>();

        private int lastOpcode;
        private Int64 expectedLength = -1;
        private bool handshakeDone = false;
        private void webSocketLog(string msg)
        {
#if WEBSOCKET_DEBUG
            foreach(string str in msg.Replace("\r", "").Split("\n"))
                Logger.InfoPrint("[WEBSOCKET] " + str);
#endif
        }

        private Dictionary<string, string> parseHttpHeaders(string httpResponse)
        {
            Dictionary<string, string> httpHeaders = new Dictionary<string, string>();
            string[] parts = httpResponse.Replace("\r", "").Split("\n");
            foreach (string part in parts)
            {
                if (part.StartsWith("GET")) continue;

                if (part.Contains(":"))
                {
                    string[] keyValuePairs = part.Split(":");
                    if (keyValuePairs.Length >= 2)
                        httpHeaders.Add(keyValuePairs[0].Trim().ToLower(), keyValuePairs[1].Trim());
                }
                else
                {
                    continue;
                }
            }

            return httpHeaders;
        }

        private string deriveWebsocketSecKey(string webSocketKey)
        {
            byte[] derivedKey = Authentication.Sha1Digest(Encoding.UTF8.GetBytes(webSocketKey.Trim() + WEBSOCKET_SEED.Trim()));
            return Convert.ToBase64String(derivedKey);
        }
        private byte[] createHandshakeResponse(string secWebsocketKey)
        {
            string msg = String.Join("\r\n", new string[] {
                    "HTTP/1.1 101 Switching Protocols",
                    "Connection: Upgrade",
                    "Upgrade: websocket",
                    "Sec-WebSocket-Accept: " + secWebsocketKey,
                    "",
                    ""
                });
            webSocketLog(msg);
            return Encoding.UTF8.GetBytes(msg);
        }

        private byte[] parseHandshake(string handshakeResponse)
        {
            webSocketLog(handshakeResponse);
            Dictionary<string, string> headers = parseHttpHeaders(handshakeResponse);

            string webSocketKey = null;
            headers.TryGetValue("sec-websocket-key", out webSocketKey);

            if (webSocketKey != null)
            {
                string secWebsocketKey = deriveWebsocketSecKey(webSocketKey);
                return createHandshakeResponse(secWebsocketKey);
            }

            return createHandshakeResponse("");
        }

        public static bool IsStartOfHandshake(byte[] data)
        {
            return Helper.ByteArrayStartsWith(data, Encoding.UTF8.GetBytes("GET"));
        }

        public static bool IsEndOfHandshake(byte[] data)
        {
            return Helper.ByteArrayEndsWith(data, Encoding.UTF8.GetBytes("\r\n\r\n"));
        }

        public override void ProcessReceivedPackets(int available, byte[] buffer)
        {
            Int64 received = 0;
            // add to current packet
            // if current packet is less than size of an expected incoming message
            // then keep receiving until full message received.

            Int64 curPacketLength = currentPacket.LongCount();
            for (received = 0; received < available; received++)
            {
                currentPacket.Add(buffer[received]);
                curPacketLength++;

                // i use a <0 value to say there is no expected length
                // check if entire packet received
                if (expectedLength > 0 && (curPacketLength >= expectedLength)) 
                    break;
            }

            if (expectedLength > 0 && (currentPacket.LongCount() < expectedLength))
                return;
            else
                expectedLength = -1;

            byte[] webSocketMsg = currentPacket.ToArray();

            if (!handshakeDone)
            {

                if (IsStartOfHandshake(webSocketMsg) && IsEndOfHandshake(webSocketMsg))
                {
                    string httpHandshake = Encoding.UTF8.GetString(webSocketMsg);
                    byte[] handshakeResponse = parseHandshake(httpHandshake);
                    base.Send(handshakeResponse);

                    currentPacket.Clear();
                    handshakeDone = true;
                }
            }
            else if(currentPacket.Count > 2) // else, begin parsing websocket message
            {

                byte[] unmaskKey = new byte[4];

                bool finished = (webSocketMsg[0] & 0b10000000) != 0;

                bool rsv1 = (webSocketMsg[0] & 0b01000000) != 0;
                bool rsv2 = (webSocketMsg[0] & 0b00100000) != 0;
                bool rsv3 = (webSocketMsg[0] & 0b00010000) != 0;

                int opcode = (webSocketMsg[0] & 0b00001111);

                bool mask = (webSocketMsg[1] & 0b10000000) != 0;
                Int64 messageLength = Convert.ToInt64(webSocketMsg[1] & 0b01111111);

                int offset = 2;


                if (messageLength == WEBSOCKET_LENGTH_INT16)
                {
                    if (webSocketMsg.LongLength >= offset + 2)
                    {
                        byte[] uint16Bytes = new byte[2];
                        Array.ConstrainedCopy(webSocketMsg, offset, uint16Bytes, 0, uint16Bytes.Length);
                        uint16Bytes = uint16Bytes.Reverse().ToArray();
                        messageLength = BitConverter.ToUInt16(uint16Bytes);

                        offset += uint16Bytes.Length;
                    }
                }
                else if (messageLength == WEBSOCKET_LENGTH_INT64)
                {
                    if (webSocketMsg.LongLength >= offset + 8)
                    {
                        byte[] int64Bytes = new byte[8];
                        Array.ConstrainedCopy(webSocketMsg, offset, int64Bytes, 0, int64Bytes.Length);
                        int64Bytes = int64Bytes.Reverse().ToArray();
                        messageLength = BitConverter.ToInt64(int64Bytes);

                        offset += int64Bytes.Length;
                    }
                }

                if (mask)
                {
                    Array.ConstrainedCopy(webSocketMsg, offset, unmaskKey, 0, unmaskKey.Length);
                    offset += unmaskKey.Length;
                }

                // Handle tcp fragmentation
                
                Int64 actualLength = (webSocketMsg.LongLength - offset);
                
                // check if full message received, if not then set expected length
                // and return, thus entering the loop at the beginning
                if (actualLength < messageLength) 
                {
                    expectedLength = messageLength + offset; // set expected length and return
                    webSocketLog("Partial websocket frame received, expected size: " + messageLength + " got size: " + actualLength);
                    return;
                }
                else
                {
                    expectedLength = -1;
                    currentPacket.Clear();
                }

                // dont care about extensions
                if (rsv1 || rsv2 || rsv3) return;

                webSocketLog("Finished: " + finished + "\nRsv1: " + rsv1 + "\nRsv2: " + rsv2 + "\nRsv3: " + rsv3 + "\nOpcode: " + opcode + "\nMask: " + mask + "\nMesssageLength: " + messageLength);

                // websocket packet fully received, begin processing

                // allow for continuing
                if (opcode == WEBSOCKET_CONTINUE)
                    lastOpcode = opcode;
                else
                    opcode = lastOpcode;

                switch (opcode)
                {
                    case WEBSOCKET_BINARY: // writing this almost killed me, because im non-binary :(
                    case WEBSOCKET_TEXT:
                        for (Int64 i = 0; i < messageLength; i++) 
                            currentMessage.Add(mask ? Convert.ToByte(webSocketMsg[offset + i] ^ unmaskKey[i % unmaskKey.Length]) : Convert.ToByte(webSocketMsg[offset + i]));
                        break;
                }

                if (finished)
                {
                    if(currentMessage.LongCount() > 0)
                    {
                        byte[] message = currentMessage.ToArray();

                        if(opcode == WEBSOCKET_TEXT)
                            webSocketLog(Encoding.UTF8.GetString(message));

                        onReceiveCallback(message);
                        currentMessage.Clear();
                    }
                }


                // is there another frame after this one?
                if(actualLength > messageLength)
                {
                    Int64 left = (actualLength - messageLength);
                    Int64 totalSent = left;

                    Int64 loc = messageLength + offset;

                    while(totalSent > 0)
                    {
                        int total = buffer.Length;
                        if (totalSent < total)
                            total = Convert.ToInt32(totalSent);

                        for (int i = 0; i < total; i++)
                            buffer[i] = webSocketMsg[loc + i];
                        
                        ProcessReceivedPackets(total, buffer);

                        totalSent -= total;
                        loc += total;
                    }
                    
                }
            }

            // parse remaining data after end.

            if (received < available)
            {
                int left = Convert.ToInt32(available - received);
                byte[] leftData = new byte[left];
                Array.ConstrainedCopy(buffer, available, leftData, 0, left);
                Array.Copy(leftData, buffer, leftData.Length);

                ProcessReceivedPackets(available, buffer);
            }

        }



        public override string Name
        {
            get
            {
                return "WebSocket";
            }
        }


        
        public override void Send(byte[] data)
        {
            throw new NotImplementedException();
        }


    }
}
