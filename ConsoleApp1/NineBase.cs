using System;
using System.Collections;
using System.Text;
using DiscordBots;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Nine
{
    public class NineBot : BotBase
    {
        private static void Start()
        {
            StrCmd = "9";
            StrBotToken = Environment.GetEnvironmentVariable("NineToken");
        }

        public static new void Main()
        {
            Start();
            new BotBase().MainAsync().GetAwaiter().GetResult();
        }

    }
}
