using System.Globalization;
using System.Windows.Data;

namespace ChatBotClient.Core.Converters
{
	public class ListToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is IEnumerable<string> list && list.Any())
			{
				return $"Теги: {string.Join(", ", list)}";
			}
			return "Теги: отсутствуют";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}