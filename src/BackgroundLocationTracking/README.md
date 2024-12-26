# Background Location Tracking

BackgroundLocationTracking is a .NET MAUI application designed to track the location of a device in the background using the [ArcGIS Runtime SDK for .NET](https://developers.arcgis.com/net/).

Android application uses Foreground Service to track the location in the background. iOS application uses [SystemLocationDaatSource.AllowBackgroundLocationUpdates](https://developers.arcgis.com/net/api-reference/api/ios/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Location.SystemLocationDataSource.AllowsBackgroundLocationUpdates.html) to track the location in the background.

<img src="https://github.com/user-attachments/assets/e5cd7f2f-c488-4953-b944-f908d3c81959" alt="Screenshot" width="500"/>


## Important Configurations

### Android

#### `MainPage.xaml.cs`
Android requires the use of a `ForegroundService` to track location in the background.
Ensure the following code is added to the `MainPage.xaml.cs` file to configure the location data source:

```C#
_locationDataSource = new SystemLocationDataSource();
```

To start the `ForegroundService` on current application context, use the following code:
```C#
var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(LocationService));

// Foreground Services are only supported and required after Android version Oreo (API level 26)
// Foreground service is required to keep the service running in the background when the main app is not in the foreground.
// Start the service as a foreground service.
_ = Android.App.Application.Context.StartForegroundService(intent);
```

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

#### `MainPage.xaml.cs`
Ensure the following code is added to the `MainPage.xaml.cs` file to configure the location data source and request background location updates:

```C#
_locationDataSource = new SystemLocationDataSource
{
    // Set AllowsBackgroundLocationUpdates to true to allow location updates when the app is in the background.
    AllowsBackgroundLocationUpdates = true,

    // Set ActivityType which is used to determine when location updates should be delivered.
    // This is used to help determine when to turn off GPS hardware to save power.
    ActivityType = CoreLocation.CLActivityType.Other,
};
```

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
