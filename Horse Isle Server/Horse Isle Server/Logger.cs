using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Logger
    {
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
    }
}
