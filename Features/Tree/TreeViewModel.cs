using ChatBotClient.Core.Models;
using ChatBotClient.Features.Chat.Views;
using ChatBotClient.Infrastructure.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Collections.ObjectModel;

namespace ChatBotClient.Features.Tree
{
    public partial class TreeViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly LocalStorageService _localStorageService;
        private readonly string _userId;

        public ObservableCollection<LeafModel> Leaves { get; } = new();

        public TreeViewModel(NavigationService navigationService, LocalStorageService localStorageService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
            var (userIds, _) = _localStorageService.LoadUserData();
            _userId = userIds?.Count > 0 ? userIds[0] : null;
            Log.Information("TreeViewModel initialized for user {UserId}", _userId);
            LoadLeaves();
        }

        private async void LoadLeaves()
        {
            if (string.IsNullOrEmpty(_userId)) return;
            var diaryEntries = await _localStorageService.GetDiaryEntriesAsync(_userId);
            Leaves.Clear();
            foreach (var entry in diaryEntries)
            {
                Leaves.Add(new LeafModel
                {
                    Id = entry.Date.Ticks.GetHashCode(),
                    Title = entry.Title,
                    Emoji = entry.Emoji,
                    X = GetLeafX(Leaves.Count),
                    Y = GetLeafY(Leaves.Count)
                });
            }
        }

        private double GetLeafX(int index) => 200 + 100 * Math.Cos(index * 0.8);
        private double GetLeafY(int index) => 300 + 60 * Math.Sin(index * 0.8);

        [RelayCommand]
        private void OpenDiaryEntry(LeafModel leaf)
        {
            _navigationService.NavigateToDiaryWithEntry(leaf.Id);
        }

        [RelayCommand]
        private void GoToChat()
        {
            _navigationService.NavigateTo<ChatPage>();
            Log.Information("Navigated to ChatPage");
        }
    }
}