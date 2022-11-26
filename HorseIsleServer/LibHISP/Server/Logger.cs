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

        private static void log(bool error, string type, string text)
        {   
            string[] msgs = text.Replace("\r", "").Split("\n");
            foreach(string msg in msgs)
            {
                logFunction(error, type, msg);
            }
        }

        public static void SetCallback(Action<bool, string, string> callback)
        {
            logFunction = callback;
        }

        public static void ErrorPrint(string text)
        {
            if (ConfigReader.LogLevel >= 1)
                log(true, "ERROR", text);
        }
        public static void WarnPrint(string text)
        {
            if (ConfigReader.LogLevel >= 2)
                log(false, "WARN", text);
        }
        public static void HackerPrint(string text) 
        {
            if (ConfigReader.LogLevel >= 3)
                log(false, "HACK", text);
        }
        public static void InfoPrint(string text)
        {
            if (ConfigReader.LogLevel >= 4)
                log(false, "INFO", text);
        }
        public static void DebugPrint(string text)
        {
            if (ConfigReader.LogLevel >= 5)
                log(false, "DEBUG", text);
        }
        public static void CrashPrint(string text)
        {
            log(true, "CRASH", text);
        }
    }
}
