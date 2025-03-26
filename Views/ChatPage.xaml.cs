using ChatBotClient.Models;
using ChatBotClient.Services;
using ChatBotClient.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
				DataContext = new ChatViewModel(apiService, storageService);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to initialize chat: {ex.Message}",
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
				Close();
			}
		}
	}

	internal class MessageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Message message)
				return string.Empty;

			return $"{message.Timestamp:HH:mm:ss} | {message.Author}: {message.Text} " +
				   $"{(message.Status != MessageStatus.None ? $"({message.Status})" : "")}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}

	internal class KeyValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not System.Collections.Generic.KeyValuePair<string, int> pair)
				return string.Empty;

			return $"{pair.Key}: {pair.Value}%";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}
}