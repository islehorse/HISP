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
        private const byte WEBSOCKET_CLOSE = 0x8;

        private const byte WEBSOCKET_PING = 0x9;
        private const byte WEBSOCKET_PONG = 0xA;
        
        private const byte WEBSOCKET_LENGTH_INT16 = 0x7E;
        private const byte WEBSOCKET_LENGTH_INT64 = 0x7F;

        private const int WEBSOCKET_EXPECTED_SIZE_SET = 0;
        private const int WEBSOCKET_EXPECTED_SIZE_UNSET = -1;

        private byte[] currentMessage = new byte[0];
        private byte[] currentPacket = new byte[0];

        private byte lastOpcode;
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

        private bool isExpectedSizeSet()
        {
            return (this.expectedLength > WEBSOCKET_EXPECTED_SIZE_SET);
        }
        private void setUnknownExpectedLength()
        {
            this.expectedLength = WEBSOCKET_EXPECTED_SIZE_UNSET;
        }

        private bool isCurrentPacketLengthLessThanExpectedLength()
        {
            if (!isExpectedSizeSet()) return false;
            return (currentPacket.LongCount() < this.expectedLength);
        }

        public override void ProcessReceivedPackets(int available, byte[] buffer)
        {
            // add to current packet
            // if current packet is less than size of an expected incoming message
            // then keep receiving until full message received.
            int oldLength = currentPacket.Length;
            Array.Resize(ref currentPacket, oldLength + available);
            Array.ConstrainedCopy(buffer, 0, currentPacket, oldLength, available);

            if (isCurrentPacketLengthLessThanExpectedLength())
                return;
            else
                setUnknownExpectedLength();

            //byte[] webSocketMsg = currentPacket.ToArray();

            if (!handshakeDone)
            {

                if (IsStartOfHandshake(currentPacket) && IsEndOfHandshake(currentPacket))
                {
                    string httpHandshake = Encoding.UTF8.GetString(currentPacket);
                    byte[] handshakeResponse = parseHandshake(httpHandshake);
                    base.Send(handshakeResponse);

                    Array.Resize(ref currentPacket, 0);
                    handshakeDone = true;
                }
            }
            else if(currentPacket.Length > 2) // else, begin parsing websocket message
            {

                byte[] unmaskKey = new byte[4];

                bool finished = (currentPacket[0] & 0b10000000) != 0;

                bool rsv1 = (currentPacket[0] & 0b01000000) != 0;
                bool rsv2 = (currentPacket[0] & 0b00100000) != 0;
                bool rsv3 = (currentPacket[0] & 0b00010000) != 0;

                byte opcode = Convert.ToByte(currentPacket[0] & 0b00001111);

                bool mask = (currentPacket[1] & 0b10000000) != 0;
                Int64 messageLength = Convert.ToInt64(currentPacket[1] & 0b01111111);

                int offset = 2;


                if (messageLength == WEBSOCKET_LENGTH_INT16)
                {
                    if (currentPacket.LongLength >= offset + 2)
                    {
                        byte[] uint16Bytes = new byte[2];
                        Array.ConstrainedCopy(currentPacket, offset, uint16Bytes, 0, uint16Bytes.Length);
                        uint16Bytes = uint16Bytes.Reverse().ToArray();
                        messageLength = BitConverter.ToUInt16(uint16Bytes);

                        offset += uint16Bytes.Length;
                    }
                }
                else if (messageLength == WEBSOCKET_LENGTH_INT64)
                {
                    if (currentPacket.LongLength >= offset + 8)
                    {
                        byte[] int64Bytes = new byte[8];
                        Array.ConstrainedCopy(currentPacket, offset, int64Bytes, 0, int64Bytes.Length);
                        int64Bytes = int64Bytes.Reverse().ToArray();
                        messageLength = BitConverter.ToInt64(int64Bytes);

                        offset += int64Bytes.Length;
                    }
                }

                if (mask)
                {
                    Array.ConstrainedCopy(currentPacket, offset, unmaskKey, 0, unmaskKey.Length);
                    offset += unmaskKey.Length;
                }

                // Handle tcp fragmentation
                
                Int64 actualLength = (currentPacket.LongLength - offset);
                
                // check if full message received, if not then set expected length
                // and return, thus entering the loop at the beginning
                if (actualLength < messageLength) 
                {
                    expectedLength = messageLength + offset; // set expected length and return
                    webSocketLog("Partial websocket frame received, expected size: " + messageLength + " got size: " + actualLength);
                    return;
                }

                // clone current packet array
                byte[] currentPacketCopy = currentPacket.ToArray();

                // set current packet array size back to 0
                setUnknownExpectedLength();
                Array.Resize(ref currentPacket, 0);

                // dont care about extensions
                if (rsv1 || rsv2 || rsv3) return;

                webSocketLog("Finished: " + finished + "\nRsv1: " + rsv1 + "\nRsv2: " + rsv2 + "\nRsv3: " + rsv3 + "\nOpcode: " + opcode + "\nMask: " + mask + "\nMesssageLength: " + messageLength);

                if (opcode != WEBSOCKET_CONTINUE)
                    lastOpcode = opcode;

                // do the thing the websocket frame says to do
                switch (opcode)
                {
                    case WEBSOCKET_CONTINUE:
                    case WEBSOCKET_BINARY:
                    case WEBSOCKET_TEXT:
                    case WEBSOCKET_PING:
                        oldLength = currentMessage.Length;
                        Array.Resize(ref currentMessage, oldLength + Convert.ToInt32(messageLength));
                        if (mask)
                        {
                            for (int i = 0; i < Convert.ToInt32(messageLength); i++)
                                currentMessage[oldLength + i] = Convert.ToByte(currentPacketCopy[offset + i] ^ unmaskKey[i % unmaskKey.Length]);
                        }
                        else
                        {
                            Array.ConstrainedCopy(currentPacketCopy, offset, currentMessage, oldLength, Convert.ToInt32(messageLength));
                        }
                        break;
                    case WEBSOCKET_CLOSE:
                        this.Disconnect();
                        return;
                }

                // handle end of websocket message.
                if (finished)
                {

                    if (lastOpcode != WEBSOCKET_PING && currentMessage.LongLength > 0)
                        onReceiveCallback(currentMessage);
                    else
                        Send(currentMessage);

                    Array.Resize(ref currentMessage, 0);
                    Array.Resize(ref currentPacket, 0);
                }


                // is there another frame after this one?
                // buffer remaining data back to ProcessReceivedPackets
                if(actualLength > messageLength)
                {
                    Int64 left = (actualLength - messageLength);

                    Int64 loc = messageLength + offset;
                    int total = buffer.Length;

                    for (Int64 totalSent = left; totalSent > 0; totalSent -= total)
                    {
                        if (totalSent <= total)
                            total = Convert.ToInt32(totalSent);

                        
                        Array.ConstrainedCopy(currentPacketCopy, Convert.ToInt32(loc), buffer, 0, total);
                        
                        webSocketLog("Found another frame at the end of this one, processing!");
                        ProcessReceivedPackets(total, buffer);

                        loc += total;
                    }
                }

            }


        }

        // specify transport name is "WebSocket"
        public override string Name
        {
            get
            {
                return "WebSocket";
            }
        }



        // encode data into websocket frames and send over network
        public override void Send(byte[] data)
        {
            if(this.Disconnected) return;
            if (data == null) return;

            // apparently you cant mask responses? chrome gets mad when i do it,
            // so dont set this to true.
            bool mask = false;

            int maxLength = this.workBuffer.Length;
            int toSend = maxLength;

            byte opcode = ((lastOpcode == WEBSOCKET_PING) ? WEBSOCKET_PONG : WEBSOCKET_BINARY);

            Int64 totalData = data.LongLength;
 
            bool finish = false;

            // despite its name, this has nothing to do with graphics
            // rather this is for WebSocket frames
            List<byte> frameHeader = new List<byte>();

            for (Int64 remain = totalData;  remain > 0; remain -= toSend)
            {
                // Is this the first frame?
                // if so; send opcode binary
                // otherwise, were continuing out last one
                if (remain != totalData)
                    opcode = WEBSOCKET_CONTINUE;

                if(remain <= maxLength)
                {
                    toSend = Convert.ToInt32(remain);
                    finish = true;
                }

                frameHeader.Add(Convert.ToByte((0x00) | (finish ? 0b10000000 : 0b00000000) | opcode));

                // do special length encoding
                byte maskAndLength = Convert.ToByte((0x00) | (mask ? 0b10000000 : 0b00000000));
                byte[] additionalLengthData = new byte[0];
                if (toSend >= WEBSOCKET_LENGTH_INT16)
                {
                    if(toSend < UInt16.MaxValue)
                    {
                        maskAndLength |= WEBSOCKET_LENGTH_INT16;
                        additionalLengthData = BitConverter.GetBytes(Convert.ToUInt16(toSend)).Reverse().ToArray();

                    }
                    else if(Convert.ToInt64(toSend) < Int64.MaxValue)
                    {
                        maskAndLength |= WEBSOCKET_LENGTH_INT64;
                        additionalLengthData = BitConverter.GetBytes(Convert.ToInt64(toSend)).Reverse().ToArray();
                    }

                }
                else
                {
                    maskAndLength |= Convert.ToByte(toSend);
                }

                // Add to buffer
                frameHeader.Add(maskAndLength);
                Helper.ByteArrayToByteList(additionalLengthData, frameHeader);

                // Generate masking key;
                byte[] maskingKey = new byte[4];

                if (mask)
                {
                    GameServer.RandomNumberGenerator.NextBytes(maskingKey);
                    Helper.ByteArrayToByteList(maskingKey, frameHeader);
                }

                int headerSize = frameHeader.Count;

                byte[] frame = new byte[toSend + headerSize];
                Array.Copy(frameHeader.ToArray(), frame, headerSize);
                frameHeader.Clear();

                Int64 totalSent = (totalData - remain);
                
                if (mask) // are we masking this response? 
                {
                    // Mask data using key.
                    for (int i = 0; i < toSend; i++)
                        frame[i + headerSize] = Convert.ToByte(data[i + totalSent] ^ maskingKey[i % maskingKey.Length]);
                }
                else if(data.LongLength < Int32.MaxValue) // is out packet *really* bigger than 32 max int??
                {
                    Array.ConstrainedCopy(data, Convert.ToInt32(totalSent), frame, headerSize, toSend);
                }

                // Finally send complete frame over the network
                base.Send(frame);

                if (this.Disconnected) return; // are we still here? 
            }


        }


    }
}
