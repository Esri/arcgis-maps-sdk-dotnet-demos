using Esri.ArcGISRuntime.Portal;

namespace MauiSignin;

public partial class SignInPage : ContentPage
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private async void SignIn_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var arcgisPortal = await ArcGISPortal.CreateAsync(AppSettings.PortalUri, true);
            if(arcgisPortal != null)
            {
                SignInCompleted?.Invoke(this, arcgisPortal);
            }
        }
        catch(System.Exception)
        {
        }
    }

    public EventHandler<ArcGISPortal>? SignInCompleted;
}