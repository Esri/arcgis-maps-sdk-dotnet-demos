using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.RealTime;
using Esri.ArcGISRuntime.UI.Controls;
using System.Text.RegularExpressions;
using System.Windows;

namespace DemoApplicationAccessibility.DescribingNonTextContent
{
    public class ThematicMapViewModel
    {
        private readonly MapView _mapView;
        private string _previousAdminLocation = "";
        private string _previousVisibleLayers = "";
        private string _previousMaxIncomeMessage = "";
        private readonly List<Feature> _keyboardShortcutFeatures = new();

        public event Action<string, string>? AnnounceRequested;
        public event Action<Polygon, string>? HighlightPolygonRequested;
        public event Action? ClearHighlightsRequested;
        public event Action<Popup>? ShowPopupRequested;

        public ThematicMapViewModel(MapView mapView)
        {
            _mapView = mapView;
        }

        public async Task AnnounceMapStateAsync(CancellationToken token)
        {
            try
            {
                _keyboardShortcutFeatures.Clear();
                ClearHighlightsRequested?.Invoke();

                var viewpoint = _mapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);
                if (viewpoint?.TargetGeometry is not MapPoint newCenter) return;
                var newScale = viewpoint.TargetScale;

                AnnounceVisibleLayers(newScale, token);
                await AnnounceCenterLocationAsync(newCenter, token);
                await AnnounceIncomeStatisticsAsync(token);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AnnounceMapStateAsync: {ex.Message}");
            }
        }

