using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using MySql.Data.MySqlClient;

namespace DiscordBots.SQL
{
    public class Connect
    {
        public static MySqlConnection ConnectDB(bool testing = false)
        {
            string connString;
            string host;
            string catalog;
            string user;
            string password;

            if(testing)
            {
                host = Environment.GetEnvironmentVariable("test_host");
                catalog = Environment.GetEnvironmentVariable("test_database");
                user = Environment.GetEnvironmentVariable("test_username");
                password = Environment.GetEnvironmentVariable("test_password");
            } else
            {
                host = Environment.GetEnvironmentVariable("host");
                catalog = Environment.GetEnvironmentVariable("Database");
                user = Environment.GetEnvironmentVariable("UserName");
                password =  Environment.GetEnvironmentVariable("Password");
            }

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
