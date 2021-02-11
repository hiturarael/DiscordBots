using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Nine.Commands
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

        public static readonly bool testing = true;

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

            string resultAlias = "", resultURL = "", resultThread = "";

            int errors = 0;

            try
            {
                DataTable dt = SqlCommand.ExecuteQuery(aliasQuery, testing);

                if (dt.Rows.Count < 1)
                {
                    DataRow row = dt.Rows[0];
                    resultAlias = row["Alias"].ToString();
                    errors += 1;
                }

                dt = SqlCommand.ExecuteQuery(threadQuery, testing);

                if (dt.Rows.Count < 1)
                {
                    DataRow row = dt.Rows[0];
                    resultThread = row["Title"].ToString();
                    errors += 2;
                }

                dt = SqlCommand.ExecuteQuery(urlQuery, testing);

                if (dt.Rows.Count < 1)
                {
                    DataRow row = dt.Rows[0];
                    resultURL = row["URL"].ToString();
                    errors += 4;
                }

                if (errors == 0)
                {
                    SqlCommand.ExecuteQuery_Params(query, parameters, values);

                    dt = SqlCommand.ExecuteQuery(aliasQuery, testing);

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
                else
                {
                    result = errors switch
                    {
                        1 => $"The alias you are trying to use is already taken for the thread {resultThread} with the url {resultURL}.",
                        2 => $"The title of the thread you are trying to add is already in the database under the alias {resultAlias} with the url {resultURL}",
                        3 => $"The alias and title are already in use for url {resultURL}",
                        4 => $"The url you are trying to use is already taken for the thread {resultThread} with the alias {resultAlias}.",
                        5 => $"The alias and url are already in use for the thread {resultThread}",
                        6 => $"The title and url are already in use for the alias {resultAlias}",
                        7 => $"The title, url, and alias are already in use.",
                        _ => "I do not know how this happened but something went terribly wrong.",
                    };
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public static string UpdateThread(string threadId, string status)
        {
            string table = "threads";
            string aliasQuery = $"SELECT * from {table} WHERE Alias LIKE %{threadId}%";
            string threadQuery = $"SELECT * from {table} WHERE Title LIKE %{threadId}%";
            string urlQuery = $"SELECT * from {table} WHERE URL LIKE %{threadId}%";
            string result = "";
            string column;
            string updateQuery;

            string[] parameters = { "@Status" };
            string[] values = { status };

            try
            {
                DataTable dt = null;

                if (!Enum.IsDefined(typeof(ThreadStatus), status))
                {
                    string vals = "";

                    for (int x = 0; x < Enum.GetValues(typeof(ThreadStatus)).Length; x++)
                    {
                        vals += $"{Enum.GetValues(typeof(ThreadStatus)).GetValue(x)} ";
                    }

                    result = $"The status you are trying to update with is not valid. Use one of the following: {vals.Trim()}.";
                }
                else
                {

                    if (threadId.Contains("https://") || threadId.Contains("srwignition.com"))
                    {
                        dt = SqlCommand.ExecuteQuery(urlQuery, testing);
                        column = "URL";

                        result = "... Why would you try to update with the url- You have a perfectly good title and alias! *Sigh* Whatever, meatbag...";
                    }
                    else
                    {
                        dt = SqlCommand.ExecuteQuery(aliasQuery, testing);
                        column = "Alias";
                        if (dt.Rows.Count < 1)
                        {
                            dt = SqlCommand.ExecuteQuery(threadQuery, testing);
                            column = "Title";
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        //update the record
                        //updateQuery = $"UPDATE `{table}` SET `Status` = '{status}' WHERE `threads`.`{column}` = {threadId};";
                        updateQuery = $"UPDATE {table} (Status) VALUES(@status) WHERE threads.{column} = {threadId};";
                        //string query = $"INSERT INTO {table}(Title, Alias, URL, Status) VALUES(@title, @alias, @url, @status)";

                        SqlCommand.ExecuteQuery_Params(updateQuery, parameters, values);

                        result = "I have updated the status of the thread.";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

    }
}
