using System;
using System.Threading.Tasks;
using DSharpPlus;

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
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");

            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
