using ChatBotClient.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace ChatBotClient.Features.Chat.Views
{
	public partial class ChatPage : Page
	{
		private readonly ChatViewModel _viewModel;

		public ChatPage(IServiceProvider serviceProvider)
		{
			InitializeComponent();
			try
			{
				_viewModel = serviceProvider.GetRequiredService<ChatViewModel>();
				DataContext = _viewModel;
				Loaded += async (s, e) =>
				{
					await _viewModel.InitializeAsync();
					Log.Information("ChatPage initialized");
				};
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize ChatPage: {Message}", ex.Message);
				MessageBox.Show($"Failed to initialize chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}