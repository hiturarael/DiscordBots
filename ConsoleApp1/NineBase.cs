using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Nine.Commands;

namespace Nine
{
    public class NineBot
    {
        public static string StrCmd = "9";
        public static string StrBotToken = Environment.GetEnvironmentVariable("NineToken");

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
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
        }
    }
}
