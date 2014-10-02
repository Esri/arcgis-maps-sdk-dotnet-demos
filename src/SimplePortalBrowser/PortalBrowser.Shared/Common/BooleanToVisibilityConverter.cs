using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace PortalBrowser.Common
{
	/// <summary>
	/// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
	/// <see cref="Visibility.Collapsed"/>.
	/// </summary>
	public sealed class BooleanToVisibilityConverter : BaseValueConverter
	{
		protected override object Convert(object value, Type targetType, object paramter, string language)
		{
			return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
		}

		protected override object ConvertBack(object value, Type targetType, object paramter, string language)
		{
			return value is Visibility && (Visibility)value == Visibility.Visible;
		}
	}
}
