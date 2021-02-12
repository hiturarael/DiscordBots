using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace DiscordBots.SQL
{
    public class SqlCommand
    {
        public static DataTable ExecuteQuery(string query, bool testing = false)
        {
            MySqlCommand cmd;
            MySqlDataReader reader;
            DataTable dt = new DataTable();

            //create and open connection
            MySqlConnection conn = Connect.ConnectDB(testing);

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
            catch (Exception ex)
            {
                throw ex;
            }
            finally 
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return dt;
        }

        public static void ExecuteQuery_Params(string query, string[] parameters, string[] parameterValues, bool testing = false)
        {
            MySqlCommand cmd;

            //create and open connection
            MySqlConnection conn = Connect.ConnectDB(testing);

            try
            {

                //create the command
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                for (int x = 0; x < parameters.Length; x++)
                {
                    cmd.Parameters.AddWithValue(parameters[x], parameterValues[x]);
                }

                //query database
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
    }
}
