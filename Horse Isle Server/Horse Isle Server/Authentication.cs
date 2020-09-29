using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Authentication
    {
        public static string DecryptLogin(string encpass)
        {
            string decrypt = "";
            string ROTPOOL = "bl7Jgk61IZdnY mfDN5zjM2XLqTCty4WSEoKR3BFVQsaUhHOAx0rPwp9uc8iGve";
            string POSPOOL = "DQc3uxiGsKZatMmOS5qYveN71zoPTk8yU0H2w9VjprBXWn l4FJd6IRbhgACfEL";
            string ROTPOOL2 = "evGi8cu9pwPr0xAOHhUasQVFB3RKoESW4ytCTqLX2Mjz5NDfm YndZI16kgJ7lb";


            int i = 0;
            int ii = 0;
            while (i < encpass.Length)
            {
                int ROT = ROTPOOL.IndexOf(encpass[i].ToString());
                int POS = POSPOOL.IndexOf(encpass[i + 1].ToString());
                POS -= (ROT + ii);
                if (POS < 0)
                {
                    POS = (POS / -1) - 1;

                    while (POS >= ROTPOOL.Length)
                    {
                        POS -= ROTPOOL.Length;
                    }

                    decrypt += ROTPOOL2[POS];
                }
                else
                {
                    while (POS >= ROTPOOL.Length)
                    {
                        POS -= ROTPOOL.Length;
                    }

                    decrypt += ROTPOOL[POS];
                }

                i += 2;
                ii += 1;
            }
            return decrypt.Replace(" ", "");
        }



        public static byte[] HashAndSalt(string plaintext, byte[] salt)
        {
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            SHA512 sha512 = SHA512.Create();
            sha512.Initialize();
            byte[] hash = sha512.TransformFinalBlock(plaintextBytes, 0x00, plaintextBytes.Length);

            for (int i = 0; i < hash.Length; i++)
            {
                hash[i] ^= salt[i];
            }

            sha512 = SHA512.Create();
            sha512.Initialize();
            byte[] finalHash = sha512.TransformFinalBlock(hash, 0x00, hash.Length);

            return finalHash;
        }

        public static bool CheckPassword(string username, string password)
        {

            try
            {
                byte[] expectedPassword = Database.GetPasswordHash(username);
                byte[] salt = Database.GetPasswordSalt(username);
                byte[] hashedPassword = HashAndSalt(password, salt);

                if (Enumerable.SequenceEqual(expectedPassword, hashedPassword))
                    return true;
                else
                    return false;
            }
            catch(KeyNotFoundException e)
            {
                Logger.DebugPrint(e.Message);
                return false;
            }
        }

    }
}
