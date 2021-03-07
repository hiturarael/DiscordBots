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

            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync("Yeah, no. This is not a DM command. Try again in the correct channels.");
                return;
            }
            else
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":expressionless:");

                await ctx.RespondAsync($"{emoji} Do I look like an old Atari to you? Ping Time: {ctx.Client.Ping}ms");
            }
        }

        [Command("PostOrder")]
        [Description("Obtain the current post order for the specified thread.")]
        public async Task PostOrder(CommandContext ctx, [Description("Thread Title or Alias")] string thread)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.PostOrder(thread));
        }

        [Command("WhosUp")]
        [Aliases("Next")]
        [Description("Queries the current post order and responds with the next in line.")]
        public async Task WhosUp(CommandContext ctx, [Description("Thread title or Alias")] string thread)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.UpNext(thread, false));
        }

        [Command("Reminder")]
        [Aliases("RemindPost", "PostReminder", "remind")]
        [Description("Queries the current post order and responds with the next in line.")]
        public async Task Reminder(CommandContext ctx, [Description("Thread title or Alias")] string thread)
        {
            await ctx.TriggerTypingAsync();

            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync("Yeah, no. This is not a DM command. Try again in the correct channels.");
                return;
            } else
            {
                await ctx.RespondAsync(Posts.UpNext(thread, true));
            }
        }

        [Command("AddThread")]
        [Aliases("AddPost", "NewPost", "NewThread")]
        [Description("Adds a new thread to the database for tracking.")]
        public async Task AddThread(CommandContext ctx, [Description("Thread Title")] string Title, [Description("URL")] string URL, [Description("Alias")] string Alias)
        {
            await ctx.TriggerTypingAsync();
            string response;

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

            await ctx.RespondAsync(response);
        }

        [Command("UpdateThread")]
        [Aliases("UpdatePost")]
        [Description("Update a thread with the status Open, Complete, Hiatus, or Abandoned")]
        public async Task UpdateThread(CommandContext ctx, [Description("Thread Title or Alias")] string Thread, [Description("Status")] string Status)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.UpdateThread(Thread, Status));
        }

        [Command("AddToPostOrder")]
        [Aliases("AddToOrder")]
        [Description("Add a user to the post order at the specified position.")]
        public async Task AddToPostOrder(CommandContext ctx, [Description("Thread Title or Alias")] string Thread, [Description("User added to order By Monicker or @")] string User)
        {
            await ctx.TriggerTypingAsync();
            string response;            
                
            string Mask;

            if (ctx.Channel.IsPrivate && User.Contains("@!"))
            {
                await ctx.RespondAsync("Yeah, no. This is not a DM command when you use a mention. Try again in the correct channels.");
                return;
            } else { 

            if (User.Contains("@!"))
            {
                Mask = Player.GetPlayer(User, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);
            } else
            {
                Mask = User;
                User = Player.GetPlayer(User, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
            }

            response = Posts.AddToPostOrder(Thread, User, Mask);
            }

            await ctx.RespondAsync(response);
        }

        [Command("RemoveFromPostOrder")]
        [Aliases("RemoveFromOrder")]
        [Description("Removes a player from the post list and bumps up all players after them in the order.")]
        public async Task RemoveFromPostOrder(CommandContext ctx, [Description("Thread Title or Alias")] string Thread, [Description("Player mention or monicker")] string Player)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.RemoveFromOrder(Thread, Player));
        }

        [Command("AddPlayer")]
        [Description("Add a player to the database")]
        public async Task AddPlayer(CommandContext ctx, [Description("Player Mention")] string Mention, [Description("Player Nickname")]string Monicker)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Player.AddPlayer(Mention, Monicker));
        }

        [Command("WhoPlays")]
        [Description("Alerts user to who plays a character")]
        public async Task WhoPlays(CommandContext ctx, [Description("Character Name")] string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Characters.WhoPlays(FirstName, LastName));
        }

        [Command("Posted")]
        [Aliases("Post", "Skip")]
        [Description("Records that you have posted, sets up for next in order.")]
        public async Task Posted(CommandContext ctx, [Description("Thread name or alias")] string Thread)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string response;
            bool skip = false;

            if(ctx.Message.Content.ToLower().Contains("9 skip") || ctx.Message.Content.ToLower().Contains("9skip"))
            {
                skip = true;
            }

            response = Posts.Posted(Thread, ctx.User.Mention, skip);            

            //prompt reply and act accordingly.

            await ctx.RespondAsync(response);

            if (response.Contains("You are not up in the roster, has the post order changed?"))
            {
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Content.ToLower().Contains("yes") || xm.Content.ToLower().Contains("no"), TimeSpan.FromSeconds(60));

                if (!msg.TimedOut)
                {
                    //if (msg.Result.Author.ToString() == ctx.User.Mention.ToString())
                    //{
                    if (msg.Result.Content.ToLower() == "yes")
                    {
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
                    } else
                    {
                        await ctx.RespondAsync("Understood. The list will not be changed.");
                    }
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

            await ctx.RespondAsync(Posts.ResetPostOrder(Thread));
        }

        [Command("ThreadComplete")]
        [Aliases("Complete", "Completed", "FinishedThread")]
        [Description("Mark's a thread as complete, purges the post order, posts, and cooldowns.")]
        public async Task CompleteThread(CommandContext ctx, [Description("Thread name or alias")] string Thread)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.ThreadComplete(Thread));
        }

        [Command("AddUnit")]
        [Aliases("AddWeapon")]
        [Description("Add an open unit to the database")]
        public async Task AddOpenUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Is the unit mass produced? (Yes/No)")] string MassProduced = "No")
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddUnit(Unit, Player.GetPlayer(ctx.User.Mention,Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker), Units.UnitStatus.Open,"",MassProduced));
        }

        [Command("AddBannedUnit")]
        [Aliases("AddBannedWeapon", "AddBanned")]
        [Description("Add a banned unit to the database")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task AddBannedUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Is the unit mass produced? (Yes/No)")] string MassProduced = "No")
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddUnit(Unit, Player.GetPlayer(ctx.User.Mention, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker), Units.UnitStatus.Banned,"", MassProduced));
        }

        [Command("AddTakenUnit")]
        [Aliases("AddTakenWeapon", "AddTaken")]
        [Description("Add an assigned unit to the database")]
        public async Task AddTakenUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Assigns the unit to a player, who can later assign the unit to a character.")]string AssignedPlayer)
        {
            await ctx.TriggerTypingAsync();

            string user = ctx.User.Mention.Replace("<@!", "").Replace("<@", "").Replace(">", "");

            await ctx.RespondAsync(Units.AddUnit(Unit, Player.GetPlayer(user, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker), Units.UnitStatus.Taken,AssignedPlayer));
        }

        [Command("AddReservedUnit")]
        [Aliases("AddReservedWeapon", "AddReserved")]
        [Description("Add a unit to the database and reserve it for specified player")]
        public async Task AddReservedUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Monicker of who it is reserved for.")] string ReservedFor)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddUnit(Unit, Player.GetPlayer(ctx.User.Mention, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker), Units.UnitStatus.Reserved, ReservedFor));
        }

        [Command("OpenUnit")]
        [Aliases("OpenWeapon", "Unban", "UnReserve")]
        [Description("Alter the status of a unit to 'Open'")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task OpenUnit(CommandContext ctx, [Description("Unit Name")] string Unit)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.UpdateUnitStatus(Unit, Units.UnitStatus.Open));
        }

        [Command("ReserveUnit")]
        [Aliases("ReserveWeapon", "Reserve")]
        [Description("Alter the status of a unit to 'Reserved' and who it is reserved for.")]
        public async Task ReserveUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Monicker of who it is reserved for.")] string ReservedFor)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.UpdateUnitStatus(Unit, Units.UnitStatus.Reserved, ReservedFor));
        }

        [Command("BanUnit")]
        [Aliases("BanWeapon", "Ban")]
        [Description("Alter the status of a unit to 'Banned'")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task BanUnit(CommandContext ctx, [Description("Unit Name")] string Unit)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.UpdateUnitStatus(Unit, Units.UnitStatus.Banned));
        }

        [Command("AssignUnit")]
        [Aliases("AssignWeapon", "Assign", "TakeUnit", "TakeWeapon", "Take")]
        [Description("Alter the status of a unit to 'Taken'")]
        public async Task AssignUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Which player is assigned the unit?")] string Assignee)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.UpdateUnitStatus(Unit, Units.UnitStatus.Taken,Assignee));
        }

        [Command("FlagMassProduced")]
        [Aliases("MP", "MassProduced")]
        [Description("Adds a notation in the records that the wepaon is mass produced and unable to be reserved or assigned.")]
        public async Task MassProduced(CommandContext ctx, [Description("Unit Name")] string Unit)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.ToggleMassProduced(Unit, "Yes"));
        }

        [Command("UnFlagMassProduced")]
        [Aliases("NotMP", "NotMassProduced")]
        [Description("Adds a notation in the records that the wepaon is not mass produced and able to be reserved or assigned.")]
        public async Task NotMassProduced(CommandContext ctx, [Description("Unit Name")] string Unit)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.ToggleMassProduced(Unit, "No"));
        }

        [Command("Define")]
        [Aliases("Whatis")]
        [Description("Queries our dictionary for the definition(s) of a term.")]
        public async Task Define(CommandContext ctx, [Description("Search Term")] string Term)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Dictionary.Definition(Term));
        }

        [Command("AddTerm")]
        [Aliases("AddToDictionary")]
        [Description("Add a term to the dictionary")]
        public async Task TermSearch(CommandContext ctx, [Description("Search Term")] string Term, [Description("Definition of the term")] string Definition)
        {
            var interactivity = ctx.Client.GetInteractivity();

            await ctx.TriggerTypingAsync();

            string response = Dictionary.NewTerm(Term, Definition);

            await ctx.RespondAsync(response);

            if(response.Contains("This term is already in the database"))
            {
                await ctx.RespondAsync("Do you wish to add this new definition to the term?");

                var msg = await interactivity.WaitForMessageAsync(xm => xm.Content.ToLower().Contains("yes") || xm.Content.ToLower().Contains("no"), TimeSpan.FromSeconds(60));

                 
                if (!msg.TimedOut)
                {
                    if (msg.Result.Content.ToLower() == "yes")
                    {
                        response = Dictionary.NewTerm(Term, Definition, true);

                        await ctx.RespondAsync(response);
                    } else
                    {
                        await ctx.RespondAsync("Understood, I will not add the term.");
                    }
                } else
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":shock:");

                    await ctx.RespondAsync($"{emoji} If you're not going to answer when I ask a question then why are you bothering me?");
                }
            }
        }

        [Command("RemoveTermDefinition")]
        [Aliases("RemoveDefinitionFromDictionary", "DeleteTermDefinition", "DeleteDefinitionFromDictionary")]
        [Description("Deletes a term's definition.")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task DeleteDefinition(CommandContext ctx, [Description("Search Term")] string Term, [Description("Definition Number")] int DefNum)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Dictionary.RemoveTermDefinition(Term,DefNum, false));
        }

        [Command("RemoveTerm")]
        [Aliases("RemoveFromDictionary", "DeleteTerm", "DeleteFromDictionary")]
        [Description("Deletes a term.")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task DeleteDefinition(CommandContext ctx, [Description("Search Term")] string Term)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Dictionary.RemoveTermDefinition(Term, 0, true));
        }

        [Command("UpdateTerm")]
        [Aliases("UpdateDefinition")]
        [Description("Update a specified term number's definition. If left blank, will update first definition.")]
        public async Task UpdateDefinition(CommandContext ctx, [Description("Search Term")] string Term, [Description("Updated definition")]string Definition, [Description("Definition number to update")] int DefNum = 1)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Dictionary.UpdateDefinition(Term, Definition, DefNum));
        }

        [Command("AddCharacter")]
        [Description("DM the executing player to obtain information to add a new character to the records.")]
        public async Task AddCharacter(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;

            string openingMsg = "To add a character to my records I need a bit more information. If you need to stop entry at any time, I'll terminate the questionaire after 60 seconds idle or if you type 'quit'.\n\nWhat is the character's first name?";

            info.Errored = false;
            info.Quit = false;
            info.Correct = false;
            info.Relist = true;

            if (ctx.Channel.IsPrivate)
            {
                msg = await ctx.RespondAsync(openingMsg);
            } else
            {
                 msg = await ctx.Member.SendMessageAsync(openingMsg);
            }

            info = await Characters.SetFirstName(msg, interactivity, info);

            if(!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("What is the character's last name?");
            } else if(info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            } else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetLastName(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("What is the character's gender?");               
            } else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            }
            else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetGender(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("What is the character's assigned Weapon?");
            } else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            }
            else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetWeapon(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
               await msg.RespondAsync("What is the character's faction? If they are not in a faction please enter 'Independent'.");
            } else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            }
            else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetFaction(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("What is the URL of the character's profile?");
            } else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            }
            else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetURL(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("Is there a description for them you'd like to use when people query them? If yes, enter that blurb. If no, enter no.");
            } else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            }
            else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetBlurb(msg, interactivity, info);

            while (!info.Correct)
            {
                if (!info.Errored && !info.Quit)
                {
                    await msg.RespondAsync($"Does this look correct?\nFirst Name: {info.FirstName}\nLast Name: {info.LastName}\nGender: {info.Gender}\nWeapon: {info.Unit}\nFaction: {info.Faction}\nProfile: {info.Url}");
                }
                else if (info.Quit)
                {
                    await msg.RespondAsync("Understood. Terminating command.");
                    return;
                } else if (info.Errored)
                { 
                    return;
                }

                info = await Characters.CorrectInfo(msg, interactivity, info);

                if(!info.Correct)
                {
                    info = await Characters.GetCorrectInfo(msg, interactivity, info);
                } else
                {
                    info.Relist = false;
                }

                while (info.Relist)
                {
                    if (!info.Errored && !info.Quit)
                    {
                            await msg.RespondAsync("What do you need to edit? Select from the list:\nFirst Name\nLast Name\nGender\nWeapon\nFaction\nURL\nBlurb");

                            info = await Characters.GetCorrectInfo(msg, interactivity, info);
                    
           
                    } else if(info.Quit)
                    {
                        await msg.RespondAsync("Understood. Terminating command.");
                        return;
                    }
                    else if(info.Errored)
                    {
                        return;
                    }
                }
            }

            if(!info.Errored && !info.Quit)
            {
                Characters.AddCharacter(info, ctx.User.Mention);

                await msg.RespondAsync($"Thank you, {info.FirstName} {info.LastName} has been added to the records.");
            }
            else if (info.Errored)
            {
                await msg.RespondAsync("$An error occured, please try again later.");
                return;
            }
        }
    }
}
