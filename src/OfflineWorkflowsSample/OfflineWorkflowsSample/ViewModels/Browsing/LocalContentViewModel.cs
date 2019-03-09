using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<PortalItemViewModel<LocalItem>> Items { get; } = new ObservableCollection<PortalItemViewModel<LocalItem>>();
        public Dictionary<LocalItem, string> PathsForItems { get; } = new Dictionary<LocalItem, string>();

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
                            Items.Add(new PortalItemViewModel<LocalItem>((LocalItem)mmpk.Item));
                            PathsForItems[(LocalItem)mmpkMap.Item] = subDirectory;
                        }
                }
                catch (Exception)
                {
                    // Ignored - not a valid map package
                }

            RaisePropertyChanged(nameof(Items));
        }
    }
}