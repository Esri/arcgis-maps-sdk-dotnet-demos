using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Data;

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
					return Microsoft.UI.Xaml.Visibility.Visible;
			}
			return Microsoft.UI.Xaml.Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return ((Microsoft.UI.Xaml.Visibility)value == Microsoft.UI.Xaml.Visibility.Visible);
		}
	}
}
