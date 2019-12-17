using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using RoutingSample.Forms.Droid.Services;
using Xamarin.Forms;


namespace RoutingSample.Forms.Droid
{
    [Activity(Label = "ArcGIS Turn-by-Turn Demo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Current { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.OnCreate(savedInstanceState);

            Current = this;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            var locationAccessService = DependencyService.Get<ILocationAccessService>() as LocationAccessService;
            if (locationAccessService != null)
            {
                locationAccessService.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}