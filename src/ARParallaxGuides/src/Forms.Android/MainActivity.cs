using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Content;
using Android;
using Esri.ArcGISRuntime.UI;

namespace ARParallaxGuidelines.Forms.Droid
{
    [Activity(Label = "ARParallaxGuidelines.Forms", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Instance = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        private const int LocationPermissionRequestCode = 99;
        private Esri.ArcGISRuntime.Xamarin.Forms.MapView _lastUsedMapView;

        public async void AskForLocationPermission(Esri.ArcGISRuntime.Xamarin.Forms.MapView myMapView)
        {
            // Save the mapview for later.
            _lastUsedMapView = myMapView;

            // Only check if permission hasn't been granted yet.
            if (ContextCompat.CheckSelfPermission(this, LocationService) != Permission.Granted)
            {
                // Show the standard permission dialog.
                // Once the user has accepted or denied, OnRequestPermissionsResult is called with the result.
                RequestPermissions(new[] { Manifest.Permission.AccessFineLocation }, LocationPermissionRequestCode);
            }
            else
            {
                try
                {
                    // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                    await myMapView.LocationDisplay.DataSource.StartAsync();
                    myMapView.LocationDisplay.IsEnabled = true;
                    myMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    ShowMessage(ex.Message);
                }
            }
        }
        private void ShowMessage(string message, string title = "Error") => new AlertDialog.Builder(this).SetTitle(title).SetMessage(message).Show();


        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            try
            {
                // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                await _lastUsedMapView.LocationDisplay.DataSource.StartAsync();
                _lastUsedMapView.LocationDisplay.IsEnabled = true;
                _lastUsedMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowMessage(ex.Message);
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            
        }
    }
}

