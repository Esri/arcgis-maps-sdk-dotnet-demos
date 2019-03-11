using Plugin.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.Storage;
#endif

namespace OfficeLocator
{
    public static class ProvisionDataHelper
    {
        private const string itemId = "dce76fb7cf1146c990427555fb3c74d2"; //The ArcGIS Portal Item ID for the basemap, network and geocoder.


        /// <summary>
        /// Gets the data folder where locally provisioned data is stored
        /// </summary>
        /// <returns></returns>
        internal static string GetDataFolder()
        {
            var appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataFolder, "CampusData");
        }
        public static Task GetDataAsync(Action<string> progress)
        {
            return GetDataAsync(GetDataFolder(), progress);
        }
        /// <summary>
        /// Downloads data from ArcGIS Portal and unzips it to the local data folder
        /// </summary>
        /// <param name="path">The path to put the data in - if the folder already exists, the data is assumed to have been downloaded and this immediately returns</param>
        /// <param name="progress">Progress reporr status callback</param>
        /// <returns></returns>
        public static async Task GetDataAsync(string path, Action<string> progress)
        {
            if (System.IO.Directory.Exists(path))
                return;
            DownloadManager.FileDownloadTask task = null;
            string tempFile = null;
            if (AppSettings.Contains("DataDownloadTask"))
            {
                var previousTask = DownloadManager.FileDownloadTask.FromJson(AppSettings.GetValueOrDefault("DataDownloadTask", string.Empty));
                if (previousTask.IsResumable)
                {
                    task = previousTask;
                    tempFile = task.Filename;
                }
            }
            if(task == null)
            {
                progress?.Invoke("Fetching offline data info...");
                var portal = await Esri.ArcGISRuntime.Portal.ArcGISPortal.CreateAsync(new Uri("https://www.arcgis.com/sharing/rest")).ConfigureAwait(false);
                var item = await Esri.ArcGISRuntime.Portal.PortalItem.CreateAsync(portal, itemId).ConfigureAwait(false);

                progress?.Invoke("Initiating download...");
                tempFile = Path.GetTempFileName();
                task = await DownloadManager.FileDownloadTask.StartDownload(tempFile, item);
                string downloadTask = task.ToJson();
                AppSettings.AddOrUpdateValue("DataDownloadTask", task.ToJson());
            }
            progress?.Invoke("Downloading data...");

            if (progress != null)
            {
                string lastState = "";
                task.Progress += (s, e) =>
                {
                    var state = "Downloading data " + (e.HasPercentage ? e.Percentage.ToString() + "%" : e.TotalBytes / 1024 + " kb...");
                    if (state != lastState)
                    {
                        progress?.Invoke(state);
                        lastState = state;
                    }
                };
            }
            await task.DownloadAsync();
            AppSettings.Remove("DataDownloadTask");
            progress?.Invoke("Unpacking data...");
            await UnpackData(tempFile, path);
            progress?.Invoke("Complete");
        }

        private static async Task UnpackData(string zipFile, string folder)
        {
            await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, folder));
        }

        private static Plugin.Settings.Abstractions.ISettings AppSettings => CrossSettings.Current;

    }
}
