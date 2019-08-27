using CoreLocation;
using Foundation;
using RoutingSample.Forms.iOS.Services;
using System;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationAccessService))]
namespace RoutingSample.Forms.iOS.Services
{
    public class LocationAccessService : ILocationAccessService
    {
        private CLLocationManager locationManager;

        public Task<LocationAccessStatus> RequestAsync()
        {
            // Check that we've already got permission
            var status = GetLocationAccessStatus();
            if (status != LocationAccessStatus.Unknown)
            {
                return Task.FromResult(status);
            }

            // Create our authorization task
            var tcs = new TaskCompletionSource<LocationAccessStatus>();

            // Create a new location manager for authorization
            locationManager = new CLLocationManager();
            locationManager.AuthorizationChanged += OnAuthorizationChanged;

            void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
            {
                if (e.Status != CLAuthorizationStatus.NotDetermined)
                {
                    // We no longer need to respond to requests here
                    locationManager.AuthorizationChanged -= OnAuthorizationChanged;

                    // And the task can be completed
                    tcs.TrySetResult(GetLocationAccessStatus());
                }
            }

            // Start the authorization request
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                /*
                var info = NSBundle.MainBundle.InfoDictionary;
                if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
                {
                    locationManager.RequestAlwaysAuthorization();
                }
                else if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                {
                    locationManager.RequestWhenInUseAuthorization();
                }
                else
                {
                    throw new UnauthorizedAccessException("Please set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in Info.plist.");
                }
                */

                var info = NSBundle.MainBundle.InfoDictionary;
                if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                {
                    // We are aiming to request location when in use; no need for always
                    locationManager.RequestWhenInUseAuthorization();
                }
                else
                {
                    throw new UnauthorizedAccessException("Please set NSLocationWhenInUsageDescription in Info.plist.");
                }
            }
            else
            {
                // You can only request authorization on iOS 8.0+
                return Task.FromResult(GetLocationAccessStatus());
            }

            return tcs.Task;
        }

        private LocationAccessStatus GetLocationAccessStatus()
        {
            if (!CLLocationManager.LocationServicesEnabled)
                return LocationAccessStatus.Disabled;

            var status = CLLocationManager.Status;
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Authorization is different in iOS 8.0+
                switch (status)
                {
                    case CLAuthorizationStatus.AuthorizedAlways:
                    case CLAuthorizationStatus.AuthorizedWhenInUse:
                        return LocationAccessStatus.Granted;
                    case CLAuthorizationStatus.Denied:
                        return LocationAccessStatus.Denied;
                    case CLAuthorizationStatus.Restricted:
                        return LocationAccessStatus.Restricted;
                }
            }
            else
            {
                switch (status)
                {
                    case CLAuthorizationStatus.Authorized:
                        return LocationAccessStatus.Granted;
                    case CLAuthorizationStatus.Denied:
                        return LocationAccessStatus.Denied;
                    case CLAuthorizationStatus.Restricted:
                        return LocationAccessStatus.Restricted;
                }
            }
            return LocationAccessStatus.Unknown;
        }
    }
}