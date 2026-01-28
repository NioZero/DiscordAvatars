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

        [JsonPropertyName("permissions")]
        public string? PermissionsRaw { get; set; }

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

        [JsonIgnore]
        public ulong Permissions => ulong.TryParse(PermissionsRaw, out var value) ? value : 0;

        [JsonIgnore]
        public bool HasAdministrator => (Permissions & DiscordPermissionBits.Administrator) != 0;

        [JsonIgnore]
        public bool HasManageGuild => (Permissions & DiscordPermissionBits.ManageGuild) != 0;

        [JsonIgnore]
        public bool HasMemberListAccess => HasAdministrator || HasManageGuild;
    }

    public static class DiscordPermissionBits
    {
        public const ulong Administrator = 0x8;
        public const ulong ManageGuild = 0x20;
    }
}
