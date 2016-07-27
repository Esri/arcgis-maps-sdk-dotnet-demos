namespace XamarinSymbolPicker.Droid
{
    using System.IO;
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Java.Util.Zip;

    [Activity(Label = "XamarinSymbolPicker", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            this.LoadApplication(new XamarinSymbolPicker.App());
        }
    }
}
