namespace SymbolPicker
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using Esri.ArcGISRuntime.Geometry;
    using Esri.ArcGISRuntime.Mapping;
    using Esri.ArcGISRuntime.Symbology;
    using Esri.ArcGISRuntime.UI;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CimSymbol symbol;
        private IList<MapPoint> parts = new List<MapPoint>();
        private Geometry geometry;

        public MainWindow()
        {
            this.InitializeComponent();
            var vm = this.MyMapView.DataContext as SearchViewModel;
            vm.PropertyChanged += this.SearchViewModel_PropertyChanged;
            this.MyMapView.Map = new Map(Basemap.CreateTopographic());
            this.MyMapView.GeoViewTapped += this.MyMapView_GeoViewTapped;
            this.MyMapView.GeoViewDoubleTapped += this.MyMapView_GeoViewDoubleTapped;
        }

        private void MyMapView_GeoViewDoubleTapped(object sender, GeoViewInputEventArgs e)
        {
            this.geometry = null;
            if (this.symbol == null)
            {
                return;
            }

            e.Handled = true;
            if (this.symbol is CimLineSymbol && this.parts.Count > 1)
            {
                this.geometry = new Polyline(this.parts);
            }
            else if (this.symbol is CimPolygonSymbol && this.parts.Count > 2)
            {
                this.geometry = new Polygon(this.parts);
            }

            if (this.geometry == null || this.geometry.IsEmpty)
            {
                return;
            }

            this.parts.Clear();
            var overlay = this.MyMapView.GraphicsOverlays.First();
            overlay.Graphics.Add(new Graphic(this.geometry, this.symbol));
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            this.geometry = null;
            if (this.symbol == null)
            {
                return;
            }

            e.Handled = true;
            if (this.symbol is CimPointSymbol)
            {
                this.geometry = e.Location;
            }
            else if (this.symbol is CimLineSymbol || this.symbol is CimPolygonSymbol)
            {
                this.parts.Add(e.Location);
            }

            if (this.geometry == null || this.geometry.IsEmpty)
            {
                return;
            }

            var overlay = this.MyMapView.GraphicsOverlays.First();
            overlay.Graphics.Add(new Graphic(this.geometry, this.symbol));
        }

        private void SearchViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedResult")
            {
                var result = ((SearchViewModel)sender).SelectedResult;
                this.symbol = result?.Symbol;
                this.geometry = null;
                this.parts.Clear();
                if (result == null)
                {
                    foreach (var overlay in this.MyMapView.GraphicsOverlays)
                    {
                        overlay.Graphics.Clear();
                    }
                }
            }
        }
    }
}
