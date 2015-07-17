using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using SceneEditingSample;
#if NETFX_CORE
using Windows.UI;
#else
using System.Windows.Media;

#endif
namespace SceneEditingDemo.Helpers
{
	public class SceneEditHelper
	{
		#region Constructor and unique instance management
	
		// Private constructor
		private SceneEditHelper() { }

		// Static initialization of the unique instance 
		private static readonly SceneEditHelper SingleInstance = new SceneEditHelper();

		/// <summary>
		/// Gets the single <see cref="MapViewController"/> instance.
		/// </summary>
		public static SceneEditHelper Current
		{
			get { return SingleInstance; }
		}

		public void Initialize(SceneView sceneView)
		{
			SceneView = sceneView;
		}

		private void CheckInitializedState()
		{
			if (SceneView == null)
				throw new Exception("SceneEditHelper is not initialized.");
		}

		#endregion // Constructor and unique instance management

		#region Default draw symbols
	
		//Symbol used by DrawPointAsync while moving the mouse
		private static MarkerSymbol DefaultMarkerSymbol = new SimpleMarkerSymbol() { Color = Colors.Blue };

		//Symbol used by Vertices on editing
		private static MarkerSymbol DefaultVertexSymbol = new SimpleMarkerSymbol() { Color = Colors.Blue };

		//Symbol used by DrawPolylineAsync	
		private static LineSymbol DefaultLineSymbol = new SimpleLineSymbol() 
		{ 
			Width = 2, 
			Color = Color.FromArgb(150, 0, 0, 255) 
		}; 
	
		//Symbol used by DrawPolygonAsync
		private static FillSymbol DefaultFillSymbol = new SimpleFillSymbol() 
		{
			Outline = new SimpleLineSymbol() { Width = 2, Color = Colors.Black },
			Color = Color.FromArgb(100, 0, 0, 255)
		};

		//Line Symbol used to show line between vertices when drawing
		private static LineSymbol DefaultLineMoveSymbol = new SimpleLineSymbol() 
		{
			Width = 5,
			Color = Color.FromArgb(100, 255, 255, 255),
			Style = SimpleLineStyle.Dot
		};

		#endregion // Default draw symbols

		public SceneView SceneView { get; protected set; }

		public bool IsActive { get; private set; }

		#region Create geometries

		public async Task<MapPoint> CreatePointAsync(CancellationToken cancellationToken)
		{
			CheckInitializedState();
			SetActivity(); // set to active
			var geometry = await SceneDrawHelper.DrawPointAsync(SceneView, cancellationToken);
			SetActivity(false); // set to deactive
			return geometry;
		}

		public async Task<Polyline> CreatePolylineAsync(CancellationToken cancellationToken)
		{
			CheckInitializedState();
			SetActivity(); // set to active
			var geometry = await SceneDrawHelper.DrawPolylineAsync(SceneView, cancellationToken);
			SetActivity(false); // set to deactive
			return geometry;
		}

		public async Task<Polygon> CreatePolygonAsync(CancellationToken cancellationToken)
		{
			CheckInitializedState();
			SetActivity(); // set to active
			var geometry = await SceneDrawHelper.DrawPolygonAsync(SceneView, cancellationToken);
			SetActivity(false); // set to deactive
			return geometry;
		}

		#endregion // Create geometries 

		#region Edit geometries

		public async Task<MapPoint> EditPointAsync(MapPoint mapPoint, CancellationToken cancellationToken)
		{
			CheckInitializedState();

			return await CreatePointAsync(cancellationToken);
		}

