using System;
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

using Nine.Commands;

namespace Nine
{
    public class NineBot
    {
        public readonly EventId BotEventId = new EventId(9, "Nine");        
        private readonly static string config = "localconfig.json";

        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public static void Main()
        {
            var nine = new NineBot();
            nine.RunBotAsync().GetAwaiter().GetResult();
        }

        /*static async Task MainAsync()
        {
             var discord = new DiscordClient(new DiscordConfiguration()
             {
                 Token = StrBotToken,
                 TokenType = TokenType.Bot
             });

             discord.UseInteractivity(new InteractivityConfiguration()
             {
                 Timeout = TimeSpan.FromSeconds(30)
             });

             discord.MessageCreated += async (e) =>
             {
                 if (e.Message.Content.ToLower().StartsWith("9"))
                 {
                     //Discord.WebSocket.SocketGuildUser user = e.Message.Author;

                     Task<string> task = Task.Run(() => Commands.Commands.ExecCommand(StrCmd, e.Message.Content, e.Message.Author.ToString()));
                     task.Wait();

                     var msg = await e.Message.RespondAsync(task.Result);
                     //await e.Message.Channel.SendMessageAsync(task.Result);
                     //(e.Message.Content.Contains("You are not up in the roster, has the post order changed?"))//
                     if (task.Result.Contains("You are not up in the roster, has the post order changed?"))
                     {
                         var interactive = discord.GetInteractivityModule();

                         await msg.CreateReactionAsync(DiscordEmoji.FromName(discord, ":white_check_mark:"));
                         await msg.CreateReactionAsync(DiscordEmoji.FromName(discord, ":negative_squared_cross_mark:"));
                         var yes = DiscordEmoji.FromName(discord, ":white_check_mark:");
                         var result = await interactive.WaitForReactionAsync(x => x.Name == yes, e.Message.Author);

                         //WaitForReactionAsync(x => x.Name == ":white_check_mark:" || x.Name == ":negative_squared_cross_mark:", msg, e.Message.Author);

                         //(x => x.Author == e.Message.Author && x.Reactions.Count == 1);
                         //CommandContext ctx = msg;


                         if (result.Emoji == ":white_check_mark:")
                         {
                             List<string> order = new List<string>();

                             await msg.RespondAsync("Code to adjust order pending");
                         } else
                         {
                             await msg.RespondAsync("Please make sure to re-adjust your post as necessary to adhere to the current order then.");
                         }
                     }                    
                 }
             };

             await discord.ConnectAsync();
             await Task.Delay(-1);
         }*/

        public async Task RunBotAsync()
        {
            //load config
            var json = "";
            using (var fs = File.OpenRead(config))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            //load values
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            //instantiate client
            this.Client = new DiscordClient(cfg);

            //hook events
            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;

            //set up commands
            var ccfg = new CommandsNextConfiguration
            {
                //string prefix from config
                StringPrefixes = new[] { cfgjson.CommandPrefix },

                //respond in dm
                EnableDms = true,

                //enable mentioning bot as prefix
                EnableMentionPrefix = true
            };

            //hook commands up
            this.Commands = this.Client.UseCommandsNext(ccfg);

            //hook events
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

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

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            //perm check
            if(e.Exception is ChecksFailedException ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access Denied",
                    Description = $"{emoji} {emoji} Access denied, insufficient permissions {emoji}{emoji}",
                    Color = new DiscordColor(0xFF0000) //red
                };

                await e.Context.RespondAsync(embed);
            }
        }
    }
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("host")]
        public string Host { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        [JsonProperty("database")]
        public string Database { get; private set; }
    }

}
