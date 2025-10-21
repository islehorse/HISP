using HISP.Server;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HISP.Security
{
    public class PacketSigning
    {
        private static string hispKeyFile = Path.GetFullPath(".hispHmacKey", ConfigReader.ConfigDirectory);
        private static byte[] signKey = new byte[0x14];
        public static byte[] SignKey
        {
            get
            {
                if (File.Exists(hispKeyFile))
                {
                    signKey = File.ReadAllBytes(hispKeyFile);
                }
                else
                {
                    GameServer.RandomNumberGenerator.NextBytes(signKey);
                    File.WriteAllBytes(hispKeyFile, signKey);
                }
                return signKey;
            }
        }
        public static string Sign(string message)
        {
            using (HMACSHA256 hmacSHA256 = new HMACSHA256(SignKey))
            {
                byte[] hash = hmacSHA256.ComputeHash(Encoding.UTF8.GetBytes(message));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

    }
}
