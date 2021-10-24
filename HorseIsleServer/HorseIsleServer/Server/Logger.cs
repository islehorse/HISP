using System;

namespace HISP.Server
{
    public class Logger
    {
        public static void HackerPrint(string text) // When someone is obviously cheating.
        {
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[HACK] " + text);
            Console.ForegroundColor = prevColor;
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
