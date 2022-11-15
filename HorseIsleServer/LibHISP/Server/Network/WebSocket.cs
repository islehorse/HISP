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

        private const byte WEBASSEMBLY_CONTINUE = 0x0;
        private const byte WEBASSEMBLY_TEXT = 0x1;

        private const byte WEBASSEMBLY_LENGTH_INT16 = 0x7E;
        private const byte WEBASSEMBLY_LENGTH_INT64 = 0x7F;

        private List<byte> currentMessage = new List<byte>();

        private string secWebsocketKey = null;
        private bool handshakeDone = false;

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
            return Encoding.UTF8.GetBytes(String.Join("\r\n", new string[] {
                    "HTTP/1.1 101 Switching Protocols",
                    "Connection: Upgrade",
                    "Upgrade: websocket",
                    "Sec-WebSocket-Accept: " + secWebsocketKey,
                    "",
                    ""
                }));
        }

        private byte[] parseHandshake(string handshakeResponse)
        {
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
            for (int i = 0; i < available; i++)
                currentPacket.Add(buffer[i]);
            byte[] webAsmMsg = currentPacket.ToArray();

            if (!handshakeDone)
            {
                if (IsStartOfHandshake(webAsmMsg) && IsEndOfHandshake(webAsmMsg))
                {
                    string httpHandshake = Encoding.UTF8.GetString(webAsmMsg);
                    byte[] handshakeResponse = parseHandshake(httpHandshake);
                    base.Send(handshakeResponse);

                    currentPacket.Clear();
                    handshakeDone = true;
                }
            }
            if (currentPacket.Count >= 2)
            {
                bool finished = (currentPacket[0] & 0b10000000) != 0;
                int opcode = (currentPacket[0] & 0b00001111);

                bool mask = (currentPacket[1] & 0b10000000) != 0;
                UInt64 messageLength = Convert.ToUInt64(currentPacket[1] & 0b01111111);

                int offset = 2;

                if (messageLength == WEBASSEMBLY_LENGTH_INT16)
                {
                    if(currentPacket.Count >= offset + 2)
                    {
                        byte[] uint16Bytes = new byte[2];
                        Array.ConstrainedCopy(webAsmMsg, offset, uint16Bytes, 0, uint16Bytes.Length);
                        uint16Bytes = uint16Bytes.Reverse().ToArray();
                        messageLength = BitConverter.ToUInt16(uint16Bytes);

                        offset += uint16Bytes.Length;
                    }
                }
                else if (messageLength == WEBASSEMBLY_LENGTH_INT64)
                {
                    if (currentPacket.Count >= offset + 8)
                    {
                        byte[] uint64Bytes = new byte[8];
                        Array.ConstrainedCopy(webAsmMsg, offset, uint64Bytes, 0, uint64Bytes.Length);
                        uint64Bytes = uint64Bytes.Reverse().ToArray(); 
                        messageLength = BitConverter.ToUInt64(uint64Bytes);

                        offset += uint64Bytes.Length;
                    }
                }


                if (mask)
                {
                    switch (opcode)
                    {
                        case WEBASSEMBLY_TEXT:

                            if (currentPacket.LongCount() >= (offset + 4))
                            {
                                byte[] unmaskKey = new byte[4];
                                Array.ConstrainedCopy(buffer, offset, unmaskKey, 0, unmaskKey.Length);
                                offset += unmaskKey.Length;

                                for (int i = 0; i < (currentPacket.Count - offset); i++)
                                {
                                    currentMessage.Add(Convert.ToByte(currentPacket[offset+ i] ^ unmaskKey[i % unmaskKey.Length]));
                                }

                                currentPacket.Clear();
                            }
                            break;
                    }

                    if (finished)
                    {
                        onReceiveCallback(currentMessage.ToArray());
                        currentMessage.Clear();
                        currentPacket.Clear();
                    }
                }

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
