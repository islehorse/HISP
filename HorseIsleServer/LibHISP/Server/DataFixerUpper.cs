using System;

namespace HISP.Server
{
    static class DataFixerUpper
    {
        private static void fixupVersion1_0()
        {
            // Add total logins column to UserExt
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt ADD COLUMN TotalLogins INT;");
            Database.TryExecuteSqlQuery("UPDATE UserExt SET TotalLogins=0;");

            // Add New column to OnlineUsers.
            Database.TryExecuteSqlQuery("ALTER TABLE OnlineUsers ADD COLUMN New TEXT(3);");
            Database.TryExecuteSqlQuery("UPDATE OnlineUsers SET New=\"NO\";");

            // Alter sizes
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt CHANGE COLUMN ProfilePage ProfilePage TEXT(4000);");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt CHANGE COLUMN PrivateNotes PrivateNotes TEXT(65535);");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox CHANGE COLUMN Subject Subject TEXT(100);");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox CHANGE COLUMN Message Message TEXT(65535);");
            Database.TryExecuteSqlQuery("ALTER TABLE Horses CHANGE COLUMN description description TEXT(4000);");
            Database.TryExecuteSqlQuery("ALTER TABLE WildHorse CHANGE COLUMN description description TEXT(4000);");
            Database.TryExecuteSqlQuery("ALTER TABLE Ranches CHANGE COLUMN title title TEXT(50);");
            Database.TryExecuteSqlQuery("ALTER TABLE Ranches CHANGE COLUMN description description TEXT(250);");
        }

        private static void fixupVersion1_1()
        {
            // Add data column to shop to ShopInventory
            Database.TryExecuteSqlQuery("ALTER TABLE ShopInventory ADD COLUMN Data INT;");
            Database.TryExecuteSqlQuery("UPDATE ShopInventory SET Data=0;");
        }

        private static void fixupVersion1_7_20()
        {
            // Add LastLoadedInVersion column to World.
            Database.TryExecuteSqlQuery("ALTER TABLE World ADD COLUMN LastLoadedInVersion TEXT(64)");
            Database.TryExecuteSqlQuery("UPDATE World SET LastLoadedInVersion=\"v1.7.20\";");
        }

        public static void FixUpDb()
        {
            string lastVersionStr = Database.GetLastLoadedVersion();
            string currentVersionStr = ServerVersion.GetVersionString();

            int lastVersion = Convert.ToInt32(Int32.Parse(lastVersionStr.ToLower().Replace("v", "").Replace(".", "")));
            int currentVersion = Convert.ToInt32(Int32.Parse(currentVersionStr.ToLower().Replace("v", "").Replace(".", "")));

            if(currentVersion > lastVersion)
            {
                Logger.WarnPrint("Migrating Database from " + lastVersionStr + " to " + currentVersionStr);

                if (lastVersion <= 10) fixupVersion1_0();
                if (lastVersion <= 11) fixupVersion1_1();
                if (lastVersion <= 1720) fixupVersion1_7_20();

                Database.SetLastLoadedVersion(ServerVersion.GetVersionString());
            }
        }
    }
}
