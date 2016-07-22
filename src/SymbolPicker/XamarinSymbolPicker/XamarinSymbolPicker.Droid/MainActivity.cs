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
            var specification = "mil2525d";
            var filename = string.Format("{0}.stylx", specification);
            var zip = string.Format("{0}.zip", specification);
            var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(folder, filename);
            if (!File.Exists(path))
            {
                this.UnzipAsset(zip, folder);
            }

            this.LoadApplication(new XamarinSymbolPicker.App());
        }

        private void UnzipAsset(string assetName, string targetFolder)
        {
            using (var asset = this.Assets.Open(assetName))
            using (var zipStream = new ZipInputStream(asset))
            {
                ZipEntry entry;
                while ((entry = zipStream.NextEntry) != null)
                {
                    var folder = Path.Combine(targetFolder, Path.GetDirectoryName(entry.Name));
                    Directory.CreateDirectory(folder);
                    string filename = Path.GetFileName(entry.Name);
                    if (string.IsNullOrEmpty(filename))
                    {
                        continue;
                    }

                    using (var fs = File.Create(Path.Combine(targetFolder, entry.Name)))
                    {
                        var size = 4096;
                        var data = new byte[size];
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size <= 0)
                            {
                                break;
                            }

                            fs.Write(data, 0, size);
                        }
                    }
                }
            }
        }
    }
}
