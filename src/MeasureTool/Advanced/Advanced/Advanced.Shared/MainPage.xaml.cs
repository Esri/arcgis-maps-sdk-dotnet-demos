using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Advanced
{
    /// <summary>
    /// This sample demonstrates how to the Editor is used sketching geometries
    /// and how it can be controlled to allow you to interact with the map without 
    /// affecting the geometry being digitized. In addition, it shows how you can
    /// customize the symbology used by the edit tools and track the sketch progress.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<string> measurements;

        public MainPage()
        {
            this.InitializeComponent();
            measurements = new ObservableCollection<string>();
            Measurements.ItemsSource = measurements;
        }

        private async void MeasureButton_Click(object sender, RoutedEventArgs e)
        {
            string message = null;
            try
            {
                var symbol = this.Resources["PolylineSymbol"] as LineSymbol;
                var progress = new Progress<GeometryEditStatus>(OnStatusUpdated);
                var geometry = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline, symbol, progress);            
                var layer = MyMapView.Map.Layers["ResultLayer"] as GraphicsLayer;
                layer.Graphics.Clear();
                measurements.Clear();
                TotalLength.Text = TotalArea.Text = string.Empty;
            }
            catch (TaskCanceledException tc)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private void OnStatusUpdated(GeometryEditStatus status)
        {
            measurements.Clear();
            var polyline = status.NewGeometry as Polyline;
            if (polyline == null || polyline.Parts == null || polyline.Parts.Count == 0)
                return;
            var vertices = polyline.Parts[0].GetPoints();
            var count = vertices != null ? vertices.Count() : 0;
            if (count <= 1)
                return;
            var result = new StringBuilder();
            MapPoint previous = null;
            int i = 1;
            foreach (var point in vertices)
            {
                if (previous == null)
                {
                    previous = point;
                    continue;
                }
                var lineSegment = new Polyline(new MapPoint[] { previous, point }, polyline.SpatialReference);
                var intermediateLength = GeometryEngine.GeodesicLength(lineSegment);
                measurements.Add(string.Format("[{0}-{1}]\t:\t{2:0} m\n", i, i + 1, intermediateLength));
                i++;
            }
            var totalLength = GeometryEngine.GeodesicLength(polyline);
            TotalLength.Text = string.Format("Total Length\t:\t{0:0} m\n", totalLength);
            if (count <= 2)
                return;
            var layer = MyMapView.Map.Layers["ResultLayer"] as GraphicsLayer;
            var graphic = layer.Graphics.FirstOrDefault();
            var polygon = new Polygon(vertices, polyline.SpatialReference);
            if (graphic != null)
                graphic.Geometry = polygon;
            else
                layer.Graphics.Add(new Graphic() { Geometry = polygon });
            if (count <= 1)
                return;
            if (count <= 2)
                return;
            var area = GeometryEngine.GeodesicArea(polygon);           
            TotalArea.Text = string.Format("Area\t\t:\t{0:0} m²\n", area);
        }

        private void SuspendButton_Click(object sender, RoutedEventArgs e)
        {
            var isSuspended = MyMapView.Editor.IsSuspended;
            (sender as AppBarButton).Icon = isSuspended ? new SymbolIcon(Windows.UI.Xaml.Controls.Symbol.Pause) : new SymbolIcon(Windows.UI.Xaml.Controls.Symbol.Play);
                (sender as AppBarButton).Label = isSuspended ? "Suspend" : "Resume";
            MyMapView.Editor.IsSuspended = !isSuspended;            
        }
    }

    public class MeasureEditor : Editor
    {
        protected override Esri.ArcGISRuntime.Symbology.Symbol OnGenerateSymbol(GenerateSymbolInfo generateSymbolInfo)
        {
            if(generateSymbolInfo.GenerateSymbolType == GenerateSymbolType.Vertex || generateSymbolInfo.GenerateSymbolType == GenerateSymbolType.SelectedVertex)
            {
                var index = generateSymbolInfo.VertexPosition.CoordinateIndex + 1;
                return new CompositeSymbol()
                {
                    Symbols = new SymbolCollection(new Esri.ArcGISRuntime.Symbology.Symbol[]
                        {
                            new SimpleMarkerSymbol()
                            {
                                Color = Colors.White,
                                Size = 14.5,
                                Outline = new SimpleLineSymbol() {Width = 1.5, Color = Colors.CornflowerBlue},
                            },
                            new TextSymbol()
                            {
                                Text =
                                    Convert.ToString(index, CultureInfo.InvariantCulture),
                                HorizontalTextAlignment = HorizontalTextAlignment.Center,
                                VerticalTextAlignment = VerticalTextAlignment.Middle
                            },
                        })
                };
            }
            return base.OnGenerateSymbol(generateSymbolInfo);
        }
    }
}
