using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Making.Cents.Common.Converters
{
	public class EqualityConverter : IValueConverter
	{
		public bool Inverse { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var equals = parameter is IList arr
				? arr.Contains(value)
				: value.Equals(parameter);
			if (Inverse)
				equals = !equals;

			return targetType == typeof(Visibility)
				? equals ? Visibility.Visible : Visibility.Collapsed
				: equals;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}
