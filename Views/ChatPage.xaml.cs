using ChatBotClient.Models;
using ChatBotClient.Services;
using ChatBotClient.ViewModels;
using System;
using System.Windows;

namespace ChatBotClient.Views
{
	public partial class ChatPage : Window
	{
		public ChatPage()
		{
			InitializeComponent();

			try
			{
				var apiService = new ApiService();
				var storageService = new LocalStorageService();
				var viewModel = new ChatViewModel(apiService, storageService);
				DataContext = viewModel;
				Loaded += async (s, e) => await viewModel.InitializeAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to initialize chat: {ex}");
				MessageBox.Show($"Failed to initialize chat: {ex.Message}\nStackTrace: {ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Close();
			}
		}
	}
}