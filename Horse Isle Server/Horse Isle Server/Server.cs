using System;
using System.Collections.Generic;
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

        public static void OnLoginRequest(Client sender, byte[] packet)
        {
            string loginRequestString = Encoding.UTF8.GetString(packet).Substring(1);

            if (!loginRequestString.Contains('|'))
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid login request" + loginRequestString);
                return;
            }

            string[] loginParts = loginRequestString.Split('|');
            if(loginParts.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent a login request of invalid length. " + loginRequestString);
                return;
            }

            int version = int.Parse(loginParts[0]);
            string encryptedUsername = loginParts[1];
            string encryptedPassword = loginParts[2];
            string username = Authentication.DecryptLogin(encryptedUsername);
            string password = Authentication.DecryptLogin(encryptedPassword);

            if(Authentication.CheckPassword(username, password))
            {

            }
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
