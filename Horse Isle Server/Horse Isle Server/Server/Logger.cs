using System;

namespace HISP.Server
{
    class Logger
    {
        public static void HackerPrint(string text) // When someone is obviously cheating.
        {
            Console.WriteLine("[HACK] " + text);
        }
        public static void DebugPrint(string text)
        {
            if (ConfigReader.Debug)
                Console.WriteLine("[DEBUG] " + text);   
        }
        public static void WarnPrint(string text)
        {
            Console.WriteLine("[WARN] " + text);
        }
        public static void ErrorPrint(string text) 
        {
            Console.WriteLine("[ERROR] " + text);
        }
        public static void InfoPrint(string text)
        {
            Console.WriteLine("[INFO] " + text);
        }
    }
}
