using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Server
    {

        public static Socket ServerSocket;
        public static List<Client> ConnectedClients = new List<Client>();

        public static void OnCrossdomainPolicyRequest(Client sender) // When a cross-domain-policy request is received.
        {
            Logger.DebugPrint("Cross-Domain-Policy request received from: " + sender.RemoteIp);

            byte[] crossDomainPolicyResponse = CrossDomainPolicy.GetPolicy(); // Generate response packet

            sender.SendPacket(crossDomainPolicyResponse); // Send to client.
        }

        public static void OnUserInfoRequest(Client sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested user information when not logged in.");
                return;
            }
            Logger.DebugPrint(sender.LoggedinUser.Username + " Requested user information.");
            MemoryStream ms = new MemoryStream();
            User user = sender.LoggedinUser;

            byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, PacketBuilder.DIRECTION_DOWN, PacketBuilder.DIRECTION_LOGIN, true);
            ms.Write(MovementPacket, 0x00, MovementPacket.Length);

            byte[] LoginMessage = PacketBuilder.CreateLoginMessage(user.Username);
            ms.Write(LoginMessage, 0x00, LoginMessage.Length);

            World.Time time = World.GetGameTime();
            int timestamp = time.hours * 60;
            timestamp += time.minutes;

            byte[] WorldData = PacketBuilder.CreateWorldData(timestamp, time.days, time.year, World.GetWeather());
            ms.Write(WorldData, 0x00, WorldData.Length);

            byte[] SecCodePacket = PacketBuilder.CreateSecCode(user.SecCodeSeeds, user.SecCodeInc, user.Administrator, user.Moderator);
            ms.Write(SecCodePacket, 0x00, SecCodePacket.Length);

            byte[] BaseStatsPacketData = PacketBuilder.CreateBaseStats(user.Money, Server.GetNumberOfPlayers(), user.MailBox.MailCount);
            ms.Write(BaseStatsPacketData, 0x00, BaseStatsPacketData.Length);

            byte[] AreaMessage = PacketBuilder.CreateAreaMessage(user.X, user.Y);
            ms.Write(AreaMessage, 0x00, AreaMessage.Length);

            byte[] IsleData = PacketBuilder.CreatePlaceData(World.Isles.ToArray(), World.Towns.ToArray(), World.Areas.ToArray());
            ms.Write(IsleData, 0x00, IsleData.Length);

            byte[] TileFlags = PacketBuilder.CreateTileOverlayFlags(Map.OverlayTileDepth);
            ms.Write(TileFlags, 0x00, TileFlags.Length);

            byte[] MotdData = PacketBuilder.CreateMotd();
            ms.Write(MotdData, 0x00, MotdData.Length);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] Packet = ms.ToArray();
            ms.Dispose();

            sender.SendPacket(Packet);
        }
        public static void OnUpdatePacket(Client sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested user information when not logged in.");
                return;
            }
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Update Packet");
                return;
            }

            if(packet[1] == PacketBuilder.PACKET_A_TERMINATOR)
            {
                Logger.DebugPrint(sender.LoggedinUser.Username + " Requested latest statistics (Money/Playercount/Mail)");
                byte[] packetResponse = PacketBuilder.CreateBaseStats(sender.LoggedinUser.Money, GetNumberOfPlayers(), sender.LoggedinUser.MailBox.MailCount);
                sender.SendPacket(packetResponse);
            }
        }
        public static void OnProfilePacket(Client sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested to change profile page when not logged in.");
                return;
            }
            if(packet.Length < 2)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Profile Packet");
                return;
            }
            
            byte method = packet[1];
            if(method == PacketBuilder.VIEW_PROFILE)
            {
                byte[] profilePacket = PacketBuilder.CreateProfilePacket(sender.LoggedinUser.ProfilePage);
                sender.SendPacket(profilePacket);
            }
            else if(method == PacketBuilder.SAVE_PROFILE)
            {

                string packetStr = Encoding.UTF8.GetString(packet);
                if (packet.Length < 3 || !packetStr.Contains('|'))
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Profile SAVE Packet");
                    return;
                }

                int characterId = (packet[2] - 20) * 64 + (packet[3] - 20);

                string profilePage = packetStr.Split('|')[1];
                profilePage = profilePage.Substring(0, profilePage.Length - 2);

                sender.LoggedinUser.CharacterId = characterId;
                sender.LoggedinUser.ProfilePage = profilePage;

                Logger.DebugPrint(sender.LoggedinUser.Username + " Changed to character id: " + characterId + " and set there Profile Description to '" + profilePage + "'");

                byte[] chatPacket = PacketBuilder.CreateChat(Messages.ProfileSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatPacket);

                UpdateArea(sender);
            }

        }

        public static void OnMovementPacket(Client sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent movement packet when not logged in.");
                return;
            }

            User loggedInUser = sender.LoggedinUser;
            byte movementDirection = packet[1];

            if(movementDirection == PacketBuilder.MOVE_UP)
            {
                if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y - 1))
                {
                    loggedInUser.Y -= 1;
                    byte[] moveUpResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_UP, PacketBuilder.DIRECTION_UP, true);
                    sender.SendPacket(moveUpResponse);
                }
                else
                {
                    byte[] moveUpResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_UP, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveUpResponse);
                }
            }
            else if(movementDirection == PacketBuilder.MOVE_LEFT)
            {
                if (Map.CheckPassable(loggedInUser.X - 1, loggedInUser.Y))
                {
                    loggedInUser.X -= 1;
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_LEFT, PacketBuilder.DIRECTION_LEFT, true);
                    sender.SendPacket(moveLeftResponse);
                }
                else
                {
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_LEFT, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveLeftResponse);
                }
            }
            else if(movementDirection == PacketBuilder.MOVE_RIGHT)
            {
                if (Map.CheckPassable(loggedInUser.X + 1, loggedInUser.Y))
                {
                    loggedInUser.X += 1;
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.DIRECTION_RIGHT, true);
                    sender.SendPacket(moveLeftResponse);
                }
                else
                {
                    byte[] moveLeftResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveLeftResponse);
                }
            }
            else if(movementDirection == PacketBuilder.MOVE_DOWN)
            {
                if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1))
                {
                    loggedInUser.Y += 1;
                    byte[] moveDownResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_DOWN, PacketBuilder.DIRECTION_DOWN, true);
                    sender.SendPacket(moveDownResponse);
                }
                else
                {
                    byte[] moveDownResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, PacketBuilder.DIRECTION_DOWN, PacketBuilder.DIRECTION_NONE, false);
                    sender.SendPacket(moveDownResponse);
                }

            }

            UpdateArea(sender);

        }

        public static void OnLoginRequest(Client sender, byte[] packet)
        {
            Logger.DebugPrint("Login request received from: " + sender.RemoteIp);

            string loginRequestString = Encoding.UTF8.GetString(packet).Substring(1);

            if (!loginRequestString.Contains('|') || packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid login request");
                return;
            }

            if(packet[1] != PacketBuilder.PACKET_A_TERMINATOR)
            {
                string[] loginParts = loginRequestString.Split('|');
                if (loginParts.Length < 3)
                {
                    Logger.ErrorPrint(sender.RemoteIp + " Sent a login request of invalid length. " + loginRequestString);
                    return;
                }

                int version = int.Parse(loginParts[0]);
                string encryptedUsername = loginParts[1];
                string encryptedPassword = loginParts[2];
                string username = Authentication.DecryptLogin(encryptedUsername);
                string password = Authentication.DecryptLogin(encryptedPassword);

                if (Authentication.CheckPassword(username, password))
                {
                    // Obtain user information
                    int userId = Database.GetUserid(username);
                    sender.Login(userId);

                    byte[] ResponsePacket = PacketBuilder.CreateLoginPacket(true);
                    sender.SendPacket(ResponsePacket);
                }
                else
                {
                    Logger.WarnPrint(sender.RemoteIp + " Attempted to login to: " + username + " with incorrect password " + password);
                    byte[] ResponsePacket = PacketBuilder.CreateLoginPacket(false);
                    sender.SendPacket(ResponsePacket);
                }
            }

        }

        public static void UpdateArea(Client forClient)
        {

            byte[] areaData = PacketBuilder.CreateAreaMessage(forClient.LoggedinUser.X, forClient.LoggedinUser.Y);
            forClient.SendPacket(areaData);

        }

        public static int GetNumberOfPlayers()
        {
            int count = 0;
            foreach(Client client in ConnectedClients)
            {
                if (client.LoggedIn)
                    count++;
            }
            return count;
        }
        public static void StartServer()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress hostIP = IPAddress.Parse(ConfigReader.BindIP);
            IPEndPoint ep = new IPEndPoint(hostIP, ConfigReader.Port);
            ServerSocket.Bind(ep);
            Logger.DebugPrint("Binding to ip: " + ConfigReader.BindIP + " On port: " + ConfigReader.Port.ToString());
            ServerSocket.Listen(10000);

            while(true)
            {
                Logger.DebugPrint("Waiting for new connections...");

                Socket cientSocket = ServerSocket.Accept();
                Client client = new Client(cientSocket);
                ConnectedClients.Add(client);
            }
        }
    }
}
