using HISP.Server;
using System;

// Some "unsafe" packets have a custom random number generator attached
// if the server and client generate the same random number, the request is authenticated
// this is security through obscurity, does nothing useful
// technically could ignore it, but implemented it anyway
// 
// its used for minigame rewards and such, can give any item in the game


namespace HISP.Security
{
    public class SecCode
    {
        public const char SEED_START_BYTE = '!';
        public const char SEED_END_BYTE = '\\';

        public byte[] Seed = new byte[3];
        public int Inc = 0;
        private int count = 0;

        
        public SecCode(byte[] seeds, int inc) {
            Array.Copy(seeds, Seed, Seed.Length);
            this.Inc = inc;
        }
        public SecCode()
        {
            Regenerate();
        }

        // regenerate SecCode Seeds, and SecCodeInc
        public void Regenerate()
        {
            Seed[0] = (byte)GameServer.RandomNumberGenerator.Next(SEED_START_BYTE, SEED_END_BYTE);
            Seed[1] = (byte)GameServer.RandomNumberGenerator.Next(SEED_START_BYTE, SEED_END_BYTE);
            Seed[2] = (byte)GameServer.RandomNumberGenerator.Next(SEED_START_BYTE, SEED_END_BYTE);
            Inc = (byte)GameServer.RandomNumberGenerator.Next(SEED_START_BYTE, SEED_END_BYTE);
        }


        // get the next sec code
        public byte[] Next
        {
            get
            {
                count = ((count + 1) % 3);

                Seed[count] = (byte)(Seed[count] + Inc);
                Seed[count] = (byte)(Seed[count] % SEED_END_BYTE);

                double i = Seed[0] + Seed[1] * Seed[2] - Seed[1];
                i = Math.Abs(i);
                i %= SEED_END_BYTE;

                byte[] nextSecCode = new byte[4];

                nextSecCode[0] = (byte)(Seed[0] + SEED_START_BYTE);
                nextSecCode[1] = (byte)(Seed[1] + SEED_START_BYTE);
                nextSecCode[2] = (byte)(Seed[2] + SEED_START_BYTE);
                nextSecCode[3] = (byte)(i + SEED_START_BYTE);

                return nextSecCode;
            }

        }

    }
}
