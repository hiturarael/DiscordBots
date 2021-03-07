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

namespace Nine
{
    public struct CharInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Unit { get; set; }
        public string Faction { get; set; }
        public string Url { get; set; }
        public string Blurb { get; set; }
        public bool Errored { get; set; }
        public bool Quit { get; set; }
        public bool Correct { get; set; }
        public bool Relist { get; set; }
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
                DataTable dt = SqlCommand.ExecuteQuery(query, testing);

                if (dt.Rows.Count == 0)
                {
                    result = "I'm sorry, there were no records with that name.";
                }
                else
                {
                    DataRow row = dt.Rows[0];
                    string player = Player.GetPlayerByID(Convert.ToInt32(row["Player"]));

                    result = $"{player} plays {firstname} {lastname}";
                }
            }
            catch
            {
                result = "I'm sorry, something went wrong with the query.";
            }

            return result;
        }

        public static void AddCharacter(CharInfo newCharacter, string player)
        {
            int playerID = Player.GetPlayerID(player);
            int unitID = Units.GetUnitID(newCharacter.Unit);
            int factionID = Factions.GetFactionID(newCharacter.Faction);

            string query = $"INSERT INTO {charTable}(PlayerID, FirstName, LastName,Gender,UnitID,FactionID,URL,Blurb) VALUES(@PlayerID, @FirstName, @LastName, @Gender, @UnitID, @FactionID, @URL, @Blurb)";

            string[] Parameters = { "@PlayerID", "@FirstName", "@LastName", "@Gender", "@UnitID", "@FactionID", "@URL", "@Blurb" };
            string[] Values = { playerID.ToString(), newCharacter.FirstName, newCharacter.LastName, newCharacter.Gender, unitID.ToString(), factionID.ToString(), newCharacter.Url, newCharacter.Blurb };

            SqlCommand.ExecuteQuery_Params(query, Parameters, Values, testing);
        }

        public static string PlayerChars(string Player)
        {
            return "";
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
                info = await Task.Run(() =>LastName(info, rsp.Result.Content));

                if(info.Errored)
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
                info.LastName = msg;

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
                info = Unit(info, rsp.Result.Content, Player.GetPlayer(msg.Author.Mention, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));

                if(info.Errored)
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
                bool added = Units.UnitAdded(msg);

                if (added)
                {
                    Units.UnitStatus status = Units.GetStatus(msg);

                    switch(status)
                    {
                        case Units.UnitStatus.Banned:
                            info.Errored = true;
                            break;
                        case Units.UnitStatus.Reserved:
                            if(Units.GetReserved(msg) == rsvd)
                            {
                                //clear reserved
                                //fill assigned to
                                Units.SetAssigneeFromReserved(msg, rsvd);                        

                                //assign info.unit
                                info.Unit = msg;
                            } else
                            {
                                info.Errored = true;
                            }
                            break;
                        case Units.UnitStatus.Taken:
                            if (Units.GetAssignee(msg) == rsvd)
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
                }
                else
                {
                    info.Errored = true;                    
                }
            }

            return info;
        }

        public static async Task<CharInfo> SetFaction(DiscordMessage msg, InteractivityExtension interactivity, CharInfo info)
        {
            var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("What is the character's faction?") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

            if (!rsp.TimedOut)
            {
                info = Faction(info, rsp.Result.Content);

                if(info.Errored)
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

                if(info.Errored)
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
                    used = await Task.Run(()=> URLInUse(msg));

                if (used)
                {
                    info.Errored = true;                    
                }

                info.Url = msg;

                if ((!info.Url.Contains("http://")
                    && !info.Url.Contains("https://"))
                    || !info.Url.Contains("srwignition.com")
                    || info.Url.Contains("#post"))
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
                info.Blurb = rsp.Result.Content.ToString();
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
                if(rsp.Result.Content.ToLower() == "yes")
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
                switch(rsp.Result.Content.ToLower())
                {
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
                        await msg.RespondAsync("What is the character's weapon?");
                        info = await SetWeapon(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "faction":
                        await msg.RespondAsync("What is the character's faction? If they are not in a faction please enter 'Independent'.");
                        info = await SetFaction(msg, interactivity, info);
                        info.Relist = false;
                        break;
                    case "url":
                        await msg.RespondAsync("What is the character's profile url?");
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
                    default:
                        info.Relist = true;
                        break;
                }
            }

            return info;
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

            DataTable dt = SqlCommand.ExecuteQuery(query, testing);

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

            return SqlCommand.ExecuteQuery(query, testing);
        }

        public static bool URLInUse(string URL)
        {
            string query = $"SELECT * FROM {charTable} WHERE URL = '{URL}'";

            if (SqlCommand.ExecuteQuery(query, testing).Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
