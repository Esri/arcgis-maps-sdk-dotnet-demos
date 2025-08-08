using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace DemoApplicationAccessibility.DescribingNonTextContent
{
    /// <summary>
    /// Interaction logic for FeatureMap.xaml
    /// </summary>
    public partial class FeatureMap : UserControl
    {
        private MapPoint? _previousCenter;
        private double? _previousScale;
        private readonly LocatorTask locatorTask;
        private CancellationTokenSource _viewpointChangeCts = new();
        private readonly TimeSpan _viewpointChangeDelay = TimeSpan.FromMilliseconds(800);
        static string[]? _previousChunks = null;

        public FeatureMap()
        {
            InitializeComponent();
            DataContext = this;
            locatorTask = new LocatorTask(new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
        }

        private async void MyMapView_NavigationCompleted(object sender, EventArgs e)
        {
            _viewpointChangeCts?.Cancel();
            _viewpointChangeCts = new CancellationTokenSource();
            var token = _viewpointChangeCts.Token;

            try
            {
                await Task.Delay(_viewpointChangeDelay, token);
                if (token.IsCancellationRequested) return;

                var viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);
                if (viewpoint == null) return;

                var newCenter = viewpoint.TargetGeometry as MapPoint;
                var newScale = (int)viewpoint.TargetScale;
                string locationName = "Unknown location";
                if (newCenter != null && !newCenter.IsEmpty)
                {
                    IReadOnlyList<GeocodeResult> results = await locatorTask.ReverseGeocodeAsync(newCenter);
                    if (results != null && results.Count > 0)
                        locationName = results[0].Label;
                }

                string movement = "";
                if (_previousCenter != null && _previousScale != null && newCenter != null)
                {
                    string pan = GetPanDirection(_previousCenter, newCenter);
                    string zoom = GetZoomChange(_previousScale.Value, newScale);
                    movement = string.Join(" and ", new[] { pan, zoom }.Where(s => !string.IsNullOrWhiteSpace(s)));
                }

                _previousCenter = newCenter;
                _previousScale = newScale;

                string extentLevel = GetExtentLevel(newScale);
                var visibleFeatures = await GetVisibleFeaturesAsync(token);
                string featuresSummary = GenerateFeatureSummary(visibleFeatures);

                string[] messageChunks = new[]
                {
                    $"You are viewing the map centered at {locationName}.",
                    $"The scale is approximately {GetFormattedScaleValue()}. ",
                    $"Showing an extent of {extentLevel}.",
                    featuresSummary
                };
                if (token.IsCancellationRequested)
                    return;
                Announce(movement, AutomationNotificationProcessing.CurrentThenMostRecent, "notifyMovement");

                // Announce only changed message chunks
                bool isFirstAnnouncement = _previousChunks == null;

                for (int i = 0; i < messageChunks.Length; i++)
                {
                    if (isFirstAnnouncement || i >= _previousChunks.Length || _previousChunks[i] != messageChunks[i])
                    {
                        Announce(messageChunks[i], AutomationNotificationProcessing.CurrentThenMostRecent, "notifyChunk" + i);
                    }
                }

                _previousChunks = messageChunks;

            }
            catch (TaskCanceledException)
            {
                // Expected due to debouncing
            }
        }

        private string GetFormattedScaleValue()
        {
            var peer = UIElementAutomationPeer.CreatePeerForElement(ScaleLine);
            if (peer == null)
                return string.Empty;

            var valueProvider = peer.GetPattern(PatternInterface.Value) as IValueProvider;
            return valueProvider?.Value ?? string.Empty;
        }
        private void Announce(string text, AutomationNotificationProcessing mode, string notificationMessagePart)
        {
            var peer = UIElementAutomationPeer.CreatePeerForElement(MyMapView);
            if (peer is FrameworkElementAutomationPeer feap)
            {
                feap.RaiseNotificationEvent(
                    AutomationNotificationKind.Other,
                    mode,
                    text,
                    notificationMessagePart);
            }
        }

        private static string GetPanDirection(MapPoint oldCenter, MapPoint newCenter)
        {
            var dx = newCenter.X - oldCenter.X;
            var dy = newCenter.Y - oldCenter.Y;

            if (Math.Abs(dx) < 1e-4 && Math.Abs(dy) < 1e-4)
                return "";

            return Math.Abs(dx) > Math.Abs(dy)
                ? (dx > 0 ? "moving east" : "moving west")
                : (dy > 0 ? "moving north" : "moving south");
        }

        private static string GetZoomChange(double oldScale, double newScale)
        {
            const double threshold = 0.05;
            double ratio = newScale / oldScale;

            if (ratio < 1 - threshold)
                return "zooming in";
            if (ratio > 1 + threshold)
                return "zooming out";
            return "";
        }

        private static string GetExtentLevel(int scale)
        {
            if (scale <= 400) return "rooms";
            if (scale <= 800) return "small building";
            if (scale <= 1250) return "building";
            if (scale <= 2500) return "buildings";
            if (scale <= 5000) return "street";
            if (scale <= 10000) return "streets";
            if (scale <= 20000) return "neighborhood";
            if (scale <= 40000) return "town";
            if (scale <= 80000) return "city";
            if (scale <= 160000) return "cities";
            if (scale <= 320000) return "metropolitan area";
            if (scale <= 750000) return "county";
            if (scale <= 1500000) return "counties";
            if (scale <= 3000000) return "state or province";
            if (scale <= 6000000) return "states or provinces";
            if (scale <= 12000000) return "small country";
            if (scale <= 25000000) return "big country";
            if (scale <= 50000000) return "continent";
            return "world";
        }

        private async Task<List<Feature>> GetVisibleFeaturesAsync(CancellationToken token)
        {
            var viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var allVisible = new List<Feature>();
            try
            {
                if (viewpoint == null || viewpoint.TargetGeometry == null || MyMapView?.Map?.OperationalLayers == null)
                    return allVisible;

                var visibleExtent = viewpoint.TargetGeometry;
                var visibleFeatureLayers = MyMapView.Map.OperationalLayers
                    .OfType<FeatureLayer>()
                    .Where(layer => layer.FeatureTable != null && layer.IsVisibleAtScale(MyMapView.MapScale));

                foreach (var layer in visibleFeatureLayers)
                {
                    if (token.IsCancellationRequested)
                        return allVisible;
                    var queryParams = new QueryParameters
                    {
                        Geometry = visibleExtent,
                        SpatialRelationship = SpatialRelationship.Intersects
                    };
                    var result = await layer.FeatureTable.QueryFeaturesAsync(queryParams, token);
                    allVisible.AddRange(result);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected due to debouncing
            }
            return allVisible;
        }

        private static string GenerateFeatureSummary(List<Feature> features)
        {
            if (features.Count == 0)
                return "No heritage sites are visible.";

            var groups = features
                        .GroupBy(f =>
                        {
                            if (f.Attributes != null && f.Attributes.TryGetValue("property_category", out var value))
                                return value?.ToString() ?? "Unknown";
                            return "Unknown";
                        })
                        .ToDictionary(g => g.Key, g => g.ToList());
            string summary = "Visible sites include " + string.Join(", ", groups.Select(kvp => $"{kvp.Value.Count} {kvp.Key}"));

            if (features.Count <= 7)
            {
                var names = features.Select(f =>
                {
                    if (f.Attributes != null && f.Attributes.TryGetValue("element_name_en", out var value))
                        return value?.ToString() ?? "Unnamed site";
                    return "Unnamed site";
                });
                summary += $". Site names: {string.Join(", ", names)}.";
            }

            return summary;
        }
    }
}