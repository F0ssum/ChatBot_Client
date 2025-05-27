using ChatBotClient.Infrastructure.Interfaces;
using System.Windows;

namespace ChatBotClient.Infrastructure.Services
{
	public class ErrorHandler : IErrorHandler
	{
		public void ShowError(string message, string title = "Error")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public async Task ShowErrorAsync(string message, string title = "Error")
		{
			await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => ShowError(message, title));
		}
	}
}