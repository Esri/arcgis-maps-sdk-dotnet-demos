using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;

namespace MauiSignin;

public partial class StartupPage : ContentPage
{
    public StartupPage()
    {
        InitializeComponent();
        this.Loaded += StartupPage_Loaded;
    }

    private async void StartupPage_Loaded(object? sender, EventArgs e)
    {
        if (AppSettings.OAuthClientId == "SET_CLIENT_ID" || AppSettings.OAuthRedirectUri.OriginalString.Contains($"SET_REDIRECT_URL"))
        {
            // Application isn't configured. Please update the oauth settings by using the ArcGIS Developer Portal at
            // https://developers.arcgis.com/applications
            System.Diagnostics.Debugger.Break();
            throw new InvalidOperationException("Please configure your client id and redirect url in 'ApplicationConfiguration.xaml' to run this sample");
        }

        progress.Progress += .2;
        status.Text = "Initializing ArcGIS Runtime...";

        Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();

#if WINDOWS
        AuthenticationManager.Current.Persistence = await WinUI.AppDataCredentialPersistence.CreateAsync();
#else
        AuthenticationManager.Current.Persistence = await CredentialPersistence.CreateDefaultAsync();
#endif
        //Register server info for portal
        ServerInfo portalServerInfo = new ServerInfo(AppSettings.PortalUri)
        {
            TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
            OAuthClientInfo = new OAuthClientInfo(AppSettings.OAuthClientId, AppSettings.OAuthRedirectUri)
        };
        AuthenticationManager.Current.RegisterServer(portalServerInfo);


        var licenseStatus = Esri.ArcGISRuntime.LicenseStatus.Invalid;
        var licenseJson = await SecureStorage.GetAsync("License");
        if (!string.IsNullOrEmpty(licenseJson))
        {
            status.Text = "Checking license...";
            try
            {
                var result = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(LicenseInfo.FromJson(licenseJson)!);
                licenseStatus = result.LicenseStatus;
            }
            catch { }
        }

        progress.Progress += .2;
        status.Text = "Signing in to ArcGIS Online...";

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

        AuthenticationManager.Current.ChallengeHandler = new DefaultChallengeHandler();
        AuthenticationManager.Current.OAuthAuthorizeHandler = OAuthAuthorizeHandler.Instance;

        progress.Progress += .2;
        if(AppSettings.Instance.Portal is null)
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

        if (licenseStatus != Esri.ArcGISRuntime.LicenseStatus.Valid)
        {
            // Refresh license from portal
            progress.Progress += .2;
            status.Text = "Getting updated license...";
            var license = await AppSettings.Instance.Portal!.GetLicenseInfoAsync();
            await SecureStorage.SetAsync("License", license.ToJson());
            var result = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(license);
            licenseStatus = result.LicenseStatus;
        }
        status.Text = "Finishing up...";

        progress.Progress += .2;

        App.Current.MainPage = new AppShell();
        //await Shell.Current.GoToAsync("//PortalPage");
    }
}