using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using Location = Esri.ArcGISRuntime.Location.Location;

namespace BackgroundLocationTracking
{
    public partial class MainPage : ContentPage
    {
        private LocationDataSource? _locationDataSource;
        private GraphicsOverlay? _locationHistoryLineOverlay;
        private PolylineBuilder? _polylineBuilder;

        public MainPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                MyMapView.Map = new Esri.ArcGISRuntime.Mapping.Map(BasemapStyle.ArcGISTopographic);
#if IOS
                _locationDataSource = new SystemLocationDataSource
                {
                    AllowsBackgroundLocationUpdates = true
                };
#else
                    _locationDataSource = new SystemLocationDataSource();
#endif
                _locationDataSource.LocationChanged += LocationDataSource_LocationChanged;
                _locationHistoryLineOverlay = new GraphicsOverlay();
                MyMapView?.GraphicsOverlays?.Add(_locationHistoryLineOverlay);
                _polylineBuilder = new PolylineBuilder(SpatialReferences.WebMercator);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: An error occurred during initialization: {ex.Message}");
            }
        }

        private async Task StartTracking(object sender, EventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;

            if (await CheckAndRequestLocationPermission() is not PermissionStatus.Granted)
            {
                StartButton.IsEnabled = true;
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
            StopButton.IsEnabled = true;
        }

        private async Task StartLocationDataSource()
        {
            _locationHistoryLineOverlay?.Graphics.Clear();
            _polylineBuilder?.Clear();
            if (_locationDataSource is not null)
            {
                await _locationDataSource.StartAsync();

                MyMapView.LocationDisplay.DataSource = _locationDataSource;
                MyMapView.LocationDisplay.IsEnabled = true;
                MyMapView.LocationDisplay.InitialZoomScale = 1000;
                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            }
        }

        private void LocationDataSource_LocationChanged(object? sender, Location e)
        {
            if (e is not null)
            {
                var message = $"Lat: {e.Position.Y:F6}, Lon: {e.Position.X:F6}, Time: {DateTime.Now:HH:mm:ss}";
                Console.WriteLine($"Location Update: {message}");

                var projectedPoint = (MapPoint)GeometryEngine.Project(e.Position, SpatialReferences.WebMercator);
                _polylineBuilder?.AddPoint(projectedPoint);
                _locationHistoryLineOverlay?.Graphics.Clear();
                _locationHistoryLineOverlay?.Graphics.Add(new Graphic(_polylineBuilder?.ToGeometry()));
            }
        }

        private async Task StopTracking(object sender, EventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;

#if ANDROID
                var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(LocationService));
                
                // Stop the foreground service when tracking is stopped.
                Android.App.Application.Context.StopService(intent);
#endif
            await StopLocationDataSource();
            StartButton.IsEnabled = true;
        }

        private async Task StopLocationDataSource()
        {
            if (_locationDataSource is not null)
            {
                await _locationDataSource.StopAsync();
                MyMapView.LocationDisplay.IsEnabled = false;
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
    }

}
