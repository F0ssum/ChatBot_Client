﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatBotClient.Converters
{
	public class SizeToRectConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length >= 2 &&
				values[0] is double width &&
				values[1] is double height)
			{
				return new Rect(0, 0, Math.Max(0, width), Math.Max(0, height));
			}
			return new Rect(0, 0, 0, 0);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}