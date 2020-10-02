using Horse_Isle_Server.Properties;
using System;
using System.IO;

namespace Horse_Isle_Server
{

    class ConfigReader
    {
        public static int Port;
        public static string BindIP;

        public static string DatabaseIP;
        public static string DatabaseUsername;
        public static string DatabaseName;
        public static string DatabasePassword;
        public static int DatabasePort;

        public static string Motd;
        public static string MapFile;
        public static string GameDataFile;
        public static string CrossDomainPolicyFile;
        public static bool Debug;

        public static bool BadWords;
        public static bool DoCorrections;

        private static string ConfigurationFileName = "server.properties";
        public static void OpenConfig()
        {
            if (!File.Exists(ConfigurationFileName))
            {
                Logger.ErrorPrint(ConfigurationFileName+" not found! writing default.");
                File.WriteAllText(ConfigurationFileName,Resources.DefaultServerProperties);
            }

            string[] configFile = File.ReadAllLines(ConfigurationFileName);
            foreach (string setting in configFile)
            {
                /*
                 * Avoid crashing.
                 */
                if (setting.Length < 1)
                    continue;
                if (setting[0] == '#')
                    continue;
                if (!setting.Contains("="))
                    continue;

                string[] dataPair = setting.Split('=');

                string key = dataPair[0];
                string data = dataPair[1];
                /*
                 *  Parse configuration file
                 */

                switch (key)
                {
                    case "port":
                        Port = int.Parse(data);
                        break;
                    case "ip":
                        BindIP = data;
                        break;
                    case "db_ip":
                        DatabaseIP = data;
                        break;
                    case "db_username":
                        DatabaseUsername = data;
                        break;
                    case "db_password":
                        DatabasePassword = data;
                        break;
                    case "db_name":
                        DatabaseName = data;
                        break;
                    case "db_port":
                        DatabasePort = int.Parse(data);
                        break;
                    case "map":
                        MapFile = data;
                        break;
                    case "motd":
                        Motd = data;
                        break;
                    case "gamedata":
                        GameDataFile = data;
                        break;
                    case "crossdomain":
                        CrossDomainPolicyFile = data;
                        break;
                    case "enable_corrections":
                        BadWords = data == "true";
                        break;
                    case "enable_word_filter":
                        DoCorrections = data == "true";
                        break;
                    case "debug":
                        Debug = data == "true";
                        break;
                }



            }


        }

    }
}
