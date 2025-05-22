using ChatBotClient.Models;
using ChatBotClient.Services;
using ChatBotClient.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChatBotClient.ViewModels
{
	public partial class DiaryViewModel : ObservableObject
	{
		private readonly ApiService _apiService;
		private readonly IServiceProvider _serviceProvider;
		private readonly string _userId;

		[ObservableProperty]
		private ObservableCollection<DiaryEntry> entries = new();

		[ObservableProperty]
		private ObservableCollection<string> tags = new();

		[ObservableProperty]
		private string newTag = string.Empty;

		[ObservableProperty]
		private string searchQuery = string.Empty;

		[ObservableProperty]
		private int sortMode = 0; // 0: По дате, 1: По тегу

		[ObservableProperty]
		private bool isEntriesTabSelected = true;

		[ObservableProperty]
		private bool isSettingsTabSelected = false;

		[ObservableProperty]
		private Visibility createNoteModalVisibility = Visibility.Collapsed;

		[ObservableProperty]
		private Visibility archiveNotesModalVisibility = Visibility.Collapsed;

		[ObservableProperty]
		private string newNoteContent = string.Empty;

		public DiaryViewModel(ApiService apiService, LocalStorageService localStorageService, IServiceProvider serviceProvider)
		{
			_apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			var (userIds, _) = localStorageService?.LoadUserData() ?? (null, null);
			_userId = userIds?.FirstOrDefault() ?? throw new ArgumentNullException("User ID not found");
			Log.Information("DiaryViewModel initialized with UserId: {UserId}", _userId);
		}

		public async Task InitializeAsync()
		{
			try
			{
				await LoadEntriesAsync();
				await LoadTagsAsync();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize DiaryViewModel: {Message}", ex.Message);
				MessageBox.Show($"Ошибка инициализации дневника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadEntriesAsync()
		{
			try
			{
				Log.Information("Fetching diary entries for user {UserId}", _userId);
				var entries = await _apiService.GetDiaryEntriesAsync(_userId);
				Entries.Clear();
				foreach (var entry in entries)
				{
					Entries.Add(entry);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load diary entries: {Message}", ex.Message);
				MessageBox.Show($"Ошибка загрузки записей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadTagsAsync()
		{
			try
			{
				Log.Information("Fetching tags for user {UserId}", _userId);
				var tags = await _apiService.GetDiaryTagsAsync(_userId);
				Tags.Clear();
				foreach (var tag in tags)
				{
					Tags.Add(tag);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load tags: {Message}", ex.Message);
				MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task AddTag()
		{
			if (string.IsNullOrWhiteSpace(NewTag)) return;

			try
			{
				string newTag = NewTag.StartsWith("#") ? NewTag : "#" + NewTag;
				if (!Tags.Contains(newTag))
				{
					await _apiService.AddDiaryTagAsync(_userId, newTag);
					Tags.Add(newTag);
					NewTag = string.Empty;
					Log.Information("Tag added: {Tag}", newTag);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to add tag: {Message}", ex.Message);
				MessageBox.Show($"Ошибка добавления тега: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void SelectTag(string tag)
		{
			try
			{
				// Заглушка: фильтрация по тегу
				Log.Information("Tag selected: {Tag}", tag);
				MessageBox.Show($"Фильтрация по тегу {tag} не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to select tag: {Message}", ex.Message);
				MessageBox.Show($"Ошибка выбора тега: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void ShowFilters()
		{
			try
			{
				Log.Information("Show filters attempted");
				MessageBox.Show("Функция фильтров пока не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to show filters: {Message}", ex.Message);
				MessageBox.Show($"Ошибка показа фильтров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CreateNote()
		{
			try
			{
				CreateNoteModalVisibility = Visibility.Visible;
				NewNoteContent = string.Empty;
				Log.Information("Create note modal shown");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to show create note modal: {Message}", ex.Message);
				MessageBox.Show($"Ошибка показа окна создания заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelCreateNote()
		{
			try
			{
				CreateNoteModalVisibility = Visibility.Collapsed;
				NewNoteContent = string.Empty;
				Log.Information("Create note modal canceled");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to cancel create note: {Message}", ex.Message);
				MessageBox.Show($"Ошибка отмены создания заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task SaveNote()
		{
			if (string.IsNullOrWhiteSpace(NewNoteContent)) return;

			try
			{
				var entry = new DiaryEntry
				{
					Title = $"День {Entries.Count + 1}",
					Date = DateTime.Now,
					Content = NewNoteContent,
					Tags = Tags.Where(t => t.Contains("Позитив")).ToList() // Заглушка для тегов
				};
				await _apiService.CreateDiaryEntryAsync(_userId, entry);
				Entries.Add(entry);
				CreateNoteModalVisibility = Visibility.Collapsed;
				NewNoteContent = string.Empty;
				Log.Information("Note saved: {Title}", entry.Title);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save note: {Message}", ex.Message);
				MessageBox.Show($"Ошибка сохранения заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void ArchiveNotes()
		{
			try
			{
				ArchiveNotesModalVisibility = Visibility.Visible;
				Log.Information("Archive notes modal shown");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to show archive notes modal: {Message}", ex.Message);
				MessageBox.Show($"Ошибка показа окна архивации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task ConfirmArchiveNotes()
		{
			try
			{
				await _apiService.ArchiveDiaryEntriesAsync(_userId);
				Entries.Clear();
				ArchiveNotesModalVisibility = Visibility.Collapsed;
				Log.Information("Notes archived");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to archive notes: {Message}", ex.Message);
				MessageBox.Show($"Ошибка архивации записей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelArchiveNotes()
		{
			try
			{
				ArchiveNotesModalVisibility = Visibility.Collapsed;
				Log.Information("Archive notes modal canceled");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to cancel archive notes: {Message}", ex.Message);
				MessageBox.Show($"Ошибка отмены архивации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void ExportNotes()
		{
			try
			{
				Log.Information("Export notes attempted");
				MessageBox.Show("Функция экспорта пока не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to export notes: {Message}", ex.Message);
				MessageBox.Show($"Ошибка экспорта записей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void GoToChat()
		{
			try
			{
				var mainWindow = Application.Current.MainWindow;
				var frame = mainWindow?.FindName("MainFrame") as Frame;
				if (frame != null)
				{
					frame.Navigate(_serviceProvider.GetRequiredService<ChatPage>());
					Log.Information("Navigated to ChatPage");
				}
				else
				{
					Log.Warning("MainFrame not found for navigation");
					MessageBox.Show("Ошибка навигации: MainFrame не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to navigate to ChatPage: {Message}", ex.Message);
				MessageBox.Show($"Ошибка перехода к чату: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}