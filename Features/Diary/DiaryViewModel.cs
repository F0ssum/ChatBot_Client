using ChatBotClient.Core.Models;
using ChatBotClient.Features.Chat.Views;
using ChatBotClient.Features.Main;
using ChatBotClient.Features.Services;
using ChatBotClient.Infrastructure.Services;
using ChatBotClient.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ChatBotClient.Features.Diary
{
	public partial class DiaryViewModel : ObservableObject
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly LocalStorageService _localStorageService;
		private readonly NavigationService _navigationService;
		private readonly string _userId;
		private string _newNoteContent;
		private string _quickNoteContent;
		private string _newTag;
		private string _searchQuery;
		private int _sortMode;
		private bool _createNoteModalVisibility;
		private bool _quickNoteModalVisibility;
		private bool _archiveNotesModalVisibility;
		private bool _isEntriesTabSelected = true;
		private bool _isSettingsTabSelected;
		private bool _isTriggersTabSelected;
		private string _selectedEmoji;
		private PlotModel _moodChartModel;
		private ObservableCollection<string> _triggers;

		[ObservableProperty]
		private ObservableCollection<DiaryEntry> _entries = new ObservableCollection<DiaryEntry>();

		[ObservableProperty]
		private ObservableCollection<string> _tags = new ObservableCollection<string>();

		public string NewNoteContent
		{
			get => _newNoteContent;
			set => SetProperty(ref _newNoteContent, value);
		}

		public string QuickNoteContent
		{
			get => _quickNoteContent;
			set => SetProperty(ref _quickNoteContent, value);
		}

		public string NewTag
		{
			get => _newTag;
			set => SetProperty(ref _newTag, value);
		}

		public string SearchQuery
		{
			get => _searchQuery;
			set
			{
				SetProperty(ref _searchQuery, value);
				FilterEntries();
			}
		}

		public int SortMode
		{
			get => _sortMode;
			set
			{
				SetProperty(ref _sortMode, value);
				SortEntries();
			}
		}

		public bool CreateNoteModalVisibility
		{
			get => _createNoteModalVisibility;
			set => SetProperty(ref _createNoteModalVisibility, value);
		}

		public bool QuickNoteModalVisibility
		{
			get => _quickNoteModalVisibility;
			set => SetProperty(ref _quickNoteModalVisibility, value);
		}

		public bool ArchiveNotesModalVisibility
		{
			get => _archiveNotesModalVisibility;
			set => SetProperty(ref _archiveNotesModalVisibility, value);
		}

		public bool IsEntriesTabSelected
		{
			get => _isEntriesTabSelected;
			set
			{
				SetProperty(ref _isEntriesTabSelected, value);
				if (value)
				{
					IsSettingsTabSelected = false;
					IsTriggersTabSelected = false;
				}
			}
		}

		public bool IsSettingsTabSelected
		{
			get => _isSettingsTabSelected;
			set
			{
				SetProperty(ref _isSettingsTabSelected, value);
				if (value)
				{
					IsEntriesTabSelected = false;
					IsTriggersTabSelected = false;
				}
			}
		}

		public bool IsTriggersTabSelected
		{
			get => _isTriggersTabSelected;
			set
			{
				SetProperty(ref _isTriggersTabSelected, value);
				if (value)
				{
					IsEntriesTabSelected = false;
					IsSettingsTabSelected = false;
					LoadTriggersAsync();
				}
			}
		}

		public string SelectedEmoji
		{
			get => _selectedEmoji;
			set => SetProperty(ref _selectedEmoji, value);
		}

		public PlotModel MoodChartModel
		{
			get => _moodChartModel;
			set => SetProperty(ref _moodChartModel, value);
		}

		public ObservableCollection<string> Triggers
		{
			get => _triggers;
			set => SetProperty(ref _triggers, value);
		}

		public DiaryViewModel(LocalStorageService localStorageService,NavigationService navigationService, IServiceProvider serviceProvider)
		{
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			var (userIds, _) = _localStorageService.LoadUserData();
			_userId = userIds?.Count > 0 ? userIds[0] : null;
			Triggers = new ObservableCollection<string>();
			InitializeAsync();
			Log.Information("DiaryViewModel initialized for user {UserId}", _userId);
		}

		[RelayCommand]
		private async Task InitializeAsync()
		{
			if (string.IsNullOrEmpty(_userId))
			{
				Log.Error("User ID is null");
				MessageBox.Show("Please log in to access the diary.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				var mainViewModel = _serviceProvider.GetService<MainViewModel>();
				mainViewModel?.NavigateToLogin();
				return;
			}

			try
			{
				var entries = await _localStorageService.GetDiaryEntriesAsync(_userId);
				_entries = new ObservableCollection<DiaryEntry>(entries);
				List<string> tags = await _localStorageService.GetDiaryTagsAsync(_userId);
				_tags = new ObservableCollection<string>(tags);
				await UpdateMoodChartAsync();
				Log.Information("Diary entries and tags loaded for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load diary entries for user {UserId}", _userId);
				MessageBox.Show($"Failed to load diary entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CreateNote()
		{
			CreateNoteModalVisibility = true;
			NewNoteContent = string.Empty;
			SelectedEmoji = string.Empty;
			Log.Information("Opened create note modal");
		}

		[RelayCommand]
		private async Task SaveNoteAsync()
		{
			if (string.IsNullOrWhiteSpace(NewNoteContent))
				return;

			var entry = new DiaryEntry
			{
				Title = $"Day {DateTime.Now:yyyy-MM-dd}",
				Content = NewNoteContent,
				Tags = !string.IsNullOrEmpty(NewTag) ? new List<string> { NewTag } : new List<string>(),
				Date = DateTime.Now,
				Emoji = SelectedEmoji
			};

			try
			{
				await _localStorageService.CreateDiaryEntryAsync(_userId, entry);
				_entries.Add(entry);
				if (!string.IsNullOrEmpty(NewTag) && !_tags.Contains(NewTag))
				{
					await _localStorageService.AddDiaryTagAsync(_userId, NewTag);
					_tags.Add(NewTag);
				}
				CreateNoteModalVisibility = false;
				NewNoteContent = string.Empty;
				NewTag = string.Empty;
				SelectedEmoji = string.Empty;
				var analyticsService = _serviceProvider.GetService<AnalyticsService>();
				await analyticsService.AddPointsAsync(_userId, 10, "DiaryEntry");
				await UpdateMoodChartAsync();
				Log.Information("Saved diary entry for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save diary entry: {Content}", NewNoteContent);
				MessageBox.Show($"Failed to save diary entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelCreateNote()
		{
			CreateNoteModalVisibility = false;
			NewNoteContent = string.Empty;
			NewTag = string.Empty;
			SelectedEmoji = string.Empty;
			Log.Information("Cancelled create note");
		}

		[RelayCommand]
		private void CreateQuickNote()
		{
			QuickNoteModalVisibility = true;
			QuickNoteContent = string.Empty;
			Log.Information("Opened quick note modal");
		}

		[RelayCommand]
		private async Task SaveQuickNoteAsync()
		{
			if (string.IsNullOrWhiteSpace(QuickNoteContent))
				return;

			var entry = new DiaryEntry
			{
				Title = $"Quick Note {DateTime.Now:yyyy-MM-dd HH:mm}",
				Content = QuickNoteContent,
				Tags = new List<string> { "Quick" },
				Date = DateTime.Now,
				Emoji = "📝"
			};

			try
			{
				await _localStorageService.CreateDiaryEntryAsync(_userId, entry);
				_entries.Add(entry);
				if (!_tags.Contains("Quick"))
				{
					await _localStorageService.AddDiaryTagAsync(_userId, "Quick");
					_tags.Add("Quick");
				}
				QuickNoteModalVisibility = false;
				QuickNoteContent = string.Empty;
				var analyticsService = _serviceProvider.GetService<AnalyticsService>();
				await analyticsService.AddPointsAsync(_userId, 5, "QuickNote");
				await UpdateMoodChartAsync();
				Log.Information("Saved quick note for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save quick note: {Content}", QuickNoteContent);
				MessageBox.Show($"Failed to save quick note: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelQuickNote()
		{
			QuickNoteModalVisibility = false;
			QuickNoteContent = string.Empty;
			Log.Information("Cancelled quick note");
		}

		[RelayCommand]
		private async Task AddTagAsync()
		{
			if (string.IsNullOrWhiteSpace(NewTag))
				return;

			try
			{
				await _localStorageService.AddDiaryTagAsync(_userId, NewTag);
				if (!_tags.Contains(NewTag))
				{
					_tags.Add(NewTag);
				}
				NewTag = string.Empty;
				Log.Information("Added tag: {Tag} for user {UserId}", NewTag, _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to add tag: {Tag}", NewTag);
				MessageBox.Show($"Failed to add tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void SelectTag(string tag)
		{
			NewTag = tag;
			FilterEntries();
			Log.Information("Selected tag: {Tag}", tag);
		}

		[RelayCommand]
		private void ShowFilters()
		{
			Log.Information("Showing filters");
		}

		[RelayCommand]
		private void ArchiveNotes()
		{
			ArchiveNotesModalVisibility = true;
			Log.Information("Opened archive notes modal");
		}

		[RelayCommand]
		private async Task ConfirmArchiveNotesAsync()
		{
			try
			{
				await _localStorageService.ArchiveDiaryEntriesAsync(_userId);
				_entries.Clear();
				ArchiveNotesModalVisibility = false;
				Log.Information("Archived diary entries for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to archive diary entries");
				MessageBox.Show($"Failed to archive entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelArchiveNotes()
		{
			ArchiveNotesModalVisibility = false;
			Log.Information("Cancelled archive notes");
		}

		[RelayCommand]
		private async Task ExportNotesAsync()
		{
			try
			{
				var saveFileDialog = new SaveFileDialog
				{
					Title = "Export Diary Entries",
					Filter = "JSON files (*.json)|*.json"
				};

				if (saveFileDialog.ShowDialog() == true)
				{
					var json = Newtonsoft.Json.JsonConvert.SerializeObject(_entries, Newtonsoft.Json.Formatting.Indented);
					File.WriteAllText(saveFileDialog.FileName, json);
					Log.Information("Exported diary entries to {FilePath}", saveFileDialog.FileName);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to export diary entries");
				MessageBox.Show($"Failed to export entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void GoToChat()
		{
			_navigationService.NavigateTo<ChatPage>();
			Log.Information("Navigated to ChatPage");
		}

		[RelayCommand]
		private void SelectEmoji(string emoji)
		{
			SelectedEmoji = emoji;
			NewNoteContent = $"{emoji} {NewNoteContent}";
			Log.Information("Selected emoji: {Emoji}", emoji);
		}

		private async Task LoadTriggersAsync()
		{
			try
			{
				var triggers = await _localStorageService.GetTriggersAsync(_userId);
				Triggers = new ObservableCollection<string>(triggers);
				Log.Information("Loaded triggers for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load triggers for user {UserId}", _userId);
				MessageBox.Show($"Failed to load triggers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task UpdateMoodChartAsync()
		{
			try
			{
				var analyticsService = _serviceProvider.GetService<AnalyticsService>();
				var ratings = await analyticsService.GetWeeklyRatingsAsync(_userId);

				var plotModel = new PlotModel { Title = "Настроение за неделю" };

				var dateAxis = new DateTimeAxis
				{
					Position = AxisPosition.Bottom,
					StringFormat = "dd MMM",
					Title = "Дата"
				};
				plotModel.Axes.Add(dateAxis);

				var valueAxis = new LinearAxis
				{
					Position = AxisPosition.Left,
					Minimum = 0,
					Maximum = 5,
					Title = "Оценка"
				};
				plotModel.Axes.Add(valueAxis);

				var series = new LineSeries
				{
					MarkerType = MarkerType.Circle,
					MarkerSize = 4,
					MarkerFill = OxyColors.Blue
				};

				foreach (var rating in ratings.OrderBy(r => r.Date))
				{
					series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(rating.Date), rating.Score));
				}

				plotModel.Series.Add(series);
				MoodChartModel = plotModel;

				Log.Information("Updated mood chart for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to update mood chart");
				var errorModel = new PlotModel { Title = "Ошибка загрузки данных" };
				MoodChartModel = errorModel;
			}
		}

		private void FilterEntries()
		{
			var filteredEntries = _entries.AsEnumerable();

			if (!string.IsNullOrEmpty(NewTag))
			{
				filteredEntries = filteredEntries.Where(e => e.Tags.Contains(NewTag));
			}

			if (!string.IsNullOrEmpty(SearchQuery))
			{
				filteredEntries = filteredEntries.Where(e => e.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
											   e.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
			}

			_entries = new ObservableCollection<DiaryEntry>(filteredEntries);
			Log.Information("Filtered entries by tag: {Tag}, search: {SearchQuery}", NewTag, SearchQuery);
		}

		private void SortEntries()
		{
			IEnumerable<DiaryEntry> sortedEntries = _sortMode switch
			{
				0 => _entries.OrderByDescending(e => e.Date),
				1 => _entries.OrderBy(e => string.Join(",", e.Tags)),
				_ => _entries
			};
			_entries = new ObservableCollection<DiaryEntry>(sortedEntries);
			Log.Information("Sorted entries by mode: {SortMode}", _sortMode);
		}
	}
}