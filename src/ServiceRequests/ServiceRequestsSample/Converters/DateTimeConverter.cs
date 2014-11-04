using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ServiceRequestsSample.Converters
{
	/// <summary>
	/// DatePicker uses DateTimeOffset type in Date property insetead of DateTime so we need to 
	/// convert DateTime used by FeatureDataField to DateTimeOffset and back.
	/// </summary>
	public class DateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			try
			{
				DateTime date = (DateTime)value;
				return new DateTimeOffset(date);
			}
			catch (Exception ex)
			{
				return DateTimeOffset.MinValue;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			try
			{
				DateTimeOffset dto = (DateTimeOffset)value;
				return dto.DateTime;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}
}
