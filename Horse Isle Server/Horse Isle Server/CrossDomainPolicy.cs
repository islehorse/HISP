using Horse_Isle_Server.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class CrossDomainPolicy
    {
        public static byte[] GetPolicy()
        {
            if (!File.Exists(ConfigReader.CrossDomainPolicyFile)) {
                if (ConfigReader.Debug)
                    Console.WriteLine("[DEBUG] Cross-Domain-Policy file not found, using default");
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
