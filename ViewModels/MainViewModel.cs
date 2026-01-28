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
        private readonly DiscordApiOptions _options;
        private readonly DiscordApiClient _apiClient;

        public ObservableCollection<DiscordGuild> Guilds { get; } = new();
        public ObservableCollection<DiscordUser> Members { get; } = new();

        public MemberSlotViewModel Slot1 { get; }
        public MemberSlotViewModel Slot2 { get; }
        public MemberSlotViewModel Slot3 { get; }
        public MemberSlotViewModel Slot4 { get; }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand RefreshMembersCommand { get; }

        [ObservableProperty]
        private string statusMessage = "Listo.";

        [ObservableProperty]
        private string footerMessage = "Configura DISCORD_BOT_TOKEN (con Server Members Intent) para cargar servidores y usuarios.";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool canRefresh;

        [ObservableProperty]
        private bool canRefreshMembers;

        [ObservableProperty]
        private DiscordGuild? selectedGuild;

        public MainViewModel()
        {
            _options = new DiscordApiOptions();
            _apiClient = new DiscordApiClient(_options);

            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => CanRefresh);
            RefreshMembersCommand = new AsyncRelayCommand(RefreshMembersAsync, () => CanRefreshMembers);

            Slot1 = new MemberSlotViewModel(Members);
            Slot2 = new MemberSlotViewModel(Members);
            Slot3 = new MemberSlotViewModel(Members);
            Slot4 = new MemberSlotViewModel(Members);

            UpdateButtonStates();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (!string.IsNullOrWhiteSpace(_options.BotToken))
            {
                await RefreshAsync();
                return;
            }

            StatusMessage = "Falta DISCORD_BOT_TOKEN. Configuralo y reinicia la app.";
        }

        private async Task RefreshAsync()
        {
            if (string.IsNullOrWhiteSpace(_options.BotToken))
            {
                StatusMessage = "Falta DISCORD_BOT_TOKEN para cargar servidores.";
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

            var guilds = await _apiClient.GetGuildsAsync(CancellationToken.None);
            foreach (var guild in guilds)
            {
                Guilds.Add(guild);
            }

            if (Guilds.Count == 0)
            {
                StatusMessage = "El bot no esta en ningun servidor disponible.";
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
            CanRefresh = !IsBusy;
            CanRefreshMembers = !IsBusy && SelectedGuild != null;
            RefreshCommand.NotifyCanExecuteChanged();
            RefreshMembersCommand.NotifyCanExecuteChanged();
        }
    }
}
