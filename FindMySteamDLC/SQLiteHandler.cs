using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Data;

namespace FindMySteamDLC
{
    static public class SQLiteHandler
    {
        static private SQLiteConnection cnx;
        
        static public void InitializeSQLite(string pathToDB)
        {
            List<Game> games = new List<Game>();
            if (!File.Exists(pathToDB))
            {
                SQLiteConnection.CreateFile(pathToDB);
                cnx = new SQLiteConnection(String.Format("Data Source={0};Version=3;", pathToDB));
                CreateTables();
            }
            else
            {
                cnx = new SQLiteConnection(String.Format("Data Source={0};Version=3;", pathToDB));
            }
        }

        static public void CreateTables()
        {
            cnx.Open();
            string queryCreateGameTable = "CREATE TABLE games (appid INT PRIMARY KEY, name VARCHAR(100))";
            string queryCreateDlcTable = "CREATE TABLE dlcs (appid INT PRIMARY KEY, name VARCHAR(100), gameappid INT, FOREIGN KEY (gameappid) REFERENCES games (appid))";
            SQLiteCommand createGameTable = new SQLiteCommand(queryCreateGameTable, cnx);
            SQLiteCommand createDlcTable = new SQLiteCommand(queryCreateDlcTable, cnx);
            createGameTable.ExecuteNonQuery();
            createDlcTable.ExecuteNonQuery();
            cnx.Close();
        }

        static public List<Game> FetchGames()
        {
            List<Game> games = new List<Game>();
            cnx.Open();
            string queryFetchGames = "SELECT appid, name FROM games";
            string queryFetchDlcs = "SELECT appid, name FROM dlcs WHERE gameappid = @gameappid";
            SQLiteCommand fetchGames = new SQLiteCommand(queryFetchGames, cnx);
            SQLiteCommand fetchDlcs = new SQLiteCommand(queryFetchDlcs, cnx);
            fetchDlcs.Parameters.Add("@gameappid", DbType.String);
            SQLiteDataReader reader = fetchGames.ExecuteReader();
            while (reader.Read())
            {
                Game game = new Game() { AppID = reader.GetInt32(0), Name = reader.GetString(1) };
                fetchDlcs.Parameters["@gameappid"].Value = game.AppID;
                SQLiteDataReader innerReader = fetchDlcs.ExecuteReader();
                while (innerReader.Read())
                {
                    game.Dlcs.Add(new Dlc(game) { AppID = innerReader.GetInt32(0), Name = innerReader.GetString(1) });
                }
                games.Add(game);
                innerReader.Close();
            }
            reader.Close();
            cnx.Close();
            return games;
        }

        static public void InsertGames(List<Game> games)
        {
            cnx.Open();
            string queryInsertGames =
                "INSERT INTO games(appid, name) " +
                "VALUES (@appid, @name)";
            string queryInsertDlcs =
                "INSERT INTO dlcs(appid, name, gameappid)" +
                "VALUES (@appid, @name, @gameappid)";
            SQLiteCommand insertGames = new SQLiteCommand(queryInsertGames, cnx);
            SQLiteCommand insertDlcs = new SQLiteCommand(queryInsertDlcs, cnx);
            insertGames.Parameters.Add("@appid", DbType.Int32);
            insertGames.Parameters.Add("@name", DbType.String);
            insertDlcs.Parameters.Add("@appid", DbType.Int32);
            insertDlcs.Parameters.Add("@name", DbType.String);
            insertDlcs.Parameters.Add("@gameappid", DbType.Int32);
            foreach (Game game in games)
            {
                insertGames.Parameters["@appid"].Value = game.AppID;
                insertGames.Parameters["@name"].Value = game.Name;
                if (!VerifyIfGameExists(game))
                    insertGames.ExecuteNonQuery();
                foreach (Dlc dlc in game.Dlcs)
                {
                    insertDlcs.Parameters["@appid"].Value = dlc.AppID;
                    insertDlcs.Parameters["@name"].Value = dlc.Name;
                    insertDlcs.Parameters["@gameappid"].Value = game.AppID;
                    if(!VerifyIfDlcExists(dlc))
                        insertDlcs.ExecuteNonQuery();
                }
            }
            cnx.Close();
        }

        static private bool VerifyIfGameExists(Game game)
        {
            SQLiteConnection cnxbis = new SQLiteConnection(cnx.ConnectionString);
            cnxbis.Open();
            string queryFetchGame = "SELECT appid FROM games WHERE appid = @appid";
            SQLiteCommand fetchGame = new SQLiteCommand(queryFetchGame, cnxbis);
            fetchGame.Parameters.AddWithValue("@appid", game.AppID);
            SQLiteDataReader reader = fetchGame.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                cnxbis.Close();
                return true;
            }
            else
            {
                reader.Close();
                cnxbis.Close();
                return false;
            }
        }

        static private bool VerifyIfGameExists(int appid)
        {
            SQLiteConnection cnxbis = new SQLiteConnection(cnx.ConnectionString);
            cnxbis.Open();
            string queryFetchGame = "SELECT appid FROM games WHERE appid = @appid";
            SQLiteCommand fetchGame = new SQLiteCommand(queryFetchGame, cnxbis);
            fetchGame.Parameters.AddWithValue("@appid", appid);
            SQLiteDataReader reader = fetchGame.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                cnxbis.Close();
                return true;
            }
            else
            {
                reader.Close();
                cnxbis.Close();
                return false;
            }
        }

        static public bool VerifyIfDlcExists(Dlc dlc)
        {
            SQLiteConnection cnxbis = new SQLiteConnection(cnx.ConnectionString);
            cnxbis.Open();
            string queryFetchDlc = "SELECT appid FROM dlcs WHERE appid = @appid";
            SQLiteCommand fetchDlc = new SQLiteCommand(queryFetchDlc, cnxbis);
            fetchDlc.Parameters.AddWithValue("@appid", dlc.AppID);
            SQLiteDataReader reader = fetchDlc.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                cnxbis.Close();
                return true;
            }
            else
            {
                reader.Close();
                cnxbis.Close();
                return false;
            }
        }

        static public bool VerifyIfDlcExists(int appid)
        {
            SQLiteConnection cnxbis = new SQLiteConnection(cnx.ConnectionString);
            cnxbis.Open();
            string queryFetchDlc = "SELECT appid FROM dlcs WHERE appid = @appid";
            SQLiteCommand fetchDlc = new SQLiteCommand(queryFetchDlc, cnxbis);
            fetchDlc.Parameters.AddWithValue("@appid", appid);
            SQLiteDataReader reader = fetchDlc.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                cnxbis.Close();
                return true;
            }
            else
            {
                reader.Close();
                cnxbis.Close();
                return false;
            }
        }
    }
}
