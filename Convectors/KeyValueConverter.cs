using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ChatBotClient.Converters
{
	public class KeyValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not KeyValuePair<string, int> pair)
				return string.Empty;

			return $"{pair.Key}: {pair.Value}%";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}
}