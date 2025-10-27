using HISP.Server;
using System;

namespace HISP.Tests.UnitTests
{
    public class ServerStartTest
    {

        public static void OnShutdown()
        {
            ResultLogger.LogTestStatus(false, "START_SERVER", "OnShutdown called");
        }
        public static void LogStdout(bool error, string type, string text)
        {
            if (type == "CRASH")
                ResultLogger.LogTestStatus(false, "START_SERVER", text);
        }

        public static bool RunServerStartTest()
        {
            try
            {
                Logger.SetCallback(LogStdout);
                Entry.SetShutdownCallback(OnShutdown);
                Entry.Start();
                ResultLogger.LogTestStatus(true, "START_SERVER_TEST", "Success.");
                return true;
            }
            catch (Exception e)
            {
                ResultLogger.LogTestStatus(false, "START_SERVER_TEST", e.Message);
                return false;
            }
        }
    }
}
