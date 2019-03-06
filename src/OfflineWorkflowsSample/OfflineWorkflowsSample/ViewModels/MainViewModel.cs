using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using OfflineWorkflowSample;
using OfflineWorkflowSample.ViewModels;
using OfflineWorkflowsSample.Infrastructure;
using OfflineWorkflowsSample.Models;
using Prism.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public MainViewModel()
        {
            _logOutCommand = new DelegateCommand(() =>
            {
                _windowService.NavigateToLoginPage();
                _userProfile = null;
                _offlineMapViewModel = null;
                _portalViewModel = null;
                _windowService = null;
                IsInitialized = false;

                AuthenticationManager.Current.RemoveAllCredentials();
            });

            // TODO - wire up CanExecute
            _openInAgolCommand = new DelegateCommand(() => { _windowService.LaunchItem(SelectedItem); });
        }

        public IWindowService _windowService;

        public bool IsInitialized { get; set; }

        private UserProfileModel _userProfile;

        public UserProfileModel UserProfile
        {
            get { return _userProfile; }
            set { SetProperty(ref _userProfile, value); }
        }

        private OfflineMapsViewModel _offlineMapViewModel;

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
                OfflineMapViewModel = new OfflineMapViewModel {MapViewService = MapViewService};
                OfflineMapsViewModel = new OfflineMapsViewModel();
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

        public OfflineMapViewModel OfflineMapViewModel { get; private set; }

        public void ShowMessage(string message)
        {
            _windowService.ShowAlertAsync(message);
        }

        private DelegateCommand _logOutCommand;
        public ICommand LogOutCommand => _logOutCommand;

        private DelegateCommand _openInAgolCommand;
        public ICommand OpenItemInAgolCommand => _openInAgolCommand;
    }
}