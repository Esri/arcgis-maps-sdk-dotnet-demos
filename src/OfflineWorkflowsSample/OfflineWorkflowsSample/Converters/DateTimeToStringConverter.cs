using System;
using Windows.UI.Xaml.Data;

namespace OfflineWorkflowSample.Converters
{
    /// <summary>
    /// Format .NET dates nicely for display.
    /// </summary>
    class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset date = (DateTimeOffset) value;
            return date.DateTime.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}