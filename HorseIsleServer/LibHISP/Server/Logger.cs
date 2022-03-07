using System;

namespace HISP.Server
{
    public class Logger
    {
        private static void defaultCallbackFunc(string txt)
        {
            return;
        }

        private static Action<string> logFunction = defaultCallbackFunc;


        public static void SetCallback(Action<string> callback)
        {
            logFunction = callback;
        }

        public static void ErrorPrint(string text)
        {
            if (ConfigReader.LogLevel >= 1)
                logFunction("[ERROR] " + text);
        }
        public static void WarnPrint(string text)
        {
            if (ConfigReader.LogLevel >= 2)
                logFunction("[WARN] " + text);
        }
        public static void HackerPrint(string text) 
        {
            if (ConfigReader.LogLevel >= 3)
                logFunction("[HACK] " + text);
        }
        public static void InfoPrint(string text)
        {
            if (ConfigReader.LogLevel >= 4)
                logFunction("[INFO] " + text);
        }
        public static void DebugPrint(string text)
        {
            if (ConfigReader.LogLevel >= 5)
                logFunction("[DEBUG] " + text);
        }
    }
}
