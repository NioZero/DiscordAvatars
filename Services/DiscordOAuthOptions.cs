using System;

namespace DiscordAvatars.Services
{
    public sealed class DiscordOAuthOptions
    {
        public string ClientId { get; init; } = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID") ?? string.Empty;
        public string? ClientSecret { get; init; } = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET");
        public string? BotToken { get; init; } = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
        public string[] Scopes { get; init; } = new[] { "identify", "guilds" };
        public string ApiBase { get; init; } = "https://discord.com/api";
        public string RedirectUri { get; init; } = Environment.GetEnvironmentVariable("DISCORD_REDIRECT_URI")
            ?? "http://127.0.0.1:53682/callback/";
        public TimeSpan LoginTimeout { get; init; } = TimeSpan.FromMinutes(2);
    }
}
