using System;
#if XAMARIN
using System.Globalization;
using Xamarin.Forms;
#elif WINDOWS_UWP
using Windows.UI.Xaml.Data;
#elif WINDOWS_WPF
using System.Globalization;
using System.Windows.Data;
#endif

namespace RoutingSample.Converters
{
	/// <summary>
	/// Base converter class for handling converter differences between .NET and Windows Runtime
	/// </summary>
    public abstract class ValueConverterBase : IValueConverter
    {
#if WINDOWS_UWP
		object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
#else
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
		{
#if !WINDOWS_UWP
			string language = culture.TwoLetterISOLanguageName;
#endif
			return Convert(value, targetType, parameter, language);
		}

		protected abstract object Convert(object value, Type targetType, object parameter, string language);

#if WINDOWS_UWP
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
#else
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif
		{
#if !WINDOWS_UWP
			string language = culture.TwoLetterISOLanguageName;
#endif
			return ConvertBack(value, targetType, parameter, language);
		}

		protected abstract object ConvertBack(object value, Type targetType, object parameter, string language);
	}
}