        public async Task HandleGeoViewTappedAsync(System.Windows.Point position)
        {
            try
            {
                var result = await _mapView.IdentifyLayersAsync(position, 3, false);
                var popup = GetPopup(result) ?? BuildPopupFromGeoElement(result);
                if (popup != null)
                    ShowPopupRequested?.Invoke(popup);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        public void ShowFeaturePopup(int index)
        {
            if (index < 0 || index >= _keyboardShortcutFeatures.Count) return;

            var feature = _keyboardShortcutFeatures[index];
            Popup popup = (feature is Feature f && f.FeatureTable?.Layer is IPopupSource src)
                ? new Popup(feature, src.PopupDefinition)
                : Popup.FromGeoElement(feature);

            ShowPopupRequested?.Invoke(popup);
        }

        private void AnnounceVisibleLayers(double scale, CancellationToken token)
        {
            var visibleLayers = string.Join(", ", GetVisibleLayers(scale).Select(l => l.Name));
            if (visibleLayers != _previousVisibleLayers)
            {
                AnnounceRequested?.Invoke("Visible Layers " + visibleLayers, "notifyVisibleLayers");
                _previousVisibleLayers = visibleLayers;
            }
        }

        private async Task AnnounceCenterLocationAsync(MapPoint center, CancellationToken token)
        {
            var adminLocationAtCenter = await GetAdminLocationFromFeaturesAsync(center, token);
            if (adminLocationAtCenter != _previousAdminLocation)
            {
                var message = !string.IsNullOrEmpty(adminLocationAtCenter)
                    ? "Map centered at unknown location"
                    : $"Map centered at, 1 {SpeakFriendly(adminLocationAtCenter)}";
                AnnounceRequested?.Invoke(message, "notifyCenterLocation");
                _previousAdminLocation = adminLocationAtCenter;
            }
        }

        private async Task AnnounceIncomeStatisticsAsync(CancellationToken token)
        {
            var (maxIncome, maxFeatures) = await GetIncomeStatsFromVisibleLayersAsync();
            if (maxFeatures == null)
            {
                AnnounceRequested?.Invoke("No income data available in the current extent.", "notifyIncomeStats");
                return;
            }

            int startingKey = _keyboardShortcutFeatures.Count + 1;
            _keyboardShortcutFeatures.AddRange(maxFeatures);
            var maxIncomeMessage = "Maximum median income " + await FormatIncomeMessageAsync("", maxFeatures, startingKey);

            if (maxIncomeMessage != _previousMaxIncomeMessage)
            {
                AnnounceRequested?.Invoke(maxIncomeMessage, "notifyIncomeStats");
                _previousMaxIncomeMessage = maxIncomeMessage;
            }

            for (int i = 0; i < _keyboardShortcutFeatures.Count; i++)
            {
                if (_keyboardShortcutFeatures[i]?.Geometry is Polygon polygon)
                    HighlightPolygonRequested?.Invoke(polygon, (i + 1).ToString());
            }
        }

        private async Task<(double? maxIncome, IEnumerable<Feature>? maxFeatures)> GetIncomeStatsFromVisibleLayersAsync()
        {
            var visibleExtent = _mapView.VisibleArea?.Extent;
            if (visibleExtent == null)
                return (null, null);

            var layer = GetVisibleLayers(_mapView.MapScale).FirstOrDefault(l => l.FeatureTable.Fields.Any(f => f.Name == "B19049_001E"));
            if (layer == null)
                return (null, null);

            try
            {
                var bufferDist = _mapView.UnitsPerPixel * 10;
                var shrunkExtent = GeometryEngine.Buffer(visibleExtent, -bufferDist);

                var statDefs = new[]
                {
                    new StatisticDefinition("B19049_001E", StatisticType.Maximum, "MAX")
                };

                var statParams = new StatisticsQueryParameters(statDefs)
                {
                    Geometry = shrunkExtent,
                    SpatialRelationship = SpatialRelationship.Intersects,
                    WhereClause = "B19049_001E IS NOT NULL"
                };

                var statResult = await layer.FeatureTable.QueryStatisticsAsync(statParams);
                var stats = statResult.FirstOrDefault();
                if (stats == null)
                    return (null, null);

                double? max = stats.Statistics.TryGetValue("MAX", out var maxObj) && maxObj is double maxVal ? maxVal : null;

                QueryParameters incomeQuery = new QueryParameters
                {
                    WhereClause = $"B19049_001E = {max}",
                    Geometry = shrunkExtent,
                    SpatialRelationship = SpatialRelationship.Intersects
                };

                FeatureQueryResult maxF = await layer.FeatureTable.QueryFeaturesAsync(incomeQuery);

                return (max, maxF);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing layer {layer.Name}: {ex.Message}");
                return (null, null);
            }
        }

        private async Task<string> GetAdminLocationFromFeaturesAsync(MapPoint center, CancellationToken token)
        {
            var buffer = _mapView.UnitsPerPixel * 10;
            var searchArea = new Envelope(center.X - buffer, center.Y - buffer, center.X + buffer, center.Y + buffer, center.SpatialReference);

            foreach (var layer in GetVisibleLayers(_mapView.MapScale))
            {
                var result = await layer.FeatureTable.QueryFeaturesAsync(new QueryParameters
                {
                    Geometry = searchArea,
                    SpatialRelationship = SpatialRelationship.Intersects,
                    MaxFeatures = 1
                }, token);

                var feature = result.FirstOrDefault();
                if (feature is ArcGISFeature agf && agf.LoadStatus != LoadStatus.Loaded)
                    await agf.LoadAsync();

                if (feature != null)
                {
                    _keyboardShortcutFeatures.Add(feature);
                    return FormatLocation(feature);
                }
            }
            return "";
        }

        //To say the numbers in the name at the center location individually as 123 and not one hundred and twenty three
        private string SpeakFriendly(string text) =>
            Regex.Replace(text, "\\d+", m => string.Join(" ", m.Value.ToCharArray()));

        private async Task<string> FormatIncomeMessageAsync(string prefix, IEnumerable<Feature> features, int startingKey)
        {
            var parts = new List<string>();
            int key = startingKey;

            foreach (var feature in features)
            {
                if (feature is ArcGISFeature agf && agf.LoadStatus != LoadStatus.Loaded)
                    await agf.LoadAsync();

                var location = FormatLocation(feature);
                parts.Add($"{key}, {SpeakFriendly(location)}");
                key++;
            }

            return $"{prefix}, at {string.Join(", ", parts)}";
        }

        private string FormatLocation(Feature feature)
        {
            var name = TryGetAttribute(feature, "NAME");
            var county = TryGetAttribute(feature, "COUNTY");
            var state = TryGetAttribute(feature, "STATE");
            return !string.IsNullOrEmpty(name) ? $"{name}, {county}, {state}" : "unknown location";
        }

        private static string TryGetAttribute(Feature feature, string field)
        {
            return feature.Attributes.TryGetValue(field, out var value) ? value?.ToString() ?? "" : "";
        }

        private List<FeatureLayer> GetVisibleLayers(double scale) =>
            GetAllFeatureLayers(_mapView.Map.OperationalLayers).Where(l => l.IsVisibleAtScale(scale)).ToList();

        private IEnumerable<FeatureLayer> GetAllFeatureLayers(LayerCollection layers)
        {
            foreach (var layer in layers)
            {
                if (layer is GroupLayer group)
                    foreach (var sub in GetAllFeatureLayers(group.Layers))
                        yield return sub;
                else if (layer is FeatureLayer fl)
                    yield return fl;
            }
        }

        private Popup? GetPopup(IdentifyLayerResult result)
        {
            var popup = result?.Popups?.FirstOrDefault();
            if (popup?.GeoElement is DynamicEntityObservation deo)
            {
                return new Popup(deo.GetDynamicEntity() ?? (GeoElement)deo, popup.PopupDefinition);
            }

            return popup ?? GetPopup(result?.SublayerResults);
        }

        private Popup? GetPopup(IEnumerable<IdentifyLayerResult> results)
        {
            if (results == null) return null;

            foreach (var result in results)
            {
                var popup = GetPopup(result);
                if (popup != null)
                    return popup;
            }

            return null;
        }

        private Popup? BuildPopupFromGeoElement(IdentifyLayerResult result)
        {
            var geoElement = result?.GeoElements?.FirstOrDefault();
            if (geoElement is DynamicEntityObservation obs)
                geoElement = obs.GetDynamicEntity() ?? (GeoElement)obs;

            if (geoElement != null)
            {
                if (result?.LayerContent is IPopupSource source && source.PopupDefinition != null)
                    return new Popup(geoElement, source.PopupDefinition);

                return Popup.FromGeoElement(geoElement);
            }

            return BuildPopupFromGeoElement(result?.SublayerResults);
        }

        private Popup? BuildPopupFromGeoElement(IEnumerable<IdentifyLayerResult> results)
        {
            if (results == null) return null;

            foreach (var result in results)
            {
                var popup = BuildPopupFromGeoElement(result);
                if (popup != null)
                    return popup;
            }

            return null;
        }
    }

}