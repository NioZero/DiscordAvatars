using DiscordAvatars.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace DiscordAvatars.Services
{
    public sealed class DiscordOAuthService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly DiscordOAuthOptions _options;
        private readonly HttpClient _httpClient;

        public DiscordOAuthService(DiscordOAuthOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient ?? new HttpClient();
        }

        public DiscordTokenResponse? CurrentToken { get; private set; }

        public bool HasValidToken =>
            CurrentToken != null &&
            !string.IsNullOrWhiteSpace(CurrentToken.AccessToken) &&
            CurrentToken.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1);

        public async Task<DiscordTokenResponse> LoginAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId))
            {
                throw new InvalidOperationException("DISCORD_CLIENT_ID no esta configurado.");
            }

            var (redirectUri, listener) = CreateLoopbackListener(_options.RedirectUri);
            var state = Guid.NewGuid().ToString("N");
            var codeVerifier = CreateCodeVerifier();
            var codeChallenge = CreateCodeChallenge(codeVerifier);
            var scope = string.Join(' ', _options.Scopes);

            var authorizeUrl =
                $"{_options.ApiBase}/oauth2/authorize" +
                $"?response_type=code" +
                $"&client_id={Uri.EscapeDataString(_options.ClientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&scope={Uri.EscapeDataString(scope)}" +
                $"&state={Uri.EscapeDataString(state)}" +
                $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                $"&code_challenge_method=S256";

            listener.Start();
            await Launcher.LaunchUriAsync(new Uri(authorizeUrl));

            DiscordTokenResponse tokenResponse;
            try
            {
                var authCode = await WaitForCodeAsync(listener, state, _options.LoginTimeout, cancellationToken);
                tokenResponse = await ExchangeCodeForTokenAsync(authCode, codeVerifier, redirectUri, cancellationToken);
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }

            tokenResponse.ObtainedAtUtc = DateTimeOffset.UtcNow;
            CurrentToken = tokenResponse;
            return tokenResponse;
        }

        private async Task<string> WaitForCodeAsync(
            HttpListener listener,
            string expectedState,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var contextTask = listener.GetContextAsync();
            var delayTask = Task.Delay(timeout, cancellationToken);
            var completed = await Task.WhenAny(contextTask, delayTask);

            if (completed != contextTask)
            {
                throw new TimeoutException("Tiempo de espera agotado para el login de Discord.");
            }

            var context = await contextTask;
            var request = context.Request;
            var code = request.QueryString["code"];
            var state = request.QueryString["state"];

            await WriteBrowserResponseAsync(context.Response, code, state, expectedState, cancellationToken);

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Discord no devolvio un codigo de autorizacion.");
            }

            if (!string.Equals(state, expectedState, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("El parametro state no coincide.");
            }

            return code;
        }

        private async Task WriteBrowserResponseAsync(
            HttpListenerResponse response,
            string? code,
            string? state,
            string expectedState,
            CancellationToken cancellationToken)
        {
            var isOk = !string.IsNullOrWhiteSpace(code) && string.Equals(state, expectedState, StringComparison.Ordinal);
            var html = isOk
                ? "<html><body><h2>Login completado.</h2><p>Puedes cerrar esta ventana.</p></body></html>"
                : "<html><body><h2>Login fallido.</h2><p>Vuelve a intentar desde la app.</p></body></html>";

            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            response.OutputStream.Close();
        }

        private async Task<DiscordTokenResponse> ExchangeCodeForTokenAsync(
            string authorizationCode,
            string codeVerifier,
            string redirectUri,
            CancellationToken cancellationToken)
        {
            var form = new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["grant_type"] = "authorization_code",
                ["code"] = authorizationCode,
                ["redirect_uri"] = redirectUri,
                ["code_verifier"] = codeVerifier
            };

            if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
            {
                form["client_secret"] = _options.ClientSecret!;
            }

            using var content = new FormUrlEncodedContent(form);
            using var response = await _httpClient.PostAsync($"{_options.ApiBase}/oauth2/token", content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Discord devolvio {response.StatusCode}: {body}");
            }

            var token = JsonSerializer.Deserialize<DiscordTokenResponse>(body, JsonOptions);
            if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException("No se pudo leer el access_token.");
            }

            return token;
        }

        private static (string RedirectUri, HttpListener Listener) CreateLoopbackListener(string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(redirectUri))
            {
                throw new InvalidOperationException("DISCORD_REDIRECT_URI no esta configurado.");
            }

            var uri = new Uri(redirectUri);
            if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La redirect URI debe usar http:// para loopback.");
            }

            var prefix = $"{uri.Scheme}://{uri.Host}:{uri.Port}/";
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            return (redirectUri, listener);
        }

        private static string CreateCodeVerifier()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Base64UrlEncode(bytes);
        }

        private static string CreateCodeChallenge(string verifier)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.ASCII.GetBytes(verifier));
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
    }
}
