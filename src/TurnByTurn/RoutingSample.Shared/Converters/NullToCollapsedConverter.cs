using System;

#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;

#endif
namespace RoutingSample.Converters
{
	public class NullToCollapsedConverter : BaseValueConverter
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
}
