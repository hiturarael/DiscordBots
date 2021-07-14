using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using DiscordBots;
using Nine.Commands;

namespace Nine
{
    public class NineBot
    {
        public readonly EventId BotEventId = new EventId(9, "Nine");
        //public readonly static string config = "localconfig.json";
        public readonly static string config = "liveconfig.json";

        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public InteractivityConfiguration Interactivity { get; set; }

        public static ConfigJson cfgjson { get; set; }

        public static void Main()
        {
            var nine = new NineBot();
            nine.RunBotAsync().GetAwaiter().GetResult();
        }


        public async Task RunBotAsync()
        {

            //load config
            cfgjson = await Config.GetConfig(config);

            var cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.All,
                AlwaysCacheMembers = true                
            };

            //instantiate client
            this.Client = new DiscordClient(cfg);

            //hook events
            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;
            

            //set up interactivity
            this.Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,                

                Timeout = TimeSpan.FromSeconds(10)
            }) ;


            //set up commands
            var ccfg = new CommandsNextConfiguration
            {
                //string prefix from config
                StringPrefixes = new[] { cfgjson.CommandPrefix },

                //respond in dm
                EnableDms = true,

                //enable mentioning bot as prefix
                EnableMentionPrefix = true,    
            };

            //hook commands up
            this.Commands = this.Client.UseCommandsNext(ccfg);
            this.Client.GuildMemberAdded += this.User_Joined;

            //hook events
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

            this.Client.MessageCreated += this.MessageRecieved;

            //register
            this.Commands.RegisterCommands<BaseCommand>();

            //register custom help formatter
            this.Commands.SetHelpFormatter<SimpleHelpFormatter>();

            //connect and log in
            await this.Client.ConnectAsync();

            //prevent premature quitting
            await Task.Delay(-1);
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            //log event occured
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

            //return task complete
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            //log name of guild sent to client
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            return Task.CompletedTask;
        }

        private Task User_Joined(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, $"User Joined: {e.Member}");

            if(e.Member.Username.ToLower().Contains("twitter.com"))
            {
                e.Member.BanAsync(0, "Due to your username and a recent influx of bots matching the pattern you have been instant kick banned, senpai. If this is in error please reach out on our website. https://srwignition.com/index.php");
            } 

            return Task.CompletedTask;
        }

        private async Task MessageRecieved(DiscordClient sender, MessageCreateEventArgs e)
        {
            if(e.Message.Content.ToLower().Contains("nine is broke") || e.Message.Content.ToLower().Contains("9 is broke"))
            {
                await e.Channel.SendMessageAsync($"I am NOT broken, you're broken!");           
            }
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Context.Message.Content.ToLower().Contains("9 is broke"))
            {
                await e.Context.RespondAsync($"I am NOT broken, you're broken!");
            }
            else
            {

                e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                //perm check
                if (e.Exception is ChecksFailedException ex)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Access Denied",
                        Description = $"{emoji} {emoji} Access denied, insufficient permissions {emoji}{emoji}",
                        Color = new DiscordColor(0xFF0000) //red
                    };

                    await e.Context.RespondAsync(embed);
                }

                if (e.Exception is CommandNotFoundException)
                {
                    //await e.Context.RespondAsync("I have no commands programmed with that name. Please use 9 help to get a list of commands.");
                }

                if (e.Exception is ArgumentException)
                {
                    string format = $"9 <{e.Command.Name}";

                    foreach (string alias in e.Command.Aliases)
                    {
                        format = $"{format}|{alias}";
                    }

                    format += ">";

                    IReadOnlyList<CommandOverload> overloads = e.Command.Overloads;

                    foreach (var arg in overloads)
                    {
                        IReadOnlyList<CommandArgument> x = arg.Arguments;
                        foreach (var y in x)
                        {
                            format = $"{format} \"{y.Name}\"";
                        }

                    }
                    await e.Context.RespondAsync($"Your command was entered in an invalid format. The correct format is: {format}");
                }
            }
        }
    }
}
