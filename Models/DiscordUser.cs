using System.Text.Json.Serialization;

namespace DiscordAvatars.Models
{
    public sealed class DiscordUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("global_name")]
        public string? GlobalName { get; set; }

        [JsonPropertyName("avatar")]
        public string? AvatarHash { get; set; }

        [JsonIgnore]
        public string? Nickname { get; set; }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Nickname))
                {
                    return Nickname!;
                }

                return !string.IsNullOrWhiteSpace(GlobalName) ? GlobalName! : Username;
            }
        }

        [JsonIgnore]
        public string? AvatarUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AvatarHash) || string.IsNullOrWhiteSpace(Id))
                {
                    return null;
                }

                return $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png";
            }
        }
    }
}
