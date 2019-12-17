using System;
using System.Drawing;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace OfflineWorkflowSample.Converters
{
    class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}