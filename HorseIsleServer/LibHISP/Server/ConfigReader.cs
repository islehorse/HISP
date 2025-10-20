using HISP.Properties;
using System.IO;

namespace HISP.Server
{

    public class ConfigReader
    {
        private static string configFilename = "server.properties";
        private static string mapFileName = "HI1.MAP";
        private static string gamedataDirname = "gamedata";
        private static string crossDomainPolicyFileName = "CrossDomainPolicy.xml";

        public static int Port = 12321;
        public static string BindIP = "0.0.0.0";

        public static string DatabaseIP = "127.0.0.1";
        public static string DatabaseName = "game1";
        public static string DatabaseUsername = "root";
        public static string DatabasePassword = "test123";

        public static int DatabasePort = 3306;
        public static int IntrestRate = 3333;
        public static string Motd = "April 11, 2020. New breed, Camarillo White Horse. Two new quests.";
        public static string SqlBackend = "mariadb";

        public static int LogLevel = 4;

        public static bool EnableSpamFilter = true;
        public static bool AllUsersSubbed = false;
        public static bool FixOfficalBugs = false;
        public static bool EnableSwearFilter = true;
        public static bool EnableCorrections = true;
        public static bool EnableNonViolations = true;

        public static bool EnableWebSocket = true;
        
        public static string ConfigDirectory = Directory.GetCurrentDirectory();
        public static string AssetsDirectory = Directory.GetCurrentDirectory();

        public static string MapFile
        {
            get
            {
                return Path.GetFullPath(mapFileName, AssetsDirectory);
            }
            set
            {
                if (value == null || value == "")
                    return;
                mapFileName = value;
            }
        }
        public static string GameData
        {
            get
            {
                return Path.GetFullPath(gamedataDirname, AssetsDirectory);
            }
            set
            {
                if (value == null || value == "")
                    return;
                gamedataDirname = value;
            }
        }
        public static string CrossDomainPolicyFile
        {
            get
            {
                return Path.GetFullPath(crossDomainPolicyFileName, ConfigDirectory);
            }
            set
            {
                if (value == null || value == "")
                    return;
                crossDomainPolicyFileName = value;
            }
        }
        public static string ConfigurationFileName
        {
            get 
            {
                return Path.GetFullPath(configFilename, ConfigDirectory);
            }
            set 
            {
                if (value == null || value == "")
                    return;
                configFilename = value; 
            }
        }
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
                        AllUsersSubbed = (data == "true");
                        break;
                    case "enable_corrections":
                        EnableCorrections = (data == "true");
                        break;
                    case "sql_backend":
                        SqlBackend = data;
                        break;
                    case "enable_non_violation_check":
                        EnableNonViolations = (data == "true");
                        break;
                    case "enable_spam_filter":
                        EnableSpamFilter = (data == "true");
                        break;
                    case "enable_websocket":
                        EnableWebSocket = (data == "true");
                        break;
                    case "fix_offical_bugs":
                        FixOfficalBugs = (data == "true");
                        break;
                    case "enable_word_filter":
                        EnableSwearFilter = (data == "true");
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
