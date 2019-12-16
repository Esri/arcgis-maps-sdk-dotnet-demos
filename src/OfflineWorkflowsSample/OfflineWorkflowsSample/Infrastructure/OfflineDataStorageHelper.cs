using System;
using System.IO;
using Windows.Storage;
using Esri.ArcGISRuntime.Mapping;

namespace OfflineWorkflowsSample.Infrastructure
{
    public static class OfflineDataStorageHelper
    {
        internal static string GetDataFolder()
        {
            string appDataFolder = "";
#if NETFX_CORE
            appDataFolder = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path;
#else
            appDataFolder = System.IO.Directory.GetCurrentDirectory();
#endif
            return appDataFolder;
        }

        internal static string GetDataFolderForMap(Map map)
        {
            return Path.Combine(GetDataFolder(), map.Item.ItemId);
        }

        internal static async void FlushLogToDisk(string errors, Map map)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(GetDataFolderForMap(map));
            var file = await folder.CreateFileAsync("error_log.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, errors);
        }
    }
}
