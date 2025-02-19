using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Location = Esri.ArcGISRuntime.Location.Location;

namespace BackgroundLocationTracking
{
    public partial class MainPage : ContentPage
    {
        private LocationDataSource? _locationDataSource;
        private GraphicsOverlay? _trackLineOverlay;
        private PolylineBuilder? _polylineBuilder;
        private bool _isTracking;

        public MainPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MyMapView.Map = new Esri.ArcGISRuntime.Mapping.Map(BasemapStyle.ArcGISTopographic) { MaxScale = 500 };
#if IOS
            // iOS requires the app to have the location capability enabled.
            _locationDataSource = new SystemLocationDataSource
            {
                // Set AllowsBackgroundLocationUpdates to true to allow location updates when the app is in the background.
                AllowsBackgroundLocationUpdates = true,

                // Set ActivityType which is used to determine when location updates should be delivered.
                // This is used to help determine when to turn off GPS hardware to save power.
                ActivityType = CoreLocation.CLActivityType.Other,
            };
#else
            _locationDataSource = new SystemLocationDataSource();
#endif
            _locationDataSource.LocationChanged += LocationDataSource_LocationChanged;

            // Create and add graphics overlay for displaying the trail.
            _trackLineOverlay = new GraphicsOverlay();
            var trackLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.LightGreen, 2);
            _trackLineOverlay.Renderer = new SimpleRenderer(trackLineSymbol);
            MyMapView?.GraphicsOverlays?.Add(_trackLineOverlay);

            _polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
        }

        private async void StartTracking(object sender, EventArgs e)
        {
            if (_isTracking) return;

            _isTracking = true;
            UpdateButtonStates();

            try
            {
                if (await CheckAndRequestLocationPermission() is not PermissionStatus.Granted)
                {
                    _isTracking = false;
                    UpdateButtonStates();
                    return;
                }
#if ANDROID
                var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(LocationService));

                // Foreground Services are only supported and required after Android version Oreo (API level 26)
                // Foreground service is required to keep the service running in the background when the main app is not in the foreground.
                // Start the service as a foreground service.
                _ = Android.App.Application.Context.StartForegroundService(intent);
#endif
                await StartLocationDataSource();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred while starting tracking: {ex.Message}.\n Please try again.", "OK");
                _isTracking = false;
            }
            UpdateButtonStates();
        }

        private async void StopTracking(object sender, EventArgs e)
        {
            if (!_isTracking) return;

            _isTracking = false;
            UpdateButtonStates();

            try
            {
#if ANDROID
                var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(LocationService));

                // Stop the foreground service when tracking is stopped.
                Android.App.Application.Context.StopService(intent);
#endif
                await StopLocationDataSource();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred while stopping tracking: {ex.Message}.\n Please try again.", "OK");
                _isTracking = true;
            }
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            StartButton.IsEnabled = !_isTracking;
            StopButton.IsEnabled = _isTracking;
        }

        private async Task StartLocationDataSource()
        {
            try
            {
                // Clear previous trail before starting the location data source.
                ClearTrail();
                if (_locationDataSource is not null)
                {
                    await _locationDataSource.StartAsync();

                    MyMapView.LocationDisplay.DataSource = _locationDataSource;
                    MyMapView.LocationDisplay.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred while starting the location data source: {ex.Message}.", "OK");
            }
        }

        private async Task StopLocationDataSource()
        {
            if (_locationDataSource is not null)
            {
                await _locationDataSource.StopAsync();
                MyMapView.LocationDisplay.IsEnabled = false;
            }
        }

        private void LocationDataSource_LocationChanged(object? sender, Location e)
        {
            if (e is not null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var projectedPoint = (MapPoint)GeometryEngine.Project(e.Position, SpatialReferences.Wgs84);
                    _polylineBuilder?.AddPoint(projectedPoint);

                    var geometry = _polylineBuilder?.ToGeometry();
                    _trackLineOverlay?.Graphics.Clear();
                    _trackLineOverlay?.Graphics.Add(new Graphic(geometry));
                    MyMapView.SetViewpointGeometryAsync(geometry!, 50);
                });
            }
        }

        private async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status == PermissionStatus.Denied || status == PermissionStatus.Unknown)
            {
                await Shell.Current.DisplayAlert("Access Requested", "Please allow precise location all the time to track while phone is locked or viewing other applications.", "OK");
                status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            }

            return status;
        }

        private void ClearTrail()
        {
            _polylineBuilder?.Parts.Clear();
            _trackLineOverlay?.Graphics.Clear();
        }
    }

}
