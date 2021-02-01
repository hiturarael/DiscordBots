using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBots.Commands
{
    public class Command
    {
        public static string ExecCommand(string strCommand, DSharpPlus.Entities.DiscordMessage message)
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
                        response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <character name>";
                    }
                    break;
                case "addthread":
                case "addpost":
                    if(content.Length > 2)
                    {
                        string url = "";
                        string alias = "";
                        bool urlConsumed = false;

                        for(int x=2; x< content.Length; x++)
                        {
                            if(content[x].Contains("https://") || content[x].Contains("https://") || content[x].Contains("srwignition.com"))
                            {
                                urlConsumed = true;
                                url = $"{content[x]}";
                            } else
                            {
                                if (!urlConsumed)
                                {
                                    value += $"{content[x]} ";
                                } else
                                {
                                    alias += $"{content[x]} ";
                                }
                            }
                        }

                        alias = alias.Trim();
                        value = value.Trim();
                        if (url != "" && value != "" && alias != "")
                        {
                            if (urlConsumed)
                            {
                                response = Posts.AddThread(value, url, alias);
                            }
                            else
                            {
                                response = "No url was present in your command.";
                            }
                        } else
                        {
                            response = "One or more fields were empty. Please try again.";
                        }
                    } else
                    {
                        response = $"Your request was put in the wrong format. Correct format is 9 {content[1].ToLower()} <thread title> <url> <alias>";
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
