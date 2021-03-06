﻿using System;
using System.Collections.Generic;
using System.Data;
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
        #region Basic
        [Command("ping")]
        [Description("Ping command, get snark then response time.")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
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

        [Command("Dingus")]
        [Hidden]
        public async Task Dingus(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            if (!ctx.Channel.IsPrivate)
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":smug:");

                await ctx.RespondAsync($"Yes, Yes you are {ctx.User.Mention} {emoji}");
            } else
            {
                await ctx.RespondAsync($"Yes, Yes you are.");
            }
        }

        [Command("Boobs")]
        [Aliases("Boob", "Breast", "Butts","Titties","Ass","Dick", "Bum", "Panties")]
        [Hidden]
        public async Task Lewd(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            if (!ctx.Channel.IsPrivate)
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":lewd:");

                await ctx.RespondAsync($"{emoji}");
            } else
            {
                await ctx.RespondAsync($"Lewd.");
            }
        }

        #endregion

        #region posts
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
        [Aliases("RemindPost", "PostReminder", "Remind")]
        [Description("Queries the current post order and responds with the next in line.")]
        public async Task Reminder(CommandContext ctx, [Description("Thread title or Alias")] string thread)
        {
            await ctx.TriggerTypingAsync();

            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync("Yeah, no. This is not a DM command. Try again in the correct channels.");
                return;
            }
            else
            {
                await ctx.RespondAsync(Posts.UpNext(thread, true));
                await ctx.RespondAsync(ctx.User.Mention);
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
                }
                else
                {
                    response = "The URL cannot be linked to an individual post. Try again meatbag.";
                }
            }
            else
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
        [Aliases("AddToOrder", "AddToThread","AddToPost")]
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
            }
            else
            {

                if (User.Contains("@!"))
                {
                    Mask = Player.GetPlayer(User, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker);
                }
                else
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

        [Command("Posted")]
        [Aliases("Post", "Skip")]
        [Description("Records that you have posted, sets up for next in order.")]
        public async Task Posted(CommandContext ctx, [Description("Thread name or alias")] string Thread)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string response;
            bool skip = false;

            if (ctx.Message.Content.ToLower().Contains("9 skip") || ctx.Message.Content.ToLower().Contains("9skip"))
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
                    }
                    else
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
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
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

        [Command("LinkThread")]
        [Aliases("LinkPost")]
        [Description("Links specified thread to chat")]
        public async Task LinkThread(CommandContext ctx, string Thread)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.linkPost(Thread));
        }

        [Command("ListActiveThreads")]
        [Description("Lists the active threads.")]
        public async Task ListActiveThreads(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.ListThreads(Posts.ThreadStatus.Open));
        }

        [Command("ListCompleteThreads")]
        [Description("Lists the active threads.")]
        public async Task ListCompleteThreads(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.ListThreads(Posts.ThreadStatus.Complete));
        }

        [Command("ListAbandonedThreads")]
        [Description("Lists the active threads.")]
        public async Task ListAbandonedThreads(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.ListThreads(Posts.ThreadStatus.Abandoned));
        }

        [Command("ListHiatusThreads")]
        [Description("Lists the active threads.")]
        public async Task ListHiatusThreads(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Posts.ListThreads(Posts.ThreadStatus.Hiatus));
        }
        #endregion

        #region Player
        [Command("AddPlayer")]
        [Description("Add a player to the database")]
        public async Task AddPlayer(CommandContext ctx, [Description("Player Mention")] string Mention, [Description("Player Nickname")] string Monicker)
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

        [Command("PlayerActive")]
        [Description("Update the player's status to Active")]
        public async Task PlayerActive(CommandContext ctx, [Description("Player to update")]string player)
        {
            await ctx.TriggerTypingAsync();

            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync("Yeah, no. This is not a DM command. Try again in the correct channels.");
                return;
            }
            else
            {
                await ctx.RespondAsync(Player.UpdatePlayerStatus(player, Player.PlayerStatus.Active));
            }
        }


        [Command("PlayerInactive")]
        [Description("Update the player's status to Active")]
        public async Task PlayerInactive(CommandContext ctx, [Description("Player to update")] string player)
        {
            await ctx.TriggerTypingAsync();

            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync("Yeah, no. This is not a DM command. Try again in the correct channels.");
                return;
            }
            else
            {
                await ctx.RespondAsync(Player.UpdatePlayerStatus(player, Player.PlayerStatus.Inactive));
            }
        }

        [Command("UpdateMonicker")]
        [Description("Update the player's status to Active")]
        public async Task UpdateMonicker(CommandContext ctx, [Description("What is the current monicker in the database?")] string oldName, [Description("What are we changing it to?")] string newName)
        {
            await ctx.TriggerTypingAsync();

            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync("Yeah, no. This is not a DM command. Try again in the correct channels.");
                return;
            }
            else
            {
                bool run = true;
                if(Player.GetPlayer(oldName, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention).Replace("<@!","").Replace("<@","").Replace(">","") != ctx.Message.Author.Mention.Replace("<@!", "").Replace("<@", "").Replace(">", ""))
                {
                    if(Player.GetAdmin(ctx.Message.Author.Mention,Player.PlayerSearch.Mention) != 1)
                    {
                        await ctx.RespondAsync("Only an administrator can alter a different player's nickname. You can only edit your own.");
                        run = false;
                    }
                }

                if (run)
                {
                    await ctx.RespondAsync(Player.UpdatePlayerMonicker(oldName, newName));
                }
            }
        }

        [Command("PingAll")]
        [Aliases("Everyone", "Navi")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task PingEveryone(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Player.GetAllPlayersMentions(Player.PlayerStatus.Active));
        }

        [Command("MyThreads")]
        public async Task MyThreads(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Player.GetThreadsImIn(ctx.User.Mention));
        }
        #endregion

        #region Weapons

        [Command("ListUnits")]
        [Description("Lists all units.")]
        public async Task ListAllUnits(CommandContext ctx, [Description("MP (yes/no)")] string MP = "no")
        {
            List<string> units = Units.ListUnits();

            List<string> pgs = new List<string>();
            string tmp = "";
            string tmp2 = "";
            foreach (string unit in units)
            {
                tmp2 += unit;

                if (unit.Length < 2000)
                {
                    tmp = tmp2;
                }
                else
                {
                    pgs.Add(tmp);
                    tmp2 = "";
                    tmp = "";
                }
            }

            if(pgs.Count < 1)
            {
                pgs.Add(tmp);
            }

            if (ctx.Channel.IsPrivate)
            {
                foreach (string page in pgs)
                {
                    await ctx.RespondAsync(page);
                }
            }
            else
            {
                foreach (string page in pgs)
                {
                    await ctx.Member.SendMessageAsync(page);
                }
            }
        }

        [Command("AddUnit")]
        [Aliases("AddWeapon")]
        [Description("Add an open unit to the database")]
        public async Task AddOpenUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Is the unit mass produced? (Yes/No)")] string MassProduced = "No")
        {
            string user = "";

            if (!ctx.User.Mention.Contains("<@!") && ctx.User.Mention.Contains("<@"))
            {
                user = ctx.User.Mention.Replace("<@", "<@!");
            }


            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddUnit(Unit, user, Units.UnitStatus.Open, "", MassProduced));
        }

        [Command("AddBannedUnit")]
        [Aliases("AddBannedWeapon", "AddBanned")]
        [Description("Add a banned unit to the database")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task AddBannedUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Is the unit mass produced? (Yes/No)")] string MassProduced = "No")
        {
            string user = "";

            if (!ctx.User.Mention.Contains("<@!") && ctx.User.Mention.Contains("<@"))
            {
                user = ctx.User.Mention.Replace("<@", "<@!");
            }


            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddUnit(Unit, user, Units.UnitStatus.Banned, "", MassProduced));
        }

        [Command("AddTakenUnit")]
        [Aliases("AddTakenWeapon", "AddTaken", "AddAssigned", "AddAssignedUnit", "AddAssignedWeapon")]
        [Description("Add an assigned unit to the database")]
        public async Task AddTakenUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Assigns the unit to a player, who can later assign the unit to a character.")] string AssignedPlayer)
        {
            await ctx.TriggerTypingAsync();

            string user ="";

            if(!ctx.User.Mention.Contains("<@!") && ctx.User.Mention.Contains("<@"))
            {
                user = ctx.User.Mention.Replace("<@", "<@!");
            }

            await ctx.RespondAsync(Units.AddUnit(Unit, user, Units.UnitStatus.Taken, AssignedPlayer));
        }

        [Command("AddReservedUnit")]
        [Aliases("AddReservedWeapon", "AddReserved")]
        [Description("Add a unit to the database and reserve it for specified player")]
        public async Task AddReservedUnit(CommandContext ctx, [Description("Unit Name")] string Unit, [Description("Monicker of who it is reserved for.")] string ReservedFor)
        {
            string user = "";

            if (!ctx.User.Mention.Contains("<@!") && ctx.User.Mention.Contains("<@"))
            {
                user = ctx.User.Mention.Replace("<@", "<@!");
            }

            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddUnit(Unit, user, Units.UnitStatus.Reserved, ReservedFor));
        }

        [Command("OpenUnit")]
        [Aliases("OpenWeapon", "Unban", "UnReserve")]
        [Description("Alter the status of a unit to 'Open'")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
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
            if (!ReservedFor.Contains("<@!") && ReservedFor.Contains("<@"))
            {
                ReservedFor = ReservedFor.Replace("<@", "<@!");
            } else
            {
                ReservedFor = Player.GetPlayer(ReservedFor, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
            }

            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.UpdateUnitStatus(Unit, Units.UnitStatus.Reserved, ReservedFor));
        }

        [Command("BanUnit")]
        [Aliases("BanWeapon", "Ban")]
        [Description("Alter the status of a unit to 'Banned'")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
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
            if (!Assignee.Contains("<@!") && Assignee.Contains("<@"))
            {
                Assignee = ctx.User.Mention.Replace("<@", "<@!");
            }
            else
            {
                Assignee = Player.GetPlayer(Assignee, Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention);
            }
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.UpdateUnitStatus(Unit, Units.UnitStatus.Taken, Assignee));
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

        [Command("BannedUnits")]
        [Aliases("BannedWeapons","AllBanned")]
        [Description("List the Mobile Weapons and other units currently not available for use in Ignition.")]
        public async Task BannedUnits(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            List<string> units = Units.ListUnits(Units.UnitStatus.Banned);

            List<string> pgs = new List<string>();
            string tmp = "";
            string tmp2 = "";
            foreach(string unit in units)
            {
                tmp2 += unit;

                if(unit.Length < 2000)
                {
                    tmp = tmp2;
                } else
                {
                    pgs.Add(tmp);
                    tmp2 = "";
                    tmp = "";
                }
            }

            if (pgs.Count < 1)
            {
                pgs.Add(tmp);
            }

            await ctx.RespondAsync("Units that fall under that category are:");

            foreach(string page in pgs)
            {
                await ctx.RespondAsync(page);
            }
        }

        [Command("OpenUnits")]
        [Aliases("OpenWeapons","AllOpen","AllAvailable","AvailableUnits","AvailableWeapons")]
        [Description("List the Mobile Weapons and other units currently not available for use in Ignition.")]
        public async Task OpenUnits(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            List<string> units = Units.ListUnits(Units.UnitStatus.Open);

            List<string> pgs = new List<string>();
            string tmp = "";
            string tmp2 = "";
            foreach (string unit in units)
            {
                tmp2 += unit;

                if (unit.Length < 2000)
                {
                    tmp = tmp2;
                }
                else
                {
                    pgs.Add(tmp);
                    tmp2 = "";
                    tmp = "";
                }
            }
            if (pgs.Count < 1)
            {
                pgs.Add(tmp);
            }
            await ctx.RespondAsync("Units that fall under that category are:");

            foreach (string page in pgs)
            {
                await ctx.RespondAsync(page);
            }
        }

        [Command("TakenUnits")]
        [Aliases("TakenWeapons", "AssignedUnits", "AssignedWeapons","AllAssigned")]
        [Description("List the Mobile Weapons and other units currently not available for use in Ignition.")]
        public async Task TakenUnits(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            List<string> units = Units.ListUnits(Units.UnitStatus.Taken);

            List<string> pgs = new List<string>();
            string tmp = "";
            string tmp2 = "";
            foreach (string unit in units)
            {
                tmp2 += unit;

                if (unit.Length < 2000)
                {
                    tmp = tmp2;
                }
                else
                {
                    pgs.Add(tmp);
                    tmp2 = "";
                    tmp = "";
                }
            }
            if (pgs.Count < 1)
            {
                pgs.Add(tmp);
            }
            await ctx.RespondAsync("Units that fall under that category are:");

            foreach (string page in pgs)
            {
                await ctx.RespondAsync(page);
            }
        }

        [Command("ReservedUnits")]
        [Aliases("ReservedWeapons", "AllReserved")]
        [Description("List the Mobile Weapons and other units currently not available for use in Ignition.")]
        public async Task ReservedUnits(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            List<string> units = Units.ListUnits(Units.UnitStatus.Reserved);

            List<string> pgs = new List<string>();
            string tmp = "";
            string tmp2 = "";
            foreach (string unit in units)
            {
                tmp2 += unit;

                if (unit.Length < 2000)
                {
                    tmp = tmp2;
                }
                else
                {
                    pgs.Add(tmp);
                    tmp2 = "";
                    tmp = "";
                }
            }

            if (pgs.Count < 1)
            {
                pgs.Add(tmp);
            }
            await ctx.RespondAsync("Units that fall under that category are:");

            foreach (string page in pgs)
            {
                await ctx.RespondAsync(page);
            }
        }

        [Command("MPUnits")]
        [Aliases("MPWeapons", "AllMP", "MassProducedUnits","MassProducedWeapons","AllMassProduced")]
        [Description("List the Mobile Weapons and other units currently not available for use in Ignition.")]
        public async Task MassProducedUnits(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            List<string> units = Units.ListUnits(Units.UnitStatus.Open, true);

            List<string> pgs = new List<string>();
            string tmp = "";
            string tmp2 = "";
            foreach (string unit in units)
            {
                tmp2 += unit;

                if (unit.Length < 2000)
                {
                    tmp = tmp2;
                }
                else
                {
                    pgs.Add(tmp);
                    tmp2 = "";
                    tmp = "";
                }
            }
            if (pgs.Count < 1)
            {
                pgs.Add(tmp);
            }
            await ctx.RespondAsync("Units that fall under that category are:");

            foreach (string page in pgs)
            {
                await ctx.RespondAsync(page);
            }
        }

        [Command("AddAlias")]
        [Description("Add an alias to a weapon.")]
        public async Task AddAlias(CommandContext ctx, [Description("Unit that will get the alias")]string unitName, [Description("Alias for the unit")]string alias)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddAlias(unitName, alias, Units.AliasType.Alias));
        }

        [Command("AddVariant")]
        [Description("Add a variant alias to a weapon.")]
        public async Task AddVariant(CommandContext ctx, [Description("Unit that will get the alias")] string unitName, [Description("Alias for the unit")] string alias)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddAlias(unitName, alias, Units.AliasType.Variant));
        }

        [Command("AddUpgrade")]
        [Description("Add an upgrade alias to a weapon.")]
        public async Task AddUpgrade(CommandContext ctx, [Description("Unit that will get the alias")] string unitName, [Description("Alias for the unit")] string alias)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.AddAlias(unitName, alias, Units.AliasType.Upgrade));
        }

        [Command("UpdateUnitName")]
        [Aliases("RenameUnit", "RenameWeapon", "UpdateWeaponName", "RenameMech")]
        [Description("Rename a unit.")]
        public async Task RenameUnit(CommandContext ctx, [Description("Unit current name")] string unitName, [Description("New Name")] string newName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Units.EditUnitName(unitName, newName));
        }

        [Command("WhoUses")]
        [Description("Query who uses the requested mech")]
        public async Task WhoUses(CommandContext ctx, [Description("Mech Name")]string mech)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Units.QueryMechOwner(mech));
        }
        #endregion

        #region Dictionary
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

            if (response.Contains("This term is already in the database"))
            {
                await ctx.RespondAsync("Do you wish to add this new definition to the term?");

                var msg = await interactivity.WaitForMessageAsync(xm => xm.Content.ToLower().Contains("yes") || xm.Content.ToLower().Contains("no"), TimeSpan.FromSeconds(60));


                if (!msg.TimedOut)
                {
                    if (msg.Result.Content.ToLower() == "yes")
                    {
                        response = Dictionary.NewTerm(Term, Definition, true);

                        await ctx.RespondAsync(response);
                    }
                    else
                    {
                        await ctx.RespondAsync("Understood, I will not add the term.");
                    }
                }
                else
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":shock:");

                    await ctx.RespondAsync($"{emoji} If you're not going to answer when I ask a question then why are you bothering me?");
                }
            }
        }

        [Command("RemoveTermDefinition")]
        [Aliases("RemoveDefinitionFromDictionary", "DeleteTermDefinition", "DeleteDefinitionFromDictionary")]
        [Description("Deletes a term's definition.")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task DeleteDefinition(CommandContext ctx, [Description("Search Term")] string Term, [Description("Definition Number")] int DefNum)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Dictionary.RemoveTermDefinition(Term, DefNum, false));
        }

        [Command("RemoveTerm")]
        [Aliases("RemoveFromDictionary", "DeleteTerm", "DeleteFromDictionary")]
        [Description("Deletes a term.")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task DeleteDefinition(CommandContext ctx, [Description("Search Term")] string Term)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Dictionary.RemoveTermDefinition(Term, 0, true));
        }

        [Command("UpdateTerm")]
        [Aliases("UpdateDefinition")]
        [Description("Update a specified term number's definition. If left blank, will update first definition.")]
        public async Task UpdateDefinition(CommandContext ctx, [Description("Search Term")] string Term, [Description("Updated definition")] string Definition, [Description("Definition number to update")] int DefNum = 1)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Dictionary.UpdateDefinition(Term, Definition, DefNum));
        }

        #endregion

        #region Characters
        [Command("AddCharacter")]
        [Aliases("AddChar")]
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
            }
            else
            {
                msg = await ctx.Member.SendMessageAsync(openingMsg);
            }

            info = await Characters.SetFirstName(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("What is the character's last name? Enter none if they have no last name.");
            }
            else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            }
            else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetLastName(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("What is the character's gender?");
            }
            else if (info.Quit)
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
                await msg.RespondAsync("Is this character yours or someone else's? If yours, enter 'mine'. If someone else's please use their monicker. Mention functionality is not enabled for this command and will result in an error.");
            }
            else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            }
            else if (info.Errored)
            {
                return;
            }

            info = await Characters.SetPlayer(msg, interactivity, info);

            if(!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("Is the character a PC, Support PC, or an NPC? Please note; Support PCs are player controlled npcs while NPCs are story team controlled.");
            }else if (info.Quit)
            {
                await msg.RespondAsync("Understood. Terminating command.");
                return;
            } else if(info.Errored)
            {
                return;
            }

            info = await Characters.SetCharaType(msg, interactivity, info);

            if (!info.Errored && !info.Quit)
            {
                await msg.RespondAsync("What is the character's assigned Weapon? If they are not a pilot, please enter 'none'.");
            }
            else if (info.Quit)
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
            }
            else if (info.Quit)
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
                await msg.RespondAsync("What is the URL of the character's profile? For support and non player characters you may link the direct post.");
            }
            else if (info.Quit)
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
            }
            else if (info.Quit)
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
                    await msg.RespondAsync($"Does this look correct?\nPlayer:{Player.GetPlayer(info.Player, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker)}\nFirst Name: {info.FirstName}\nLast Name: {info.LastName}\nGender: {info.Gender}\nWeapon: {info.Unit}\nFaction: {info.Faction}\nProfile: {info.Url}\nCharacter Type: {info.type}");
                }
                else if (info.Quit)
                {
                    await msg.RespondAsync("Understood. Terminating command.");
                    return;
                }
                else if (info.Errored)
                {
                    return;
                }

                info = await Characters.CorrectInfo(msg, interactivity, info);

                if (!info.Correct)
                {
                    info = await Characters.GetCorrectInfo(msg, interactivity, info);
                }
                else
                {
                    info.Relist = false;
                }

                while (info.Relist)
                {
                    if (!info.Errored && !info.Quit)
                    {
                        await msg.RespondAsync("What do you need to edit? Select from the list:\nPlayer\nFirst Name\nLast Name\nGender\nWeapon\nFaction\nURL\nBlurb\nCharacter Type");

                        info = await Characters.GetCorrectInfo(msg, interactivity, info);


                    }
                    else if (info.Quit)
                    {
                        await msg.RespondAsync("Understood. Terminating command.");
                        return;
                    }
                    else if (info.Errored)
                    {
                        return;
                    }
                }
            }

            if (!info.Errored && !info.Quit)
            {
                Characters.AddCharacter(info);

                await msg.RespondAsync($"Thank you, {info.FirstName} {info.LastName} has been added to the records.");
            }
            else if (info.Errored)
            {
                await msg.RespondAsync("$An error occured, please try again later.");
                return;
            }
        }

        [Command("PlayedBy")]
        [Description("Lists all characters played by specified player.")]
        public async Task PlayedBy(CommandContext ctx, [Description("Player monicker.")] string player)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Characters.PlayerChars(player));
        }

        [Command("FlagPC")]
        [Description("Sets a specified character to PC Status")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FlagPC(CommandContext ctx, string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.SetCharType(CharType.PC, FirstName, LastName));
        }
        [Command("FlagNPC")]
        [Description("Sets a specified character to PC Status")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FlagNPC(CommandContext ctx, string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.SetCharType(CharType.NPC, FirstName, LastName));
        }
        [Command("FlagSupport")]
        [Description("Sets a specified character to PC Status")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FlagSupport(CommandContext ctx, string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.SetCharType(CharType.Support, FirstName, LastName));
        }

        [Command("FlagActiveChar")]
        [Description("Sets a specified character to PC Status")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FlagActiveChar(CommandContext ctx, string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.SetCharActivity(CharStatus.Active, FirstName, LastName));
        }
        [Command("FlagInactiveChar")]
        [Description("Sets a specified character to PC Status")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FlagInactiveChar(CommandContext ctx, string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.SetCharActivity(CharStatus.Inactive, FirstName, LastName));
        }
        [Command("FlagDeadChar")]
        [Description("Sets a specified character to PC Status")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FlagDeadChar(CommandContext ctx, string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.SetCharActivity(CharStatus.Dead, FirstName, LastName));
        }

        [Command("SetPC")]
        [Description("Sets specified character to PC status")]
        public async Task SetPC(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(ctx.Message.Author.Mention));

            string openMsg = $"Please select the character you would like to set as a PC. \n0 - Quit{Characters.ListInfo(chars)}";

            if(chars.Count > 0)
            {
                while(loop)
                {
                    if (ctx.Channel.IsPrivate)
                    {
                        msg = await ctx.RespondAsync(openMsg);
                    }
                    else
                    {
                        msg = await ctx.Member.SendMessageAsync(openMsg);
                    }

                    var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Please select the character you would like to set as a PC.") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

                    
                    if (!rsp.TimedOut)
                    {
                        if (rsp.Result.Content.ToString() != "0")
                        {
                            info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));
                            Characters.SetCharType(CharType.PC, info);

                            await msg.RespondAsync($"I have set {info.FirstName} {info.LastName} to be a PC.");
                        } else
                        {
                            loop = false;
                            await msg.RespondAsync($"Let me know if you change your mind.");
                        }
                    } else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }
            } else
            {
                await msg.RespondAsync("You have no characters in the database.");
            }
        }

        [Command("SetNPC")]
        [Description("Sets specified character to NPC status")]
        public async Task SetNPC(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(ctx.Message.Author.Mention));

            string openMsg = $"Please select the character you would like to set as an NPC. \n0 - Quit{Characters.ListInfo(chars)}";

            if (chars.Count > 0)
            {
                while (loop)
                {
                    if (ctx.Channel.IsPrivate)
                    {
                        msg = await ctx.RespondAsync(openMsg);
                    }
                    else
                    {
                        msg = await ctx.Member.SendMessageAsync(openMsg);
                    }

                    var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Please select the character you would like to set as a NPC.") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));


                    if (!rsp.TimedOut)
                    {
                        if (rsp.Result.Content.ToString() != "0")
                        {
                            info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));
                            Characters.SetCharType(CharType.NPC, info);

                            await msg.RespondAsync($"I have set {info.FirstName} {info.LastName} to be an NPC.");
                        }
                        else
                        {
                            loop = false;
                            await msg.RespondAsync($"Let me know if you change your mind.");
                        }
                    }
                    else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }
            }
            else
            {
                await msg.RespondAsync("You have no characters in the database.");
            }
        }

        [Command("SetSupport")]
        [Description("Sets specified character to Support status")]
        public async Task SetSupport(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(ctx.Message.Author.Mention));

            string openMsg = $"Please select the character you would like to set as a PC. \n0 - Quit{Characters.ListInfo(chars)}";

            if (chars.Count > 0)
            {
                while (loop)
                {
                    if (ctx.Channel.IsPrivate)
                    {
                        msg = await ctx.RespondAsync(openMsg);
                    }
                    else
                    {
                        msg = await ctx.Member.SendMessageAsync(openMsg);
                    }

                    var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Please select the character you would like to set as a support npc.") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));


                    if (!rsp.TimedOut)
                    {
                        if (rsp.Result.Content.ToString() != "0")
                        {
                            info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));
                            Characters.SetCharType(CharType.Support, info);

                            await msg.RespondAsync($"I have set {info.FirstName} {info.LastName} to be a support npc.");
                        }
                        else
                        {
                            loop = false;
                            await msg.RespondAsync($"Let me know if you change your mind.");
                        }
                    }
                    else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }
            }
            else
            {
                await msg.RespondAsync("You have no characters in the database.");
            }
        }

        [Command("SetActive")]
        [Description("Sets specified character to Active")]
        public async Task SetActive(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(ctx.Message.Author.Mention));

            string openMsg = $"Please select the character you would like to set as active. \n0 - Quit{Characters.ListInfo(chars)}";

            if (chars.Count > 0)
            {
                while (loop)
                {
                    if (ctx.Channel.IsPrivate)
                    {
                        msg = await ctx.RespondAsync(openMsg);
                    }
                    else
                    {
                        msg = await ctx.Member.SendMessageAsync(openMsg);
                    }

                    var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Please select the character you would like to set as active.") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));


                    if (!rsp.TimedOut)
                    {
                        if (rsp.Result.Content.ToString() != "0")
                        {
                            info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));
                            Characters.SetCharActivity(CharStatus.Active, info);

                            await msg.RespondAsync($"I have set {info.FirstName} {info.LastName} to Active.");
                        }
                        else
                        {
                            loop = false;
                            await msg.RespondAsync($"Let me know if you change your mind.");
                        }
                    }
                    else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }
            }
            else
            {
                await msg.RespondAsync("You have no characters in the database.");
            }
        }

        [Command("SetInactive")]
        [Description("Sets specified character to Inactive")]
        public async Task SetInactive(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(ctx.Message.Author.Mention));

            string openMsg = $"Please select the character you would like to set as inactive. \n0 - Quit{Characters.ListInfo(chars)}";

            if (chars.Count > 0)
            {
                while (loop)
                {
                    if (ctx.Channel.IsPrivate)
                    {
                        msg = await ctx.RespondAsync(openMsg);
                    }
                    else
                    {
                        msg = await ctx.Member.SendMessageAsync(openMsg);
                    }

                    var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Please select the character you would like to set as inactive.") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));


                    if (!rsp.TimedOut)
                    {
                        if (rsp.Result.Content.ToString() != "0")
                        {
                            info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));
                            Characters.SetCharActivity(CharStatus.Inactive, info);

                            await msg.RespondAsync($"I have set {info.FirstName} {info.LastName} to Inctive.");
                        }
                        else
                        {
                            loop = false;
                            await msg.RespondAsync($"Let me know if you change your mind.");
                        }
                    }
                    else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }
            }
            else
            {
                await msg.RespondAsync("You have no characters in the database.");
            }
        }

        [Command("SetDead")]
        [Description("Sets specified character to Active")]
        public async Task SetDead(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(ctx.Message.Author.Mention));

            string openMsg = $"Please select the character you would like to set as dead. \n0 - Quit{Characters.ListInfo(chars)}";

            if (chars.Count > 0)
            {
                while (loop)
                {
                    if (ctx.Channel.IsPrivate)
                    {
                        msg = await ctx.RespondAsync(openMsg);
                    }
                    else
                    {
                        msg = await ctx.Member.SendMessageAsync(openMsg);
                    }

                    var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Please select the character you would like to set as dead.") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));


                    if (!rsp.TimedOut)
                    {
                        if (rsp.Result.Content.ToString() != "0")
                        {
                            info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));
                            Characters.SetCharActivity(CharStatus.Dead, info);

                            await msg.RespondAsync($"I have set {info.FirstName} {info.LastName} to Dead. My condolenses, Senpai.");
                        }
                        else
                        {
                            loop = false;
                            await msg.RespondAsync($"Let me know if you change your mind.");
                        }
                    }
                    else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }
            }
            else
            {
                await msg.RespondAsync("You have no characters in the database.");
            }
        }

        [Command("EditCharacter")]
        [Aliases("EditChar")]
        [Description("Allows a player to edit their character information.")]
        public async Task EditChar(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(ctx.Message.Author.Mention));

            string openMsg = $"Here are all the characters registered under your ID, which would you like to update? Type quit at anytime to terminate the command. \nPlease specify with the number corresponding to the character.\n{Characters.ListInfo(chars)}";

            if (chars.Count > 0)
            {
                //list player's characters
                if (ctx.Channel.IsPrivate)
                {
                    msg = await ctx.RespondAsync(openMsg);
                }
                else
                {
                    msg = await ctx.Member.SendMessageAsync(openMsg);
                }

                var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Here are all the characters registered under your ID,") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

                while (loop)
                {
                    if (!rsp.TimedOut)
                    {
                        info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));

                        if (!info.Errored)
                        { 
                            await rsp.Result.RespondAsync($"Here is the current data for {info.FirstName}.\n\nFirst Name: {info.FirstName}\nLast Name: {info.LastName}\nGender: {info.Gender}\nWeapon: {info.Unit}\nFaction: {info.Faction}\nURL: {info.Url}\nNotes: {info.Blurb}");
                            loop = false;
                        }
                        else
                        {
                            if (rsp.Result.Content.ToLower() != "quit")
                            {
                                loop = true;
                                await rsp.Result.RespondAsync(openMsg);
                            } else
                            {
                                loop = false;
                                await rsp.Result.RespondAsync("Understood. Terminating command.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }

                while (!info.Correct)
                {
                    info.Relist = true;
                    while (info.Relist)
                    {
                        //list what youd like to edit
                        await rsp.Result.RespondAsync("What do you need to edit?");

                        //await response
                        //switch statement
                        info = await Characters.GetCorrectInfo(rsp.Result, interactivity, info);

                        //timeout condition
                        if (!info.Errored && !info.Quit)
                        {
                            loop = false;
                        }
                        else if (info.Quit)
                        {
                            loop = false;
                            await rsp.Result.RespondAsync("Understood. Terminating command.");
                            return;
                        }
                        else if (info.Errored)
                        {
                            await rsp.Result.RespondAsync("Looks like something went wrong. Try again or enter 'quit' to terminate command.");
                            info.Errored = false;
                            info.Relist = true;
                        }
                    }

                    if (!info.Errored && !info.Quit)
                    {
                        await msg.RespondAsync($"Does this look correct?\nPlayer:{Player.GetPlayer(info.Player, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker)}\nFirst Name: {info.FirstName}\nLast Name: {info.LastName}\nGender: {info.Gender}\nWeapon: {info.Unit}\nFaction: {info.Faction}\nProfile: {info.Url}");
                    }
                    else if (info.Quit)
                    {
                        await msg.RespondAsync("Understood. Terminating command.");
                        return;
                    }
                    else if (info.Errored)
                    {
                        return;
                    }

                    info = await Characters.CorrectInfo(msg, interactivity, info);
                }

                //execute update
                await rsp.Result.RespondAsync(Characters.EditChar(info));
            } else
            {
                await ctx.Member.SendMessageAsync("You have no characters in the database to edit.");
            }
        }

        [Command("EditOtherCharacter")]
        [Aliases("EditOtherChar")]
        [Description("Allows an admin to edit character information.")]
        [RequireRoles(RoleCheckMode.Any, "Glitter Armament Infinity", "CEO", "Story Mod")]
        public async Task EditChar(CommandContext ctx, [Description("Player to Edit")] string monicker)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<CharInfo> chars = new List<CharInfo>();
            CharInfo info = new CharInfo();
            var msg = ctx.Message;
            bool loop = true;

            chars = await Task.Run(() => Characters.GetListCharacters(Player.GetPlayer(monicker,Player.PlayerSearch.Monicker, Player.PlayerSearch.Mention)));

            string openMsg = $"Here are all the characters registered under their ID, which would you like to update? Type quit at anytime to terminate the command. \nPlease specify with the number corresponding to the character.\n{Characters.ListInfo(chars)}";

            if (chars.Count > 0)
            {
                //list player's characters
                if (ctx.Channel.IsPrivate)
                {
                    msg = await ctx.RespondAsync(openMsg);
                }
                else
                {
                    msg = await ctx.Member.SendMessageAsync(openMsg);
                }

                var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("Here are all the characters registered under your ID,") && xm.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(60));

                while (loop)
                {
                    if (!rsp.TimedOut)
                    {
                        info = await Task.Run(() => Characters.SelectChar(chars, rsp.Result.Content));

                        if (!info.Errored)
                        {
                            await rsp.Result.RespondAsync($"Here is the current data for {info.FirstName}.\n\nFirst Name: {info.FirstName}\nLast Name: {info.LastName}\nGender: {info.Gender}\nWeapon: {info.Unit}\nFaction: {info.Faction}\nURL: {info.Url}\nNotes: {info.Blurb}");
                            loop = false;
                        }
                        else
                        {
                            if (rsp.Result.Content.ToLower() != "quit")
                            {
                                loop = true;
                                await rsp.Result.RespondAsync(openMsg);
                            }
                            else
                            {
                                loop = false;
                                await rsp.Result.RespondAsync("Understood. Terminating command.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        await msg.RespondAsync("I see you're busy now. Try again later then.");
                        loop = false;
                    }
                }

                while (!info.Correct)
                {
                    info.Relist = true;
                    while (info.Relist)
                    {
                        //list what youd like to edit
                        await rsp.Result.RespondAsync("What do you need to edit?");

                        //await response
                        //switch statement
                        info = await Characters.GetCorrectInfo(rsp.Result, interactivity, info);

                        //timeout condition
                        if (!info.Errored && !info.Quit)
                        {
                            loop = false;
                        }
                        else if (info.Quit)
                        {
                            loop = false;
                            await rsp.Result.RespondAsync("Understood. Terminating command.");
                            return;
                        }
                        else if (info.Errored)
                        {
                            await rsp.Result.RespondAsync("Looks like something went wrong. Try again or enter 'quit' to terminate command.");
                            info.Errored = false;
                            info.Relist = true;
                        }
                    }

                    if (!info.Errored && !info.Quit)
                    {
                        await msg.RespondAsync($"Does this look correct?\nPlayer:{Player.GetPlayer(info.Player, Player.PlayerSearch.Mention, Player.PlayerSearch.Monicker)}\nFirst Name: {info.FirstName}\nLast Name: {info.LastName}\nGender: {info.Gender}\nWeapon: {info.Unit}\nFaction: {info.Faction}\nProfile: {info.Url}");
                    }
                    else if (info.Quit)
                    {
                        await msg.RespondAsync("Understood. Terminating command.");
                        return;
                    }
                    else if (info.Errored)
                    {
                        return;
                    }

                    info = await Characters.CorrectInfo(msg, interactivity, info);
                }

                //execute update
                await rsp.Result.RespondAsync(Characters.EditChar(info));
            }
            else
            {
                await ctx.Member.SendMessageAsync("You have no characters in the database to edit.");
            }
        }

        [Command("WhoIs")]
        [Aliases("CharInfo")]
        [Description("Links the character's profile in the chat.")]
        public async Task WhoIs(CommandContext ctx, [Description("Character First Name")] string firstName, [Description("Character Last Name")] string LastName = "")
        {
            await ctx.TriggerTypingAsync();
            var interactivity = ctx.Client.GetInteractivity();

            DataTable dt = await Task.Run(() => Characters.WhoIs(firstName, LastName));           

            if(dt.Rows.Count == 0)
            {
                await ctx.RespondAsync("Sorry, looks like your search came up empty.");
                return;
            } else
            {
                DataRow row = null; 
                
                if (dt.Rows.Count > 1)
                {
                    bool loop = true;

                    while (loop)
                    {
                        string listChars = "There are multiple records with your parameters. Please use the listed number to narrow it down.";
                        List<CharInfo> info = new List<CharInfo>();

                        int x = 1;

                        foreach (DataRow r in dt.Rows)
                        {
                            CharInfo i = new CharInfo();

                            i.FirstName = r["FirstName"].ToString();
                            i.LastName = r["LastName"].ToString();

                            listChars += $"\n{x} - {i.FirstName} {i.LastName}";
                        }
                        await ctx.RespondAsync(listChars);

                        var rsp = await interactivity.WaitForMessageAsync(xm => !xm.Content.Contains("There are multiple records with your parameters.Please use the listed number to narrow it down.") && xm.ChannelId == ctx.Message.ChannelId, TimeSpan.FromSeconds(60));

                        if (!rsp.TimedOut)
                        {
                            CharInfo i = Characters.SelectChar(info, rsp.Result.Content);
                            if (!i.Errored)
                            {
                                loop = false;
                                row = dt.Rows[Convert.ToInt32(rsp.Result.Content)];
                            }
                        }
                        else
                        {
                            await ctx.RespondAsync("I see you're busy. Try again later.");
                            return;
                        }
                    }
                }
                else
                {
                    row = dt.Rows[0];
                }

                await ctx.RespondAsync(Characters.WhoIs(row));
            }

            
        }

        [Command("ListActiveCharacters")]
        [Aliases("ActiveChars", "ActiveCharacters")]
        [Description("Lists the characters with the status active")]
        public async Task ActiveChars(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.ListChars(CharStatus.Active));
        }

        [Command("ListInactiveCharacters")]
        [Aliases("InactiveChars", "InactiveCharacters")]
        [Description("Lists the characters with the status inactive")]
        public async Task InactiveChars(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.ListChars(CharStatus.Inactive));
        }

        [Command("ListDeadCharacters")]
        [Aliases("DeadChars", "DeadCharacters")]
        [Description("Lists the characters with the status inactive")]
        public async Task DeadChars(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.ListChars(CharStatus.Inactive));
        }

        [Command("ListPlayerCharacters")]
        [Aliases("PlayerChars", "PlayerCharacters", "PCs")]
        [Description("Lists all PCs")]
        public async Task PlayerChars(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.ListChars(CharType.PC));
        }

        [Command("ListSupportCharacters")]
        [Aliases("SupportChars", "SupportCharacters", "SupportPCs", "SPCs")]
        [Description("Lists all PCs")]
        public async Task SupportChars(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.ListChars(CharType.Support));
        }

        [Command("ListNonPlayerCharacters")]
        [Aliases("NonPlayerChars", "NonPlayerCharacters", "NPCs")]
        [Description("Lists all PCs")]
        public async Task NonPlayerChars(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.ListChars(CharType.NPC));
        }

        [Command("LinkCharProfile")]
        [Aliases("LinkProfile")]
        [Description("Link a character's profile")]
        public async Task LinkProfile(CommandContext ctx, string FirstName, string LastName)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.LinkProfile(FirstName, LastName));
        }

        [Command("LinkTemplate")]
        [Aliases("LinkNewCharTemplate")]
        [Description("Link a character's profile")]
        public async Task LinkTemplate(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(Characters.LinkProfile("Character", "Template"));
        }
        #endregion

        #region Factions
        [Command("AddFaction")]
        [Description("Record a new faction into the database")]
        public async Task AddFaction(CommandContext ctx, [Description("Name of Faction")] string Faction, [Description("Leader's First Name")] string FirstName, [Description("Leader's Last Name")] string LastName, [Description("Faction Profile URL")] string URL)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.AddFaction(Faction, FirstName, LastName, URL));
        }

        [Command("Faction")]
        [Aliases("FactionInfo")]
        [Description("Retrieve information about the faction specified")]
        public async Task FactionInfo(CommandContext ctx, [Description("Faction Name")]string Faction)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.Faction(Faction));
        }

        [Command("UpdateLeader")]
        [Aliases("UpdateFactionLeader", "NewFactionLeader", "NewLeader")]
        [Description("Change the leader of the specified faction")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FactionLeader(CommandContext ctx, [Description("The faction you wish to update")] string Faction, [Description("Leader's First Name")] string FirstName, [Description("Leader's Last Name")] string LastName)
        {

            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.UpdateFactionLeader(Faction, FirstName, LastName));
        }

        [Command("UpdateFactionName")]
        [Description("Change/Correct a faction's name")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FactionName(CommandContext ctx, [Description("The faction you wish to update")] string Faction, [Description("Faction's Updated/corrected name")] string NewName)
        {

            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.UpdateFactionName(Faction, NewName));
        }

        [Command("UpdateFactionURL")]
        [Description("Change/Correct a faction's url")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task FactionURL(CommandContext ctx, [Description("The faction you wish to update")] string Faction, [Description("Faction's Updated/corrected profile url")] string NewURL)
        {

            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.UpdateFactionURL(Faction, NewURL));
        }

        [Command("ActiveFaction")]
        [Aliases("OpenFaction")]
        [Description("Set the faction to 'Active', signifying it is open for anyone to join.")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task OpenFaction(CommandContext ctx, [Description("Sets the named faction to 'open' status -- Ie anyone can join.")] string Faction)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.UpdateFactionStatus(Faction, Factions.FactionStatus.Active));
        }

        [Command("CloseFaction")]
        [Aliases("ClosedFaction")]
        [Description("Set the faction to 'closed', signifying that the faction is full and/or not taking new members.")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task CloseFaction(CommandContext ctx, [Description("Sets the named faction to 'open' status -- Ie anyone can join.")] string Faction)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.UpdateFactionStatus(Faction, Factions.FactionStatus.Closed));
        }

        [Command("RestrictFaction")]
        [Aliases("RestrictedFaction")]
        [Description("Set the faction to 'restricted', signifying that the faction is only accepting members by selective recruitment.")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task RestrictFaction(CommandContext ctx, [Description("Sets the named faction to 'open' status -- Ie anyone can join.")] string Faction)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.UpdateFactionStatus(Faction, Factions.FactionStatus.Restricted));
        }

        [Command("DefunctFaction")]
        [Aliases("DeadFaction")]
        [Description("Set the faction to 'defunct', signifying that the faction is out of operations. Or use it as mask to make it look dead.")]
        [RequireRoles(RoleCheckMode.Any, "Tech Mod", "CEO", "Story Mod")]
        public async Task DeadFaction(CommandContext ctx, [Description("Sets the named faction to 'open' status -- Ie anyone can join.")] string Faction)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.UpdateFactionStatus(Faction, Factions.FactionStatus.Defunct));
        }

        [Command("ListActiveFactions")]
        [Aliases("ActiveFactions")]
        [Description("Returns a list of factions with the status 'Active'")]
        public async Task OpenFactions(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.ListFactions(Factions.FactionStatus.Active));
        }

        [Command("ListRestrictedFactions")]
        [Aliases("RestrictedFactions")]
        [Description("Returns a list of factions with the status 'Restricted'")]
        public async Task RestrictedFactions(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.ListFactions(Factions.FactionStatus.Restricted));
        }

        [Command("ListClosedFactions")]
        [Aliases("ClosedFactions")]
        [Description("Returns a list of factions with the status 'Closed'")]
        public async Task ClosedFactions(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.ListFactions(Factions.FactionStatus.Closed));
        }

        [Command("ListDefunctFactions")]
        [Aliases("DefunctFactions")]
        [Description("Returns a list of factions with the status 'Closed'")]
        public async Task DefunctFactions(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.ListFactions(Factions.FactionStatus.Closed));
        }

        [Command("ListFactions")]
        [Aliases("Factions")]
        [Description("Returns a list of factions with the status 'Closed'")]
        public async Task FactionsList(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.ListAllFactions());
        }

        [Command("FactionMembers")]
        [Description("Returns a list of members in a specific faction")]
        public async Task FactionMembers(CommandContext ctx, [Description("Faction to query")]string Faction)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync(Factions.ListFactionMembers(Faction));

        }
        #endregion
    }
}
