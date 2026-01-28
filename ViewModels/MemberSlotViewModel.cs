using CommunityToolkit.Mvvm.ComponentModel;
using DiscordAvatars.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace DiscordAvatars.ViewModels
{
    public sealed partial class MemberSlotViewModel : ObservableObject
    {
        private bool _isActive;
        private DiscordUser? _selectedMember;
        private string _displayName = "Sin seleccionar";
        private BitmapImage? _avatarImage;
        private string _searchText = string.Empty;
        private readonly BitmapImage _placeholderImage;
        private readonly string _placeholderUri;

        public MemberSlotViewModel(
            ObservableCollection<DiscordUser> members,
            string placeholderUri,
            string defaultTextFileName,
            string defaultImageFileName)
        {
            Members = members;
            FilteredMembers = new ObservableCollection<DiscordUser>();
            Members.CollectionChanged += OnMembersChanged;
            _placeholderUri = placeholderUri;
            _placeholderImage = new BitmapImage(new Uri(placeholderUri));
            _avatarImage = _placeholderImage;
            TextFileName = defaultTextFileName;
            ImageFileName = defaultImageFileName;
            RefreshFilteredMembers();
        }

        public ObservableCollection<DiscordUser> Members { get; }
        public ObservableCollection<DiscordUser> FilteredMembers { get; }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public DiscordUser? SelectedMember
        {
            get => _selectedMember;
            set
            {
                if (SetProperty(ref _selectedMember, value))
                {
                    UpdateSelectedMember();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    RefreshFilteredMembers();
                }
            }
        }

        public string DisplayName
        {
            get => _displayName;
            private set => SetProperty(ref _displayName, value);
        }

        [ObservableProperty]
        private string textFileName = string.Empty;

        [ObservableProperty]
        private string imageFileName = string.Empty;

        public BitmapImage? AvatarImage
        {
            get => _avatarImage;
            private set => SetProperty(ref _avatarImage, value);
        }

        public void Reset()
        {
            SearchText = string.Empty;
            SelectedMember = null;
        }

        public string GetPlaceholderUri()
        {
            return _placeholderUri;
        }

        private void UpdateSelectedMember()
        {
            DisplayName = SelectedMember?.DisplayName ?? "Sin seleccionar";

            var avatarUrl = SelectedMember?.AvatarUrl;
            AvatarImage = string.IsNullOrWhiteSpace(avatarUrl)
                ? _placeholderImage
                : new BitmapImage(new Uri(avatarUrl));

            if (!string.IsNullOrWhiteSpace(SelectedMember?.DisplayName))
            {
                SearchText = string.Empty;
            }
        }

        private void OnMembersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshFilteredMembers();
        }

        private void RefreshFilteredMembers()
        {
            var query = SearchText?.Trim() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(query)
                ? Members.ToList()
                : Members.Where(m =>
                        (m.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (m.Username?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

            var selected = SelectedMember;
            if (selected != null && filtered.All(m => m.Id != selected.Id))
            {
                filtered.Insert(0, selected);
            }

            FilteredMembers.Clear();
            foreach (var member in filtered)
            {
                FilteredMembers.Add(member);
            }
        }
    }
}
