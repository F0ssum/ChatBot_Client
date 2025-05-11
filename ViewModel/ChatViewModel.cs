using ChatBotClient.Models;
using ChatBotClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ChatBotClient.ViewModels
{
	public partial class ChatViewModel : ObservableObject
	{
		private readonly ApiService _apiService;
		private readonly LocalStorageService _storageService;
		private string _userId;
		private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

		[ObservableProperty]
		private string userId;

		[ObservableProperty]
		private string inputUserId;

		[ObservableProperty]
		private bool isFirstRun;

		[ObservableProperty]
		private string inputText;

		[ObservableProperty]
		private ObservableCollection<Message> messages = [];

		[ObservableProperty]
		private string sendStatus;

		[ObservableProperty]
		private Dictionary<string, int> moodStats;

		[ObservableProperty]
		private ObservableCollection<string> userIds = [];

		[ObservableProperty]
		private string selectedUserId;

		[ObservableProperty]
		private string newUserId;

		public ChatViewModel(ApiService apiService, LocalStorageService storageService)
		{
			_apiService = apiService;
			_storageService = storageService;
		}

		public async Task InitializeAsync()
		{
			var userIds = _storageService.GetUserIds();
			if (userIds == null || userIds.Count == 0)
			{
				IsFirstRun = true;
				Console.WriteLine("No userIds found, waiting for user input");
			}
			else
			{
				IsFirstRun = false;
				await _dispatcher.InvokeAsync(() =>
				{
					UserIds = new ObservableCollection<string>(userIds);
					SelectedUserId = UserIds[0];
					_userId = SelectedUserId;
					UserId = _userId;
				});
				Console.WriteLine($"Initializing with userId: {_userId}");
				await Task.WhenAll(LoadHistoryAsync(), LoadMoodStatsAsync());
			}
		}

		[RelayCommand]
		async Task StartChat()
		{
			if (string.IsNullOrWhiteSpace(InputUserId))
			{
				MessageBox.Show("Please enter a User ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			_userId = InputUserId.Trim();
			if (UserIds.Contains(_userId))
			{
				MessageBox.Show("This User ID already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			await _dispatcher.InvokeAsync(() => UserIds.Add(_userId));
			_storageService.SaveUserIds(UserIds);
			await _dispatcher.InvokeAsync(() =>
			{
				SelectedUserId = _userId;
				UserId = _userId;
				IsFirstRun = false;
				InputUserId = null;
			});
			Console.WriteLine($"Started chat with userId: {_userId}");
			await Task.WhenAll(LoadHistoryAsync(), LoadMoodStatsAsync());
		}

		[RelayCommand]
		async Task CreateUserId()
		{
			if (string.IsNullOrWhiteSpace(NewUserId))
			{
				MessageBox.Show("Please enter a new User ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var newId = NewUserId.Trim();
			if (UserIds.Contains(newId))
			{
				MessageBox.Show("This User ID already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			await _dispatcher.InvokeAsync(() => UserIds.Add(newId));
			_storageService.SaveUserIds(UserIds);
			await _dispatcher.InvokeAsync(() =>
			{
				SelectedUserId = newId;
				NewUserId = null;
			});
			Console.WriteLine($"Created new userId: {newId}");
		}

		partial void OnSelectedUserIdChanged(string value)
		{
			if (!string.IsNullOrEmpty(value) && value != _userId)
			{
				_userId = value;
				_dispatcher.InvokeAsync(() =>
				{
					UserId = _userId;
					Messages.Clear();
					MoodStats = null;
				});
				Console.WriteLine($"Switched to userId: {_userId}");
				Task.Run(async () => await Task.WhenAll(LoadHistoryAsync(), LoadMoodStatsAsync()));
			}
		}

		[RelayCommand]
		async Task Send()
		{
			if (string.IsNullOrWhiteSpace(InputText)) return;
			if (string.IsNullOrWhiteSpace(_userId))
			{
				await _dispatcher.InvokeAsync(() => SendStatus = "Failed: No User ID selected");
				MessageBox.Show("No User ID selected. Please select or create a User ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var userMessage = new Message
			{
				Text = InputText,
				Author = "User",
				Timestamp = DateTime.Now,
				Status = MessageStatus.Sending
			};
			await _dispatcher.InvokeAsync(() => Messages.Add(userMessage));
			await _dispatcher.InvokeAsync(() => SendStatus = "Sending...");

			try
			{
				var botResponse = await _apiService.SendMessageAsync(_userId, InputText, Messages);
				await _dispatcher.InvokeAsync(() => userMessage.Status = MessageStatus.Sent);

				await _dispatcher.InvokeAsync(() =>
				{
					Messages.Add(new Message
					{
						Text = botResponse,
						Author = "Bot",
						Timestamp = DateTime.Now,
						Status = MessageStatus.Sent
					});
				});

				await _dispatcher.InvokeAsync(() =>
				{
					SendStatus = "";
					InputText = "";
				});
			}
			catch (HttpRequestException ex)
			{
				await _dispatcher.InvokeAsync(() => userMessage.Status = MessageStatus.Error);
				await _dispatcher.InvokeAsync(() => SendStatus = "Failed");
				Console.WriteLine($"HTTP error sending message: {ex}");
				MessageBox.Show("Failed to send message. Please check your connection or try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				await _dispatcher.InvokeAsync(() => userMessage.Status = MessageStatus.Error);
				await _dispatcher.InvokeAsync(() => SendStatus = "Failed");
				Console.WriteLine($"Error sending message: {ex}");
				MessageBox.Show("An error occurred while processing your message. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadHistoryAsync()
		{
			try
			{
				var history = await _apiService.GetHistoryAsync(_userId);
				await _dispatcher.InvokeAsync(() =>
				{
					foreach (var item in history)
					{
						item.Status = MessageStatus.Sent;
						Messages.Add(item);
					}
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading history: {ex}");
				MessageBox.Show("Failed to load chat history. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async Task LoadMoodStatsAsync()
		{
			try
			{
				var stats = await _apiService.GetMoodStatsAsync(_userId);
				await _dispatcher.InvokeAsync(() => MoodStats = stats);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading mood stats: {ex}");
				MessageBox.Show("Failed to load mood statistics. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}