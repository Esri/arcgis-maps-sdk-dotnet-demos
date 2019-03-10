using System;
using System.Diagnostics;
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
            if (value is DateTimeOffset dto)
            {
                DateTimeOffset date = (DateTimeOffset) value;
                return date.LocalDateTime.ToShortDateString();
            } else if (value is DateTime dt)
            {
                return dt.ToShortDateString();
            }

            Debug.WriteLine($"This shouldn't happen. {value} passed to {nameof(DateTimeToStringConverter)}.");
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}