		public async Task<Polygon> EditPolygonAsync(Polygon polygon, CancellationToken cancellationToken)
		{
			CheckInitializedState();
			SetActivity(); // set to active

			var tcs = new TaskCompletionSource<Polygon>();
			PolygonBuilder polylineBuilder = new PolygonBuilder(SceneView.SpatialReference);
			var sketchlayer = CreateSketchLayer(SceneView);
			var vertexlayer = CreateSketchLayer(SceneView);

			// Create vertices from the original polyline
			var vertices = new List<Graphic>();
			foreach (var vertex in (polygon.Parts[0].GetPoints()))
				vertices.Add(new Graphic(vertex, DefaultVertexSymbol));

			vertices.RemoveAt(vertices.Count - 1); // don't add closing point

			// Default to original polyline
			var newPolygon = new Polygon(polygon.Parts);

			Graphic fillGraphic = new Graphic(newPolygon) { Symbol = DefaultFillSymbol };
			Graphic lineMoveGraphic = new Graphic() { Symbol = DefaultLineMoveSymbol };
			
			sketchlayer.Graphics.AddRange(new Graphic[] { fillGraphic, lineMoveGraphic });
			vertexlayer.Graphics.AddRange(vertices);

			CancellationTokenSource tokenSource = null;
			Graphic selectedVertex = null;
			bool isEditingVertex = false;

			Action cleanupEvents = SetUpHandlers(SceneView,
				(p) => //On mouse move, move completion line around
				{
					if (p != null && isEditingVertex)
					{
						// Update visual indicator polyline
						var vertexPoints = newPolygon.Parts[0].GetPoints().ToList();
						vertexPoints.RemoveAt(vertexPoints.Count - 1); // don't add closing point
						var index = vertexPoints
							.IndexOf(vertexPoints.Where
								(point => GeometryEngine.Equals(point, selectedVertex.Geometry)).First());
						var temporaryVertices = new List<MapPoint>();
						
						if (index > 0)
							temporaryVertices.Add(vertexPoints[index - 1]); // Add previous segment
						else
							temporaryVertices.Add(vertexPoints.Last()); // Add start segment from end
						temporaryVertices.Add(p);
						if (index != vertexPoints.Count() - 1)
							temporaryVertices.Add(vertexPoints[index + 1]); // Add next segment
						else
							temporaryVertices.Add(vertexPoints.First());

						var builder = new PolylineBuilder(temporaryVertices);
						lineMoveGraphic.Geometry = builder.ToGeometry();
						lineMoveGraphic.IsVisible = true;
					}
					else
					{
						lineMoveGraphic.IsVisible = false;
					}
				},
				async (p) => //On tap add a vertex
				{
					if (p == null) return;
					if (isEditingVertex) return;
					if (selectedVertex != null) selectedVertex.IsSelected = false;

					selectedVertex = await vertexlayer.HitTestAsync(SceneView, SceneView.LocationToScreen(p));

					// No vertex found so return
					if (selectedVertex == null)
						return;

					isEditingVertex = true;
					selectedVertex.IsSelected = true;
					tokenSource = new CancellationTokenSource();
					try
					{
						var newPoint = await SceneDrawHelper.DrawPointAsync(SceneView, tokenSource.Token);
						if (newPoint == null) return;

						var vertexPoints = newPolygon.Parts[0].GetPoints().ToList();
						vertexPoints.RemoveAt(vertexPoints.Count - 1); // don't add closing point
						var index = vertexPoints
							.IndexOf(vertexPoints.Where
								(point => GeometryEngine.Equals(point, selectedVertex.Geometry)).First());
						var builder = new PolygonBuilder(vertexPoints);
						builder.Parts[0].MovePoint(index, newPoint);
						
						// Update polyline
						newPolygon = builder.ToGeometry();
						fillGraphic.Geometry = newPolygon;
						// Update vertex
						selectedVertex.Geometry = newPoint;
						tokenSource = null;				
					}
					catch (TaskCanceledException tce)
					{	
					}
					finally
					{
						lineMoveGraphic.IsVisible = false;
						selectedVertex.IsSelected = false;
						selectedVertex = null;
						isEditingVertex = false;
					}
				},
				(p) => // Douple tapped - completes task and returns new polygon
				{
					fillGraphic.IsVisible = false;
					lineMoveGraphic.IsVisible = false;
					tcs.SetResult(newPolygon);
				});
			Action cleanup = () =>
			{
				cleanupEvents();
				SceneView.GraphicsOverlays.Remove(sketchlayer);
				SceneView.GraphicsOverlays.Remove(vertexlayer);
				if (tokenSource != null) tokenSource.Cancel(); // Cancel vertex draw if it isn't finished
				SetActivity(false);
			};
			cancellationToken.Register(() => tcs.SetCanceled());

			Polygon result = null;
			try
			{
				result = await tcs.Task;
			}
			finally
			{
				cleanup();
			}
			return result;
		}

