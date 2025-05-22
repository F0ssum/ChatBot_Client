using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatBotClient.Converters
{
	public class MessageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string author)
			{
				return author == "Bot" ? Brushes.LightBlue : Brushes.LightGreen;
			}
			return Brushes.Gray; // Запасной фон для null или некорректных значений
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}