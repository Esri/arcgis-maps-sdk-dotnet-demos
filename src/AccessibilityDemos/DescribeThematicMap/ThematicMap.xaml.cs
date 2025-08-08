using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;

namespace DemoApplicationAccessibility.DescribingNonTextContent
{
    public partial class ThematicMap : UserControl
    {
        private readonly ThematicMapViewModel _viewModel;
        private CancellationTokenSource _viewpointChangeCts = new();
        private readonly TimeSpan _viewpointChangeDelay = TimeSpan.FromMilliseconds(1000);
        private const string HighlightOverlayId = "HighlightOverlay";

        public ThematicMap()
        {
            InitializeComponent();
            _viewModel = new ThematicMapViewModel(MyMapView);

            _viewModel.AnnounceRequested += Announce;
            _viewModel.HighlightPolygonRequested += (polygon, label) => HighlightPolygonWithLabel(polygon, label, Color.Gold);
            _viewModel.ClearHighlightsRequested += () => GetOrCreateHighlightOverlay().Graphics.Clear();
            _viewModel.ShowPopupRequested += ShowPopup;

            MyMapView.DrawStatusChanged += MyMapView_DrawStatusChanged;
            MyMapView.SizeChanged += MyMapView_SizeChanged;
        }
        private async void MyMapView_NavigationCompleted(object sender, EventArgs e)
        {
            _viewpointChangeCts.Cancel();
            _viewpointChangeCts = new CancellationTokenSource();
            var token = _viewpointChangeCts.Token;

            try
            {
                await Task.Delay(_viewpointChangeDelay, token);
                if (token.IsCancellationRequested) return;
                await _viewModel.AnnounceMapStateAsync(token);
            }
            catch (TaskCanceledException) { }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            await _viewModel.HandleGeoViewTappedAsync(e.Position);
        }
        private void MyMapView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D1 && e.Key <= Key.D7)
            {
                int index = e.Key - Key.D1;
                _viewModel.ShowFeaturePopup(index);
            }
            else if (e.Key == Key.Escape)
            {
                HidePopup();
            }
        }
        private void PopupBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HidePopup();
        }
        private async void MyMapView_DrawStatusChanged(object? sender, DrawStatusChangedEventArgs e)
        {
            if (e.Status == DrawStatus.Completed && MyMapView != null)
            {
                HighlightQueriedArea();
                await _viewModel.AnnounceMapStateAsync(CancellationToken.None);
                // Unsubscribe after successful update to avoid repeated calls
                MyMapView.DrawStatusChanged -= MyMapView_DrawStatusChanged;
            }
        }

        private async void MyMapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await MyMapView.Map.LoadAsync();
            if (MyMapView?.VisibleArea == null) return;
            // Subsribe to DrawStatusChanged to handle updates after the map is resized
            MyMapView.DrawStatusChanged += MyMapView_DrawStatusChanged;
        }

        private void Announce(string message, string notificationMessagePart)
        {
            if (UIElementAutomationPeer.CreatePeerForElement(MyMapView) is FrameworkElementAutomationPeer peer)
            {
                peer.RaiseNotificationEvent(
                    AutomationNotificationKind.Other,
                    AutomationNotificationProcessing.CurrentThenMostRecent,
                    message,
                    notificationMessagePart);
            }
        }

        private void HighlightPolygonWithLabel(Polygon polygon, string labelText, Color borderColor)
        {
            if (MyMapView == null || polygon == null) return;

            var highlightOverlay = GetOrCreateHighlightOverlay();
            var fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Transparent,
                new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, borderColor, 2));

            var polygonGraphic = new Graphic(polygon, fillSymbol);
            highlightOverlay.Graphics.Add(polygonGraphic);

            var textSymbol = new TextSymbol
            {
                Text = labelText,
                Color = Color.Red,
                Size = 18,
                HaloColor = Color.White,
                HaloWidth = 2,
                HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle
            };

            var labelGraphic = new Graphic(polygon.Extent.GetCenter(), textSymbol);
            highlightOverlay.Graphics.Add(labelGraphic);
        }

        private GraphicsOverlay GetOrCreateHighlightOverlay()
        {
            var overlay = MyMapView.GraphicsOverlays.FirstOrDefault(go => go.Id == HighlightOverlayId);
            if (overlay == null)
            {
                overlay = new GraphicsOverlay { Id = HighlightOverlayId };
                MyMapView.GraphicsOverlays.Add(overlay);
            }
            return overlay;
        }

        private void ShowPopup(Popup popup)
        {
            popupViewer.Popup = popup;
            PopupBackground.Visibility = Visibility.Visible;
            Keyboard.Focus(popupViewer);
        }

        private void HidePopup()
        {
            popupViewer.Popup = null;
            PopupBackground.Visibility = Visibility.Collapsed;
            Keyboard.Focus(MyMapView);
        }

        private void HighlightQueriedArea()
        {
            if (MyMapView == null || MyMapView.VisibleArea == null) return;

            var visibleExtent = MyMapView.VisibleArea.Extent;
            if (visibleExtent == null) return;

            var spatialReference = visibleExtent.SpatialReference;
            if (spatialReference == null) return;

            var bufferDist = MyMapView.UnitsPerPixel * 10;
            var shrunkExtent = GeometryEngine.Buffer(visibleExtent, -bufferDist).Extent;

            var corners = new[]
            {
                new MapPoint(shrunkExtent.XMin, shrunkExtent.YMin, shrunkExtent.SpatialReference),
                new MapPoint(shrunkExtent.XMax, shrunkExtent.YMin, shrunkExtent.SpatialReference),
                new MapPoint(shrunkExtent.XMax, shrunkExtent.YMax, shrunkExtent.SpatialReference),
                new MapPoint(shrunkExtent.XMin, shrunkExtent.YMax, shrunkExtent.SpatialReference)
            };

            var screenPoints = corners.Select(p => MyMapView.LocationToScreen(p)).ToList();
            var polygon = new System.Windows.Shapes.Polygon
            {
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 2,
                Fill = System.Windows.Media.Brushes.Transparent,
                Points = new System.Windows.Media.PointCollection(screenPoints.Append(screenPoints[0]))
            };

            OverlayCanvas.Children.Clear();
            OverlayCanvas.Children.Add(polygon);
        }
    }
}