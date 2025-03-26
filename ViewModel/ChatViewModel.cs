using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChatBotClient.Models;
using ChatBotClient.Services;
using System.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChatBotClient.ViewModels
{
	public partial class ChatViewModel : ObservableObject
	{
		private readonly ApiService _apiService;
		private readonly LocalStorageService _storageService;
		private string _userId;

		[ObservableProperty]
		private string inputText;

		[ObservableProperty]
		private ObservableCollection<Message> messages = [];

		[ObservableProperty]
		private string sendStatus;

		[ObservableProperty]
		private UserProfile profile;

		[ObservableProperty]
		private Dictionary<string, int> moodStats;

		public ChatViewModel(ApiService apiService, LocalStorageService storageService)
		{
			_apiService = apiService;
			_storageService = storageService;
			LoadUserData();
			LoadHistory();
			LoadProfile();
			LoadMoodStats();
		}

		private void LoadUserData()
		{
			var storedData = _storageService.LoadData<dynamic>();
			if (storedData != null)
			{
				_userId = storedData.Username;
				_apiService.SetToken(storedData.Token);
			}
			else
			{
				MessageBox.Show("Please log in first.");
				Application.Current.Shutdown();
			}
		}

		[RelayCommand]
		async Task Send()
		{
			if (string.IsNullOrWhiteSpace(InputText)) return;

			var userMessage = new Message
			{
				Text = InputText,
				Author = "User",
				Timestamp = DateTime.Now,
				Status = MessageStatus.Sending
			};
			Messages.Add(userMessage);
			SendStatus = "Sending...";

			try
			{
				var botResponse = await _apiService.SendMessageAsync(_userId, InputText, Messages);
				userMessage.Status = MessageStatus.Sent;

				Messages.Add(new Message
				{
					Text = botResponse,
					Author = "Bot",
					Timestamp = DateTime.Now,
					Status = MessageStatus.Sent
				});

				SendStatus = "";
				InputText = "";
			}
			catch (Exception ex)
			{
				userMessage.Status = MessageStatus.Error;
				SendStatus = "Failed";
				MessageBox.Show($"Error: {ex.Message}");
			}
		}

		private async void LoadHistory()
		{
			try
			{
				var history = await _apiService.GetHistoryAsync(_userId);
				foreach (var item in history)
				{
					item.Status = MessageStatus.Sent;
					Messages.Add(item);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error loading history: {ex.Message}");
			}
		}

		private async void LoadProfile()
		{
			try
			{
				Profile = await _apiService.GetProfileAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error loading profile: {ex.Message}");
			}
		}

		private async void LoadMoodStats()
		{
			try
			{
				MoodStats = await _apiService.GetMoodStatsAsync(_userId);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error loading mood stats: {ex.Message}");
			}
		}
	}
}