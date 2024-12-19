using Esri.ArcGISRuntime.Location;
using Location = Esri.ArcGISRuntime.Location.Location;

namespace BackgroundLocationTracking
{
    public partial class MainPage : ContentPage
    {
        private LocationDataSource? _locationDataSource;

        public MainPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                _locationDataSource = new SystemLocationDataSource()
                {
#if IOS
                    AllowsBackgroundLocationUpdates = true 
#endif
                };
            }
            catch (Exception ex)
            {
                // Handle initialization errors
                LogMessage.Text = $"Error: An error occurred during initialization: {ex.Message}";
            }
        }

        /// <summary>
        /// Starts the location service.
        /// </summary>
        private async void StartTracking(object sender, EventArgs e)
        {
            if (_locationDataSource is null ||
                            await CheckAndRequestLocationPermission() is not PermissionStatus.Granted)
            {
                return; // Exit if the location data source is not initialized.
            }
#if ANDROID
            var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(LocationService));
            // Check if the Android version is at least Oreo (API level 26)
            // Start the service as a foreground service
            _ = Android.App.Application.Context.StartForegroundService(intent);
#endif
            await StartLocationDataSource();
        }

        private async Task StartLocationDataSource()
        {
            if (_locationDataSource is not null)
            {
                // Subscribe to the LocationChanged event
                _locationDataSource.LocationChanged += LocationDataSource_LocationChanged;

                // Start the location data source
                await _locationDataSource.StartAsync(); 
            }
        }

        private void LocationDataSource_LocationChanged(object? sender, Location e)
        {
            if (e != null)
            {
                // Format the location update message
                var message = $"Lat: {e.Position.Y:F6}, Lon: {e.Position.X:F6}, Time: {DateTime.Now:HH:mm:ss}";

                // Log location update
                LogMessage.Text += $"Location Update: {message}\n";
            }
        }

        /// <summary>
        /// Stops the location service.
        /// </summary>
        private async void StopTracking(object sender, EventArgs e)
        {
#if ANDROID
            var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(LocationService));
            Android.App.Application.Context.StopService(intent);
#endif
            await StopLocationDataSource();
        }

        private async Task StopLocationDataSource()
        {
            if (_locationDataSource is not null)
            {
                // Stop the location data source
                await _locationDataSource.StopAsync();

                // Unsubscribe from the LocationChanged event
                _locationDataSource.LocationChanged -= LocationDataSource_LocationChanged;
            }
        }

        // Checks and requests location permission
        private async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            // Check the current location permission status
            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status == PermissionStatus.Denied || status == PermissionStatus.Unknown)
            {
                // Request location permission if denied or unknown
                await Shell.Current.DisplayAlert("Access Requested", "Please allow precise location all the time to track while phone is locked or viewing other applications.", "OK");
                status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            }

            return status;
        }
    }

}
