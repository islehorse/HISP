using HISP.Server;
using System;

namespace HISP
{
    public static class Program
    {
        public static bool ShuttingDown = false;
        public static void OnShutdown()
        {
            ShuttingDown = true;
        }
        public static void Main(string[] args)
        {
            Logger.SetCallback(Console.WriteLine);
            Entry.SetShutdownCallback(OnShutdown);
            Entry.Start();

            while (!ShuttingDown) {  /* Allow asyncronous operations to happen. */ };
        }
    }
}
