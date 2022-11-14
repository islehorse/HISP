using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Tests
{
    public class ResultLogger
    {
        public static void LogTestStatus(bool successful, string testname, string message)
        {
            if (successful)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Out.Write("[ " + testname + " ] ");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine("* " + message + " *");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Error.Write("[ " + testname + " ] ");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("* " + message + " *");
            }
        }

        public static void LogTestResult(bool successful, string testname, string message, string expected)
        {
            if (successful)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Out.Write("[ " + testname + " ] ");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine("* " + message + " == " + expected + " *");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Error.Write("[ " + testname + " ] ");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("* " + message + " != " + expected + " *");
            }
        }
    }
}
