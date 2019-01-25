using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using OfflineWorkflowsSample;
using OfflineWorkflowsSample.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace OfflineWorkflowSample.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        #region OAuth constants
        private const string AppClientId = @"lgAdHkYZYlwwfAhC";
        private const string ClientSecret = "";
        private const string OAuthRedirectUrl = @"my-ags-app://auth";
        #endregion OAuth constants

        #region command pattern
        private readonly DelegateCommand _loginWithOAuthCommand;
        public ICommand LoginWithOAuthCommand => _loginWithOAuthCommand;
        private readonly DelegateCommand _loginWithoutOAuthCommand;
        public ICommand LoginWithoutOAuthCommand => _loginWithoutOAuthCommand;

        #endregion command pattern

        public IDialogService DialogService = null;
        
        public ArcGISPortal Portal { get; set; }

        private string _portalUrl = "https://www.arcgis.com/sharing/rest";

        public string PortalUrl
        {
            get => _portalUrl;
            set => SetProperty(ref _portalUrl, value);
        }

        public UserProfileModel UserProfile { get; set; }

        public LoginViewModel()
        {
            _loginWithOAuthCommand = new DelegateCommand(StartOAuth);
            _loginWithoutOAuthCommand = new DelegateCommand(StartCredentialAuth);
        }

        private void StartOAuth()
        {
            ConfigureOAuth();
            DoLogin();
        }

        private void StartCredentialAuth()
        {
            DoLogin();
        }

        private async void DoLogin()
        {
            Portal = await AuthenticateAndCreatePortal();
            if (Portal == null)
            {
                return;
            }
            UserProfile = GetProfile();
            RaiseLoggedIn();
        }

        private UserProfileModel GetProfile()
        {
            var currentUser = Portal.User;

            BitmapImage profilePicture = null;
            profilePicture = currentUser.ThumbnailUri != null ? new BitmapImage(currentUser.ThumbnailUri) : null;

            var userProfile = new UserProfileModel()
            {
                Username = currentUser.UserName,
                FullName = currentUser.FullName,
                ProfilePicture = profilePicture
            };

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
                await DialogService.ShowMessageAsync($"Couldn't log in - {e.Message}");
                return null;
            }
            catch (OperationCanceledException)
            {
                await DialogService.ShowMessageAsync($"Log in canceled");
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
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode;
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