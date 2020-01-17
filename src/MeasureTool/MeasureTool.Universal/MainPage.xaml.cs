using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MeasureTool.Universal
{
    /// <summary>
    /// This sample demonstrates how the Editor is used for sketching geometries
    /// and how to interact with the map without affecting the geometry being digitized.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
			MyMapView.SketchEditor.GeometryChanged += SketchEditor_GeometryChanged;
			MyMapView.SketchEditor.IsEnabled = false;
		}

		private void SketchEditor_GeometryChanged(object sender, Esri.ArcGISRuntime.UI.GeometryChangedEventArgs e)
		{
			var polyline = e.NewGeometry as Polyline;
			var result = new StringBuilder();
			var totalLength = GeometryEngine.LengthGeodetic(polyline);
			result.AppendFormat("Total Length\t:\t{0:0} m\n", totalLength);
			var polygon = new Polygon(polyline.Parts, polyline.SpatialReference);
			var area = GeometryEngine.AreaGeodetic(polygon);
			result.AppendFormat("Area\t\t:\t{0:0} m²\n", area);
			measurementOutput.Text = result.ToString();
		}

		private async void MeasureButton_Click(object sender, RoutedEventArgs e)
	    {
		    try
		    {
				pauseButton.IsEnabled = true;
				pauseButton.Icon = new SymbolIcon(Symbol.Pause);
				pauseButton.Label = "Suspend";
				MyMapView.SketchEditor.IsEnabled = true;
				measurementOutput.Text = "Click the map to begin the measure";
				var geometry = await MyMapView.SketchEditor.StartAsync(Esri.ArcGISRuntime.UI.SketchCreationMode.Polyline);
			    var polyline = geometry as Polyline;
				if (polyline != null && polyline.Parts != null && polyline.Parts.Count > 0)
				{
					var result = new StringBuilder();
					var totalLength = GeometryEngine.LengthGeodetic(polyline);
					result.AppendFormat("Total Length\t:\t{0:0} m\n", totalLength);
					var polygon = new Polygon(polyline.Parts, polyline.SpatialReference);
					var area = GeometryEngine.AreaGeodetic(polygon);
					result.AppendFormat("Area\t\t:\t{0:0} m²\n", area);
					measurementOutput.Text = result.ToString();
				}
		    }
		    catch (TaskCanceledException)
		    {
		    }
		    catch (Exception ex)
		    {
				measurementOutput.Text = ex.Message;
		    }
			pauseButton.IsEnabled = false;
			pauseButton.Icon = new SymbolIcon(Symbol.Play);
	    }

	    private void SuspendButton_Click(object sender, RoutedEventArgs e)
	    {
		    var button = (AppBarButton) sender;
		    var isSuspended = !MyMapView.SketchEditor.IsEnabled;
		    button.Icon = isSuspended ? new SymbolIcon(Symbol.Pause) : new SymbolIcon(Symbol.Play);
		    button.Label = isSuspended ? "Suspend" : "Resume";
		    MyMapView.SketchEditor.IsEnabled = isSuspended;
	    }
    }
}
