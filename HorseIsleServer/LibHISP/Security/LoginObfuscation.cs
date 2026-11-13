using HISP.Server;
using HISP.Util;


// The horse isle client does some weird obfuscation to incoming user/pass
// this is mostly security-through-obscurity, but we have to play along
//
// Reverse Engineered from horseisle.swf.

namespace HISP.Security
{
    public class LoginObfuscation
    {

        private const string ROT_POOL = "bl7Jgk61IZdnY mfDN5zjM2XLqTCty4WSEoKR3BFVQsaUhHOAx0rPwp9uc8iGve";
        private const string POS_POOL = "DQc3uxiGsKZatMmOS5qYveN71zoPTk8yU0H2w9VjprBXWn l4FJd6IRbhgACfEL";


        public static string ObfuscateLogin(string plainpass)
        {
            string obfus = "";

            for(int i = 0; i < plainpass.Length; i++)
            {
                int rotation = GameServer.RandomNumberGenerator.Next(0, ROT_POOL.Length);
                int position = ROT_POOL.IndexOf(plainpass[i]);

                position = position + (rotation + i);
                while (position >= ROT_POOL.Length)
                    position -= ROT_POOL.Length;
                obfus += ROT_POOL[rotation];
                obfus += POS_POOL[position];

            }
            return obfus;

        }
        public static string DeobfuscateLogin(string encpass)
        {
            string deobfus = "";

            int rotationCycle = 0;
            for (int i = 0; i < encpass.Length; i += 2)
            {
                int rotation = ROT_POOL.IndexOf(encpass[i].ToString());
                int position = POS_POOL.IndexOf(encpass[i + 1].ToString());
                position -= (rotation + rotationCycle);
                if (position < 0)
                {
                    position = (position / -1) - 1;

                    while (position >= ROT_POOL.Length)
                        position -= ROT_POOL.Length;

                    deobfus += Helper.ReverseString(ROT_POOL)[position];
                }
                else
                {
                    while (position >= ROT_POOL.Length)
                        position -= ROT_POOL.Length;

                    deobfus += ROT_POOL[position];
                }

                rotationCycle++;
            }
            return deobfus.Replace(" ", "");
        }
    }
}
