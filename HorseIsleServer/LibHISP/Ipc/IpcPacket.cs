using HISP.Player;
using HISP.Security;
using HISP.Server;
using System;
using System.Linq;
using System.Text;

namespace HISP.Ipc
{
    internal class IpcPacket
    {
        public const byte PACKET_IPC = 0xFF;

        const byte IPC_CREATE_USER = 0xFE;
        const byte IPC_CHANGE_PASSWORD = 0xFD;
        const byte IPC_MAKE_ADMIN = 0xFC;
        const byte IPC_MAKE_MOD = 0xFB;


        public static void OnIpcReceived(GameClient sender, byte[] packet)
        {
            if (packet.Length < 1) return;
            string packetStr = Encoding.UTF8.GetString(packet);
            string dynamicInputStr = packetStr.Substring(1, packetStr.Length - 1 - PacketBuilder.PACKET_CLIENT_TERMINATOR_LENGTH);
            string[] args = dynamicInputStr.Split("|");

            string expectedSig = PacketSigning.Sign(string.Join("|", args.SkipLast(1)));
            string gotSig = args.Last();
            
            if(!expectedSig.Equals(gotSig, StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.HackerPrint(sender.RemoteIp + " tried to send an IPC request with the wrong signature.");
                return;
            }

            switch (packet[1])
            {
                case IPC_CREATE_USER:
                    if (args.Length < 5) return;
                    Authentication.CreateAccount(args[0], args[1], args[2], args[3] == "true", args[4] == "true");
                    break;
                case IPC_CHANGE_PASSWORD:
                    if (args.Length < 2) return;
                    Authentication.ChangePassword(args[0], args[1]);
                    break;
                case IPC_MAKE_ADMIN:
                    if (args.Length < 2) return;
                    if (!Database.CheckUserExist(args[0])) return;

                    int userId = Database.GetUserId(args[0]);
                    bool isAdmin = args[1] == "YES";
                    Database.SetUserAdmin(userId, isAdmin);

                    if (User.IsUserOnline(userId)) User.GetUserById(userId).Administrator = isAdmin;

                    break;
                case IPC_MAKE_MOD:
                    if (args.Length < 2) return;
                    if (!Database.CheckUserExist(args[0])) return;

                    userId = Database.GetUserId(args[0]);
                    bool isMod = args[1] == "YES";
                    Database.SetUserMod(userId, isMod);

                    if (User.IsUserOnline(userId)) User.GetUserById(userId).Moderator = isMod;

                    break;
            }

            sender.SendPacket(PacketBuilder.CreateLogin(true));

        }
    }
}
