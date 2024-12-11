using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System.Globalization;
using System.Windows.Data;

namespace EditorDemo
{
    public class LayerTypeToNameConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FeatureLayer featureLayer)
            {
                return featureLayer.FeatureTable?.DisplayName;
            }
            else if (value is GraphicsOverlay graphicsOverlay)
            {
                return graphicsOverlay.Id;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}