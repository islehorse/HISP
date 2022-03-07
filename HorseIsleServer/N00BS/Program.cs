// An HTTP Server, and Horse Isle Server, Flash Player, in a single package
// Idea is to just be open and play.

using HISP.Security;
using HISP.Server;
using HTTP;
using System.Diagnostics;
using System.Reflection;

namespace HISP
{
    public static class Program
    {
        private static string baseDir;
        private static ContentServer cs;
        private static void addToList(string path)
        {
            string Name = path.Remove(0, Path.Combine(baseDir, "client").Length+1);
            Name = Name.Replace("\\", "/");

            ContentItem ci = new ContentItem(Name, path);
            cs.Contents.Add(ci);

        }


        public static void Main(string[] args)
        {
            baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ConfigReader.OpenConfig();
            if (File.Exists(ConfigReader.ConfigurationFileName))
                File.WriteAllText(ConfigReader.ConfigurationFileName, File.ReadAllText(ConfigReader.ConfigurationFileName).Replace("sql_lite=false", "sql_lite=true"));

            Database.OpenDatabase();
            if(Database.GetUsers().Length == 0)
            {
                Console.Write("Username: ");
                string username = Console.ReadLine();
                Console.Write("Password: ");
                string password = Console.ReadLine();
                Console.Write("Gender: ");
                string gender = Console.ReadLine();
                
                Random r = new Random();
                byte[] salt = new byte[64];

                r.NextBytes(salt);
                
                string saltText  = BitConverter.ToString(salt).Replace("-", "");

                string hashsalt = BitConverter.ToString(Authentication.HashAndSalt(password, salt)).Replace("-", "");

                Database.CreateUser(0, username, hashsalt, saltText, gender, true, true);
            }

            // Start Web Server
            cs = new ContentServer();
            string[] fileList = Directory.GetFiles(Path.Combine(baseDir, "client"), "*", SearchOption.AllDirectories);
            foreach (string file in fileList)
                addToList(file);

            // Start HI1 Server
            Logger.SetCallback(Console.WriteLine);
            Start.InitalizeAndStart();

            // Start Flash (Windows)
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(baseDir, "flash.dll");
            p.StartInfo.Arguments = "http://127.0.0.1:12515/horseisle.swf?SERVER=127.0.0.1&PORT=12321";
            p.Start();

            while (true) {  /* Allow asyncronous operations to happen. */ };

        }
    }
}
