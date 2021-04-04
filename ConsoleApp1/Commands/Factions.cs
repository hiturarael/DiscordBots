using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Nine.Commands
{
    public class Factions
    {
        private static readonly string charTable = "characterinfo";
        private static readonly string factionTable = "factions";
        public static readonly bool testing = false;

        public enum FactionStatus
        {
            Active,
            Restricted,
            Closed,
            Defunct
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

                return $"{Faction} \n\t\tLeader: {leaderName}\n\t\tFaction Status: {row["FactionStatus"]}\n\t\tProfile Link: {row["ProfileURL"]}";
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

                string insertQuery = $"INSERT INTO {factionTable}(Faction, LeaderID, ProfileURL) VALUES(@Faction, @Leader, @URL)";
                string[] Parameters = { "@Faction", "@Leader", "@URL" };
                string[] Values = { Faction, leaderID.ToString(), URL};

                SqlCommand.ExecuteQuery_Params(insertQuery, NineBot.cfgjson, Parameters, Values);

                if (leaderID != 0)
                {
                    return "I have added the faction to the records.";
                } else
                {
                    return "The character designated to lead the faction was not in the database. I have added the faction with no leader as a temporary measure. Please update the faction once the leader is in the database.";
                }
            } else
            {
                return "That faction already exists in my records.";
            }
        }

        public static string UpdateFactionLeader(string Faction, string LeaderFirstName, string LeaderLastName)
        {
            int leaderID = Characters.GetCharID(LeaderFirstName, LeaderLastName);
            string query = $"UPDATE {factionTable} SET LeaderID={leaderID} WHERE Faction = '{Faction}'";

            SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (leaderID != 0)
            {
                return $"I have updated the leader of '{Faction}'.";
            } else
            {
                return $"I guess the faction is leaderless or the character you specified is not yet in the database. Anyway, I updated the record to blank.";
            }
        }

        public static string UpdateFactionName(string Faction, string NewName)
        {

            string query = $"UPDATE {factionTable} SET Faction='{NewName}' WHERE Faction = '{Faction}'";

            SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return $"{Faction} has been updated to be {NewName}.";
        }

        public static string UpdateFactionStatus(string Faction, FactionStatus status)
        {
            string query = $"UPDATE {factionTable} SET FactionStatus='{status}' WHERE Faction = '{Faction}'";

            SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            switch(status)
            {
                case FactionStatus.Active:
                    return $"{Faction} has been set to active and accepting new meat.";
                case FactionStatus.Closed:
                    return $"{Faction} has been set to closed and is no longer accepting meat.";
                case FactionStatus.Restricted:
                    return $"{Faction} is now restricted and only accepting meat based on approvals.";
                case FactionStatus.Defunct:
                    return $"{Faction} is now defunct and no longer in operation.... or is it?";
                default:
                    return "Something went REALLY wrong and I have no idea how we got here... someone call Sess!";
            }
        }

        public static string UpdateFactionURL(string Faction, string URL)
        {
            string query = $"UPDATE {factionTable} SET ProfileURL='{URL}' WHERE Faction = '{Faction}'";

            SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return $"The Url for {Faction} has been updated.";
        }

        public static string ListFactions(FactionStatus status)
        {
            string query = $"SELECT Faction FROM {factionTable} WHERE FactionStatus = '{status}'";
            string output = $"The following factions have the status '{status}':";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            foreach(DataRow row in dt.Rows)
            {
                output += $"\n{row["Faction"]}";
            }

            return output;
        }

        public static string ListAllFactions()
        {
            string query = $"SELECT Faction FROM {factionTable}";
            string output = $"Here is a list of all factions in our records:";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            foreach (DataRow row in dt.Rows)
            {
                output += $"\n{row["Faction"]}";
            }

            return output;
        }

        public static string ListFactionMembers(string Faction)
        {
            int factionID = GetFactionID(Faction);
            string query = $"SELECT FirstName, LastName FROM {charTable} WHERE FactionID = {factionID}";
            string output = $"The following characters are members of {Faction}:";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            foreach(DataRow row in dt.Rows)
            {
                output += $"\n{row["FirstName"]} {row["LastName"]}";
            }

            return output;
        }

        #region support
        static DataTable QueryFaction(string Faction)
        {
            string factionQuery = $"SELECT * FROM {factionTable} where Faction = '{Faction}'";

            return SqlCommand.ExecuteQuery(factionQuery, NineBot.cfgjson);
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

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {
                return (int)dt.Rows[0]["FactionID"];
            }
            else
            {
                return 0;
            }
        }

        public static string GetFactionByID(int FactionID)
        {
            string query = $"SELECT Faction FROM {factionTable} WHERE FactionID ={FactionID}";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return dt.Rows[0]["Faction"].ToString();
        }
        #endregion
    }
}
