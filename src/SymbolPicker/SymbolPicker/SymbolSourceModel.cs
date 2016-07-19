namespace SymbolPicker
{
#if NETFX_CORE
    using Windows.UI.Xaml.Media;
#elif !XAMARIN
    using System.Windows.Media;
#else
#if __ANDROID__
    using ImageSource = Android.Graphics.Bitmap;
#elif __IOS__
using ImageSource = UIKit.UIImage;
#endif

#endif
    using Esri.ArcGISRuntime.Symbology;

    public sealed class SymbolSourceModel
    {
        public SymbolSourceModel(ImageSource source, StyleSymbolSearchResult result)
        {
            this.Source = source;
            this.Symbol = result.Symbol;
            this.Category = string.Format("Category: {0}", result.Category);
            this.Key = string.Format("Key: {0}", result.Key);
            this.Name = string.Format("Name: {0}", result.Name);
            this.SymbolClass = string.Format("SymbolClass: {0}", result.SymbolClass);
            this.Tags = string.Format("Tags: {0}", string.Join(";", result.Tags));
        }

        public CimSymbol Symbol { get; private set; }

        public ImageSource Source { get; private set; }

        public string Category { get; private set; }

        public string Key { get; private set; }

        public string Name { get; private set; }

        public string SymbolClass { get; private set; }

        public string Tags { get; private set; }
    }
}