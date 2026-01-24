using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;

namespace MauiSignin;

public partial class StartupPage : ContentPage
{
    private bool _firstTimeSetupComplete = false;
    private LicenseStatus _licenseStatus = LicenseStatus.Invalid;

    public StartupPage()
    {
        InitializeComponent();
        this.Loaded += StartupPage_Loaded;
    }

    private async void StartupPage_Loaded(object? sender, EventArgs e)
    {
        status.Text = "Initializing ArcGIS Maps SDK...";

        // "loaded" event happens every time we return to the StartupPage
        // (in the beginning, after OAuth authorization, and again after signing out)
        // but some of the initialization only needs to happen once.
        if (!_firstTimeSetupComplete)
        {
            await FirstTimeSetupAsync();
        }

        progress.Progress = 0.5;
        status.Text = "Signing in to the portal...";
        if (AppSettings.Instance.Portal is null)
        {
            var page = new SignInPage();
            TaskCompletionSource<ArcGISPortal> signinTask = new TaskCompletionSource<ArcGISPortal>();
            page.SignInCompleted += (s, e) => signinTask.SetResult(e);
            await Navigation.PushModalAsync(page);
            var portal = await signinTask.Task;
#if WINDOWS
            var window = this.Window.Handler.PlatformView as Microsoft.UI.Xaml.Window;
            if (window != null)
                WinUIEx.WindowExtensions.SetForegroundWindow(window); //Bring window to the front after browser sign in
#endif
            AppSettings.Instance.SetUser(portal.User);
            await Navigation.PopModalAsync();
        }

        if (_licenseStatus != Esri.ArcGISRuntime.LicenseStatus.Valid)
        {
            // Refresh license from portal
            progress.Progress = 0.75;
            status.Text = "Getting updated license...";
            var license = await AppSettings.Instance.Portal!.GetLicenseInfoAsync();
            await SecureStorage.SetAsync("License", license.ToJson());
            var result = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(license);
            _licenseStatus = result.LicenseStatus;
        }
        progress.Progress = 1;
        status.Text = "Finishing up...";

        App.Current!.MainPage = new AppShell();
    }

    private async Task FirstTimeSetupAsync()
    {
        if (AppSettings.OAuthClientId == "SET_CLIENT_ID" || AppSettings.OAuthRedirectUri.OriginalString.Contains($"SET_REDIRECT_URL"))
        {
            // Application isn't configured. Please update the oauth settings by using the ArcGIS Developer Portal at
            // https://developers.arcgis.com/applications
            System.Diagnostics.Debugger.Break();
            throw new InvalidOperationException("Please configure your client id and redirect url in 'AppSettings.cs' to run this sample");
        }

        AuthenticationManager.Current.Persistence = await CredentialPersistence.CreateDefaultAsync();

        //Register oauth app info for portal
        AuthenticationManager.Current.OAuthUserConfigurations.Add(new OAuthUserConfiguration(AppSettings.PortalUri, AppSettings.OAuthClientId, AppSettings.OAuthRedirectUri));

        if (AuthenticationManager.Current.Credentials.Any())
        {
            // Old credentials restored from persistance. Try to use them to load the portal
            try
            {
                // Do this without oauth handler - we want to fail if credential persistance was empty / or stored credentials no longer working
                var portal = await ArcGISPortal.CreateAsync(AppSettings.PortalUri, true);
                AppSettings.Instance.SetUser(portal.User);
            }
            catch (Exception)
            {
                await AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
            }
        }
        progress.Progress = 0.25;

        AuthenticationManager.Current.ChallengeHandler = new DefaultChallengeHandler();
        AuthenticationManager.Current.OAuthAuthorizeHandler = OAuthAuthorizeHandler.Instance;
        
        var licenseJson = await SecureStorage.GetAsync("License");
        if (!string.IsNullOrEmpty(licenseJson))
        {
            status.Text = "Checking license...";
            try
            {
                var result = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(LicenseInfo.FromJson(licenseJson)!);
                _licenseStatus = result.LicenseStatus;
            }
            catch { }
        }
        _firstTimeSetupComplete = true;
    }
}