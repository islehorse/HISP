using System;
using System.Collections.Generic;
using System.Data.Common;

using HISP.Game;
using HISP.Player;
using HISP.Game.Horse;
using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Security;
using HISP.Game.Services;
using HISP.Game.SwfModules;

using MySqlConnector;
using Microsoft.Data.Sqlite;

namespace HISP.Server
{
    public class Database
    {
        public static string ConnectionString = "";
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
            if (!ConfigReader.SqlLite)
                return new MySqlConnection(ConnectionString);
            else
                return new SqliteConnection(ConnectionString);
        }

        public static void OnShutdown()
        {
            MySqlConnection.ClearAllPools();
            SqliteConnection.ClearAllPools();
        }

        public static void OpenDatabase()
        {
            if (!ConfigReader.SqlLite)
               ConnectionString = "server=" + ConfigReader.DatabaseIP + ";user=" + ConfigReader.DatabaseUsername + ";password=" + ConfigReader.DatabasePassword + ";database=" + ConfigReader.DatabaseName;
            else
               ConnectionString = "Data Source=./" + ConfigReader.DatabaseName + ".db;";

            Logger.InfoPrint(ConnectionString);

            using (DbConnection db = connectDb())
            {
                
                try
                {
                    db.Open();
                }
                catch (DbException e)
                {
                    Logger.ErrorPrint("Failed to connect to Database: "+e.Message);
                    Environment.Exit(1);
                }

                string SqlPragma = "PRAGMA journal_mode=WAL;";

                string UserTable = "CREATE TABLE IF NOT EXISTS Users(Id INT, Username TEXT(16), PassHash TEXT(128), Salt TEXT(128), Gender TEXT(16), Admin TEXT(3), Moderator TEXT(3))";
                string ExtTable = "CREATE TABLE IF NOT EXISTS UserExt(Id INT, X INT, Y INT, LastLogin INT, Money INT, QuestPoints INT, BankBalance DOUBLE, BankInterest DOUBLE, ProfilePage Text(4000),IpAddress TEXT(1028),PrivateNotes Text(65535), CharId INT, ChatViolations INT,Subscriber TEXT(3), SubscribedUntil INT,  Experience INT, Tiredness INT, Hunger INT, Thirst INT, FreeMinutes INT, TotalLogins INT)";
                string MailTable = "CREATE TABLE IF NOT EXISTS Mailbox(RandomId INT, IdTo INT, IdFrom INT, Subject TEXT(100), Message Text(65535), TimeSent INT, BeenRead TEXT(3))";
                string BuddyTable = "CREATE TABLE IF NOT EXISTS BuddyList(Id INT, IdFriend INT)";
                string MessageQueue = "CREATE TABLE IF NOT EXISTS MessageQueue(Id INT, Message TEXT(1028))";
                string WorldTable = "CREATE TABLE World(Time INT, Day INT, Year INT, StartTime INT)";
                string WeatherTable = "CREATE TABLE IF NOT EXISTS Weather(Area TEXT(1028), Weather TEXT(64))";
                string InventoryTable = "CREATE TABLE IF NOT EXISTS Inventory(PlayerID INT, RandomID INT, ItemID INT, Data INT)";
                string ShopInventory = "CREATE TABLE IF NOT EXISTS ShopInventory(ShopID INT, RandomID INT, ItemID INT, Data INT)";
                string DroppedItems = "CREATE TABLE IF NOT EXISTS DroppedItems(X INT, Y INT, RandomID INT, ItemID INT, DespawnTimer INT, Data INT)";
                string TrackedQuest = "CREATE TABLE IF NOT EXISTS TrackedQuest(playerId INT, questId INT, timesCompleted INT)";
                string CompetitionGear = "CREATE TABLE IF NOT EXISTS CompetitionGear(playerId INT, headItem INT, bodyItem INT, legItem INT, feetItem INT)";
                string Awards = "CREATE TABLE IF NOT EXISTS Awards(playerId INT, awardId INT)";
                string Jewelry = "CREATE TABLE IF NOT EXISTS Jewelry(playerId INT, slot1 INT, slot2 INT, slot3 INT, slot4 INT)";
                string AbuseReorts = "CREATE TABLE IF NOT EXISTS AbuseReports(ReportCreator TEXT(1028), Reporting TEXT(1028), ReportReason TEXT(1028))";
                string Leaderboards = "CREATE TABLE IF NOT EXISTS Leaderboards(playerId INT, minigame TEXT(128), wins INT, looses INT, timesplayed INT, score INT, type TEXT(128))";
                string NpcStartPoint = "CREATE TABLE IF NOT EXISTS NpcStartPoint(playerId INT, npcId INT, chatpointId INT)";
                string NpcPos = "CREATE TABLE IF NOT EXISTS NpcPos(npcId INT, X INT, Y INT, UdlrPointer INT)";
                string PoetryRooms = "CREATE TABLE IF NOT EXISTS PoetryRooms(poetId INT, X INT, Y INT, roomId INT)";
                string SavedDrawings = "CREATE TABLE IF NOT EXISTS SavedDrawings(playerId INT, Drawing1 TEXT(65535), Drawing2 TEXT(65535), Drawing3 TEXT(65535))";
                string DrawingRooms = "CREATE TABLE IF NOT EXISTS DrawingRooms(roomId INT, Drawing TEXT(65535))";
                string DressupRooms = "CREATE TABLE IF NOT EXISTS DressupRooms(roomId INT, peiceId INT, active TEXT(3), x INT, y INT)";
                string Horses = "CREATE TABLE IF NOT EXISTS Horses(randomId INT, ownerId INT, leaseTime INT, leaser INT, breed INT, name TEXT(128), description TEXT(4000), sex TEXT(128), color TEXT(128), health INT, shoes INT, hunger INT, thirst INT, mood INT, groom INT, tiredness INT, experience INT, speed INT, strength INT, conformation INT, agility INT, endurance INT, inteligence INT, personality INT, height INT, saddle INT, saddlepad INT, bridle INT, companion INT, autoSell INT, trainTimer INT, category TEXT(128), spoiled INT, magicUsed INT, hidden TEXT(3))";
                string WildHorse = "CREATE TABLE IF NOT EXISTS WildHorse(randomId INT, originalOwner INT, breed INT, x INT, y INT, name TEXT(128), description TEXT(4000), sex TEXT(128), color TEXT(128), health INT, shoes INT, hunger INT, thirst INT, mood INT, groom INT, tiredness INT, experience INT, speed INT, strength INT, conformation INT, agility INT, endurance INT, inteligence INT, personality INT, height INT, saddle INT, saddlepad INT, bridle INT, companion INT, timeout INT, autoSell INT, trainTimer INT, category TEXT(128), spoiled INT, magicUsed INT)";
                string LastPlayer = "CREATE TABLE IF NOT EXISTS LastPlayer(roomId TEXT(1028), playerId INT)";
                string SolvedRealTimeRiddles = "CREATE TABLE IF NOT EXISTS SolvedRealTimeRiddles(playerId INT, riddleId INT)";
                string TrackingStats = "CREATE TABLE IF NOT EXISTS Tracking(playerId INT, what TEXT(128), count INT)";
                string Treasure = "CREATE TABLE IF NOT EXISTS Treasure(randomId INT, x INT, y INT, value INT, type TEXT(128))";
                string Ranches = "CREATE TABLE IF NOT EXISTS Ranches(ranchId INT, playerId INT, title TEXT(50), description TEXT(250), upgradeLevel INT, building1 INT, building2 INT, building3 INT, building4 INT, building5 INT, building6 INT, building7 INT, building8 INT, building9 INT, building10 INT, building11 INT, building12 INT, building13 INT, building14 INT, building15 INT, building16 INT, investedMoney INT)";
                string BannedPlayers = "CREATE TABLE IF NOT EXISTS BannedPlayers(playerId INT, ipAddress TEXT(1028), reason TEXT(1028))";
                string RiddlesComplete = "CREATE TABLE IF NOT EXISTS RiddlesComplete(playerId INT, riddleId INT, solved TEXT(1028))";
                string AuctionTable = "CREATE TABLE IF NOT EXISTS Auctions(roomId INT, randomId INT, horseRandomId INT, ownerId INT, timeRemaining INT, highestBid INT, highestBidder INT, Done TEXT(3))";
                string SolvedRealTimeRiddle = "CREATE TABLE IF NOT EXISTS SolvedRealTimeRiddles(playerId INT, riddleId INT)";
                string MutedPlayers = "CREATE TABLE IF NOT EXISTS MutedPlayers(playerId INT, mutePlayerId INT)";
                string ItemQueue = "CREATE TABLE IF NOT EXISTS ItemPurchaseQueue(playerId INT, itemId INT, count INT)";
                string DeleteOnlineUsers = "DROP TABLE OnlineUsers";
                string OnlineUsers = "CREATE TABLE IF NOT EXISTS OnlineUsers(playerId INT, Admin TEXT(3), Moderator TEXT(3), Subscribed TEXT(3), New TEXT(3))";

                if (ConfigReader.SqlLite)
                {
                    try
                    {
                        DbCommand sqlCommand = db.CreateCommand();
                        sqlCommand.CommandText = SqlPragma;
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Logger.WarnPrint(e.Message);
                    };
                }

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = ItemQueue;
                    sqlCommand.ExecuteNonQuery();                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = SolvedRealTimeRiddles;
                    sqlCommand.ExecuteNonQuery();                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = MutedPlayers;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = MessageQueue;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = SolvedRealTimeRiddle;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DressupRooms;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                
                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = AuctionTable;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = NpcPos;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = RiddlesComplete;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Ranches;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Treasure;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = SavedDrawings;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DrawingRooms;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = TrackingStats;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };



                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Horses;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = UserTable;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = AbuseReorts;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = ExtTable;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = MailTable;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = BuddyTable;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Jewelry;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = WeatherTable;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Awards;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DroppedItems;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = InventoryTable;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = ShopInventory;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = TrackedQuest;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = PoetryRooms;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = BannedPlayers;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = CompetitionGear;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = NpcStartPoint;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = LastPlayer;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = WildHorse;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };



                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = WorldTable;
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "INSERT INTO World VALUES(0,0,0,@startDate)";
                    addWithValue(sqlCommand, "@startDate", (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Leaderboards;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DeleteOnlineUsers;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = OnlineUsers;
                    sqlCommand.ExecuteNonQuery();
                    
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };
            }

        }

        public static void DeleteRanchOwner(int ranchId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT mutePlayerId FROM MutedPlayers WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO MutedPlayers VALUES(@playerId, @mutedPlayerId)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM MutedPlayers WHERE playerId=@playerId AND mutePlayerId=@mutedPlayerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM ItemPurchaseQueue WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM ItemPurchaseQueue WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO DressupRooms VALUES(@roomId, @peiceId, @active, @x, @y)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO MessageQueue VALUES(@id,@message)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM MessageQueue WHERE Id=@id";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT message FROM MessageQueue WHERE Id=@id";
                addWithValue(sqlCommand, "@id", userId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    msgQueue.Add(reader.GetString(0));
                }
                
            }
            return msgQueue.ToArray();
        }

        public static void SetDressupRoomPeiceX(int roomId, int peiceId, int newX)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DressupRooms SET x=@x WHERE roomId=@roomId AND peiceId=@peiceId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DressupRooms SET y=@y WHERE roomId=@roomId AND peiceId=@peiceId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DressupRooms SET active=@active WHERE roomId=@roomId AND peiceId=@peiceId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM DressupRooms WHERE roomId=@roomId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT riddleId FROM SolvedRealTimeRiddles WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT investedMoney FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET upgradeLevel=@upgradeLevel WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET investedMoney=@investedMoney WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET playerId=@ownerId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET description=@description WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET title=@title WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building16=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building15=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building14=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building13=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building12=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building11=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building10=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building9=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building8=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building7=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building6=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building5=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building4=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building3=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building2=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building1=@buildingId WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building16 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building15 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building14 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building13 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building12 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building11 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building10 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building9 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building8 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building7 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building6 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building5 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building4 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building3 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building2 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building1 FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT upgradeLevel FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT description FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT title FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT playerId FROM Ranches WHERE ranchId=@ranchId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM RiddlesComplete WHERE playerId=@playerId AND solved=\"YES\"";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM SolvedRealTimeRiddles WHERE riddleId=@riddleId AND playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO SolvedRealTimeRiddles VALUES(@playerId, @riddleId)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM RiddlesComplete WHERE riddleId=@riddleId AND playerId=@playerId AND solved=\"YES\"";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO RiddlesComplete VALUES(@playerId, @riddleId, \"YES\")";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Ranches VALUES(@ranchId, @playerId, @title, @description, @upgradeLevel, @building1, @building2, @building3, @building4, @building5, @building6, @building7, @building8, @building9, @building10, @building11, @building12, @building13, @building14, @building15, @building16, @investedMoney)";
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
        public static void SetTreasureValue(int randomId, int value)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Treasure SET value=@value WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                addWithValue(sqlCommand, "@value", value);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void DeleteTreasure(int randomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Treasure  WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void AddTreasure(int randomId, int x, int y, int value, string type)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Treasure VALUES(@randomId, @x, @y, @value, @type)";
                addWithValue(sqlCommand, "@randomId", randomId);
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Treasure";
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    int randomId = reader.GetInt32(0);
                    int x = reader.GetInt32(1);
                    int y = reader.GetInt32(2);
                    int value = reader.GetInt32(3);
                    string type = reader.GetString(4);
                    Treasure treasure = new Treasure(x, y, type, randomId, value);
                    treasures.Add(treasure);
                }
                
                return treasures.ToArray();
            }
        }

        public static void AddTrackedItem(int playerId, Tracking.TrackableItem what, int count)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Tracking VALUES(@playerId, @what, @count)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM Tracking WHERE playerId=@playerId AND what=@what";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT count FROM Tracking WHERE playerId=@playerId AND what=@what";
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
                db.Open();

                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM DroppedItems WHERE itemId=@itemId";
                addWithValue(sqlCommand, "@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void DeleteAllItemsFromUsers(int itemId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();

                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Inventory WHERE itemId=@itemId";
                addWithValue(sqlCommand, "@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }


        public static void SetTrackedItemCount(int playerId, Tracking.TrackableItem what, int count)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Tracking SET count=@count WHERE playerId=@playerId AND what=@what";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO LastPlayer VALUES(@roomId,@playerId)";
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SetWildHorseX(int randomId, int x)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE WildHorse SET x=@x WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                addWithValue(sqlCommand, "@x", x);
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetWildHorseTimeout(int randomId, int timeout)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE WildHorse SET timeout=@timeout WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                addWithValue(sqlCommand, "@timeout", timeout);
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void RemoveWildHorse(int randomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM WildHorse WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetWildHorseY(int randomId, int x)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE WildHorse SET y=@y WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                addWithValue(sqlCommand, "@y", x);
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void RemoveHorse(int randomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Horses WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void AddHorse(HorseInstance horse)
        {

            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Horses VALUES(@randomId,@originalOwner,@leaseTime,@leaser,@breed,@name,@description,@sex,@color,@health,@shoes,@hunger,@thirst,@mood,@groom,@tiredness,@experience,@speed,@strength,@conformation,@agility,@endurance,@inteligence,@personality,@height,@saddle,@saddlepad,@bridle,@companion,@autosell,@training,@category,@spoiled,@magicused,@hidden)";

                addWithValue(sqlCommand, "@randomId", horse.RandomId);
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
            int randomId = reader.GetInt32(0);
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

            HorseInstance inst = new HorseInstance(horseBreed, randomId, color, name, description, spoiled, category, magicUsed, autosell, leaseTime, hidden, owner);
            
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE ownerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Auctions WHERE roomId=@roomId";
                addWithValue(sqlCommand, "@roomId", roomId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    int randomId = reader.GetInt32(1);
                    int timeRemaining = reader.GetInt32(4);
                    int highestBid = reader.GetInt32(5);
                    int highestBidder = reader.GetInt32(6);
                    int horseId = reader.GetInt32(2);

                    Auction.AuctionEntry auctionEntry = new Auction.AuctionEntry(timeRemaining, highestBid, highestBidder, randomId);

                    auctionEntry.Horse = GetPlayerHorse(horseId);
                    auctionEntry.OwnerId = reader.GetInt32(3);
                    auctionEntry.Completed = reader.GetString(7) == "YES";
                    auctionEntry.auctionRoomPlacedIn = auction;
                    auction.AddExistingEntry(auctionEntry);

                }

                
            }
        }

        public static void DeleteAuctionRoom(int randomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Auctions WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void AddAuctionRoom(Auction.AuctionEntry entry, int roomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Auctions VALUES(@roomId, @randomId, @horseRandomId, @ownerId, @timeRemaining, @highestBid, @highestBidder, @done)";
                addWithValue(sqlCommand, "@roomId", roomId);
                addWithValue(sqlCommand, "@randomId", entry.RandomId);
                addWithValue(sqlCommand, "@horseRandomId", entry.Horse.RandomId);
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO BannedPlayers VALUES(@playerId,@ipAddress,@reason)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM BannedPlayers WHERE playerId=@playerId";
                addWithValue(sqlCommand, "@playerId", userId);
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static bool IsIpBanned(string ip)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM BannedPlayers WHERE ipAddress=@ipAddr";
                addWithValue(sqlCommand, "@ipAddr", ip);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count >= 1;
            }
        }
        public static bool IsUserBanned(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM BannedPlayers WHERE playerId=@playerId";
                addWithValue(sqlCommand, "@playerId", userId);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count >= 1;
            }

        }

        public static void AddWildHorse(WildHorse horse)
        {

            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO WildHorse VALUES(@randomId,@originalOwner,@breed,@x,@y,@name,@description,@sex,@color,@health,@shoes,@hunger,@thirst,@mood,@groom,@tiredness,@experience,@speed,@strength,@conformation,@agility,@endurance,@inteligence,@personality,@height,@saddle,@saddlepad,@bridle,@companion,@timeout,@autosell,@training,@category,@spoiled,@magicused)";

                addWithValue(sqlCommand, "@randomId", horse.Instance.RandomId);
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM WildHorse";


                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    int randomId = reader.GetInt32(0);
                    int breedId = reader.GetInt32(2);
                    HorseInfo.Breed horseBreed = HorseInfo.GetBreedById(breedId);
                    HorseInstance inst = new HorseInstance(horseBreed, randomId);
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM LastPlayer WHERE roomId=@roomId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT playerId FROM LastPlayer WHERE roomId=@roomId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE LastPlayer SET playerId=@playerId WHERE roomId=@roomId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO PoetryRooms VALUES(@id,@x,@y,@room)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE PoetryRooms SET X=@x, Y=@y WHERE poetId=@id AND roomId=@room";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM PoetryRooms WHERE poetId=@id AND roomId=@room";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT X FROM PoetryRooms WHERE poetId=@id AND roomId=@room";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Y FROM PoetryRooms WHERE poetId=@id AND roomId=@room";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM SavedDrawings WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO SavedDrawings VALUES(@playerId,'','','')";
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static bool DrawingRoomExists(int room)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM DrawingRooms WHERE roomId=@room";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO DrawingRooms VALUES(@roomId,'')";
                addWithValue(sqlCommand, "@roomId", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SetDrawingRoomDrawing(int room, string Drawing)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DrawingRooms SET Drawing=@drawing WHERE roomId=@room";
                addWithValue(sqlCommand, "@drawing", Drawing);
                addWithValue(sqlCommand, "@room", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static string GetDrawingRoomDrawing(int room)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing FROM DrawingRooms WHERE roomId=@room";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing3 FROM SavedDrawings WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing2 FROM SavedDrawings WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing1 FROM SavedDrawings WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE SavedDrawings SET Drawing1=@drawing WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE SavedDrawings SET Drawing2=@drawing WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE SavedDrawings SET Drawing3=@drawing WHERE playerId=@playerId";
                addWithValue(sqlCommand, "@drawing", drawing);
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SetStartTime(int startTime)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE World SET StartTime=@startTimer";
                addWithValue(sqlCommand, "@startTimer", startTime);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetServerTime(int time, int day, int year)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE World SET Time=@time,Day=@day,Year=@year";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Time FROM World";
                int serverTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return serverTime;
            }
        }

        public static int GetServerStartTime()
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT StartTime FROM World";
                int startTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return startTime;
            }
        }

        public static int GetServerDay()
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Day FROM World";
                int serverTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return serverTime;
            }
        }

        public static int GetServerYear()
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Year FROM World";
                int creationTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return creationTime;
            }
        }




        public static bool WeatherExists(string Area)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM Weather WHERE Area=@area";
                addWithValue(sqlCommand, "@area", Area);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count > 0;
            }
        }

        public static void InsertWeather(string Area, string Weather)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Weather VALUES(@area,@weather)";
                addWithValue(sqlCommand, "@weather", Weather);
                addWithValue(sqlCommand, "@area", Area);
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetWeather(string Area, string Weather)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Weather SET Weather=@weather WHERE Area=@area";
                addWithValue(sqlCommand, "@weather", Weather);
                addWithValue(sqlCommand, "@area", Area);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static string GetWeather(string Area)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Weather FROM Weather WHERE Area=@area";
                addWithValue(sqlCommand, "@area", Area);
                string Weather = sqlCommand.ExecuteScalar().ToString();
                
                return Weather;
            }
        }

        public static void DecHorseTrainTimeout()
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET trainTimer=trainTimer-1 WHERE trainTimer-1 > -1";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static HorseInstance[] GetPlayerHorsesInCategory(int playerId, string category)
        {

            List<HorseInstance> instances = new List<HorseInstance>();
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE ownerId=@playerId AND category=@category";
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

        public static HorseInstance GetPlayerHorse(int horseRandomId)
        {
            HorseInstance instance = null;
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE randomId=@horseRandomId";
                addWithValue(sqlCommand, "@horseRandomId", horseRandomId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    instance = ReadHorseInstance(reader);
                    break;
                }
                
                
                if (instance == null)
                    throw new KeyNotFoundException();
                return instance;
            }
        }

        public static int GetHorseTrainTimeout(int horseRandomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT trainTimer FROM Horses WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                int trainTimer = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return trainTimer;
            }
        }

        public static void SetAuctionDone(int randomId, bool done)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET done=@done WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@done", done ? "YES" : "NO");
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetAuctionTimeout(int randomId, int timeRemaining)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET timeRemaining=@timeRemaining WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@timeRemaining", timeRemaining);
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetAuctionHighestBid(int randomId, int highestBid)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET highestBid=@highestBid WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@highestBid", highestBid);
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetAuctionHighestBidder(int randomId, int highestBidder)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET highestBidder=@highestBidder WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@highestBidder", highestBidder);
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static void SetHorseOwner(int randomId, int owner)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET ownerId=@owner WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@owner", owner);
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseHidden(int randomId, bool hidden)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET hidden=@hidden WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@hidden", hidden ? "YES" : "NO");
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseTrainTimeout(int horseRandomId, int trainTimeout)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET trainTimer=@trainTimer WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@trainTimer", trainTimeout);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseColor(int horseRandomId, string Color)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET color=@color WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@color", Color);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseCategory(int horseRandomId, string Category)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET category=@category WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@category", Category);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseAutoSell(int horseRandomId, int AutoSell)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET autosell=@autosell WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@autosell", AutoSell);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseMagicUsed(int horseRandomId, int MagicUsed)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET magicused=@magicused WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@magicused", MagicUsed);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetLeaseTime(int horseRandomId, int leaseTime)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET leaseTime=@leaseTime WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@leaseTime", leaseTime);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseName(int horseRandomId, string Name)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET name=@name WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@name", Name);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseDescription(int horseRandomId, string Description)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET description=@description WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@description", Description);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseTiredness(int horseRandomId, int Tiredness)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET tiredness=@tiredness WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@tiredness", Tiredness);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseSpeed(int horseRandomId, int Speed)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET speed=@speed WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@speed", Speed);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseStrength(int horseRandomId, int Strength)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET strength=@strength WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@strength", Strength);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseConformation(int horseRandomId, int Conformation)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET conformation=@conformation WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@conformation", Conformation);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseAgility(int horseRandomId, int Agility)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET agility=@agility WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@agility", Agility);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseEndurance(int horseRandomId, int Endurance)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET endurance=@endurance WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@endurance", Endurance);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorsePersonality(int horseRandomId, int Personality)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET personality=@personality WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@personality", Personality);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseInteligence(int horseRandomId, int Inteligence)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET inteligence=@inteligence WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@inteligence", Inteligence);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseSpoiled(int horseRandomId, int Spoiled)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET spoiled=@spoiled WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@spoiled", Spoiled);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseExperience(int horseRandomId, int Experience)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET experience=@experience WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@experience", Experience);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseShoes(int horseRandomId, int Shoes)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET shoes=@shoes WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@shoes", Shoes);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseHeight(int horseRandomId, int Height)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET height=@height WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@height", Height);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseMood(int horseRandomId, int Mood)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET mood=@mood WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@mood", Mood);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseGroom(int horseRandomId, int Groom)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET groom=@groom WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@groom", Groom);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetHorseHunger(int horseRandomId, int Hunger)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET hunger=@hunger WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@hunger", Hunger);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseThirst(int horseRandomId, int Thirst)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET thirst=@thirst WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@thirst", Thirst);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetHorseHealth(int horseRandomId, int Health)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET health=@health WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@health", Health);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetSaddle(int horseRandomId, int saddleItemId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddle=@saddle WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@saddle", saddleItemId);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetSaddlePad(int horseRandomId, int saddlePadItemId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddlepad=@saddlepad WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@saddlepad", saddlePadItemId);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetBridle(int horseRandomId, int bridleItemId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET bridle=@bridle WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@bridle", bridleItemId);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetCompanion(int horseRandomId, int companionItemId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET companion=@companion WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@companion", companionItemId);
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearSaddle(int horseRandomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddle=NULL WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearSaddlePad(int horseRandomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddlepad=NULL WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearBridle(int horseRandomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET bridle=NULL WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void ClearCompanion(int horseRandomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET companion=NULL WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetWorldWeather(string Weather)
        {
            using (DbConnection db = connectDb())
            {
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE World SET Weather=@weather";
                addWithValue(sqlCommand, "@weather", Weather);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static byte[] GetPasswordSalt(string username)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Salt FROM Users WHERE Username=@name";
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string expectedHash = sqlCommand.ExecuteScalar().ToString();
                    
                    return Util.StringToByteArray(expectedHash);
                }
                else
                {
                    throw new KeyNotFoundException("Username " + username + " not found in database.");
                }
            }
        }

        public static bool HasJewelry(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM Jewelry WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO Jewelry VALUES(@playerId,0,0,0,0)";
                addWithValue(sqlCommand, "@playerId", playerId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }



        public static void SetJewelrySlot1(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot1=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot1 FROM Jewelry WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot2=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot2 FROM Jewelry WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot3=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot3 FROM Jewelry WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot4=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot4 FROM Jewelry WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT awardId FROM Awards WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO Awards VALUES(@playerId,@awardId)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM CompetitionGear WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO CompetitionGear VALUES(@playerId,0,0,0,0)";
                addWithValue(sqlCommand, "@playerId", playerId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void SetCompetitionGearHeadPeice(int playerId, int itemId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET headItem=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT headItem FROM CompetitionGear WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET bodyItem=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT bodyItem FROM CompetitionGear WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET legItem=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT legItem FROM CompetitionGear WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET feetItem=@itemId WHERE playerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT feetItem FROM CompetitionGear WHERE playerId=@playerId";
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
                    db.Open();
                    DbCommand sqlCommand = db.CreateCommand();

                    sqlCommand.CommandText = "SELECT timesCompleted FROM TrackedQuest WHERE playerId=@playerId AND questId=@questId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(*) FROM TrackedQuest WHERE playerId=@playerId AND questId=@questId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT questId,timesCompleted FROM TrackedQuest WHERE playerId=@playerId";
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
                    db.Open();
                    DbCommand sqlCommand = db.CreateCommand();

                    sqlCommand.CommandText = "UPDATE TrackedQuest SET timesCompleted=@timesCompleted WHERE playerId=@playerId AND questId=@questId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET Subscriber=@subscribed WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Gender FROM Users WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT TotalLogins FROM UserExt WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET TotalLogins=@count WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Experience FROM UserExt WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET Experience=@xp WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET FreeMinutes=FreeMinutes+@minutes";
                addWithValue(sqlCommand, "@minutes", minutes);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetFreeTime(int playerId, int minutes)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET FreeMinutes=@minutes WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT FreeMinutes FROM UserExt WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT SubscribedUntil FROM UserExt WHERE Id=@playerId";
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                int subscribedUntil = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                return subscribedUntil;
            }
        }
        public static bool IsUserAdmin(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Admin FROM Users WHERE Id=@playerId";
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                bool admin = sqlCommand.ExecuteScalar().ToString() == "YES";
                

                return admin;
            }
        }
        public static bool IsUserSubscribed(int playerId)
        {
            if (ConfigReader.AllUsersSubbed)
                return true;

            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Subscriber FROM UserExt WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO TrackedQuest VALUES(@playerId,@questId,@timesCompleted)";
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@questId", questId);
                addWithValue(sqlCommand, "@timesCompleted", timesCompleted);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void AddOnlineUser(int playerId, bool Admin, bool Moderator, bool Subscribed, bool New)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO OnlineUsers VALUES(@playerId, @admin, @moderator, @subscribed, @new)";
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@admin", Admin ? "YES" : "NO");
                addWithValue(sqlCommand, "@moderator", Moderator ? "YES" : "NO");
                addWithValue(sqlCommand, "@subscribed", Subscribed ? "YES" : "NO");
                addWithValue(sqlCommand, "@new", New ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void RemoveOnlineUser(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM OnlineUsers WHERE (playerId=@playerId)";
                addWithValue(sqlCommand, "@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static ItemInstance[] GetShopInventory(int shopId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT ItemId,RandomId,Data FROM ShopInventory WHERE ShopID=@shopId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO ShopInventory VALUES(@shopId,@randomId,@itemId,@data)";
                addWithValue(sqlCommand, "@shopId", shopId);
                addWithValue(sqlCommand, "@randomId", instance.RandomId);
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM ShopInventory WHERE (ShopID=@shopId AND RandomId=@randomId)";
                addWithValue(sqlCommand, "@shopId", shopId);
                addWithValue(sqlCommand, "@randomId", instance.RandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static ItemInstance[] GetPlayerInventory(int playerId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT ItemId,RandomId,Data FROM Inventory WHERE PlayerId=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT id FROM Users WHERE Moderator=\"YES\" AND Admin=\"YES\"";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT id FROM Users";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT id FROM Users WHERE Moderator=\"YES\" OR Admin=\"YES\"";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                
                sqlCommand.CommandText = "INSERT INTO Inventory VALUES(@playerId,@randomId,@itemId, @data)";
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@randomId", instance.RandomId);
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM Inventory WHERE (PlayerId=@playerId AND ItemID=@itemId)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM Inventory WHERE (PlayerId=@playerId AND RandomId=@randomId)";
                addWithValue(sqlCommand, "@playerId", playerId);
                addWithValue(sqlCommand, "@randomId", instance.RandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static bool HasNpcStartpointSet(int playerId, int npcId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM NpcStartPoint WHERE playerId=@playerId AND npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM NpcPos WHERE npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcPos SET Y=@yPos WHERE npcId=@npcId";
                addWithValue(sqlCommand, "@yPos", x);
                addWithValue(sqlCommand, "@npcId", npcId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void SetNpcX(int npcId, int x)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcPos SET X=@xPos WHERE npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcPos SET UdlrPointer=@udlr WHERE npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT UdlrPointer FROM NpcPos WHERE npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT y FROM NpcPos WHERE npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT X FROM NpcPos WHERE npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO NpcPos VALUES(@npcId, @xPos, @yPos, @udlr)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO NpcStartPoint VALUES(@playerId, @npcId, @chatpointId)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcStartPoint SET chatpointId=@chatpointId WHERE playerId=@playerId AND npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM DroppedItems";
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                return count;
            }
        }
        public static int GetNpcStartPoint(int playerId, int npcId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT chatpointId FROM NpcStartPoint WHERE playerId=@playerId AND npcId=@npcId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM DroppedItems WHERE despawnTimer <=0";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }


        public static void RemoveDroppedItem(int randomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM DroppedItems WHERE (RandomId=@randomId)";
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static DroppedItems.DroppedItem[] GetDroppedItems()
        {
            List<DroppedItems.DroppedItem> itemList = new List<DroppedItems.DroppedItem>();
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM DroppedItems";
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
                db.Open();


                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DroppedItems SET DespawnTimer=DespawnTimer-1";
                sqlCommand.ExecuteNonQuery();
                

            }
        }

        public static void AddDroppedItem(DroppedItems.DroppedItem item)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();


                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO DroppedItems VALUES(@x, @y, @randomId, @itemId, @despawnTimer, @data)";
                addWithValue(sqlCommand, "@x", item.X);
                addWithValue(sqlCommand, "@y", item.Y);
                addWithValue(sqlCommand, "@randomId", item.Instance.RandomId);
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                sqlCommand.CommandText = "INSERT INTO AbuseReports VALUES(@reportCreator,@reporting,@reportReason)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "SELECT * FROM Mailbox WHERE IdTo=@toId";
                addWithValue(sqlCommand, "@toId", toId);
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    Mailbox.Mail MailMessage = new Mailbox.Mail();
                    MailMessage.RandomId = RandomID.NextRandomId(reader.GetInt32(0));
                    MailMessage.ToUser = reader.GetInt32(1);
                    MailMessage.FromUser = reader.GetInt32(2);
                    MailMessage.Subject = reader.GetString(3);
                    MailMessage.Message = reader.GetString(4);
                    MailMessage.Timestamp = reader.GetInt32(5);
                    MailMessage.Read = reader.GetString(6) == "YES";
                    mailList.Add(MailMessage);
                }
                
            }
            return mailList.ToArray();
        }
        public static void ReadAllMail(int toId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "UPDATE Mailbox SET BeenRead='YES' WHERE IdTo=@toId";
                addWithValue(sqlCommand, "@toId", toId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void DeleteMail(int randomId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "DELETE FROM Mailbox WHERE randomId=@randomId";
                addWithValue(sqlCommand, "@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }
        public static void AddMail(int randomId, int toId, int fromId, string subject, string message, int timestamp, bool read)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "INSERT INTO Mailbox VALUES(@randomId, @toId,@from,@subject,@message,@time,@read)";
                addWithValue(sqlCommand, "@randomId", randomId);
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

        public static bool CheckUserExist(int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Id=@id";
                addWithValue(sqlCommand, "@id", id);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                return count >= 1;
            }
        }
        public static bool CheckUserExist(string username)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Username=@name";
                addWithValue(sqlCommand, "@name", username);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count >= 1;
            }
        }
        public static bool CheckUserExtExists(int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM UserExt WHERE Id=@id";
                addWithValue(sqlCommand, "@id", id);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                
                return count >= 1;
            }
        }


        public static bool CheckUserIsModerator(string username)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Moderator FROM Users WHERE Username=@name";
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string modStr = sqlCommand.ExecuteScalar().ToString();

                    
                    return modStr == "YES";
                }
                else
                {
                    throw new KeyNotFoundException("Username " + username + " not found in database.");
                }
            }
        }


        public static bool CheckUserIsAdmin(string username)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Admin FROM Users WHERE Username=@name";
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string adminStr = sqlCommand.ExecuteScalar().ToString();

                    
                    return adminStr == "YES";
                }
                else
                {
                    throw new KeyNotFoundException("Username " + username + " not found in database.");
                }
            }
        }

        public static int GetBuddyCount(int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM BuddyList WHERE Id=@id OR IdFriend=@id";
                addWithValue(sqlCommand, "@id", id);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                

                return count;
            }
        }

        public static int[] GetBuddyList(int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (GetBuddyCount(id) <= 0)
                    return new int[0];      // user is forever alone.

                List<int> BuddyList = new List<int>();

                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Id,IdFriend FROM BuddyList WHERE Id=@id OR IdFriend=@id";
                addWithValue(sqlCommand, "@id", id);
                sqlCommand.Prepare();
                DbDataReader dataReader = sqlCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    int adder = dataReader.GetInt32(0);
                    int friend = dataReader.GetInt32(1);
                    if (adder != id)
                        BuddyList.Add(adder);
                    else if (friend != id)
                        BuddyList.Add(friend);
                }

                
                return BuddyList.ToArray();
            }
        }

        public static void RemoveBuddy(int id, int friendId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM BuddyList WHERE (Id=@id AND IdFriend=@friendId) OR (Id=@friendid AND IdFriend=@Id)";
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@friendId", friendId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                
            }
        }

        public static void AddBuddy(int id, int friendId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO BuddyList VALUES(@id,@friendId)";
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@friendId", friendId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }
        public static string GetIpAddress(int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (!CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Does not exist in UserExt.");

                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT IpAddress FROM UserExt WHERE Id=@playerId";
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
                db.Open();
                if (!CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Does not exist in UserExt.");

                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE UserExt SET IpAddress=@ipAddr WHERE Id=@playerId";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT MAX(Id)+1 FROM Users";
                sqlCommand.Prepare();

                object res = sqlCommand.ExecuteScalar();
                if (res == DBNull.Value)
                    return 0;

                return Convert.ToInt32(res);
            }
        }
         
        public static void CreateUser(int id, string username, string passhash, string salt, string gender, bool admin, bool moderator)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Users VALUES(@id,@username,@passhash,@salt,@gender,@admin,@moderator)";
                addWithValue(sqlCommand, "@id", id);
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


        public static void CreateUserExt(int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Allready in UserExt.");

                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO UserExt VALUES(@id,@x,@y,@timestamp,0,0,0,0,'','','',0,0,'NO',0,0,1000,1000,1000, 180,1)";
                addWithValue(sqlCommand, "@id", id);
                addWithValue(sqlCommand, "@timestamp", Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));
                addWithValue(sqlCommand, "@x", Map.NewUserStartX);
                addWithValue(sqlCommand, "@y", Map.NewUserStartY);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static int GetUserid(string username)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Id FROM Users WHERE Username=@name";
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    int userId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return userId;
                }
                else
                {
                    throw new KeyNotFoundException("Username " + username + " not found in database.");
                }
            }
        }

        public static string GetPlayerNotes(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT PrivateNotes FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    string privateNotes = sqlCommand.ExecuteScalar().ToString();

                    
                    return privateNotes;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerNotes(int id, string notes)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET PrivateNotes=@notes WHERE Id=@id";
                    addWithValue(sqlCommand, "@notes", notes);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }


        public static int GetPlayerCharId(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT CharId FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int CharId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return CharId;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerCharId(int charid, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET CharId=@charId WHERE Id=@id";
                    addWithValue(sqlCommand, "@charId", charid);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerX(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT X FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int X = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return X;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerX(int x, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET X=@x WHERE Id=@id";
                    addWithValue(sqlCommand, "@x", x);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerY(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Y FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int Y = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return Y;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static int GetChatViolations(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT ChatViolations FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int violations = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return violations;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }


        public static void SetChatViolations(int violations, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET ChatViolations=@violations WHERE Id=@id";
                    addWithValue(sqlCommand, "@violations", violations);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }
        public static void SetPlayerY(int y, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Y=@y WHERE Id=@id";
                    addWithValue(sqlCommand, "@y", y);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerQuestPoints(int qp, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET QuestPoints=@questPoints WHERE Id=@id";
                    addWithValue(sqlCommand, "@questPoints", qp);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }
        public static int GetPlayerQuestPoints(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT QuestPoints FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int QuestPoints = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return QuestPoints;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }


        public static void SetPlayerMoney(int money, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Money=@money WHERE Id=@id";
                    addWithValue(sqlCommand, "@money", money);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static HorseInstance[] GetMostExperiencedHorses()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (DbConnection db = connectDb())
            {

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses ORDER BY experience DESC LIMIT 25";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT DISTINCT playerId, SUM(timesplayed) OVER (PARTITION BY playerId) AS totalPlays FROM Leaderboards ORDER BY totalPlays DESC LIMIT 25";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT id FROM UserExt ORDER BY Experience DESC LIMIT 25";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT id FROM UserExt ORDER BY QuestPoints DESC LIMIT 25";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT id FROM UserExt ORDER BY Money+BankBalance DESC LIMIT 25";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses ORDER BY spoiled DESC LIMIT 100";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE autoSell > 0 ORDER BY experience DESC LIMIT 50";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE autoSell > 0 ORDER BY autoSell LIMIT 100";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT SUM(timesplayed) FROM Leaderboards WHERE playerId=@playerId";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Leaderboards VALUES(@playerId,@gameTitle,@wins,@loose,1,0,@type)";
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
                
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Leaderboards VALUES(@playerId,@gameTitle,0,0,1,@score,@type)";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Leaderboards WHERE playerId=@playerId ORDER BY score DESC";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY wins DESC LIMIT @limit";
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

        public static Highscore.HighscoreTableEntry[] GetTopScores(string gameTitle, int limit, bool scores=true)
        {
            List<Highscore.HighscoreTableEntry> entires = new List<Highscore.HighscoreTableEntry>();
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                if(scores)
                    sqlCommand.CommandText = "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score DESC LIMIT @limit";
                else
                    sqlCommand.CommandText = "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score ASC LIMIT @limit";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                if(time)
                    sqlCommand.CommandText = "SELECT DISTINCT score FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score ASC";
                else
                    sqlCommand.CommandText = "SELECT DISTINCT score FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score DESC";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Leaderboards SET wins=wins+1, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Leaderboards SET looses=looses+1, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle";
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

                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Leaderboards SET score=@score, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle";
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Horses WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime <= 0 AND leaser > 0";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }
        public static void TpOfflinePlayersBackToUniterForOfflinePlayers()
        {
            using (DbConnection db = connectDb())
            {
                db.Open();

                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT ownerId, breed, leaser FROM Horses WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime <= 0 AND leaser > 0";
                sqlCommand.Prepare();
                DbDataReader reader = sqlCommand.ExecuteReader();

                while(reader.Read())
                {
                    int playerId = reader.GetInt32(0);
                    string horseType = HorseInfo.GetBreedById(reader.GetInt32(1)).Type;
                    int leaserId = reader.GetInt32(2);

                    if(horseType == "pegasus" || horseType == "unicorn")
                    {
                        foreach(World.SpecialTile tile in World.SpecialTiles)
                        {
                            if (tile.Code == null)
                                continue;
                            if(tile.Code.StartsWith("HORSELEASER-"))
                            {
                                int id = int.Parse(tile.Code.Split("-")[1]);
                                if(leaserId == id)
                                {
                                    SetPlayerX(tile.X, playerId);
                                    SetPlayerY(tile.Y, playerId);
                                }
                            }
                        }
                    }


                }

                
                return;
            }
        }

        public static void DecrementHorseLeaseTimeForOfflinePlayers()
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET leaseTime = leaseTime - 1 WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime > 0 AND leaser > 0";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
            
        }
        public static void IncPlayerTirednessForOfflineUsers()
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE UserExt SET tiredness = tiredness + 1 WHERE id NOT IN (SELECT playerId FROM OnlineUsers) AND NOT tiredness +1 > 1000";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
                return;
            }
        }

        public static int GetPlayerTiredness(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Tiredness FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int tiredness = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return tiredness;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerTiredness(int id, int tiredness)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Tiredness=@tiredness WHERE Id=@id";
                    addWithValue(sqlCommand, "@tiredness", tiredness);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerHunger(int id, int hunger)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Hunger=@hunger WHERE Id=@id";
                    addWithValue(sqlCommand, "@Hunger", hunger);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }



        public static int GetPlayerHunger(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Hunger FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int hunger = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return hunger;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerThirst(int id, int thirst)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Thirst=@thirst WHERE Id=@id";
                    addWithValue(sqlCommand, "@thirst", thirst);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerThirst(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Thirst FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int tiredness = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return tiredness;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static int GetPlayerLastLogin(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT LastLogin FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int lastLogin = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return lastLogin;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static void SetPlayerLastLogin(int lastlogin, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET LastLogin=@lastlogin WHERE Id=@id";
                    addWithValue(sqlCommand, "@lastlogin", lastlogin);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerMoney(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Money FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    int Money = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    
                    return Money;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static double GetPlayerBankMoney(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT BankBalance FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    double BankMoney = Convert.ToDouble(sqlCommand.ExecuteScalar());

                    
                    return BankMoney;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }

        public static double GetPlayerBankInterest(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT BankInterest FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    double BankInterest = Convert.ToDouble(sqlCommand.ExecuteScalar());

                    
                    return BankInterest;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
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
                db.Open();
                DbCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE UserExt SET BankInterest = BankInterest + (BankInterest * (1/@interestRate)) WHERE NOT BankInterest + (BankInterest * (1/@interestRate)) > 9999999999.9999";
                addWithValue(sqlCommand, "@interestRate", intrestRate);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                
            }
        }

        public static void SetPlayerBankInterest(double interest, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET BankInterest=@interest WHERE Id=@id";
                    addWithValue(sqlCommand, "@interest", interest);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }
        public static void SetPlayerBankMoney(double bankMoney, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET BankBalance=@bankMoney WHERE Id=@id";
                    addWithValue(sqlCommand, "@bankMoney", bankMoney);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerProfile(string profilePage, int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET ProfilePage=@profilePage WHERE Id=@id";
                    addWithValue(sqlCommand, "@profilePage", profilePage);
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static string GetPlayerProfile(int id)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT ProfilePage FROM UserExt WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", id);
                    sqlCommand.Prepare();
                    string profilePage = sqlCommand.ExecuteScalar().ToString();

                    
                    return profilePage;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }


        public static string GetUsername(int userId)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(userId))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Username FROM Users WHERE Id=@id";
                    addWithValue(sqlCommand, "@id", userId);
                    sqlCommand.Prepare();
                    string username = sqlCommand.ExecuteScalar().ToString();

                    
                    return username;
                }
                else
                {
                    throw new KeyNotFoundException("Id " + userId + " not found in database.");
                }
            }
        }
        public static byte[] GetPasswordHash(string username)
        {
            using (DbConnection db = connectDb())
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    DbCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT PassHash FROM Users WHERE Username=@name";
                    addWithValue(sqlCommand, "@name", username);
                    sqlCommand.Prepare();
                    string expectedHash = sqlCommand.ExecuteScalar().ToString();

                    
                    return Util.StringToByteArray(expectedHash);
                }
                else
                {
                    throw new KeyNotFoundException("Username " + username + " not found in database.");
                }
            }
        }
    }

}