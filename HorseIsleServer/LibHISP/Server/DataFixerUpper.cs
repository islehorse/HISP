using System;
using System.Linq;

namespace HISP.Server
{
    static class DataFixerUpper
    {

        private static UInt32 verToNum(string version)
        {
            UInt32 val = 0;

            string[] points = version.ToLower().Replace("v", "").Split('.');
            int pointsLocation = points.Length; 

            Array.Resize(ref points, 4);
            for (int i = pointsLocation; i < points.Length; i++)
                points[i] = "0";

            
            foreach (string point in points)
            {
                val <<= 8;
                val |= Byte.Parse(point);
            }

            return val;
        }
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
        }


        private static void fixupVersion2_2_4()
        {
            Database.TryExecuteSqlQuery("UPDATE DroppedItems SET X=X-4, Y=Y-1;");
            Database.TryExecuteSqlQuery("UPDATE NpcPos SET X=X-4, Y=Y-1;");
            Database.TryExecuteSqlQuery("UPDATE Treasure SET x=x-4, y=y-1;");
            Database.TryExecuteSqlQuery("UPDATE UserExt SET X=X-4, Y=Y-1;");
            Database.TryExecuteSqlQuery("UPDATE WildHorse SET x=x-4, y=y-1;");
        }

        private static void fixupVersion2_2_36()
        {
            Database.TryExecuteSqlQuery("DELETE FROM DroppedItems WHERE randomId IN (SELECT randomId FROM DroppedItems GROUP BY RandomId HAVING COUNT(*)>1);");
            Database.TryExecuteSqlQuery("DELETE FROM WildHorse WHERE randomId IN (SELECT randomId FROM WildHorse GROUP BY RandomId HAVING COUNT(*)>1);");
            Database.TryExecuteSqlQuery("DELETE FROM Inventory WHERE randomId IN (SELECT randomId FROM Inventory GROUP BY RandomId HAVING COUNT(*)>1);");
            Database.TryExecuteSqlQuery("DELETE FROM Horses WHERE randomId IN (SELECT randomId FROM Horses GROUP BY RandomId HAVING COUNT(*)>1);");   
        }


        public static void FixUpDb()
        {
            string lastVersionStr = Database.GetLastLoadedVersion();
            string currentVersionStr = ServerVersion.GetVersionString();

            lastVersionStr.Split('.');

            UInt32 lastVersion = verToNum(lastVersionStr);
            UInt32 currentVersion = verToNum(currentVersionStr);

            if (currentVersion > lastVersion)
            {
                Logger.WarnPrint("Migrating Database from " + lastVersionStr + " to " + currentVersionStr);
                if (lastVersion <= verToNum("v1.0")) fixupVersion1_0();
                if (lastVersion <= verToNum("v1.1")) fixupVersion1_1();
                if (lastVersion <= verToNum("v1.7.20")) fixupVersion1_7_20();
                if (lastVersion <= verToNum("v2.2.4")) fixupVersion2_2_4();
                if (lastVersion <= verToNum("v2.2.36")) fixupVersion2_2_36();
            }

            if (Database.GetTotalWorldEntries() != 1)
            {
                Database.TryExecuteSqlQuery("DELETE FROM World");
                Database.InitWorldData();
            }

            Database.SetLastLoadedVersion(currentVersionStr);


        }
    }
}
