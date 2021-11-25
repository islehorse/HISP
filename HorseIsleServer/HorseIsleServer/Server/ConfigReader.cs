using HISP.Properties;
using System.IO;

namespace HISP.Server
{

    public class ConfigReader
    {
        public static int Port;
        public static string BindIP = "0.0.0.0";

        public static string DatabaseIP;
        public static string DatabaseUsername;
        public static string DatabaseName;
        public static string DatabasePassword;
        public static int DatabasePort;
        public static int IntrestRate;
        public static string Motd;
        public static string MapFile;
        public static string GameDataFile;
        public static string CrossDomainPolicyFile;

        public static int LogLevel = 0;
        public static bool EnableSpamFilter = true;
        public static bool AllUsersSubbed = false;
        public static bool AllowBbcode = false;
        public static bool BadWords = true;
        public static bool DoCorrections = true;
        public static bool DoNonViolations = true;

        public const int MAX_STACK = 50;

        private static string ConfigurationFileName = "server.properties";
        public static void OpenConfig()
        {
            if (!File.Exists(ConfigurationFileName))
            {
                Logger.WarnPrint(ConfigurationFileName+" not found! writing default.");
                File.WriteAllText(ConfigurationFileName,Resources.DefaultServerProperties);
                Logger.InfoPrint("! Its very likely database connection will fail...");
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
                    case "all_users_subscribed":
                        AllUsersSubbed = data == "true";
                        break;
                    case "enable_corrections":
                        DoCorrections = data == "true";
                        break;
                    case "non_violation":
                        DoNonViolations = data == "true";
                        break;
                    case "enable_spam_filter":
                        EnableSpamFilter = data == "true";
                        break;
                    case "allow_bbcode_in_chat":
                        AllowBbcode = data == "true";
                        break;
                    case "enable_word_filter":
                        BadWords = data == "true";
                        break;
                    case "intrest_rate":
                        IntrestRate = int.Parse(data);
                        break;
                    case "log_level":
                        LogLevel = int.Parse(data);
                        break;
                }



            }


        }

    }
}
