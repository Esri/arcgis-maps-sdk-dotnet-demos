using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace OfflineWorkflowsSample.Infrastructure.Converter
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueBool = (bool) value;
            if (valueBool)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}