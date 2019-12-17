using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace KmlViewer
{
	public class TimeOfDayTimeConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return new DateTime(2015, 1, 1).AddHours((double)value).ToString("t");
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
