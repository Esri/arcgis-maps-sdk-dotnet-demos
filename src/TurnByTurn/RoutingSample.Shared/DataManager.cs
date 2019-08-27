using Esri.ArcGISRuntime.Portal;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace RoutingSample
{
    /// <summary>
    /// Provides methods for downloading and using sample data.
    /// </summary>
    public static class DataManager
    {
        // The data manager is compatible with other ArcGIS samples.
        private const string SampleDataId = "567e14f3420d40c5a206e5c0284cf8fc";

        public static async Task EnsureDataPresent()
        {
            var portal = await ArcGISPortal.CreateAsync();
            var item = await PortalItem.CreateAsync(portal, SampleDataId);

            if (!IsDataPresent(item))
            {
                // We download the latest data from using the ArcGIS portal
                await DownloadData(item);
            }
        }

        private static bool IsDataPresent(PortalItem item)
        {
            var configPath = GetDataFolder("__sample.config");
            if (!File.Exists(configPath))
                return false;

            // Determine when the data was last downloaded
            var downloadDate = File.GetLastWriteTime(configPath);

            // The data is present if it's up-to-date
            return downloadDate >= item.Modified;
        }

        public static string GetAppDataFolder()
        {
#if XAMARIN
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);;
#elif WINDOWS_UWP
            var appData = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif WINDOWS_WPF
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif

            var folder = Path.Combine(appData, "ArcGISRuntimeSampleData");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        public static string GetDataFolder()
        {
            return Path.Combine(GetAppDataFolder(), SampleDataId);
        }

        public static string GetDataFolder(string file)
        {
            return Path.Combine(GetAppDataFolder(), SampleDataId, file);
        }

        private static async Task DownloadData(PortalItem item)
        {
            var dataDir = GetDataFolder();
            var dataFile = Path.Combine(dataDir, item.Name);

            // Create the directory if needed
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            // Download the data
            using (var stream = await item.GetDataAsync())
            using (var output = File.Create(dataFile))
                await stream.CopyToAsync(output);

            // Uzip the data if needed
            if (Path.GetExtension(dataFile) == ".zip")
                await UnpackData(dataFile, dataDir);

            // Update __sample.config to save the last time downloaded
            var configFilePath = Path.Combine(dataDir, "__sample.config");
#if WINDOWS_UWP
            await File.WriteAllTextAsync(configFilePath, $"Data downloaded: {DateTime.Now}");
#else
            File.WriteAllText(configFilePath, $"Data downloaded: {DateTime.Now}");
#endif
        }

        private static Task UnpackData(string zipFile, string folder)
        {
            return Task.Run(() =>
            {
                using (var archive = ZipFile.OpenRead(zipFile))
                {
                    foreach (var entry in archive.Entries.Where(m => !string.IsNullOrWhiteSpace(m.Name)))
                    {
                        string path = Path.Combine(folder, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        entry.ExtractToFile(path, true);
                    }
                }
            });
        }
    }
}
