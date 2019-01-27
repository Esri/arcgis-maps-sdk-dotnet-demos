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

namespace OfflineWorkflowsSample
{
    public class MainViewModel : BaseViewModel
    {
        private IWindowService _windowService;

        public bool IsInitialized { get; set; } = false;

        private UserProfileModel _userProfile;
        public UserProfileModel UserProfile
        {
            get { return _userProfile; }
            set { SetProperty(ref _userProfile, value); }
        }

        private GenerateMapAreaViewModel _generateMapAreaViewModel;
        public GenerateMapAreaViewModel GenerateMapAreaViewModel
        {
            get { return _generateMapAreaViewModel; }
            set { SetProperty(ref _generateMapAreaViewModel, value); }
        }

        private DownloadMapAreaViewModel _downloadMapAreaViewModel;
        public DownloadMapAreaViewModel DownloadMapAreaViewModel
        {
            get { return _downloadMapAreaViewModel; }
            set { SetProperty(ref _downloadMapAreaViewModel, value); }
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
                PortalViewModel = await PortalViewModel.GetRootVM(portal, true, true);
                await OfflineMapsViewModel.Initialize();
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                // handle nicely, pretty please!
                Debug.WriteLine(ex);
                await _windowService.ShowAlertAsync(ex.Message);
            }
        }

        public async void SelectMap(Map map)
        {
            Map = map;
            GenerateMapAreaViewModel = new GenerateMapAreaViewModel();
            DownloadMapAreaViewModel = new DownloadMapAreaViewModel();

            GenerateMapAreaViewModel.MapChanged += UpdateMap;
            DownloadMapAreaViewModel.MapChanged += UpdateMap;

            await Task.WhenAll(
                GenerateMapAreaViewModel.Initialize(map, _windowService, MapViewService), 
                DownloadMapAreaViewModel.Initialize(map, _windowService, MapViewService));
        }

        private void UpdateMap(object sender, Map newMap)
        {
            Map = newMap;
            GenerateMapAreaViewModel.Map = newMap;
            DownloadMapAreaViewModel.Map = newMap;
        }

        public void ShowMessage(string message)
        {
            _windowService.ShowAlertAsync(message);
        }
    }
}
