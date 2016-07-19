#if !XAMARIN
namespace SymbolPicker
{
    using System;
#if NETFX_CORE
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
#else
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
#endif

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
#if NETFX_CORE
            string language)
#else
            CultureInfo culture)
#endif
        {
            if (value is bool && targetType == typeof(Visibility))
            {
                return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
#if NETFX_CORE
            string language)
#else
            CultureInfo culture)
#endif
        {
            if (value is Visibility && targetType == typeof(bool))
            {
                return ((Visibility)value == Visibility.Visible) ? true : false;
            }

            return value;
        }
    }
}
#endif