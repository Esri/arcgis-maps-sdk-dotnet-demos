using Esri.ArcGISRuntime.Security;

namespace RoutingSample.MAUI;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        LoginStatus.Text = "Signing in...";
        var username = Username.Text.Trim();
        var password = Password.Text.Trim();
        try
        {
            var credential = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri("https://www.arcgis.com/sharing/rest"), username, password);
            AuthenticationManager.Current.AddCredential(credential);
            LoginStatus.Text = "Success!";
            await Navigation.PushAsync(new MainPage());
            Navigation.RemovePage(this);
        }
        catch (Exception ex)
        {
            LoginStatus.Text = "Error: " + ex.Message;
        }
    }
}