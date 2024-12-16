using CommunityToolkit.Mvvm.Messaging;
using Esri.ArcGISRuntime.Location;

namespace BackgroundLocationTracking
{
    public class LocationService
    {
        private LocationDataSource? _locationDataSource;

        public LocationService()
        {
            // Initialize the location data source with background updates enabled
            _locationDataSource = new SystemLocationDataSource()
            {
                AllowsBackgroundLocationUpdates = true
            };
        }

        // Starts the location service
        public async Task Start()
        {
            if (_locationDataSource is null ||
                await CheckAndRequestLocationPermission() is not PermissionStatus.Granted)
            {
                return; // Exit if the location data source is not initialized.
            }

            // Subscribe to the LocationChanged event
            _locationDataSource.LocationChanged += LocationDataSource_LocationChanged;

            // Start the location data source
            await _locationDataSource.StartAsync();
        }

        // Event handler for location changes
        private void LocationDataSource_LocationChanged(object? sender, Esri.ArcGISRuntime.Location.Location e)
        {
            // Send the location data using the WeakReferenceMessenger
            WeakReferenceMessenger.Default.Send(e);
        }

        // Stops the location service
        public async Task Stop()
        {
            if (_locationDataSource != null)
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
