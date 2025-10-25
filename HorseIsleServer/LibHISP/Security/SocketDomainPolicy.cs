using System.IO;
using System.Text;
using HISP.Server;
using HISP.Game;

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
