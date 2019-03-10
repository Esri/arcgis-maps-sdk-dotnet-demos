using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using OfflineWorkflowsSample.Infrastructure;
using OfflineWorkflowsSample.Models;
using OfflineWorkflowSample;
using OfflineWorkflowSample.ViewModels;
using Prism.Commands;

namespace OfflineWorkflowsSample
{
    public class MainViewModel : BaseViewModel
    {
        // Commands enable binding controls to behavior. https://visualstudiomagazine.com/articles/2012/04/10/command-pattern-in-net.aspx
        private readonly DelegateCommand _logOutCommand;
        private readonly DelegateCommand _openInAgolCommand;

        private PortalItemViewModel _selectedItem;

        private string _title = "ArcGIS Maps Offline";

        // WindowService allows the ViewModel to communicate with the view without
        //     exposing details of the view to the ViewModel. 
        private IWindowService _windowService;

        public IWindowService WindowService => _windowService;

        public MainViewModel()
        {
            _logOutCommand = new DelegateCommand(LogOut);
            // TODO - wire up CanExecute & set up events
            _openInAgolCommand = new DelegateCommand(() => { _windowService.LaunchItem(SelectedItem.Item); });
        }

        public PortalItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                // Notifies the window service that it should navigate to the appropriate
                //     page for the selected item.
                if (value != null) _windowService.NavigateToPageForItem(_selectedItem);
            }
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsInitialized { get; private set; }

        public UserProfileModel UserProfile { get; private set; }

        public LocalContentViewModel LocalContentViewModel { get; private set; }

        public PortalViewModel PortalViewModel { get; private set; }

        // Log out of portal.
        public ICommand LogOutCommand => _logOutCommand;

        // Open the current selected item in ArcGIS Online.
        public ICommand OpenItemInAgolCommand => _openInAgolCommand;

        private void LogOut()
        {
            // Go back to the login page.
            _windowService.NavigateToLoginPage();

            // Clear all content from the last user.
            UserProfile = null;
            LocalContentViewModel = null;
            PortalViewModel = null;
            _windowService = null;

            // Clear the credentials - completes the log out.
            AuthenticationManager.Current.RemoveAllCredentials();
            IsInitialized = false;
        }

        public async Task Initialize(UserProfileModel userProfile, IWindowService windowService)
        {
            // Store user details & the window service.
            UserProfile = userProfile;
            _windowService = windowService;

            try
            {
                _windowService.SetBusy(true);
                _windowService.SetBusyMessage("Loading portal and local content...");

                // Create the view models.
                PortalViewModel = new PortalViewModel();
                LocalContentViewModel = new LocalContentViewModel();

                // Wait for the view models to finish initialization.
                await Task.WhenAll(PortalViewModel.LoadPortalAsync(UserProfile.Portal), LocalContentViewModel.Initialize());
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                // handle nicely, pretty please!
                Debug.WriteLine(ex);
                await _windowService.ShowAlertAsync(ex.Message);
            }
            finally
            {
                _windowService.SetBusy(false);
                _windowService.SetBusyMessage("");
            }
        }
    }
}