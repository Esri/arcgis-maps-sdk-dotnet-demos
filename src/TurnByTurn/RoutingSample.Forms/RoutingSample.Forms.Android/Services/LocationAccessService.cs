using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using RoutingSample.Forms.Droid.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationAccessService))]
namespace RoutingSample.Forms.Droid.Services
{
    public class LocationAccessService : ILocationAccessService
    {
        private const string LocationPermission = Manifest.Permission.AccessFineLocation;
        private const int PermissionCode = 99;

        private TaskCompletionSource<LocationAccessStatus> requestSource;

        public Task<LocationAccessStatus> RequestAsync()
        {
            if (requestSource != null && !requestSource.Task.IsCompleted)
            {
                requestSource.SetCanceled();
                requestSource = null;
            }

            var activity = MainActivity.Current;
            if (activity == null)
            {
                return Task.FromResult(LocationAccessStatus.Unknown);
            }

            if (ContextCompat.CheckSelfPermission(activity, LocationPermission) == Permission.Granted)
            {
                return Task.FromResult(LocationAccessStatus.Granted);
            }

            requestSource = new TaskCompletionSource<LocationAccessStatus>();

            // Request location permissions
            ActivityCompat.RequestPermissions(activity, new[] { LocationPermission }, PermissionCode);

            return requestSource.Task;
        }

        // Make sure to call this from MainActivity so we can process the callback
        public void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode != PermissionCode ||
                requestSource == null)
            {
                return;
            }

            var result = LocationAccessStatus.Unknown;
            for (int i = 0; i < permissions.Length; i++)
            {
                if (permissions[i] == LocationPermission)
                {
                    result = grantResults[i] == Permission.Granted ? LocationAccessStatus.Granted : LocationAccessStatus.Denied;
                }
            }

            requestSource.TrySetResult(result);
        }
    }
}