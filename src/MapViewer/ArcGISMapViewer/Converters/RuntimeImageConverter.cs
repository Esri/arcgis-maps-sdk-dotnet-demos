using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.UI;
using Microsoft.UI.Xaml.Data;

namespace ArcGISMapViewer.Converters
{
    public sealed partial class RuntimeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is RuntimeImage image)
            {
                Microsoft.UI.Xaml.Media.Imaging.BitmapImage bmi = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                LoadImage(bmi, image);
                return bmi;
            }
            return value;
        }
        private async static void LoadImage(Microsoft.UI.Xaml.Media.Imaging.BitmapSource bmi, RuntimeImage image)
        {
            try
            {
                var buffer = await image.GetEncodedBufferAsync();
                await bmi.SetSourceAsync(buffer.AsRandomAccessStream());
            }
            catch { }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
