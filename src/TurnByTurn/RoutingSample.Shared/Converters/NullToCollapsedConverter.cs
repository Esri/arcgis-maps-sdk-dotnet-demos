using System;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#elif WINDOWS_WPF
using System.Windows;
#endif

namespace RoutingSample.Converters
{
#if WINDOWS_WPF || WINDOWS_UWP
    public class NullToCollapsedConverter : ValueConverterBase
	{
		protected override object Convert(object value, Type targetType, object parameter, string language)
		{
			return value == null ? Visibility.Collapsed : Visibility.Visible;
		}

		protected override object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
#endif
}
