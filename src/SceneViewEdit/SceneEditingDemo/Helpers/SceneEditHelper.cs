using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

#if NETFX_CORE
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace SceneEditingDemo.Helpers
{
    /// <summary>
    /// <see cref="SceneEditHelper"/> provides methods for creating and editing geometries on the SceneView.
    /// When draw or edit is requested, draw experience is started in the assosiated SceneView that provides
    /// visual feedback for the user about the operation. 
    /// </summary>
    /// <remarks><see cref="SceneEditHelper"/> provides basic draw and edit operations for working with geometries that
    /// can come from a <see cref="Esri.ArcGISRuntime.Data.Feature"/> or <see cref="Graphic"/>.
    /// </remarks>
    public class SceneEditHelper
	{
        private static CancellationTokenSource _drawTaskTokenSource;

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

        /// <summary>
        /// Gets the value indicating wheter there is a draw or edit session ongoing.
        /// </summary>
		public static bool IsActive { get; private set; }

		#region Create geometries

        /// <summary>
        /// Create a new point. This will activate drawing experience on the map. To complete it, select location from the map.
        /// </summary>
        /// <param name="sceneView">The <see cref="SceneView"/> that is used for drawing.</param>
        /// <exception cref="TaskCanceledException">If previous task wasn't completed, <see cref="TaskCanceledException"/>
        /// will be thrown. The task is cancelled if <see cref="Cancel"/> method or if any other draw or edit method is called.
        /// </exception>
        /// <returns>Return new <see cref="MapPoint"/> based on the user interactions.</returns>
        public static async Task<MapPoint> CreatePointAsync(SceneView sceneView)
        {
            Initialize();
            var geometry = await SceneDrawHelper.DrawPointAsync(sceneView, _drawTaskTokenSource.Token);
            Cleanup();
            return geometry;
        }

        /// <summary>
        /// Create a new <see cref="Polyline"/>. This will activate drawing experience on the map. Draw is completed on douple click.
        /// </summary>
        /// <param name="sceneView">The <see cref="SceneView"/> that is used for drawing.</param>
        /// <exception cref="TaskCanceledException">If previous task wasn't completed, <see cref="TaskCanceledException"/>
        /// will be thrown. The task is cancelled if <see cref="Cancel"/> method or if any other draw or edit method is called.
        /// </exception>
        /// <returns>Return new <see cref="Polyline"/> based on the user interactions.</returns>
        public static async Task<Polyline> CreatePolylineAsync(SceneView sceneView)
		{
            Initialize();
            var geometry = await SceneDrawHelper.DrawPolylineAsync(sceneView, _drawTaskTokenSource.Token);
            Cleanup();
            return geometry;
		}

        /// <summary>
        /// Create a new <see cref="Polygon"/>. This will activate drawing experience on the map. Draw is completed on douple click.
        /// </summary>
        /// <param name="sceneView">The <see cref="SceneView"/> that is used for drawing.</param>
        /// <exception cref="TaskCanceledException">If previous task wasn't completed, <see cref="TaskCanceledException"/>
        /// will be thrown. The task is cancelled if <see cref="Cancel"/> method or if any other draw or edit method is called.
        /// </exception>
        /// <returns>Return new <see cref="Polygon"/> based on the user interactions.</returns>
        public static async Task<Polygon> CreatePolygonAsync(SceneView sceneView)
        {
            Initialize();
            var geometry = await SceneDrawHelper.DrawPolygonAsync(sceneView, _drawTaskTokenSource.Token);
            Cleanup();
            return geometry;
        }

        #endregion // Create geometries 

        #region Edit geometries

        /// <summary>
        /// Edit existing <see cref="Polygon"/>. This will activate editing experience on the map. Edit is completed on douple click.
        /// </summary>
        /// <param name="sceneView">The <see cref="SceneView"/> that is used for editing.</param>
        /// <exception cref="TaskCanceledException">If previous task wasn't completed, <see cref="TaskCanceledException"/>
        /// will be thrown. The task is cancelled if <see cref="Cancel"/> method or if any other draw or edit method is called.
        /// </exception>
        /// <returns>Return edited <see cref="Polygon"/> based on the user interactions.</returns>
        public static async Task<Polygon> EditPolygonAsync(SceneView sceneView, Polygon polygon)
		{
            Initialize();

			var tcs = new TaskCompletionSource<Polygon>();
			PolygonBuilder polylineBuilder = new PolygonBuilder(sceneView.SpatialReference);
			var sketchlayer = CreateSketchLayer(sceneView);
			var vertexlayer = CreateSketchLayer(sceneView);

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

			Action cleanupEvents = SetUpHandlers(sceneView,
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

					selectedVertex = await vertexlayer.HitTestAsync(sceneView, sceneView.LocationToScreen(p));

					// No vertex found so return
					if (selectedVertex == null)
						return;

					isEditingVertex = true;
					selectedVertex.IsSelected = true;
					tokenSource = new CancellationTokenSource();
					try
					{
						var newPoint = await SceneDrawHelper.DrawPointAsync(sceneView, tokenSource.Token);
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
                sceneView.GraphicsOverlays.Remove(sketchlayer);
                sceneView.GraphicsOverlays.Remove(vertexlayer);
				if (tokenSource != null) tokenSource.Cancel(); // Cancel vertex draw if it isn't finished
                Cleanup();
			};
            _drawTaskTokenSource.Token.Register(() => tcs.SetCanceled());

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

        /// <summary>
        /// Edit existing <see cref="Polyline"/>. This will activate editing experience on the map. Edit is completed on douple click.
        /// </summary>
        /// <param name="sceneView">The <see cref="SceneView"/> that is used for editing.</param>
        /// <exception cref="TaskCanceledException">If previous task wasn't completed, <see cref="TaskCanceledException"/>
        /// will be thrown. The task is cancelled if <see cref="Cancel"/> method or if any other draw or edit method is called.
        /// </exception>
        /// <returns>Return edited <see cref="Polygon"/> based on the user interactions.</returns>
        public static async Task<Polyline> EditPolylineAsync(SceneView sceneView, Polyline polyline)
		{
            Initialize();

			var tcs = new TaskCompletionSource<Polyline>();
			PolylineBuilder polylineBuilder = new PolylineBuilder(sceneView.SpatialReference);
			var sketchlayer = CreateSketchLayer(sceneView);
			var vertexlayer = CreateSketchLayer(sceneView);

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

			Action cleanupEvents = SetUpHandlers(sceneView,
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

					selectedVertex = await vertexlayer.HitTestAsync(sceneView, sceneView.LocationToScreen(p));

					// No vertex found so return
					if (selectedVertex == null)
						return;

					isEditingVertex = true;
					selectedVertex.IsSelected = true;
					tokenSource = new CancellationTokenSource();
					try
					{
						var newPoint = await SceneDrawHelper.DrawPointAsync(sceneView, tokenSource.Token);

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
                sceneView.GraphicsOverlays.Remove(sketchlayer);
                sceneView.GraphicsOverlays.Remove(vertexlayer);
				if (tokenSource != null) tokenSource.Cancel();
                Cleanup();
			};
            _drawTaskTokenSource.Token.Register(() => tcs.SetCanceled());

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

        #region Cancel

        /// <summary>
        /// Cancels current draw or edit session. Only one session can be active at the time.
        /// </summary>
        public static void Cancel()
        {
            if (!IsActive) return;

            // Cancel previous source and create new
            if (_drawTaskTokenSource != null)
                _drawTaskTokenSource.Cancel();
            Cleanup();
        }

        #endregion // Cancel

        #region Private utility methods

        /// <summary>
        /// Call to start new drawing / editing session.
        /// </summary>
        private static void Initialize()
        {
            SetActivity(); // set to active

            // Cancel previous source and create new
            if (_drawTaskTokenSource != null)
                _drawTaskTokenSource.Cancel();
            _drawTaskTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Call to close existing drawing / editing session.
        /// </summary>
        private static void Cleanup()
        {
            _drawTaskTokenSource = null;
            SetActivity(false);
        }

        /// <summary>
        /// Call to change activity. This will cancel previous task if it exists and current status is `true`.
        /// </summary>
        private static void SetActivity(bool isActive = true)
		{
            if (IsActive && isActive)
                _drawTaskTokenSource.Cancel();
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
