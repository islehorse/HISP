using HISP.Game;
using System;

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

        private static void fixupVersion2_5_6()
        {
            Database.TryExecuteSqlQuery("ALTER TABLE Mailbox RENAME COLUMN RandomId TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Inventory RENAME COLUMN RandomID TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE ShopInventory RENAME COLUMN RandomID TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE DroppedItems RENAME COLUMN RandomID TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Horses RENAME COLUMN randomId TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE WildHorse RENAME COLUMN randomId TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Treasure RENAME COLUMN randomId TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Auctions RENAME COLUMN randomId TO uniqueId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Auctions RENAME COLUMN horseRandomId TO horseUniqueId;");
        }

        private static void fixupVersion2_5_26()
        {
            // rename tables
            Database.TryExecuteSqlQuery("ALTER TABLE Inventory RENAME COLUMN PlayerID TO playerId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Inventory RENAME COLUMN ItemID TO itemId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Inventory RENAME COLUMN Data TO data;");

            Database.TryExecuteSqlQuery("ALTER TABLE ShopInventory RENAME COLUMN ShopId TO shopId;");
            Database.TryExecuteSqlQuery("ALTER TABLE ShopInventory RENAME COLUMN ItemID TO itemId;");
            Database.TryExecuteSqlQuery("ALTER TABLE ShopInventory RENAME COLUMN Data TO data;");

            Database.TryExecuteSqlQuery("ALTER TABLE DroppedItems RENAME COLUMN ItemID TO itemId;");
            Database.TryExecuteSqlQuery("ALTER TABLE DroppedItems RENAME COLUMN DespawnTimer TO despawnTimer;");
            Database.TryExecuteSqlQuery("ALTER TABLE DroppedItems RENAME COLUMN Data TO data;");

            Database.TryExecuteSqlQuery("ALTER TABLE MessageQueue RENAME COLUMN Id TO playerId;");
            Database.TryExecuteSqlQuery("ALTER TABLE MessageQueue RENAME COLUMN Message TO message;");

            Database.TryExecuteSqlQuery("ALTER TABLE Weather RENAME COLUMN Area TO area;");
            Database.TryExecuteSqlQuery("ALTER TABLE Weather RENAME COLUMN Weather TO weather;");

            Database.TryExecuteSqlQuery("ALTER TABLE AbuseReports RENAME COLUMN ReportCreator TO reportCreator;");
            Database.TryExecuteSqlQuery("ALTER TABLE AbuseReports RENAME COLUMN Reporting TO reporting;");
            Database.TryExecuteSqlQuery("ALTER TABLE AbuseReports RENAME COLUMN ReportReason TO reportReason;");

            Database.TryExecuteSqlQuery("ALTER TABLE NpcPos RENAME COLUMN UdlrPointer TO udlrPointer;");

            Database.TryExecuteSqlQuery("ALTER TABLE SavedDrawings RENAME COLUMN Drawing1 TO drawing1;");
            Database.TryExecuteSqlQuery("ALTER TABLE SavedDrawings RENAME COLUMN Drawing2 TO drawing2;");
            Database.TryExecuteSqlQuery("ALTER TABLE SavedDrawings RENAME COLUMN Drawing3 TO drawing3;");

            Database.TryExecuteSqlQuery("ALTER TABLE DrawingRooms RENAME COLUMN Drawing TO drawing;");

            Database.TryExecuteSqlQuery("ALTER TABLE OnlineUsers RENAME COLUMN Admin TO admin;");
            Database.TryExecuteSqlQuery("ALTER TABLE OnlineUsers RENAME COLUMN Moderator TO moderator;");
            Database.TryExecuteSqlQuery("ALTER TABLE OnlineUsers RENAME COLUMN Subscribed TO subscribed;");
            Database.TryExecuteSqlQuery("ALTER TABLE OnlineUsers RENAME COLUMN New TO new;");

            Database.TryExecuteSqlQuery("ALTER TABLE Users RENAME COLUMN Id TO playerId;");
            Database.TryExecuteSqlQuery("ALTER TABLE Users RENAME COLUMN Username TO username;");
            Database.TryExecuteSqlQuery("ALTER TABLE Users RENAME COLUMN PassHash TO passHash;");
            Database.TryExecuteSqlQuery("ALTER TABLE Users RENAME COLUMN Salt TO salt;");
            Database.TryExecuteSqlQuery("ALTER TABLE Users RENAME COLUMN Gender TO gender;");
            Database.TryExecuteSqlQuery("ALTER TABLE Users RENAME COLUMN Admin TO admin;");
            Database.TryExecuteSqlQuery("ALTER TABLE Users RENAME COLUMN Moderator TO moderator;");

            Database.TryExecuteSqlQuery("ALTER TABLE BuddyList RENAME COLUMN Id TO sendPlayerId;");
            Database.TryExecuteSqlQuery("ALTER TABLE BuddyList RENAME COLUMN IdFriend TO recvPlayerId;");

            Database.TryExecuteSqlQuery("ALTER TABLE MailBox RENAME COLUMN IdTo TO toPlayerId;");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox RENAME COLUMN IdFrom TO fromPlayerId;");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox RENAME COLUMN Subject TO subject;");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox RENAME COLUMN Message TO message;");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox RENAME COLUMN TimeSent TO timeSent;");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox RENAME COLUMN BeenRead TO beenRead;");

            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN Id TO playerId;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN LastLogin TO lastLogin;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN Money TO money;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN QuestPoints TO questPoints;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN BankBalance TO bankBalance;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN BankInterest TO bankInterest;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN ProfilePage TO profilePage;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN IpAddress TO ipAddress;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN PrivateNotes TO privateNotes;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN CharId TO charId;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN ChatViolations TO chatViolations;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN Subscriber TO subscriber;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN SubscribedUntil TO subscribedUntil;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN Experience TO experience;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN Tiredness TO tiredness;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN Hunger TO hunger;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN Thirst TO thirst;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN FreeMinutes TO freeMinutes;");
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt RENAME COLUMN TotalLogins TO totalLogins;");


            Database.TryExecuteSqlQuery("ALTER TABLE World RENAME COLUMN Time TO time;");
            Database.TryExecuteSqlQuery("ALTER TABLE World RENAME COLUMN Day TO day;");
            Database.TryExecuteSqlQuery("ALTER TABLE World RENAME COLUMN Year TO year;");
            Database.TryExecuteSqlQuery("ALTER TABLE World RENAME COLUMN StartTime TO startTime;");
            Database.TryExecuteSqlQuery("ALTER TABLE World RENAME COLUMN LastLoadedInVersion TO lastLoadedInVersion;");


            // add primary keys ...
            Database.TryExecuteSqlQuery("ALTER TABLE Users ADD PRIMARY KEY (playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Mailbox ADD PRIMARY KEY (uniqueId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Inventory ADD PRIMARY KEY (uniqueId);");
            Database.TryExecuteSqlQuery("ALTER TABLE ShopInventory ADD PRIMARY KEY (uniqueId);");
            Database.TryExecuteSqlQuery("ALTER TABLE DroppedItems ADD PRIMARY KEY (uniqueId);");
            Database.TryExecuteSqlQuery("ALTER TABLE DrawingRooms ADD PRIMARY KEY (roomId);");
            Database.TryExecuteSqlQuery("ALTER TABLE NpcPos ADD PRIMARY KEY (npcId);");
            Database.TryExecuteSqlQuery("ALTER TABLE PoetryRooms ADD PRIMARY KEY (poetId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Horses ADD PRIMARY KEY (uniqueId);");
            Database.TryExecuteSqlQuery("ALTER TABLE WildHorse ADD PRIMARY KEY (uniqueId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Treasure ADD PRIMARY KEY (uniqueId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Ranches ADD PRIMARY KEY (ranchId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Auctions ADD PRIMARY KEY (uniqueId);");


            // add foreign keys ...
            Database.TryExecuteSqlQuery("ALTER TABLE UserExt ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox ADD CONSTRAINT fk_toPlayerId FOREIGN KEY (toPlayerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE MailBox ADD CONSTRAINT fk_fromPlayerId FOREIGN KEY (fromPlayerId) REFERENCES Users(playerId)");

            Database.TryExecuteSqlQuery("ALTER TABLE BuddyList ADD CONSTRAINT fk_sendPlayerId FOREIGN KEY (sendPlayerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE BuddyList ADD CONSTRAINT fk_recvPlayerId FOREIGN KEY (recvPlayerId) REFERENCES Users(playerId);");

            Database.TryExecuteSqlQuery("ALTER TABLE MessageQueue ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");

            Database.TryExecuteSqlQuery("ALTER TABLE Inventory ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE TrackedQuest ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE CompetitionGear ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Awards ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Jewelry ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Leaderboards ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE NpcStartPoint ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE SavedDrawings ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");

            Database.TryExecuteSqlQuery("ALTER TABLE Horses ADD CONSTRAINT fk_ownerId FOREIGN KEY (ownerId) REFERENCES Users(playerId);");

            Database.TryExecuteSqlQuery("ALTER TABLE SolvedRealTimeRiddles ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Tracking ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Ranches ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE BannedPlayers ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE RiddlesComplete ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");

            Database.TryExecuteSqlQuery("ALTER TABLE Auctions ADD CONSTRAINT fk_ownerId FOREIGN KEY (ownerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE Auctions ADD CONSTRAINT fk_horseUniqueId FOREIGN KEY (horseUniqueId) REFERENCES Horses(uniqueId);");

            Database.TryExecuteSqlQuery("ALTER TABLE MutedPlayers ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE MutedPlayers ADD CONSTRAINT fk_mutePlayerId FOREIGN KEY (mutePlayerId) REFERENCES Users(playerId);");

            Database.TryExecuteSqlQuery("ALTER TABLE ItemPurchaseQueue ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");
            Database.TryExecuteSqlQuery("ALTER TABLE OnlineUsers ADD CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId);");

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
                if (lastVersion <= verToNum("v2.5.6")) fixupVersion2_5_6();
                if (lastVersion <= verToNum("v2.5.26")) fixupVersion2_5_26();
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
