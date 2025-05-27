using ChatBotClient.Core.Models;
using ChatBotClient.Features.Diary.Views;
using ChatBotClient.Features.Services;
using ChatBotClient.Infrastructure.Services;
using ChatBotClient.ViewModel.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Speech.Recognition;
using System.Windows;

namespace ChatBotClient.ViewModel
{
	public partial class ChatViewModel : ObservableObject
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ApiService _apiService;
		private readonly LocalStorageService _localStorageService;
		private readonly NavigationService _navigationService;
		private readonly NotificationSettingsViewModel _notificationSettings;
		private readonly OfflineQueueService _queueService;
		private readonly string _userId;
		private ObservableCollection<Message> _messages;
		private ObservableCollection<string> _chatHistory;
		private string _messageText;
		private bool _newDialogModalVisibility;
		private bool _chatHistoryModalVisibility;
		private bool _isExerciseModalVisible;
		private bool _isRatingModalVisible;
		private bool _isEmojiModalVisible;
		private int _sessionRating;
		private WaveInEvent _waveIn;
		private WaveFileWriter _waveWriter;
		private bool _isRecording;

		public ObservableCollection<Message> Messages
		{
			get => _messages;
			set => SetProperty(ref _messages, value);
		}

		public ObservableCollection<string> ChatHistory
		{
			get => _chatHistory;
			set => SetProperty(ref _chatHistory, value);
		}

		public string MessageText
		{
			get => _messageText;
			set => SetProperty(ref _messageText, value);
		}

		public bool NewDialogModalVisibility
		{
			get => _newDialogModalVisibility;
			set => SetProperty(ref _newDialogModalVisibility, value);
		}

		public bool ChatHistoryModalVisibility
		{
			get => _chatHistoryModalVisibility;
			set => SetProperty(ref _chatHistoryModalVisibility, value);
		}

		public bool IsExerciseModalVisible
		{
			get => _isExerciseModalVisible;
			set => SetProperty(ref _isExerciseModalVisible, value);
		}

		public bool IsRatingModalVisible
		{
			get => _isRatingModalVisible;
			set => SetProperty(ref _isRatingModalVisible, value);
		}

		public bool IsEmojiModalVisible
		{
			get => _isEmojiModalVisible;
			set => SetProperty(ref _isEmojiModalVisible, value);
		}

		public int SessionRating
		{
			get => _sessionRating;
			set => SetProperty(ref _sessionRating, value);
		}

