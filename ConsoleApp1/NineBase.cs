using System;
using System.Collections.Generic;
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
            StrBotToken = "NjY1NjY3Nzg4NTM0NDQ4MTM5.XhpUCg.9T6S7tbEDq-FOxL2YyeTg1ziAd0";
        }

        public static void Main(string[] args)
        {
            Start();
            new BotBase().MainAsync().GetAwaiter().GetResult();
        }

    }
}
