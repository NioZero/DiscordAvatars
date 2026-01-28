using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordAvatars.Models;
using DiscordAvatars.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordAvatars.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly DiscordApiOptions _options;
        private readonly DiscordApiClient _apiClient;
        private readonly AppStateStore _stateStore;
        private AppState? _loadedState;
        private bool _isRestoringState;
        private readonly SemaphoreSlim _membersLoadLock = new(1, 1);

        public ObservableCollection<DiscordGuild> Guilds { get; } = new();
        public ObservableCollection<DiscordUser> Members { get; } = new();

        public MemberSlotViewModel Slot1 { get; }
        public MemberSlotViewModel Slot2 { get; }
        public MemberSlotViewModel Slot3 { get; }
        public MemberSlotViewModel Slot4 { get; }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand RefreshMembersCommand { get; }
        public IAsyncRelayCommand UpdateFilesCommand { get; }

        [ObservableProperty]
        private string statusMessage = "Listo.";

        [ObservableProperty]
        private string footerMessage = "Configura DISCORD_BOT_TOKEN (con Server Members Intent) para cargar servidores y usuarios.";

        [ObservableProperty]
        private string selectedFolderPath = string.Empty;

        [ObservableProperty]
        private string folderStatusMessage = string.Empty;

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
            _stateStore = new AppStateStore();

            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => CanRefresh);
            RefreshMembersCommand = new AsyncRelayCommand(RefreshMembersAsync, () => CanRefreshMembers);
            UpdateFilesCommand = new AsyncRelayCommand(UpdateFilesAsync, () => !IsBusy);

            Slot1 = new MemberSlotViewModel(Members, "ms-appx:///Assets/Placeholders/player1.png", "player1.txt", "player1.png");
            Slot2 = new MemberSlotViewModel(Members, "ms-appx:///Assets/Placeholders/player2.png", "player2.txt", "player2.png");
            Slot3 = new MemberSlotViewModel(Members, "ms-appx:///Assets/Placeholders/player3.png", "player3.txt", "player3.png");
            Slot4 = new MemberSlotViewModel(Members, "ms-appx:///Assets/Placeholders/player4.png", "player4.txt", "player4.png");

            UpdateButtonStates();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (!string.IsNullOrWhiteSpace(_options.BotToken))
            {
                _loadedState = _stateStore.Load();
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
            foreach (var guild in guilds.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase))
            {
                Guilds.Add(guild);
            }

            if (Guilds.Count == 0)
            {
                StatusMessage = "El bot no esta en ningun servidor disponible.";
            }

            if (_loadedState != null)
            {
                await ApplySavedStateAsync(_loadedState);
                _loadedState = null;
            }
            else
            {
                if (SelectedGuild == null && Guilds.Count > 0)
                {
                    SelectedGuild = Guilds[0];
                }
                else
                {
                    UpdateButtonStates();
                }
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
            await _membersLoadLock.WaitAsync();
            try
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

            var seen = new HashSet<string>();
            foreach (var member in members.OrderBy(m => m.DisplayName, StringComparer.OrdinalIgnoreCase))
            {
                if (!seen.Add(member.Id))
                {
                    continue;
                }

                Members.Add(member);
            }

            RemoveInvalidSelections();
            }
            finally
            {
                _membersLoadLock.Release();
            }
        }

        private async Task ApplySavedStateAsync(AppState state)
        {
            if (Guilds.Count == 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(state.SelectedFolderPath))
            {
                SetSelectedFolder(state.SelectedFolderPath);
            }

            _isRestoringState = true;
            try
            {
                if (!string.IsNullOrWhiteSpace(state.SelectedGuildId))
                {
                    var target = Guilds.FirstOrDefault(g => g.Id == state.SelectedGuildId);
                    if (target != null)
                    {
                        SelectedGuild = target;
                    }
                }

                if (SelectedGuild == null)
                {
                    SelectedGuild = Guilds[0];
                }
            }
            finally
            {
                _isRestoringState = false;
            }

            await LoadMembersAsync();
            await RestoreSlotSelectionsAsync(state);
        }

        private async Task RestoreSlotSelectionsAsync(AppState state)
        {
            var slots = GetSlots();
            for (var index = 0; index < slots.Length; index++)
            {
                var slot = slots[index];
                var slotState = state.Slots.Count > index ? state.Slots[index] : null;
                slot.IsActive = slotState?.IsActive ?? false;

                if (string.IsNullOrWhiteSpace(slotState?.SelectedUserId))
                {
                    slot.SelectedMember = null;
                    continue;
                }

                var member = Members.FirstOrDefault(m => m.Id == slotState.SelectedUserId);
                if (member == null && SelectedGuild != null)
                {
                    member = await _apiClient.GetGuildMemberAsync(
                        SelectedGuild.Id,
                        slotState.SelectedUserId,
                        CancellationToken.None);

                    if (member != null)
                    {
                        AddMemberSorted(member);
                    }
                }

                slot.SelectedMember = member;
            }
        }

        public void SaveState()
        {
            var state = new AppState
            {
                SelectedGuildId = SelectedGuild?.Id,
                SelectedFolderPath = SelectedFolderPath
            };

            var slots = GetSlots();
            for (var index = 0; index < slots.Length; index++)
            {
                var slot = slots[index];
                if (state.Slots.Count <= index)
                {
                    state.Slots.Add(new SlotState());
                }

                state.Slots[index].IsActive = slot.IsActive;
                state.Slots[index].SelectedUserId = slot.SelectedMember?.Id;
            }

            _stateStore.Save(state);
        }

        public void SetSelectedFolder(string? path)
        {
            SelectedFolderPath = path ?? string.Empty;
            FolderStatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            {
                return;
            }

            if (!TryEnsureWritable(SelectedFolderPath, out var message))
            {
                FolderStatusMessage = message;
            }
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

            if (!_isRestoringState)
            {
                _ = RefreshMembersAsync();
            }
        }

        private void UpdateButtonStates()
        {
            CanRefresh = !IsBusy;
            CanRefreshMembers = !IsBusy && SelectedGuild != null;
            RefreshCommand.NotifyCanExecuteChanged();
            RefreshMembersCommand.NotifyCanExecuteChanged();
            UpdateFilesCommand.NotifyCanExecuteChanged();
        }

        private MemberSlotViewModel[] GetSlots()
        {
            return new[] { Slot1, Slot2, Slot3, Slot4 };
        }

        private void AddMemberSorted(DiscordUser member)
        {
            if (Members.Any(m => m.Id == member.Id))
            {
                return;
            }

            var index = 0;
            for (; index < Members.Count; index++)
            {
                if (string.Compare(Members[index].DisplayName, member.DisplayName, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    break;
                }
            }

            Members.Insert(index, member);
        }

        private async Task UpdateFilesAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            {
                FolderStatusMessage = "Selecciona una carpeta de salida.";
                return;
            }

            if (!TryEnsureWritable(SelectedFolderPath, out var message))
            {
                FolderStatusMessage = message;
                return;
            }

            FolderStatusMessage = string.Empty;
            StatusMessage = "Actualizando archivos...";

            var slots = GetSlots();
            var errors = new List<string>();

            foreach (var slot in slots)
            {
                try
                {
                    await WriteSlotFilesAsync(slot);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            StatusMessage = errors.Count == 0
                ? "Archivos actualizados."
                : $"Errores al escribir archivos: {errors.Count}.";
        }

        private async Task WriteSlotFilesAsync(MemberSlotViewModel slot)
        {
            var textFile = Path.Combine(SelectedFolderPath, slot.TextFileName);
            var imageFile = Path.Combine(SelectedFolderPath, slot.ImageFileName);

            var isActive = slot.IsActive && slot.SelectedMember != null;
            var textContent = isActive ? slot.SelectedMember!.DisplayName : string.Empty;

            Directory.CreateDirectory(SelectedFolderPath);
            await File.WriteAllTextAsync(textFile, textContent);

            var avatarUri = isActive ? slot.SelectedMember!.AvatarUrl : null;
            var sourceUri = string.IsNullOrWhiteSpace(avatarUri) ? slot.GetPlaceholderUri() : avatarUri!;
            await SaveImageAsync(sourceUri, imageFile);
        }

        private static async Task SaveImageAsync(string sourceUri, string destinationPath)
        {
            if (sourceUri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                using var httpClient = new HttpClient();
                var bytes = await httpClient.GetByteArrayAsync(sourceUri);
                await File.WriteAllBytesAsync(destinationPath, bytes);
                return;
            }

            if (sourceUri.StartsWith("ms-appx", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(sourceUri);
                var relativePath = uri.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var localPath = Path.Combine(AppContext.BaseDirectory, relativePath);

                if (!File.Exists(localPath))
                {
                    throw new FileNotFoundException($"No se encontro el placeholder en {localPath}.");
                }

                using var input = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var output = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await input.CopyToAsync(output);
                return;
            }

            throw new InvalidOperationException("Formato de imagen no soportado.");
        }

        private static bool TryEnsureWritable(string path, out string message)
        {
            try
            {
                var testFile = Path.Combine(path, $".discordavatars_write_test_{Guid.NewGuid():N}.tmp");
                using (var stream = new FileStream(testFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.WriteByte(0);
                }

                File.Delete(testFile);
                message = string.Empty;
                return true;
            }
            catch
            {
                message = "No tienes permisos de escritura en la carpeta seleccionada.";
                return false;
            }
        }
    }
}
