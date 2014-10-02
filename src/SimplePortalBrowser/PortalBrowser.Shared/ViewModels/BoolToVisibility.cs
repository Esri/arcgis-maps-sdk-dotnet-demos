using System;

#if NETFX_CORE
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace PortalBrowser.ViewModels
{
	public class BoolToVisibility : IValueConverter
	{
#if NETFX_CORE
		public object Convert(object value, Type targetType, object parameter, string language)
#else
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
		{
			if (value is bool)
			{
				bool val = (bool)value;
				return val ? Visibility.Visible : Visibility.Collapsed;
			}
			return value;
		}

#if NETFX_CORE
		public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
		{
			throw new NotImplementedException();
		}
	}
}
