﻿using System;
#if MAUI
using System.Globalization;
#elif NETFX_CORE
using Windows.UI.Xaml.Data;
#else
using System.Globalization;
using System.Windows.Data;
#endif

namespace RoutingSample.Converters
{
	/// <summary>
	/// Base converter class for handling converter differences between .NET and Windows Runtime
	/// </summary>
    public abstract class ValueConverter : IValueConverter
    {
#if NETFX_CORE
		object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
#else
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
		{
#if !NETFX_CORE
			string language = culture.TwoLetterISOLanguageName;
#endif
			return Convert(value, targetType, parameter, language);
		}

		protected abstract object Convert(object value, Type targetType, object parameter, string language);

#if NETFX_CORE
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
#else
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif
		{
#if !NETFX_CORE
			string language = culture.TwoLetterISOLanguageName;
#endif
			return ConvertBack(value, targetType, parameter, language);
		}

		protected abstract object ConvertBack(object value, Type targetType, object parameter, string language);
	}

	public abstract class StringFormatter : IValueConverter
	{
		protected abstract string Format(object value, object parameter, string language);

#if NETFX_CORE
		object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
#else
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
		{
#if !NETFX_CORE
			string language = culture.TwoLetterISOLanguageName;
#endif

			return Format(value, parameter, language);
		}

#if NETFX_CORE
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
#else
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif
		{
			throw new NotSupportedException();
		}
	}
}
