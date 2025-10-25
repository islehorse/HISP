using System.IO;
using HISP.Server;
using HISP.Game;
using System.Text;

namespace HISP.Security
{
    public class SocketDomainPolicy
    {
        public static byte[] GetPolicyFile()
        {
            string policy = Messages.FormatCrossDomainPolicy("*", ConfigReader.Port);

            if (ConfigReader.SocketPolicyFile != "internal") {
                if (!File.Exists(ConfigReader.SocketPolicyFile)) {
                    File.WriteAllText(ConfigReader.SocketPolicyFile, policy);
                }

                policy =  File.ReadAllText(ConfigReader.SocketPolicyFile);
            }

            Logger.DebugPrint("Sending Socket Policy File: \"" + policy + "\"");
            return Encoding.UTF8.GetBytes(policy);
        }
    }
}
