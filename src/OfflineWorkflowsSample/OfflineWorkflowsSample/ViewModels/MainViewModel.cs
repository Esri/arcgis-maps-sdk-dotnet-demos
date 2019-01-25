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
        private IDialogService _dialogService;
        private ArcGISPortal _portal;

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

        public ArcGISPortal Portal => _portal;

        public async Task Initialize(ArcGISPortal portal, UserProfileModel userProfile, IDialogService dialogService)
        {
            UserProfile = userProfile;
            _portal = portal;
            _dialogService = dialogService;
            try
            {
                PortalViewModel = await PortalViewModel.GetRootVM(_portal, true, true);
                await OfflineMapsViewModel.Initialize();
            }
            catch (Exception ex)
            {
                // handle nicely, pretty please!
                Debug.WriteLine(ex);
                await _dialogService.ShowMessageAsync(ex.Message);
            }
        }

        public async void SelectMap(Map map)
        {
            Map = map;
            GenerateMapAreaViewModel = new GenerateMapAreaViewModel(this);
            DownloadMapAreaViewModel = new DownloadMapAreaViewModel(this);
            await Task.WhenAll(GenerateMapAreaViewModel.Initialize(), DownloadMapAreaViewModel.Initialize());
        }

        public void ShowMessage(string message)
        {
            _dialogService.ShowMessageAsync(message);
        }
    }
}
