using System;

namespace RoutingSample.Converters
{
	public class TimeSpanFormatter : BaseValueConverter
	{
		protected override object Convert(object value, Type targetType, object parameter, string language)
		{
            try
            {
                if (value is TimeSpan)
                {
                    var ts = (TimeSpan)value;
                    if (ts.TotalDays > 1)
                        return $"{ts.Days} days {ts.Hours} h";
                    if (ts.TotalHours > 1)
                        return string.Format("{0:hh}:{0:mm}", ts);
                    if (ts.TotalSeconds > 0)
                        return string.Format("{0:mm}:{0:ss}", ts);
                    else
                        return ts.ToString();
                }
                return value;
            }
            catch(System.Exception ex)
            {
                return null;
            }
		}

		protected override object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
