using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

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

        public static readonly bool testing = false;

        public static string AddThread(string threadName, string url, string alias)
        {
            string result;
            string table = "threads";
            string query = $"INSERT INTO {table}(Title, Alias, URL, Status) VALUES(@title, @alias, @url, @status)";
            string aliasQuery = $"SELECT * from {table} WHERE Alias LIKE '%{alias}%'";
            string threadQuery = $"SELECT * from {table} WHERE Title LIKE '%{threadName}%'";

            string urlQuery = $"SELECT * from {table} WHERE URL LIKE '%{url}%'";

            string[] parameters = { "@title", "@alias", "@url", "@status" };
            string[] values = { threadName, alias, url, ThreadStatus.Open.ToString() };

            string resultAlias = "", resultURL = "", resultThread = "";

            int errors = 0;

            try
            {
                DataTable dt = SqlCommand.ExecuteQuery(aliasQuery, testing);
                
                if (dt.Rows.Count >= 1)
                {
                    DataRow row = dt.Rows[0];
                    object urlBlob = row["URL"];

                    resultAlias = row["Alias"].ToString();
                    resultURL = Functions.GetUrl((byte[])urlBlob);
                    resultThread = row["Title"].ToString();
                    errors += 1;
                }
                 
                dt = SqlCommand.ExecuteQuery(threadQuery, testing);

                if (dt.Rows.Count >= 1)
                {
                    DataRow row = dt.Rows[0];
                    object urlBlob = row["URL"];

                    resultAlias = row["Alias"].ToString();
                    resultURL = Functions.GetUrl((byte[])urlBlob);
                    resultThread = row["Title"].ToString();
                    errors += 2;
                }

                dt = SqlCommand.ExecuteQuery(urlQuery, testing);

                if (dt.Rows.Count >= 1)
                {
                    DataRow row = dt.Rows[0];
                    object urlBlob = row["URL"];

                    resultAlias = row["Alias"].ToString();
                    resultURL = Functions.GetUrl((byte[])urlBlob);
                    resultThread = row["Title"].ToString();
                    errors += 4;
                }

                if (errors == 0)
                {
                    SqlCommand.ExecuteQuery_Params(query, parameters, values);

                    dt = SqlCommand.ExecuteQuery(aliasQuery, testing);

                    if (dt.Rows.Count == 1)
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
                        2 => $"The title of the thread you are trying to add is already in the database under the alias {resultAlias} with the url {resultURL}.",
                        3 => $"The alias and title are already in use for url {resultURL}.",
                        4 => $"The url you are trying to use is already taken for the thread {resultThread} with the alias {resultAlias}.",
                        5 => $"The alias and url are already in use for the thread {resultThread}.",
                        6 => $"The title and url are already in use for the alias {resultAlias}.",
                        7 => $"The title, url, and alias are already in my records.",
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
            string aliasQuery = $"SELECT * from {table} WHERE Alias LIKE '%{threadId}%'";
            string threadQuery = $"SELECT * from {table} WHERE Title LIKE '%{threadId}%'";
            string urlQuery = $"SELECT * from {table} WHERE URL LIKE '%{threadId}%'";
            string result = "";
            string column;
            string updateQuery;

            status = char.ToUpper(status[0]) + status.Substring(1);

            string[] parameters = { "@Status" };
            string[] values = { status };

            try
            {
                DataTable dt = null;

                status = char.ToUpper(status[0]) + status.Substring(1);

                if (!Enum.IsDefined(typeof(ThreadStatus), status))
                {
                    string vals = "";

                    for (int x = 0; x < Enum.GetValues(typeof(ThreadStatus)).Length; x++)
                    {
                        vals += $"/n{Enum.GetValues(typeof(ThreadStatus)).GetValue(x)}";
                    }

                    result = $"The status you are trying to update with is not valid. Use one of the following: {vals.Trim()}";
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
                        updateQuery = $"UPDATE {table} SET Status=@status where {column} = '{threadId}'";//(Status) VALUES(@status) WHERE {column} = {threadId}";
                        //string query = $"INSERT INTO {table}(Title, Alias, URL, Status) VALUES(@title, @alias, @url, @status)";

                        SqlCommand.ExecuteQuery_Params(updateQuery, parameters, values);

                        if (result == "")
                        {
                            result = "I have updated the status of the thread.";
                        } else
                        {
                            result += " I have updated the status of the thread.";
                        }
                    } else
                    {
                        result = "There is no thread with that identifier. Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public static string AddToPostOrder(string threadId, string player, string position)
        {
            string postOrderTable = "postorder";
            string threadTable = "threads";
            DataTable dt = QueryThread(threadTable, threadId);
            string result;
            //query thread exists in the thread table
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                //get idnum
                int threadNum = Convert.ToInt32(dr["ID"]);

                string addPlayerQuery = $"INSERT INTO {postOrderTable}(ThreadID, Player, PostPosition) VALUES(@threadId, @player, @position)";
                string[] parameters = { "@threadId", "@player", "@position" };
                string[] values = { threadNum.ToString(), player, position };

                //check player is not added to list already
                if (!PlayerAdded(postOrderTable, player))
                {
                    //check player post position is not already taken
                    if (!PositionAdded(postOrderTable, position))
                    {
                        //add to post order
                        try
                        {
                            SqlCommand.ExecuteQuery_Params(addPlayerQuery, parameters, values, testing);

                            if (!PlayerAdded(postOrderTable, player))
                            {
                                result = "An unexpected error occured, please try again later.";
                            }
                            else
                            {
                                result = $"{player} has been added to the posting order";
                            }
                        }
                        catch (Exception ex)
                        {
                            result = ex.Message;
                        }
                    }
                    else
                    {
                        result = $"{position} has already been added to the post order.";
                    }
                }
                else
                {
                    result = $"{player} has already been added to the post order.";
                }

            }
            else
            {
                result = $"There is no thread in the database with the Title or Alias '{threadId}'.";
            }

            return result;
        }

        static DataTable QueryThread(string table, string threadId)
        {
            string threadTitleQuery = $"SELECT * from {table} where Title = '{threadId}'";
            string threadAliasQuery = $"SELECT * from {table} where Alias = '{threadId}'";
            DataTable dt = SqlCommand.ExecuteQuery(threadTitleQuery, testing);
            if (dt.Rows.Count == 0)
            {
                dt = SqlCommand.ExecuteQuery(threadAliasQuery, testing);
            }

            return dt;
        }

        static bool PlayerAdded(string table, string PlayerID)
        {
            string playerQuery = $"SELECT Player from {table} where Player = '{PlayerID}'";

            DataTable dt = SqlCommand.ExecuteQuery(playerQuery, testing);

            if(dt.Rows.Count > 0)
            {
                return true; 
            } else
            {
                return false;
            }
        }

        static bool PositionAdded(string table, string positionNum)
        {
            string playerQuery = $"SELECT PostPosition from {table} where PostPosition = '{positionNum}'";

            DataTable dt = SqlCommand.ExecuteQuery(playerQuery, testing);

            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
