using System;
using System.Text.Json.Serialization;

namespace DiscordAvatars.Models
{
    public sealed class DiscordTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        public DateTimeOffset ObtainedAtUtc { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset ExpiresAtUtc => ObtainedAtUtc.AddSeconds(ExpiresInSeconds);
    }
}
