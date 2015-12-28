using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Advanced
{
	/// <summary>
	/// This sample demonstrates how the Editor is used for sketching geometries
	/// and how to interact with the map without affecting the geometry being digitized. 
	/// This sample also shows how to override the default symbology as well as track the sketch progress.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private readonly ObservableCollection<string> _measurements;

		public MainPage()
		{
			this.InitializeComponent();
			_measurements = new ObservableCollection<string>();
			Measurements.ItemsSource = _measurements;
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
				if (layer == null)
					return;
				layer.Graphics.Clear();
				_measurements.Clear();
				TotalLength.Text = TotalArea.Text = string.Empty;
			}
			catch (TaskCanceledException)
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
			_measurements.Clear();
			var polyline = status.NewGeometry as Polyline;
			if (polyline == null || polyline.Parts == null || polyline.Parts.Count == 0)
				return;
			var vertices = polyline.Parts[0].GetPoints();
			if (vertices == null || !vertices.Any())
				return;
			var count = vertices.Count();
			if (count <= 1)
				return;
			MapPoint previous = null;
			int i = 1;
			foreach (var point in vertices)
			{
				if (previous == null)
				{
					previous = point;
					continue;
				}
				var lineSegment = new Polyline(new MapPoint[] {previous, point}, polyline.SpatialReference);
				var intermediateLength = GeometryEngine.GeodesicLength(lineSegment);
				_measurements.Add(string.Format("[{0}-{1}]\t:\t{2:0} m\n", i, i + 1, intermediateLength));
				previous = point;
				i++;
			}
			var totalLength = GeometryEngine.GeodesicLength(polyline);
			TotalLength.Text = string.Format("Total Length\t:\t{0:0} m\n", totalLength);
			if (count <= 2)
				return;
			var layer = MyMapView.Map.Layers["ResultLayer"] as GraphicsLayer; 
			if (layer == null)
				return;
			var graphic = layer.Graphics.FirstOrDefault();
			var polygon = new Polygon(vertices, polyline.SpatialReference);
			if (graphic != null)
				graphic.Geometry = polygon;
			else
				layer.Graphics.Add(new Graphic(polygon));
			if (count <= 1)
				return;
			if (count <= 2)
				return;
			var area = GeometryEngine.GeodesicArea(polygon);
			TotalArea.Text = string.Format("Area\t\t:\t{0:0} m²\n", area);
		}

		private void SuspendButton_Click(object sender, RoutedEventArgs e)
		{
			var button = (AppBarButton) sender;
			var isSuspended = MyMapView.Editor.IsSuspended;
			button.Icon = isSuspended
				? new SymbolIcon(Windows.UI.Xaml.Controls.Symbol.Pause)
				: new SymbolIcon(Windows.UI.Xaml.Controls.Symbol.Play);
			button.Label = isSuspended ? "Suspend" : "Resume";
			MyMapView.Editor.IsSuspended = !isSuspended;
		}
	}

	public class MeasureEditor : Editor
	{
		protected override Esri.ArcGISRuntime.Symbology.Symbol OnGenerateSymbol(GenerateSymbolInfo generateSymbolInfo)
		{
			if (generateSymbolInfo.GenerateSymbolType == GenerateSymbolType.Vertex ||
			    generateSymbolInfo.GenerateSymbolType == GenerateSymbolType.SelectedVertex)
			{
				var index = generateSymbolInfo.VertexPosition.CoordinateIndex + 1;
				return new CompositeSymbol()
				{
					Symbols = new SymbolCollection(new Esri.ArcGISRuntime.Symbology.Symbol[]
					{
						new SimpleMarkerSymbol
						{
							Color = Colors.White,
							Size = 14.5,
							Outline = new SimpleLineSymbol() {Width = 1.5, Color = Colors.CornflowerBlue},
						},
						new TextSymbol
						{
							Text =
								Convert.ToString(index, CultureInfo.InvariantCulture),
							HorizontalTextAlignment = HorizontalTextAlignment.Center,
							VerticalTextAlignment = VerticalTextAlignment.Middle
						}
					})
				};
			}
			return base.OnGenerateSymbol(generateSymbolInfo);
		}
	}
}