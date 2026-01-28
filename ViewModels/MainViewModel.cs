using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordAvatars.Models;
using DiscordAvatars.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ObservableCollection<DiscordUser> Members { get; } = new();

        public MemberSlotViewModel Slot1 { get; }
        public MemberSlotViewModel Slot2 { get; }
        public MemberSlotViewModel Slot3 { get; }
        public MemberSlotViewModel Slot4 { get; }

        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand RefreshMembersCommand { get; }

        [ObservableProperty]
        private string statusMessage = "Listo para iniciar sesion.";

        [ObservableProperty]
        private string footerMessage = "Configura DISCORD_CLIENT_ID, DISCORD_REDIRECT_URI (opcional), DISCORD_CLIENT_SECRET (opcional) y DISCORD_BOT_TOKEN (con Server Members Intent) para cargar usuarios.";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isAuthenticated;

        [ObservableProperty]
        private bool canLogin = true;

        [ObservableProperty]
        private bool canRefresh;

        [ObservableProperty]
        private bool canRefreshMembers;

        [ObservableProperty]
        private DiscordGuild? selectedGuild;

        public MainViewModel()
        {
            _options = new DiscordOAuthOptions();
            _oauthService = new DiscordOAuthService(_options);
            _apiClient = new DiscordApiClient(_options);

            LoginCommand = new AsyncRelayCommand(LoginAsync, () => CanLogin);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => CanRefresh);
            RefreshMembersCommand = new AsyncRelayCommand(RefreshMembersAsync, () => CanRefreshMembers);

            Slot1 = new MemberSlotViewModel(Members);
            Slot2 = new MemberSlotViewModel(Members);
            Slot3 = new MemberSlotViewModel(Members);
            Slot4 = new MemberSlotViewModel(Members);

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
                StatusMessage = "Actualizando servidores...";
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
            var filtered = guilds.Where(g => g.HasMemberListAccess).ToList();

            foreach (var guild in filtered)
            {
                Guilds.Add(guild);
            }

            if (Guilds.Count == 0 && guilds.Count > 0)
            {
                StatusMessage = "No tienes permisos para listar usuarios en tus servidores.";
            }

            if (SelectedGuild == null && Guilds.Count > 0)
            {
                SelectedGuild = Guilds[0];
            }
            else
            {
                UpdateButtonStates();
            }
        }

        private async Task RefreshMembersAsync()
        {
            if (SelectedGuild == null)
            {
                StatusMessage = "Selecciona un servidor primero.";
                return;
            }

            if (!IsAuthenticated)
            {
                StatusMessage = "Necesitas iniciar sesion primero.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.BotToken))
            {
                StatusMessage = "Falta DISCORD_BOT_TOKEN para leer miembros del servidor.";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Cargando miembros del servidor...";
                await LoadMembersAsync();
                StatusMessage = $"Miembros cargados: {Members.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error cargando miembros: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMembersAsync()
        {
            Members.Clear();

            if (SelectedGuild == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.BotToken))
            {
                return;
            }

            var members = await _apiClient.GetGuildMembersAsync(
                _options.BotToken,
                SelectedGuild.Id,
                CancellationToken.None);

            foreach (var member in members)
            {
                Members.Add(member);
            }

            RemoveInvalidSelections();
        }

        private void RemoveInvalidSelections()
        {
            if (Members.Count == 0)
            {
                Slot1.Reset();
                Slot2.Reset();
                Slot3.Reset();
                Slot4.Reset();
                return;
            }

            if (Slot1.SelectedMember != null && !Members.Any(m => m.Id == Slot1.SelectedMember.Id))
            {
                Slot1.Reset();
            }

            if (Slot2.SelectedMember != null && !Members.Any(m => m.Id == Slot2.SelectedMember.Id))
            {
                Slot2.Reset();
            }

            if (Slot3.SelectedMember != null && !Members.Any(m => m.Id == Slot3.SelectedMember.Id))
            {
                Slot3.Reset();
            }

            if (Slot4.SelectedMember != null && !Members.Any(m => m.Id == Slot4.SelectedMember.Id))
            {
                Slot4.Reset();
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

        partial void OnSelectedGuildChanged(DiscordGuild? value)
        {
            UpdateButtonStates();
            if (value == null)
            {
                Members.Clear();
                Slot1.Reset();
                Slot2.Reset();
                Slot3.Reset();
                Slot4.Reset();
                return;
            }

            _ = RefreshMembersAsync();
        }

        private void UpdateButtonStates()
        {
            CanLogin = !IsBusy;
            CanRefresh = !IsBusy && IsAuthenticated;
            CanRefreshMembers = !IsBusy && SelectedGuild != null && IsAuthenticated && !string.IsNullOrWhiteSpace(_options.BotToken);
            LoginCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
            RefreshMembersCommand.NotifyCanExecuteChanged();
        }
    }
}
