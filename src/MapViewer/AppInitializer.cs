using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;

namespace ArcGISMapViewer;

internal class AppInitializer
{
    public AppInitializer()
    {
    }

    public async Task Initialize()
    {
        StatusText = Resources.GetString("InitializingMapsSDKStatus");

        //Register server info for portal
        var settings = ApplicationViewModel.Instance.AppSettings;
        if (settings.OAuthClientId == "SET_CLIENT_ID" || settings.OAuthRedirectUrl == "SET_REDIRECT_URL")
        {
            // Application isn't configured. Please update the oauth settings by using the ArcGIS Developer Portal at
            // https://developers.arcgis.com/applications
            System.Diagnostics.Debugger.Break();
            throw new InvalidOperationException("Please configure your client id and redirect url in 'ApplicationConfiguration.xaml' to run this sample");
        }

        Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize( config => config
            .ConfigureAuthentication(auth => auth
            .AddOAuthUserConfiguration(new OAuthUserConfiguration(settings.PortalUrl, settings.OAuthClientId, new Uri(settings.OAuthRedirectUrl)))
            )
            .ConfigureHttp(http => http
                .UseResponseCacheSize(200 * 1024 * 1024) // Increase cache size to 200mb
                    .UseDefaultReferer(new Uri("https://github.com/Esri/arcgis-runtime-demos-dotnet/ArcGISMapViewer"))
                    .AddUserAgentValue("ArcGISMapViewer", "1.0")
            )
        );
        AuthenticationManager.Current.Persistence = await CredentialPersistence.CreateDefaultAsync();

        Progress += 20;


        if (!string.IsNullOrEmpty(ApplicationViewModel.Instance.AppSettings.PortalUser)) // We have signed in before
        {
            StatusText = Resources.GetString("SigningIntoArcGISOnlineStatus");
            try
            {
                // Do this without oauth handler - we want to fail if credential persistance was empty / or stored credentials no longer working
                ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(settings.PortalUrl, true);
                StatusText = Resources.GetString("LoadingUserStatus");
                await ApplicationViewModel.Instance.SetUserAsync(arcgisPortal.User!);
            }
            catch (Exception)
            {
                AuthenticationManager.Current.RemoveAllCredentials(); // Start over
            }
        }

        AuthenticationManager.Current.ChallengeHandler = new DefaultChallengeHandler();
        AuthenticationManager.Current.OAuthAuthorizeHandler = OAuthAuthorizeHandler.Instance;

        StatusText = Resources.GetString("CheckingLicenseStatus");
        var license = ApplicationViewModel.Instance.AppSettings.License;
        var licenseStatus = Esri.ArcGISRuntime.LicenseStatus.Invalid;
        if (license is not null)
        {
            try
            {
                var result = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(license);
                licenseStatus = result.LicenseStatus;
            }
            catch { }
        }

        Progress += 20;

        if (ApplicationViewModel.Instance.PortalUser is null)
        {

            TaskCompletionSource<PortalUser> signinTask = new TaskCompletionSource<PortalUser>();
            SigninRequested?.Invoke(this, signinTask);
            try
            {
                var user = await signinTask.Task;
                await ApplicationViewModel.Instance.SetUserAsync(user);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        if (licenseStatus != Esri.ArcGISRuntime.LicenseStatus.Valid)
        {
            // Refresh license from portal
            Progress += 20;
            StatusText = Resources.GetString("GettingUpdatedLicenseStatus");
            license = await ApplicationViewModel.Instance.PortalUser!.Portal.GetLicenseInfoAsync();
            ApplicationViewModel.Instance.AppSettings.License = license;
            var result = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(license);
            if (result.LicenseStatus != Esri.ArcGISRuntime.LicenseStatus.Valid)
                throw new NotSupportedException(string.Format(Resources.GetString("GetLicenseFromPortalError"), result.LicenseStatus));
        }
        StatusText = Resources.GetString("FinishingUpStatus");
        Progress = 80;
        var lastMap = await ApplicationViewModel.Instance.LoadLastMapAsync();
        ApplicationViewModel.Instance.IsMapVisible = lastMap is not null;
        Progress = 100;
        ApplicationInitialized?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<int>? ProgressChanged;

    public event EventHandler<string>? StatusTextChanged;

    public event EventHandler<TaskCompletionSource<PortalUser>>? SigninRequested;
    public event EventHandler? ApplicationInitialized;

    private string m_text = "";

    public string StatusText
    {
        get { return m_text; }
        private set { m_text = value; StatusTextChanged?.Invoke(this, value); }
    }

    private int m_progress;

    public int Progress
    {
        get { return m_progress; }
        private set { m_progress = value; ProgressChanged?.Invoke(this, value); }
    }
}
