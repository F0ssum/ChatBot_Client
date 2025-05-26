using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatBotClient.Core.Converters
{
	public class MessageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string author = value as string;
			string param = parameter as string;
			if (param == "Alignment")
			{
				return author == "User" ? HorizontalAlignment.Right : HorizontalAlignment.Left;
			}
			else if (param == "GridColumn")
			{
				return author == "User" ? 1 : 0;
			}
			else
			{
				return author == "User" ? Brushes.LightGreen : Brushes.LightBlue;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}