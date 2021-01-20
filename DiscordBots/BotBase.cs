using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBots
{
    public class BotBase
    {
        public static string StrCmd { get; set; }
        public static string StrBotToken { get; set; }
        private DiscordSocketClient _client;

        public static void Main()
    => new BotBase().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var _config = new DiscordSocketConfig
            {
                MessageCacheSize = 100
            };

            _client = new DiscordSocketClient(_config);

            await _client.LoginAsync(TokenType.Bot, StrBotToken);
            await _client.StartAsync();

            _client.MessageUpdated += MessageUpdated;

            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected");

                return Task.CompletedTask;
            };

            await Task.Delay(-1);
        }

        private async Task MessageUpdated( Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
