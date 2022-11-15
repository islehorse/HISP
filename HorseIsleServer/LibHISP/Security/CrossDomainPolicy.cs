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
                

            MemoryStream ms = new MemoryStream();
            byte[] policyFileBytes = File.ReadAllBytes(ConfigReader.CrossDomainPolicyFile);
            ms.Write(policyFileBytes, 0x00, policyFileBytes.Length);
            ms.WriteByte(0x00);

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] policyFileData = ms.ToArray();
            ms.Close();

            return policyFileData;
        }
    }
}
