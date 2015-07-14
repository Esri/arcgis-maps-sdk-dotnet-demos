using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace KmlViewer
{
    public class MeterFormatConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is double)
			{
				var meters = (double)value;
				if (double.IsNaN(meters))
					return "";

				var miles = Esri.ArcGISRuntime.Geometry.LinearUnits.Miles.ConvertFromMeters(meters);
				if (miles > 1)
				{
					if (miles > 10)
						return string.Format("{0:0}mi", miles);
					else
						return string.Format("{0:0.0}mi", miles);
				}
				var feet = Esri.ArcGISRuntime.Geometry.LinearUnits.Feet.ConvertFromMeters(meters);
				return string.Format("{0:0}ft", feet);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
