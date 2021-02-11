using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Nine
{
    public class Character
    {
        public static readonly bool testing = true;

        public static string WhoPlays(string name)
        {
            string table = "nine_whoplays";
            string query = $"SELECT * FROM {table} WHERE CharName LIKE '%{name}%'";

            string result;

            try
            {
                DataTable dt = SqlCommand.ExecuteQuery(query,testing);

                if (dt.Rows.Count == 0)
                {
                    result = "I'm sorry, there were no records with that name.";
                }
                else
                {

                    if (dt.Rows.Count > 1)
                    {
                        result = $"There are multiple results containing the name {name}, please try again and narrow down your search.";
                    }
                    else
                    {
                        DataRow row = dt.Rows[0];

                        result = $"The character {row["CharName"]} is played by ${row["Player"]} and pilots the {row["Unit"]}";
                    }
                }

            }
            catch
            {
                result = "I'm sorry, something went wrong with the query.";
            }

            return result;
        }

    }
}
