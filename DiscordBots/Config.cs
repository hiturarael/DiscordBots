using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscordBots
{
    public class Config
    {
        public static async Task<ConfigJson> GetConfig(string config)
        {
            var json = "";
            using (var fs = File.OpenRead(config))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("host")]
        public string Host { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        [JsonProperty("database")]
        public string Database { get; private set; }
    }
}
