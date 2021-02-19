using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.WebSocket;

namespace Nine.Commands
{
    public class Commands
    {
        public static string ExecCommand(string strCommand, string message, string author)
        {
            string response;
            string conts = CheckCommandFormat(message);

            string[] content = conts.Split(" ", StringSplitOptions.RemoveEmptyEntries);

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
                    response = AddPost(strCommand, content, author);
                    break;
                case "updatethread":
                case "updatepost":
                    response = UpdatePost(strCommand, content, author);
                    break;
                case "addtopostorder":
                    response = AddToPostOrder(strCommand, content, author);
                    break;

                case "addplayer":
                    response = AddPlayer(content, author);
                    break;

                case "postorder":
                    response = PostOrder(content);
                    break;

                case "removefrompostorder":
                case "removefromorder":
                    response = RemoveFromPostOrder(content, author);
                    break;
                case "resetpostorder":
                    if(IsAdmin(author))
                    {
                        response = ResetPostOrder(content, author);
                    } else
                    {
                        response = "You do not have permissions to execute that command.";
                    }

                    break;
                case "whosup":
                    response = WhosUpReminder(content, author, false);
                    break;
                case "remind":
                case "remindpost":
                    response = WhosUpReminder(content, author, true);
                    break;
                case "posted":
                case "post":
                    response = Posted(content, author);
                    break;
                default:
                    response = $"No command exists for {content[1]}";
                    break;
            }

            return response;
        }

        static string CheckCommandFormat(string message)
        {
            string regex = "9[a-zA-z]";
            string conts;


            if (Regex.IsMatch(message, regex))
            {
                conts = message.Insert(1, " ");
            }
            else
            {
                conts = message;
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

        static string AddPost(string strCommand, string[] content, string author)
        {
            string value = "";
            string response;

            if (content.Length > 4)
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
                    if (!url.Contains("#post-"))
                    {
                        if (urlConsumed)
                        {
                            response = Posts.AddThread(value, url, alias);

                            Logging.LogCommand(content[1], $"{value} {url} {alias}", Player.GetPlayer(author, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));
                        }
                        else
                        {
                            response = "No url was present in your command.";
                        }
                    } else
                    {
                        response = "URL Cannot be linked to a specific post for threads.";
                    }
                } else
                {
                    response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title> <url> <alias>";
                }
            }
            else
            {
                response = "One or more fields were empty. Please try again.";
            }

            return response;
        }

