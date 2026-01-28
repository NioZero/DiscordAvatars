using System.Text.Json.Serialization;

namespace DiscordAvatars.Models
{
    public sealed class DiscordGuildMember
    {
        [JsonPropertyName("user")]
        public DiscordUser? User { get; set; }

        [JsonPropertyName("nick")]
        public string? Nick { get; set; }
    }
}
