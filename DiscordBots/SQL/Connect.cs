using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using MySql.Data.MySqlClient;

namespace DiscordBots.SQL
{
    public class Connect
    {
        public static MySqlConnection ConnectDB()
        {
            string connString;
            string host = Environment.GetEnvironmentVariable("host");
            string catalog = Environment.GetEnvironmentVariable("Database");
            string user = Environment.GetEnvironmentVariable("UserName");
            string password = Environment.GetEnvironmentVariable("Password");

            MySqlConnection connection;

            connString = @$"Data Source={host}; Database={catalog}; UID={user}; password={password}";

            try
            {
                connection = new MySqlConnection(connString);

                connection.Open();
            }
            catch (Exception ex)
            {
                connection = null;
            }

            return connection;
        }

        public static void CloseConnection(MySqlConnection connection)
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            } catch
            {

            }
        }
    }
}
