using Esri.ArcGISRuntime.Portal;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace FormsDemoAR
{
    public static class DataManager
    {
        public static async Task DownloadItem(string itemId)
        {
            var portal = await ArcGISPortal.CreateAsync().ConfigureAwait(false);
            var item = await PortalItem.CreateAsync(portal, itemId).ConfigureAwait(false);

            string dataDir = Path.Combine(GetDataFolder(itemId));
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);
            
            Task<Stream> downloadTask = item.GetDataAsync();

            string tempFile = Path.Combine(dataDir, item.Name);
            using (var s = await downloadTask.ConfigureAwait(false))
            {
                using (var f = File.Create(tempFile))
                {
                    await s.CopyToAsync(f).ConfigureAwait(false);
                }
            }

            if (tempFile.EndsWith(".zip"))
                await UnpackData(tempFile, dataDir);

            string configFilePath = Path.Combine(dataDir, "__sample.config");
            File.WriteAllText(configFilePath, @"Data downloaded: " + DateTime.Now);
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

        internal static string GetDataFolder(string itemId, params string[] pathParts)
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string sampleDataFolder = Path.Combine(appDataFolder, "FormsARDemoSampleData");

            if (!Directory.Exists(sampleDataFolder)) 
                Directory.CreateDirectory(sampleDataFolder);

            return Path.Combine(Path.Combine(sampleDataFolder, itemId), Path.Combine(pathParts));
        }
    }
}
