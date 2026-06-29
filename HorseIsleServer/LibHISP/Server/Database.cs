using System;
using System.Collections.Generic;
using System.Data.Common;

using HISP.Game;
using HISP.Player;
using HISP.Game.Horse;
using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Game.Services;
using HISP.Game.SwfModules;

using MySqlConnector;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using HISP.Util;
using System.IO;
using System.Linq;

namespace HISP.Server
{
    public class Database
    {
        public const string SQL_BACKEND_MARIADB = "mariadb";
        public const string SQL_BACKEND_SQLITE = "sqllite";

        public static string ConnectionString = "";
        private static DbCommand createCommand(DbConnection db, string command)
        {
            DbCommand sqlCommand = db.CreateCommand();
            sqlCommand.CommandText = command;
            return sqlCommand;
        }
        private static int addWithValue(DbCommand cmd, string param, object value)
        {
            DbParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = param;

            if (value == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = value;

            return cmd.Parameters.Add(parameter);
        }

        private static DbConnection connectDb()
        {
            try
            {
                if (ConfigReader.SqlBackend.Equals(Database.SQL_BACKEND_MARIADB, StringComparison.InvariantCultureIgnoreCase))
                {
                    DbConnection conn = new MySqlConnection(ConnectionString);
                    conn.Open();
                    return conn;
                }
                else if (ConfigReader.SqlBackend.Equals(Database.SQL_BACKEND_SQLITE, StringComparison.InvariantCultureIgnoreCase))
                {
                    DbConnection conn = new SqliteConnection(ConnectionString);
                    conn.Open();
                    return conn;
                }
            }
            catch (DbException e)
            {
                Entry.Shutdown("Failed to connect to " + ConfigReader.SqlBackend + " database: " + e.Message);
            }
            

            Entry.Shutdown("SqlBackend has invalid value: " + ConfigReader.SqlBackend);
            return null;
        }

        public static void OnShutdown()
        {
            if (ConfigReader.SqlBackend.Equals(Database.SQL_BACKEND_MARIADB, StringComparison.InvariantCultureIgnoreCase))
                MySqlConnection.ClearAllPools();
            else if (ConfigReader.SqlBackend.Equals(Database.SQL_BACKEND_SQLITE, StringComparison.InvariantCultureIgnoreCase))
                SqliteConnection.ClearAllPools();   
        }

        public static bool TryExecuteSqlQuery(DbConnection db, string query)
        {

            DbCommand sqlCommand = createCommand(db, query);
            try
            {
                sqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Logger.ErrorPrint("Failed to SQL run command: "+query+"\n"+e.Message);
                return false;
            }
        }
        public static bool TryExecuteSqlQuery(string query)
        {
            using (DbConnection db = connectDb())
            {
                return TryExecuteSqlQuery(db, query);
            }
        }

        public static void OpenDatabase()
        {
            if (ConfigReader.SqlBackend == Database.SQL_BACKEND_MARIADB)
            {
                ConnectionString = "server=" + ConfigReader.DatabaseIP + ";user=" + ConfigReader.DatabaseUsername + ";password=" + ConfigReader.DatabasePassword + ";database=" + ConfigReader.DatabaseName;
            }
            else if(ConfigReader.SqlBackend == Database.SQL_BACKEND_SQLITE)
            {
                ConnectionString = "Data Source=\"" + Path.GetFullPath(ConfigReader.DatabaseName + ".db", ConfigReader.ConfigDirectory) + "\";";
                SqliteConnection s = new SqliteConnection(ConnectionString);
                Batteries.Init();
            }


            using (DbConnection db = connectDb())
            {
                if (ConfigReader.SqlBackend == Database.SQL_BACKEND_SQLITE) TryExecuteSqlQuery(db, "PRAGMA journal_mode=WAL");

                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Users(playerId INT PRIMARY KEY, username TEXT(16), passHash TEXT(128), salt TEXT(128), gender TEXT(16), admin TEXT(3), moderator TEXT(3))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS UserExt(playerId INT, X INT, Y INT, lastLogin INT, money INT, questPoints INT, bankBalance DOUBLE PRECISION, bankInterest DOUBLE PRECISION, profilePage TEXT(4000),ipAddress TEXT(1028),privateNotes TEXT(65535), charId INT, chatViolations INT, subscriber TEXT(3), subscribedUntil INT, experience INT, tiredness INT, hunger INT, thirst INT, freeMinutes INT, totalLogins INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Mailbox(uniqueId INT PRIMARY KEY, toPlayerId INT, fromPlayerId INT, subject TEXT(100), message TEXT(65535), timeSent INT, beenRead TEXT(3), CONSTRAINT fk_toPlayerId FOREIGN KEY (toPlayerId) REFERENCES Users(playerId), CONSTRAINT fk_fromPlayerId FOREIGN KEY (fromPlayerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS BuddyList(sendPlayerId INT, recvPlayerId INT, CONSTRAINT fk_sendPlayerId FOREIGN KEY (sendPlayerId) REFERENCES Users(playerId), CONSTRAINT fk_recvPlayerId FOREIGN KEY (recvPlayerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS MessageQueue(playerId INT, message TEXT(1028), CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Weather(area TEXT(1028), weather TEXT(64))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Inventory(playerId INT, uniqueId INT PRIMARY KEY, itemId INT, data INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS ShopInventory(shopId INT, uniqueId INT PRIMARY KEY, itemId INT, data INT)");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS DroppedItems(X INT, Y INT, uniqueId INT PRIMARY KEY, itemId INT, despawnTimer INT, data INT)");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS TrackedQuest(playerId INT, questId INT, timesCompleted INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS CompetitionGear(playerId INT, headItem INT, bodyItem INT, legItem INT, feetItem INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Awards(playerId INT, awardId INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Jewelry(playerId INT, slot1 INT, slot2 INT, slot3 INT, slot4 INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS AbuseReports(reportCreator TEXT(1028), reporting TEXT(1028), reportReason TEXT(1028))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Leaderboards(playerId INT, minigame TEXT(128), wins INT, looses INT, timesplayed INT, score INT, type TEXT(128), CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS NpcStartPoint(playerId INT, npcId INT, chatpointId INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS NpcPos(npcId INT PRIMARY KEY, X INT, Y INT, udlrPointer INT)");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS PoetryRooms(poetId INT PRIMARY KEY, X INT, Y INT, roomId INT)");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS SavedDrawings(playerId INT, drawing1 TEXT(65535), drawing2 TEXT(65535), drawing3 TEXT(65535), CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS DrawingRooms(roomId INT PRIMARY KEY, drawing TEXT(65535))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS DressupRooms(roomId INT, peiceId INT, active TEXT(3), x INT, y INT)");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Horses(uniqueId INT PRIMARY KEY, ownerId INT, leaseTime INT, leaser INT, breed INT, name TEXT(128), description TEXT(4000), sex TEXT(128), color TEXT(128), health INT, shoes INT, hunger INT, thirst INT, mood INT, groom INT, tiredness INT, experience INT, speed INT, strength INT, conformation INT, agility INT, endurance INT, inteligence INT, personality INT, height INT, saddle INT, saddlepad INT, bridle INT, companion INT, autoSell INT, trainTimer INT, category TEXT(128), spoiled INT, magicUsed INT, hidden TEXT(3), CONSTRAINT fk_ownerId FOREIGN KEY (ownerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS WildHorse(uniqueId INT PRIMARY KEY, originalOwner INT, breed INT, x INT, y INT, name TEXT(128), description TEXT(4000), sex TEXT(128), color TEXT(128), health INT, shoes INT, hunger INT, thirst INT, mood INT, groom INT, tiredness INT, experience INT, speed INT, strength INT, conformation INT, agility INT, endurance INT, inteligence INT, personality INT, height INT, saddle INT, saddlepad INT, bridle INT, companion INT, timeout INT, autoSell INT, trainTimer INT, category TEXT(128), spoiled INT, magicUsed INT)");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS LastPlayer(roomId TEXT(1028), playerId INT)");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS SolvedRealTimeRiddles(playerId INT, riddleId INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Tracking(playerId INT, what TEXT(128), count INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Treasure(uniqueId INT PRIMARY KEY, x INT, y INT, value INT, type TEXT(128))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Ranches(ranchId INT PRIMARY KEY, playerId INT, title TEXT(50), description TEXT(250), upgradeLevel INT, building1 INT, building2 INT, building3 INT, building4 INT, building5 INT, building6 INT, building7 INT, building8 INT, building9 INT, building10 INT, building11 INT, building12 INT, building13 INT, building14 INT, building15 INT, building16 INT, investedMoney INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS BannedPlayers(playerId INT, ipAddress TEXT(1028), reason TEXT(1028), CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS RiddlesComplete(playerId INT, riddleId INT, solved TEXT(1028), CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS Auctions(roomId INT, uniqueId INT PRIMARY KEY, horseUniqueId INT, ownerId INT, timeRemaining INT, highestBid INT, highestBidder INT, Done TEXT(3), CONSTRAINT fk_ownerId FOREIGN KEY (ownerId) REFERENCES Users(playerId), CONSTRAINT fk_horseUniqueId FOREIGN KEY (horseUniqueId) REFERENCES Horses(uniqueId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS MutedPlayers(playerId INT, mutePlayerId INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId), CONSTRAINT fk_mutePlayerId FOREIGN KEY (mutePlayerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS ItemPurchaseQueue(playerId INT, itemId INT, count INT, CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS OnlineUsers(playerId INT, admin TEXT(3), moderator TEXT(3), subscribed TEXT(3), new TEXT(3), CONSTRAINT fk_playerId FOREIGN KEY (playerId) REFERENCES Users(playerId))");
                TryExecuteSqlQuery(db, "CREATE TABLE IF NOT EXISTS World(time INT, day INT, year INT, startTime INT, lastLoadedInVersion TEXT(64))");

                // clear online users
                TryExecuteSqlQuery(db, "DELETE FROM OnlineUsers");

            }
            DataFixerUpper.FixUpDb();
        }
        public static int GetTotalWorldEntries()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM World");
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                return count;
            }
        }

        public static void InitWorldData()
        {
            if (GetTotalWorldEntries() <= 0)
            {
                using (DbConnection db = connectDb())
                {
                    DbCommand sqlCommand = createCommand(db, "INSERT INTO World VALUES(0,0,0,@startDate,@version)");
                    addWithValue(sqlCommand, "@startDate", Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds));
                    addWithValue(sqlCommand, "@version", ServerVersion.GetVersionString());
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }


        public static void DeleteRanchOwner(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int[] GetMutedPlayers(int playerId)
        {
            List<int> MutedPlayerIds = new List<int>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT mutePlayerId FROM MutedPlayers WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                    MutedPlayerIds.Add(reader.GetInt32(0));
                
            }
            return MutedPlayerIds.ToArray();
        }
        public static void AddMutedPlayer(int playerId, int playerToMute)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO MutedPlayers VALUES(@playerId, @mutedPlayerId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@mutedPlayerId", playerToMute);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void DeleteMutedPlayer(int playerId, int playerToMute)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM MutedPlayers WHERE playerId=@playerId AND mutePlayerId=@mutedPlayerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@mutedPlayerId", playerToMute);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static bool IsRanchOwned(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count >= 1;
            }
        }

        public static void ClearItemPurchaseQueue(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM ItemPurchaseQueue WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static Item.ItemPurchaseQueueItem[] GetItemPurchaseQueue(int playerId)
        {
            List<Item.ItemPurchaseQueueItem> queueItems = new List<Item.ItemPurchaseQueueItem>();

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM ItemPurchaseQueue WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    Item.ItemPurchaseQueueItem itm = new Item.ItemPurchaseQueueItem();
                    itm.ItemId = reader.GetInt32(1);
                    itm.ItemCount = reader.GetInt32(2);
                    queueItems.Add(itm);
                }
                
            }
            return queueItems.ToArray();
        }

        public static void CreateDressupRoomPeice(int roomId, int peiceId, bool active, int x, int y)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO DressupRooms VALUES(@roomId, @peiceId, @active, @x, @y)");
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@peiceId", peiceId);
                addWithValue(sqlCommand, "@active", active ? "YES" : "NO");
                addWithValue(sqlCommand, "@x", x);
                addWithValue(sqlCommand, "@y", y);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void AddMessageToQueue(int userId, string message)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO MessageQueue VALUES(@id,@message)");
                addWithValue(sqlCommand, "@id", userId);
                addWithValue(sqlCommand, "@message", message);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void ClearMessageQueue(int userId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM MessageQueue WHERE playerId=@id");
                addWithValue(sqlCommand, "@id", userId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static string[] GetMessageQueue(int userId)
        {
            List<string> msgQueue = new List<string>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT message FROM MessageQueue WHERE playerId=@id");
                addWithValue(sqlCommand, "@id", userId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                    msgQueue.Add(reader.GetString(0));
                
            }
            return msgQueue.ToArray();
        }

        public static void SetDressupRoomPeiceX(int roomId, int peiceId, int newX)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE DressupRooms SET x=@x WHERE roomId=@roomId AND peiceId=@peiceId");
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@peiceId", peiceId);
                addWithValue(sqlCommand, "@x", newX);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void SetDressupRoomPeiceY(int roomId, int peiceId, int newY)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE DressupRooms SET y=@y WHERE roomId=@roomId AND peiceId=@peiceId");
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@peiceId", peiceId);
                addWithValue(sqlCommand, "@y", newY);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetDressupRoomPeiceActive(int roomId, int peiceId, bool active)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE DressupRooms SET active=@active WHERE roomId=@roomId AND peiceId=@peiceId");
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@peiceId", peiceId);
                addWithValue(sqlCommand, "@active", active ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static Dressup.DressupPeice[] LoadDressupRoom(Dressup.DressupRoom room)
        {
            List<Dressup.DressupPeice> peices = new List<Dressup.DressupPeice>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM DressupRooms WHERE roomId=@roomId");
                addWithValue(sqlCommand, "@roomId", room.RoomId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    int peiceId = reader.GetInt32(1);
                    bool active = reader.GetString(2) == "YES";
                    int x = reader.GetInt32(3);
                    int y = reader.GetInt32(4);
                    Dressup.DressupPeice peice = new Dressup.DressupPeice(room, peiceId, x, y, active, false);
                    peices.Add(peice);
                }
                
            }
            return peices.ToArray();
        }

        public static int[] GetSolvedRealTimeRiddles(int playerId)
        {
            List<int> solvedRiddleId = new List<int>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT riddleId FROM SolvedRealTimeRiddles WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    solvedRiddleId.Add(reader.GetInt32(0));
                }
                
                return solvedRiddleId.ToArray();
            }
        }

        public static int GetRanchInvestment(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT investedMoney FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int invested = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return invested;
            }
        }
        public static void SetRanchUpgradeLevel(int ranchId, int upgradeLevel)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET upgradeLevel=@upgradeLevel WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@upgradeLevel", upgradeLevel);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchInvestment(int ranchId, int investedMoney)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET investedMoney=@investedMoney WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@investedMoney", investedMoney);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchOwner(int ranchId, int ownerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET playerId=@ownerId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@ownerId", ownerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchDescription(int ranchId, string description)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET description=@description WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@description", description);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchTitle(int ranchId, string title)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET title=@title WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@title", title);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding16(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building16=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding15(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building15=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding14(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building14=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding13(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building13=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding12(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building12=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding11(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building11=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding10(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building10=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding9(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building9=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding8(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building8=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding7(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building7=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding6(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building6=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding5(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building5=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding4(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building4=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding3(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building3=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding2(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building2=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetRanchBuilding1(int ranchId, int buildingId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Ranches SET building1=@buildingId WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetRanchBuilding16(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building16 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding15(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building15 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding14(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building14 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding13(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building13 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding12(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building12 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding11(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building11 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding10(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building10 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding9(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building9 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding8(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building8 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding7(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building7 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding6(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building6 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding5(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building5 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding4(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building4 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding3(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building3 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding2(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building2 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchBuilding1(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT building1 FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return building;
            }
        }
        public static int GetRanchUpgradeLevel(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT upgradeLevel FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int upgradeLevel = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return upgradeLevel;
            }
        }

        public static string GetRanchDescription(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT description FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                string description = sqlCommand.ExecuteScalar().ToString();
                
                return description;
            }
        }
        public static string GetRanchTitle(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT title FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                string title = sqlCommand.ExecuteScalar().ToString();
                
                return title;
            }
        }
        public static int GetRanchOwner(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM Ranches WHERE ranchId=@ranchId");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                sqlCommand.Prepare();
                int playerId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return playerId;
            }
        }

