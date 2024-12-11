using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Data;

namespace KmlViewer
{
	public class NullOrEmptyToCollapsedConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			bool reverse = (parameter is string && ((string)parameter) == "reverse");
			if (value is string)
			{
				bool v = string.IsNullOrEmpty((string)value);
				if (reverse) v = !v;
				return v ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
			}
			if (value is IEnumerable)
			{
				var e = (IEnumerable)value;
				if(e.GetEnumerator().MoveNext())
					return reverse ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
			}
			return reverse ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
