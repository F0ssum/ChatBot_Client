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
	public partial class ChatViewModel : ObservableObject
	{
		private readonly ApiService _apiService;
		private readonly LocalStorageService _localStorageService;
		private readonly IServiceProvider _serviceProvider;
		private string _userId;

		[ObservableProperty]
		private ObservableCollection<Models.Message> messages = new();

		[ObservableProperty]
		private string messageText = string.Empty;

		[ObservableProperty]
		private Visibility newDialogModalVisibility = Visibility.Collapsed;

		[ObservableProperty]
		private Visibility chatHistoryModalVisibility = Visibility.Collapsed;

		[ObservableProperty]
		private ObservableCollection<string> chatHistory = new();

		public ChatViewModel(ApiService apiService, LocalStorageService localStorageService, IServiceProvider serviceProvider)
		{
			_apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Log.Information("ChatViewModel initialized");
		}

		public async Task InitializeAsync()
		{
			try
			{
				var (userIds, _) = _localStorageService.LoadUserData();
				_userId = userIds?.FirstOrDefault();
				Log.Information("Loaded userIds: {UserId}", _userId ?? "none");

				if (!string.IsNullOrEmpty(_userId))
				{
					await LoadHistoryAsync();
					await LoadMoodStatsAsync();
					await LoadChatHistoryAsync();
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize ChatViewModel: {Message}", ex.Message);
				MessageBox.Show($"Ошибка инициализации чата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadHistoryAsync()
		{
			try
			{
				Log.Information("Fetching history for user {UserId}", _userId);
				var history = await _apiService.GetHistoryAsync(_userId);
				Messages.Clear();
				foreach (var msg in history)
				{
					Messages.Add(msg);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load chat history: {Message}", ex.Message);
				MessageBox.Show($"Ошибка загрузки истории чатов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadMoodStatsAsync()
		{
			try
			{
				Log.Information("Fetching mood stats for user {UserId}", _userId);
				var stats = await _apiService.GetMoodStatsAsync(_userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load mood stats: {Message}", ex.Message);
				MessageBox.Show($"Ошибка загрузки статистики настроений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadChatHistoryAsync()
		{
			try
			{
				Log.Information("Fetching chat history list for user {UserId}", _userId);
				var historyList = await _apiService.GetChatHistoryListAsync(_userId);
				ChatHistory.Clear();
				foreach (var item in historyList)
				{
					ChatHistory.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load chat history list: {Message}", ex.Message);
				MessageBox.Show($"Ошибка загрузки списка чатов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task SendMessage()
		{
			if (string.IsNullOrWhiteSpace(MessageText)) return;

			try
			{
				var message = new Models.Message { Text = MessageText, Author = _userId, Timestamp = DateTime.Now };
				Messages.Add(message);
				var botResponse = await _apiService.SendMessageAsync(_userId, MessageText, Messages);
				Messages.Add(new Models.Message { Text = botResponse, Author = "Bot", Timestamp = DateTime.Now });
				MessageText = string.Empty;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to send message: {Message}", ex.Message);
				MessageBox.Show($"Ошибка отправки сообщения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void AddEmoji()
		{
			try
			{
				MessageText += "😊";
				Log.Information("Emoji added to message");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to add emoji: {Message}", ex.Message);
				MessageBox.Show($"Ошибка добавления эмодзи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void AttachFile()
		{
			try
			{
				MessageBox.Show("Функция прикрепления файла пока не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
				Log.Information("Attach file attempted");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to attach file: {Message}", ex.Message);
				MessageBox.Show($"Ошибка прикрепления файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void VoiceInput()
		{
			try
			{
				MessageBox.Show("Функция голосового ввода пока не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
				Log.Information("Voice input attempted");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initiate voice input: {Message}", ex.Message);
				MessageBox.Show($"Ошибка голосового ввода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void ShowChatHistory()
		{
			try
			{
				ChatHistoryModalVisibility = Visibility.Visible;
				Log.Information("Chat history modal shown");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to show chat history: {Message}", ex.Message);
				MessageBox.Show($"Ошибка показа истории чатов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CloseChatHistory()
		{
			try
			{
				ChatHistoryModalVisibility = Visibility.Collapsed;
				Log.Information("Chat history modal closed");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to close chat history: {Message}", ex.Message);
				MessageBox.Show($"Ошибка закрытия истории чатов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void NewDialog()
		{
			try
			{
				NewDialogModalVisibility = Visibility.Visible;
				Log.Information("New dialog modal shown");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to show new dialog modal: {Message}", ex.Message);
				MessageBox.Show($"Ошибка показа модального окна нового диалога: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelNewDialog()
		{
			try
			{
				NewDialogModalVisibility = Visibility.Collapsed;
				Log.Information("New dialog modal canceled");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to cancel new dialog: {Message}", ex.Message);
				MessageBox.Show($"Ошибка отмены нового диалога: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private async Task ConfirmNewDialog()
		{
			try
			{
				Messages.Clear();
				NewDialogModalVisibility = Visibility.Collapsed;
				await _apiService.ClearChatHistoryAsync(_userId);
				Log.Information("New dialog confirmed, messages cleared");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to confirm new dialog: {Message}", ex.Message);
				MessageBox.Show($"Ошибка подтверждения нового диалога: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void GoToDiary()
		{
			try
			{
				var mainWindow = Application.Current.MainWindow;
				var frame = mainWindow?.FindName("MainFrame") as Frame;
				if (frame != null)
				{
					frame.Navigate(_serviceProvider.GetRequiredService<DiaryPage>());
					Log.Information("Navigated to DiaryPage");
				}
				else
				{
					Log.Warning("MainFrame not found for navigation");
					MessageBox.Show("Ошибка навигации: MainFrame не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to navigate to DiaryPage: {Message}", ex.Message);
				MessageBox.Show($"Ошибка перехода к дневнику: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}