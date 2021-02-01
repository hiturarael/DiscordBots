using System;
using System.Threading.Tasks;
using DSharpPlus;
using DiscordBots.Commands;

namespace DiscordBots
{
    public class BotBase
    {
        public static string StrCmd { get; set; }
        public static string StrBotToken { get; set; }

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
