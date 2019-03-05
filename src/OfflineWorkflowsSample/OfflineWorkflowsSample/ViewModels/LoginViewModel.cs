using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using OfflineWorkflowsSample;
using OfflineWorkflowsSample.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace OfflineWorkflowSample.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        #region OAuth constants
        // TODO - Make sure these are up to date with the registration in ArcGIS Online or your portal.
        private const string AppClientId = "collectorwindowsstore";
        private const string ClientSecret = "";
        private const string OAuthRedirectUrl = @"urn:ietf:wg:oauth:2.0:oob";
        private const string ArcGISOnlinePortalUrl = "https://www.arcgis.com/sharing/rest";
        #endregion OAuth constants

        #region command pattern
        private readonly DelegateCommand _logInToPortalCommand;
        public ICommand LogInToPortalCommand => _logInToPortalCommand;
        private readonly DelegateCommand _logInToEnterpriseCommand;
        public ICommand LogInToEnterpriseCommand => _logInToEnterpriseCommand;
        private readonly DelegateCommand _showEnterpriseFormCommand;
        public ICommand ShowEnterpriseFormCommand => _showEnterpriseFormCommand;

        #endregion command pattern

        public IWindowService WindowService = null;
        
        public ArcGISPortal Portal { get; set; }

        private string _portalUrl = ArcGISOnlinePortalUrl;

        public string PortalUrl
        {
            get => _portalUrl;
            set => SetProperty(ref _portalUrl, value);
        }

        public UserProfileModel UserProfile { get; set; }

        private bool _portalFormOpen = false;

        public bool PortalFormIsOpen
        {
            get => _portalFormOpen;
            set => SetProperty(ref _portalFormOpen, value);
        }

        public LoginViewModel()
        {
            _logInToPortalCommand = new DelegateCommand(LoginToAGOL);
            _logInToEnterpriseCommand = new DelegateCommand(LoginToEnterprise);
            _showEnterpriseFormCommand = new DelegateCommand(ShowPortalForm);
        }

        private void LoginToAGOL()
        {
            _portalUrl = ArcGISOnlinePortalUrl;
            DoLogin();
        }

        private void LoginToEnterprise()
        {
            DoLogin();
        }

        private async void DoLogin()
        {
            ConfigureOAuth();
            Portal = await AuthenticateAndCreatePortal();
            if (Portal == null)
            {
                return;
            }
            UserProfile = GetProfile();
            RaiseLoggedIn();
        }

        private void ShowPortalForm()
        {
            PortalFormIsOpen = !PortalFormIsOpen;
        }
        
        private UserProfileModel GetProfile()
        {
            var currentUser = Portal.User;

            BitmapImage profilePicture = null;
            profilePicture = currentUser.ThumbnailUri != null ? new BitmapImage(currentUser.ThumbnailUri) : null;

            var userProfile = new UserProfileModel(Portal.User);

            return userProfile;
        }

        private async Task<ArcGISPortal> AuthenticateAndCreatePortal()
        {
            try
            {
                // Authenticate
                Credential cred;
                CredentialRequestInfo cri = new CredentialRequestInfo();
                cri.ServiceUri = new Uri(_portalUrl);
                cred = await AuthenticationManager.Current.GetCredentialAsync(cri, true);

                // Create the portal with authentication info
                return await ArcGISPortal.CreateAsync(cred.ServiceUri, cred);
            }
            catch (ArcGISWebException e)
            {
                await WindowService.ShowAlertAsync($"Couldn't log in - {e.Message}");
                return null;
            }
            catch (HttpRequestException)
            {
                await WindowService.ShowAlertAsync(
                    "Couldn't log in - this app isn't registered with the selected portal.\n" +
                    "https://developers.arcgis.com/documentation/core-concepts/security-and-authentication/accessing-arcgis-online-services/");
                return null;
            }
            catch (OperationCanceledException)
            {
                await WindowService.ShowAlertAsync($"Log in canceled");
                return null;
            }
        }

        private void ConfigureOAuth()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri(_portalUrl),
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                }
                
            };
            
            // If a client secret has been configured, set the authentication type to OAuthAuthorizationCode.
            if (!String.IsNullOrEmpty(ClientSecret))
            {
                // Use OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret).
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit;
                serverInfo.OAuthClientInfo.ClientSecret = ClientSecret;
            }
            

            // Register this server with AuthenticationManager.
            AuthenticationManager.Current.RegisterServer(serverInfo);

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateOAuthCredentialAsync);
        }

        private async Task<Credential> CreateOAuthCredentialAsync(CredentialRequestInfo info)
        {
            // ChallengeHandler function for AuthenticationManager that will be called whenever a secured resource is accessed.
            Credential credential = null;
            try
            {
                // AuthenticationManager will handle challenging the user for credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            return credential;
        }
        
        #region Allow page to react to login

        public delegate void LoginCompletionHandler(object sender);

        public event LoginCompletionHandler CompletedLogin;

        private void RaiseLoggedIn()
        {
            CompletedLogin?.Invoke(this);
        }

        #endregion Allow page to react to login
    }
}