        static string UpdatePost(string strCommand, string[] content, string author)
        {
            string response;

            if (content.Length > 3)
            {
                string searchTerm = "";
                string status = "";

                for (int x = 2; x < content.Length; x++)
                {
                    if (x == content.Length - 1)
                    {
                        status = content[x];
                    } else
                    {
                        searchTerm += $"{content[x]} ";
                    }
                }

                status = status.Trim();
                searchTerm = searchTerm.Trim();

                if (searchTerm != "" && status != "")
                {
                    response = Posts.UpdateThread(searchTerm, status);

                    Logging.LogCommand(content[1], $"{searchTerm} {status}", Player.GetPlayer(author, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));
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

        static string AddToPostOrder(string strCommand, string[] content, string author)
        {
            string response = "";

            if (content.Length > 4)
            {
                bool userIDFound = false;
                string title = "";
                string position = "";
                string user = "";
                string masked = "";

                for (int x = 2; x < content.Length; x++)
                {
                    if (content[x].Contains("@!"))
                    {
                        user = content[x];
                        masked = Player.GetPlayer(user, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);
                        userIDFound = true;
                    } else
                    {
                        if (!userIDFound)
                        {
                            title += $"{content[x]} ";
                        } else
                        {
                            position = content[x];
                        }
                    }
                }

                if (!userIDFound)
                {
                    user = content[^2];
                    position = content[^1];
                    title = "";

                    for (int x = 2; x < content.Length - 2; x++)
                    {
                        title += $"{content[x]} ";
                    }
                    masked = Player.GetPlayer(user, Player.PlayerSearch.Monicker, Player.PlayerSearch.Monicker);

                    if (masked != "")
                    {
                        user = Player.GetPlayer(masked, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
                        userIDFound = true;
                    }
                }

                title = title.Trim();
                user = user.Trim();
                position = position.Trim();

                if (masked != "")
                {
                    if (Player.GetPlayerStatus(masked, Player.PlayerSearch.Monicker))
                    {
                        try
                        {
                            Convert.ToInt32(position);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "Input string was not in a correct format.")
                            {
                                response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title or alias> <@player> <post order position>. And if you add in a non numeric character for position again I will send spiderbots to watch you in your sleep.";
                            }
                            else
                            {
                                throw ex;
                            }
                        }

                        if (!response.Contains("spider") && userIDFound)
                        {
                            response = Posts.AddToPostOrder(title, user, position, masked);
                            Logging.LogCommand(content[1], $"{title} {masked} {position}", Player.GetPlayer(author, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));
                        }
                        else if (!userIDFound)
                        {
                            response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title or alias> <@player> <post order position>";
                        }
                    } else
                    {
                        response = $"{masked} has an Inactive status and cannot be added to the post.";
                    }
                } else
                {
                    response = $"User mentioned not found in the database. Please add them before you add them to posts.";
                }
            }
            else
            {
                response = $"Your request was put in the wrong format. Correct format is {strCommand} {content[1].ToLower()} <thread title or alias> <@player> <post order position>";
            }

            return response;
        }

        static string AddPlayer(string[] content, string author)
        {
            string response;

            if (content.Length > 3)
            {
                string player = content[2];
                string monicker = "";

                if (content[2].Contains("@!"))
                {
                    for (int x = 3; x < content.Length; x++)
                    {
                        monicker = content[x] + " ";
                    }

                    monicker = monicker.Trim();


                    response = Player.AddPlayer(player, monicker);

                    Logging.LogCommand(content[1], $"{monicker}", Player.GetPlayer(author, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));
                } else
                {
                    response = "You must mention the player you are adding for the records.";
                }
            } else
            {
                response = "Invalid argument format, correct format is 9 addplayer @<player> monicker";
            }

            return response;
        }

        static string PostOrder(string[] content)
        {
            string threadID = "";
            string response;

            if(content.Length > 2)
            {
                for(int x = 2; x < content.Length; x++)
                {
                    threadID += $"{content[x]} ";
                }
                
                threadID = threadID.Trim();

                response = Posts.PostOrder(threadID);
            } else
            {
                response = "The command is in the incorrect format. Format should be 9 PostOrder <thread name|alias>";
            }

            return response;
        }

        static string RemoveFromPostOrder(string[] content, string author)
        {
            string response;
            string threadID = "";
            string player = "";

            if (content.Length > 3)
            {
                for(int x = 2; x < content.Length; x++)
                {
                    if(!content[x].Contains("@!"))
                    {
                        threadID += $"{content[x]} ";
                    } else
                    {
                        player += content[x];
                    }
                }

                if(player == "")
                {
                    player = content[^1];

                    player = Player.GetPlayer(player, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);

                    threadID = "";

                    for (int x=2; x< content.Length-1;x++)
                    {
                        threadID += $"{content[x]} ";
                    }
                }

                player = player.Trim();
                threadID = threadID.Trim();

                if (!string.IsNullOrEmpty(threadID) && !string.IsNullOrEmpty(player))
                {
                    response = Posts.RemoveFromOrder(threadID, player);
                    Logging.LogCommand(content[1], $"{Player.GetPlayer(player, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker)}", Player.GetPlayer(author, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));
                } else {
                    response = "The command is in the incorrect format. Format should be 9 RemoveFromPostOrder <thread> player";
                }
            } else
            {
                response = "The command is in the incorrect format. Format should be 9 RemoveFromPostOrder <thread> player";
            }

            return response;
        }

        static string ResetPostOrder(string[] content, string author)
        {
            string threadID = "";
            string response;

            if(content.Length > 2)
            {
                for(int x = 2; x < content.Length; x++)
                {
                    threadID += content[x] + " ";
                }

                threadID = threadID.Trim();

                response = Posts.ResetPostOrder(threadID);
                Logging.LogCommand(content[1], threadID, Player.GetPlayer(author, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));
            } else 
            {
                response = "Incorrect command format, correct format is 9 resetpostorder <thread>.";
            }
            return response;
        }

        static bool IsAdmin(string author)
        {
            int admin = Player.GetAdmin(author, Player.PlayerSearch.Mention);

            if(admin != 1)
            {
                return false;
            } else
            {
                return true;
            }
        }

        static string WhosUpReminder(string[] content, string author, bool ping)
        {
            string response;
            string thread = "";

            if(content.Length > 2)
            {
                for(int x = 2; x < content.Length; x++)
                {
                    thread += $"{content[x]} ";
                }

                thread = thread.Trim();

                response = Posts.UpNext(thread, ping);

                if (ping)
                {
                    Logging.LogCommand(content[1], thread, Player.GetPlayer(author, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker));
                } 
            } else
            {
                response = $"Incorrect command format, correct format is 9 {content[1]} <thread>.";
            }

            return response;
        }

        static string Posted(string[] content, string author)
        {
            string response;

            if(content.Length > 2)
            {
                string threadId = "";

                for(int x = 2; x < content.Length; x++)
                {
                    threadId += $"{content[x]} ";
                }

                threadId = threadId.Trim();

                response = Posts.Posted(threadId, author);
            } else
            {
                response = $"Incorrect command format, correct format is 9 {content[1]} <thread>.";
            }

            return response;
        }

    }
}