		public async Task<Polyline> EditPolylineAsync(Polyline polyline, CancellationToken cancellationToken)
		{
			CheckInitializedState();
			SetActivity(); // set to active

			var tcs = new TaskCompletionSource<Polyline>();
			PolylineBuilder polylineBuilder = new PolylineBuilder(SceneView.SpatialReference);
			var sketchlayer = CreateSketchLayer(SceneView);
			var vertexlayer = CreateSketchLayer(SceneView);

			// Create vertices from the original polyline
			var vertices = new List<Graphic>();
			foreach (var vertex in (polyline.Parts[0].GetPoints()))
				vertices.Add(new Graphic(vertex, DefaultVertexSymbol));

			// Default to original polyline
			var newPolyline = new Polyline(polyline.Parts);

			Graphic lineGraphic = new Graphic(newPolyline) { Symbol = DefaultLineSymbol };
			Graphic lineMoveGraphic = new Graphic() { Symbol = DefaultLineMoveSymbol };

			sketchlayer.Graphics.AddRange(new Graphic[] { lineGraphic, lineMoveGraphic });
			vertexlayer.Graphics.AddRange(vertices);

			CancellationTokenSource tokenSource = null;
			Graphic selectedVertex = null;
			bool isEditingVertex = false;

			Action cleanupEvents = SetUpHandlers(SceneView,
				(p) => //On mouse move, move completion line around
				{
					if (p != null && isEditingVertex)
					{
						// Update visual indicator polyline
						var vertexPoints = newPolyline.Parts[0].GetPoints().ToList();
						var index = vertexPoints
							.IndexOf(vertexPoints.Where
								(point => GeometryEngine.Equals(point, selectedVertex.Geometry)).First());
						var temporaryVertices = new List<MapPoint>();

						if (index > 0)
							temporaryVertices.Add(vertexPoints[index - 1]); // Add previous segment
						temporaryVertices.Add(p);
						if (index != vertexPoints.Count() - 1)
							temporaryVertices.Add(vertexPoints[index + 1]); // Add next segment

						var builder = new PolylineBuilder(temporaryVertices);
						lineMoveGraphic.Geometry = builder.ToGeometry();
						lineMoveGraphic.IsVisible = true;
					}
				},
				async (p) => //On tap add a vertex
				{
					if (p == null) return;
					if (isEditingVertex) return;
					if (selectedVertex != null) selectedVertex.IsSelected = false;

					selectedVertex = await vertexlayer.HitTestAsync(SceneView, SceneView.LocationToScreen(p));

					// No vertex found so return
					if (selectedVertex == null)
						return;

					isEditingVertex = true;
					selectedVertex.IsSelected = true;
					tokenSource = new CancellationTokenSource();
					try
					{
						var newPoint = await SceneDrawHelper.DrawPointAsync(SceneView, tokenSource.Token);

						if (newPoint == null) return;
						
						var vertexPoints = newPolyline.Parts[0].GetPoints();
						var index = vertexPoints.ToList()
							.IndexOf(vertexPoints.Where
								(point => GeometryEngine.Equals(point, selectedVertex.Geometry)).First());
						var builder = new PolylineBuilder(vertexPoints);
						builder.Parts[0].MovePoint(index, newPoint);

						lineGraphic.Geometry = null;

						// Update polyline
						newPolyline = builder.ToGeometry();
						lineGraphic.Geometry = newPolyline;

						// Update vertex
						selectedVertex.Geometry = newPoint;
						tokenSource = null;
					}
					catch (TaskCanceledException tce)
					{
					}
					finally
					{
						lineMoveGraphic.IsVisible = false;
						selectedVertex.IsSelected = false;
						selectedVertex = null;
						isEditingVertex = false;
					}
				},
				(p) => // Douple tapped - completes task and returns new polyline
				{
					tcs.SetResult(newPolyline);
				});
			Action cleanup = () =>
			{
				cleanupEvents();
				SceneView.GraphicsOverlays.Remove(sketchlayer);
				SceneView.GraphicsOverlays.Remove(vertexlayer);
				if (tokenSource != null) tokenSource.Cancel();
				SetActivity(false);
			};
			cancellationToken.Register(() => tcs.SetCanceled());

			Polyline result = null;
			try
			{
				result = await tcs.Task;
			}
			finally
			{
				cleanup();
			}
			return result;
		}

		#endregion // Edit geometries

		#region Private utility methods

		private void SetActivity(bool isActive = true)
		{
			IsActive = isActive;
		}

		/// <summary>
		/// Helper for adding mouse events
		/// </summary>
		/// <param name="view">The view to listen for events on.</param>
		/// <param name="onMove">Action when the mouse moves.</param>
		/// <param name="onTapped">Action when the view is tapped.</param>
		/// <param name="onDoubleTapped">Action when the view is double tapped.</param>
		/// <returns>An Action that cleans up all the event handlers.</returns>
		private static Action SetUpHandlers(SceneView view, Action<MapPoint> onMove, Action<MapPoint> onTapped, Action<MapPoint> onDoubleTapped)
		{
#if NETFX_CORE
			Windows.UI.Xaml.Input.PointerEventHandler movehandler = null;
#else
			System.Windows.Input.MouseEventHandler movehandler = null;
#endif

			if (onMove != null)
			{
#if NETFX_CORE
				movehandler = (s, e) => onMove(view.ScreenToLocation(e.GetCurrentPoint(view).Position));
				view.PointerMoved += movehandler;
#else
				movehandler = (s, e) => onMove(view.ScreenToLocation(e.GetPosition(view)));
				view.MouseMove += movehandler;
#endif
			}
			EventHandler<MapViewInputEventArgs> tappedHandler = null;
			if (onTapped != null)
			{
				tappedHandler = (s, e) => onTapped(e.Location);
				view.SceneViewTapped += tappedHandler;
			}
			EventHandler<MapViewInputEventArgs> doubletappedHandler = null;
			if (onDoubleTapped != null)
			{
				doubletappedHandler = (s, e) => { e.Handled = true; onDoubleTapped(e.Location); };
				view.SceneViewDoubleTapped += doubletappedHandler;
			}
			Action cleanup = () =>
			{
				if (movehandler != null)
#if NETFX_CORE
						view.PointerMoved -= movehandler;
#else
					view.MouseMove -= movehandler;
#endif
				if (tappedHandler != null) view.SceneViewTapped -= tappedHandler;
				if (doubletappedHandler != null) view.SceneViewDoubleTapped -= doubletappedHandler;
			};
			return cleanup;
		}

		private static GraphicsOverlay CreateSketchLayer(ViewBase scene)
		{
			GraphicsOverlay go = new GraphicsOverlay();
			scene.GraphicsOverlays.Add(go);
			return go;
		}
		#endregion Private utility methods
	}
}
