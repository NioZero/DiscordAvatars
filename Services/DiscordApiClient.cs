using DiscordAvatars.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordAvatars.Services
{
    public sealed class DiscordApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly DiscordApiOptions _options;
        private readonly HttpClient _httpClient;

        public DiscordApiClient(DiscordApiOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<IReadOnlyList<DiscordGuild>> GetGuildsAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.BotToken))
            {
                throw new InvalidOperationException("DISCORD_BOT_TOKEN no esta configurado.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.ApiBase}/users/@me/guilds");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bot", _options.BotToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Discord devolvio {response.StatusCode}: {body}");
            }

            var guilds = JsonSerializer.Deserialize<List<DiscordGuild>>(body, JsonOptions);
            return guilds ?? new List<DiscordGuild>();
        }

        public async Task<IReadOnlyList<DiscordUser>> GetGuildMembersAsync(
            string guildId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.BotToken))
            {
                throw new InvalidOperationException("DISCORD_BOT_TOKEN no esta configurado.");
            }

            if (string.IsNullOrWhiteSpace(guildId))
            {
                throw new ArgumentException("Guild id vacio.", nameof(guildId));
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_options.ApiBase}/guilds/{guildId}/members?limit=1000");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bot", _options.BotToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Discord devolvio {response.StatusCode}: {body}");
            }

            var members = JsonSerializer.Deserialize<List<DiscordGuildMember>>(body, JsonOptions);
            if (members == null)
            {
                return Array.Empty<DiscordUser>();
            }

            var users = new List<DiscordUser>(members.Count);
            foreach (var member in members)
            {
                if (member.User == null)
                {
                    continue;
                }

                member.User.Nickname = member.Nick;
                users.Add(member.User);
            }

            return users;
        }
    }
}
