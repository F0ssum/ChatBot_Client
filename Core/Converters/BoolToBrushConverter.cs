using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatBotClient.Core.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        // parameter: "Green;Red"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colors = (parameter as string)?.Split(';');
            if (colors == null || colors.Length < 2)
                return Brushes.Gray;
            return (value is bool b && b)
                ? (SolidColorBrush)new BrushConverter().ConvertFromString(colors[0])
                : (SolidColorBrush)new BrushConverter().ConvertFromString(colors[1]);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}