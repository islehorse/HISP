using HISP.Properties;
using System.IO;
using HISP.Server;
namespace HISP.Security
{
    public class CrossDomainPolicy
    {
        public static byte[] GetPolicyFile()
        {
            if (!File.Exists(ConfigReader.CrossDomainPolicyFile)) {
                Logger.InfoPrint("Cross-Domain-Policy file not found, using default");
                File.WriteAllText(ConfigReader.CrossDomainPolicyFile, Resources.DefaultCrossDomain);
            }
            byte[] policyFileBytes = File.ReadAllBytes(ConfigReader.CrossDomainPolicyFile);
            return policyFileBytes;
        }
    }
}
