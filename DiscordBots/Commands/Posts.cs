using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DSharpPlus;
using DiscordBots.SQL;

namespace DiscordBots.Commands
{
    public class Posts
    {
        public enum ThreadStatus
        {
            Open,
            Complete,
            Hiatus,
            Abandoned
        }

        public static string AddThread(string threadName, string url, string alias)
        {
            string result;
            string table = "threads";
            string query = $"INSERT INTO {table}(Title, Alias, URL, Status) VALUES(@title, @alias, @url, @status)";
            string aliasQuery = $"SELECT * from {table} WHERE Alias LIKE %{alias}%";
            string threadQuery = $"SELECT * from {table} WHERE Title LIKE %{threadName}%";

            string urlQuery = $"SELECT * from {table} WHERE URL LIKE %{url}%";

            string[] parameters = { "@title", "@alias", "@url", "@status" };
            string[] values = { threadName, alias, url, ThreadStatus.Open.ToString() };
            try
            {
                DataTable dt = SqlCommand.ExecuteQuery(aliasQuery);

                if(dt.Rows.Count < 1)
                {
                    result = "A thread with this alias already exists.";
                } else
                {
                    dt = SqlCommand.ExecuteQuery(threadQuery);

                    if(dt.Rows.Count < 1)
                    {
                        result = "A thread with this title already exists in the database.";
                    } else
                    {
                        dt = SqlCommand.ExecuteQuery(urlQuery);

                        if (dt.Rows.Count < 1)
                        {
                            result = "A thread with this url already exists in the database.";
                        } else
                        {
                            //open connection & insert row
                            SqlCommand.ExecuteQuery_Insert(query, parameters, values);

                            dt = SqlCommand.ExecuteQuery(aliasQuery);

                            if (dt.Rows.Count > 1)
                            {
                                //return confirmation of add
                                result = "The thread has been added to the database.";
                            }
                            else
                            {
                                result = "Something went wrong, unable to add thread.";
                            }
                        }
                    }
                }
                
            } catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}