		public ChatViewModel(IServiceProvider serviceProvider, ApiService apiService,
							LocalStorageService localStorageService, NavigationService navigationService,
							NotificationSettingsViewModel notificationSettings, OfflineQueueService queueService)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
			_notificationSettings = notificationSettings ?? throw new ArgumentNullException(nameof(notificationSettings));
			_queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));

			// Retrieve UserId from LocalStorageService
			var (userIds, _) = _localStorageService.LoadUserData();
			_userId = userIds?.Count > 0 ? userIds[0] : throw new InvalidOperationException("User ID not found");

			Messages = new ObservableCollection<Message>();
			ChatHistory = new ObservableCollection<string>();
			Log.Information("ChatViewModel initialized with ID {Id}", _userId);
		}

		public async Task InitializeAsync()
		{
			try
			{
				var messages = await _localStorageService.LoadDataAsync<List<Message>>($"chat_{_userId}.json");
				if (messages != null)
				{
					Messages = new ObservableCollection<Message>(messages);
				}
				var history = await _localStorageService.LoadDataAsync<List<string>>("chat_history.json");
				if (history != null)
				{
					ChatHistory = new ObservableCollection<string>(history);
				}
				Log.Information("Chat data loaded for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize chat data for user {UserId}", _userId);
			}
		}

		[RelayCommand]
		private async Task SendMessageAsync()
		{
			if (string.IsNullOrWhiteSpace(MessageText)) return;

			var message = new Message { Text = MessageText, Author = _userId, Timestamp = DateTime.Now, StatusCode = MessageStatus.Sending };
			Messages.Add(message);

			try
			{
				string language = _notificationSettings.LangIndex switch { 0 => "ru", 1 => "en", _ => "ru" };
				int maxResponseLength = int.TryParse(_notificationSettings.MaxResponseLength, out int length) ? length : 200;
				var response = await _apiService.SendMessageAsync(_userId, MessageText, Messages.ToList(), language,
					_notificationSettings.CustomPrompt, _notificationSettings.Temperature, _notificationSettings.TopP, maxResponseLength);
				message.StatusCode = MessageStatus.Sent;
				Messages.Add(new Message { Text = response, Author = "Bot", Timestamp = DateTime.Now, StatusCode = MessageStatus.Sent });
				MessageText = "";
				await _notificationSettings.NotifyAsync("Новое сообщение получено");
				// Save chat messages
				await _localStorageService.SaveDataAsync($"chat_{_userId}.json", Messages.ToList());
				// Suggest exercise with 20% probability
				if (new Random().NextDouble() < 0.2)
				{
					Messages.Add(new Message
					{
						Text = "Попробуй дыхательное упражнение 4-7-8: вдох 4 сек, задержка 7 сек, выдох 8 сек.",
						Author = "Bot",
						Timestamp = DateTime.Now,
						StatusCode = MessageStatus.Sent
					});
					IsExerciseModalVisible = true;
					await _localStorageService.SaveDataAsync($"chat_{_userId}.json", Messages.ToList());
				}
				// Add points
				var pointsService = _serviceProvider.GetService<AnalyticsService>();
				await pointsService.AddPointsAsync(_userId, 5, "Chat");
				// Show rating
				IsRatingModalVisible = true;
			}
			catch (Exception ex)
			{
				message.StatusCode = MessageStatus.Error;
				_queueService.QueueAction("SendMessage", new { UserId = _userId, Text = MessageText });
				MessageBox.Show($"Не удалось отправить сообщение: {ex.Message}. Действие добавлено в очередь.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Log.Error(ex, "Failed to send message for user {UserId}", _userId);
			}
		}

		[RelayCommand]
		private async Task VoiceInputAsync()
		{
			try
			{
				Log.Information("Starting voice input...");
				using var recognizer = new SpeechRecognitionEngine();
				recognizer.SetInputToDefaultAudioDevice();
				recognizer.LoadGrammar(new DictationGrammar());
				var tcs = new TaskCompletionSource<string>();
				recognizer.SpeechRecognized += (s, e) => tcs.TrySetResult(e.Result.Text);
				recognizer.SpeechRecognitionRejected += (s, e) => tcs.TrySetException(new Exception("Распознавание речи не удалось"));
				recognizer.RecognizeAsync(RecognizeMode.Single);
				MessageText = await tcs.Task;
				Log.Information("Recognized speech: {Text}", MessageText);
				await SendMessageAsync();
				await _notificationSettings.NotifyAsync("Голосовое сообщение отправлено");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to process voice input");
				MessageBox.Show($"Ошибка голосового ввода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void AttachFile()
		{
			// Placeholder for file attachment logic
			MessageBox.Show("Функция прикрепления файла пока не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
			Log.Information("AttachFile command invoked");
		}

		[RelayCommand]
		private void NewDialog()
		{
			NewDialogModalVisibility = true;
			Log.Information("Opened new dialog modal");
		}

		[RelayCommand]
		private async Task ConfirmNewDialogAsync()
		{
			try
			{
				Messages.Clear();
				ChatHistory.Add($"Диалог {DateTime.Now:yyyy-MM-dd HH:mm}");
				// Save chat history
				await _localStorageService.SaveDataAsync("chat_history.json", ChatHistory.ToList());
				// Save empty messages
				await _localStorageService.SaveDataAsync($"chat_{_userId}.json", new List<Message>());
				NewDialogModalVisibility = false;
				Log.Information("Started new dialog for user {UserId}", _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to start new dialog");
				MessageBox.Show($"Не удалось начать новый диалог: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelNewDialog()
		{
			NewDialogModalVisibility = false;
			Log.Information("Cancelled new dialog");
		}

		[RelayCommand]
		private void ShowChatHistory()
		{
			ChatHistoryModalVisibility = true;
			Log.Information("Opened chat history modal");
		}

		[RelayCommand]
		private void CloseChatHistory()
		{
			ChatHistoryModalVisibility = false;
			Log.Information("Closed chat history modal");
		}

		[RelayCommand]
		private void GoToDiary()
		{
			_navigationService.NavigateTo<DiaryPage>();
			Log.Information("Navigated to DiaryPage");
		}

		[RelayCommand]
		private void TryExercise()
		{
			IsExerciseModalVisible = true;
			Log.Information("Opened exercise modal");
		}

		[RelayCommand]
		private async Task StartExerciseAsync()
		{
			try
			{
				await _notificationSettings.NotifyAsync("Начните: вдох 4 сек, задержка 7 сек, выдох 8 сек");
				Log.Information("Started breathing exercise");
				IsExerciseModalVisible = false;
				var pointsService = _serviceProvider.GetService<AnalyticsService>();
				await pointsService.AddPointsAsync(_userId, 15, "Exercise");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to start exercise");
				MessageBox.Show($"Ошибка при запуске упражнения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CloseExercise()
		{
			IsExerciseModalVisible = false;
			Log.Information("Closed exercise modal");
		}

		[RelayCommand]
		private async Task SubmitRatingAsync()
		{
			try
			{
				var analyticsService = _serviceProvider.GetService<AnalyticsService>();
				await analyticsService.SaveSessionRatingAsync(_userId, SessionRating + 1); // 0-based to 1-based
				IsRatingModalVisible = false;
				await _notificationSettings.NotifyAsync($"Оценка {SessionRating + 1} сохранена");
				Log.Information("Saved session rating {Rating} for user {UserId}", SessionRating + 1, _userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save session rating");
				MessageBox.Show($"Не удалось сохранить оценку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void AddEmoji()
		{
			IsEmojiModalVisible = true;
			Log.Information("Opened emoji modal");
		}

		[RelayCommand]
		private void SelectEmoji(string emoji)
		{
			try
			{
				MessageText += emoji;
				IsEmojiModalVisible = false;
				Log.Information("Selected emoji: {Emoji}", emoji);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to select emoji");
				MessageBox.Show($"Ошибка при выборе эмодзи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CloseEmojiModal()
		{
			IsEmojiModalVisible = false;
			Log.Information("Closed emoji modal");
		}

		[RelayCommand]
		private async Task RecordAudioAsync()
		{
			try
			{
				if (!_isRecording)
				{
					_waveIn = new WaveInEvent();
					var filePath = Path.Combine(Path.GetTempPath(), $"recording_{DateTime.Now:yyyyMMddHHmmss}.wav");
					_waveWriter = new WaveFileWriter(filePath, _waveIn.WaveFormat);
					_waveIn.DataAvailable += (s, e) =>
					{
						_waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
					};
					_waveIn.StartRecording();
					_isRecording = true;
					await _notificationSettings.NotifyAsync("Запись начата");
					Log.Information("Started audio recording to {FilePath}", filePath);
				}
				else
				{
					_waveIn.StopRecording();
					try
					{
						var response = await _apiService.SendAudioAsync(_userId, _waveWriter.Filename);
						Messages.Add(new Message
						{
							Text = $"Аудио записано: {response}",
							Author = _userId,
							Timestamp = DateTime.Now,
							StatusCode = MessageStatus.Sent
						});
						await _localStorageService.SaveDataAsync($"chat_{_userId}.json", Messages.ToList());
						await _notificationSettings.NotifyAsync("Запись остановлена");
						Log.Information("Stopped audio recording and sent to API");
					}
					finally
					{
						_waveIn.Dispose();
						_waveWriter.Close();
						_waveWriter.Dispose();
						_isRecording = false;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to record audio");
				MessageBox.Show($"Ошибка записи аудио: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}