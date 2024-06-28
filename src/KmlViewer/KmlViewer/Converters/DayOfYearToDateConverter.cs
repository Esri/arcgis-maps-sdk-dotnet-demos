using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Data;

namespace KmlViewer
{
    public class DayOfYearToDateConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return new DateTime(2015, 1, 1).AddDays((int)(double)value).ToString("M");
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
