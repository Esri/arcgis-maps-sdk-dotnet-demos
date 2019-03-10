using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowsSample.Infrastructure;
using Prism.Windows.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace OfflineWorkflowSample.ViewModels
{
    public class LocalContentViewModel : ViewModelBase
    {
        public ObservableCollection<PortalItemViewModel> Items { get; } = new ObservableCollection<PortalItemViewModel>();

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
                        Items.Add(new PortalItemViewModel(mmpk.Item));
                }
                catch (Exception)
                {
                    // Ignored - not a valid map package
                }

            RaisePropertyChanged(nameof(Items));
        }
    }
}