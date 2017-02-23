using Esri.ArcGISRuntime.Security;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RoutingSample
{
    public sealed partial class LoginPage : Page
    {
        private int loginAttempts = 0;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void OnSignIn_Click(object sender, RoutedEventArgs e)
        {
            LoginStatus.Text = "Signing in...";
            var username = Username.Text.Trim();
            var password = Password.Password.Trim();
            try
            {
                var credential = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri("http://www.arcgis.com/sharing/rest"), username, password);
                AuthenticationManager.Current.AddCredential(credential);
                LoginStatus.Text = string.Format("Signed in as: {0}", credential.UserName);
                this.Frame.Navigate(typeof(MainPage));
            }
            catch (Exception ex)
            {
                LoginStatus.Text = string.Format("Sign-in failed: {0}", ex.Message);
                loginAttempts++;
                if (loginAttempts >= 3)
                    Application.Current.Exit();
            }
        }
    }
}
