using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using OfflineWorkflowsSample.DownloadMapArea;
using OfflineWorkflowsSample.GenerateMapArea;
using OfflineWorkflowsSample.Infrastructure;
using OfflineWorkflowsSample.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using OfflineWorkflowSample;
using OfflineWorkflowSample.ViewModels;

namespace OfflineWorkflowsSample
{
    public class MainViewModel : BaseViewModel
    {
        private IDialogService _dialogService;
        private ArcGISPortal _portal;

        public MainViewModel(IDialogService dialogService, LoginViewModel login)
        {
            _dialogService = dialogService;
            UserProfile = login.UserProfile;
            _portal = login.Portal;
        }

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

        private PortalViewModel _portalViewModel;

        public PortalViewModel PortalViewModel
        {
            get => _portalViewModel;
            set => SetProperty(ref _portalViewModel, value);
        }

        public ArcGISPortal Portal => _portal;

        public async Task Initialize()
        {
            try
            {
               

                // Create portal item that points to the webmap by 
                // it's id. ArcGISPortal is required to define which portal
                // is used. Remember to hook authentication if needed
                var webmapItem = await PortalItem.CreateAsync(
                    _portal, "acc027394bc84c2fb04d1ed317aac674");

                // Construct map from the item. Remember to load map if you 
                // need to access map before setting it to the MapView
                Map = new Map(webmapItem);
                await Map.LoadAsync();

                GenerateMapAreaViewModel = new GenerateMapAreaViewModel(this);
                DownloadMapAreaViewModel = new DownloadMapAreaViewModel(this);
                PortalViewModel = await PortalViewModel.GetRootVM(_portal, true, true);
            }
            catch (Exception ex)
            {
                // handle nicely, pretty please!
                Debug.WriteLine(ex);
                await _dialogService.ShowMessageAsync(ex.Message);
            }
        }
    }
}
