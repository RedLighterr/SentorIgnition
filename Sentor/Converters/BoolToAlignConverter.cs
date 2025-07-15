using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Globalization;

namespace Sentor.Converters
{
	public class BoolToAlignConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? HorizontalAlignment.Right : HorizontalAlignment.Left;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}
