using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace BackgroundLocationTracking
{
    public class NotificationHelper
    {
        private const string ChannelId = "LocationServiceChannel";
        private readonly Context _context;

        // Constructor to initialize the NotificationHelper with a context
        public NotificationHelper(Context context)
        {
            _context = context;
        }

        // Creates and returns a notification for the location service
        public Notification GetServiceStartedNotification()
        {
            // Create the notification channel if necessary for Android 8.0 and above
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var channel = new NotificationChannel(ChannelId, "Location Service Channel", NotificationImportance.Default);
                var notificationManager = _context.GetSystemService(Context.NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);
            }

            // Create an intent to launch the MainActivity when the notification is tapped
            var intent = new Intent(_context, typeof(MainActivity));
            var pendingIntentFlags = OperatingSystem.IsAndroidVersionAtLeast(23) ? PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent;
            var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, pendingIntentFlags);

            // Build and return the notification
            return new NotificationCompat.Builder(_context, ChannelId)
                .SetContentTitle("Location Tracking")
                .SetContentText("Tracking your location")
                .SetSmallIcon(Resource.Drawable.dotnet_bot)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent)
                .Build();
        }
    }
}
