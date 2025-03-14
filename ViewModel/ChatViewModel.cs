using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChatBotClient.Models;
using ChatBotClient.Services;
using System.Windows;
using Newtonsoft.Json;

namespace ChatBotClient.ViewModels
{
	public partial class ChatViewModel : ObservableObject
	{
		private readonly ApiService _apiService;

		[ObservableProperty]
		private string inputText;

		[ObservableProperty]
		private ObservableCollection<Message> messages = new();

		[ObservableProperty]
		private string sendStatus;

		public ChatViewModel(ApiService apiService)
		{
			_apiService = apiService;
			LoadHistory();
		}

		[RelayCommand]
		async Task Send()
		{
			if (string.IsNullOrWhiteSpace(InputText)) return;

			var userMessage = new Message { Text = InputText, Author = "User", Timestamp = DateTime.Now, Status = "Sending..." };
			Messages.Add(userMessage);
			SendStatus = "Sending...";

			try
			{
				var botResponse = await _apiService.SendMessageAsync(InputText, Messages);
				userMessage.Status = "Sent";
				Messages.Add(new Message { Text = botResponse, Author = "Bot", Timestamp = DateTime.Now, Status = "Sent" });
				SendStatus = "";
				InputText = "";
			}
			catch (Exception ex)
			{
				userMessage.Status = "Failed";
				SendStatus = "Failed";
				MessageBox.Show($"Error: {ex.Message}");
			}
		}

		private async void LoadHistory()
		{
			try
			{
				var history = await _apiService.GetHistoryAsync();
				foreach (var item in history)
				{
					item.Status = "Sent";
					Messages.Add(item);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error loading history: {ex.Message}");
			}
		}
	}
}