using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SymbolEditorApp.Controls
{
    /// <summary>
    /// Interaction logic for GeometryEditor.xaml
    /// </summary>
    public partial class GeometryEditor : MetroWindow
    {
        public GeometryEditor()
        {
            InitializeComponent();
            mapView.BackgroundGrid.Color = System.Drawing.Color.White;
            mapView.BackgroundGrid.GridLineWidth = .5f;
            mapView.Map = new Esri.ArcGISRuntime.Mapping.Map(SpatialReferences.Wgs84) { InitialViewpoint = new Esri.ArcGISRuntime.Mapping.Viewpoint(new Envelope(0, 0, 10, 10, SpatialReferences.Wgs84)) };
            mapView.InteractionOptions = new Esri.ArcGISRuntime.UI.MapViewInteractionOptions() { IsZoomEnabled = false, IsPanEnabled = false };
            mapView.GraphicsOverlays.Add(new Esri.ArcGISRuntime.UI.GraphicsOverlay() { Renderer = new SimpleRenderer(new SimpleFillSymbol() { Color = System.Drawing.Color.Red }) });
            this.Loaded += GeometryEditor_Loaded;
        }

        private async void GeometryEditor_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var geometry = await mapView.SketchEditor.StartAsync(Esri.ArcGISRuntime.UI.SketchCreationMode.FreehandPolygon, false);
                mapView.GraphicsOverlays[0].Graphics.Clear();
                mapView.GraphicsOverlays[0].Graphics.Add(new Esri.ArcGISRuntime.UI.Graphic(geometry));
                GeometryEditor_Loaded(sender, e);
            }
            catch(System.Threading.Tasks.TaskCanceledException)
            {
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        public Esri.ArcGISRuntime.Geometry.Geometry   Geometry => mapView.GraphicsOverlays[0].Graphics.FirstOrDefault()?.Geometry;
    }
}
