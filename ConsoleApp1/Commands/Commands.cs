using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Nine.Commands
{
    public class Commands
    {
        public static string ExecCommand(string strCommand, DiscordMessage message)
        {
            string response = "";
            string conts = CheckCommandFormat(message);

            string[] content = conts.Split(" ");
            switch (content[1].ToLower())
            {
                case "ping":
                    response = "Do I look like an old Atari to you?";
                    break;
                case "whoplays":
                    response = WhoPlays(strCommand, content);
                    break;
                case "addthread":
                case "addpost":
                    response = AddPost(strCommand, content);
                    break;
                case "updatethread":
                    response = UpdatePost(strCommand, content);
                    break;
                case "addtopostorder":
                    response = AddToPostOrder(strCommand, content);
                    break;
                default:
                    response = $"No command exists for {content[1]}";
                    break;
            }

            return response;
        }

        static string CheckCommandFormat(DiscordMessage message)
        {
            string regex = "9[a-zA-z]";
            string conts;


            if (Regex.IsMatch(message.Content, regex))
            {
                conts = message.Content.Insert(1, " ");
            }
            else
            {
                conts = message.Content;
            }

            return conts;
        }

        static string WhoPlays(string strCommand, string[] content)
        {
            string value = "";
            string response;
            if (content.Length > 2)
            {
                for (int x = 2; x < content.Length; x++)
                {
                    value += $"{content[x]} ";
                }

                value = value.Trim();

                response = Character.WhoPlays(value);
            }
            else
            {
                response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <character name>";
            }

            return response;
        }

        static string AddPost(string strCommand, string[] content)
        {
            string value = "";
            string response;

            if (content.Length > 2)
            {
                string url = "";
                string alias = "";
                bool urlConsumed = false;

                for (int x = 2; x < content.Length; x++)
                {
                    if (content[x].Contains("https://") || content[x].Contains("https://") || content[x].Contains("srwignition.com"))
                    {
                        urlConsumed = true;
                        url = $"{content[x]}";
                    }
                    else
                    {
                        if (!urlConsumed)
                        {
                            value += $"{content[x]} ";
                        }
                        else
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
                }
                else
                {
                    response = "One or more fields were empty. Please try again.";
                }
            }
            else
            {
                response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title> <url> <alias>";
            }

            return response;
        }

        static string UpdatePost(string strCommand, string[] content)
        {
            string response;

            if(content.Length > 2)
            {
                string searchTerm = "";
                string status = "";

                for(int x = 2; x < content.Length; x++)
                {
                    if(x == content.Length -1)
                    {
                        status = content[x];
                    } else
                    {
                        searchTerm += $"{content[x]} ";
                    }
                }

                status = status.Trim();
                searchTerm = searchTerm.Trim();

                if(searchTerm != "" && status != "")
                {
                    response = Posts.UpdateThread(searchTerm, status);
                } else
                {
                    response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title or alias> <thread satus>";
                }
            } else
            {
                response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title or alias> <thread satus>";
            }

            return response;
        }

        static string AddToPostOrder(string strCommand, string[] content)
        {
            string response = "";
            
            if (content.Length > 4)
            {
                bool userIDFound = false;
                string title = "";
                string position = "";
                string user = "";
                for(int x = 2; x < content.Length; x++)
                {
                    if(content[x].Contains("@!"))
                    {
                        user = content[x];
                        userIDFound = true;
                    } else
                    {
                        if(!userIDFound)
                        {
                            title += $"{content[x]} ";
                        } else
                        {                            
                            position = content[x];
                        }
                    }
                }

                title = title.Trim();
                user = user.Trim();
                position = position.Trim();

                try
                {
                    Convert.ToInt32(position);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Input string was not in a correct format.")
                    {
                        response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title or alias> <@player> <post order position>. And if you add in a non numeric character for position again I will send spiderbots to watch you in your sleep.";
                    } else
                    {
                        throw ex;
                    }
                }

                if (!response.Contains("spider"))
                {
                   response =  Posts.AddToPostOrder(title, user, position);
                }

            }
            else
            {
                response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title or alias> <@player> <post order position>";
            }

            return response;
        }
    }
}
