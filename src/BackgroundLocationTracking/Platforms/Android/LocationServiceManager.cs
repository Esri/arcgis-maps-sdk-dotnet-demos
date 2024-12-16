using Android.Content;

namespace BackgroundLocationTracking
{
    public static class LocationServiceManager
    {
        /// <summary>
        /// Starts the location service for Android.
        /// </summary>
        public static void StartService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(LocationService));
            // Check if the Android version is at least Oreo (API level 26)
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                // Start the service as a foreground service
                _ = Android.App.Application.Context.StartForegroundService(intent);
            }
            else
            {
                // Start the service as a background service
                _ = Android.App.Application.Context.StartService(intent);
            }
        }

        /// <summary>
        /// Stops the location service.
        /// </summary>
        public static void StopService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(LocationService));
            Android.App.Application.Context.StopService(intent);
        }
    }

}
