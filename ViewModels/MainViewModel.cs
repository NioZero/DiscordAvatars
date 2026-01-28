using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordAvatars.Models;
using DiscordAvatars.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordAvatars.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly DiscordOAuthOptions _options;
        private readonly DiscordOAuthService _oauthService;
        private readonly DiscordApiClient _apiClient;

        public ObservableCollection<DiscordGuild> Guilds { get; } = new();

        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }

        [ObservableProperty]
        private string statusMessage = "Listo para iniciar sesion.";

        [ObservableProperty]
        private string footerMessage = "Configura DISCORD_CLIENT_ID, DISCORD_REDIRECT_URI (opcional) y DISCORD_CLIENT_SECRET (opcional) en tus variables de entorno.";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isAuthenticated;

        [ObservableProperty]
        private bool canLogin = true;

        [ObservableProperty]
        private bool canRefresh;

        public MainViewModel()
        {
            _options = new DiscordOAuthOptions();
            _oauthService = new DiscordOAuthService(_options);
            _apiClient = new DiscordApiClient(_options);

            LoginCommand = new AsyncRelayCommand(LoginAsync, () => CanLogin);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => CanRefresh);

            UpdateButtonStates();
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId))
            {
                StatusMessage = "Falta DISCORD_CLIENT_ID. Configuralo y reinicia la app.";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Abriendo login de Discord...";

                await _oauthService.LoginAsync(CancellationToken.None);

                IsAuthenticated = _oauthService.HasValidToken;
                StatusMessage = IsAuthenticated ? "Login completado." : "Login incompleto.";

                if (IsAuthenticated)
                {
                    await LoadGuildsAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Login fallo: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshAsync()
        {
            if (!_oauthService.HasValidToken)
            {
                StatusMessage = "Necesitas iniciar sesion primero.";
                IsAuthenticated = false;
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Cargando servidores...";
                await LoadGuildsAsync();
                StatusMessage = $"Servidores cargados: {Guilds.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error cargando servidores: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadGuildsAsync()
        {
            Guilds.Clear();

            var token = _oauthService.CurrentToken?.AccessToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Access token no disponible.");
            }

            var guilds = await _apiClient.GetGuildsAsync(token, CancellationToken.None);
            foreach (var guild in guilds)
            {
                Guilds.Add(guild);
            }
        }

        partial void OnIsBusyChanged(bool value)
        {
            UpdateButtonStates();
        }

        partial void OnIsAuthenticatedChanged(bool value)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            CanLogin = !IsBusy;
            CanRefresh = !IsBusy && IsAuthenticated;
            LoginCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
        }
    }
}
