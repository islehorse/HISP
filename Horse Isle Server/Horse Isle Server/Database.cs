using System;
using System.Collections.Generic;
using MySqlConnector;

namespace Horse_Isle_Server
{
    class Database
    {

        public static MySqlConnection db;

        public static void OpenDatabase()
        {
            db = new MySqlConnection("server=" + ConfigReader.DatabaseIP + ";user=" + ConfigReader.DatabaseUsername + ";password=" + ConfigReader.DatabasePassword+";database="+ConfigReader.DatabaseName);
            db.Open();
            string UserTable = "CREATE TABLE Users(Id INT, Username TEXT(16),Email TEXT(128),Country TEXT(128),SecurityQuestion Text(128),SecurityAnswerHash TEXT(128),Age INT,PassHash TEXT(128), Salt TEXT(128),Gender TEXT(16), Admin TEXT(3), Moderator TEXT(3))";

            try
            {

                MySqlCommand sqlCommand = db.CreateCommand();
                sqlCommand.CommandText = UserTable;
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e) {
                Logger.ErrorPrint("Failed to setup database (perhaps its allready setup?) "+e.Message);
            };
        }

        public static byte[] GetPasswordSalt(string username)
        {
            MySqlCommand sqlCommand = db.CreateCommand();
            sqlCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Username=$name";
            sqlCommand.Parameters.AddWithValue("$name", username);
            Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());
            if (count >= 1)
            {
                sqlCommand.CommandText = "SELECT Salt FROM Users WHERE Username=$name";
                string expectedHash = sqlCommand.ExecuteScalar().ToString();
                return Converters.StringToByteArray(expectedHash);
            }
            else
            {
                throw new KeyNotFoundException("Username " + username + " not found in database.");
            }
        }
        public static byte[] GetPasswordHash(string username)
        {
            MySqlCommand sqlCommand = db.CreateCommand();
            sqlCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Username=$name";
            sqlCommand.Parameters.AddWithValue("$name", username);
            Int32 count = Convert.ToInt32(sqlCommand.ExecuteScalar());
            if(count >= 1)
            {
                sqlCommand.CommandText = "SELECT PassHash FROM Users WHERE Username=$name";
                string expectedHash = sqlCommand.ExecuteScalar().ToString();
                return Converters.StringToByteArray(expectedHash);
            }
            else
            {
                throw new KeyNotFoundException("Username " + username + " not found in database.");
            }
        }
    }

}