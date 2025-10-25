using HISP.Properties;
using System;
using System.Collections;
using System.IO;

namespace HISP.Server
{

    public class ConfigReader
    {
        private static string configFilename = "server.properties";
        private static string mapFileName = "HI1.MAP";
        private static string gamedataDirname = "gamedata";
        private static string socketPolicyFileName = "internal";
        private static string configPath = Directory.GetCurrentDirectory();
        private static string assetsPath = Directory.GetCurrentDirectory();

        public static short Port = 12321;
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

        public static bool EnableSocketPolicyServer = false;

        public static bool EnableWebSocket = true;
        public static bool SigninAsSignup = true;

        public static string ConfigDirectory
        {
            get
            {
                return configPath;
            }
            set
            {
                if (value == null || value == "")
                    return;
                configPath = value;
            }
        }
        public static string AssetsDirectory
        {
            get
            {
                return assetsPath;
            }
            set
            {
                if (value == null || value == "")
                    return;
                assetsPath = value;
            }
        }
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
        public static string SocketPolicyFile
        {
            get
            {
                if (socketPolicyFileName != "internal")
                    return Path.GetFullPath(socketPolicyFileName, ConfigDirectory);
                return socketPolicyFileName;
            }
            set
            {
                if (value == null || value == "")
                    return;
                socketPolicyFileName = value;
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

        private static void readConfigKey(string key, string value)
        {
            switch (key.ToLowerInvariant())
            {
                case "port":
                    Port = short.Parse(value);
                    break;
                case "ip":
                    BindIP = value;
                    break;
                case "db_ip":
                    DatabaseIP = value;
                    break;
                case "db_username":
                    DatabaseUsername = value;
                    break;
                case "db_password":
                    DatabasePassword = value;
                    break;
                case "db_name":
                    DatabaseName = value;
                    break;
                case "db_port":
                    DatabasePort = int.Parse(value);
                    break;
                case "map":
                    MapFile = value;
                    break;
                case "motd":
                    Motd = value;
                    break;
                case "gamedata":
                    GameData = value;
                    break;
                case "socket_policy_file":
                    SocketPolicyFile = value;
                    break;
                case "all_users_subscribed":
                    AllUsersSubbed = (value == "true");
                    break;
                case "enable_corrections":
                    EnableCorrections = (value == "true");
                    break;
                case "sql_backend":
                    SqlBackend = value;
                    break;
                case "enable_policy_server":
                    EnableSocketPolicyServer = (value == "true");
                    break;
                case "enable_non_violation_check":
                    EnableNonViolations = (value == "true");
                    break;
                case "enable_spam_filter":
                    EnableSpamFilter = (value == "true");
                    break;
                case "enable_websocket":
                    EnableWebSocket = (value == "true");
                    break;
                case "signin_as_signup":
                    SigninAsSignup = (value == "true");
                    break;
                case "fix_offical_bugs":
                    FixOfficalBugs = (value == "true");
                    break;
                case "enable_word_filter":
                    EnableSwearFilter = (value == "true");
                    break;
                case "intrest_rate":
                    IntrestRate = int.Parse(value);
                    break;
                case "log_level":
                    LogLevel = int.Parse(value);
                    break;
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

                readConfigKey(key, data);

            }
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                const string prefix = "HISP_";
                if (entry.Key is null) continue;
                string eKey = (entry.Key as string).ToUpperInvariant();
                string eValue = (entry.Value as string);

                if (eKey.StartsWith(prefix))
                {
                    Logger.WarnPrint("Ignoring setting: " + prefix.ToLowerInvariant() + " because environment variable " + eKey + " is set.");
                    readConfigKey(eKey.Substring(prefix.Length), eValue);
                }
            }
        }
    }
}
