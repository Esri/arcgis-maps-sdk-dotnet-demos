using System;
using System.IO;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.Storage;
#endif

namespace OfficeLocator
{
    internal static class ProvisionDataHelper
    {
        private const string itemId = "dce76fb7cf1146c990427555fb3c74d2"; //The ArcGIS Portal Item ID for the basemap, network and geocoder.

        /// <summary>
        /// Downloads data from ArcGIS Portal and unzips it to the local data folder
        /// </summary>
        /// <param name="path">The path to put the data in - if the folder already exists, the data is assumed to have been downloaded and this immediately returns</param>
        /// <param name="progress">Progress reporr status callback</param>
        /// <returns></returns>
        public static async Task GetData(string path, Action<string> progress)
        {
            if (System.IO.Directory.Exists(path))
                return;
            var portal = await Esri.ArcGISRuntime.Portal.ArcGISPortal.CreateAsync().ConfigureAwait(false);
            var item = await Esri.ArcGISRuntime.Portal.ArcGISPortalItem.CreateAsync(portal, itemId).ConfigureAwait(false);
            
            progress?.Invoke("Initiating download...");
            var tempFile = Path.GetTempFileName();
            progress?.Invoke("Downloading data...");
            using (var s = await item.GetItemDataAsync().ConfigureAwait(false))
            {
                using (var f = File.Create(tempFile))
                {
                    await s.CopyToAsync(f).ConfigureAwait(false);
                }
            }
            progress?.Invoke("Unpacking data...");
            await UnpackData(tempFile, path);
            progress?.Invoke("Complete");
        }

        private static async Task UnpackData(string zipFile, string folder)
        {
#if NETFX_CORE
            using (var zipStream = File.OpenRead(zipFile))
            {
                using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Read))
                {
                    Action<DirectoryInfo> createDir = null;
                    createDir = (s) =>
                    {
                        System.Diagnostics.Debug.WriteLine(s.FullName);
                        if (Directory.Exists(s.FullName)) return;
                        if (!Directory.Exists(s.Parent.FullName))
                            createDir(s.Parent);
                        Directory.CreateDirectory(s.FullName);
                    };
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("/")) continue;
                        var fileInfo = new System.IO.FileInfo(Path.Combine(folder, entry.FullName));
                        System.Diagnostics.Debug.WriteLine("Unzipping " + fileInfo.FullName);
                        createDir(fileInfo.Directory);
                        using (var fileStream = File.Create(fileInfo.FullName))
                        {
                            using (var entryStream = entry.Open())
                            {
                                await entryStream.CopyToAsync(fileStream).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
#elif __ANDROID__ || __IOS__
            await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, folder));
#endif
        }
    }
}
