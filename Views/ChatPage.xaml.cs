using ChatBotClient.Services;
using ChatBotClient.ViewModels;
using System.Windows;

namespace ChatBotClient.Views
{
	public partial class ChatPage : Window
	{
		public ChatPage()
		{
			InitializeComponent();
			var apiService = new ApiService();
			var chatViewModel = new ChatViewModel(apiService);
			DataContext = chatViewModel;
		}
	}
}