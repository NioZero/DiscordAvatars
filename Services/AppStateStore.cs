using DiscordAvatars.Models;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DiscordAvatars.Services
{
    public sealed class AppStateStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly string _statePath;

        public AppStateStore()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appPath = Path.Combine(basePath, "DiscordAvatars");
            _statePath = Path.Combine(appPath, "appstate.json");
        }

        public AppState? Load()
        {
            try
            {
                if (!File.Exists(_statePath))
                {
                    return null;
                }

                var json = File.ReadAllText(_statePath, Encoding.UTF8);
                return JsonSerializer.Deserialize<AppState>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public void Save(AppState state)
        {
            var directory = Path.GetDirectoryName(_statePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            Directory.CreateDirectory(directory);
            var json = JsonSerializer.Serialize(state, JsonOptions);
            File.WriteAllText(_statePath, json, Encoding.UTF8);
        }
    }
}