        public static int TotalRiddlesCompletedByPlayer(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM RiddlesComplete WHERE playerId=@playerId AND solved=\"YES\"");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count;
            }
        }
        public static bool HasPlayerCompletedRealTimeRiddle(int riddleId, int playerId)
        {
            
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM SolvedRealTimeRiddles WHERE riddleId=@riddleId AND playerId=@playerId");
                addWithValue(sqlCommand, "@riddleId", riddleId);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count >= 1;
            }

        }
        public static void CompleteRealTimeRiddle(int riddleId, int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO SolvedRealTimeRiddles VALUES(@playerId, @riddleId)");
                addWithValue(sqlCommand, "@riddleId", riddleId);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static bool HasPlayerCompletedRiddle(int riddleId, int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM RiddlesComplete WHERE riddleId=@riddleId AND playerId=@playerId AND solved=\"YES\"");
                addWithValue(sqlCommand, "@riddleId", riddleId);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count >= 1;
            }
        }
        public static void CompleteRiddle(int riddleId, int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO RiddlesComplete VALUES(@playerId, @riddleId, \"YES\")");
                addWithValue(sqlCommand, "@riddleId", riddleId);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void AddRanch(int ranchId, int playerId, string title, string description, int upgradeLevel, int building1, int building2, int building3, int building4, int building5, int building6, int building7, int building8, int building9, int building10, int building11, int building12, int building13, int building14, int building15, int building16, int investedMoney)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Ranches VALUES(@ranchId, @playerId, @title, @description, @upgradeLevel, @building1, @building2, @building3, @building4, @building5, @building6, @building7, @building8, @building9, @building10, @building11, @building12, @building13, @building14, @building15, @building16, @investedMoney)");
                addWithValue(sqlCommand, "@ranchId", ranchId);
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@title", title);
                addWithValue(sqlCommand, "@description", description);
                addWithValue(sqlCommand, "@upgradeLevel", upgradeLevel);
                addWithValue(sqlCommand, "@building1", building1);
                addWithValue(sqlCommand, "@building2", building2);
                addWithValue(sqlCommand, "@building3", building3);
                addWithValue(sqlCommand, "@building4", building4);
                addWithValue(sqlCommand, "@building5", building5);
                addWithValue(sqlCommand, "@building6", building6);
                addWithValue(sqlCommand, "@building7", building7);
                addWithValue(sqlCommand, "@building8", building8);
                addWithValue(sqlCommand, "@building9", building9);
                addWithValue(sqlCommand, "@building10", building10);
                addWithValue(sqlCommand, "@building11", building11);
                addWithValue(sqlCommand, "@building12", building12);
                addWithValue(sqlCommand, "@building13", building13);
                addWithValue(sqlCommand, "@building14", building14);
                addWithValue(sqlCommand, "@building15", building15);
                addWithValue(sqlCommand, "@building16", building16);
                addWithValue(sqlCommand, "@investedMoney", investedMoney);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static void SetTreasureValue(int uniqueId, int value)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Treasure SET value=@value WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                addWithValue(sqlCommand, "@value", value);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void DeleteTreasure(int uniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Treasure  WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void AddTreasure(int uniqueId, int x, int y, int value, string type)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Treasure VALUES(@uniqueId, @x, @y, @value, @type)");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                addWithValue(sqlCommand, "@x", x);
                addWithValue(sqlCommand, "@y", y);
                addWithValue(sqlCommand, "@value", value);
                addWithValue(sqlCommand, "@type", type);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static Treasure[] GetTreasures()
        {
            List<Treasure> treasures = new List<Treasure>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Treasure");
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    int uniqueId = reader.GetInt32(0);
                    int x = reader.GetInt32(1);
                    int y = reader.GetInt32(2);
                    int value = reader.GetInt32(3);
                    string type = reader.GetString(4);
                    Treasure treasure = new Treasure(x, y, type, uniqueId, value);
                    treasures.Add(treasure);
                }
                
                return treasures.ToArray();
            }
        }

        public static void AddTrackedItem(int playerId, Tracking.TrackableItem what, int count)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Tracking VALUES(@playerId, @what, @count)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@what", what.ToString());
                addWithValue(sqlCommand, "@count", count);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static bool HasTrackedItem(int playerId, Tracking.TrackableItem what)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM Tracking WHERE playerId=@playerId AND what=@what");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@what", what.ToString());
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count > 0;
            }
        }
        public static int GetTrackedCount(int playerId, Tracking.TrackableItem what)
        {

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT count FROM Tracking WHERE playerId=@playerId AND what=@what");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@what", what.ToString());
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count;
            }
        }
        public static void DeleteAllDroppedItemsWithId(int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM DroppedItems WHERE itemId=@itemId");
                addWithValue(sqlCommand, "@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void DeleteAllItemsFromUsers(int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Inventory WHERE itemId=@itemId");
                addWithValue(sqlCommand, "@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();                
            }
        }


        public static void SetTrackedItemCount(int playerId, Tracking.TrackableItem what, int count)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Tracking SET count=@count WHERE playerId=@playerId AND what=@what");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@what", what.ToString());
                addWithValue(sqlCommand, "@count", count);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void AddLastPlayer(string roomId, int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO LastPlayer VALUES(@roomId,@playerId)");
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SetWildHorseX(int uniqueId, int x)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE WildHorse SET x=@x WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                addWithValue(sqlCommand, "@x", x);
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetWildHorseTimeout(int uniqueId, int timeout)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE WildHorse SET timeout=@timeout WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                addWithValue(sqlCommand, "@timeout", timeout);
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void RemoveWildHorse(int uniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM WildHorse WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetWildHorseY(int uniqueId, int x)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE WildHorse SET y=@y WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                addWithValue(sqlCommand, "@y", x);
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void RemoveHorse(int uniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Horses WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void AddHorse(HorseInstance horse)
        {

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Horses VALUES(@uniqueId,@originalOwner,@leaseTime,@leaser,@breed,@name,@description,@sex,@color,@health,@shoes,@hunger,@thirst,@mood,@groom,@tiredness,@experience,@speed,@strength,@conformation,@agility,@endurance,@inteligence,@personality,@height,@saddle,@saddlepad,@bridle,@companion,@autosell,@training,@category,@spoiled,@magicused,@hidden)");

                addWithValue(sqlCommand, "@uniqueId", horse.UniqueId);
                addWithValue(sqlCommand, "@originalOwner", horse.Owner);
                addWithValue(sqlCommand, "@leaseTime", horse.LeaseTime);
                addWithValue(sqlCommand, "@leaser", horse.Leaser);
                addWithValue(sqlCommand, "@breed", horse.Breed.Id);
                addWithValue(sqlCommand, "@name", horse.Name);
                addWithValue(sqlCommand, "@description", horse.Description);
                addWithValue(sqlCommand, "@sex", horse.Gender);
                addWithValue(sqlCommand, "@color", horse.Color);

                addWithValue(sqlCommand, "@health", horse.BasicStats.Health);
                addWithValue(sqlCommand, "@shoes", horse.BasicStats.Shoes);
                addWithValue(sqlCommand, "@hunger", horse.BasicStats.Hunger);
                addWithValue(sqlCommand, "@thirst", horse.BasicStats.Thirst);
                addWithValue(sqlCommand, "@mood", horse.BasicStats.Mood);
                addWithValue(sqlCommand, "@groom", horse.BasicStats.Groom);
                addWithValue(sqlCommand, "@tiredness", horse.BasicStats.Tiredness);
                addWithValue(sqlCommand, "@experience", horse.BasicStats.Experience);

                addWithValue(sqlCommand, "@speed", horse.AdvancedStats.Speed);
                addWithValue(sqlCommand, "@strength", horse.AdvancedStats.Strength);
                addWithValue(sqlCommand, "@conformation", horse.AdvancedStats.Conformation);
                addWithValue(sqlCommand, "@agility", horse.AdvancedStats.Agility);
                addWithValue(sqlCommand, "@endurance", horse.AdvancedStats.Endurance);
                addWithValue(sqlCommand, "@inteligence", horse.AdvancedStats.Inteligence);
                addWithValue(sqlCommand, "@personality", horse.AdvancedStats.Personality);
                addWithValue(sqlCommand, "@height", horse.AdvancedStats.Height);

                if (horse.Equipment.Saddle != null)
                    addWithValue(sqlCommand, "@saddle", horse.Equipment.Saddle.Id);
                else
                    addWithValue(sqlCommand, "@saddle", null);

                if (horse.Equipment.SaddlePad != null)
                    addWithValue(sqlCommand, "@saddlepad", horse.Equipment.SaddlePad.Id);
                else
                    addWithValue(sqlCommand, "@saddlepad", null);

                if (horse.Equipment.Bridle != null)
                    addWithValue(sqlCommand, "@bridle", horse.Equipment.Bridle.Id);
                else
                    addWithValue(sqlCommand, "@bridle", null);

                if (horse.Equipment.Companion != null)
                    addWithValue(sqlCommand, "@companion", horse.Equipment.Companion.Id);
                else
                    addWithValue(sqlCommand, "@companion", null);





                addWithValue(sqlCommand, "@autosell", horse.AutoSell);
                addWithValue(sqlCommand, "@training", horse.TrainTimer);
                addWithValue(sqlCommand, "@category", horse.Category);
                addWithValue(sqlCommand, "@spoiled", horse.Spoiled);
                addWithValue(sqlCommand, "@magicused", horse.MagicUsed);

                addWithValue(sqlCommand, "@hidden", horse.Hidden ? "YES" : "NO");

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }

        }

        public static HorseInstance ReadHorseInstance(DbDataReader reader)
        {
            int uniqueId = reader.GetInt32(0);
            int breedId = reader.GetInt32(4);

            HorseInfo.Breed horseBreed = HorseInfo.GetBreedById(breedId);
            string name = reader.GetString(5);
            string description = reader.GetString(6);
            int spoiled = reader.GetInt32(32);
            string category = reader.GetString(31);
            int magicUsed = reader.GetInt32(33);
            int autosell = reader.GetInt32(29);
            int leaseTime = reader.GetInt32(2);
            bool hidden = reader.GetString(34) == "YES";
            int owner = reader.GetInt32(1);
            string color = reader.GetString(8);

            HorseInstance inst = new HorseInstance(horseBreed, uniqueId, color, name, description, spoiled, category, magicUsed, autosell, leaseTime, hidden, owner);
            
            inst.Leaser = reader.GetInt32(3);
            inst.Gender = reader.GetString(7);
            


            int health = reader.GetInt32(9);
            int shoes = reader.GetInt32(10);
            int hunger = reader.GetInt32(11);
            int thirst = reader.GetInt32(12);
            int mood = reader.GetInt32(13);
            int groom = reader.GetInt32(14);
            int tiredness = reader.GetInt32(15);
            int experience = reader.GetInt32(16);
            inst.BasicStats = new HorseInfo.BasicStats(inst, health, shoes, hunger, thirst, mood, groom, tiredness, experience);


            int speed = reader.GetInt32(17);
            int strength = reader.GetInt32(18);
            int conformation = reader.GetInt32(19);
            int agility = reader.GetInt32(20);
            int endurance = reader.GetInt32(21);
            int inteligence = reader.GetInt32(22);
            int personality = reader.GetInt32(23);
            int height = reader.GetInt32(24);
            inst.AdvancedStats = new HorseInfo.AdvancedStats(inst, speed, strength, conformation, agility, inteligence, endurance, personality, height);

            if (!reader.IsDBNull(25))
                inst.Equipment.Saddle = Item.GetItemById(reader.GetInt32(25));
            if (!reader.IsDBNull(26))
                inst.Equipment.SaddlePad = Item.GetItemById(reader.GetInt32(26));
            if (!reader.IsDBNull(27))
                inst.Equipment.Bridle = Item.GetItemById(reader.GetInt32(27));
            if (!reader.IsDBNull(28))
                inst.Equipment.Companion = Item.GetItemById(reader.GetInt32(28));

            
            return inst;
        }

        public static void LoadHorseInventory(HorseInventory inv, int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Horses WHERE ownerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    inv.AddHorse(ReadHorseInstance(reader), false, true);
                }

                
            }
        }

        public static void LoadAuctionRoom(Auction auction, int roomId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Auctions WHERE roomId=@roomId");
                addWithValue(sqlCommand, "@roomId", roomId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    int uniqueId = reader.GetInt32(1);
                    int timeRemaining = reader.GetInt32(4);
                    int highestBid = reader.GetInt32(5);
                    int highestBidder = reader.GetInt32(6);
                    int horseId = reader.GetInt32(2);

                    Auction.AuctionEntry auctionEntry = new Auction.AuctionEntry(timeRemaining, highestBid, highestBidder, uniqueId);

                    auctionEntry.Horse = GetPlayerHorse(horseId);
                    auctionEntry.OwnerId = reader.GetInt32(3);
                    auctionEntry.Completed = reader.GetString(7) == "YES";
                    auctionEntry.auctionRoomPlacedIn = auction;
                    auction.AddExistingEntry(auctionEntry);

                }

                
            }
        }

        public static void DeleteAuctionRoom(int uniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Auctions WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void AddAuctionRoom(Auction.AuctionEntry entry, int roomId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Auctions VALUES(@roomId, @uniqueId, @horseUniqueId, @ownerId, @timeRemaining, @highestBid, @highestBidder, @done)");
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@uniqueId", entry.UniqueId);
                addWithValue(sqlCommand, "@horseUniqueId", entry.Horse.UniqueId);
                addWithValue(sqlCommand, "@ownerId", entry.OwnerId);
                addWithValue(sqlCommand, "@timeRemaining", entry.TimeRemaining);
                addWithValue(sqlCommand, "@highestBid", entry.HighestBid);
                addWithValue(sqlCommand, "@highestBidder", entry.HighestBidder);
                addWithValue(sqlCommand, "@done", entry.Completed ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void BanUser(int userId, string ip, string reason)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO BannedPlayers VALUES(@playerId,@ipAddress,@reason)");
                addWithValue(sqlCommand, "@playerId", userId);
                addWithValue(sqlCommand, "@ipAddress", ip);
                addWithValue(sqlCommand, "@reason", reason);
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void UnBanUser(int userId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM BannedPlayers WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", userId);
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static bool IsIpBanned(string ip)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM BannedPlayers WHERE ipAddress=@ipAddr");
                addWithValue(sqlCommand, "@ipAddr", ip);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count >= 1;
            }
        }
        public static bool IsUserBanned(int userId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM BannedPlayers WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", userId);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count >= 1;
            }

        }

        public static void AddWildHorse(WildHorse horse)
        {

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO WildHorse VALUES(@uniqueId,@originalOwner,@breed,@x,@y,@name,@description,@sex,@color,@health,@shoes,@hunger,@thirst,@mood,@groom,@tiredness,@experience,@speed,@strength,@conformation,@agility,@endurance,@inteligence,@personality,@height,@saddle,@saddlepad,@bridle,@companion,@timeout,@autosell,@training,@category,@spoiled,@magicused)");

                addWithValue(sqlCommand, "@uniqueId", horse.Instance.UniqueId);
                addWithValue(sqlCommand, "@originalOwner", horse.Instance.Owner);
                addWithValue(sqlCommand, "@breed", horse.Instance.Breed.Id);
                addWithValue(sqlCommand, "@x", horse.X);
                addWithValue(sqlCommand, "@y", horse.Y);
                addWithValue(sqlCommand, "@name", horse.Instance.Name);
                addWithValue(sqlCommand, "@description", horse.Instance.Description);
                addWithValue(sqlCommand, "@sex", horse.Instance.Gender);
                addWithValue(sqlCommand, "@color", horse.Instance.Color);

                addWithValue(sqlCommand, "@health", horse.Instance.BasicStats.Health);
                addWithValue(sqlCommand, "@shoes", horse.Instance.BasicStats.Shoes);
                addWithValue(sqlCommand, "@hunger", horse.Instance.BasicStats.Hunger);
                addWithValue(sqlCommand, "@thirst", horse.Instance.BasicStats.Thirst);
                addWithValue(sqlCommand, "@mood", horse.Instance.BasicStats.Mood);
                addWithValue(sqlCommand, "@groom", horse.Instance.BasicStats.Groom);
                addWithValue(sqlCommand, "@tiredness", horse.Instance.BasicStats.Tiredness);
                addWithValue(sqlCommand, "@experience", horse.Instance.BasicStats.Experience);

                addWithValue(sqlCommand, "@speed", horse.Instance.AdvancedStats.Speed);
                addWithValue(sqlCommand, "@strength", horse.Instance.AdvancedStats.Strength);
                addWithValue(sqlCommand, "@conformation", horse.Instance.AdvancedStats.Conformation);
                addWithValue(sqlCommand, "@agility", horse.Instance.AdvancedStats.Agility);
                addWithValue(sqlCommand, "@endurance", horse.Instance.AdvancedStats.Endurance);
                addWithValue(sqlCommand, "@inteligence", horse.Instance.AdvancedStats.Inteligence);
                addWithValue(sqlCommand, "@personality", horse.Instance.AdvancedStats.Personality);
                addWithValue(sqlCommand, "@height", horse.Instance.AdvancedStats.Height);

                if (horse.Instance.Equipment.Saddle != null)
                    addWithValue(sqlCommand, "@saddle", horse.Instance.Equipment.Saddle.Id);
                else
                    addWithValue(sqlCommand, "@saddle", null);

                if (horse.Instance.Equipment.SaddlePad != null)
                    addWithValue(sqlCommand, "@saddlepad", horse.Instance.Equipment.SaddlePad.Id);
                else
                    addWithValue(sqlCommand, "@saddlepad", null);

                if (horse.Instance.Equipment.Bridle != null)
                    addWithValue(sqlCommand, "@bridle", horse.Instance.Equipment.Bridle.Id);
                else
                    addWithValue(sqlCommand, "@bridle", null);

                if (horse.Instance.Equipment.Companion != null)
                    addWithValue(sqlCommand, "@companion", horse.Instance.Equipment.Companion.Id);
                else
                    addWithValue(sqlCommand, "@companion", null);





                addWithValue(sqlCommand, "@timeout", horse.Timeout);
                addWithValue(sqlCommand, "@autosell", horse.Instance.AutoSell);
                addWithValue(sqlCommand, "@training", horse.Instance.TrainTimer);
                addWithValue(sqlCommand, "@category", horse.Instance.Category);
                addWithValue(sqlCommand, "@spoiled", horse.Instance.Spoiled);
                addWithValue(sqlCommand, "@magicused", horse.Instance.MagicUsed);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }

        }


        public static void LoadWildHorses()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM WildHorse");


                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    int uniqueId = reader.GetInt32(0);
                    int breedId = reader.GetInt32(2);
                    HorseInfo.Breed horseBreed = HorseInfo.GetBreedById(breedId);
                    HorseInstance inst = new HorseInstance(horseBreed, uniqueId);
                    inst.Owner = reader.GetInt32(1);
                    inst.Name = reader.GetString(5);
                    inst.Description = reader.GetString(6);
                    inst.Gender = reader.GetString(7);
                    inst.Color = reader.GetString(8);

                    inst.BasicStats.Health = reader.GetInt32(9);
                    inst.BasicStats.Shoes = reader.GetInt32(10);
                    inst.BasicStats.Hunger = reader.GetInt32(11);
                    inst.BasicStats.Thirst = reader.GetInt32(12);
                    inst.BasicStats.Mood = reader.GetInt32(13);
                    inst.BasicStats.Groom = reader.GetInt32(14);
                    inst.BasicStats.Tiredness = reader.GetInt32(15);
                    inst.BasicStats.Experience = reader.GetInt32(16);

                    inst.AdvancedStats.Speed = reader.GetInt32(17);
                    inst.AdvancedStats.Strength = reader.GetInt32(18);
                    inst.AdvancedStats.Conformation = reader.GetInt32(19);
                    inst.AdvancedStats.Agility = reader.GetInt32(20);
                    inst.AdvancedStats.Endurance = reader.GetInt32(21);
                    inst.AdvancedStats.Inteligence = reader.GetInt32(22);
                    inst.AdvancedStats.Personality = reader.GetInt32(23);
                    inst.AdvancedStats.Height = reader.GetInt32(24);

                    if (!reader.IsDBNull(25))
                        inst.Equipment.Saddle = Item.GetItemById(reader.GetInt32(25));
                    if (!reader.IsDBNull(26))
                        inst.Equipment.SaddlePad = Item.GetItemById(reader.GetInt32(26));
                    if (!reader.IsDBNull(27))
                        inst.Equipment.Bridle = Item.GetItemById(reader.GetInt32(27));
                    if (!reader.IsDBNull(28))
                        inst.Equipment.Companion = Item.GetItemById(reader.GetInt32(28));

                    inst.AutoSell = reader.GetInt32(30);
                    inst.TrainTimer = reader.GetInt32(31);
                    inst.Category = reader.GetString(32);
                    inst.Spoiled = reader.GetInt32(33);
                    inst.MagicUsed = reader.GetInt32(34);

                    int x = reader.GetInt32(3);
                    int y = reader.GetInt32(4);
                    int timeout = reader.GetInt32(29);
                    WildHorse WildHorse = new WildHorse(inst, x, y, timeout, false);

                }

                
            }
        }

        public static bool LastPlayerExist(string roomId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM LastPlayer WHERE roomId=@roomId");
                addWithValue(sqlCommand, "@roomId", roomId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count > 0;
            }
        }

        public static int GetLastPlayer(string roomId)
        {
            if (!Database.LastPlayerExist(roomId))
                Database.AddLastPlayer(roomId, -1);

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM LastPlayer WHERE roomId=@roomId");
                addWithValue(sqlCommand, "@roomId", roomId);
                sqlCommand.Prepare();
                int playerId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return playerId;
            }
        }


        public static void SetLastPlayer(string roomId, int playerId)
        {
            if (!Database.LastPlayerExist(roomId))
                Database.AddLastPlayer(roomId, -1);
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE LastPlayer SET playerId=@playerId WHERE roomId=@roomId");
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void AddPoetWord(int id, int x, int y, int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO PoetryRooms VALUES(@id,@x,@y,@room)");
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@x", x);
                addWithValue(sqlCommand, "@y", y);
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SetPoetPosition(int id, int x, int y, int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE PoetryRooms SET X=@x, Y=@y WHERE poetId=@id AND roomId=@room");
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@x", x);
                addWithValue(sqlCommand, "@y", y);
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static bool GetPoetExist(int id, int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM PoetryRooms WHERE poetId=@id AND roomId=@room");
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count > 0;
            }
        }
        public static int GetPoetPositionX(int id, int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT X FROM PoetryRooms WHERE poetId=@id AND roomId=@room");
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                int xpos = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return xpos;
            }
        }

        public static int GetPoetPositionY(int id, int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT Y FROM PoetryRooms WHERE poetId=@id AND roomId=@room");
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                int ypos = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return ypos;
            }
        }

        public static bool SavedDrawingsExist(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM SavedDrawings WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count >= 1;
            }
        }
        public static void CreateSavedDrawings(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO SavedDrawings VALUES(@playerId,'','','')");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static bool DrawingRoomExists(int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM DrawingRooms WHERE roomId=@room");
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count >= 1;
            }
        }

        public static void CreateDrawingRoom(int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO DrawingRooms VALUES(@roomId,'')");
                addWithValue(sqlCommand, "@roomId", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SetDrawingRoomDrawing(int room, string drawing)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE DrawingRooms SET drawing=@drawing WHERE roomId=@room");
                addWithValue(sqlCommand, "@drawing", drawing);
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static string GetDrawingRoomDrawing(int room)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT drawing FROM DrawingRooms WHERE roomId=@room");
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                
                return drawing;
            }
        }
        public static string LoadDrawingSlot3(int playerId)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT drawing3 FROM SavedDrawings WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                
                return drawing;
            }
        }
        public static string LoadDrawingSlot2(int playerId)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT drawing2 FROM SavedDrawings WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                
                return drawing;
            }
        }
        public static string LoadDrawingSlot1(int playerId)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT drawing1 FROM SavedDrawings WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                
                return drawing;
            }
        }
        public static void SaveDrawingSlot1(int playerId, string drawing)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE SavedDrawings SET drawing1=@drawing WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@drawing", drawing);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SaveDrawingSlot2(int playerId, string drawing)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE SavedDrawings SET drawing2=@drawing WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@drawing", drawing);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SaveDrawingSlot3(int playerId, string drawing)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE SavedDrawings SET drawing3=@drawing WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@drawing", drawing);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }


        public static void SetLastLoadedVersion(string version)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE World SET lastLoadedInVersion=@version");
                addWithValue(sqlCommand, "@version", version);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }

        // Tests if the game was previously loaded in 1.0
        public static bool TestFor1_0()
        {
            try
            {
                using (DbConnection db = connectDb())
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT TotalLogins FROM UserExt LIMIT 1;");
                    sqlCommand.ExecuteNonQuery();
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            };
        }
        //Tests if game was previously loaded in v1.1
        public static bool TestFor1_1()
        {
            try
            {
                using (DbConnection db = connectDb())
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT data FROM ShopInventory LIMIT 1;");
                    sqlCommand.ExecuteNonQuery();
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static string GetLastLoadedVersion()
        {
            try
            {
                if (Database.GetTotalWorldEntries() <= 0) return ServerVersion.GetVersionString();

                using (DbConnection db = connectDb())
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT LastLoadedInVersion FROM World ORDER BY StartTime DESC");
                    string lastVersion = sqlCommand.ExecuteScalar().ToString();             
                    return lastVersion;
                }
            }
            catch (Exception) // table not found? must have been before v1.7.20.
            {
                if (TestFor1_0()) return "v1.0";
                if (TestFor1_1()) return "v1.1";
                return "v1.7.20";
            }
        }

        public static void SetStartTime(int startTime)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE World SET startTime=@startTimer");
                addWithValue(sqlCommand, "@startTimer", startTime);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetServerTime(int time, int day, int year)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE World SET time=@time,day=@day,year=@year");
                addWithValue(sqlCommand, "@time", time);
                addWithValue(sqlCommand, "@day", day);
                addWithValue(sqlCommand, "@year", year);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static int GetServerTime()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT time FROM World");
                int serverTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return serverTime;
            }
        }

        public static int GetServerStartTime()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT startTime FROM World");
                int startTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return startTime;
            }
        }

        public static int GetServerDay()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT day FROM World");
                int serverTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return serverTime;
            }
        }

        public static int GetServerYear()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT year FROM World");
                int creationTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return creationTime;
            }
        }




        public static bool WeatherExists(string area)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM Weather WHERE area=@area");
                addWithValue(sqlCommand, "@area", area);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count > 0;
            }
        }

        public static void InsertWeather(string area, string weather)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Weather VALUES(@area,@weather)");
                addWithValue(sqlCommand, "@weather", weather);
                addWithValue(sqlCommand, "@area", area);
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetWeather(string area, string weather)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Weather SET weather=@weather WHERE area=@area");
                addWithValue(sqlCommand, "@weather", weather);
                addWithValue(sqlCommand, "@area", area);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static string GetWeather(string area)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT weather FROM Weather WHERE area=@area");
                addWithValue(sqlCommand, "@area", area);
                string Weather = sqlCommand.ExecuteScalar().ToString();
                
                return Weather;
            }
        }

        public static void DecHorseTrainTimeout()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET trainTimer=trainTimer-1 WHERE trainTimer-1 > -1");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static HorseInstance[] GetPlayerHorsesInCategory(int playerId, string category)
        {

            List<HorseInstance> instances = new List<HorseInstance>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Horses WHERE ownerId=@playerId AND category=@category");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@category", category);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    instances.Add(ReadHorseInstance(reader));
                }
                
                return instances.ToArray(); 
            }
        }

        public static HorseInstance GetPlayerHorse(int horseUniqueId)
        {
            HorseInstance instance = null;
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Horses WHERE uniqueId=@horseUniqueId");
                addWithValue(sqlCommand, "@horseUniqueId", horseUniqueId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    instance = ReadHorseInstance(reader);
                    break;
                }
                
                
                if (instance == null)
                    throw new InvalidOperationException();
                return instance;
            }
        }

        public static int GetHorseTrainTimeout(int horseUniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT trainTimer FROM Horses WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                int trainTimer = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return trainTimer;
            }
        }

        public static void SetAuctionDone(int uniqueId, bool done)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Auctions SET done=@done WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@done", done ? "YES" : "NO");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetAuctionTimeout(int uniqueId, int timeRemaining)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Auctions SET timeRemaining=@timeRemaining WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@timeRemaining", timeRemaining);
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetAuctionHighestBid(int uniqueId, int highestBid)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Auctions SET highestBid=@highestBid WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@highestBid", highestBid);
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetAuctionHighestBidder(int uniqueId, int highestBidder)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Auctions SET highestBidder=@highestBidder WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@highestBidder", highestBidder);
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static void SetHorseOwner(int uniqueId, int owner)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET ownerId=@owner WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@owner", owner);
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseHidden(int uniqueId, bool hidden)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET hidden=@hidden WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@hidden", hidden ? "YES" : "NO");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseTrainTimeout(int horseUniqueId, int trainTimeout)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET trainTimer=@trainTimer WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@trainTimer", trainTimeout);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseColor(int horseUniqueId, string Color)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET color=@color WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@color", Color);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseCategory(int horseUniqueId, string Category)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET category=@category WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@category", Category);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseAutoSell(int horseUniqueId, int AutoSell)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET autosell=@autosell WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@autosell", AutoSell);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseMagicUsed(int horseUniqueId, int MagicUsed)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET magicused=@magicused WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@magicused", MagicUsed);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetLeaseTime(int horseUniqueId, int leaseTime)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET leaseTime=@leaseTime WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@leaseTime", leaseTime);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseName(int horseUniqueId, string Name)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET name=@name WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@name", Name);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseDescription(int horseUniqueId, string Description)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET description=@description WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@description", Description);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseTiredness(int horseUniqueId, int Tiredness)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET tiredness=@tiredness WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@tiredness", Tiredness);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseSpeed(int horseUniqueId, int Speed)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET speed=@speed WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@speed", Speed);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseStrength(int horseUniqueId, int Strength)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET strength=@strength WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@strength", Strength);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseConformation(int horseUniqueId, int Conformation)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET conformation=@conformation WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@conformation", Conformation);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseAgility(int horseUniqueId, int Agility)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET agility=@agility WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@agility", Agility);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseEndurance(int horseUniqueId, int Endurance)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET endurance=@endurance WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@endurance", Endurance);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorsePersonality(int horseUniqueId, int Personality)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET personality=@personality WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@personality", Personality);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseInteligence(int horseUniqueId, int Inteligence)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET inteligence=@inteligence WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@inteligence", Inteligence);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseSpoiled(int horseUniqueId, int Spoiled)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET spoiled=@spoiled WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@spoiled", Spoiled);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseExperience(int horseUniqueId, int Experience)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET experience=@experience WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@experience", Experience);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseShoes(int horseUniqueId, int Shoes)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET shoes=@shoes WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@shoes", Shoes);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseHeight(int horseUniqueId, int Height)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET height=@height WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@height", Height);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseMood(int horseUniqueId, int Mood)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET mood=@mood WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@mood", Mood);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseGroom(int horseUniqueId, int Groom)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET groom=@groom WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@groom", Groom);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseHunger(int horseUniqueId, int Hunger)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET hunger=@hunger WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@hunger", Hunger);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseThirst(int horseUniqueId, int Thirst)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET thirst=@thirst WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@thirst", Thirst);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseHealth(int horseUniqueId, int Health)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET health=@health WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@health", Health);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetSaddle(int horseUniqueId, int saddleItemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET saddle=@saddle WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@saddle", saddleItemId);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetSaddlePad(int horseUniqueId, int saddlePadItemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET saddlepad=@saddlepad WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@saddlepad", saddlePadItemId);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetBridle(int horseUniqueId, int bridleItemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET bridle=@bridle WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@bridle", bridleItemId);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetCompanion(int horseUniqueId, int companionItemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET companion=@companion WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@companion", companionItemId);
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearSaddle(int horseUniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET saddle=NULL WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearSaddlePad(int horseUniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET saddlepad=NULL WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearBridle(int horseUniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET bridle=NULL WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearCompanion(int horseUniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET companion=NULL WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", horseUniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static byte[] GetPasswordSalt(string username)
        {
            using (DbConnection db = connectDb())
            {
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT salt FROM Users WHERE username=@name");
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string expectedHash = sqlCommand.ExecuteScalar().ToString();
                    
                    return Helper.StringToByteArray(expectedHash);
                }
                else
                {
                    throw new InvalidOperationException("Username " + username + " not found in database.");
                }
            }
        }

        public static bool HasJewelry(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM Jewelry WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);

                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete > 0;
            }
        }

        public static void InitJewelry(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Jewelry VALUES(@playerId,0,0,0,0)");
                addWithValue(sqlCommand, "@playerId", playerId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetJewelrySlot1(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Jewelry SET slot1=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetJewelrySlot1(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT slot1 FROM Jewelry WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }

        public static void SetJewelrySlot2(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Jewelry SET slot2=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetJewelrySlot2(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT slot2 FROM Jewelry WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }


        public static void SetJewelrySlot3(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Jewelry SET slot3=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetJewelrySlot3(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT slot3 FROM Jewelry WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }

        public static void SetJewelrySlot4(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Jewelry SET slot4=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetJewelrySlot4(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT slot4 FROM Jewelry WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }
        


        public static int[] GetAwards(int playerId)
        {
            List<int> Awards = new List<int>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT awardId FROM Awards WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);

                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    Awards.Add(reader.GetInt32(0));
                }
                
                return Awards.ToArray();
            }
        }
        public static void AddAward(int playerId, int awardId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Awards VALUES(@playerId,@awardId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@awardId", awardId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
                return;
            }
        }


        public static bool HasCompetitionGear(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM CompetitionGear WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);

                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete > 0;
            }
        }

        public static void InitCompetitionGear(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO CompetitionGear VALUES(@playerId,0,0,0,0)");
                addWithValue(sqlCommand, "@playerId", playerId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetCompetitionGearHeadPeice(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE CompetitionGear SET headItem=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetCompetitionGearHeadPeice(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT headItem FROM CompetitionGear WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }

        public static void SetCompetitionGearBodyPeice(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE CompetitionGear SET bodyItem=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetCompetitionGearBodyPeice(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT bodyItem FROM CompetitionGear WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }

        public static void SetCompetitionGearLegPeice(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE CompetitionGear SET legItem=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetCompetitionGearLegPeice(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT legItem FROM CompetitionGear WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }

        public static void SetCompetitionGearFeetPeice(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE CompetitionGear SET feetItem=@itemId WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetCompetitionGearFeetPeice(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT feetItem FROM CompetitionGear WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return timesComplete;
            }
        }

        public static int GetTrackedQuestCompletedCount(int playerId, int questId)
        {
            if(CheckTrackeQuestExists(playerId,questId))
            {

                using (DbConnection db = connectDb())
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT timesCompleted FROM TrackedQuest WHERE playerId=@playerId AND questId=@questId");
                    addWithValue(sqlCommand, "@playerId", playerId);
                    addWithValue(sqlCommand, "@questId", questId);
                    sqlCommand.Prepare();
                    int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    
                    return timesComplete;
                }
            }
            else
            {
                return 0;
            }

        }
        public static bool CheckTrackeQuestExists(int playerId, int questId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(*) FROM TrackedQuest WHERE playerId=@playerId AND questId=@questId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@questId", questId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                if (count >= 1)
                    return true;
                else
                    return false;
            }

        }

        public static TrackedQuest[] GetTrackedQuests(int playerId)
        {
            List<TrackedQuest> TrackedQuests = new List<TrackedQuest>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT questId,timesCompleted FROM TrackedQuest WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    TrackedQuest TrackedQuest = new TrackedQuest(playerId, reader.GetInt32(0), reader.GetInt32(1));
                    TrackedQuests.Add(TrackedQuest);
                }
                
            }
            return TrackedQuests.ToArray();
        }
        public static void SetTrackedQuestCompletedCount(int playerId, int questId, int timesCompleted)
        {
            if(CheckTrackeQuestExists(playerId,questId))
            {
                using (DbConnection db = connectDb())
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE TrackedQuest SET timesCompleted=@timesCompleted WHERE playerId=@playerId AND questId=@questId");
                    addWithValue(sqlCommand, "@playerId", playerId);
                    addWithValue(sqlCommand, "@questId", questId);
                    addWithValue(sqlCommand, "@timesCompleted", timesCompleted);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();
                    
                }
            }
            else
            {
                AddNewTrackedQuest(playerId, questId, timesCompleted);
            }

        }
        public static bool SetUserSubscriptionStatus(int playerId, bool subscribed)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET subscriber=@subscribed WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@subscribed", subscribed ? "YES" : "NO");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                

                return subscribed;
            }
        }
        public static string GetGender(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT gender FROM Users WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                string gender = sqlCommand.ExecuteScalar().ToString();
                

                return gender;
            }
        }
        public static int GetLoginCount(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT totalLogins FROM UserExt WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                return count;
            }
        }
        public static void SetLoginCount(int playerId, int count)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET totalLogins=@count WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@count", count);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetExperience(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT experience FROM UserExt WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int xp = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                return xp;
            }
        }
        public static void SetExperience(int playerId, int exp)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET experience=@xp WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@xp", exp);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void IncAllUsersFreeTime(int minutes)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET freeMinutes=freeMinutes+@minutes");
                addWithValue(sqlCommand, "@minutes", minutes);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetFreeTime(int playerId, int minutes)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET freeMinutes=@minutes WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@minutes", minutes);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetFreeTime(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT freeMinutes FROM UserExt WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int freeMinutes = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                return freeMinutes;
            }
        }
        public static int GetUserSubscriptionExpireDate(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT subscribedUntil FROM UserExt WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int subscribedUntil = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                return subscribedUntil;
            }
        }

        public static void SetUserSubscriptionStatus(int playerId, int subscribedUntil)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET subscribedUntil=@subscribedUntil WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@subscribedUntil", subscribedUntil);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

            }
        }


        public static bool GetUserModerator(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT moderator FROM Users WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                bool admin = sqlCommand.ExecuteScalar().ToString() == "YES";


                return admin;
            }
        }
        public static bool GetUserAdmin(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT admin FROM Users WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                bool admin = sqlCommand.ExecuteScalar().ToString() == "YES";
                

                return admin;
            }
        }
        public static bool GetUserSubscribed(int playerId)
        {
            if (ConfigReader.AllUsersSubbed)
                return true;

            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT subscriber FROM UserExt WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                bool subscribed = sqlCommand.ExecuteScalar().ToString() == "YES";
                

                return subscribed; 
            }
        }
        public static void AddNewTrackedQuest(int playerId, int questId, int timesCompleted)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO TrackedQuest VALUES(@playerId,@questId,@timesCompleted)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@questId", questId);
                addWithValue(sqlCommand, "@timesCompleted", timesCompleted);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void AddOnlineUser(int playerId, bool admin, bool moderator, bool subscribed, bool newUser)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO OnlineUsers VALUES(@playerId, @admin, @moderator, @subscribed, @new)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@admin", admin ? "YES" : "NO");
                addWithValue(sqlCommand, "@moderator", moderator ? "YES" : "NO");
                addWithValue(sqlCommand, "@subscribed", subscribed ? "YES" : "NO");
                addWithValue(sqlCommand, "@new", newUser ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void RemoveOnlineUser(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM OnlineUsers WHERE (playerId=@playerId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static ItemInstance[] GetShopInventory(int shopId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT itemId,uniqueId,data FROM ShopInventory WHERE shopId=@shopId");
                addWithValue(sqlCommand, "@shopId", shopId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                List<ItemInstance> instances = new List<ItemInstance>();

                while (reader.Read())
                {
                    instances.Add(new ItemInstance(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
                }
                
                return instances.ToArray();
            }
        }

        public static void AddItemToShopInventory(int shopId, ItemInstance instance)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO ShopInventory VALUES(@shopId,@uniqueId,@itemId,@data)");
                addWithValue(sqlCommand, "@shopId", shopId);
                addWithValue(sqlCommand, "@uniqueId", instance.UniqueId);
                addWithValue(sqlCommand, "@itemId", instance.ItemId);
                addWithValue(sqlCommand, "@data", instance.Data);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void RemoveItemFromShopInventory(int shopId, ItemInstance instance)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM ShopInventory WHERE (shopId=@shopId AND uniqueId=@uniqueId)");
                addWithValue(sqlCommand, "@shopId", shopId);
                addWithValue(sqlCommand, "@uniqueId", instance.UniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static ItemInstance[] GetPlayerInventory(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT itemId,uniqueId,data FROM Inventory WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                List<ItemInstance> instances = new List<ItemInstance>();

                while (reader.Read())
                {
                    instances.Add(new ItemInstance(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
                }
                
                return instances.ToArray();
            }
        }
        public static int[] GetModsAndAdmins()
        {
            List<int> userList = new List<int>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM Users WHERE moderator=\"YES\" AND admin=\"YES\"");
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userList.Add(reader.GetInt32(0));
                }
                
            }
            return userList.ToArray();
        }

        public static int[] GetUsers()
        {
            List<int> userList = new List<int>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM Users");
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    userList.Add(reader.GetInt32(0));
                }
                
            }
            return userList.ToArray();
        }
        public static int[] GetModeratorUsers()
        {
            List<int> userList = new List<int>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM Users WHERE moderator=\"YES\" OR admin=\"YES\"");
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userList.Add(reader.GetInt32(0));
                }

            }
            return userList.ToArray();
        }

        public static void AddItemToInventory(int playerId, ItemInstance instance)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Inventory VALUES(@playerId,@uniqueId,@itemId, @data)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@uniqueId", instance.UniqueId);
                addWithValue(sqlCommand, "@itemId", instance.ItemId);
                addWithValue(sqlCommand, "@data", instance.Data);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void RemoveAllItemTypesFromPlayerInventory(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Inventory WHERE (playerId=@playerId AND itemId=@itemId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void RemoveItemFromInventory(int playerId, ItemInstance instance)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Inventory WHERE (playerId=@playerId AND uniqueId=@uniqueId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@uniqueId", instance.UniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static bool HasNpcStartpointSet(int playerId, int npcId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM NpcStartPoint WHERE playerId=@playerId AND npcId=@npcId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                int total = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return total >= 1;
            }
        }
        public static bool HasNpcPos(int npcId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM NpcPos WHERE npcId=@npcId");
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                int total = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return total >= 1;
            }
        }
        public static void SetNpcY(int npcId, int x)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE NpcPos SET Y=@yPos WHERE npcId=@npcId");
                addWithValue(sqlCommand, "@yPos", x);
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetUserMod(int playerId, bool moderator)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Users SET moderator=@moderator WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@moderator", (moderator ? "YES" : "NO"));
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void SetUserAdmin(int playerId, bool admin)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Users SET admin=@admin WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@admin", (admin ? "YES" : "NO"));
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void SetNpcX(int npcId, int x)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE NpcPos SET X=@xPos WHERE npcId=@npcId");
                addWithValue(sqlCommand, "@xPos", x);
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetNpcUdlrPointer(int npcId, int udlr)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE NpcPos SET udlrPointer=@udlr WHERE npcId=@npcId");
                addWithValue(sqlCommand, "@udlr", udlr);
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static int GetNpcUdlrPointer(int npcId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT udlrPointer FROM NpcPos WHERE npcId=@npcId");
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                int udlrPointer = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return udlrPointer;
            }
        }
        public static int GetNpcPosY(int npcId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT y FROM NpcPos WHERE npcId=@npcId");
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                int y = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return y;
            }
        }
        public static int GetNpcPosX(int npcId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT X FROM NpcPos WHERE npcId=@npcId");
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                int x = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return x;
            }
        }
        public static void AddNpcPos(int npcId, int X, int Y, int udlrPointer)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO NpcPos VALUES(@npcId, @xPos, @yPos, @udlr)");
                addWithValue(sqlCommand, "@npcId", npcId);
                addWithValue(sqlCommand, "@xPos", X);
                addWithValue(sqlCommand, "@yPos", Y);
                addWithValue(sqlCommand, "@udlr", udlrPointer);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void AddNpcStartPoint(int playerId, int npcId, int startChatpoint)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO NpcStartPoint VALUES(@playerId, @npcId, @chatpointId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@npcId", npcId);
                addWithValue(sqlCommand, "@chatpointId", startChatpoint);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetNpcStartPoint(int playerId, int npcId, int startChatpoint)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE NpcStartPoint SET chatpointId=@chatpointId WHERE playerId=@playerId AND npcId=@npcId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@npcId", npcId);
                addWithValue(sqlCommand, "@chatpointId", startChatpoint);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static int GetDroppedItemsCount()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM DroppedItems");
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count;
            }
        }
        public static int GetNpcStartPoint(int playerId, int npcId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT chatpointId FROM NpcStartPoint WHERE playerId=@playerId AND npcId=@npcId");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                int startPoint = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return startPoint;
            }
        }

        public static void RemoveDespawningItems()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM DroppedItems WHERE despawnTimer <=0");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static void RemoveDroppedItem(int uniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM DroppedItems WHERE (uniqueId=@uniqueId)");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static DroppedItems.DroppedItem[] GetDroppedItems()
        {
            List<DroppedItems.DroppedItem> itemList = new List<DroppedItems.DroppedItem>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM DroppedItems");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    ItemInstance instance = new ItemInstance(reader.GetInt32(3), reader.GetInt32(2));
                    DroppedItems.DroppedItem droppedItem = new DroppedItems.DroppedItem(instance);
                    droppedItem.X = reader.GetInt32(0);
                    droppedItem.Y = reader.GetInt32(1);
                    droppedItem.DespawnTimer = reader.GetInt32(4);
                    droppedItem.Data = reader.GetInt32(5);
                    itemList.Add(droppedItem);
                }
                

            }
            return itemList.ToArray();
        }
        public static void DecrementDroppedItemDespawnTimer()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE DroppedItems SET DespawnTimer=DespawnTimer-1");
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void AddDroppedItem(DroppedItems.DroppedItem item)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO DroppedItems VALUES(@x, @y, @uniqueId, @itemId, @despawnTimer, @data)");
                addWithValue(sqlCommand, "@x", item.X);
                addWithValue(sqlCommand, "@y", item.Y);
                addWithValue(sqlCommand, "@uniqueId", item.Instance.UniqueId);
                addWithValue(sqlCommand, "@itemId", item.Instance.ItemId);
                addWithValue(sqlCommand, "@despawnTimer", item.DespawnTimer);
                addWithValue(sqlCommand, "@data", item.Data);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                

            }
        }


        public static void AddReport(string reportCreator, string reporting, string reportReason)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO AbuseReports VALUES(@reportCreator,@reporting,@reportReason)");
                int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                addWithValue(sqlCommand, "@reportCreator", reportCreator);
                addWithValue(sqlCommand, "@reporting", reporting);
                addWithValue(sqlCommand, "@reportReason", reportReason);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }

        }
        public static Mailbox.Mail[] LoadMailbox(int toId)
        {
            List<Mailbox.Mail> mailList = new List<Mailbox.Mail>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Mailbox WHERE toPlayerId=@toId");
                addWithValue(sqlCommand, "@toId", toId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    Mailbox.Mail mailMessage = new Mailbox.Mail();
                    mailMessage.UniqueId = UniqueID.NextUniqueId(reader.GetInt32(0));
                    mailMessage.ToUser = reader.GetInt32(1);
                    mailMessage.FromUser = reader.GetInt32(2);
                    mailMessage.Subject = reader.GetString(3);
                    mailMessage.Message = reader.GetString(4);
                    mailMessage.Timestamp = reader.GetInt32(5);
                    mailMessage.Read = reader.GetString(6) == "YES";
                    mailList.Add(mailMessage);
                }
                
            }
            return mailList.ToArray();
        }
        public static void ReadAllMail(int toId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Mailbox SET beenRead='YES' WHERE toPlayerId=@toId");
                addWithValue(sqlCommand, "@toId", toId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void DeleteMail(int uniqueId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Mailbox WHERE uniqueId=@uniqueId");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void AddMail(int uniqueId, int toId, int fromId, string subject, string message, int timestamp, bool read)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Mailbox VALUES(@uniqueId, @toId,@from,@subject,@message,@time,@read)");
                addWithValue(sqlCommand, "@uniqueId", uniqueId);
                addWithValue(sqlCommand, "@toId", toId);
                addWithValue(sqlCommand, "@from", fromId);
                addWithValue(sqlCommand, "@subject", subject);
                addWithValue(sqlCommand, "@message", message);
                addWithValue(sqlCommand, "@time", timestamp);
                addWithValue(sqlCommand, "@read", read ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }

        }

        public static bool CheckUserExist(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM Users WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                return count >= 1;
            }
        }
        public static bool CheckUserExist(string username)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM Users WHERE username=@name");
                addWithValue(sqlCommand, "@name", username);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count >= 1;
            }
        }
        public static bool CheckUserExtExists(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM UserExt WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count >= 1;
            }
        }


        public static bool CheckUsernameIsModerator(string username)
        {
            using (DbConnection db = connectDb())
            {
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT moderator FROM Users WHERE username=@name");
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string modStr = sqlCommand.ExecuteScalar().ToString();

                    
                    return modStr == "YES";
                }
                else
                {
                    throw new InvalidOperationException("Username " + username + " not found in database.");
                }
            }
        }


        public static bool CheckUsernameIsAdmin(string username)
        {
            using (DbConnection db = connectDb())
            {
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT admin FROM Users WHERE username=@name");
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string adminStr = sqlCommand.ExecuteScalar().ToString();

                    
                    return adminStr == "YES";
                }
                else
                {
                    throw new InvalidOperationException("Username " + username + " not found in database.");
                }
            }
        }

        public static int GetBuddyCount(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT COUNT(1) FROM BuddyList WHERE sendPlayerId=@playerId OR recvPlayerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                return count;
            }
        }

        public static int[] GetBuddyList(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                if (GetBuddyCount(playerId) <= 0)
                    return new int[0];      // user is forever alone.

                List<int> buddyList = new List<int>();

                DbCommand sqlCommand = createCommand(db, "SELECT sendPlayerId,recvPlayerId FROM BuddyList WHERE sendPlayerId=@playerId OR recvPlayerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader dataReader = sqlCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    int adder = dataReader.GetInt32(0);
                    int friend = dataReader.GetInt32(1);
                    if (adder != playerId)
                        buddyList.Add(adder);
                    else if (friend != playerId)
                        buddyList.Add(friend);
                }

                
                return buddyList.ToArray();
            }
        }

        public static void RemoveBuddy(int playerId, int friendId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM BuddyList WHERE (sendPlayerId=@playerId AND recvPlayerId=@friendId) OR (sendPlayerId=@friendid AND recvPlayerId=@playerId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@friendId", friendId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void AddBuddy(int playerId, int friendId)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO BuddyList VALUES(@playerId,@friendId)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@friendId", friendId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static string GetIpAddress(int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (!CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Does not exist in UserExt.");

                DbCommand sqlCommand = createCommand(db, "SELECT ipAddress FROM UserExt WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", id);
                sqlCommand.Prepare();
                string IpAddress = sqlCommand.ExecuteScalar().ToString();
                
                return IpAddress;
            }
        }
        public static void SetIpAddress(int id, string ipAddress)
        {
            using (DbConnection db = connectDb())
            {
                
                if (!CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Does not exist in UserExt.");

                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET ipAddress=@ipAddr WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@ipAddr", ipAddress);
                addWithValue(sqlCommand, "@playerId", id);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static int GetNextFreeUserId()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT MAX(playerId)+1 FROM Users");
                sqlCommand.Prepare();

                object res = sqlCommand.ExecuteScalar();
                if (res == DBNull.Value)
                    return 0;

                return Convert.ToInt32(res);
            }
        }
         
        public static void CreateUser(int playerId, string username, string passhash, string salt, string gender, bool admin, bool moderator)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Users VALUES(@playerId,@username,@passhash,@salt,@gender,@admin,@moderator)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@username", username);
                addWithValue(sqlCommand, "@passhash", passhash);
                addWithValue(sqlCommand, "@salt", salt);
                addWithValue(sqlCommand, "@gender", gender);
                addWithValue(sqlCommand, "@admin", admin ? "YES" : "NO");
                addWithValue(sqlCommand, "@moderator", moderator ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }


        public static void CreateUserExt(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(playerId)) // user already exists!
                    throw new Exception("Userid " + playerId + " Already in UserExt.");

                DbCommand sqlCommand = createCommand(db, "INSERT INTO UserExt VALUES(@playerId,@x,@y,@timestamp,0,0,0,0,'','','',0,0,'NO',0,0,1000,1000,1000, 180,1)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@timestamp", Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));
                addWithValue(sqlCommand, "@x", Map.NewUserStartX);
                addWithValue(sqlCommand, "@y", Map.NewUserStartY);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static int GetUserId(string username)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM Users WHERE username=@name");
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    int userId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return userId;
                }
                else
                {
                    throw new InvalidOperationException("Username " + username + " not found in database.");
                }
            }
        }

        public static string GetPlayerNotes(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT privateNotes FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    string privateNotes = sqlCommand.ExecuteScalar().ToString();

                    
                    return privateNotes;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerNotes(int id, string notes)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET privateNotes=@notes WHERE playerId=@id");
                    addWithValue(sqlCommand, "@notes", notes);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }


        public static int GetPlayerCharId(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT charId FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int CharId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return CharId;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerCharId(int charid, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET charId=@charId WHERE playerId=@id");
                    addWithValue(sqlCommand, "@charId", charid);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerX(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT X FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int X = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return X;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerX(int x, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET X=@x WHERE playerId=@id");
                    addWithValue(sqlCommand, "@x", x);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerY(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT Y FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int Y = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return Y;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static int GetChatViolations(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT chatViolations FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int violations = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return violations;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }


        public static void SetChatViolations(int violations, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET chatViolations=@violations WHERE playerId=@id");
                    addWithValue(sqlCommand, "@violations", violations);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }
        public static void SetPlayerY(int y, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET Y=@y WHERE playerId=@id");
                    addWithValue(sqlCommand, "@y", y);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerQuestPoints(int qp, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET questPoints=@questPoints WHERE playerId=@id");
                    addWithValue(sqlCommand, "@questPoints", qp);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }
        public static int GetPlayerQuestPoints(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT questPoints FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int QuestPoints = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return QuestPoints;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }


        public static void SetPlayerMoney(int money, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET money=@money WHERE playerId=@id");
                    addWithValue(sqlCommand, "@money", money);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static HorseInstance[] GetMostExperiencedHorses()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Horses ORDER BY experience DESC LIMIT 25");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                
            }
            return inst.ToArray();
        }
        public static int[] GetMinigamePlayers()
        {
            List<int> userIds = new List<int>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT DISTINCT playerId, SUM(timesplayed) OVER (PARTITION BY playerId) AS totalPlays FROM Leaderboards ORDER BY totalPlays DESC LIMIT 25");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                
            }
            return userIds.ToArray();
        }
        
        
        public static int[] GetExperiencedPlayers()
        {
            List<int> userIds = new List<int>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM UserExt ORDER BY experience DESC LIMIT 25");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                
            }
            return userIds.ToArray();

        }
        public static int[] GetAdventurousPlayers()
        {
            List<int> userIds = new List<int>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM UserExt ORDER BY questPoints DESC LIMIT 25");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                
            }
            return userIds.ToArray();

        }
        public static int[] GetRichestPlayers()
        {
            List<int> userIds = new List<int>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT playerId FROM UserExt ORDER BY money+bankBalance DESC LIMIT 25");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                
            }
            return userIds.ToArray();
            
        }
        public static HorseInstance[] GetMostSpoiledHorses()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Horses ORDER BY spoiled DESC LIMIT 100");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                
            }
            return inst.ToArray();
        }
        public static HorseInstance[] GetBiggestExpAutoSell()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Horses WHERE autoSell > 0 ORDER BY experience DESC LIMIT 50");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                
            }
            return inst.ToArray();
        }
        public static HorseInstance[] GetCheapestHorseAutoSell()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Horses WHERE autoSell > 0 ORDER BY autoSell LIMIT 100");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                
            }
            return inst.ToArray();
        }
        public static int GetPlayerTotalMinigamesPlayed(int playerId)
        {
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "SELECT SUM(timesplayed) FROM Leaderboards WHERE playerId=@playerId");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count;
            }
        }

        public static void AddNewWinner(int playerId, string gameTitle, int wins, int looses)
        {
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "INSERT INTO Leaderboards VALUES(@playerId,@gameTitle,@wins,@loose,1,0,@type)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                addWithValue(sqlCommand, "@wins", wins);
                addWithValue(sqlCommand, "@loose", looses);
                addWithValue(sqlCommand, "@type", "WINLOSS");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }
        public static void AddNewHighscore(int playerId, string gameTitle, int score, string type)
        {
            using (DbConnection db = connectDb())
            {
                
                DbCommand sqlCommand = createCommand(db, "INSERT INTO Leaderboards VALUES(@playerId,@gameTitle,0,0,1,@score,@type)");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                addWithValue(sqlCommand, "@score", score);
                addWithValue(sqlCommand, "@type", type);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }

        public static Highscore.HighscoreTableEntry[] GetPlayerHighScores(int playerId)
        {
            List<Highscore.HighscoreTableEntry> entires = new List<Highscore.HighscoreTableEntry>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Leaderboards WHERE playerId=@playerId ORDER BY score DESC");
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    Highscore.HighscoreTableEntry highscoreEntry = new Highscore.HighscoreTableEntry();
                    highscoreEntry.UserId = reader.GetInt32(0);
                    highscoreEntry.GameName = reader.GetString(1);
                    highscoreEntry.Wins = reader.GetInt32(2);
                    highscoreEntry.Looses = reader.GetInt32(3);
                    highscoreEntry.TimesPlayed = reader.GetInt32(4);
                    highscoreEntry.Score = reader.GetInt32(5);
                    highscoreEntry.Type = reader.GetString(6);
                    entires.Add(highscoreEntry);
                }


                
                return entires.ToArray();
            }
        }

        public static Highscore.HighscoreTableEntry[] GetTopWinners(string gameTitle, int limit)
        {
            List<Highscore.HighscoreTableEntry> entires = new List<Highscore.HighscoreTableEntry>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY wins DESC LIMIT @limit");
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                addWithValue(sqlCommand, "@limit", limit);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    Highscore.HighscoreTableEntry highscoreEntry = new Highscore.HighscoreTableEntry();
                    highscoreEntry.UserId = reader.GetInt32(0);
                    highscoreEntry.GameName = gameTitle;
                    highscoreEntry.Wins = reader.GetInt32(2);
                    highscoreEntry.Looses = reader.GetInt32(3);
                    highscoreEntry.TimesPlayed = reader.GetInt32(4);
                    highscoreEntry.Score = reader.GetInt32(5);
                    highscoreEntry.Type = reader.GetString(6);
                    entires.Add(highscoreEntry);
                }


                
                return entires.ToArray();
            }
        }

        public static Highscore.HighscoreTableEntry[] GetTopScores(string gameTitle, int limit, bool score=true)
        {
            List<Highscore.HighscoreTableEntry> entires = new List<Highscore.HighscoreTableEntry>();
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, score ? "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score DESC LIMIT @limit" : "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score ASC LIMIT @limit");
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                addWithValue(sqlCommand, "@limit", limit);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                
                while(reader.Read())
                {
                    Highscore.HighscoreTableEntry highscoreEntry = new Highscore.HighscoreTableEntry();
                    highscoreEntry.UserId = reader.GetInt32(0);
                    highscoreEntry.GameName = gameTitle;
                    highscoreEntry.Wins = reader.GetInt32(2);
                    highscoreEntry.Looses = reader.GetInt32(3);
                    highscoreEntry.TimesPlayed = reader.GetInt32(4);
                    highscoreEntry.Score = reader.GetInt32(5);
                    highscoreEntry.Type = reader.GetString(6);
                    entires.Add(highscoreEntry);
                }


                
                return entires.ToArray();
            }
        }

        public static int GetRanking(int score, string gameTitle, bool time=false)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, time ? "SELECT DISTINCT score FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score ASC" : "SELECT DISTINCT score FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score DESC");
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                int i = 1;
                while(reader.Read())
                {
                    if (reader.GetInt32(0) == score)
                        break;
                    i++;
                }

                
                return i;
            }
        }
        public static void UpdateHighscoreWinGame(int playerId, string gameTitle)
        {
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "UPDATE Leaderboards SET wins=wins+1, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }
        public static void UpdateHighscoreLooseGame(int playerId, string gameTitle)
        {
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "UPDATE Leaderboards SET looses=looses+1, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }
        public static void UpdateHighscore(int playerId, string gameTitle, int score)
        {
            using (DbConnection db = connectDb())
            {

                DbCommand sqlCommand = createCommand(db, "UPDATE Leaderboards SET score=@score, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle");
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@gameTitle", gameTitle);
                addWithValue(sqlCommand, "@score", score);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }

        public static void DeleteExpiredLeasedHorsesForOfflinePlayers()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "DELETE FROM Horses WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime <= 0 AND leaser > 0");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }
        public static void TpOfflinePlayersBackToUniterForOfflinePlayers()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "SELECT ownerId, breed, leaser FROM Horses WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime <= 0 AND leaser > 0");
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while(reader.Read())
                {
                    int playerId = reader.GetInt32(0);
                    string horseType = HorseInfo.GetBreedById(reader.GetInt32(1)).Type;
                    int leaserId = reader.GetInt32(2);

                    if(horseType == "pegasus" || horseType == "unicorn")
                    {
                        // find the specific relevant special tile ..
                        World.SpecialTile tile = World.GetSpecialTileById("HORSELEASER", leaserId).First();

                        // set player position to it.
                        SetPlayerX(tile.X, playerId);
                        SetPlayerY(tile.Y, playerId);

                    }
                }
            }
        }

        public static void DecrementHorseLeaseTimeForOfflinePlayers()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE Horses SET leaseTime = leaseTime - 1 WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime > 0 AND leaser > 0");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
            
        }
        public static void IncPlayerTirednessForOfflineUsers()
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET tiredness = tiredness + 1 WHERE playerId NOT IN (SELECT playerId FROM OnlineUsers) AND NOT tiredness +1 > 1000");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static int GetPlayerTiredness(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT tiredness FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int tiredness = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return tiredness;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerTiredness(int id, int tiredness)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET tiredness=@tiredness WHERE playerId=@id");
                    addWithValue(sqlCommand, "@tiredness", tiredness);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerHunger(int id, int hunger)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET hunger=@hunger WHERE playerId=@id");
                    addWithValue(sqlCommand, "@hunger", hunger);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }



        public static int GetPlayerHunger(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT hunger FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int hunger = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return hunger;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerThirst(int id, int thirst)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET thirst=@thirst WHERE playerId=@id");
                    addWithValue(sqlCommand, "@thirst", thirst);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerThirst(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT thirst FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int tiredness = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return tiredness;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static int GetPlayerLastLogin(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT lastLogin FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int lastLogin = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return lastLogin;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerLastLogin(int lastlogin, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET lastLogin=@lastlogin WHERE playerId=@id");
                    addWithValue(sqlCommand, "@lastlogin", lastlogin);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerMoney(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT money FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int Money = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return Money;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static double GetPlayerBankMoney(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT bankBalance FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    double BankMoney = Convert.ToDouble(sqlCommand.ExecuteScalar());

                    
                    return BankMoney;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static double GetPlayerBankInterest(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT bankInterest FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    double BankInterest = Convert.ToDouble(sqlCommand.ExecuteScalar());

                    
                    return BankInterest;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void DoIntrestPayments(int intrestRate)
        {
            if (intrestRate == 0)
            {
                Logger.WarnPrint("Intrest rate is 0, as deviding by 0 causes the universe to implode, adding intrest has been skipped.");
                return;
            }
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET bankInterest = bankInterest + (bankInterest * (1/@interestRate)) WHERE NOT bankInterest + (bankInterest * (1/@interestRate)) > 9999999999.9999");
                addWithValue(sqlCommand, "@interestRate", intrestRate);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void SetPlayerBankInterest(double interest, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET bankInterest=@interest WHERE playerId=@id");
                    addWithValue(sqlCommand, "@interest", interest);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }
        public static void SetPlayerBankMoney(double bankMoney, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET bankBalance=@bankMoney WHERE playerId=@id");
                    addWithValue(sqlCommand, "@bankMoney", bankMoney);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerProfile(string profilePage, int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE UserExt SET profilePage=@profilePage WHERE playerId=@id");
                    addWithValue(sqlCommand, "@profilePage", profilePage);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }

        public static string GetPlayerProfile(int id)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT profilePage FROM UserExt WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    string profilePage = sqlCommand.ExecuteScalar().ToString();

                    
                    return profilePage;
                }
                else
                {
                    throw new InvalidOperationException("Id " + id + " not found in database.");
                }
            }
        }


        public static string GetUsername(int userId)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(userId))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT username FROM Users WHERE playerId=@id");
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    string username = sqlCommand.ExecuteScalar().ToString();

                    
                    return username;
                }
                else
                {
                    throw new InvalidOperationException("Id " + userId + " not found in database.");
                }
            }
        }
        public static void SetPasswordHash(string username, string passhash)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = createCommand(db, "UPDATE Users SET passHash=@hash WHERE username=@name");
                    addWithValue(sqlCommand, "@hash", passhash);
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();
                }
                else
                {
                    throw new InvalidOperationException("Username " + username + " not found in database.");
                }
            }
        }
        public static byte[] GetPasswordHash(string username)
        {
            using (DbConnection db = connectDb())
            {
                
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = createCommand(db, "SELECT passHash FROM Users WHERE username=@name");
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string expectedHash = sqlCommand.ExecuteScalar().ToString();

                    
                    return Helper.StringToByteArray(expectedHash);
                }
                else
                {
                    throw new InvalidOperationException("Username " + username + " not found in database.");
                }
            }
        }
    }

}