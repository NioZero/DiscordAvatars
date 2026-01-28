using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscordAvatars.Models
{
    public sealed class AppState
    {
        [JsonPropertyName("selectedGuildId")]
        public string? SelectedGuildId { get; set; }

        [JsonPropertyName("selectedFolderPath")]
        public string? SelectedFolderPath { get; set; }

        [JsonPropertyName("slots")]
        public List<SlotState> Slots { get; set; } = new List<SlotState>
        {
            new SlotState(),
            new SlotState(),
            new SlotState(),
            new SlotState()
        };
    }

    public sealed class SlotState
    {
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("selectedUserId")]
        public string? SelectedUserId { get; set; }
    }
}
