using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace DiscordBots.SQL
{
    public class SqlCommand
    {
        public static DataTable ExecuteQuery(string query)
        {
            MySqlCommand cmd;
            MySqlDataReader reader;
            DataTable dt = new DataTable();

            //create and open connection
            MySqlConnection conn = Connect.ConnectDB();

            try
            {

                //create the command
                cmd = conn.CreateCommand();
                cmd.CommandText = query;

                //query database
                reader = cmd.ExecuteReader();

                //load data from reader
                dt.Load(reader);
            }
            catch 
            {
                throw new Exception();
            }
            finally 
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return dt;
        }
    }
}
