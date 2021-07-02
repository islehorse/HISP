using System;
using System.Collections.Generic;
using MySqlConnector;
using HISP.Game;
using HISP.Player;
using HISP.Game.Horse;
using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Security;
using HISP.Game.Services;
using HISP.Game.SwfModules;

namespace HISP.Server
{
    public class Database
    {
        public static string ConnectionString = "";

        public static void OpenDatabase()
        {
            ConnectionString = "server=" + ConfigReader.DatabaseIP + ";user=" + ConfigReader.DatabaseUsername + ";password=" + ConfigReader.DatabasePassword + ";database=" + ConfigReader.DatabaseName;
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                string UserTable = "CREATE TABLE IF NOT EXISTS Users(Id INT, Username TEXT(16), PassHash TEXT(128), Salt TEXT(128), Gender TEXT(16), Admin TEXT(3), Moderator TEXT(3))";
                string ExtTable = "CREATE TABLE IF NOT EXISTS UserExt(Id INT, X INT, Y INT, LastLogin INT, Money INT, QuestPoints INT, BankBalance DOUBLE, BankInterest DOUBLE, ProfilePage Text(4000),IpAddress TEXT(1028),PrivateNotes Text(65535), CharId INT, ChatViolations INT,Subscriber TEXT(3), SubscribedUntil INT,  Experience INT, Tiredness INT, Hunger INT, Thirst INT, FreeMinutes INT, TotalLogins INT)";
                string MailTable = "CREATE TABLE IF NOT EXISTS Mailbox(RandomId INT, IdTo INT, IdFrom INT, Subject TEXT(100), Message Text(65535), TimeSent INT, BeenRead TEXT(3))";
                string BuddyTable = "CREATE TABLE IF NOT EXISTS BuddyList(Id INT, IdFriend INT)";
                string MessageQueue = "CREATE TABLE IF NOT EXISTS MessageQueue(Id INT, Message TEXT(1028))";
                string WorldTable = "CREATE TABLE World(Time INT, Day INT, Year INT, StartTime INT)";
                string WeatherTable = "CREATE TABLE IF NOT EXISTS Weather(Area TEXT(1028), Weather TEXT(64))";
                string InventoryTable = "CREATE TABLE IF NOT EXISTS Inventory(PlayerID INT, RandomID INT, ItemID INT, Data INT)";
                string ShopInventory = "CREATE TABLE IF NOT EXISTS ShopInventory(ShopID INT, RandomID INT, ItemID INT)";
                string DroppedItems = "CREATE TABLE IF NOT EXISTS DroppedItems(X INT, Y INT, RandomID INT, ItemID INT, DespawnTimer INT, Data INT)";
                string TrackedQuest = "CREATE TABLE IF NOT EXISTS TrackedQuest(playerId INT, questId INT, timesCompleted INT)";
                string OnlineUsers = "CREATE TABLE IF NOT EXISTS OnlineUsers(playerId INT, Admin TEXT(3), Moderator TEXT(3), Subscribed TEXT(3), New TEXT(3))";
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
                string DeleteOnlineUsers = "DELETE FROM OnlineUsers";

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = ItemQueue;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = SolvedRealTimeRiddles;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = MutedPlayers;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = MessageQueue;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = SolvedRealTimeRiddle;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DressupRooms;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                
                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = AuctionTable;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = NpcPos;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = RiddlesComplete;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Ranches;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Treasure;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = SavedDrawings;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DrawingRooms;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = TrackingStats;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };



                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Horses;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = UserTable;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = AbuseReorts;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = ExtTable;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = MailTable;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = BuddyTable;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Jewelry;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = WeatherTable;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Awards;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DroppedItems;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = InventoryTable;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = ShopInventory;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = TrackedQuest;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = PoetryRooms;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = BannedPlayers;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = CompetitionGear;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = NpcStartPoint;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = LastPlayer;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };


                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = WildHorse;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };



                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = WorldTable;
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "INSERT INTO World VALUES(0,0,0,@startDate)";
                    sqlCommand.Parameters.AddWithValue("@startDate", (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };
                try
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = OnlineUsers;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = Leaderboards;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };

                try
                {

                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = DeleteOnlineUsers;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
                catch (Exception e)
                {
                    Logger.WarnPrint(e.Message);
                };
            }

        }

        public static void DeleteRanchOwner(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int[] GetMutedPlayers(int playerId)
        {
            List<int> MutedPlayerIds = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT mutePlayerId FROM MutedPlayers WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                    MutedPlayerIds.Add(reader.GetInt32(0));
                sqlCommand.Dispose();
            }
            return MutedPlayerIds.ToArray();
        }
        public static void AddMutedPlayer(int playerId, int playerToMute)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO MutedPlayers VALUES(@playerId, @mutedPlayerId)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@mutedPlayerId", playerToMute);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void DeleteMutedPlayer(int playerId, int playerToMute)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM MutedPlayers WHERE playerId=@playerId AND mutePlayerId=@mutedPlayerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@mutedPlayerId", playerToMute);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static bool IsRanchOwned(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count >= 1;
            }
        }

        public static void ClearItemPurchaseQueue(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM ItemPurchaseQueue WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static Item.ItemPurchaseQueueItem[] GetItemPurchaseQueue(int playerId)
        {
            List<Item.ItemPurchaseQueueItem> queueItems = new List<Item.ItemPurchaseQueueItem>();

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM ItemPurchaseQueue WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    Item.ItemPurchaseQueueItem itm = new Item.ItemPurchaseQueueItem();
                    itm.ItemId = reader.GetInt32(1);
                    itm.ItemCount = reader.GetInt32(2);
                    queueItems.Add(itm);
                }
                sqlCommand.Dispose();
            }
            return queueItems.ToArray();
        }

        public static void CreateDressupRoomPeice(int roomId, int peiceId, bool active, int x, int y)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO DressupRooms VALUES(@roomId, @peiceId, @active, @x, @y)";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Parameters.AddWithValue("@peiceId", peiceId);
                sqlCommand.Parameters.AddWithValue("@active", active ? "YES" : "NO");
                sqlCommand.Parameters.AddWithValue("@x", x);
                sqlCommand.Parameters.AddWithValue("@y", y);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void AddMessageToQueue(int userId, string message)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO MessageQueue VALUES(@id,@message)";
                sqlCommand.Parameters.AddWithValue("@id", userId);
                sqlCommand.Parameters.AddWithValue("@message", message);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void ClearMessageQueue(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM MessageQueue WHERE Id=@id";
                sqlCommand.Parameters.AddWithValue("@id", userId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static string[] GetMessageQueue(int userId)
        {
            List<string> msgQueue = new List<string>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT message FROM MessageQueue WHERE Id=@id";
                sqlCommand.Parameters.AddWithValue("@id", userId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    msgQueue.Add(reader.GetString(0));
                }
                sqlCommand.Dispose();
            }
            return msgQueue.ToArray();
        }

        public static void SetDressupRoomPeiceX(int roomId, int peiceId, int newX)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DressupRooms SET x=@x WHERE roomId=@roomId AND peiceId=@peiceId";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Parameters.AddWithValue("@peiceId", peiceId);
                sqlCommand.Parameters.AddWithValue("@x", newX);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetDressupRoomPeiceY(int roomId, int peiceId, int newY)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DressupRooms SET y=@y WHERE roomId=@roomId AND peiceId=@peiceId";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Parameters.AddWithValue("@peiceId", peiceId);
                sqlCommand.Parameters.AddWithValue("@y", newY);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetDressupRoomPeiceActive(int roomId, int peiceId, bool active)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DressupRooms SET active=@active WHERE roomId=@roomId AND peiceId=@peiceId";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Parameters.AddWithValue("@peiceId", peiceId);
                sqlCommand.Parameters.AddWithValue("@active", active ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static Dressup.DressupPeice[] LoadDressupRoom(Dressup.DressupRoom room)
        {
            List<Dressup.DressupPeice> peices = new List<Dressup.DressupPeice>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM DressupRooms WHERE roomId=@roomId";
                sqlCommand.Parameters.AddWithValue("@roomId", room.RoomId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    int peiceId = reader.GetInt32(1);
                    bool active = reader.GetString(2) == "YES";
                    int x = reader.GetInt32(3);
                    int y = reader.GetInt32(4);
                    Dressup.DressupPeice peice = new Dressup.DressupPeice(room, peiceId, x, y, active, false);
                    peices.Add(peice);
                }
                sqlCommand.Dispose();
            }
            return peices.ToArray();
        }

        public static int[] GetSolvedRealTimeRiddles(int playerId)
        {
            List<int> solvedRiddleId = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT riddleId FROM SolvedRealTimeRiddles WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    solvedRiddleId.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
                return solvedRiddleId.ToArray();
            }
        }

        public static int GetRanchInvestment(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT investedMoney FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int invested = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return invested;
            }
        }
        public static void SetRanchUpgradeLevel(int ranchId, int upgradeLevel)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET upgradeLevel=@upgradeLevel WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@upgradeLevel", upgradeLevel);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchInvestment(int ranchId, int investedMoney)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET investedMoney=@investedMoney WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@investedMoney", investedMoney);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchOwner(int ranchId, int ownerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET playerId=@ownerId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@ownerId", ownerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchDescription(int ranchId, string description)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET description=@description WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@description", description);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchTitle(int ranchId, string title)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET title=@title WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@title", title);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding16(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building16=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding15(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building15=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding14(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building14=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding13(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building13=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding12(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building12=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding11(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building11=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding10(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building10=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding9(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building9=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding8(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building8=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding7(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building7=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding6(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building6=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding5(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building5=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding4(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building4=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding3(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building3=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding2(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building2=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetRanchBuilding1(int ranchId, int buildingId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Ranches SET building1=@buildingId WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@buildingId", buildingId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetRanchBuilding16(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building16 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding15(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building15 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding14(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building14 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding13(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building13 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding12(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building12 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding11(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building11 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding10(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building10 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding9(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building9 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding8(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building8 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding7(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building7 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding6(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building6 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding5(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building5 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding4(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building4 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding3(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building3 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding2(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building2 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchBuilding1(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT building1 FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int building = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return building;
            }
        }
        public static int GetRanchUpgradeLevel(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT upgradeLevel FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int upgradeLevel = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return upgradeLevel;
            }
        }

        public static string GetRanchDescription(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT description FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                string description = sqlCommand.ExecuteScalar().ToString();
                sqlCommand.Dispose();
                return description;
            }
        }
        public static string GetRanchTitle(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT title FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                string title = sqlCommand.ExecuteScalar().ToString();
                sqlCommand.Dispose();
                return title;
            }
        }
        public static int GetRanchOwner(int ranchId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT playerId FROM Ranches WHERE ranchId=@ranchId";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Prepare();
                int playerId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return playerId;
            }
        }

        public static int TotalRiddlesCompletedByPlayer(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM RiddlesComplete WHERE playerId=@playerId AND solved=\"YES\"";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count;
            }
        }
        public static bool HasPlayerCompletedRealTimeRiddle(int riddleId, int playerId)
        {
            
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM SolvedRealTimeRiddles WHERE riddleId=@riddleId AND playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@riddleId", riddleId);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count >= 1;
            }

        }
        public static void CompleteRealTimeRiddle(int riddleId, int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO SolvedRealTimeRiddles VALUES(@playerId, @riddleId)";
                sqlCommand.Parameters.AddWithValue("@riddleId", riddleId);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static bool HasPlayerCompletedRiddle(int riddleId, int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM RiddlesComplete WHERE riddleId=@riddleId AND playerId=@playerId AND solved=\"YES\"";
                sqlCommand.Parameters.AddWithValue("@riddleId", riddleId);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count >= 1;
            }
        }
        public static void CompleteRiddle(int riddleId, int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO RiddlesComplete VALUES(@playerId, @riddleId, \"YES\")";
                sqlCommand.Parameters.AddWithValue("@riddleId", riddleId);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void AddRanch(int ranchId, int playerId, string title, string description, int upgradeLevel, int building1, int building2, int building3, int building4, int building5, int building6, int building7, int building8, int building9, int building10, int building11, int building12, int building13, int building14, int building15, int building16, int investedMoney)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Ranches VALUES(@ranchId, @playerId, @title, @description, @upgradeLevel, @building1, @building2, @building3, @building4, @building5, @building6, @building7, @building8, @building9, @building10, @building11, @building12, @building13, @building14, @building15, @building16, @investedMoney)";
                sqlCommand.Parameters.AddWithValue("@ranchId", ranchId);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@title", title);
                sqlCommand.Parameters.AddWithValue("@description", description);
                sqlCommand.Parameters.AddWithValue("@upgradeLevel", upgradeLevel);
                sqlCommand.Parameters.AddWithValue("@building1", building1);
                sqlCommand.Parameters.AddWithValue("@building2", building2);
                sqlCommand.Parameters.AddWithValue("@building3", building3);
                sqlCommand.Parameters.AddWithValue("@building4", building4);
                sqlCommand.Parameters.AddWithValue("@building5", building5);
                sqlCommand.Parameters.AddWithValue("@building6", building6);
                sqlCommand.Parameters.AddWithValue("@building7", building7);
                sqlCommand.Parameters.AddWithValue("@building8", building8);
                sqlCommand.Parameters.AddWithValue("@building9", building9);
                sqlCommand.Parameters.AddWithValue("@building10", building10);
                sqlCommand.Parameters.AddWithValue("@building11", building11);
                sqlCommand.Parameters.AddWithValue("@building12", building12);
                sqlCommand.Parameters.AddWithValue("@building13", building13);
                sqlCommand.Parameters.AddWithValue("@building14", building14);
                sqlCommand.Parameters.AddWithValue("@building15", building15);
                sqlCommand.Parameters.AddWithValue("@building16", building16);
                sqlCommand.Parameters.AddWithValue("@investedMoney", investedMoney);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }
        public static void SetTreasureValue(int randomId, int value)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Treasure SET value=@value WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Parameters.AddWithValue("@value", value);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void DeleteTreasure(int randomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Treasure  WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void AddTreasure(int randomId, int x, int y, int value, string type)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Treasure VALUES(@randomId, @x, @y, @value, @type)";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Parameters.AddWithValue("@x", x);
                sqlCommand.Parameters.AddWithValue("@y", y);
                sqlCommand.Parameters.AddWithValue("@value", value);
                sqlCommand.Parameters.AddWithValue("@type", type);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }
        public static Treasure[] GetTreasures()
        {
            List<Treasure> treasures = new List<Treasure>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Treasure";
                MySqlDataReader reader = sqlCommand.ExecuteReader();
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
                sqlCommand.Dispose();
                return treasures.ToArray();
            }
        }

        public static void AddTrackedItem(int playerId, Tracking.TrackableItem what, int count)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Tracking VALUES(@playerId, @what, @count)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@what", what.ToString());
                sqlCommand.Parameters.AddWithValue("@count", count);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static bool HasTrackedItem(int playerId, Tracking.TrackableItem what)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM Tracking WHERE playerId=@playerId AND what=@what";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@what", what.ToString());
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count > 0;
            }
        }
        public static int GetTrackedCount(int playerId, Tracking.TrackableItem what)
        {

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT count FROM Tracking WHERE playerId=@playerId AND what=@what";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@what", what.ToString());
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count;
            }
        }
        public static void DeleteAllDroppedItemsWithId(int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM DroppedItems WHERE itemId=@itemId";
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void DeleteAllItemsFromUsers(int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Inventory WHERE itemId=@itemId";
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }


        public static void SetTrackedItemCount(int playerId, Tracking.TrackableItem what, int count)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Tracking SET count=@count WHERE playerId=@playerId AND what=@what";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@what", what.ToString());
                sqlCommand.Parameters.AddWithValue("@count", count);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void AddLastPlayer(string roomId, int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO LastPlayer VALUES(@roomId,@playerId)";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void SetWildHorseX(int randomId, int x)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE WildHorse SET x=@x WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Parameters.AddWithValue("@x", x);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetWildHorseTimeout(int randomId, int timeout)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE WildHorse SET timeout=@timeout WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Parameters.AddWithValue("@timeout", timeout);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void RemoveWildHorse(int randomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM WildHorse WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetWildHorseY(int randomId, int x)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE WildHorse SET y=@y WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Parameters.AddWithValue("@y", x);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void RemoveHorse(int randomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Horses WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void AddHorse(HorseInstance horse)
        {

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Horses VALUES(@randomId,@originalOwner,@leaseTime,@leaser,@breed,@name,@description,@sex,@color,@health,@shoes,@hunger,@thirst,@mood,@groom,@tiredness,@experience,@speed,@strength,@conformation,@agility,@endurance,@inteligence,@personality,@height,@saddle,@saddlepad,@bridle,@companion,@autosell,@training,@category,@spoiled,@magicused,@hidden)";

                sqlCommand.Parameters.AddWithValue("@randomId", horse.RandomId);
                sqlCommand.Parameters.AddWithValue("@originalOwner", horse.Owner);
                sqlCommand.Parameters.AddWithValue("@leaseTime", horse.LeaseTime);
                sqlCommand.Parameters.AddWithValue("@leaser", horse.Leaser);
                sqlCommand.Parameters.AddWithValue("@breed", horse.Breed.Id);
                sqlCommand.Parameters.AddWithValue("@name", horse.Name);
                sqlCommand.Parameters.AddWithValue("@description", horse.Description);
                sqlCommand.Parameters.AddWithValue("@sex", horse.Gender);
                sqlCommand.Parameters.AddWithValue("@color", horse.Color);

                sqlCommand.Parameters.AddWithValue("@health", horse.BasicStats.Health);
                sqlCommand.Parameters.AddWithValue("@shoes", horse.BasicStats.Shoes);
                sqlCommand.Parameters.AddWithValue("@hunger", horse.BasicStats.Hunger);
                sqlCommand.Parameters.AddWithValue("@thirst", horse.BasicStats.Thirst);
                sqlCommand.Parameters.AddWithValue("@mood", horse.BasicStats.Mood);
                sqlCommand.Parameters.AddWithValue("@groom", horse.BasicStats.Groom);
                sqlCommand.Parameters.AddWithValue("@tiredness", horse.BasicStats.Tiredness);
                sqlCommand.Parameters.AddWithValue("@experience", horse.BasicStats.Experience);

                sqlCommand.Parameters.AddWithValue("@speed", horse.AdvancedStats.Speed);
                sqlCommand.Parameters.AddWithValue("@strength", horse.AdvancedStats.Strength);
                sqlCommand.Parameters.AddWithValue("@conformation", horse.AdvancedStats.Conformation);
                sqlCommand.Parameters.AddWithValue("@agility", horse.AdvancedStats.Agility);
                sqlCommand.Parameters.AddWithValue("@endurance", horse.AdvancedStats.Endurance);
                sqlCommand.Parameters.AddWithValue("@inteligence", horse.AdvancedStats.Inteligence);
                sqlCommand.Parameters.AddWithValue("@personality", horse.AdvancedStats.Personality);
                sqlCommand.Parameters.AddWithValue("@height", horse.AdvancedStats.Height);

                if (horse.Equipment.Saddle != null)
                    sqlCommand.Parameters.AddWithValue("@saddle", horse.Equipment.Saddle.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@saddle", null);

                if (horse.Equipment.SaddlePad != null)
                    sqlCommand.Parameters.AddWithValue("@saddlepad", horse.Equipment.SaddlePad.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@saddlepad", null);

                if (horse.Equipment.Bridle != null)
                    sqlCommand.Parameters.AddWithValue("@bridle", horse.Equipment.Bridle.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@bridle", null);

                if (horse.Equipment.Companion != null)
                    sqlCommand.Parameters.AddWithValue("@companion", horse.Equipment.Companion.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@companion", null);





                sqlCommand.Parameters.AddWithValue("@autosell", horse.AutoSell);
                sqlCommand.Parameters.AddWithValue("@training", horse.TrainTimer);
                sqlCommand.Parameters.AddWithValue("@category", horse.Category);
                sqlCommand.Parameters.AddWithValue("@spoiled", horse.Spoiled);
                sqlCommand.Parameters.AddWithValue("@magicused", horse.MagicUsed);

                sqlCommand.Parameters.AddWithValue("@hidden", horse.Hidden ? "YES" : "NO");

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }

        }

        public static HorseInstance ReadHorseInstance(MySqlDataReader reader)
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE ownerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    inv.AddHorse(ReadHorseInstance(reader), false);

                }

                sqlCommand.Dispose();
            }
        }

        public static void LoadAuctionRoom(Auction auction, int roomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Auctions WHERE roomId=@roomId";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();

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
                    auction.AuctionEntries.Add(auctionEntry);

                }

                sqlCommand.Dispose();
            }
        }

        public static void DeleteAuctionRoom(int randomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Auctions WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void AddAuctionRoom(Auction.AuctionEntry entry, int roomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Auctions VALUES(@roomId, @randomId, @horseRandomId, @ownerId, @timeRemaining, @highestBid, @highestBidder, @done)";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Parameters.AddWithValue("@randomId", entry.RandomId);
                sqlCommand.Parameters.AddWithValue("@horseRandomId", entry.Horse.RandomId);
                sqlCommand.Parameters.AddWithValue("@ownerId", entry.OwnerId);
                sqlCommand.Parameters.AddWithValue("@timeRemaining", entry.TimeRemaining);
                sqlCommand.Parameters.AddWithValue("@highestBid", entry.HighestBid);
                sqlCommand.Parameters.AddWithValue("@highestBidder", entry.HighestBidder);
                sqlCommand.Parameters.AddWithValue("@done", entry.Completed ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void BanUser(int userId, string ip, string reason)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO BannedPlayers VALUES(@playerId,@ipAddress,@reason)";
                sqlCommand.Parameters.AddWithValue("@playerId", userId);
                sqlCommand.Parameters.AddWithValue("@ipAddress", ip);
                sqlCommand.Parameters.AddWithValue("@reason", reason);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void UnBanUser(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM BannedPlayers WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", userId);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }


        public static bool IsIpBanned(string ip)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM BannedPlayers WHERE ipAddress=@ipAddr";
                sqlCommand.Parameters.AddWithValue("@ipAddr", ip);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count >= 1;
            }
        }
        public static bool IsUserBanned(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM BannedPlayers WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", userId);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count >= 1;
            }

        }

        public static void AddWildHorse(WildHorse horse)
        {

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO WildHorse VALUES(@randomId,@originalOwner,@breed,@x,@y,@name,@description,@sex,@color,@health,@shoes,@hunger,@thirst,@mood,@groom,@tiredness,@experience,@speed,@strength,@conformation,@agility,@endurance,@inteligence,@personality,@height,@saddle,@saddlepad,@bridle,@companion,@timeout,@autosell,@training,@category,@spoiled,@magicused)";

                sqlCommand.Parameters.AddWithValue("@randomId", horse.Instance.RandomId);
                sqlCommand.Parameters.AddWithValue("@originalOwner", horse.Instance.Owner);
                sqlCommand.Parameters.AddWithValue("@breed", horse.Instance.Breed.Id);
                sqlCommand.Parameters.AddWithValue("@x", horse.X);
                sqlCommand.Parameters.AddWithValue("@y", horse.Y);
                sqlCommand.Parameters.AddWithValue("@name", horse.Instance.Name);
                sqlCommand.Parameters.AddWithValue("@description", horse.Instance.Description);
                sqlCommand.Parameters.AddWithValue("@sex", horse.Instance.Gender);
                sqlCommand.Parameters.AddWithValue("@color", horse.Instance.Color);

                sqlCommand.Parameters.AddWithValue("@health", horse.Instance.BasicStats.Health);
                sqlCommand.Parameters.AddWithValue("@shoes", horse.Instance.BasicStats.Shoes);
                sqlCommand.Parameters.AddWithValue("@hunger", horse.Instance.BasicStats.Hunger);
                sqlCommand.Parameters.AddWithValue("@thirst", horse.Instance.BasicStats.Thirst);
                sqlCommand.Parameters.AddWithValue("@mood", horse.Instance.BasicStats.Mood);
                sqlCommand.Parameters.AddWithValue("@groom", horse.Instance.BasicStats.Groom);
                sqlCommand.Parameters.AddWithValue("@tiredness", horse.Instance.BasicStats.Tiredness);
                sqlCommand.Parameters.AddWithValue("@experience", horse.Instance.BasicStats.Experience);

                sqlCommand.Parameters.AddWithValue("@speed", horse.Instance.AdvancedStats.Speed);
                sqlCommand.Parameters.AddWithValue("@strength", horse.Instance.AdvancedStats.Strength);
                sqlCommand.Parameters.AddWithValue("@conformation", horse.Instance.AdvancedStats.Conformation);
                sqlCommand.Parameters.AddWithValue("@agility", horse.Instance.AdvancedStats.Agility);
                sqlCommand.Parameters.AddWithValue("@endurance", horse.Instance.AdvancedStats.Endurance);
                sqlCommand.Parameters.AddWithValue("@inteligence", horse.Instance.AdvancedStats.Inteligence);
                sqlCommand.Parameters.AddWithValue("@personality", horse.Instance.AdvancedStats.Personality);
                sqlCommand.Parameters.AddWithValue("@height", horse.Instance.AdvancedStats.Height);

                if (horse.Instance.Equipment.Saddle != null)
                    sqlCommand.Parameters.AddWithValue("@saddle", horse.Instance.Equipment.Saddle.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@saddle", null);

                if (horse.Instance.Equipment.SaddlePad != null)
                    sqlCommand.Parameters.AddWithValue("@saddlepad", horse.Instance.Equipment.SaddlePad.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@saddlepad", null);

                if (horse.Instance.Equipment.Bridle != null)
                    sqlCommand.Parameters.AddWithValue("@bridle", horse.Instance.Equipment.Bridle.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@bridle", null);

                if (horse.Instance.Equipment.Companion != null)
                    sqlCommand.Parameters.AddWithValue("@companion", horse.Instance.Equipment.Companion.Id);
                else
                    sqlCommand.Parameters.AddWithValue("@companion", null);





                sqlCommand.Parameters.AddWithValue("@timeout", horse.Timeout);
                sqlCommand.Parameters.AddWithValue("@autosell", horse.Instance.AutoSell);
                sqlCommand.Parameters.AddWithValue("@training", horse.Instance.TrainTimer);
                sqlCommand.Parameters.AddWithValue("@category", horse.Instance.Category);
                sqlCommand.Parameters.AddWithValue("@spoiled", horse.Instance.Spoiled);
                sqlCommand.Parameters.AddWithValue("@magicused", horse.Instance.MagicUsed);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }

        }


        public static void LoadWildHorses()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM WildHorse";


                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();

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

                sqlCommand.Dispose();
            }
        }

        public static bool LastPlayerExist(string roomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM LastPlayer WHERE roomId=@roomId";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count > 0;
            }
        }

        public static int GetLastPlayer(string roomId)
        {
            if (!Database.LastPlayerExist(roomId))
                Database.AddLastPlayer(roomId, -1);

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT playerId FROM LastPlayer WHERE roomId=@roomId";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Prepare();
                int playerId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return playerId;
            }
        }


        public static void SetLastPlayer(string roomId, int playerId)
        {
            if (!Database.LastPlayerExist(roomId))
                Database.AddLastPlayer(roomId, -1);
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE LastPlayer SET playerId=@playerId WHERE roomId=@roomId";
                sqlCommand.Parameters.AddWithValue("@roomId", roomId);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void AddPoetWord(int id, int x, int y, int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO PoetryRooms VALUES(@id,@x,@y,@room)";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@x", x);
                sqlCommand.Parameters.AddWithValue("@y", y);
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void SetPoetPosition(int id, int x, int y, int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE PoetryRooms SET X=@x, Y=@y WHERE poetId=@id AND roomId=@room";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@x", x);
                sqlCommand.Parameters.AddWithValue("@y", y);
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static bool GetPoetExist(int id, int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM PoetryRooms WHERE poetId=@id AND roomId=@room";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count > 0;
            }
        }
        public static int GetPoetPositionX(int id, int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT X FROM PoetryRooms WHERE poetId=@id AND roomId=@room";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                int xpos = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return xpos;
            }
        }

        public static int GetPoetPositionY(int id, int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Y FROM PoetryRooms WHERE poetId=@id AND roomId=@room";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                int ypos = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return ypos;
            }
        }

        public static bool SavedDrawingsExist(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM SavedDrawings WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count >= 1;
            }
        }
        public static void CreateSavedDrawings(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO SavedDrawings VALUES(@playerId,'','','')";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static bool DrawingRoomExists(int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM DrawingRooms WHERE roomId=@room";
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count >= 1;
            }
        }

        public static void CreateDrawingRoom(int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO DrawingRooms VALUES(@roomId,'')";
                sqlCommand.Parameters.AddWithValue("@roomId", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void SetDrawingRoomDrawing(int room, string Drawing)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DrawingRooms SET Drawing=@drawing WHERE roomId=@room";
                sqlCommand.Parameters.AddWithValue("@drawing", Drawing);
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }
        public static string GetDrawingRoomDrawing(int room)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing FROM DrawingRooms WHERE roomId=@room";
                sqlCommand.Parameters.AddWithValue("@room", room);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                sqlCommand.Dispose();
                return drawing;
            }
        }
        public static string LoadDrawingSlot3(int playerId)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing3 FROM SavedDrawings WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                sqlCommand.Dispose();
                return drawing;
            }
        }
        public static string LoadDrawingSlot2(int playerId)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing2 FROM SavedDrawings WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                sqlCommand.Dispose();
                return drawing;
            }
        }
        public static string LoadDrawingSlot1(int playerId)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Drawing1 FROM SavedDrawings WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                string drawing = sqlCommand.ExecuteScalar().ToString();

                sqlCommand.Dispose();
                return drawing;
            }
        }
        public static void SaveDrawingSlot1(int playerId, string drawing)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE SavedDrawings SET Drawing1=@drawing WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@drawing", drawing);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void SaveDrawingSlot2(int playerId, string drawing)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE SavedDrawings SET Drawing2=@drawing WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@drawing", drawing);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void SaveDrawingSlot3(int playerId, string drawing)
        {
            if (!SavedDrawingsExist(playerId))
                CreateSavedDrawings(playerId);
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE SavedDrawings SET Drawing3=@drawing WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@drawing", drawing);
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void SetStartTime(int startTime)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE World SET StartTime=@startTimer";
                sqlCommand.Parameters.AddWithValue("@startTimer", startTime);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetServerTime(int time, int day, int year)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE World SET Time=@time,Day=@day,Year=@year";
                sqlCommand.Parameters.AddWithValue("@time", time);
                sqlCommand.Parameters.AddWithValue("@day", day);
                sqlCommand.Parameters.AddWithValue("@year", year);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static int GetServerTime()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Time FROM World";
                int serverTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return serverTime;
            }
        }

        public static int GetServerStartTime()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT StartTime FROM World";
                int startTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return startTime;
            }
        }

        public static int GetServerDay()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Day FROM World";
                int serverTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return serverTime;
            }
        }

        public static int GetServerYear()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Year FROM World";
                int creationTime = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return creationTime;
            }
        }




        public static bool WeatherExists(string Area)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(*) FROM Weather WHERE Area=@area";
                sqlCommand.Parameters.AddWithValue("@area", Area);
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count > 0;
            }
        }

        public static void InsertWeather(string Area, string Weather)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Weather VALUES(@area,@weather)";
                sqlCommand.Parameters.AddWithValue("@weather", Weather);
                sqlCommand.Parameters.AddWithValue("@area", Area);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetWeather(string Area, string Weather)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Weather SET Weather=@weather WHERE Area=@area";
                sqlCommand.Parameters.AddWithValue("@weather", Weather);
                sqlCommand.Parameters.AddWithValue("@area", Area);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }
        public static string GetWeather(string Area)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Weather FROM Weather WHERE Area=@area";
                sqlCommand.Parameters.AddWithValue("@area", Area);
                string Weather = sqlCommand.ExecuteScalar().ToString();
                sqlCommand.Dispose();
                return Weather;
            }
        }

        public static void DecHorseTrainTimeout()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET trainTimer=trainTimer-1 WHERE trainTimer-1 > -1";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }


        public static HorseInstance[] GetPlayerHorsesInCategory(int playerId, string category)
        {

            List<HorseInstance> instances = new List<HorseInstance>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE ownerId=@playerId AND category=@category";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@category", category);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    instances.Add(ReadHorseInstance(reader));
                }
                sqlCommand.Dispose();
                return instances.ToArray(); 
            }
        }

        public static HorseInstance GetPlayerHorse(int horseRandomId)
        {
            HorseInstance instance = null;
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses WHERE randomId=@horseRandomId";
                sqlCommand.Parameters.AddWithValue("@horseRandomId", horseRandomId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    instance = ReadHorseInstance(reader);
                    break;
                }
                
                sqlCommand.Dispose();
                if (instance == null)
                    throw new KeyNotFoundException();
                return instance;
            }
        }

        public static int GetHorseTrainTimeout(int horseRandomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT trainTimer FROM Horses WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                int trainTimer = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return trainTimer;
            }
        }

        public static void SetAuctionDone(int randomId, bool done)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET done=@done WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@done", done ? "YES" : "NO");
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetAuctionTimeout(int randomId, int timeRemaining)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET timeRemaining=@timeRemaining WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@timeRemaining", timeRemaining);
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetAuctionHighestBid(int randomId, int highestBid)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET highestBid=@highestBid WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@highestBid", highestBid);
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetAuctionHighestBidder(int randomId, int highestBidder)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Auctions SET highestBidder=@highestBidder WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@highestBidder", highestBidder);
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }


        public static void SetHorseOwner(int randomId, int owner)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET ownerId=@owner WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@owner", owner);
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetHorseHidden(int randomId, bool hidden)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET hidden=@hidden WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@hidden", hidden ? "YES" : "NO");
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseTrainTimeout(int horseRandomId, int trainTimeout)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET trainTimer=@trainTimer WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@trainTimer", trainTimeout);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseColor(int horseRandomId, string Color)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET color=@color WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@color", Color);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseCategory(int horseRandomId, string Category)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET category=@category WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@category", Category);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetHorseAutoSell(int horseRandomId, int AutoSell)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET autosell=@autosell WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@autosell", AutoSell);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseMagicUsed(int horseRandomId, int MagicUsed)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET magicused=@magicused WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@magicused", MagicUsed);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetLeaseTime(int horseRandomId, int leaseTime)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET leaseTime=@leaseTime WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@leaseTime", leaseTime);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetHorseName(int horseRandomId, string Name)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET name=@name WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@name", Name);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseDescription(int horseRandomId, string Description)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET description=@description WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@description", Description);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseTiredness(int horseRandomId, int Tiredness)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET tiredness=@tiredness WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@tiredness", Tiredness);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseSpeed(int horseRandomId, int Speed)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET speed=@speed WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@speed", Speed);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseStrength(int horseRandomId, int Strength)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET strength=@strength WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@strength", Strength);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseConformation(int horseRandomId, int Conformation)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET conformation=@conformation WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@conformation", Conformation);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseAgility(int horseRandomId, int Agility)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET agility=@agility WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@agility", Agility);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseEndurance(int horseRandomId, int Endurance)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET endurance=@endurance WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@endurance", Endurance);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorsePersonality(int horseRandomId, int Personality)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET personality=@personality WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@personality", Personality);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseInteligence(int horseRandomId, int Inteligence)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET inteligence=@inteligence WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@inteligence", Inteligence);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseSpoiled(int horseRandomId, int Spoiled)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET spoiled=@spoiled WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@spoiled", Spoiled);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseExperience(int horseRandomId, int Experience)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET experience=@experience WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@experience", Experience);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseShoes(int horseRandomId, int Shoes)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET shoes=@shoes WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@shoes", Shoes);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseHeight(int horseRandomId, int Height)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET height=@height WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@height", Height);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseMood(int horseRandomId, int Mood)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET mood=@mood WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@mood", Mood);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseGroom(int horseRandomId, int Groom)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET groom=@groom WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@groom", Groom);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetHorseHunger(int horseRandomId, int Hunger)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET hunger=@hunger WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@hunger", Hunger);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseThirst(int horseRandomId, int Thirst)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET thirst=@thirst WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@thirst", Thirst);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetHorseHealth(int horseRandomId, int Health)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET health=@health WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@health", Health);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetSaddle(int horseRandomId, int saddleItemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddle=@saddle WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@saddle", saddleItemId);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetSaddlePad(int horseRandomId, int saddlePadItemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddlepad=@saddlepad WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@saddlepad", saddlePadItemId);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetBridle(int horseRandomId, int bridleItemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET bridle=@bridle WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@bridle", bridleItemId);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetCompanion(int horseRandomId, int companionItemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET companion=@companion WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@companion", companionItemId);
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void ClearSaddle(int horseRandomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddle=NULL WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void ClearSaddlePad(int horseRandomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET saddlepad=NULL WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void ClearBridle(int horseRandomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET bridle=NULL WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void ClearCompanion(int horseRandomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET companion=NULL WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", horseRandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetWorldWeather(string Weather)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE World SET Weather=@weather";
                sqlCommand.Parameters.AddWithValue("@weather", Weather);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static byte[] GetPasswordSalt(string username)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Salt FROM Users WHERE Username=@name";
                    sqlCommand.Parameters.AddWithValue("@name", username);
                    sqlCommand.Prepare();
                    string expectedHash = sqlCommand.ExecuteScalar().ToString();
                    sqlCommand.Dispose();
                    return Converters.StringToByteArray(expectedHash);
                }
                else
                {
                    throw new KeyNotFoundException("Username " + username + " not found in database.");
                }
            }
        }

        public static bool HasJewelry(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM Jewelry WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);

                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete > 0;
            }
        }

        public static void InitJewelry(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO Jewelry VALUES(@playerId,0,0,0,0)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }



        public static void SetJewelrySlot1(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot1=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetJewelrySlot1(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot1 FROM Jewelry WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }

        public static void SetJewelrySlot2(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot2=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetJewelrySlot2(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot2 FROM Jewelry WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }


        public static void SetJewelrySlot3(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot3=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetJewelrySlot3(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot3 FROM Jewelry WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }

        public static void SetJewelrySlot4(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE Jewelry SET slot4=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetJewelrySlot4(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT slot4 FROM Jewelry WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }
        


        public static int[] GetAwards(int playerId)
        {
            List<int> Awards = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT awardId FROM Awards WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);

                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    Awards.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
                return Awards.ToArray();
            }
        }
        public static void AddAward(int playerId, int awardId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO Awards VALUES(@playerId,@awardId)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@awardId", awardId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                return;
            }
        }


        public static bool HasCompetitionGear(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM CompetitionGear WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);

                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete > 0;
            }
        }

        public static void InitCompetitionGear(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO CompetitionGear VALUES(@playerId,0,0,0,0)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void SetCompetitionGearHeadPeice(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET headItem=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetCompetitionGearHeadPeice(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT headItem FROM CompetitionGear WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }

        public static void SetCompetitionGearBodyPeice(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET bodyItem=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetCompetitionGearBodyPeice(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT bodyItem FROM CompetitionGear WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }

        public static void SetCompetitionGearLegPeice(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET legItem=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetCompetitionGearLegPeice(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT legItem FROM CompetitionGear WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }

        public static void SetCompetitionGearFeetPeice(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE CompetitionGear SET feetItem=@itemId WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);

                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetCompetitionGearFeetPeice(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT feetItem FROM CompetitionGear WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return timesComplete;
            }
        }

        public static int GetTrackedQuestCompletedCount(int playerId, int questId)
        {
            if(CheckTrackeQuestExists(playerId,questId))
            {

                using (MySqlConnection db = new MySqlConnection(ConnectionString))
                {
                    db.Open();
                    MySqlCommand sqlCommand = db.CreateCommand();

                    sqlCommand.CommandText = "SELECT timesCompleted FROM TrackedQuest WHERE playerId=@playerId AND questId=@questId";
                    sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                    sqlCommand.Parameters.AddWithValue("@questId", questId);
                    sqlCommand.Prepare();
                    int timesComplete = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(*) FROM TrackedQuest WHERE playerId=@playerId AND questId=@questId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@questId", questId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();

                if (count >= 1)
                    return true;
                else
                    return false;
            }

        }

        public static TrackedQuest[] GetTrackedQuests(int playerId)
        {
            List<TrackedQuest> TrackedQuests = new List<TrackedQuest>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT questId,timesCompleted FROM TrackedQuest WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    TrackedQuest TrackedQuest = new TrackedQuest(playerId, reader.GetInt32(0), reader.GetInt32(1));
                    TrackedQuests.Add(TrackedQuest);
                }
                sqlCommand.Dispose();
            }
            return TrackedQuests.ToArray();
        }
        public static void SetTrackedQuestCompletedCount(int playerId, int questId, int timesCompleted)
        {
            if(CheckTrackeQuestExists(playerId,questId))
            {
                using (MySqlConnection db = new MySqlConnection(ConnectionString))
                {
                    db.Open();
                    MySqlCommand sqlCommand = db.CreateCommand();

                    sqlCommand.CommandText = "UPDATE TrackedQuest SET timesCompleted=@timesCompleted WHERE playerId=@playerId AND questId=@questId";
                    sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                    sqlCommand.Parameters.AddWithValue("@questId", questId);
                    sqlCommand.Parameters.AddWithValue("@timesCompleted", timesCompleted);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }
            }
            else
            {
                AddNewTrackedQuest(playerId, questId, timesCompleted);
            }

        }
        public static bool SetUserSubscriptionStatus(int playerId, bool subscribed)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET Subscriber=@subscribed WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@subscribed", subscribed ? "YES" : "NO");
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();

                return subscribed;
            }
        }
        public static string GetGender(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Gender FROM Users WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                string gender = sqlCommand.ExecuteScalar().ToString();
                sqlCommand.Dispose();

                return gender;
            }
        }
        public static int GetLoginCount(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT TotalLogins FROM UserExt WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();

                return count;
            }
        }
        public static void SetLoginCount(int playerId, int count)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET TotalLogins=@count WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@count", count);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetExperience(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Experience FROM UserExt WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int xp = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();

                return xp;
            }
        }
        public static void SetExperience(int playerId, int exp)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET Experience=@xp WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@xp", exp);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void IncAllUsersFreeTime(int minutes)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET FreeMinutes=FreeMinutes+@minutes";
                sqlCommand.Parameters.AddWithValue("@minutes", minutes);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetFreeTime(int playerId, int minutes)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE UserExt SET FreeMinutes=@minutes WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@minutes", minutes);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetFreeTime(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT FreeMinutes FROM UserExt WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int freeMinutes = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();

                return freeMinutes;
            }
        }
        public static int GetUserSubscriptionExpireDate(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT SubscribedUntil FROM UserExt WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int subscribedUntil = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();

                return subscribedUntil;
            }
        }
        public static bool IsUserAdmin(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Admin FROM Users WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                bool admin = sqlCommand.ExecuteScalar().ToString() == "YES";
                sqlCommand.Dispose();

                return admin;
            }
        }
        public static bool IsUserSubscribed(int playerId)
        {
            if (ConfigReader.AllUsersSubbed)
                return true;

            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT Subscriber FROM UserExt WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                bool subscribed = sqlCommand.ExecuteScalar().ToString() == "YES";
                sqlCommand.Dispose();

                return subscribed; 
            }
        }
        public static void AddNewTrackedQuest(int playerId, int questId, int timesCompleted)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO TrackedQuest VALUES(@playerId,@questId,@timesCompleted)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@questId", questId);
                sqlCommand.Parameters.AddWithValue("@timesCompleted", timesCompleted);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void AddOnlineUser(int playerId, bool Admin, bool Moderator, bool Subscribed, bool New)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO OnlineUsers VALUES(@playerId, @admin, @moderator, @subscribed, @new)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@admin", Admin ? "YES" : "NO");
                sqlCommand.Parameters.AddWithValue("@moderator", Moderator ? "YES" : "NO");
                sqlCommand.Parameters.AddWithValue("@subscribed", Subscribed ? "YES" : "NO");
                sqlCommand.Parameters.AddWithValue("@new", New ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void RemoveOnlineUser(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM OnlineUsers WHERE (playerId=@playerId)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static List<ItemInstance> GetShopInventory(int shopId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT ItemId,RandomId FROM ShopInventory WHERE ShopID=@shopId";
                sqlCommand.Parameters.AddWithValue("@shopId", shopId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                List<ItemInstance> instances = new List<ItemInstance>();

                while (reader.Read())
                {
                    instances.Add(new ItemInstance(reader.GetInt32(0), reader.GetInt32(1)));
                }
                sqlCommand.Dispose();
                return instances;
            }
        }

        public static void AddItemToShopInventory(int shopId, ItemInstance instance)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO ShopInventory VALUES(@shopId,@randomId,@itemId)";
                sqlCommand.Parameters.AddWithValue("@shopId", shopId);
                sqlCommand.Parameters.AddWithValue("@randomId", instance.RandomId);
                sqlCommand.Parameters.AddWithValue("@itemId", instance.ItemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void RemoveItemFromShopInventory(int shopId, ItemInstance instance)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM ShopInventory WHERE (ShopID=@shopId AND RandomId=@randomId)";
                sqlCommand.Parameters.AddWithValue("@shopId", shopId);
                sqlCommand.Parameters.AddWithValue("@randomId", instance.RandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static List<ItemInstance> GetPlayerInventory(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT ItemId,RandomId,Data FROM Inventory WHERE PlayerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                List<ItemInstance> instances = new List<ItemInstance>();

                while (reader.Read())
                {
                    instances.Add(new ItemInstance(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
                }
                sqlCommand.Dispose();
                return instances;
            }
        }
        public static int[] GetModsAndAdmins()
        {
            List<int> userList = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT id FROM Users WHERE Moderator=\"YES\" AND Admin=\"YES\"";
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userList.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
            }
            return userList.ToArray();
        }

        public static int[] GetUsers()
        {
            List<int> userList = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT id FROM Users";
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    userList.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
            }
            return userList.ToArray();
        }
        public static void AddItemToInventory(int playerId, ItemInstance instance)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                
                sqlCommand.CommandText = "INSERT INTO Inventory VALUES(@playerId,@randomId,@itemId, @data)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@randomId", instance.RandomId);
                sqlCommand.Parameters.AddWithValue("@itemId", instance.ItemId);
                sqlCommand.Parameters.AddWithValue("@data", instance.Data);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void RemoveAllItemTypesFromPlayerInventory(int playerId, int itemId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM Inventory WHERE (PlayerId=@playerId AND ItemID=@itemId)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@itemId", itemId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void RemoveItemFromInventory(int playerId, ItemInstance instance)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM Inventory WHERE (PlayerId=@playerId AND RandomId=@randomId)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@randomId", instance.RandomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static bool HasNpcStartpointSet(int playerId, int npcId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM NpcStartPoint WHERE playerId=@playerId AND npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                int total = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return total >= 1;
            }
        }
        public static bool HasNpcPos(int npcId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM NpcPos WHERE npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                int total = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return total >= 1;
            }
        }
        public static void SetNpcY(int npcId, int x)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcPos SET Y=@yPos WHERE npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@yPos", x);
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetNpcX(int npcId, int x)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcPos SET X=@xPos WHERE npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@xPos", x);
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetNpcUdlrPointer(int npcId, int udlr)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcPos SET UdlrPointer=@udlr WHERE npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@udlr", udlr);
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static int GetNpcUdlrPointer(int npcId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT UdlrPointer FROM NpcPos WHERE npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                int udlrPointer = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return udlrPointer;
            }
        }
        public static int GetNpcPosY(int npcId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT y FROM NpcPos WHERE npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                int y = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return y;
            }
        }
        public static int GetNpcPosX(int npcId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT X FROM NpcPos WHERE npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                int x = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return x;
            }
        }
        public static void AddNpcPos(int npcId, int X, int Y, int udlrPointer)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO NpcPos VALUES(@npcId, @xPos, @yPos, @udlr)";
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Parameters.AddWithValue("@xPos", X);
                sqlCommand.Parameters.AddWithValue("@yPos", Y);
                sqlCommand.Parameters.AddWithValue("@udlr", udlrPointer);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void AddNpcStartPoint(int playerId, int npcId, int startChatpoint)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "INSERT INTO NpcStartPoint VALUES(@playerId, @npcId, @chatpointId)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Parameters.AddWithValue("@chatpointId", startChatpoint);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void SetNpcStartPoint(int playerId, int npcId, int startChatpoint)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "UPDATE NpcStartPoint SET chatpointId=@chatpointId WHERE playerId=@playerId AND npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Parameters.AddWithValue("@chatpointId", startChatpoint);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static int GetDroppedItemsCount()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT COUNT(1) FROM DroppedItems";
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return count;
            }
        }
        public static int GetNpcStartPoint(int playerId, int npcId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "SELECT chatpointId FROM NpcStartPoint WHERE playerId=@playerId AND npcId=@npcId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@npcId", npcId);
                sqlCommand.Prepare();
                int startPoint = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();
                return startPoint;
            }
        }

        public static void RemoveDespawningItems()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM DroppedItems WHERE despawnTimer <=0";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }


        public static void RemoveDroppedItem(int randomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();

                sqlCommand.CommandText = "DELETE FROM DroppedItems WHERE (RandomId=@randomId)";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static DroppedItems.DroppedItem[] GetDroppedItems()
        {
            List<DroppedItems.DroppedItem> itemList = new List<DroppedItems.DroppedItem>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM DroppedItems";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
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
                sqlCommand.Dispose();

            }
            return itemList.ToArray();
        }
        public static void DecrementDroppedItemDespawnTimer()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();


                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE DroppedItems SET DespawnTimer=DespawnTimer-1";
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();

            }
        }

        public static void AddDroppedItem(DroppedItems.DroppedItem item)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();


                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO DroppedItems VALUES(@x, @y, @randomId, @itemId, @despawnTimer, @data)";
                sqlCommand.Parameters.AddWithValue("@x", item.X);
                sqlCommand.Parameters.AddWithValue("@y", item.Y);
                sqlCommand.Parameters.AddWithValue("@randomId", item.Instance.RandomId);
                sqlCommand.Parameters.AddWithValue("@itemId", item.Instance.ItemId);
                sqlCommand.Parameters.AddWithValue("@despawnTimer", item.DespawnTimer);
                sqlCommand.Parameters.AddWithValue("@data", item.Data);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();

            }
        }


        public static void AddReport(string reportCreator, string reporting, string reportReason)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                sqlCommand.CommandText = "INSERT INTO AbuseReports VALUES(@reportCreator,@reporting,@reportReason)";
                sqlCommand.Parameters.AddWithValue("@reportCreator", reportCreator);
                sqlCommand.Parameters.AddWithValue("@reporting", reporting);
                sqlCommand.Parameters.AddWithValue("@reportReason", reportReason);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }

        }
        public static Mailbox.Mail[] LoadMailbox(int toId)
        {
            List<Mailbox.Mail> mailList = new List<Mailbox.Mail>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "SELECT * FROM Mailbox WHERE IdTo=@toId";
                sqlCommand.Parameters.AddWithValue("@toId", toId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
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
                sqlCommand.Dispose();
            }
            return mailList.ToArray();
        }
        public static void ReadAllMail(int toId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "UPDATE Mailbox SET BeenRead='YES' WHERE IdTo=@toId";
                sqlCommand.Parameters.AddWithValue("@toId", toId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void DeleteMail(int randomId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "DELETE FROM Mailbox WHERE randomId=@randomId";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }
        public static void AddMail(int randomId, int toId, int fromId, string subject, string message, int timestamp, bool read)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();


                sqlCommand.CommandText = "INSERT INTO Mailbox VALUES(@randomId, @toId,@from,@subject,@message,@time,@read)";
                sqlCommand.Parameters.AddWithValue("@randomId", randomId);
                sqlCommand.Parameters.AddWithValue("@toId", toId);
                sqlCommand.Parameters.AddWithValue("@from", fromId);
                sqlCommand.Parameters.AddWithValue("@subject", subject);
                sqlCommand.Parameters.AddWithValue("@message", message);
                sqlCommand.Parameters.AddWithValue("@time", timestamp);
                sqlCommand.Parameters.AddWithValue("@read", read ? "YES" : "NO");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }

        }

        public static bool CheckUserExist(int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Id=@id";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count >= 1;
            }
        }
        public static bool CheckUserExist(string username)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Username=@name";
                sqlCommand.Parameters.AddWithValue("@name", username);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count >= 1;
            }
        }
        public static bool CheckUserExtExists(int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM UserExt WHERE Id=@id";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count >= 1;
            }
        }


        public static bool CheckUserIsModerator(string username)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Moderator FROM Users WHERE Username=@name";
                    sqlCommand.Parameters.AddWithValue("@name", username);
                    sqlCommand.Prepare();
                    string modStr = sqlCommand.ExecuteScalar().ToString();

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Admin FROM Users WHERE Username=@name";
                    sqlCommand.Parameters.AddWithValue("@name", username);
                    sqlCommand.Prepare();
                    string adminStr = sqlCommand.ExecuteScalar().ToString();

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT COUNT(1) FROM BuddyList WHERE Id=@id OR IdFriend=@id";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Prepare();

                Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Dispose();

                return count;
            }
        }

        public static int[] GetBuddyList(int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (GetBuddyCount(id) <= 0)
                    return new int[0];      // user is forever alone.

                List<int> BuddyList = new List<int>();

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT Id,IdFriend FROM BuddyList WHERE Id=@id OR IdFriend=@id";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Prepare();
                MySqlDataReader dataReader = sqlCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    int adder = dataReader.GetInt32(0);
                    int friend = dataReader.GetInt32(1);
                    if (adder != id)
                        BuddyList.Add(adder);
                    else if (friend != id)
                        BuddyList.Add(friend);
                }

                sqlCommand.Dispose();
                return BuddyList.ToArray();
            }
        }

        public static void RemoveBuddy(int id, int friendId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM BuddyList WHERE (Id=@id AND IdFriend=@friendId) OR (Id=@friendid AND IdFriend=@Id)";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@friendId", friendId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
        }

        public static void AddBuddy(int id, int friendId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO BuddyList VALUES(@id,@friendId)";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@friendId", friendId);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }
        public static string GetIpAddress(int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (!CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Does not exist in UserExt.");

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT IpAddress FROM UserExt WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", id);
                sqlCommand.Prepare();
                string IpAddress = sqlCommand.ExecuteScalar().ToString();
                sqlCommand.Dispose();
                return IpAddress;
            }
        }
        public static void SetIpAddress(int id, string ipAddress)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (!CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Does not exist in UserExt.");

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE UserExt SET IpAddress=@ipAddr WHERE Id=@playerId";
                sqlCommand.Parameters.AddWithValue("@ipAddr", ipAddress);
                sqlCommand.Parameters.AddWithValue("@playerId", id);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void CreateUserExt(int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(id)) // user allready exists!
                    throw new Exception("Userid " + id + " Allready in UserExt.");

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO UserExt VALUES(@id,@x,@y,@timestamp,0,0,0,0,'','','',0,0,'NO',0,0,1000,1000,1000, 180,1)";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@timestamp", Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));
                sqlCommand.Parameters.AddWithValue("@x", Map.NewUserStartX);
                sqlCommand.Parameters.AddWithValue("@y", Map.NewUserStartY);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static int GetUserid(string username)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Id FROM Users WHERE Username=@name";
                    sqlCommand.Parameters.AddWithValue("@name", username);
                    sqlCommand.Prepare();
                    int userId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT PrivateNotes FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    string privateNotes = sqlCommand.ExecuteScalar().ToString();

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET PrivateNotes=@notes WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@notes", notes);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }


        public static int GetPlayerCharId(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT CharId FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int CharId = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET CharId=@charId WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@charId", charid);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerX(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT X FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int X = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET X=@x WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@x", x);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerY(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Y FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int Y = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT ChatViolations FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int violations = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET ChatViolations=@violations WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@violations", violations);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }
        public static void SetPlayerY(int y, int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Y=@y WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@y", y);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerQuestPoints(int qp, int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET QuestPoints=@questPoints WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@questPoints", qp);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }
        public static int GetPlayerQuestPoints(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT QuestPoints FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int QuestPoints = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Money=@money WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@money", money);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses ORDER BY experience DESC LIMIT 25";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                sqlCommand.Dispose();
            }
            return inst.ToArray();
        }
        public static int[] GetMinigamePlayers()
        {
            List<int> userIds = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT DISTINCT playerId, SUM(timesplayed) OVER (PARTITION BY playerId) AS totalPlays FROM Leaderboards ORDER BY totalPlays DESC LIMIT 25";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
            }
            return userIds.ToArray();
        }
        
        
        public static int[] GetExperiencedPlayers()
        {
            List<int> userIds = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT id FROM UserExt ORDER BY Experience DESC LIMIT 25";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
            }
            return userIds.ToArray();

        }
        public static int[] GetAdventurousPlayers()
        {
            List<int> userIds = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT id FROM UserExt ORDER BY QuestPoints DESC LIMIT 25";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
            }
            return userIds.ToArray();

        }
        public static int[] GetRichestPlayers()
        {
            List<int> userIds = new List<int>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT id FROM UserExt ORDER BY Money+BankBalance DESC LIMIT 25";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    userIds.Add(reader.GetInt32(0));
                }
                sqlCommand.Dispose();
            }
            return userIds.ToArray();
            
        }
        public static HorseInstance[] GetMostSpoiledHorses()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses ORDER BY spoiled DESC LIMIT 100";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                sqlCommand.Dispose();
            }
            return inst.ToArray();
        }
        public static HorseInstance[] GetBiggestExpAutoSell()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses ORDER BY experience DESC LIMIT 50";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                sqlCommand.Dispose();
            }
            return inst.ToArray();
        }
        public static HorseInstance[] GetCheapestHorseAutoSell()
        {
            List<HorseInstance> inst = new List<HorseInstance>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Horses ORDER BY autoSell LIMIT 100";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                while(reader.Read())
                {
                    inst.Add(ReadHorseInstance(reader));
                }
                sqlCommand.Dispose();
            }
            return inst.ToArray();
        }
        public static int GetPlayerTotalMinigamesPlayed(int playerId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT SUM(timesplayed) FROM Leaderboards WHERE playerId=@playerId";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlCommand.Dispose();
                return count;
            }
        }

        public static void AddNewWinner(int playerId, string gameTitle, int wins, int looses)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Leaderboards VALUES(@playerId,@gameTitle,@wins,@loose,1,0,@type)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Parameters.AddWithValue("@wins", wins);
                sqlCommand.Parameters.AddWithValue("@loose", looses);
                sqlCommand.Parameters.AddWithValue("@type", "WINLOSS");
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
        }
        public static void AddNewHighscore(int playerId, string gameTitle, int score, string type)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "INSERT INTO Leaderboards VALUES(@playerId,@gameTitle,0,0,1,@score,@type)";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Parameters.AddWithValue("@score", score);
                sqlCommand.Parameters.AddWithValue("@type", type);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
        }

        public static Highscore.HighscoreTableEntry[] GetPlayerHighScores(int playerId)
        {
            List<Highscore.HighscoreTableEntry> entires = new List<Highscore.HighscoreTableEntry>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Leaderboards WHERE playerId=@playerId ORDER BY score DESC";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();

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


                sqlCommand.Dispose();
                return entires.ToArray();
            }
        }

        public static Highscore.HighscoreTableEntry[] GetTopWinners(string gameTitle, int limit)
        {
            List<Highscore.HighscoreTableEntry> entires = new List<Highscore.HighscoreTableEntry>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY wins DESC LIMIT @limit";
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Parameters.AddWithValue("@limit", limit);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();

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


                sqlCommand.Dispose();
                return entires.ToArray();
            }
        }

        public static Highscore.HighscoreTableEntry[] GetTopScores(string gameTitle, int limit)
        {
            List<Highscore.HighscoreTableEntry> entires = new List<Highscore.HighscoreTableEntry>();
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score DESC LIMIT @limit";
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Parameters.AddWithValue("@limit", limit);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                
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


                sqlCommand.Dispose();
                return entires.ToArray();
            }
        }

        public static int GetRanking(int score, string gameTitle)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT DISTINCT score FROM Leaderboards WHERE minigame=@gameTitle ORDER BY score DESC";
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();
                int i = 1;
                while(reader.Read())
                {
                    if (reader.GetInt32(0) == score)
                        break;
                    i++;
                }

                sqlCommand.Dispose();
                return i;
            }
        }
        public static void UpdateHighscoreWinGame(int playerId, string gameTitle)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Leaderboards SET wins=wins+1, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
        }
        public static void UpdateHighscoreLooseGame(int playerId, string gameTitle)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Leaderboards SET looses=looses+1, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
        }
        public static void UpdateHighscore(int playerId, string gameTitle, int score)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {

                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Leaderboards SET score=@score, timesplayed=timesplayed+1 WHERE playerId=@playerId AND minigame=@gameTitle";
                sqlCommand.Parameters.AddWithValue("@playerId", playerId);
                sqlCommand.Parameters.AddWithValue("@gameTitle", gameTitle);
                sqlCommand.Parameters.AddWithValue("@score", score);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
        }

        public static void DeleteExpiredLeasedHorsesForOfflinePlayers()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "DELETE FROM Horses WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime <= 0 AND leaser > 0";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
        }
        public static void TpOfflinePlayersBackToUniterForOfflinePlayers()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "SELECT ownerId, breed, leaser FROM Horses WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime <= 0 AND leaser > 0";
                sqlCommand.Prepare();
                MySqlDataReader reader = sqlCommand.ExecuteReader();

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

                sqlCommand.Dispose();
                return;
            }
        }

        public static void DecrementHorseLeaseTimeForOfflinePlayers()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE Horses SET leaseTime = leaseTime - 1 WHERE ownerId NOT IN (SELECT playerId FROM OnlineUsers) AND leaseTime > 0 AND leaser > 0";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
            
        }
        public static void IncPlayerTirednessForOfflineUsers()
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE UserExt SET tiredness = tiredness + 1 WHERE id NOT IN (SELECT playerId FROM OnlineUsers) AND NOT tiredness +1 > 1000";
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
                return;
            }
        }

        public static int GetPlayerTiredness(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Tiredness FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int tiredness = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Tiredness=@tiredness WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@tiredness", tiredness);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerHunger(int id, int hunger)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Hunger=@hunger WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@Hunger", hunger);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }



        public static int GetPlayerHunger(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Hunger FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int hunger = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET Thirst=@thirst WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@thirst", thirst);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerThirst(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Thirst FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int tiredness = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT LastLogin FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int lastLogin = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET LastLogin=@lastlogin WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@lastlogin", lastlogin);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static int GetPlayerMoney(int userId)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Money FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    int Money = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT BankBalance FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    double BankMoney = Convert.ToDouble(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExtExists(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT BankInterest FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    double BankInterest = Convert.ToDouble(sqlCommand.ExecuteScalar());

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = "UPDATE UserExt SET BankInterest = BankInterest + (BankInterest * (1/@interestRate)) WHERE NOT BankInterest + (BankInterest * (1/@interestRate)) > 9999999999.9999";
                sqlCommand.Parameters.AddWithValue("@interestRate", intrestRate);
                sqlCommand.Prepare();
                sqlCommand.ExecuteNonQuery();

                sqlCommand.Dispose();
            }
        }

        public static void SetPlayerBankInterest(double interest, int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET BankInterest=@interest WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@interest", interest);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }
        public static void SetPlayerBankMoney(double bankMoney, int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET BankBalance=@bankMoney WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@bankMoney", bankMoney);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static void SetPlayerProfile(string profilePage, int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "UPDATE UserExt SET ProfilePage=@profilePage WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@profilePage", profilePage);
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();
                }
                else
                {
                    throw new KeyNotFoundException("Id " + id + " not found in database.");
                }
            }
        }

        public static string GetPlayerProfile(int id)
        {
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(id))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT ProfilePage FROM UserExt WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Prepare();
                    string profilePage = sqlCommand.ExecuteScalar().ToString();

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(userId))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT Username FROM Users WHERE Id=@id";
                    sqlCommand.Parameters.AddWithValue("@id", userId);
                    sqlCommand.Prepare();
                    string username = sqlCommand.ExecuteScalar().ToString();

                    sqlCommand.Dispose();
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
            using (MySqlConnection db = new MySqlConnection(ConnectionString))
            {
                db.Open();
                if (CheckUserExist(username))
                {
                    MySqlCommand sqlCommand = db.CreateCommand();
                    sqlCommand.CommandText = "SELECT PassHash FROM Users WHERE Username=@name";
                    sqlCommand.Parameters.AddWithValue("@name", username);
                    sqlCommand.Prepare();
                    string expectedHash = sqlCommand.ExecuteScalar().ToString();

                    sqlCommand.Dispose();
                    return Converters.StringToByteArray(expectedHash);
                }
                else
                {
                    throw new KeyNotFoundException("Username " + username + " not found in database.");
                }
            }
        }
    }

}