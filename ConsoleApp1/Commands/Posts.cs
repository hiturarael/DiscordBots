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
        private static readonly string threadTable = "threads";
        private static readonly string postOrderTable = "postorder";
        private static readonly string postTable = "post";
        private static readonly string pingTable = "ping_cooldown";

        public static string AddThread(string threadName, string url, string alias)
        {
            string result;
            string query = $"INSERT INTO {threadTable}(Title, Alias, URL, Status) VALUES(@title, @alias, @url, @status)";
            string aliasQuery = $"SELECT * from {threadTable} WHERE Alias = '{alias}'";
            string threadQuery = $"SELECT * from {threadTable} WHERE Title = '{threadName}'";

            string urlQuery = $"SELECT * from {threadTable} WHERE URL = '{url}'";

            string[] parameters = { "@title", "@alias", "@url", "@status" };
            string[] values = { threadName, alias, url, ThreadStatus.Open.ToString() };

            string resultAlias = "", resultURL = "", resultThread = "";

            int errors = 0;

            try
            {
                DataTable dt;

                for (int x = 0; x < 2; x++)
                { 
                    dt = SqlCommand.ExecuteQuery(aliasQuery, testing);
             
                    if (dt.Rows.Count >= 1)
                    {
                        DataRow row = dt.Rows[0];

                        resultAlias = row["Alias"].ToString();
                        resultURL = row["URL"].ToString();
                        resultThread = row["Title"].ToString();

                        if (x == 0)
                        {
                            errors += 1;
                        } else
                        {
                            errors += 2;
                        }
                        break;
                    }
                        aliasQuery = $"SELECT * from {threadTable} WHERE Alias = '{threadName}'";
                }

                aliasQuery = $"SELECT * from {threadTable} WHERE Alias = '{alias}'";

                dt = SqlCommand.ExecuteQuery(threadQuery, testing);

                for (int x = 0; x < 2; x++)
                {
                    if (dt.Rows.Count >= 1)
                    {
                        DataRow row = dt.Rows[0];

                        resultAlias = row["Alias"].ToString();
                        resultURL = row["URL"].ToString();
                        resultThread = row["Title"].ToString();
                        if (x == 0)
                        {
                            errors += 4;
                        }
                        else
                        {
                            errors += 8;
                        }
                        break;
                    }

                    threadQuery = $"SELECT * from {threadTable} WHERE Title LIKE '%{alias}%'"; 
                }

                
                dt = SqlCommand.ExecuteQuery(urlQuery, testing);

                if (dt.Rows.Count >= 1)
                {
                    DataRow row = dt.Rows[0];

                    resultAlias = row["Alias"].ToString();
                    resultURL = row["URL"].ToString();
                    resultThread = row["Title"].ToString();
                    errors += 16;
                        
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
                    result = $"A match has been found. Please remember that title and alias are both searched for both values since meatbags are flawed creatures that mix the two up sometimes.\nTitle: {resultThread}\nAlias: {resultAlias}\nURL: {resultURL}";
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
            string aliasQuery = $"SELECT * from {threadTable} WHERE Alias LIKE '%{threadId}%'";
            string threadQuery = $"SELECT * from {threadTable} WHERE Title LIKE '%{threadId}%'";
            string urlQuery = $"SELECT * from {threadTable} WHERE URL LIKE '%{threadId}%'";
            string result = "";
            string column;
            string updateQuery;

            status = char.ToUpper(status[0]) + status[1..];

            string[] parameters = { "@Status" };
            string[] values = { status };

            try
            {
                DataTable dt = null;

                status = char.ToUpper(status[0]) + status[1..];

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
                        updateQuery = $"UPDATE {threadTable} SET Status=@status where {column} = '{threadId}'";//(Status) VALUES(@status) WHERE {column} = {threadId}";
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

        public static string AddToPostOrder(string threadId, string player, string position, string maskPlayer)
        {
            DataTable dt = QueryThread(threadTable, threadId);
            string result;
            //query thread exists in the thread table
            if (dt.Rows.Count > 0)
            {
                //get idnum
                int threadNum = GetThreadID(dt, 0);

                string addPlayerQuery = $"INSERT INTO {postOrderTable}(ThreadID, Player, PostPosition) VALUES(@threadId, @player, @position)";
                string[] parameters = { "@threadId", "@player", "@position" };
                string[] values = { threadNum.ToString(), player, position };

                //check player is not added to list already
                if (!PlayerAdded(postOrderTable, player, threadNum) && NonMentionPlayerAdded("players", maskPlayer))
                {
                    //check player post position is not already taken
                    if (!PositionAdded(postOrderTable, position,threadNum))
                    {
                        //add to post order
                        try
                        {
                            SqlCommand.ExecuteQuery_Params(addPlayerQuery, parameters, values, testing);

                            if (!PlayerAdded(postOrderTable, player, threadNum))
                            {
                                result = "An unexpected error occured, please try again later.";
                            }
                            else
                            {
                                result = $"{maskPlayer} has been added to the posting order";
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
                    result = $"{maskPlayer} has already been added to the post order.";
                }

            }
            else
            {
                result = $"There is no thread in the database with the Title or Alias '{threadId}'.";
            }

            return result;
        }

        public static string PostOrder(string threadId)
        {
            DataTable dt = QueryThread(threadTable, threadId);
            string result;

            if(dt.Rows.Count > 0)
            {
                dt = QueryPostOrder(dt, postOrderTable);

                if (dt.Rows.Count >= 1)
                {
                    result = $"The order for {threadId} is as follows:\n";

                    for(int x = 0; x < dt.Rows.Count; x++)
                    {
                        DataRow row = dt.Rows[x];

                        result += $"{row["PostPosition"]}: {Player.GetPlayer(row["Player"].ToString(),Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker)}\n";
                    }

                    result = result.Trim();
                } else
                {
                    result = "The posting order for that thread has not yet been established.";
                }

            }
            else
            {
                result = $"There are no threads under title or alias with the text {threadId}";
            }

            return result;
        }

        public static string RemoveFromOrder(string threadId, string player)
        {
            DataTable dt = QueryThread(threadTable, threadId);
            string result;

            //confirm thread exists
            if (dt.Rows.Count > 0)
            {
                string unmasked = player;
                string masked = player;
                List<string> plr = new List<string>();
                List<string> pos = new List<string>();
                int newPosition = 1;
                int threadNum = 0;
                int added = 0;

                if(!player.Contains("@!"))
                {
                   unmasked = Player.GetPlayer(player, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
                }

                if(masked.Contains("@!"))
                {
                    masked = Player.GetPlayer(player, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);
                }

                //get existing post order
                dt = QueryPostOrder(dt, postOrderTable);
                
                //confirm player is in order
                foreach(DataRow r in dt.Rows)
                {
                    if(r["Player"].ToString() != unmasked && r["player"].ToString() != player)
                    {
                        plr.Add(r["Player"].ToString());

                        if (r["PostPosition"].ToString() != newPosition.ToString())
                        {
                            pos.Add(newPosition.ToString());
                        } else
                        {
                            pos.Add(r["PostPosition"].ToString());
                        }

                        newPosition++;
                    } else
                    {
                        threadNum = Convert.ToInt32(r["ThreadID"]);
                    }
                }

                if (threadNum != 0)
                {
                    //remove player from order
                    string removalQuery = $"DELETE From postorder where ThreadID='{threadNum}' AND Player='{unmasked}'";

                    SqlCommand.ExecuteQuery(removalQuery, testing);

                    //update remaining players with new order
                    foreach (string playr in plr)
                    {
                        string updateQuery = $"UPDATE {postOrderTable} SET PostPosition=@position WHERE ThreadID ='{threadNum}' AND Player='{playr}'";
                        string[] par = { "@position" };
                        string[] val = { pos[added] };

                        SqlCommand.ExecuteQuery_Params(updateQuery, par, val);

                        added++;
                    }

                    result = $"The post order has been updated and {masked} has been removed.";
                } else
                {
                    if (masked != "")
                    {
                        result = $"Player '{masked}' not found in order.";
                    } else
                    {
                        result = "Player not found in database, please add to database before attempting to remove from order.";
                    }
                }
            } else { 
                result = $"There are no threads under title or alias with the text {threadId}";

            }


            return result;
        }

        public static string ResetPostOrder(string threadID)
        {
            DataTable dt = QueryThread(threadTable, threadID);
            string result;

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string threadNum = row["ID"].ToString();
                string deleteQuery = $"DELETE FROM {postOrderTable} WHERE ThreadID= {threadNum}";

                SqlCommand.ExecuteQuery(deleteQuery, testing);

                result = "Post order has been reset. You may now execute adding to the order.";
            } else
            {
                result = $"The thread '{threadID}' was not found in the table.";
            }

            return result;
        }

        public static string UpNext(string threadID, bool pingUser)
        {
            string response;
            DataTable dt = QueryThread(threadTable, threadID);

            //query thread exists
            if (dt.Rows.Count > 0)
            {
                int threadNum = GetThreadID(dt, 0);

                string plyr = QueryNextName(threadNum);

                if (pingUser)
                {
                    DateTime pingCooldown = DateTime.Now;

                    if (CooldownExpired(plyr, pingCooldown, threadNum))
                    {
                        SetCooldown(plyr, pingCooldown, threadNum);
                        response = $"Reminder to post in {threadID}, {Player.GetPlayer(plyr, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention)}";
                    } else
                    {
                        response = $"Reminder to post in {threadID}, {plyr}";
                    }
                } else
                {
                    response = $"Currently up to post in {threadID} is {plyr}";
                }
            } else
            {
                response = $"There is no thread in the database for '{threadID}'.";
            }
            return response;
        }
        
        public static string Posted(string threadId, string user)
        {
            //check thread exists
            string response;
            DataTable dt = QueryThread(threadTable, threadId);

            if (dt.Rows.Count > 0)
            {
                int threadNum = GetThreadID(dt, 0);
                string upNext = QueryNextName(threadNum);
                string masked = Player.GetPlayer(user, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);
                string postPos = QueryNextPos(threadNum);

                //check user is up
                if(masked == upNext)
                {
                    string addPostQuery = $"INSERT INTO {postTable}(PostDate, ThreadID, Player, PostPosition) VALUES(@date, @thread, @player, @pos)";

                    string[] arguments = { "@date", "@thread", "@player", "@pos" };
                    string[] values = { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), threadNum.ToString(), user, postPos };
                    string mentionPosters = "";

                    
                    //if user is up, add to database
                    ClearCooldown(user, threadNum);
                    SqlCommand.ExecuteQuery_Params(addPostQuery, arguments, values, testing);

                    upNext = QueryNextName(threadNum);
                    dt = QueryPostOrder(threadNum, postOrderTable);

                    foreach(DataRow row in dt.Rows)
                    {
                        if(row["Player"].ToString() != user)
                        {
                            mentionPosters += $"\n{row["Player"]}";
                        }
                    }                    

                    response = $"Thank you for your post, {masked}. You're up, {upNext} \n {mentionPosters}";
                }
                else
                {
                    //if user is not up, respond asking if order changed

                    response = "You are not up in the roster, has the post order changed?";
                }
            }
            else
            {
                response = $"There is no thread in the database for '{threadId}'.";
            }

            return response;
        }

        public static string UpdatePostOrder(string thread, List<string> players, bool posted = false)
        {
            int x = 1;
            int threadID = GetThreadID(QueryThread(threadTable, thread), 0);
            //purge current order
            ResetPostOrder(thread);

            //purge posted records
            PurgePostedForThread(threadID);

            //insert new post order
            foreach(string player in players)
            {
                string masked = Player.GetPlayer(player, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);

                AddToPostOrder(thread, player, x.ToString(), masked);

                x++;
            }

            //add posted record for #1
            if (posted)
            {
                Posted(thread, players[0]);
            }

            return "The post order has been updated.";
        }

        #region Support
        public static DataTable QueryNextPosts(int threadNum)
        {
            string lastPlayer = "";
            int pos = 0;

            //query the posts table
            DataTable dt = QueryWhosUp(threadNum);

            int lastPos;
            //if posts table is empty set order at 1
            //else get latest record player, post position + 1            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                lastPos = Convert.ToInt32(row["PostPosition"].ToString());
                lastPlayer = row["Player"].ToString();
            }
            else
            {
                lastPos = 0;
            }

            if (lastPos != 0)
            {
                //query the post order
                dt = QueryPostOrder(threadNum, postOrderTable);

                //check user and position match up
                foreach (DataRow row in dt.Rows)
                {
                    //if match up, get by position
                    //if not get user after current player
                    if (lastPlayer != "")
                    {
                        if (row["Player"].ToString() == lastPlayer)
                        {
                            pos = Convert.ToInt32(row["PostPosition"]);
                        }
                    }
                }
            }

            if (pos == dt.Rows.Count)
            {
                pos = 1;
            }
            else { pos++; }

           return QueryPostOrderPosition(postOrderTable, threadNum, pos);
        }
        public static string QueryNextName(int threadNum)
        {
            DataTable dt = QueryNextPosts(threadNum);

            return Player.GetPlayer(dt.Rows[0]["Player"].ToString(), Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker); 
        }

        public static string QueryNextPos(int threadNum)
        {
            DataTable dt = QueryNextPosts(threadNum);

            return dt.Rows[0]["PostPosition"].ToString();
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

        static int GetThreadID(DataTable dt, int rowNum)
        {
            DataRow row = dt.Rows[rowNum];

            return Convert.ToInt32(row["ID"]);
        }

        static bool PlayerAdded(string table, string PlayerID, int threadId)
        {
            string playerQuery = $"SELECT Player from {table} where Player = '{PlayerID}' AND ThreadID ='{threadId}'";            

            DataTable dt = SqlCommand.ExecuteQuery(playerQuery, testing);

            if(dt.Rows.Count > 0)
            {
                return true; 
            } else
            {
                return false;
            }
        }

        static bool NonMentionPlayerAdded(string table, string PlayerID)
        {
            string playerQuery = $"SELECT Player from {table} where Monicker = '{PlayerID}'";

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

        static bool PositionAdded(string table, string positionNum, int threadId)
        {
            string playerQuery = $"SELECT PostPosition from {table} where PostPosition = '{positionNum}'  AND ThreadID ='{threadId}'";

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

        static DataTable QueryPostOrder(DataTable dt, string postOrderTable)
        {
            int threadNum = GetThreadID(dt, 0);
            return QueryPostOrder(threadNum, postOrderTable);
        }

        static DataTable QueryPostOrder(int threadNum, string postOrderTable)
        {
            string PostOrderQuery = $"SELECT * FROM {postOrderTable} WHERE ThreadID = {threadNum} ORDER BY PostPosition ASC";

            return SqlCommand.ExecuteQuery(PostOrderQuery, testing);

        }

        static DataTable QueryPostOrderPosition(string postOrderTable, int threadNum, int postPos)
        {
            string PostOrderQuery = $"SELECT * FROM {postOrderTable} WHERE ThreadID = {threadNum} AND PostPosition = {postPos}";

            return SqlCommand.ExecuteQuery(PostOrderQuery, testing);
        }

        static DataTable QueryWhosUp(int threadID)
        {
            string postQuery = $"SELECT * FROM {postTable} where ThreadID ='{threadID}' ORDER BY PostDate ASC";

            return SqlCommand.ExecuteQuery(postQuery, testing);
        }

        static void PurgePostedForThread(int threadID)
        {
            string postQuery = $"DELETE FROM {postTable} WHERE ThreadID = {threadID}";

            SqlCommand.ExecuteQuery(postQuery, testing);

            ClearCooldown(threadID);
        }

        static void SetCooldown(string user,  DateTime pingTime, int threadNum)
        {
            DateTime cooldownTime = pingTime.AddDays(2);
            string pingQuery = $"INSERT INTO {pingTable} (user, ThreadID, pingAt, cooldown) VALUES(@user, @thread, @pinged, @cooldown)";
            string[] parameters = { "@user", "@thread", "@pinged", "@cooldown" };
            string[] values = { user, threadNum.ToString(), pingTime.ToString("yyyy-MM-dd HH:mm:ss"), cooldownTime.ToString("yyyy-MM-dd HH:mm:ss") };
            SqlCommand.ExecuteQuery_Params(pingQuery, parameters, values);
        }

        static bool CooldownExpired(string user, DateTime pingTime, int threadNum)
        {
            string cooldownQuery = $"SELECT cooldown FROM {pingTable} WHERE ThreadID = {threadNum} AND user = '{user}' ORDER BY cooldown asc";
            DataTable dt = SqlCommand.ExecuteQuery(cooldownQuery, testing);

            if(dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                DateTime cooldown = (DateTime)row["cooldown"];

                if(pingTime > cooldown)
                {
                    ClearCooldown(user, threadNum);
                    return true;
                } else
                {
                    return false;
                }
            } else
            {
                return true;
            }
        }

        static void ClearCooldown(string user, int threadNum)
        {
            string cooldownQuery = $"SELECT cooldown FROM {pingTable} WHERE ThreadID = {threadNum} AND user = '{user}' ORDER BY cooldown asc";
            string deleteCooldownQuery = $"DELETE FROM {pingTable} WHERE ThreadID = {threadNum} AND user='{user}'";

            DataTable dt = SqlCommand.ExecuteQuery(cooldownQuery, testing);

            if(dt.Rows.Count > 0)
            {
                SqlCommand.ExecuteQuery(deleteCooldownQuery,testing);
            }
        }

        static void ClearCooldown(int threadNum)
        {
            string cooldownQuery = $"SELECT cooldown FROM {pingTable} WHERE ThreadID = {threadNum} ORDER BY cooldown asc";
            string deleteCooldownQuery = $"DELETE FROM {pingTable} WHERE ThreadID = {threadNum}";

            DataTable dt = SqlCommand.ExecuteQuery(cooldownQuery, testing);

            if (dt.Rows.Count > 0)
            {
                SqlCommand.ExecuteQuery(deleteCooldownQuery, testing);
            }
        }
        #endregion

    }
}
