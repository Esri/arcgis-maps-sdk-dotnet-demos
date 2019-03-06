using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample.Infrastructure;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample.ViewModels
{
    public class LocalContentViewModel : ViewModelBase
    {
        private List<Map> OfflineMaps => MapItems.Values.ToList();
        public List<Item> Items => MapItems.Keys.ToList();
        private Dictionary<Item, Map> MapItems { get; } = new Dictionary<Item, Map>();
        public Dictionary<Item, string> PathsForItems { get; } = new Dictionary<Item, string>();

        public async Task Initialize()
        {
            // Get the data folder.
            string filepath = OfflineDataStorageHelper.GetDataFolder();

            foreach (string subDirectory in Directory.GetDirectories(filepath))
                // Note: the downloaded map packages are stored as *unpacked* mobile map packages.
                // This constructor accepts paths to unpacked packages.
                try
                {
                    var mmpk = await MobileMapPackage.OpenAsync(subDirectory);
                    if (mmpk?.Item != null)
                        foreach (var mmpkMap in mmpk.Maps)
                        {
                            MapItems[mmpkMap.Item] = mmpkMap;
                            PathsForItems[mmpkMap.Item] = subDirectory;
                        }
                }
                catch (Exception)
                {
                    // Ignored - not a valid map package
                }

            RaisePropertyChanged(nameof(Items));
            RaisePropertyChanged(nameof(OfflineMaps));
            RaisePropertyChanged(nameof(MapItems));
        }
    }
}