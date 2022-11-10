using System;

namespace HISP.Server
{
    public class Logger
    {
        private static void defaultCallbackFunc(bool error, string type, string text)
        {
            return;
        }

        private static Action<bool, string, string> logFunction = defaultCallbackFunc;


        public static void SetCallback(Action<bool, string, string> callback)
        {
            logFunction = callback;
        }

        public static void ErrorPrint(string text)
        {
            if (ConfigReader.LogLevel >= 1)
                logFunction(true, "ERROR", text);
        }
        public static void WarnPrint(string text)
        {
            if (ConfigReader.LogLevel >= 2)
                logFunction(false, "WARN", text);
        }
        public static void HackerPrint(string text) 
        {
            if (ConfigReader.LogLevel >= 3)
                logFunction(false, "HACK", text);
        }
        public static void InfoPrint(string text)
        {
            if (ConfigReader.LogLevel >= 4)
                logFunction(false, "INFO", text);
        }
        public static void DebugPrint(string text)
        {
            if (ConfigReader.LogLevel >= 5)
                logFunction(false, "DEBUG", text);
        }
        public static void CrashPrint(string text)
        {
            logFunction(true, "CRASH", text);
        }
    }
}
