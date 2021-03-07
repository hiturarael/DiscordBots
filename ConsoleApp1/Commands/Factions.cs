using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Nine.Commands
{
    public class Factions
    {
        private static readonly string factionTable = "factions";
        public static readonly bool testing = false;

        public enum FactionStatus
        {
            Active,
            Restricted,
            Closed
        }
        
        public static string Faction(string Faction)
        {
            if(FactionExists(Faction))
            {
                DataRow row = QueryFaction(Faction).Rows[0];
                string leaderName = Characters.GetCharName(Convert.ToInt32(row["LeaderID"]));
                if(leaderName == "")
                {
                    leaderName = "None";
                }

                return $"Faction: {Faction} \n     Leader: {leaderName}\n     {row["FactionStatus"]}     \n{row["ProfileURL"]}";
            } else
            {
                return "That faction is not in my records.";
            }
        }

        public static string AddFaction(string Faction, string LeaderFirstName, string LeaderLastName, string URL)
        {
            if(!FactionExists(Faction))
            {
                int leaderID = Characters.GetCharID(LeaderFirstName, LeaderLastName);

                if (leaderID != 0)
                {
                    string insertQuery = $"INSERT INTO {factionTable}(Faction, LeaderID, ProfileURL) VALUES(@Faction, @Leader, @URL)";
                    string[] Parameters = { "@Faction", "@Leader", "@URL" };
                    string[] Values = { Faction, leaderID.ToString(), URL};

                    SqlCommand.ExecuteQuery_Params(insertQuery, Parameters, Values, testing);

                    return "I have added the faction to the records.";
                } else
                {
                    return "The character specified is not yet in the records. Please add them and try again.";
                }
            } else
            {
                return "That faction already exists in my records.";
            }
        }

        #region support
        static DataTable QueryFaction(string Faction)
        {
            string factionQuery = $"SELECT * FROM {factionTable} where Faction = '{Faction}'";

            return SqlCommand.ExecuteQuery(factionQuery, testing);
        }

        public static bool FactionExists(string Faction)
        {
            if(QueryFaction(Faction).Rows.Count > 0)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public static int GetFactionID(string Faction)
        {
            string query = $"SELECT FactionID FROM {factionTable} WHERE Faction='{Faction}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, testing);

            if (dt.Rows.Count > 0)
            {
                return (int)dt.Rows[0]["FactionID"];
            }
            else
            {
                return 0;
            }
        }
        #endregion
    }
}
