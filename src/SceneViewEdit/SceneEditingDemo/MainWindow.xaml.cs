using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using SceneEditingDemo.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime.UI.Controls;
using System.Linq;
using System;
using System.Globalization;

namespace SceneEditingDemo
{
    public partial class MainWindow : Window
	{
		GraphicsOverlay _pointsOverlay;
		GraphicsOverlay _polylinesOverlay;
		GraphicsOverlay _polygonsOverlay;

		// Store graphic that is being edited
		GraphicSelection _selection;

		public enum DrawShape
		{
			Point, Polyline, Polygon
		}

		public MainWindow()
		{
			InitializeComponent();
			DrawShapes.ItemsSource = new DrawShape[]
            {
                DrawShape.Point,
                DrawShape.Polyline,
                DrawShape.Polygon
            };
			DrawShapes.SelectedIndex = 0;

			_pointsOverlay = MySceneView.GraphicsOverlays["PointGraphicsOverlay"];
			_polylinesOverlay = MySceneView.GraphicsOverlays["PolylineGraphicsOverlay"];
			_polygonsOverlay = MySceneView.GraphicsOverlays["PolygonGraphicsOverlay"];

			EditButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel existing draw or edit operation.
            SceneEditHelper.Cancel();
        }

        private async void DrawButton_Click(object sender, RoutedEventArgs e)
        {
	        try
	        {
		        Geometry geometry = null; 
		        Graphic graphic = null;

                CancelButton.IsEnabled = true;
                ClearButton.IsEnabled = false;

                // Draw geometry and create a new graphic using it
                switch ((DrawShape)DrawShapes.SelectedValue)
		        {
			        case DrawShape.Point:
                        geometry = await SceneEditHelper.CreatePointAsync(MySceneView);
				        graphic = new Graphic(geometry);
				        _pointsOverlay.Graphics.Add(graphic);
				        break;
			        case DrawShape.Polygon:
				        geometry = await SceneEditHelper.CreatePolygonAsync(MySceneView);
				        graphic = new Graphic(geometry);
				        _polygonsOverlay.Graphics.Add(graphic);
				        break;
			        case DrawShape.Polyline:
				        geometry = await SceneEditHelper.CreatePolylineAsync(MySceneView);
				        graphic = new Graphic(geometry);
				        _polylinesOverlay.Graphics.Add(graphic);
				        break;
			        default:
				        break;
		        }
	        }
	        catch (TaskCanceledException)
	        {
                // This occurs if draw operation is canceled or new operation is started before previous was finished.
		        Debug.WriteLine("Previous draw operation was canceled.");
	        }			
            finally
            {
                CancelButton.IsEnabled = false;
                ClearButton.IsEnabled = true;
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
	        if (_selection == null) return; // Selection missing

            // Cancel previous edit
            if (SceneEditHelper.IsActive)
                SceneEditHelper.Cancel();

	        Geometry editedGeometry = null;

	        DrawButton.IsEnabled = false;
	        ClearButton.IsEnabled = false;
	        EditButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

	        try
	        {
                // Edit selected geometry and set it back to the selected graphic
		        switch (_selection.GeometryType)
		        {
			        case GeometryType.Point:
                        editedGeometry = await SceneEditHelper.CreatePointAsync(
                            MySceneView);
				        break;
			        case GeometryType.Polyline:
				        _selection.SetHidden(); // Hide selected graphic from the UI
                        editedGeometry = await SceneEditHelper.EditPolylineAsync(
                            MySceneView,
                            _selection.SelectedGraphic.Geometry as Polyline);
				        break;
			        case GeometryType.Polygon:
				        _selection.SetHidden(); // Hide selected graphic from the UI
                        editedGeometry = await SceneEditHelper.EditPolygonAsync(
                            MySceneView,
                            _selection.SelectedGraphic.Geometry as Polygon);
				        break;
			        default:
				        break;
		        }

		        _selection.SelectedGraphic.Geometry = editedGeometry; // Set edited geometry to selected graphic
	        }
	        catch (TaskCanceledException)
	        {
                // This occurs if draw operation is canceled or new operation is started before previous was finished.
                Debug.WriteLine("Previous edit operation was canceled.");
	        }
			finally
			{
				_selection.Unselect();
				_selection.SetVisible(); // Show selected graphic from the UI
                DrawButton.IsEnabled = true; 
				ClearButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
		}

		private void Clear_Click(object sender, RoutedEventArgs e)
		{
			// Clear any existing graphics
			_pointsOverlay.Graphics.Clear();
			_polylinesOverlay.Graphics.Clear();
			_polygonsOverlay.Graphics.Clear();

			if (_selection != null)
			{
				_selection.Unselect();
				_selection = null;
			}
			DrawButton.IsEnabled = true;
			EditButton.IsEnabled = false; 
		}

		private async void MySceneView_SceneViewTapped(object sender, GeoViewInputEventArgs e)
		{
			// If draw or edit is active, return
			if (SceneEditHelper.IsActive) return; 

            // Try to select a graphic from the map location
			await SelectGraphicAsync(e.Position);
		}

		private async Task SelectGraphicAsync(Point point)
		{
			// Clear previous selection
			if (_selection != null)
			{	
				_selection.Unselect();
				_selection.SetVisible();
			}
			_selection = null;

			// Find first graphic from the overlays
			foreach (var overlay in MySceneView.GraphicsOverlays)
			{
				var foundGraphic = (await MySceneView.IdentifyGraphicsOverlayAsync(overlay, point, 2, false, 1)).Graphics.FirstOrDefault();

				if (foundGraphic != null)
				{
					_selection = new GraphicSelection(foundGraphic, overlay);
					_selection.Select();
					break;
				}
			}

			EditButton.IsEnabled = _selection == null ? false : true;
		}
	}

	public class ColorConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType == typeof(System.Drawing.Color))
			{
				if(value is string name)
				{
					return System.Drawing.Color.FromName(name);
				}
				else if (parameter is string pname)
				{
					return System.Drawing.Color.FromName(pname);
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
