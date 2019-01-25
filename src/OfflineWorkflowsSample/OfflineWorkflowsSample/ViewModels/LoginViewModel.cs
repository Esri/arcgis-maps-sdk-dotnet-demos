using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using OfflineWorkflowsSample;
using OfflineWorkflowsSample.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using Esri.ArcGISRuntime.Http;

namespace OfflineWorkflowSample.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        #region OAuth constants

        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        private const string AppClientId = @"lgAdHkYZYlwwfAhC";
        private const string ClientSecret = "";
        private const string OAuthRedirectUrl = @"my-ags-app://auth";

        #endregion OAuth constants

        #region command pattern

        public IDialogService DialogService = null;
        private readonly DelegateCommand _loginWithCredsCommand;
        public ICommand LoginWithCredsCommand => _loginWithCredsCommand;
        private readonly DelegateCommand _loginWithOAuthCommand;
        public ICommand LoginWithOAuthCommand => _loginWithOAuthCommand;
        private readonly DelegateCommand _showCredentialFormCommand;
        public ICommand ShowCredentialFormCommand => _showCredentialFormCommand;

        #endregion command pattern

        private bool _isCredentialsOpen;

        public bool IsCredentialsOpen
        {
            get => _isCredentialsOpen;
            set => SetProperty(ref _isCredentialsOpen, value);
        }

        public ArcGISPortal Portal { get; set; }

        private string _username = "";
        private string _password = "";
        private string _portalUrl = "https://www.arcgis.com/sharing/rest";

        public string PortalUrl
        {
            get => _portalUrl;
            set => SetProperty(ref _portalUrl, value);
        }

        public string UserName
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                _loginWithCredsCommand.RaiseCanExecuteChanged();
            }
        }

        public string Password 
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                _loginWithCredsCommand.RaiseCanExecuteChanged();
            }
        }

        public UserProfileModel UserProfile { get; set; }

        public LoginViewModel()
        {
            _loginWithCredsCommand = new DelegateCommand(DoLogin, CanDoFormLogin);
            _loginWithOAuthCommand = new DelegateCommand(StartOAuth);
            _showCredentialFormCommand = new DelegateCommand(StartCredentialAuth);
        }

        private void StartOAuth()
        {
            IsCredentialsOpen = false;
            ConfigureOAuth();
            DoLogin();
        }

        private void StartCredentialAuth()
        {
            IsCredentialsOpen = true;
        }

        private bool CanDoFormLogin() => !String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(Password);

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
                if (IsCredentialsOpen)
                {
                    AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialsFromFormAsync);
                    cred = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri(PortalUrl), UserName, Password);
                }
                else
                {
                    cred = await GetOAuthCredentials();
                }
                AuthenticationManager.Current.AddCredential(cred);

                // Create the portal with authentication info
                return await ArcGISPortal.CreateAsync(cred.ServiceUri, cred);
            }
            catch (ArcGISWebException e)
            {
                await DialogService.ShowMessageAsync($"Couldn't log in - {e.Message}");
                return null;
            }
            catch (OperationCanceledException e)
            {
                await DialogService.ShowMessageAsync($"Log in canceled");
                return null;
            }
        }

        private async Task<Credential> GetOAuthCredentials()
        {
            // define the credential request
            CredentialRequestInfo cri = new CredentialRequestInfo
            {
                // token authentication
                AuthenticationType = AuthenticationType.Token,
                // define the service URI
                ServiceUri = new Uri(ServerUrl),
                // OAuth (implicit flow) token type
                GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                }
            };

            // issue a proactive challenge by explicitly getting a credential
            Credential cred = await AuthenticationManager.Current.GetCredentialAsync(cri, true);

            return cred;
        }

        private void ConfigureOAuth()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
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

        private async Task<Credential> CreateCredentialsFromFormAsync(CredentialRequestInfo info)
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