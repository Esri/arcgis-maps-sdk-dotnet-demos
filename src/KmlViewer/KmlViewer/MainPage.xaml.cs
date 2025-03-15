using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using Esri.ArcGISRuntime.Ogc;
using System.Diagnostics;

namespace KmlViewer
{
    /// <summary>
	/// Main KML Viewer page - Contains only the view-specific code
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Day.Value = DateTime.Now.DayOfYear;
            Hour.Value = DateTime.Now.TimeOfDay.TotalHours;
            Initialize();
        }

        public MainPageVM VM { get; } = new MainPageVM();

        private async void Initialize()
        {
            LoadLocation();
            if (VM.Is3D)
            {
                Camera homeCamera = new Camera(41, -180, 31000000, 0, 0, 0);
                sceneView.SetViewpointCamera(homeCamera);
                if (position != null)
                {
                    homeCamera = new Camera(position.Coordinate.Point.Position.Latitude, position.Coordinate.Point.Position.Longitude, 11000000, 0, 0, 0);
                }
                else
                    homeCamera = new Camera(41, -92, 11000000, 0, 0, 0);
                await sceneView.SetViewpointCameraAsync(homeCamera, new TimeSpan(0, 0, 4));
            }
        }

        private Windows.Devices.Geolocation.Geoposition position;

        private async void LoadLocation()
        {
            Windows.Devices.Geolocation.Geoposition l = null;
            try
            {
                var loc = new Windows.Devices.Geolocation.Geolocator();
                l = position = await loc.GetGeopositionAsync();

            }
            catch { return; }
            foreach (var coll in new GraphicsOverlayCollection[] {
                sceneView.GraphicsOverlays , mapView.GraphicsOverlays
            })
            {
                GraphicsOverlay overlay = new GraphicsOverlay();
                coll.Add(overlay);
                Graphic g = new Graphic()
                {
                    Symbol = new SimpleMarkerSymbol()
                    {
                        Color = System.Drawing.Color.FromArgb(255, 0, 122, 194),
                        Size = 20,
                        Outline = new SimpleLineSymbol()
                        {
                            Width = 2,
                            Color = System.Drawing.Color.White
                        }
                    },

                };
                g.Geometry = new MapPoint(l.Coordinate.Point.Position.Longitude, l.Coordinate.Point.Position.Latitude, SpatialReferences.Wgs84);
                overlay.Graphics.Add(g);
                break;
            }
        }

        public void LoadKml(Windows.ApplicationModel.Activation.IFileActivatedEventArgs args)
        {
            var file = args.Files.FirstOrDefault() as Windows.Storage.StorageFile;
            AddKmlLayer("file:///" + file.Path);
        }

        private async void AddKmlLayer(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            Uri uri;
            try
            {
                uri = new Uri(url, UriKind.RelativeOrAbsolute);
            }
            catch (System.Exception ex)
            {
                Debugger.Break(); //TODO
                //var _ = new MessageDialog("Invalid url: " + ex.Message).ShowAsync();
                return;
            }
            if (uri.Scheme == "ms-appx")
            {
                var path = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                uri = new Uri("file:///" + path.Path.Replace("\\", "/"));
            }
            var layer = VM.LoadKmlLayer(uri.OriginalString.Replace("file:///", ""));

            try
            {
                await layer.LoadAsync();
            }
            catch (System.Exception ex)
            {
                Debugger.Break(); //TODO
                // var _ = new MessageDialog(ex.Message).ShowAsync();
                return;
            }

            sidePanel.SelectedIndex = 0;
            try
            {
                await layer.LoadAsync();
                var root = layer.Dataset?.RootNodes?.FirstOrDefault();
                if (root == null)
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                    System.ComponentModel.PropertyChangedEventHandler handler = null;
                    handler = (s, e) =>
                    {
                        if (e.PropertyName == nameof(KmlLayer.Dataset))
                        {
                            layer.PropertyChanged -= handler;
                            tcs.SetResult(null);
                        }
                    };
                    layer.PropertyChanged += handler;
                    await tcs.Task;
                    root = layer.Dataset?.RootNodes?.FirstOrDefault();
                }

                //Viewpoint? viewPoint = null;
                //if (layer.FullExtent == null && layer.RootFeature != null)
                //{
                //    viewPoint = layer.RootFeature.Viewpoint;
                //}
                //if(viewPoint == null && layer.FullExtent != null)
                //    viewPoint = new Viewpoint(layer.FullExtent);
                //if (viewPoint != null)
                //{
                //    if (VM.Is3D)
                //        sceneView.SetViewpointAsync(viewPoint);
                //    else
                //        mapView.SetViewpointAsync(viewPoint);
                //}
            }
            catch { }
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((KmlViewer.App)App.Current).Window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Users expect to have a filtered view of their folders depending on the scenario.
            // For example, when choosing a documents folder, restrict the filetypes to documents for your application.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".kml");
            openPicker.FileTypeFilter.Add(".kmz");
            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                AddKmlLayer("file:///" + file.Path);
            }
        }

        private void LoadFromUrlButton_Click(object sender, RoutedEventArgs e)
        {
            AddKmlLayer(kmlUrl.Text);
        }

        private void LoadSampleButton_Click(object sender, RoutedEventArgs e)
        {
            var sample = (KmlSample)((FrameworkElement)sender).DataContext;
            AddKmlLayer(sample.Path);
            if (sample.InitialViewpoint != null)
            {
                GeoView view = VM.Is3D ? (GeoView)sceneView : (GeoView)mapView;
                var _ = view.SetViewpointAsync(sample.InitialViewpoint);
            }
        }

        private void LoadKmlButton_Click(object sender, RoutedEventArgs e)
        {
            sidePanel.SelectedIndex = 3;
        }

        KmlNode highlightedFeature;
        private void OnFeatureTapped(object sender, TreeViewItemInvokedEventArgs args)
        {
            var node = args.InvokedItem as KmlNode;
            if (node != null)
                HighlightFeature(node);
        }

        private void HighlightFeature(KmlNode feature)
        {
            if (highlightedFeature != null)
                highlightedFeature.IsHighlighted = false;
            if (highlightedFeature == feature)
            {
                highlightedFeature = null;
                return;
            }
            if (feature != null)
                feature.IsHighlighted = true;

            highlightedFeature = feature;
        }

        private UIElement currentMaptip;

        private async void ShowMapTip(KmlNode feature, MapPoint location = null)
        {
            GeoView view = VM.Is3D ? (GeoView)sceneView : (GeoView)mapView;
            var placemark = feature as KmlPlacemark;
            var border = (Border)view.Overlays.Items.First();
            if (feature == null || string.IsNullOrWhiteSpace(feature.BalloonContent) ||
                placemark?.Geometry == null)
            {
                border.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                currentMaptip = null;
                return;
            }
            var grid = (Microsoft.UI.Xaml.Controls.Grid)border.Child;
            var webview = grid.Children.OfType<WebView2>().First();
            await webview.EnsureCoreWebView2Async();
            webview.NavigateToString(feature.BalloonContent);
            GeoView.SetViewOverlayAnchor(border, location ?? placemark.Geometry.Extent.GetCenter());
            border.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            currentMaptip = border;
        }

        private void CloseTip_Clicked(object sender, RoutedEventArgs e)
        {
            ShowMapTip(null);
        }

        private void OnFeatureDoubleTapped(object sender, KmlNode feature)
        {
            var kmlvp = feature.Viewpoint;
            if (kmlvp != null)
            {
                Viewpoint vp = null;
                if (kmlvp.Type == KmlViewpointType.Camera)
                    vp = new Viewpoint(kmlvp.Location, new Camera(kmlvp.Location, kmlvp.Heading, kmlvp.Pitch, kmlvp.Roll));
                else
                    vp = new Viewpoint(kmlvp.Location, new Camera(kmlvp.Location, kmlvp.Range, kmlvp.Heading, kmlvp.Pitch, kmlvp.Roll));
                if (VM.Is3D)
                {
                    sceneView.SetViewpointAsync(vp);
                    sceneView.Focus(FocusState.Keyboard);
                }
                else
                {
                    mapView.Focus(FocusState.Keyboard);
                    mapView.SetViewpointAsync(vp);
                }
                if (highlightedFeature != null)
                    highlightedFeature.IsHighlighted = false;
                feature.IsHighlighted = true;
                highlightedFeature = feature;
            }
        }

        private async void Is3DCheckBox_Toggled(object sender, RoutedEventArgs e)
        {
            if (sceneView != null && mapView != null)
            {
                bool isOn = ((ToggleSwitch)sender).IsOn;
                if (isOn == VM.Is3D)
                    return; //this happens the first time it loads
                HighlightFeature(null);
                if (VM.Is3D)
                {
                    var c = sceneView.Camera;
                    //If there's tilt or heading, reset before switching to 2D
                    if (c.Pitch > 1 || Math.Abs(c.Heading) > 1)
                    {
                        var center = await sceneView.ScreenToLocationAsync(new Point(sceneView.ActualWidth * .5, sceneView.ActualHeight * .5));
                        c = c.RotateAround(center, c.Heading, -c.Pitch, 0).RotateTo(0, 0, c.Roll);
                        await sceneView.SetViewpointCameraAsync(c);
                    }
                }
                GeoView from = VM.Is3D ? (GeoView)sceneView : (GeoView)mapView;
                GeoView to = !VM.Is3D ? (GeoView)sceneView : (GeoView)mapView;
                var vp = from.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                if (vp != null)
                {
                    to.SetViewpoint(vp);
                }
                VM.Is3D = !VM.Is3D;
            }
        }

        public void OnLocationPicked(object sender, Esri.ArcGISRuntime.Geometry.Geometry location)
        {
            if (VM.Is3D)
            {
                sceneView.SetViewpointAsync(new Viewpoint(location));
                sceneView.Focus(FocusState.Keyboard);
            }
            else
            {
                mapView.Focus(FocusState.Keyboard);
                mapView.SetViewpointGeometryAsync(location);
            }
        }

        private async void ViewTapped(object sender, GeoViewInputEventArgs e)
        {
            GeoView view = (GeoView)sender;
            KmlLayer kmlLayer = null;
            if (view is SceneView)
                kmlLayer = ((SceneView)sender).Scene.OperationalLayers.OfType<KmlLayer>().FirstOrDefault();
            else if (view is MapView)
                kmlLayer = ((MapView)sender).Map.OperationalLayers.OfType<KmlLayer>().FirstOrDefault();
            if (kmlLayer == null)
                return;
            var result = await view.IdentifyLayerAsync(kmlLayer, e.Position, 2, false);
            var feature = result.GeoElements?.OfType<KmlGeoElement>().FirstOrDefault()?.KmlNode;
            HighlightFeature(feature);
            ShowMapTip(feature, e.Location);
        }
        private void WebView_FrameNavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            if (args.Uri != null && args.Uri.StartsWith("http"))
            {
                args.Cancel = true;
                var _ = Windows.System.Launcher.LaunchUriAsync(new Uri(args.Uri));
            }
        }

        private void Month_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Day == null || Hour == null) return;
            var date = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).Date.AddDays((int)Day.Value).AddHours(Hour.Value);
            sceneView.SunTime = date;
        }

        private void SetTimeOfDay_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.UtcNow;
            Day.Value = now.DayOfYear - 1;
            Hour.Value = now.TimeOfDay.TotalHours;
        }

        private void SliderValueTick(object sender, double value)
        {
            var c = sceneView.Camera;
            if (c.Pitch + value > 0 && c.Pitch + value < 180)
            {
                c = c.RotateTo(c.Heading, c.Pitch + value, c.Roll);
                sceneView.SetViewpointCamera(c);
            }
        }

        private void ViewChanged(object sender, EventArgs e)
        {
            //If a maptip is open, hide it when the view starts navigating
            if (currentMaptip != null)
            {
                ShowMapTip(null);
            }
        }

        private void HelpIcon_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            aboutView.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        private void TableOfContents_ContextMenuRequested(object sender, ContextMenuRequestedEventArgs e)
        {
            if(e.TreeViewNode is KmlNode node)
            {
                if(node.Viewpoint != null)
                {
                    var item = new MenuFlyoutItem() { Text = "Fly to" };
                    Camera camera = null;
                    if (node.Viewpoint.Type == KmlViewpointType.LookAt)
                        camera = new Camera(node.Viewpoint.Location, node.Viewpoint.Range, node.Viewpoint.Heading, node.Viewpoint.Pitch, node.Viewpoint.Roll);
                    else
                        camera = new Camera(node.Viewpoint.Location, node.Viewpoint.Heading, node.Viewpoint.Pitch, node.Viewpoint.Roll);
                    
                    item.Click += (s, e2) => sceneView.SetViewpointAsync(new Viewpoint(node.Viewpoint.Location, camera));
                    e.MenuItems.Add(item);
                }
                else if(node is KmlPlacemark kp && kp.Geometry != null)
                {
                    var item = new MenuFlyoutItem() { Text = "Fly to" };
                    item.Click += (s, e2) => sceneView.SetViewpointAsync(new Viewpoint(kp.Geometry));
                    e.MenuItems.Add(item);
                }
                if (node is KmlNetworkLink knl)
                {
                    var item = new MenuFlyoutItem() { Text = "Refresh" };
                    item.Click += (s, e2) => knl.Refresh();
                    e.MenuItems.Add(item);
                    if (knl.RefreshMode == KmlRefreshMode.OnInterval)
                    {
                        item = new MenuFlyoutItem() { Text = "Disable autorefresh" };
                        item.Click += (s, e2) => knl.RefreshMode = KmlRefreshMode.OnChange;
                        e.MenuItems.Add(item);
                    }
                }
                else if (node is KmlTour tour)
                {
                    var item = new MenuFlyoutItem() { Text = tour.TourStatus == KmlTourStatus.Playing ? "Stop" : "Play" };
                    item.Click += (s, e2) => ToggleTour(tour);
                    e.MenuItems.Add(item);
                }
            }
        }

        private void ToggleTour(KmlTour tour)
        {
            if (tourController == null)
                tourController = new KmlTourController();
            if (tourController.Tour != null && tourController.Tour != tour)
            {
                tourController.Pause();
                tourController.Reset();
            }
            if(tourController.Tour != tour)
            {
                tourController.Tour = tour;
            }
            if(tour.TourStatus == KmlTourStatus.Playing)
            {
                tourController.Pause();
                tourController.Reset();
            }
            else
            {
                tourController.Play();
            }
        }

        private KmlTourController tourController;

        private bool isPanePinned = true;

        private void BurgerButton_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isPanePinned)
            {
                splitView.DisplayMode = SplitViewDisplayMode.Inline;
                splitView.IsPaneOpen = true;
                isPanePinned = true;
            }
            else
            {
                splitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                splitView.IsPaneOpen = false;
                isPanePinned = false;
            }
        }

        private void SidePanel_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!isPanePinned)
                splitView.IsPaneOpen = true;
        }

        private void SidePanel_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!isPanePinned)
                splitView.IsPaneOpen = false;
        }
    }
}