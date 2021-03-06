﻿using DiscordBots.SQL;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Nine.Commands
{
    public class Player
    {
        public static readonly bool testing = false;
        public static readonly string playerTable = "players";
        public enum PlayerStatus
        {
            Active,
            Inactive
        }

        public enum PlayerSearch
        {
            Mention,
            Monicker
        }

        public static string AddPlayer(string player, string monicker)
        {
            string userQuery = $"SELECT * from {playerTable} where Player = '{player}'";
            string monickerQuery = $"SELECT * from {playerTable} where Monicker LIKE '%{monicker}%'";
            string addQuery = $"INSERT INTO {playerTable}(Player, Monicker) VALUES(@player, @monicker)";

            string[] parameters = { "@player", "@monicker"};
            string[] values = { player, monicker };

            string response;
            try
            {
                //check for @user existing
                DataTable dt = SqlCommand.ExecuteQuery(userQuery, NineBot.cfgjson);

                if (dt.Rows.Count < 1)
                {
                    //check for monicker existing
                    dt = SqlCommand.ExecuteQuery(monickerQuery, NineBot.cfgjson);

                    if (dt.Rows.Count < 1)
                    {
                        //add user
                        SqlCommand.ExecuteQuery_Params(addQuery, NineBot.cfgjson, parameters, values);

                        dt = SqlCommand.ExecuteQuery(userQuery, NineBot.cfgjson);

                        if (dt.Rows.Count == 1)
                        {
                            response = "Player has been added.";
                        }
                        else if (dt.Rows.Count < 1)
                        {
                            response = "Something went wrong and player was not added.";
                        }
                        else
                        {
                            response = "Something went REALLY wrong and the player was added more than once.";
                        }
                    }
                    else
                    {
                        response = "That monicker is already in use.";
                    }
                }
                else
                {
                    response = "That user has already been added.";
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return response;
        }

        public static string UpdatePlayerStatus(string player, PlayerStatus status)
        {
            string query = $"UPDATE {playerTable} SET Status='{status}' WHERE Monicker='{player}'";
            if(!player.Contains("<@!"))
            {
                player = GetPlayer(player, PlayerSearch.Monicker, PlayerSearch.Mention);
            }

            if(GetPlayerID(player) < 1)
            {
                return "There is no player by that name in the database.";
            }

            if(player.Contains("<@") || player.Contains("<@!"))
            {
                player = GetPlayer(player, PlayerSearch.Mention, PlayerSearch.Monicker);
            }

            SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return $"{player} has been updated.";
        }

        public static string GetAllPlayersMentions(PlayerStatus status)
        {
            string query = $"SELECT Player from {playerTable} WHERE Status = '{status}'";
            DataTable dt = null;
            string users = "";
            dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {                
                foreach (DataRow row in dt.Rows)
                {
                    users += $"{row["Player"].ToString()} ";
                }
            } else
            {
                users = $"No users are marked as {status}";
            }

            return users;
        }

        public static string UpdatePlayerMonicker(string player, string Monicker)
        {
            string query = $"UPDATE {playerTable} SET Monicker='{Monicker}' WHERE Monicker='{player}'";

            if (GetPlayerID(player) > 0)
            {
                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                return $"{player} has been updated, please remember to use {Monicker} for them from now on.";
            } else
            {
                return "That player is not in the database.";
            }
        }

        public static string GetThreadsImIn(string player)
        {
            string id = player.Replace("!@","").Replace("<","").Replace(">","");

            string query = $"use `{NineBot.cfgjson.Database}`; SELECT `postorder`.`ThreadID`, `postorder`.`Player`, `threads`.`Title` FROM `postorder` INNER JOIN `threads` ON `postorder`.`ThreadID` = `threads`.`ID` WHERE `postorder`.`Player` LIKE \"%{id}%\"";
            string resp = "";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    resp += $"{row["Title"]}\n";
                }
            } else
            {
                resp = "You are currently not in any threads, senpai.";
            }

            return resp;

        }
        #region Support
        public static bool GetPlayerStatus(string player, PlayerSearch search)
        {
            string playerCheck = $"Select * from {playerTable} where Player = '{player}'";
            string monickerCheck = $"Select * from {playerTable} where Monicker = '{player}'";

            DataTable dt = null;
            string active = "";

            switch (search)
            {
                case PlayerSearch.Mention:
                    dt = SqlCommand.ExecuteQuery(playerCheck, NineBot.cfgjson);
                    break;
                case PlayerSearch.Monicker:
                    dt = SqlCommand.ExecuteQuery(monickerCheck, NineBot.cfgjson);
                    break;
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                 active = row["Status"].ToString();
            }

            if(active == PlayerStatus.Active.ToString())
            {
                return true;
            } else { return false; }
        }

        public static string GetPlayer(string player, PlayerSearch search, PlayerSearch ret)
        {
            string name = "";

            string playerCheck;
            string monickerCheck;

            player = player.Replace("<@!", "").Replace("<@", "").Replace(">", "");

            if(player.Contains("Member") && player.Contains("#"))
            {
                string[] authorSearch = player.Split(" ");

                player = $"<@!{authorSearch[1].Replace(";", "").Trim()}>";
            }

            playerCheck = $"Select * from {playerTable} where Player LIKE '%{player}%'";
            monickerCheck = $"Select * from {playerTable} where Monicker LIKE '%{player}%'";

            DataTable dt = null;

            switch(search)
            {
                case PlayerSearch.Mention:
                    dt = SqlCommand.ExecuteQuery(playerCheck, NineBot.cfgjson);
                    break;
                case PlayerSearch.Monicker:
                    dt = SqlCommand.ExecuteQuery(monickerCheck, NineBot.cfgjson);
                    break;
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                switch (ret)
                {
                    case PlayerSearch.Mention:
                        name = row["Player"].ToString();
                        break;
                    case PlayerSearch.Monicker:
                        name = row["Monicker"].ToString();
                        break;
                }
            }

            return name;
        }

        public static int GetAdmin(string player, PlayerSearch search)
        {
            int adminStatus = 0;

            string playerCheck;
            string monickerCheck;

            if (player.Contains("Member") && player.Contains("#"))
            {
                string[] authorSearch = player.Split(" ");

                player = $"<@!{authorSearch[1].Replace(";", "").Trim()}>";
            }

            playerCheck = $"Select * from {playerTable} where Player = '{player}'";
            monickerCheck = $"Select * from {playerTable} where Monicker = '{player}'";

            DataTable dt = null;

            switch (search)
            {
                case PlayerSearch.Mention:
                    dt = SqlCommand.ExecuteQuery(playerCheck, NineBot.cfgjson);
                    break;
                case PlayerSearch.Monicker:
                    dt = SqlCommand.ExecuteQuery(monickerCheck, NineBot.cfgjson);
                    break;
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                adminStatus = Convert.ToInt32(row["Admin"]);
            }

            return adminStatus;
        }

        public static int GetPlayerID(string playerMention)
        {
            if(!playerMention.Contains("<@"))
            {
                playerMention = GetPlayer(playerMention, PlayerSearch.Monicker, PlayerSearch.Mention);
            }

            playerMention = playerMention.Replace("<@!", "").Replace("<@", "").Replace(">", "");
            string query = $"SELECT ID FROM {playerTable} WHERE Player LIKE'%{playerMention}%'";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if(dt.Rows.Count > 0)
            {
                return (int)dt.Rows[0]["ID"];
            } else
            {
                return 0;
            }
        }

        public static string GetPlayerByID(int ID)
        {
            string query = $"SELECT Monicker FROM {playerTable} WHERE ID={ID}";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return dt.Rows[0]["Monicker"].ToString();
        }
        #endregion
    }
}
