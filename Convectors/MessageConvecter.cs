using ChatBotClient.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ChatBotClient.Converters
{
	public class MessageConverter : IValueConverter
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
}