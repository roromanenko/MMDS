using System;
using System.Globalization;
using System.Windows.Data;

namespace UI.Converters
{
	public sealed class DoubleInvariantConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is double d ? d.ToString("G", CultureInfo.InvariantCulture) : "";

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			double.TryParse((value as string) ?? "", NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
				? d : 0d;
	}
}
