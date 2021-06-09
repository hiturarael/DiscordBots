using DiscordBots.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using Nine.Commands;
using System.Linq;

namespace Nine
{
    public struct CharInfo
    {
        public int charID { get; set; }
        public string FirstName { get; set; }
        public string PrevFN { get; set; }
        public string LastName { get; set; }
        public string PrevLN { get; set; }
        public string Gender { get; set; }
        public string PrevGender { get; set; }
        public string Unit { get; set; }
        public string PrevUnit { get; set; }
        public string Faction { get; set; }
        public string PrevFaction { get; set; }
        public string Url { get; set; }
        public string PrevUrl { get; set; }
        public string Blurb { get; set; }
        public string PrevBlurb { get; set; }
        public string Player { get; set; }
        public string PrevPlayer { get; set; }
        public bool Errored { get; set; }
        public bool Quit { get; set; }
        public bool Correct { get; set; }
        public bool Relist { get; set; }
        public CharStatus status { get; set; }
        public CharType type { get; set; }
    }

    public enum CharStatus
    {
        Active,
        Inactive,
        Dead,
        Template
    }

    public enum CharType
    {
        PC,
        NPC,
        Support,
        Template
    }
    public class Characters
    {
        public static readonly bool testing = false;
        private static readonly string charTable = "characterinfo";
        public static string WhoPlays(string firstname, string lastname)
        {
            string query = $"SELECT * FROM {charTable} WHERE FirstName='{firstname}' AND LastName='{lastname}'";

            string result;

            try
            {
                DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                if (dt.Rows.Count == 0)
                {
                    result = "I'm sorry, there were no records with that name.";
                }
                else
                {
                    DataRow row = dt.Rows[0];
                    string player = Player.GetPlayerByID(Convert.ToInt32(row["PlayerID"]));

                    result = $"{player} plays {firstname} {lastname}";
                }
            }
            catch (Exception ex)
            {
                result = "I'm sorry, something went wrong with the query.";
            }

            return result;
        }

        public static DataTable WhoIs(string Firstname, string Lastname)
        {
            string query = $"SELECT * FROM {charTable} WHERE FirstName='{Firstname}'";

            if (Lastname == "")
            {
                query += $" AND LastName='{Lastname}'";
            }

            query += $" OR LastName='{Firstname}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);


            return dt;
        }

        public static string WhoIs(DataRow row)
        {
            string fullname = row["FirstName"].ToString();
            string pronoun = "";
            string faction = Factions.GetFactionByID(Convert.ToInt32(row["FactionID"]));
            string factioninfo = "is a member of";

            if (row["LastName"].ToString() != "")
            {
                fullname = $"{row["FirstName"]} {row["LastName"]}";
            }

            if (faction == "Independent")
            {
                factioninfo = $"is an {faction}";
            }
            switch (row["Gender"].ToString().ToLower())
            {
                case "male":
                    pronoun = "his";
                    break;
                case "female":
                    pronoun = "her";
                    break;
                default:
                    pronoun = "their";
                    break;
            }

            return $"{fullname} {factioninfo}. {pronoun.First().ToString().ToUpper() + pronoun.Substring(1)} profile can be found at {row["URL"]}";

        }

        public static void AddCharacter(CharInfo newCharacter)
        {
            int unitID = Units.GetUnitID(newCharacter.Unit);
            int factionID = Factions.GetFactionID(newCharacter.Faction);

            string query = $"INSERT INTO {charTable}(PlayerID, FirstName, LastName,Gender,UnitID,FactionID,URL,Blurb,CharType,Status) VALUES(@PlayerID, @FirstName, @LastName, @Gender, @UnitID, @FactionID, @URL, @Blurb, @Type, @Status)";

            string[] Parameters = { "@PlayerID", "@FirstName", "@LastName", "@Gender", "@UnitID", "@FactionID", "@URL", "@Blurb", "@Type", "@Status" };

            string[] Values = { Player.GetPlayerID(newCharacter.Player).ToString(), newCharacter.FirstName, newCharacter.LastName, newCharacter.Gender, unitID.ToString(), factionID.ToString(), newCharacter.Url, newCharacter.Blurb, newCharacter.type.ToString(), newCharacter.status.ToString() };

            SqlCommand.ExecuteQuery_Params(query, NineBot.cfgjson, Parameters, Values);

            Units.UpdateUnitStatus(newCharacter.Unit, Units.UnitStatus.Taken, newCharacter.Player);
        }

