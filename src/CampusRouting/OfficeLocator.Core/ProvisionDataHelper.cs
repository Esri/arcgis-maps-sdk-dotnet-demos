//using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OfficeLocator
{
    public static class ProvisionDataHelper
    {
        private const string itemId = "a0a519c95ec24ecd9f60eabbe53af6ba"; //The ArcGIS Portal Item ID for the basemap, network and geocoder.


        /// <summary>
        /// Gets the data folder where locally provisioned data is stored
        /// </summary>
        /// <returns></returns>
        internal static string GetDataFolder()
        {
            return Path.Combine(AppDataDirectory, "CampusData");
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
            if (AppSettings.ContainsKey("DataDownloadTask"))
            {
                var previousTask = DownloadManager.FileDownloadTask.FromJson(AppSettings.Get("DataDownloadTask", string.Empty));
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
                AppSettings.Set("DataDownloadTask", task.ToJson());
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

        public static IPreferences AppSettings { get; set; }

        public static string AppDataDirectory { get; set; }

    }

    public interface IPreferences
    {
        T Get<T>(string key, T defaultValue);
        void Set<T>(string key, T value);
        void Remove(string key);
        bool ContainsKey(string key);
    }
}
