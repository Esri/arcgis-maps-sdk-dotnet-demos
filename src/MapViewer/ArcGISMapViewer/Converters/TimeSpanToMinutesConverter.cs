using Microsoft.UI.Xaml.Data;

namespace ArcGISMapViewer.Converters
{
    public partial class TimeSpanToMinutesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan ts)
                return ts.TotalMinutes;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
                return TimeSpan.FromMinutes(d);
            return value;
        }
    }
}
