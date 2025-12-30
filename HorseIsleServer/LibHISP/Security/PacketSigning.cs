using HISP.Server;
using HISP.Util;
using System;
using System.Security.Cryptography;
using System.Text;

namespace HISP.Security
{
    public class PacketSigning
    {
        private static byte[] signKey = new byte[0x14];
        public static byte[] SignKey
        {
            get
            {
                return GenerateHmacKey();
            }
        }

        public static void Init()
        {
            _ = GenerateHmacKey();
        }

        public static byte[] GenerateHmacKey()
        {
            string hmacKeyEnv = Environment.GetEnvironmentVariable("HISP_HMAC_KEY");

            if (hmacKeyEnv is not null)
            {
                signKey = Helper.StringToByteArray(hmacKeyEnv);
            }
            else
            {
                GameServer.RandomNumberGenerator.NextBytes(signKey);
                string key = BitConverter.ToString(signKey).Replace("-", "");
                Environment.SetEnvironmentVariable("HISP_HMAC_KEY", key);
            }
            return signKey;
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
