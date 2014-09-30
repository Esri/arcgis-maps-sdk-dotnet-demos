using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Data;
using System.Windows;
#endif

namespace LocalNetworkSample.Common
{
	public sealed class NullToCollapsedConverter : IValueConverter
    {
#if NETFX_CORE
		public object Convert(object value, Type targetType, object parameter, string language)
#else
       public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
		{
			if (value is string && string.IsNullOrEmpty((string)value))
				return Visibility.Collapsed;
			return (value == null) ? Visibility.Collapsed : Visibility.Visible;
		}
#if NETFX_CORE
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotSupportedException();
        }
	}
}
