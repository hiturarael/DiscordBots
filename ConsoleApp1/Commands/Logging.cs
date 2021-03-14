using System;
using System.Collections.Generic;
using System.Text;
using DiscordBots.SQL;

namespace Nine.Commands
{
    public class Logging
    {
        public static readonly bool testing = false;

        public static void LogCommand(string command, string arguments, string player)
        {
            string query = "INSERT INTO commandlog(Command, Arguments, Author) VALUES(@command, @arguments, @player)";
            string[] parameters = { "@command", "@arguments", "@player" };
            string[] values = { command, arguments, player };

            SqlCommand.ExecuteQuery_Params(query, NineBot.cfgjson, parameters, values);
        }
    }
}
