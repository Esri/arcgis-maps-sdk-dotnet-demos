using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace KmlViewer
{
    public class TrueToVisibleConverter : IValueConverter
    {

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is bool)
			{
				bool b = (bool)value;
				if (parameter is string && ((string)parameter) == "reverse")
					b = !b;
				if (b)
					return Windows.UI.Xaml.Visibility.Visible;
			}
			return Windows.UI.Xaml.Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return ((Windows.UI.Xaml.Visibility)value == Windows.UI.Xaml.Visibility.Visible);
		}
	}
}