        public static string PlayerChars(string player)
        {
            if (!player.Contains("<@"))
            {
                player = Player.GetPlayer(player, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
            }
            string query = $"SELECT FirstName, LastName FROM {charTable} WHERE PlayerID={Player.GetPlayerID(player)}";

            string result;

            try
            {
                DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                if (dt.Rows.Count == 0)
                {
                    result = "I'm sorry, there were no records with that name.";
                }
                else
                {
                    result = $"{player} plays the following characters:\n";
                    foreach (DataRow row in dt.Rows)
                    {
                        result += $"\n\t{row["FirstName"]} {row["LastName"]}";
                    }
                }
            }
            catch
            {
                result = "I'm sorry, something went wrong with the query.";
            }

            return result;
        }

        public static string ListChars(CharStatus status)
        {
            string query = $"Select FirstName, LastName, CharType FROM {charTable} where Status = '{status}' ORDER BY LastName Asc";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);
            string response = $"Characters falling under the {status} category are:";

            if (dt.Rows.Count > 0)
            {
                DataRowCollection rows = dt.Rows;

                foreach (DataRow row in rows)
                {
                    response += $"\n{row["FirstName"]} {row["LastName"]} - {row["CharType"]}";
                }
            }
            else
            {
                response = $"There are no characters with the status {status}";
            }

            return response;
        }

        public static string ListChars(CharType type)
        {
            string query = $"Select FirstName, LastName, Status FROM {charTable} where CharType = '{type}' ORDER BY LastName Asc";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);
            string response = $"Characters falling under the {type} category are:";

            if (dt.Rows.Count > 0)
            {
                DataRowCollection rows = dt.Rows;

                foreach (DataRow row in rows)
                {
                    response += $"\n{row["FirstName"]} {row["LastName"]} - {row["Status"]}";
                }
            }
            else
            {
                response = $"There are no characters with the type {type}";
            }

