using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DiscordBots.SQL
{
    public class Connect
    {
        public static MySqlConnection ConnectDB(ConfigJson cfgjson)
        {
            string connString;

            MySqlConnection connection;

            connString = @$"Data Source={cfgjson.Host}; Database={cfgjson.Database}; UID={cfgjson.Username}; password={cfgjson.Password}";

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
