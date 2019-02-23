using Esri.ArcGISRuntime.Ogc;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KmlViewer.Controls
{
    public class KmlFeatureItemView : Control
    {
        public KmlFeatureItemView()
        {
            DefaultStyleKey = typeof(KmlFeatureItemView);
        }

        public KmlNode KmlFeature
        {
            get { return (KmlNode)GetValue(KmlFeatureProperty); }
            set { SetValue(KmlFeatureProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KmlFeature.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KmlFeatureProperty =
            DependencyProperty.Register("KmlFeature", typeof(KmlNode), typeof(KmlFeatureItemView), new PropertyMetadata(null));
    }
}