            return response;
        }
        public static string LinkProfile(string FirstName, string LastName)
        {
            string query = $"SELECT URL FROM {charTable} where FirstName='{FirstName}' AND LastName ='{LastName}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                return row["URL"].ToString();
            } else
            {
                return "I did not find anyone with that name in the records.";
            }
        }

        public static async Task<CharInfo> SetFirstName(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What is the character's first name") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = FirstName(info, rsp.Result.Content.ToString());
            }
            else
            {
                await msg.RespondAsync("I see you're busy now. Try again later then.");

                info.Errored = true;
            }

            return info;
        }

        public static CharInfo FirstName(CharInfo info, string msg)
        {
            if (msg.ToLower() == "quit")
            {
                info.Quit = true;
            }
            else
            {

                info.FirstName = msg;
                info.Errored = false;
            }

            return info;
        }

        public static async Task<CharInfo> SetLastName(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What is the character's last name?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = await Task.Run(() => LastName(info, rsp.Result.Content));

                if (info.Errored)
                {
                    await rsp.Result.RespondAsync("That character already exists. Terminating add function.");
                }
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }

            return info;
        }

        public static CharInfo LastName(CharInfo info, string msg)
        {
            if (msg.ToLower() == "quit")
            {
                info.Quit = true;
            }
            else
            {
                if (msg == "\"\"" || msg == "none")
                {
                    info.LastName = "";
                }
                else
                {
                    info.LastName = msg;
                }

                bool exists = CharExists(info.FirstName, info.LastName);

                if (exists)
                {
                    info.Errored = true;
                }
            }

            return info;
        }

        public static async Task<CharInfo> SetGender(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What is the character's gender?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = Gender(info, rsp.Result.Content);
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }

            return info;
        }

        public static CharInfo Gender(CharInfo info, string msg)
        {
            if (msg == "quit")
            {
                info.Quit = true;
            }
            else
            {
                info.Gender = msg;
                info.Errored = false;
            }

            return info;
        }

        public static async Task<CharInfo> SetWeapon(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What is the character's assigned Weapon?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = Unit(info, rsp.Result.Content, info.Player);

                if (info.Errored)
                {
                    await rsp.Result.RespondAsync("That weapon is either not available or not in our records. Terminating command.");
                }
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }

            return info;
        }

        public static CharInfo Unit(CharInfo info, string msg, string rsvd = "")
        {
            if (msg.ToLower() == "quit")
            {
                info.Quit = true;
            }
            else
            {
                if (msg != "none")
                {
                    bool added = Units.UnitAdded(msg);

                    if (!added)
                    {
                        Units.AddUnit(msg, info.Player, Units.UnitStatus.Taken, info.Player);
                    }

                    Units.UnitStatus status = Units.GetStatus(msg);
                    string reservedForMention = Player.GetPlayer(Units.GetReserved(msg), Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);

                    switch (status)
                    {
                        case Units.UnitStatus.Banned:
                            info.Errored = true;
                            break;
                        case Units.UnitStatus.Reserved:
                            if (reservedForMention == rsvd)
                            {
                                //clear reserved
                                //fill assigned to
                                Units.SetAssigneeFromReserved(msg, rsvd);
                                //assign info.unit
                                info.Unit = msg;
                            }
                            else
                            {
                                info.Errored = true;
                            }
                            break;
                        case Units.UnitStatus.Taken:
                            if (reservedForMention == rsvd)
                            {
                                //assign info.unit
                                info.Unit = msg;
                            }
                            else
                            {
                                info.Errored = true;
                            }
                            break;
                        case Units.UnitStatus.Open:
                            info.Unit = msg;
                            break;
                    }
                } else
                {
                    info.Unit = "";
                }
            }

            return info;
        }

        public static CharInfo CharType(CharInfo info, string msg)
        {
            switch (msg.ToLower())
            {
                case "pc":
                    info.type = Nine.CharType.PC;
                    break;
                case "npc":
                    info.type = Nine.CharType.NPC;
                    break;
                case "support":
                case "spc":
                    info.type = Nine.CharType.Support;
                    break;
                default:
                    info.type = Nine.CharType.PC;
                    break;
            }

            return info;
        }

        public static async Task<CharInfo> SetCharaType(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Is the character a PC, Support PC, or an NPC?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = CharType(info, rsp.Result.Content);
            } else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }

            return info;
        }

        public static async Task<CharInfo> SetFaction(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What is the character's faction?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = Faction(info, rsp.Result.Content);

                if (info.Errored)
                {
                    await rsp.Result.RespondAsync("That faction does not exist. Terminating command.");
                }
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }

            return info;
        }

        public static CharInfo Faction(CharInfo info, string msg)
        {
            if (msg.ToLower() == "quit")
            {
                info.Quit = true;
            }
            else
            {
                info.Faction = msg;

                bool exists = Factions.FactionExists(info.Faction);

                if (!exists)
                {
                    info.Errored = true;
                }
            }

            return info;
        }

        public static async Task<CharInfo> SetURL(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What is the URL of the character's profile?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = await URL(info, rsp.Result.Content);

                if (info.Errored)
                {
                    await rsp.Result.RespondAsync("That profile link is in the records or formatted poorly. Must contain http or https and link to Ignition. Terminating command.");
                }
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }

            return info;
        }

        public static async Task<CharInfo> URL(CharInfo info, string msg)
        {
            if (msg.ToLower() == "quit")
            {
                info.Quit = true;
            }
            else
            {
                bool used = false;
                used = await Task.Run(() => URLInUse(msg));

                if (used)
                {
                    info.Errored = true;
                }

                info.Url = msg;

                if ((!info.Url.Contains("http://") && !info.Url.Contains("https://"))
                    || !info.Url.Contains("srwignition.com")
                    || (info.Url.Contains("#post") && info.type == Nine.CharType.PC))
                {
                    info.Errored = true;
                }
            }
            return info;
        }

        public static async Task<CharInfo> SetBlurb(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Is there a description for them you'd like") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                if (rsp.Result.Content.ToLower() != "no")
                {
                    info.Blurb = rsp.Result.Content.ToString();
                }
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }
            return info;
        }

        public static async Task<CharInfo> SetPlayer(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Is this character yours or someone else's?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                if (rsp.Result.Content.ToLower().ToString() == "mine")
                {
                    info.Player = rsp.Result.Author.Mention;

                    if (info.Player.Contains("<@") && !info.Player.Contains("<@!"))
                    {
                        info.Player = info.Player.Replace("<@", "<@!");
                    }
                }
                else
                {
                    if (rsp.Result.Content.Contains("<@") || rsp.Result.Content.Contains("<@!"))
                    {
                        info.Errored = true;
                    }
                    else
                    {
                        info.Player = Player.GetPlayer(rsp.Result.Content, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
                    }
                }
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }
            return info;
        }

        public static async Task<CharInfo> CorrectInfo(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => xm.Content.ToLower().Contains("yes") || xm.Content.ToLower().Contains("no") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                if (rsp.Result.Content.ToLower() == "yes")
                {
                    info.Correct = true;
                } else
                {
                    info.Correct = false;
                }
            }
            else
            {
                info.Errored = true;
                await msg.RespondAsync("I see you're busy now. Try again later then.");
            }

            return info;
        }

        public static async Task<CharInfo> GetCorrectInfo(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What do you need to edit?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                switch (rsp.Result.Content.ToLower())
                {
                    case "player":
                        await msg.RespondAsync("Is this character yours or someone else's? If yours, enter 'mine'. If someone else's please use their monicker. Mention functionality is not enabled for this command and will result in an error.");
                        info = await Characters.SetPlayer(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "first name":
                        await msg.RespondAsync("What is the character's First Name?");
                        info = await SetFirstName(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "last name":
                        await msg.RespondAsync("What is the character's Last Name?");
                        info = await SetLastName(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "gender":
                        await msg.RespondAsync("What is the character's Gender?");
                        info = await SetGender(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "weapon":
                    case "unit":
                        await msg.RespondAsync("What is the character's assigned Weapon? If they are not a pilot, please enter 'none'.");
                        info = await SetWeapon(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "faction":
                        await msg.RespondAsync("What is the character's faction? If they are not in a faction please enter 'Independent'.");
                        info = await SetFaction(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "url":
                        await msg.RespondAsync("What is the URL of the character's profile? For support and non player characters you may link the direct post.");
                        info = await SetURL(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "blurb":
                        await msg.RespondAsync("Is there a description for them you'd like to use when people query them? If yes, enter that blurb. If no, enter no.");
                        info = await SetBlurb(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "quit":
                        info.Relist = false;
                        info.Quit = true;
                        break;
                    case "character type":
                        await msg.RespondAsync("Is the character a PC, Support PC, or an NPC? Please note; Support PCs are player controlled npcs while NPCs are story team controlled.");
                        info = await SetCharaType(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    default:
                        info.Relist = true;
                        break;
                }
            }

            return info;
        }

        public static string EditChar(CharInfo info)
        {
            string msg = "I have successfully updated the following:";

            if (info.Player != info.PrevPlayer)
            {
                string query = $"UPDATE {charTable} SET PlayerID='{info.Player}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                msg += "\nPlayer";
            }

            if (info.FirstName != info.PrevFN)
            {
                string query = $"UPDATE {charTable} SET FirstName='{info.FirstName}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                msg += "\nFirst Name";
            }

            if (info.LastName != info.PrevLN)
            {
                string query = $"UPDATE {charTable} SET LastName='{info.LastName}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                msg += "\nLast Name";
            }

            if (info.Gender != info.PrevGender)
            {
                string query = $"UPDATE {charTable} SET Gender='{info.Gender}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                msg += "\nGender";
            }

            if (info.Unit != info.PrevUnit)
            {
                string query = $"UPDATE {charTable} SET UnitId='{Units.GetUnitID(info.Unit)}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                Units.ClearOnOpen(info.PrevUnit);
                msg += "\nWeapon";
            }

            if (info.Faction != info.PrevFaction)
            {
                string query = $"UPDATE {charTable} SET Faction='{info.Faction}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                msg += "\nFaction";
            }

            if (info.Url != info.PrevUrl)
            {
                string query = $"UPDATE {charTable} SET URL='{info.Url}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                msg += "\nUrl";
            }

            if (info.Blurb != info.PrevBlurb)
            {
                string query = $"UPDATE {charTable} SET Blurb='{info.Blurb}' WHERE ID={info.charID}";

                SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                msg += "\nNotes";
            }

            return msg;
        }

        public static string SetCharType(CharType type, CharInfo info)
        {
            CharType t = info.type;

            string query = $"UPDATE {charTable} SET CharType='{type}' WHERE ID={info.charID}";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return $"Character has been updated from {t} to {type}.";
        }

        public static string SetCharType(CharType type, string FirstName, string LastName)
        {
            string query = $"UPDATE {charTable} SET CharType='{type}' WHERE FirstName='{FirstName}' AND LastName='{LastName}'";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);
            return $"Character has been updated to {type}.";
        }

        public static string SetCharActivity(CharStatus status, string FirstName, string LastName)
        {
            string query = $"UPDATE {charTable} SET Status='{status}' WHERE FirstName='{FirstName}' AND LastName='{LastName}'";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);
            return $"Character has been updated to {status}.";
        }

        public static string SetCharActivity(CharStatus status, CharInfo info)
        {
            CharStatus s = info.status;

            string query = $"UPDATE {charTable} SET Status={status} WHERE ID={info.charID}";
            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            return $"Character has been updated from {s} to {status}";
        }
        #region support
        public static CharInfo NewChar()
        {
            return new CharInfo();
        }

        public static int GetCharID(string CharFirstName, string CharLastName)
        {
            DataTable dt = QueryChar(CharFirstName, CharLastName);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return Convert.ToInt32(row["ID"].ToString());
            }
            else
            {
                return 0;
            }
        }

        public static string GetCharName(int CharID)
        {
            string query = $"SELECT FirstName, LastName FROM {charTable} WHERE ID='{CharID}'";

            DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return $"{row["FirstName"]} {row["LastName"]}";
            }
            else
            {
                return "";
            }
        }

        public static bool CharExists(string CharFirstName, string CharLastName)
        {
            if (QueryChar(CharFirstName, CharLastName).Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static DataTable QueryChar(string CharFirstName, string CharLastName)
        {
            string query = $"SELECT ID FROM {charTable} WHERE FirstName='{CharFirstName}' AND LastName='{CharLastName}'";

            return SqlCommand.ExecuteQuery(query, NineBot.cfgjson);
        }

        public static bool URLInUse(string URL)
        {
            string query = $"SELECT * FROM {charTable} WHERE URL = '{URL}'";

            if (SqlCommand.ExecuteQuery(query, NineBot.cfgjson).Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<CharInfo> GetListCharacters(string player)
        {
            List<CharInfo> chars = new List<CharInfo>();

            if (!player.Contains("<@"))
            {
                player = Player.GetPlayer(player, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
            }

            string query = $"SELECT * FROM {charTable} WHERE PlayerID={Player.GetPlayerID(player)}";

            try
            {
                DataTable dt = SqlCommand.ExecuteQuery(query, NineBot.cfgjson);

                foreach (DataRow r in dt.Rows)
                {
                    CharInfo info = new CharInfo();

                    info.charID = Convert.ToInt32(r["ID"]);
                    info.Player = Player.GetPlayerByID(Convert.ToInt32(r["PlayerID"]));
                    info.PrevPlayer = info.Player;
                    info.FirstName = r["FirstName"].ToString();
                    info.PrevFN = info.FirstName;
                    info.LastName = r["LastName"].ToString();
                    info.PrevLN = info.LastName;
                    info.Gender = r["Gender"].ToString();
                    info.Unit = Units.GetUnitbyID(Convert.ToInt32(r["UnitID"]));
                    info.PrevUnit = info.Unit;
                    info.Faction = Factions.GetFactionByID(Convert.ToInt32(r["FactionID"]));
                    info.PrevFaction = info.Faction;
                    info.Url = r["URL"].ToString();
                    info.PrevUrl = info.Url;
                    info.Blurb = r["Blurb"].ToString();

                    if(info.Blurb == "no")
                    {
                        info.Blurb = "";
                    }

                    info.PrevBlurb = info.Blurb;

                    chars.Add(info);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return chars;
        }

        public static string ListInfo(List<CharInfo> chars)
        {
            string list = "";
            int x = 1;

            foreach(CharInfo chara in chars)
            {
                list += $"\n{x} - {chara.FirstName} {chara.LastName}";
                x++;
            }

            return list;
        }

        public static CharInfo SelectChar(List<CharInfo> chars, string msg)
        {
            try
            {
                int selection = Convert.ToInt32(msg);

                return chars[selection - 1];
            } catch 
            {
                CharInfo info = new CharInfo()
                {
                    Errored = true
                };

                return info;
            }
        }
        #endregion
    }
}
