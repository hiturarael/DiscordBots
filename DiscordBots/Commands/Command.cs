using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBots.Commands
{
    public class Command
    {
        public static string ExecCommand(DSharpPlus.Entities.DiscordMessage message)
        {
            string response = "";

            string[] content = message.Content.Split(" ");
            switch (content[1].ToLower())
            {
                case "ping":
                    response = "Pong!";
                    break;
            }

            return response;
        }

        //public static async Task<string> ExecCommand(string message)
        // {
        //     string response = "";
        //     switch (message.ToLower())
        //     {
        //         case "ping":
        //             response = "Pong!";
        //             break;
        //     }

        //     return response;
        // }
    }
}
