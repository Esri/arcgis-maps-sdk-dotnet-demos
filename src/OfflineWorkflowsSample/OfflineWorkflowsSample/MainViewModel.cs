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

namespace OfflineWorkflowsSample
{
    public class MainViewModel : BaseViewModel
    {
        #region Setup
        private const string PortalUrl = "https://www.arcgis.com/sharing/rest";
        private const string UserName = "MyUserName";
        private const string Password = "MyPassword";
        #endregion //Setup

        private IDialogService _dialogService = null;
        private ArcGISPortal _portal = null;

        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateKnownCredentials);
            Initialize();
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

        private async void Initialize()
        {
            try
            {
                // If username and password aren't set, show message so that we remember
                if (UserName == "MyUserName" || Password == "MyPassword")
                {
                    await _dialogService.ShowMessageAsync("Please add username and password in MainViewModel");
                    return;
                }

                // Authenticate direclty against defined portal and fetch user information
                UserProfile = await AuthenticateAndLoadUserProfileAsync();

                GenerateMapAreaViewModel = new GenerateMapAreaViewModel(_portal);
                DownloadMapAreaViewModel = new DownloadMapAreaViewModel(_portal);

                // Create portal item that points to the webmap by 
                // it's id. ArcGISPortal is required to define which portal
                // is used. Remember to hook authentication if needed
                var webmapItem = await PortalItem.CreateAsync(
                    _portal, "7d30900e1e9848159e272284e07e3cab");

                // Construct map from the item. Remember to load map if you 
                // need to access map before setting it to the MapView
                Map = new Map(webmapItem);
                await Map.LoadAsync();
            }
            catch (Exception ex)
            {
                // handle nicely, pretty please!
                Debug.WriteLine(ex);
                await _dialogService.ShowMessageAsync(ex.Message);
            }
        }

        private async Task<UserProfileModel> AuthenticateAndLoadUserProfileAsync()
        {
            // Generate credentials to ArcGIS Online
            var credentials = await AuthenticationManager.Current.GenerateCredentialAsync(
              new Uri(PortalUrl), UserName, Password);

            _portal = await ArcGISPortal.CreateAsync(
                new Uri(PortalUrl), credentials, CancellationToken.None);

            // Store credentials to authentication manager
            if (!AuthenticationManager.Current.Credentials.Contains(credentials))
                AuthenticationManager.Current.AddCredential(credentials);

            var currentUser = _portal.User;

            BitmapImage profilePicture = null;
            if (currentUser.ThumbnailUri != null)
                profilePicture = new BitmapImage(currentUser.ThumbnailUri);
            else
                profilePicture = null;

            var userProfile = new UserProfileModel()
            {
                Username = currentUser.UserName,
                FullName = currentUser.FullName,
                ProfilePicture = profilePicture
            };

            return userProfile;
        }

        private async Task<Credential> CreateKnownCredentials(CredentialRequestInfo info)
        {
            // If this isn't the expected resource, the credential will stay null
            Credential knownCredential = null;

            try
            {
                // Create a credential for this resource
                knownCredential = await AuthenticationManager.Current.GenerateCredentialAsync
                                        (info.ServiceUri,
                                         UserName,
                                         Password,
                                         info.GenerateTokenOptions);

            }
            catch (Exception ex)
            {
                // Report error accessing a secured resource
                Debug.WriteLine(ex);
                throw;
            }

            // Return the credential
            return knownCredential;
        }
    }
}
