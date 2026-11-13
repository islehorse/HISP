
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

using HISP.Server;

namespace HISP.Security
{

    // Handles Hash and Salt calculation for Login Authentication.
    public class Authentication
    {

        public static byte[] Sha1Digest(byte[] message)
        {
            using (SHA1 sha1 = SHA1.Create())
                return sha1.ComputeHash(message);
        }


        public static byte[] Sha512Digest(byte[] message)
        {
            using (SHA512 sha512 = SHA512.Create())
                return sha512.ComputeHash(message);
        }

        public static byte[] XorBytes(byte[] plaintext, byte[] key)
        {
            int length = Math.Min(plaintext.Length, key.Length);

            byte[] ciphertext = new byte[length];
            for(int i = 0; i < length; i++)
                ciphertext[i] = Convert.ToByte(plaintext[i] ^ key[i]);

            return ciphertext;
        }

        public static byte[] HashAndSalt(string plaintext, byte[] salt)
        {
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            byte[] hash = Sha512Digest(plaintextBytes);
            byte[] saltedHash = XorBytes(hash, salt);
            byte[] finalHash = Sha512Digest(saltedHash);

            return finalHash;
        }

        public static bool CheckPassword(string username, string password)
        {
            if(Database.CheckUserExist(username))
            {
                byte[] expectedPassword = Database.GetPasswordHash(username);
                byte[] salt = Database.GetPasswordSalt(username);
                byte[] hashedPassword = HashAndSalt(password, salt);

                if (Enumerable.SequenceEqual(expectedPassword, hashedPassword)) return true;
            }
            else if(ConfigReader.SigninAsSignup)
            {
                CreateAccount(username, password, "FEMALE", false, false);
                return true;
            }
            return false;
        }

        public static bool ChangePassword(string username, string newPassword)
        {
            if(Database.CheckUserExist(username))
            {
                byte[] salt = Database.GetPasswordSalt(username);
                byte[] hashsalt = Authentication.HashAndSalt(newPassword, salt);
                Database.SetPasswordHash(username, BitConverter.ToString(hashsalt).Replace("-", ""));
                return true;
            }
            return false;
        }
        public static int CreateAccount(string username, string password, string gender, bool admin, bool moderator)
        {
            // Get a free user id
            int userId = Database.GetNextFreeUserId();

            // Generate salt value
            byte[] salt = new byte[64];
            GameServer.RandomNumberGenerator.NextBytes(salt);
            string saltText = BitConverter.ToString(salt).Replace("-", "");
            string hashsalt = BitConverter.ToString(Authentication.HashAndSalt(password, salt)).Replace("-", "");

            // Add user to database
            Database.CreateUser(userId, username, hashsalt, saltText, gender, admin, moderator);

            return userId;
        }


    }
}
