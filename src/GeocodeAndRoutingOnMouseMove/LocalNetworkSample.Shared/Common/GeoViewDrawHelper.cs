using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace LocalNetworkSample.Common
{
    /// <summary>
    /// Utility class that provides helper methods for drawing geometries on a MapView.
    /// </summary>
    class GeoViewDrawHelper
    {
        #region Default draw symbols
        //Symbol used by DrawPointAsync while moving the mouse
        private static MarkerSymbol DefaultMarkerSymbol = new SimpleMarkerSymbol() { Color = Colors.Blue, Size = 12, Style = SimpleMarkerSymbolStyle.Circle };

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

        //Line Symbol used to show line between last added vertex and current mouse location
        private static LineSymbol DefaultLineMoveSymbol = new SimpleLineSymbol()
        {
            Width = 5,
            Color = Color.FromArgb(100, 255, 255, 255),
            Style = SimpleLineSymbolStyle.Dot
        };
        #endregion Default draw symbols

        #region public draw operations
        public static async Task<MapPoint> DrawPointAsync(GeoView view, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<MapPoint>();
            var sketchlayer = CreateSketchLayer(view);
            sketchlayer.Opacity = .5;
            Graphic pointGraphic = null;
            Action cleanupEvents = SetUpHandlers(view,
                (p) => //On mouse move move graphic around
                {
                    if (p != null)
                    {
                        if (pointGraphic == null)
                        {
                            pointGraphic = new Graphic(p, DefaultMarkerSymbol);
                            sketchlayer.Graphics.Add(pointGraphic);
                        }
                        else pointGraphic.Geometry = p;
                    }
                },
                (p) => //View tapped - completes task and returns point
                {
                    tcs.SetResult(p);
                }
                , null);
            Action cleanup = () =>
            {
                cleanupEvents();
                view.GraphicsOverlays.Remove(sketchlayer);
            };
            cancellationToken.Register(() => tcs.SetCanceled());

            MapPoint result = null;
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

        public static async Task<Polyline> DrawPolylineAsync(GeoView view, System.Threading.CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Polyline>();
            PolylineBuilder polylineBuilder = new PolylineBuilder(view.SpatialReference);
            var sketchlayer = CreateSketchLayer(view);
            Graphic lineGraphic = new Graphic() { Symbol = DefaultLineSymbol };
            Graphic lineMoveGraphic = new Graphic() { Symbol = DefaultLineMoveSymbol };
            sketchlayer.Graphics.Add(lineGraphic);
            sketchlayer.Graphics.Add(lineMoveGraphic);
            Action cleanupEvents = SetUpHandlers(view,
                (p) => //On mouse move, move completion line around
                {
                    if (p != null && polylineBuilder.Parts.Count > 0 && polylineBuilder.Parts[0].Count > 0)
                    {
                        lineMoveGraphic.Geometry = new Polyline(new MapPoint[] { polylineBuilder.Parts[0].Last().EndPoint, p });
                    }
                },
                (p) => //On tap add a vertex
                {
                    if (p != null)
                    {
                        polylineBuilder.AddPoint(p);
                        if (polylineBuilder.Parts.Count > 0 && polylineBuilder.Parts[0].Count >= 1)
                            lineGraphic.Geometry = polylineBuilder.ToGeometry();
                    }
                },
                (p) => //View tapped - completes task and returns point
                {
                    tcs.SetResult(polylineBuilder.ToGeometry());
                });
            Action cleanup = () =>
            {
                cleanupEvents();
                view.GraphicsOverlays.Remove(sketchlayer);
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

        public static async Task<Polygon> DrawPolygonAsync(GeoView view, System.Threading.CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Polygon>();
            PolygonBuilder polygonBuilder = new PolygonBuilder(view.SpatialReference);
            var sketchlayer = CreateSketchLayer(view);
            Graphic polygonGraphic = new Graphic() { Symbol = DefaultFillSymbol };
            Graphic lineMoveGraphic = new Graphic() { Symbol = DefaultLineMoveSymbol };
            sketchlayer.Graphics.Add(polygonGraphic);
            sketchlayer.Graphics.Add(lineMoveGraphic);
            Action cleanupEvents = SetUpHandlers(view,
                (p) => //On mouse move move completion line around
                {
                    if (p != null && polygonBuilder.Parts.Count > 0)
                    {
                        lineMoveGraphic.Geometry = new Polyline(new MapPoint[]
                                {
                                 polygonBuilder.Parts[0].Last().EndPoint,
                                 p,
                                 polygonBuilder.Parts[0].First().StartPoint
                                });
                    }
                },
                (p) => //On tap add a vertex
                {
                    if (p != null)
                    {
                        polygonBuilder.AddPoint(p);
                        if (polygonBuilder.Parts.Count > 0 && polygonBuilder.Parts[0].Count > 0)
                        {
                            polygonGraphic.Geometry = polygonBuilder.ToGeometry();
                            lineMoveGraphic.Geometry = null;
                        }
                    }
                },
                (p) => //View tapped - completes task and returns point
                {
                    tcs.SetResult(polygonBuilder.ToGeometry());
                });
            Action cleanup = () =>
            {
                cleanupEvents();
                view.GraphicsOverlays.Remove(sketchlayer);
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
        #endregion public draw operations

        #region Private utility methods
        /// <summary>
        /// Helper for adding mouse events
        /// </summary>
        /// <param name="view">The view to listen for events on.</param>
        /// <param name="onMove">Action when the mouse moves.</param>
        /// <param name="onTapped">Action when the view is tapped.</param>
        /// <param name="onDoubleTapped">Action when the view is double tapped.</param>
        /// <returns>An Action that cleans up all the event handlers.</returns>
        private static Action SetUpHandlers(GeoView view, Action<MapPoint> onMove, Action<MapPoint> onTapped, Action<MapPoint> onDoubleTapped)
        {
#if NETFX_CORE
            Windows.UI.Xaml.Input.PointerEventHandler movehandler = null;
#else
            System.Windows.Input.MouseEventHandler movehandler = null;
#endif

            if (onMove != null)
            {
#if NETFX_CORE
                movehandler = (s, e) => onMove(((MapView)view).ScreenToLocation(e.GetCurrentPoint(view).Position));
                view.PointerMoved += movehandler;
#else
                movehandler = (s, e) => onMove(((MapView)view).ScreenToLocation(e.GetPosition(view)));
                view.MouseMove += movehandler;
#endif
            }
            EventHandler<GeoViewInputEventArgs> tappedHandler = null;
            if (onTapped != null)
            {
                tappedHandler = (s, e) => onTapped(e.Location);
                view.GeoViewTapped += tappedHandler;
            }
            EventHandler<GeoViewInputEventArgs> doubletappedHandler = null;
            if (onDoubleTapped != null)
            {
                doubletappedHandler = (s, e) => { e.Handled = true; onDoubleTapped(e.Location); };
                view.GeoViewDoubleTapped += doubletappedHandler;
            }
            Action cleanup = () =>
            {
                if (movehandler != null)
#if NETFX_CORE
                    view.PointerMoved -= movehandler;
#else
                    view.MouseMove -= movehandler;
#endif
                if (tappedHandler != null) view.GeoViewTapped -= tappedHandler;
                if (doubletappedHandler != null) view.GeoViewDoubleTapped -= doubletappedHandler;
            };
            return cleanup;
        }

        private static GraphicsOverlay CreateSketchLayer(GeoView geoView)
        {
            GraphicsOverlay go = new GraphicsOverlay();
            geoView.GraphicsOverlays.Add(go);
            return go;
        }
        #endregion Private utility methods
    }
}
