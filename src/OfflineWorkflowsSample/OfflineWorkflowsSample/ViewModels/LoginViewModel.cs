using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using OfflineWorkflowsSample;
using OfflineWorkflowsSample.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        // Commands enable binding controls to behavior. https://visualstudiomagazine.com/articles/2012/04/10/command-pattern-in-net.aspx
        private readonly DelegateCommand _logInToEnterpriseCommand;
        private readonly DelegateCommand _logInToPortalCommand;
        private readonly DelegateCommand _showEnterpriseFormCommand;

        private bool _isLoggingIn;

        private bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                SetProperty(ref _isLoggingIn, value);
                _logInToPortalCommand.RaiseCanExecuteChanged();
                _showEnterpriseFormCommand.RaiseCanExecuteChanged();
                _logInToEnterpriseCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _portalFormOpen;

        private string _portalUrl = ArcGISOnlinePortalUrl;
        
        // WindowService allows the ViewModel to communicate with the view without
        //     exposing details of the view to the ViewModel. 
        public IWindowService WindowService = null;

        public LoginViewModel()
        {
            _logInToPortalCommand = new DelegateCommand(LoginToAgol, () => !IsLoggingIn);
            _logInToEnterpriseCommand = new DelegateCommand(LoginToEnterprise, () => !IsLoggingIn);
            _showEnterpriseFormCommand = new DelegateCommand(TogglePortalForm, () => !IsLoggingIn);
        }

        public ICommand LogInToPortalCommand => _logInToPortalCommand;
        public ICommand LogInToEnterpriseCommand => _logInToEnterpriseCommand;
        public ICommand ShowEnterpriseFormCommand => _showEnterpriseFormCommand;

        private ArcGISPortal Portal { get; set; }

        public string PortalUrl
        {
            get => _portalUrl;
            set => SetProperty(ref _portalUrl, value);
        }

        public UserProfileModel UserProfile { get; private set; }

        public bool PortalFormIsOpen
        {
            get => _portalFormOpen;
            private set => SetProperty(ref _portalFormOpen, value);
        }

        private void LoginToAgol()
        {
            // Update the portal URL.
            _portalUrl = ArcGISOnlinePortalUrl;

            // Complete the login.
            DoLogin();
        }

        private void LoginToEnterprise()
        {
            DoLogin();
        }

        private async void DoLogin()
        {
            IsLoggingIn = true;

            try
            {
                // Set up OAuth authentication.
                ConfigureOAuth();

                // Authenticate and load the portal.
                Portal = await AuthenticateAndCreatePortal();

                // Skip out if the portal is null.
                if (Portal != null)
                {
                    UserProfile = new UserProfileModel(Portal.User);
                    RaiseLoggedIn();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await WindowService.ShowAlertAsync("Couldn't log in to the portal.", "Log in failed");
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private void TogglePortalForm()
        {
            PortalFormIsOpen = !PortalFormIsOpen;
        }
        
        private async Task<ArcGISPortal> AuthenticateAndCreatePortal()
        {
            try
            {
                // Authenticate
                CredentialRequestInfo cri = new CredentialRequestInfo {ServiceUri = new Uri(_portalUrl)};
                var cred = await AuthenticationManager.Current.GetCredentialAsync(cri, true);

                // Create the portal with authentication info
                return await ArcGISPortal.CreateAsync(cred.ServiceUri);
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
                await WindowService.ShowAlertAsync("Log in canceled");
                return null;
            }
        }

        private void ConfigureOAuth()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo serverInfo = new ServerInfo(new Uri(_portalUrl))
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,

                OAuthClientInfo = new OAuthClientInfo(AppClientId, new Uri(OAuthRedirectUrl))
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
            Credential credential;
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

        #region log in event

        // Event allows the view to listen for successful log in.
        public delegate void LoginCompletionHandler(object sender);

        public event LoginCompletionHandler CompletedLogin;

        private void RaiseLoggedIn()
        {
            CompletedLogin?.Invoke(this);
        }
        #endregion log in event

        #region OAuth constants

        // TODO - Make sure these are up to date with the registration in ArcGIS Online or your portal.
        private const string AppClientId = "YOUR_APP_CLIENT_ID_HERE";
        private const string ClientSecret = "GET IT FROM https://developers.arcgis.com/applications/";
        private const string OAuthRedirectUrl = @"DON'T FORGET A REDIRECT URL";
        private const string ArcGISOnlinePortalUrl = "https://www.arcgis.com/sharing/rest";

        #endregion OAuth constants
    }
}