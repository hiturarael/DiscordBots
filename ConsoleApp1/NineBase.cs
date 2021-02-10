using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DiscordBots.Commands;

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

            discord.MessageCreated += async (e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("9"))
                {
                    Task<string> task = Task.Run(() => Command.ExecCommand(StrCmd, e.Message));
                    task.Wait();

                    await e.Message.RespondAsync(task.Result);
                }
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

    }
}
