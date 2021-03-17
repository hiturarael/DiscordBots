using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;


namespace Nine.Commands
{
    public class Dictionary
    {
        public static readonly bool testing = false;
        public static readonly string dictionaryTable = "dictionary";

        public static string NewTerm(string Term, string Definition, bool addAnyway = false)
        {
            string response;

            if (!TermExists(Term) || (TermExists(Term) && addAnyway))
            {
                int x = 1;

                if (addAnyway)
                {
                    x = 0;
                }

                AddTerm(Term, Definition, x);

                if (TermExists(Term))
                {
                    response = "The term has been added to the dictionary.";
                } else
                {
                    response = "Something went wrong and the term has not been added";
                }
            } else
            {
                response = $"This term is already in the database with the following definitions: \n{PrintDefinitions(Term)}";
            }

            return response;
        }

        public static string Definition(string Term)
        {
            if(TermExists(Term))
            {
                return $"{Term} - \n{PrintDefinitions(Term)}";
            } else
            {
                return "This term is not in the dictionary yet.";
            }
        }

        public static string UpdateDefinition(string Term, string Definition, int DefNum)
        {
            if(TermExists(Term))
            {
                string updateQuery = $"UPDATE {dictionaryTable} SET Definition='{Definition}' WHERE Term = '{Term}' AND DefinitionNum = {DefNum}";

                SqlCommand.ExecuteQuery(updateQuery, NineBot.cfgjson);
            } 
            
            return "I have updated that term at the specified definition number.";
        }

        public static string RemoveTermDefinition(string Term, int DefNum, bool deleteAll)
        {
            if(TermExists(Term))
            {
                if(!deleteAll)
                {
                    string deleteQuery = $"DELETE FROM {dictionaryTable} WHERE Term = '{Term}' AND DefinitionNum = {DefNum}";
                    string updateQuery;

                    SqlCommand.ExecuteQuery(deleteQuery, NineBot.cfgjson);

                    DataTable dt = QueryTerm(Term);

                    for(int x = DefNum; x <= dt.Rows.Count; x++)
                    {
                        updateQuery = $"UPDATE {dictionaryTable} SET DefinitionNum={x} WHERE Term = '{Term}' AND DefinitionNum = {x+1}";

                        SqlCommand.ExecuteQuery(updateQuery, NineBot.cfgjson);
                    }

                    return "The term definition was removed and the positions of following definitions updated.";
                } else
                {
                    string deleteQuery = $"DELETE FROM {dictionaryTable} WHERE Term = '{Term}'";
                    SqlCommand.ExecuteQuery(deleteQuery, NineBot.cfgjson);

                    return "The term and definitions have been removed from the dictionary.";
                }
            } else
            {
                return "That term is not present in the dictionary at this time.";
            }
        }

        #region Support
        static void AddTerm(string Term, string Definition, int DefNum = 0)
        {
            string query = $"INSERT INTO {dictionaryTable}(Term, DefinitionNum, Definition) VALUES(@Term, @DefinitionNum, @Definition)";

            if(DefNum == 0)
            {
                DefNum = NextTermDefNum(Term);
            }

            string[] Parameters = { "@Term", "@DefinitionNum", "@Definition" };
            string[] Values = { Term, DefNum.ToString(), Definition };

            SqlCommand.ExecuteQuery_Params(query, NineBot.cfgjson, Parameters, Values);
        }

        static int NextTermDefNum(string Term)
        {
            DataTable dt = QueryTerm(Term);
            List<int> definitionNums = new List<int>();
            int defNumOpen = 0;

            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    definitionNums.Add((int)row["DefinitionNum"]);
                }

                for (int x = 1; x < definitionNums.Count+1; x++)
                {
                    if (x != definitionNums[x-1])
                    {
                        defNumOpen = x;
                    }
                }

                if (defNumOpen == 0)
                {
                    defNumOpen = definitionNums.Count + 1;
                }
            } catch (Exception ex)
            {
                throw ex;
            }

            return defNumOpen;
        }
        static DataTable QueryTerm(string Term)
        {
            string Query = $"SELECT * FROM {dictionaryTable} WHERE Term = '{Term}' ORDER BY 'DefinitionNum' ASC";

           return SqlCommand.ExecuteQuery(Query, NineBot.cfgjson);
        }

        static string PrintDefinitions(string Term)
        {
            DataTable dt = QueryTerm(Term);
            string definitions = "";

            foreach(DataRow row in dt.Rows)
            {
                definitions += $"({row["DefinitionNum"]}) - {row["Definition"]}\n\n";
            }

            definitions = definitions.Trim('\n');

            return definitions;
        }

        static bool TermExists(string Term)
        {
            if(QueryTerm(Term).Rows.Count > 0)
            {
                return true;
            } else
            {
                return false;
            }
        }
        #endregion

    }
}
