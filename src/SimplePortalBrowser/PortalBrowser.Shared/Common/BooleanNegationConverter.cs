using System;

namespace PortalBrowser.Common
{
	/// <summary>
	/// Value converter that translates true to false and vice versa.
	/// </summary>
	public sealed class BooleanNegationConverter : BaseValueConverter
	{
		protected override object Convert(object value, Type targetType, object paramter, string language)
		{
			return !(value is bool && (bool)value);
		}

		protected override object ConvertBack(object value, Type targetType, object paramter, string language)
		{
			return !(value is bool && (bool)value);
		}

	}
}
