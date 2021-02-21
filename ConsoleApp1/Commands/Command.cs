using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Nine.Commands
{
    public class BaseCommand : BaseCommandModule
    {
        [Command("ping")]
        [Description("Ping command, get snark then response time.")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task Ping(CommandContext ctx)
        {
            //triger typing indicator
            await ctx.TriggerTypingAsync();

            var emoji = DiscordEmoji.FromName(ctx.Client, ":expressionless:");

            await ctx.RespondAsync($"{emoji} Do I look like an old Atari to you? Ping Time: {ctx.Client.Ping}ms");
        }

        [Command("PostOrder")]
        [Description("Obtain the current post order for the specified thread.")]
        public async Task PostOrder(CommandContext ctx, [Description("Thread Title or Alias")] params string[] args)
        {
            await ctx.TriggerTypingAsync();

            string thread = string.Join(' ', args);
            string response;

            if (!string.IsNullOrEmpty(thread))
            {
                response = Posts.PostOrder(thread);
            } else
            {
                response = "Invalid command syntax. Correct syntax is 9 postorder <title|alias>";
            }

            await ctx.RespondAsync(response);
        }

        [Command("WhosUp")]
        [Aliases("Next")]
        [Description("Queries the current post order and responds with the next in line.")]
        public async Task WhosUp(CommandContext ctx, [Description("Thread title or Alias")] params string[] args)
        {
            await ctx.TriggerTypingAsync();

            string thread = string.Join(' ', args);
            string response;

            if (!string.IsNullOrEmpty(thread))
            {
                response = Posts.UpNext(thread, false);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 whosup <title|alias>";
            }

            await ctx.RespondAsync(response);
        }

        [Command("Reminder")]
        [Aliases("RemindPost", "PostReminder", "remind")]
        [Description("Queries the current post order and responds with the next in line.")]
        public async Task Reminder(CommandContext ctx, [Description("Thread title or Alias")] params string[] args)
        {
            await ctx.TriggerTypingAsync();

            string thread = string.Join(' ', args);
            string response;

            if (!string.IsNullOrEmpty(thread))
            {
                response = Posts.UpNext(thread, true);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 reminder <title|alias>";
            }

            await ctx.RespondAsync(response);
        }

        [Command("AddThread")]
        [Aliases("AddPost", "NewPost", "NewThread")]
        [Description("Adds a new thread to the database for tracking.")]
        public async Task AddThread(CommandContext ctx, [Description("Thread Title")] string Title, [Description("URL")] string URL, [Description("Alias")] string Alias)
        {
            await ctx.TriggerTypingAsync();
            string response;

            if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(URL) && !string.IsNullOrEmpty(Alias))
            {
                //(content[x].Contains("https://") || content[x].Contains("https://") || content[x].Contains("srwignition.com"))
                if ((URL.Contains("https://") || URL.Contains("http://")) && URL.Contains("srwignition.com"))
                {
                    if (!URL.Contains("#post-"))
                    {
                        response = Posts.AddThread(Title, URL, Alias);
                    } else
                    {
                        response = "The URL cannot be linked to an individual post. Try again meatbag.";
                    }
                } else
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":no:");
                    response = $"{emoji} The URL must be an actual url linking to Ignition.";
                }
            } else
            {
                response = "Invalid command syntax. Correct syntax is 9 reminder \"<title>\" \"<url>\" \"<alias>\"";
            }

            await ctx.RespondAsync(response);
        }

        [Command("UpdateThread")]
        [Aliases("UpdatePost")]
        [Description("Update a thread with the status Open, Complete, Hiatus, or Abandoned")]
        public async Task UpdateThread(CommandContext ctx, [Description("Thread Title or Alias")] string Thread, [Description("Status")] string Status)
        {
            await ctx.TriggerTypingAsync();
            string response;

            if (!string.IsNullOrEmpty(Thread) && !string.IsNullOrEmpty(Status))
            {
                response = Posts.UpdateThread(Thread, Status);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 UpdateThread \"<title|alias>\" \"<Open|Complete|Hiatus|Abandoned>\"";
            }

            await ctx.RespondAsync(response);
        }

        [Command("AddToPostOrder")]
        [Aliases("AddToOrder")]
        [Description("Add a user to the post order at the specified position.")]
        public async Task AddToPostOrder(CommandContext ctx, [Description("Thread Title or Alias")] string Thread, [Description("User added to order By Monicker or @")] string User, [Description("Position")]string Position)
        {
            await ctx.TriggerTypingAsync();
            string response;            

            if (!string.IsNullOrEmpty(Thread) && !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Position))
            {
                string Mask;
                
                if(Position.All(char.IsNumber))
                {
                    if(User.Contains("@!"))
                    {
                        Mask = Player.GetPlayer(User, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);
                    } else
                    {
                        Mask = User;
                        User = Player.GetPlayer(User, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
                    }

                    response = Posts.AddToPostOrder(Thread, User, Position, Mask);
                } else
                {
                    response = "Invalid command syntax. Correct syntax is 9 AddToPostOrder \"<title|alias>\" \"<@Player|Monicker>\" \"<Position>\"\nAnd if you give me a non numeric for a numeric value again I will deploy spider bots to watch you in your sleep.";
                }
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 AddToPostOrder \"<title|alias>\" \"<@Player|Monicker>\" \"<Position>\"";
            }

            await ctx.RespondAsync(response);
        }

        [Command("RemoveFromPostOrder")]
        [Aliases("RemoveFromOrder")]
        [Description("Removes a player from the post list and bumps up all players after them in the order.")]
        public async Task RemoveFromPostOrder(CommandContext ctx, [Description("Thread Title or Alias")] string Thread, [Description("Player mention or monicker")] string Player)
        {
            await ctx.TriggerTypingAsync();
            string response;

            if (!string.IsNullOrEmpty(Thread) && !string.IsNullOrEmpty(Player))
            {
                response = Posts.RemoveFromOrder(Thread, Player);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 RemoveFromPostOrder \"<title|alias>\" \"<Player>\"";
            }

            await ctx.RespondAsync(response);
        }

        [Command("AddPlayer")]
        [Description("Add a player to the database")]
        public async Task AddPlayer(CommandContext ctx, [Description("Player Mention")] string Mention, [Description("Player Nickname")]string Monicker)
        {
            await ctx.TriggerTypingAsync();
            string response;

            if (!string.IsNullOrEmpty(Mention) && !string.IsNullOrEmpty(Monicker))
            {
                response = Player.AddPlayer(Mention, Monicker);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 AddPlayer \"<mention>\" \"<nickname>\"";
            }

            await ctx.RespondAsync(response);
        }

        [Command("WhoPlays")]
        [Description("Alerts user to who plays a character")]
        public async Task WhoPlays(CommandContext ctx, [Description("Character Name")] string Player)
        {
            await ctx.TriggerTypingAsync();
            string response;

            if (!string.IsNullOrEmpty(Player))
            {
                response = Character.WhoPlays(Player);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 WhoPlays \"<character name>\"";
            }

            await ctx.RespondAsync(response);
        }

        [Command("Posted")]
        [Aliases("Post")]
        [Description("Records that you have posted, sets up for next in order.")]
        public async Task Posted(CommandContext ctx, [Description("Thread name or alias")] string Thread)
        {
            var interactivity = ctx.Client.GetInteractivity();

            string[] opts = { "yes", "no" };

            //await ctx.TriggerTypingAsync();
            string response;

            if (!string.IsNullOrEmpty(Thread))
            {
                response = Posts.Posted(Thread, ctx.User.Mention);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 Posted \"<thread>\"";
            }

            //prompt reply and act accordingly.

            await ctx.RespondAsync(response);

            if (response.Contains("You are not up in the roster, has the post order changed?"))
            {
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Content.ToLower().Contains("yes") || xm.Content.ToLower().Contains("no"), TimeSpan.FromSeconds(60));

                if (!msg.TimedOut)
                {
                    //if (msg.Result.Author.ToString() == ctx.User.Mention.ToString())
                    //{
                        int x = 2;
                        List<string> newOrder = new List<string>();

                        int resp = 1;
                        string first = Player.GetPlayer(ctx.User.Mention, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);

                        newOrder.Add(ctx.User.Mention);

                        await ctx.RespondAsync($"{first} will be set to position 1");

                        while (resp != 0)
                        {
                            if (msg.Result.Content.ToString() == "0")
                            {
                                resp = 0;
                            }
                            else
                            {

                                await ctx.RespondAsync($"Who is in position {x}? Enter 0 to stop adding to the list.");

                                msg = await interactivity.WaitForMessageAsync(xm => xm.Content != "", TimeSpan.FromSeconds(60));

                            string player = Player.GetPlayer(msg.Result.Content.ToString(), Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);

                                if (player != "0")
                                {
                                    newOrder.Add(player);
                                }
                                x++;
                            }
                        }

                    await ctx.RespondAsync(Posts.UpdatePostOrder(Thread, newOrder, true));
                    //} 
                }
                else
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":shock:");

                    await ctx.RespondAsync($"{emoji} If you're not going to answer when I ask a question then why are you bothering me?");
                }
            }
        }

        [Command("ResetPostOrder")]
        [Aliases("ResetOrder")]
        [Description("Clears the post order to be re-added")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task ResetPostOrder(CommandContext ctx, [Description("Thread name or alias")] string Thread)
        {
            await ctx.TriggerTypingAsync();
            string response;

            if (!string.IsNullOrEmpty(Thread))
            {
                response = Posts.ResetPostOrder(Thread);
            }
            else
            {
                response = "Invalid command syntax. Correct syntax is 9 Posted \"<thread>\"";
            }

            //prompt reply and act accordingly.

            await ctx.RespondAsync(response);
        }
    }
}
