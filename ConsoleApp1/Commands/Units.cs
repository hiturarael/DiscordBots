﻿using DiscordBots.SQL;
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

        public enum AliasType
        {
            Variant,
            Alias,
            Upgrade
        }

        public static readonly bool testing = false;
        private static readonly string unitTable = "unitlisting";
        private static readonly string aliasTable = "unitAlias";

        public static string AddUnit(string Unit, string AddedBy, UnitStatus Status, string ReservedFor = "", string MassProduced = "No")
        {
            string response;

            Unit = Functions.CleanString(Unit);
            ReservedFor = Functions.CleanString(ReservedFor);
            MassProduced = Functions.CleanString(MassProduced);

            if(MassProduced.ToLower() != "yes" && MassProduced.ToLower() != "no")
            {
                return "The mass produced flag must be set as yes or no.";
            }

            if(!UnitAdded(Unit))
            {
                if(Status == UnitStatus.Reserved || Status == UnitStatus.Taken)
                {
                    if(MassProduced.ToLower() == "yes")
                    {
                        return "A mass produced weapon cannot be reserved or assigned to a player.";
                    }
                }

                if(Player.GetPlayerID(ReservedFor) < 1)
                {
                    return "I'm sorry, there is no player by that name in the database to reserve or assign to them.";
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

                SqlCommand.ExecuteQuery_Params(addQuery, NineBot.cfgjson, Parameters, Values);

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

        public static string EditUnitName(string UnitName, string updatedName)
        {
            if(UnitAdded(UnitName))
            {
                //update name
               string query =  $"UPDATE {unitTable} SET UnitName = @unitName WHERE UnitName='{UnitName}'";
                string[] parameters = { "@unitName" };
                string[] values = { updatedName };

                SqlCommand.ExecuteQuery_Params(query, NineBot.cfgjson, parameters, values);

                return "The name has been updated.";
            } else
            {
                return "I couldn't find the unit to update.";
            }
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
                    Values[0] = UnitStatus.ToString();
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
                    SqlCommand.ExecuteQuery($"UPDATE {unitTable} SET AssignedTo = '{ReservedFor}' where UnitName='{Unit}'", NineBot.cfgjson);
                }

                SqlCommand.ExecuteQuery_Params(updateQuery, NineBot.cfgjson, Parameters, Values);

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
                    query = $"UPDATE {unitTable} SET MassProduced='Yes' WHERE UnitName = '{Unit}'";
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
                    query = $"UPDATE {unitTable} SET MassProduced='No' WHERE UnitName = '{Unit}'";
                }
            }

            SqlCommand.ExecuteQuery(query, NineBot.cfgjson);
            return $"Flag has been set to '{MassProduced}' for {Unit}.";
        }

        public static List<string> ListUnits(UnitStatus status, bool MP = false)
        {
            string query = $"SELECT ID, UnitName FROM {unitTable} WHERE Status = '{status}'";
            List<string> outcome = new List<string>();

            if(MP)
            {
                query += "AND MassProduced = 'Yes'";
            }

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            foreach(DataRow row in dt.Rows)
            {
                string aliasQuery = $"SELECT UnitName, Type FROM {aliasTable} WHERE parentID = {row["ID"]} ORDER BY Type DESC";
               DataTable dt2 = SqlCommand.ExecuteQuery(aliasQuery, NineBot.cfgjson);
                outcome.Add($"\n{row["UnitName"]}");

                foreach(DataRow aliasRow in dt2.Rows)
                {
                    outcome.Add($"\n\t*{aliasRow["UnitName"]} ({aliasRow["Type"]})");
                }
            }

            return outcome;
        }

        public static List<string> ListUnits()
        {
            string query = $"SELECT ID, UnitName FROM {unitTable}";
            List<string> outcome = new List<string>();

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            foreach (DataRow row in dt.Rows)
            {
                string aliasQuery = $"SELECT UnitName, Type FROM {aliasTable} WHERE parentID = {row["ID"]} ORDER BY Type DESC";
                DataTable dt2 = SqlCommand.ExecuteQuery(aliasQuery, NineBot.cfgjson);
                outcome.Add($"\n{row["UnitName"]}");

                foreach (DataRow aliasRow in dt2.Rows)
                {
                    outcome.Add($"\n\t*{aliasRow["UnitName"]} ({aliasRow["Type"]})");
                }
            }

            return outcome;
        }

        public static string AddAlias(string unitName, string aliasName, AliasType type)
        {
            int unitId = GetUnitID(unitName);
            string unitQuery = $"INSERT INTO {aliasTable}(ParentID, UnitName, Type) VALUES(@parent, @unit, @type)";
            //$"INSERT INTO {unitTable}(UnitName, AddedBy, Status, MassProduced) VALUES(@Unit, @AddedBy, @Status, @MP)";
            if (unitId == 0)
            {
                unitId = GetAliasID(unitName);

                if (unitId == 0)
                {
                    //AddUnit(unitName, "", UnitStatus.Open);
                    //unitId = GetUnitID(unitName);
                } else
                {
                    unitId = GetAliasParent(unitId);
                }
            }

            if (unitId > 0)
            {
                string[] parameters = { "@parent", "@unit", "@type" };
                string[] parameterValues = { unitId.ToString(), aliasName, type.ToString() };

                SqlCommand.ExecuteQuery_Params(unitQuery, NineBot.cfgjson, parameters, parameterValues);

                return $"I have added {aliasName} as a {type} of {unitName}";
            }
            else
            {
                return "The base unit was not in the database and I was unable to add it.";
            }
        }

        public static string QueryMechOwner(string Mech)
        {
            DataTable dt = QueryUnit(Mech);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                UnitStatus status = GetStatus(Mech);
                string mp = GetMassProduced(Mech);

                if(status == UnitStatus.Banned)
                {
                    return "That unit is banned, sooo... no one.";
                } else if(mp == "Yes")
                {
                    return "That unit is a mass produced, I don't keep track of who uses them.";
                }
                else if (row["ReservedFor"].ToString() != "")
                {
                    return $"The unit is utilized by {row["ReservedFor"]}";
                }
                else if (row["AssignedTo"].ToString() != "")
                {
                    return $"The unit is utilized by { Player.GetPlayer(row["AssignedTo"].ToString(), Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker)}";
                }
                else
                {
                    return "That unit is not in use at this time.";
                }
            } else
            {
                return "That unit is not in the database.";
            }
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

        public static int GetAliasID(string unit)
        {
            string query = $"SELECT * FROM {aliasTable} where UnitName = '{unit}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                return (int)row["ID"];
            } else
            {
                return 0;
            }
        }

        public static int GetAliasParent(int aliasID)
        {
            string query = $"SELECT * FROM {aliasTable} where ID = {aliasID}";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                return (int)row["ParentID"];
            } else
            {
                return 0;
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
            string query = $"SELECT * From {unitTable} Left JOIN {aliasTable} on {aliasTable}.ParentID = {unitTable}.ID where {unitTable}.unitname = '{Unit}' OR {aliasTable}.UnitName = '{Unit}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return dt;
        }

        public static int GetUnitID(string unitName)
        {
            string query = $"SELECT ID FROM {unitTable} WHERE UnitName='{unitName}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

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

            SqlCommand.ExecuteQuery(query, NineBot.cfgjson);
        }

        public static void ClearOnOpen(string Unit)
        {
            string query = $"UPDATE {unitTable} SET ReservedFor = @rsvdFor, AssignedTo = @asgnTo WHERE UnitName='{Unit}'";
            string[] parameters = { "@rsvdFor", "@asgnTo" };
            string[] values = { "", "" };
            SqlCommand.ExecuteQuery_Params(query, NineBot.cfgjson, parameters, values);
        }

        public static string GetUnitbyID(int UnitID)
        {
            string query = $"SELECT UnitName FROM {unitTable} WHERE ID={UnitID}";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["UnitName"].ToString();
            } else
            {
                return "";
            }
        }
        #endregion
    }
}
