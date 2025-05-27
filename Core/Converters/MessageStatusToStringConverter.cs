using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using ChatBotClient.Core.Models;

namespace ChatBotClient.Core.Converters
{
    public class MessageStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                MessageStatus.Sending => "⏳",
                MessageStatus.Sent => "✓",
                MessageStatus.Error => "⚠️",
                _ => ""
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}