﻿using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowsSample.Infrastructure;
using Prism.Windows.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;

namespace OfflineWorkflowSample.ViewModels
{
    public class LocalContentViewModel : ViewModelBase
    {

        public LocalContentViewModel()
        {
            _refreshCommand = new DelegateCommand(
                async () => await Initialize());
        }

        public ObservableCollection<PortalItemViewModel> Items { get; } = new ObservableCollection<PortalItemViewModel>();

        public async Task Initialize()
        {
            // Clear any existing items.
            Items.Clear();

            // Get the data folder.
            string filepath = OfflineDataStorageHelper.GetDataFolder();

            foreach (string subDirectory in Directory.GetDirectories(filepath))
            {
                try
                {
                    // Note: the downloaded map packages are stored as *unpacked* mobile map packages.
                    var mmpk = await MobileMapPackage.OpenAsync(subDirectory);
                    if (mmpk?.Item != null)
                        Items.Add(new PortalItemViewModel(mmpk.Item));
                }
                catch (Exception)
                {
                    // Ignored - not a valid map package
                }
            }
        }

        private readonly DelegateCommand _refreshCommand;

        public ICommand RefreshCommand => _refreshCommand;
    }
}