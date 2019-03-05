using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowSample;
using OfflineWorkflowSample.ViewModels;
using OfflineWorkflowsSample.DownloadMapArea;
using OfflineWorkflowsSample.GenerateMapArea;
using OfflineWorkflowsSample.Infrastructure;
using OfflineWorkflowsSample.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;
using Esri.ArcGISRuntime.UI.Controls;

namespace OfflineWorkflowsSample
{
    public class MainViewModel : BaseViewModel
    {
        private Item _selectedItem;

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                if (value != null)
                {
                    _windowService.NavigateToPageForItem(_selectedItem);
                }
            }
        }

        private string _title = "ArcGIS Maps Offline";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public IWindowService _windowService;

        public bool IsInitialized { get; set; } = false;

        private UserProfileModel _userProfile;

        public UserProfileModel UserProfile
        {
            get { return _userProfile; }
            set { SetProperty(ref _userProfile, value); }
        }

        private OfflineMapsViewModel _offlineMapViewModel = new OfflineMapsViewModel();

        public OfflineMapsViewModel OfflineMapsViewModel
        {
            get => _offlineMapViewModel;
            set => SetProperty(ref _offlineMapViewModel, value);
        }

        private PortalViewModel _portalViewModel;

        public PortalViewModel PortalViewModel
        {
            get => _portalViewModel;
            set => SetProperty(ref _portalViewModel, value);
        }

        public async Task Initialize(ArcGISPortal portal, UserProfileModel userProfile, IWindowService windowService)
        {
            UserProfile = userProfile;
            _windowService = windowService;
            try
            {
                PortalViewModel = new PortalViewModel();
                await PortalViewModel.LoadPortalAsync(portal);
                await OfflineMapsViewModel.Initialize();
                OfflineMapViewModel.MapViewService = MapViewService;
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                // handle nicely, pretty please!
                Debug.WriteLine(ex);
                await _windowService.ShowAlertAsync(ex.Message);
            }
        }

        public OfflineMapViewModel OfflineMapViewModel { get; } = new OfflineMapViewModel();
        
        public void ShowMessage(string message)
        {
            _windowService.ShowAlertAsync(message);
        }
    }
}