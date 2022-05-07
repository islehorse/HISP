using HISP.Properties;
using System.IO;

namespace HISP.Server
{

    public class ConfigReader
    {
        public static int Port = 12321;
        public static string BindIP = "0.0.0.0";

        public static string DatabaseIP = "127.0.0.1";
        public static string DatabaseName = "game1";
        public static string DatabaseUsername = "root";
        public static string DatabasePassword = "test123";

        public static int DatabasePort = 3306;
        public static int IntrestRate = 3333;
        public static string Motd = "April 11, 2020. New breed, Camarillo White Horse. Two new quests.";
        public static string MapFile = "HI1.MAP";
        public static string GameData = "gamedata.json";
        public static string CrossDomainPolicyFile = "CrossDomainPolicy.xml";

        public static string ModsFolder = "mods";
        public static int LogLevel = 4;

        public static bool SqlLite = false;
        public static bool EnableSpamFilter = true;
        public static bool AllUsersSubbed = false;
        public static bool FixOfficalBugs = false;
        public static bool BadWords = true;
        public static bool DoCorrections = true;
        public static bool DoNonViolations = true;

        public static string ConfigurationFileName = "server.properties";
        public static void OpenConfig()
        {
            if (!File.Exists(ConfigurationFileName))
            {
                Logger.WarnPrint(ConfigurationFileName+" not found! writing default.");
                File.WriteAllText(ConfigurationFileName, Resources.DefaultServerProperties);
                Logger.WarnPrint("! Its very likely database connection will fail...");
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
                        GameData = data;
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
                    case "sql_lite":
                        SqlLite = data == "true";
                        break;
                    case "enable_non_violation_check":
                        DoNonViolations = data == "true";
                        break;
                    case "enable_spam_filter":
                        EnableSpamFilter = data == "true";
                        break;
                    case "fix_offical_bugs":
                        FixOfficalBugs = data == "true";
                        break;
                    case "enable_word_filter":
                        BadWords = data == "true";
                        break;
                    case "mods_folder":
                        ModsFolder = data;
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
