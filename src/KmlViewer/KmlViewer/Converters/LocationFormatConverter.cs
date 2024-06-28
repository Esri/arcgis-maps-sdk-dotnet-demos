using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Data;

namespace KmlViewer
{
	public class LocationFormatConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is Envelope)
				value = ((Envelope)value).GetCenter();
			if (value is MapPoint)
			{
				var p = (MapPoint)value;
				if (!p.SpatialReference.IsGeographic) //Convert to long/lat
					p = GeometryEngine.Project(p, p.SpatialReference.BaseGeographic) as MapPoint;
				if (p == null)
					return "";
				double lat = p.Y;
				double lon = p.X;
				char ns = lat < 0 ? 'S' : 'N';
				char ew = lon < 0 ? 'W' : 'E';
				lon = Math.Abs(lon);
				lat = Math.Abs(lat);
				var majorLong = Math.Floor(lon);
				var majorLat = Math.Floor(lat);
				var minorLong = (lon - majorLong) * 60;
				var minorLat = (lat - majorLat) * 60;
				var minutesLong = Math.Floor(minorLong);
				var minutesLat = Math.Floor(minorLat);
				var secondsLong = Math.Floor((minorLong - minutesLong) * 60);
				var secondsLat = Math.Floor((minorLat - minutesLat) * 60);
				return string.Format("{0}{1}°{2:00}'{3:00}\" {4}{5}°{6:00}'{7:00}\"",
					ns, majorLat, minutesLat, secondsLat,
					ew, majorLong, minutesLong, secondsLong);
			}
			return value;
		}
		

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
