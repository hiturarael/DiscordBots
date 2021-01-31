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
            string value = "";
            string[] content = message.Content.Split(" ");
            switch (content[1].ToLower())
            {
                case "ping":
                    response = "Pong!";
                    break;
                case "whoplays":

                    if (content.Length > 2)
                    {
                        for (int x = 2; x < content.Length; x++)
                        {
                            value += $"{content[x]} ";
                        }

                        value = value.Trim();

                        response = Characters.WhoPlays(value);
                    } else
                    {
                        response = "Your request was put in the wrong format. Correct format is 9 WhoPlays <character name>";
                    }
                    break;
                default:
                    response = $"No command exists for {content[1]}";
                    break;
            }

            return response;
        }
    }
}
