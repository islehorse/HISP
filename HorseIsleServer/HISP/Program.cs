using HISP.Server;
using System;

namespace HISP
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logger.SetCallback(Console.WriteLine);
            Start.InitalizeAndStart();

            while (true) {  /* Allow asyncronous operations to happen. */ };
        }
    }
}
