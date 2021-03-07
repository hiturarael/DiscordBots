using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Nine.Commands
{
    public class Units
    {
        public enum UnitStatus
        {
            Open,
            Taken,
            Banned,
            Reserved
        }

        public static readonly bool testing = false;
        private static readonly string unitTable = "unitlisting";

        public static string AddUnit(string Unit, string AddedBy, UnitStatus Status, string ReservedFor = "", string MassProduced = "No")
        {
            string response;

            if(!UnitAdded(Unit))
            {

                if(Status == UnitStatus.Reserved || Status == UnitStatus.Taken)
                {
                    if(MassProduced == "Yes")
                    {
                        return "A mass produced weapon cannot be reserved or assigned to a player.";
                    }
                }

                string addQuery = $"INSERT INTO {unitTable}(UnitName, AddedBy, Status, MassProduced) VALUES(@Unit, @AddedBy, @Status, @MP)";
                string[] Parameters = { "@Unit", "@AddedBy", "@Status", "@MP" };
                string[] Values = { Unit, AddedBy, Status.ToString(), MassProduced };

                if ((!string.IsNullOrEmpty(ReservedFor) && Status == UnitStatus.Reserved) || (!string.IsNullOrEmpty(ReservedFor) && Status == UnitStatus.Taken))
                {
                    string fields = "(UnitName, AddedBy, Status, ReservedFor, MassProduced)";

                    if(Status == UnitStatus.Taken && ReservedFor != "")
                    {
                        fields = "(UnitName, AddedBy, Status, AssignedTo, MassProduced)";
                    }
                    addQuery = $"INSERT INTO {unitTable}{fields} VALUES(@Unit, @AddedBy, @Status, @Reserved, @MP)";

                    Parameters = new string[5];
                    Values = new string[5];

                    Parameters[0] = "@Unit";
                    Parameters[1] = "@AddedBy";
                    Parameters[2] = "@Status";
                    Parameters[3] = "@Reserved";
                    Parameters[4] = "@MP";
                    Values[0] = Unit;
                    Values[1] = AddedBy;
                    Values[2] = Status.ToString();
                    Values[3] = ReservedFor;
                    Values[4] = MassProduced;
                }
                else if (string.IsNullOrEmpty(ReservedFor) && Status == UnitStatus.Reserved)
                {
                    return "Since you are trying to reserve a machine, you need to tell me who you're reserving it for by their monicker registered in the player database.";
                }
                else if (string.IsNullOrEmpty(ReservedFor) && Status == UnitStatus.Taken)
                {
                    return "Since you are trying to assign a machine, you need to tell me which player it is assigned to by their monicker registered in the player database.";
                }

                SqlCommand.ExecuteQuery_Params(addQuery, Parameters, Values, testing);

                if(UnitAdded(Unit))
                {
                    response = "The unit has been added to the database.";
                } else
                {
                    response = "Something went wrong, I couldn't add it to the datbase.";
                }
                
            } else
            {
                response = $"'{Unit}' has already been added to the database.";
            }

            return response;
        }

        public static string UpdateUnitStatus(string Unit, UnitStatus UnitStatus, string ReservedFor = "")
        {
            string response;

            if(UnitAdded(Unit))
            {
                if (UnitStatus == UnitStatus.Reserved || UnitStatus == UnitStatus.Taken)
                {
                    string MassProduced = GetMassProduced(Unit);

                    if (MassProduced == "Yes")
                    {
                        return "A mass produced weapon cannot be reserved or assigned to a player.";
                    }
                }

                string updateQuery = $"UPDATE {unitTable} SET Status = @status WHERE UnitName='{Unit}'";
                string[] Parameters = { "@status" };
                string[] Values = { UnitStatus.ToString() };
                string rsvd = GetReserved(Unit);

                if(!string.IsNullOrEmpty(ReservedFor) && UnitStatus == UnitStatus.Reserved)
                {
                    updateQuery = $"UPDATE {unitTable} SET Status = @status, ReservedFor = @reserved WHERE UnitName='{Unit}'";
                    
                    Parameters = new string[2];
                    Values = new string[2];

                    Parameters[0] = "@status";
                    Parameters[1] = "@reserved";
                    Values[0] = Unit;
                    Values[1] = ReservedFor;

                } else if(string.IsNullOrEmpty(ReservedFor) && UnitStatus == UnitStatus.Reserved)
                {
                    return "Since you are trying to reserve a machine, you need to tell me who you're reserving it for by their monicker registered in the player database.";
                }

                if (UnitStatus == UnitStatus.Taken && rsvd != "")
                {
                    if (rsvd == ReservedFor && ReservedFor != "")
                    {
                        SetAssigneeFromReserved(Unit, rsvd);
                    } else if (ReservedFor != "" && rsvd != ReservedFor)
                    {
                        return $"I'm sorry, that unit is reserved for {rsvd}.";
                    } 
                }
                else if (UnitStatus == UnitStatus.Taken && rsvd == "")
                {
                    SqlCommand.ExecuteQuery($"UPDATE {unitTable} SET AssignedTo = '{ReservedFor}' where UnitName='{Unit}'", testing);
                }

                SqlCommand.ExecuteQuery_Params(updateQuery, Parameters, Values);

                if(UnitStatus == UnitStatus.Open || UnitStatus == UnitStatus.Banned)
                {
                    ClearOnOpen(Unit);
                }

                response = $"I have updated the status of the unit to {UnitStatus}.";

            } else
            {
                response = "I can't seem to find that unit in my database. You sure it's been added?";
            }

            return response;
        }

        public static string ToggleMassProduced(string Unit, string MassProduced = "No")
        {
            string MP = GetMassProduced(Unit);
            string query;

            if(MP == "No")
            {
                if(MassProduced == "Yes")
                {
                    query = $"UPDATE {unitTable} SET MassProduced='Yes' WHERE UnitName = '{Unit}''";
                } else
                {
                    return "That unit is already flagged as not Mass Produced";
                }
            } else
            {
                if(MassProduced == "Yes")
                {
                    return "That unit is already flagged as Mass Produced";
                } else
                {
                    query = $"UPDATE {unitTable} SET MassProduced='No' WHERE UnitName = '{Unit}''";
                }
            }

            SqlCommand.ExecuteQuery(query, testing);
            return $"Flag has been set to '{MassProduced}' for {Unit}.";
        }

        #region Support
        public static bool UnitAdded(string Unit)
        {
            if(QueryUnit(Unit).Rows.Count > 0)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public static UnitStatus GetStatus(string Unit)
        {
            DataRow row = QueryUnit(Unit).Rows[0];
            return (UnitStatus) Enum.Parse(typeof(UnitStatus), row["Status"].ToString());
        }

        public static string GetAssignee(string Unit)
        {
            DataRow row = QueryUnit(Unit).Rows[0];
            return row["AssignedTo"].ToString();
        }

        public static string GetReserved(string Unit)
        {
            DataRow row = QueryUnit(Unit).Rows[0];
            return row["ReservedFor"].ToString();
        }

        public static string GetMassProduced(string Unit)
        {
            DataRow row = QueryUnit(Unit).Rows[0];
            return row["MassProduced"].ToString();
        }
        static DataTable QueryUnit(string Unit)
        {
            string query = $"SELECT * FROM {unitTable} WHERE UnitName = '{Unit}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, testing);

            return dt;
        }

        public static int GetUnitID(string unitName)
        {
            string query = $"SELECT ID FROM {unitTable} WHERE UnitName='{unitName}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, testing);

            if (dt.Rows.Count > 0)
            {
                return (int)dt.Rows[0]["ID"];
            }
            else
            {
                return 0;
            }
        }

        public static void SetAssigneeFromReserved(string Unit, string Player)
        {
            string query = $"UPDATE {unitTable} SET ReservedFor='', AssignedTo='{Player}' WHERE UnitName='{Unit}'";

            SqlCommand.ExecuteQuery(query, testing);
        }

        public static void ClearOnOpen(string Unit)
        {
            string query = $"UPDATE {unitTable} SET ReservedFor = @rsvdFor, AssignedTo = @asgnTo WHERE UnitName='{Unit}'";
            string[] parameters = { "@rsvdFor", "@asgnTo" };
            string[] values = { "", "" };
            SqlCommand.ExecuteQuery_Params(query, parameters, values, testing);
        }
        #endregion
    }
}
