# Background Location Tracking

BackgroundLocationTracking is a .NET MAUI application designed to track the location of a device in the background using the [ArcGIS Runtime SDK for .NET](https://developers.arcgis.com/net/).

Android application uses Foreground Service to track the location in the background. iOS application uses [SystemLocationDaatSource.AllowBackgroundLocationUpdates](https://developers.arcgis.com/net/api-reference/api/ios/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Location.SystemLocationDataSource.AllowsBackgroundLocationUpdates.html) to track the location in the background.


![Screenshot](https://github.com/user-attachments/assets/1d8d251f-5b03-44d0-9d15-dadf54983e70)

## Important Configurations

### Android

#### `AndroidManifest.xml`

Ensure the following permissions and service declaration are added:
```xml
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />
<application>
    <service android:name=".LocationService" android:foregroundServiceType="location" />
</application>
```

#### `LocationService.cs`

Define the Android foreground service for location tracking:
```C#
[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeLocation)]
public class LocationService : Service
{
    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var notification = GetServiceStartedNotification(this);
        StartForeground(1, notification, Android.Content.PM.ForegroundService.TypeLocation);
        return base.OnStartCommand(intent, flags, startId);
    }
}
```


### iOS

#### `Info.plist`

Add location usage descriptions and background modes:

```xml
<key>UIBackgroundModes</key>
<array>
    <string>location</string>
</array>
<key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
<string>This app requires background location updates for tracking your position.</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>This app needs access to location when open.</string>
<key>NSLocationAlwaysUsageDescription</key>
<string>This app needs access to location when in the background.</string>

```