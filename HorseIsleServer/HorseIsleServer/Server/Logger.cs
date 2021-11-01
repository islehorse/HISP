using System;

namespace HISP.Server
{
    public class Logger
    {
        public static void ErrorPrint(string text)
        {
            if (ConfigReader.LogLevel >= 1)
                Console.WriteLine("[ERROR] " + text);
        }
        public static void WarnPrint(string text)
        {
            if (ConfigReader.LogLevel >= 2)
                Console.WriteLine("[WARN] " + text);
        }
        public static void HackerPrint(string text) // When someone is obviously cheating.
        {
            if (ConfigReader.LogLevel >= 3)
            {
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[HACK] " + text);
                Console.ForegroundColor = prevColor;
            }
        }
        public static void InfoPrint(string text)
        {
            if (ConfigReader.LogLevel >= 4)
                Console.WriteLine("[INFO] " + text);
        }
        public static void DebugPrint(string text)
        {
            if (ConfigReader.LogLevel >= 5)
                Console.WriteLine("[DEBUG] " + text);
        }
    }
}
