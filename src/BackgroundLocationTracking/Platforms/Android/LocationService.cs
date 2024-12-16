using Android.App;
using Android.Content;
using Android.OS;
using CommunityToolkit.Mvvm.Messaging;
using Esri.ArcGISRuntime.Location;
using Location = Esri.ArcGISRuntime.Location.Location;

namespace BackgroundLocationTracking
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        private LocationDataSource? _locationDataSource;

        // This method is called when the service is bound to an activity.
        public override IBinder? OnBind(Intent? intent) => null;

        // This method is called when the service is started.
        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            var notificationHelper = new NotificationHelper(this);
            var notification = notificationHelper.GetServiceStartedNotification();
            StartForeground(1, notification); // Start the service in the foreground with a notification.
            _ = Start(); // Start the location tracking.
            return StartCommandResult.Sticky; // Ensure the service is restarted if it gets terminated.
        }

        public override void OnCreate()
        {
            base.OnCreate();
            _locationDataSource = new SystemLocationDataSource();
        }

        private async Task Start()
        {
            if (_locationDataSource is null ||
                await CheckAndRequestLocationPermission() is not PermissionStatus.Granted)
            {
                return; // Exit if the location data source is not initialized.
            }

            await _locationDataSource.StartAsync(); // Start the location data source.

            _locationDataSource.LocationChanged += LocationDataSource_LocationChanged; // Subscribe to location changes.
        }

        // This method handles location changes.
        private void LocationDataSource_LocationChanged(object? sender, Location e)
        {
            WeakReferenceMessenger.Default.Send(e); // Send the new location using the messenger.
        }

        private async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status == PermissionStatus.Denied || status == PermissionStatus.Unknown)
            {
                await Shell.Current.DisplayAlert("Access Requested", "Please allow precise location all the time to track while phone is locked or viewing other applications.", "OK");
                status = await Permissions.RequestAsync<Permissions.LocationAlways>(); // Request location permissions.
            }

            return status; // Return the permission status.
        }

        // This method is called when the service is destroyed.
        public override void OnDestroy()
        {
            base.OnDestroy();
            _ = Stop(); // Stop the location tracking.
        }

        private async Task Stop()
        {
            if (_locationDataSource != null)
            {
                await _locationDataSource.StopAsync(); // Stop the location data source.
                _locationDataSource.LocationChanged -= LocationDataSource_LocationChanged; // Unsubscribe from location changes.
            }
        }
    }

}