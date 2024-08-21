using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Controls
{

    public sealed partial class MapPropertiesView : UserControl
    {
        public class ShowMapPropertiesMessage
        {
            public ShowMapPropertiesMessage(Layer layer)
            {
                Layer = layer;
            }
            public Layer Layer { get; }
        }
        public MapPropertiesView()
        {
            this.InitializeComponent();

            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<ShowMapPropertiesMessage>(this, (r, m) =>
            {
                ContentPicker.SelectedItem = m.Layer;
            });
        }

        private void MapContentsPicker_SelectedItemChanged(object? sender, object? e)
        {
            PickerFlyout.Hide();
        }

        public GeoModel GeoModel
        {
            get { return (GeoModel)GetValue(GeoModelProperty); }
            set { SetValue(GeoModelProperty, value); }
        }

        public static readonly DependencyProperty GeoModelProperty =
            DependencyProperty.Register("GeoModel", typeof(GeoModel), typeof(MapPropertiesView), new PropertyMetadata(null));
    }
    public class LayerTemplateSelector : DataTemplateSelector
    {
        public LayerTemplateSelector()
        {

        }
        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Esri.ArcGISRuntime.Data.FeatureTable)
                return FeatureTableTemplate;
            if (item is FeatureLayer)
                return FeatureLayerTemplate;
            else if (item is Layer)
                return LayerTemplate;
            else if (item is ILayerContent)
                return LayerContentTemplate;
            return base.SelectTemplateCore(item, container);
        }
        public DataTemplate? FeatureLayerTemplate { get; set; }
        public DataTemplate? FeatureTableTemplate { get; set; }
        public DataTemplate? LayerTemplate { get; set; }
        public DataTemplate? LayerContentTemplate { get; set; }
    }
}
