using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace BackgroundLocationTracking
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        // This method is called when the service is bound to an activity.
        public override IBinder? OnBind(Intent? intent) => null;

        // This method is called when the service is started.
        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            var notification = GetServiceStartedNotification(this);

            // Start the service in the foreground with a notification.
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                StartForeground(1, notification, Android.Content.PM.ForegroundService.TypeLocation);
            }
            else
            {
                StartForeground(1, notification);
            }

            return base.OnStartCommand(intent, flags, startId);
        }

        // Creates and returns a notification for the location service
        public Notification GetServiceStartedNotification(Context context)
        {
            string channelId = "LocationServiceChannel";
            // Create the notification channel if necessary for Android 8.0 and above
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var channel = new NotificationChannel(channelId, "Location Service Channel", NotificationImportance.Default);
                var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);
            }

            // Create an intent to launch the MainActivity when the notification is tapped
            var intent = new Intent(context, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            // Build and return the notification
            return new NotificationCompat.Builder(context, channelId)
                .SetContentTitle("Location Tracking")
                .SetContentText("Tracking your location")
                .SetSmallIcon(Resource.Drawable.dotnet_bot)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent)
                .Build();
        }
    }

}