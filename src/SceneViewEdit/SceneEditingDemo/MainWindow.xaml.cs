using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using SceneEditingDemo.Helpers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SceneEditingDemo
{
	public partial class MainWindow : Window
	{
		CancellationTokenSource _drawTaskTokenSource;

		GraphicsOverlay _pointsOverlay;
		GraphicsOverlay _polylinesOverlay;
		GraphicsOverlay _polygonsOverlay;

		// Store graphic that is being edited
		GraphicSelection _selection;

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

			SceneEditHelper.Current.Initialize(MySceneView);

			EditButton.IsEnabled = false;
		}

		private async void DrawButton_Click(object sender, RoutedEventArgs e)
		{
			// Cancel previous source and create new
			if (_drawTaskTokenSource != null)
				_drawTaskTokenSource.Cancel();
			_drawTaskTokenSource = new CancellationTokenSource();

			try
			{
				Geometry geometry = null; 
				Graphic graphic = null;

				// Execute draw logic
				switch ((DrawShape)DrawShapes.SelectedValue)
				{
					case DrawShape.Point:
						geometry = await SceneEditHelper.Current.CreatePointAsync(_drawTaskTokenSource.Token);
						graphic = new Graphic(geometry);
						_pointsOverlay.Graphics.Add(graphic);
						break;
					case DrawShape.Polygon:
						geometry = await SceneEditHelper.Current.CreatePolygonAsync(_drawTaskTokenSource.Token);
						graphic = new Graphic(geometry);
						_polygonsOverlay.Graphics.Add(graphic);
						break;
					case DrawShape.Polyline:
						geometry = await SceneEditHelper.Current.CreatePolylineAsync(_drawTaskTokenSource.Token);
						graphic = new Graphic(geometry);
						_polylinesOverlay.Graphics.Add(graphic);
						break;
					default:
						break;
				}
			}
			catch (TaskCanceledException tce)
			{
				Debug.WriteLine("Previous draw operation was cancelled.");
			}
			finally
			{
				_drawTaskTokenSource = null;
			}			
		}

		private async void EditButton_Click(object sender, RoutedEventArgs e)
		{
			if (_selection == null) return; // Selection missing

			// Cancel previous source and create new
			if (_drawTaskTokenSource != null)
				_drawTaskTokenSource.Cancel();
			_drawTaskTokenSource = new CancellationTokenSource();

			_selection.SetVisible();
			Geometry editedGeometry = null;

			DrawButton.IsEnabled = false;
			ClearButton.IsEnabled = false;
			EditButton.IsEnabled = false;

			try
			{
				switch (_selection.GeometryType)
				{
					case GeometryType.Point:
						editedGeometry = await SceneEditHelper.Current.EditPointAsync(
							_selection.SelectedGraphic.Geometry as MapPoint, 
							_drawTaskTokenSource.Token);
						break;
					case GeometryType.Polyline:
						_selection.SetHidden();
						editedGeometry = await SceneEditHelper.Current.EditPolylineAsync(
							_selection.SelectedGraphic.Geometry as Polyline, 
							_drawTaskTokenSource.Token);
						break;
					case GeometryType.Polygon:
						_selection.SetHidden();
						editedGeometry = await SceneEditHelper.Current.EditPolygonAsync(
							_selection.SelectedGraphic.Geometry as Polygon, 
							_drawTaskTokenSource.Token);
						break;
					default:
						break;
				}

				_selection.SelectedGraphic.Geometry = editedGeometry;
			}
			catch (TaskCanceledException tce)
			{
				Debug.WriteLine("Previous edit operation was cancelled.");
			}
			finally
			{
				_selection.Unselect();
				_selection.SetVisible();
				_drawTaskTokenSource = null;
				DrawButton.IsEnabled = true; 
				ClearButton.IsEnabled = true;
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

		private async void MySceneView_SceneViewTapped(object sender, MapViewInputEventArgs e)
		{
			// If draw or edit is active, return
			if (SceneEditHelper.Current.IsActive) return; 

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
				var foundGraphic = await overlay.HitTestAsync(
						MySceneView,
						point);

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
}
