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
        private readonly DiscordOAuthOptions _options;
        private readonly HttpClient _httpClient;

        public DiscordApiClient(DiscordOAuthOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<IReadOnlyList<DiscordGuild>> GetGuildsAsync(string accessToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token vacio.", nameof(accessToken));
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.ApiBase}/users/@me/guilds");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Discord devolvio {response.StatusCode}: {body}");
            }

            var guilds = JsonSerializer.Deserialize<List<DiscordGuild>>(body, JsonOptions);
            return guilds ?? new List<DiscordGuild>();
        }
    }
}
