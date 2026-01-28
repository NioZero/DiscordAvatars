using System.Text.Json.Serialization;

namespace DiscordAvatars.Models
{
    public sealed class DiscordGuild
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string? IconHash { get; set; }

        [JsonIgnore]
        public string? IconUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(IconHash) || string.IsNullOrWhiteSpace(Id))
                {
                    return null;
                }

                return $"https://cdn.discordapp.com/icons/{Id}/{IconHash}.png";
            }
        }
    }
}
