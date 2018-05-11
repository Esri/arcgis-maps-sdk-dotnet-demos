namespace OfflineWorkflowsSample.Infrastructure
{
    public class OfflineDataStorageHelper
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
    }
}
