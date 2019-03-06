using System;
using Windows.UI.Xaml.Data;

namespace OfflineWorkflowSample.Infrastructure.Converter
{
    class NullEnumToBlankStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return value;
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}