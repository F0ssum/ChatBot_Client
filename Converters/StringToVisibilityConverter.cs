using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatBotClient.Converters
{
	public class StringToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}