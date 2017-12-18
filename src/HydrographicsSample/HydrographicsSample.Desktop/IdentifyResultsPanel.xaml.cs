using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace HydrographicsSample
{
    /// <summary>
    /// A panel that shows a set of identify results
    /// </summary>
    public partial class IdentifyResultsPanel : UserControl
    {
        private GraphicsOverlay go;
        private Action Unselect;

        public IdentifyResultsPanel()
        {
            InitializeComponent();
        }

        public void Show(IReadOnlyList<IdentifyLayerResult> results, GraphicsOverlay highlightLayer)
        {
            go = highlightLayer;
            Items.SelectedItem = null;
            Items.ItemsSource = null;
            ClearSelection();
            List<IdentifyResultItem> list = new List<IdentifyResultItem>();
            foreach (var layerResult in results)
            {
                foreach(var result in layerResult.GeoElements)
                {
                    list.Add(new IdentifyResultItem() { Element = result, Container = layerResult.LayerContent });
                    foreach(var sublayerResults in layerResult.SublayerResults)
                    {
                        foreach (var subresult in sublayerResults.GeoElements)
                        {
                            list.Add(new IdentifyResultItem() { Element = result, Container = sublayerResults.LayerContent });
                        }
                    }
                }
            }
            Items.ItemsSource = list;
            this.Visibility = Visibility.Visible;
            Opened?.Invoke(this, EventArgs.Empty);
            if (results.Count == 0)
            {
                attributeView.Text = "No results";
            }
        }

        private void ClearSelection()
        {
            if (Unselect != null)
            {
                Unselect();
                Unselect = null;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            this.Visibility = Visibility.Hidden;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Closed;
        public event EventHandler Opened;
        public event EventHandler<GeoElement> ZoomButtonClicked;

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            attributeView.Text = "";
            attributeView.Inlines.Clear();
            ClearSelection();
            var item = Items.SelectedItem as IdentifyResultItem;
            if(item != null)
            {

                var encFeature = item.Element as Esri.ArcGISRuntime.Hydrography.EncFeature;
                foreach (var attr in item.Element.Attributes)
                {
                    string keyName = attr.Key;
                    if (encFeature != null)
                        keyName = encFeature.GetAttributeDescription(keyName);
                    attributeView.Inlines.Add(new Run(keyName + ":\n") { FontWeight = FontWeights.Bold });
                    
                    attributeView.Inlines.Add(new Run(attr.Value?.ToString() + "\n"));
                }
                if(item.Container is FeatureLayer)
                {
                    ((FeatureLayer)item.Container).ClearSelection();
                    ((FeatureLayer)item.Container).SelectFeature(item.Element as Feature);
                    Unselect = () => ((FeatureLayer)item.Container).UnselectFeature(item.Element as Feature);
                }
                else if (item.Element is Graphic)
                {
                    ((Graphic)item.Element).IsSelected = true;
                    Unselect = () => ((Graphic)item.Element).IsSelected = false;
                }
                else if(item.Element is Esri.ArcGISRuntime.Hydrography.EncFeature)
                {
                    ((EncLayer)item.Container).SelectFeature(encFeature);
                    Unselect = () => ((EncLayer)item.Container).ClearSelection();
                }
                else
                {
                    go.Graphics.Clear();
                    Graphic g = new Graphic(item.Element.Geometry, item.Element.Attributes) { IsSelected = true };
                    if (g.Geometry is MapPoint || g.Geometry is Multipoint)
                        g.Symbol = new SimpleMarkerSymbol() { Outline = new SimpleLineSymbol() { Color = Colors.Cyan, Width = 2 },  Color = Color.FromArgb(1, 0, 0, 0), Size = 20 };
                    else if (g.Geometry is Polyline || g.Geometry is Polygon || g.Geometry is Envelope)
                        g.Symbol = new SimpleLineSymbol() { Color = Color.FromArgb(1, 255, 255, 0), Width = 1 };
                    else
                    {

                    }
                    go.Graphics.Add(g);
                    Unselect = () => go.Graphics.Clear();
                }
            }
        }

        public class IdentifyResultItem
        {
            public string DisplayName
            {
                get
                {
                    if(Element is Esri.ArcGISRuntime.Hydrography.EncFeature)
                    {
                        var feature = (Element as Esri.ArcGISRuntime.Hydrography.EncFeature);
                        return $"{feature.Description} ({feature.Acronym})";
                        //return feature.GetAttributeDescription("Acronym");
                        //if (Container is EncLayer && Element.Attributes.ContainsKey("Acronym"))
                        //    return Element.Attributes["Acronym"]?.ToString();
                    }
                    else if (Element is Feature)
                    {
                        var f = (Feature)Element;
                        var fieldName = (f.FeatureTable as ArcGISFeatureTable)?.LayerInfo?.DisplayFieldName;
                        if (fieldName != null)
                            return f.Attributes[fieldName]?.ToString() ?? "Feature";
                    }
                    return Element.GetType().Name;
                }
            }
            public GeoElement Element { get; set; }
            public ILayerContent Container { get; set; }
        }

        private void ZoomTo_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as IdentifyResultItem;
            if(item != null)
            {
                Items.SelectedItem = item;
                ZoomButtonClicked(this, item.Element);
            }
        }
    }
}
