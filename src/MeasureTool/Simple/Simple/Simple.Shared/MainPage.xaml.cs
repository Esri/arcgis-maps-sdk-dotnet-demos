using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Simple
{
    /// <summary>
    /// This sample demonstrates how to the Editor is used sketching geometries
    /// and how it can be controlled to allow you to interact with the map without 
    /// affecting the geometry being digitized.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();            
        }

        private async void MeasureButton_Click(object sender, RoutedEventArgs e)
        {
            string message = null;
            try
            {
                var geometry = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline);
                var polyline = geometry as Polyline;
                if (polyline != null && polyline.Parts != null && polyline.Parts.Count > 0)
                {
                    var vertices = polyline.Parts[0].GetPoints();
                    var count = vertices != null ? vertices.Count() : 0;
                    if (count > 1)
                    {
                        var result = new StringBuilder();
                        var totalLength = GeometryEngine.GeodesicLength(polyline);
                        result.AppendFormat("Total Length\t:\t{0:0} m\n", totalLength);
                        if (count > 2)
                        {
                            var polygon = new Polygon(polyline.Parts, polyline.SpatialReference);
                            var area = GeometryEngine.GeodesicArea(polygon);
                            result.AppendFormat("Area\t\t:\t{0:0} m²\n", area);
                        }
                        if (result.Length > 0)
                            message = string.Format("Measure results:\n{0}", result);
                    }
                }
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

        private void SuspendButton_Click(object sender, RoutedEventArgs e)
        {
            var isSuspended = MyMapView.Editor.IsSuspended;
            (sender as AppBarButton).Icon = isSuspended ? new SymbolIcon(Symbol.Pause) : new SymbolIcon(Symbol.Play);
            (sender as AppBarButton).Label = isSuspended ? "Suspend" : "Resume";
            MyMapView.Editor.IsSuspended = !isSuspended;
        }
    }
}
