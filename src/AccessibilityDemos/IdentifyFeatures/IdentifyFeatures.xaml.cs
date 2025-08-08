using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.RealTime;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace DemoApplicationAccessibility.IdentifyFeatures
{
    public class FeatureItem
    {
        public string LayerName { get; set; }
        public GeoElement Feature { get; set; }
        public string DisplayName { get; set; }
        private int _indexOnPage;
        public int IndexOnPage
        {
            get => _indexOnPage;
            set
            {
                if (_indexOnPage != value)
                {
                    _indexOnPage = value;
                    OnPropertyChanged(nameof(IndexOnPage));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public partial class IdentifyFeatures : UserControl
    {
        private CollectionViewSource _collectionView;
        private CancellationTokenSource _identifyCts = new();
        private ObservableCollection<FeatureItem> _featureItems = new ObservableCollection<FeatureItem>();
        private int _currentPage = 0;
        private const int PageSize = 7;
        public ObservableCollection<FeatureItem> FeatureItems => _featureItems;
        public IdentifyFeatures()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ViewFeatures_Loaded;
        }
        private void ViewFeatures_Loaded(object sender, RoutedEventArgs e)
        {
            _collectionView = (CollectionViewSource)FindResource("GroupedFeatures");
            _collectionView.View.Filter = PagingFilter;
            _collectionView.View.Refresh();
        }
        private bool PagingFilter(object item)
        {
            var index = FeatureItems.IndexOf(item as FeatureItem);
            bool inPage = index >= _currentPage * PageSize && index < (_currentPage + 1) * PageSize;
            if (inPage)
                (item as FeatureItem).IndexOnPage = index - (_currentPage * PageSize) + 1;
            return inPage;
        }
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!FeatureItems.Any()) return;
            if (e.Key >= Key.D1 && e.Key <= Key.D7)
            {
                var pagedItems = FeatureItems.Skip(_currentPage * PageSize).Take(PageSize).ToList();
                int indexOnPage = e.Key - Key.D1;

                if (indexOnPage < pagedItems.Count)
                {
                    var selectedItem = pagedItems[indexOnPage];
                    FeaturesListBox.SelectedItem = selectedItem;
                    FeaturesListBox.ScrollIntoView(selectedItem);
                    ShowFeaturePopup(selectedItem.Feature);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.D9) // Next page
            {
                if ((_currentPage + 1) * PageSize < FeatureItems.Count)
                {
                    _currentPage++;
                    _collectionView.View.Refresh();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.D8) // Previous page
            {
                if (_currentPage > 0)
                {
                    _currentPage--;
                    _collectionView.View.Refresh();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                HidePopup();
            }
        }
        private void ShowFeaturePopup(GeoElement feature)
        {
            if (feature is Feature f && f.FeatureTable?.Layer is IPopupSource src)
            {
                PopupViewer.Popup = new Popup(feature, src.PopupDefinition);
                PopupContainer.Visibility = Visibility.Visible;
                return;
            }
            PopupViewer.Popup = Popup.FromGeoElement(feature);
            PopupContainer.Visibility = Visibility.Visible;
        }
        private async void MyMapView_ViewpointChanged(object sender, EventArgs e)
        {
            _identifyCts.Cancel();
            _identifyCts = new CancellationTokenSource();
            await IdentifyFeaturesAsync(_identifyCts.Token);
        }
        private void FeaturesListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && FeaturesListBox.SelectedItem is FeatureItem selectedItem)
            {
                ShowFeaturePopup(selectedItem.Feature);
            }
        }
        private void FeaturesListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FeaturesListBox.SelectedItem is FeatureItem selectedItem)
            {
                ShowFeaturePopup(selectedItem.Feature);
            }
        }
        private async Task IdentifyFeaturesAsync(CancellationToken ct)
        {
            try
            {
                HidePopup();

                var screenCenter = new Point(MyMapView.ActualWidth / 2, MyMapView.ActualHeight / 2);
                var mapCenter = MyMapView.ScreenToLocation(screenCenter);
                if (mapCenter == null || ct.IsCancellationRequested) return;

                double rectangleHalfLength = 150;
                var rightPoint = new Point(screenCenter.X + rectangleHalfLength, screenCenter.Y);
                var rightMap = MyMapView.ScreenToLocation(rightPoint);
                double mapDistance = GeometryEngine.Distance(mapCenter, rightMap);

                var envelope = new Envelope(
                    mapCenter.X - mapDistance,
                    mapCenter.Y - mapDistance,
                    mapCenter.X + mapDistance,
                    mapCenter.Y + mapDistance,
                    mapCenter.SpatialReference);

                var newItems = new List<FeatureItem>();

                foreach (var layer in MyMapView.Map.OperationalLayers)
                {
                    if (layer is FeatureLayer featureLayer && featureLayer.IsVisibleAtScale(MyMapView.MapScale))
                    {
                        var query = new QueryParameters { Geometry = envelope, SpatialRelationship = SpatialRelationship.Intersects };
                        var results = await featureLayer.FeatureTable.QueryFeaturesAsync(query, ct);

                        foreach (var feature in results)
                        {
                            string name = feature.Attributes.ContainsKey("Name") ? feature.Attributes["Name"].ToString() : "Unnamed Feature";
                            newItems.Add(new FeatureItem
                            {
                                LayerName = featureLayer.Name,
                                Feature = feature,
                                DisplayName = name
                            });
                        }
                    }
                }
                if (ct.IsCancellationRequested) return;

                FeatureItems.Clear();
                foreach (var item in newItems)
                    FeatureItems.Add(item);

                _currentPage = 0;
                _collectionView.View.Refresh();
            }
            catch (OperationCanceledException)
            {
                // Trown when canceled, safe to ignore
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void HidePopup()
        {
            PopupContainer.Visibility = Visibility.Collapsed;
            PopupViewer.Popup = null;
            FeaturesListBox.SelectedIndex = -1;
        }
        private void PopupBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HidePopup();
        }
        private async void popupViewer_PopupAttachmentClicked(object sender, PopupAttachmentClickedEventArgs e)
        {
            if (!e.Attachment.IsLocal) // Attachment hasn't been downloaded
            {
                try
                {
                    // Make first click just load the attachment (or cancel a loading operation). Otherwise fallback to default behavior
                    if (e.Attachment.LoadStatus == LoadStatus.NotLoaded)
                    {
                        e.Handled = true;
                        await e.Attachment.LoadAsync();
                    }
                    else if (e.Attachment.LoadStatus == LoadStatus.FailedToLoad)
                    {
                        e.Handled = true;
                        await e.Attachment.RetryLoadAsync();
                    }
                    else if (e.Attachment.LoadStatus == LoadStatus.Loading)
                    {
                        e.Handled = true;
                        e.Attachment.CancelLoad();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to download attachment", ex.Message);
                }
            }
        }
        private void popupViewer_LinkClicked(object sender, HyperlinkClickedEventArgs e)
        {
            Debug.WriteLine(e.Uri);
        }
        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                var result = await MyMapView.IdentifyLayersAsync(e.Position, 3, false);
                var popup = GetPopup(result) ?? BuildPopupFromGeoElement(result);
                if (popup != null)
                {
                    PopupContainer.Visibility = Visibility.Visible;
                    PopupViewer.Popup = popup;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }
        private Popup GetPopup(IdentifyLayerResult result)
        {
            if (result == null)
            {
                return null;
            }
            var popup = result.Popups.FirstOrDefault();
            if (popup != null)
            {
                if (popup.GeoElement is DynamicEntityObservation deo)
                {
                    return new Popup(deo.GetDynamicEntity(), popup.PopupDefinition);
                }
                return popup;
            }
            return GetPopup(result.SublayerResults);
        }
        private Popup GetPopup(IEnumerable<IdentifyLayerResult> results)
        {
            if (results == null)
                return null;
            foreach (var result in results)
            {
                var popup = GetPopup(result);
                if (popup != null)
                    return popup;
            }
            return null;
        }
        private Popup BuildPopupFromGeoElement(IdentifyLayerResult result)
        {
            if (result == null)
                return null;
            var geoElement = result.GeoElements.FirstOrDefault();
            if (geoElement != null)
            {
                if (geoElement is DynamicEntityObservation obs)
                    geoElement = obs.GetDynamicEntity();
                if (result.LayerContent is IPopupSource source)
                {
                    var popupDefinition = source.PopupDefinition;
                    if (popupDefinition != null)
                    {
                        return new Popup(geoElement, popupDefinition);
                    }
                }
                return Popup.FromGeoElement(geoElement);
            }
            return BuildPopupFromGeoElement(result.SublayerResults);
        }
        private Popup BuildPopupFromGeoElement(IEnumerable<IdentifyLayerResult> results)
        {
            if (results == null)
            {
                return null;
            }
            foreach (var result in results)
            {
                var popup = BuildPopupFromGeoElement(result);
                if (popup != null)
                {
                    return popup;
                }
            }
            return null;
        }
    }
}