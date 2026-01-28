using System;

namespace DiscordAvatars.Services
{
    public sealed class DiscordApiOptions
    {
        public string? BotToken { get; init; } = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
        public string ApiBase { get; init; } = "https://discord.com/api";
    }
}
