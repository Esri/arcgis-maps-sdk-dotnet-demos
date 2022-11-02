using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RoutingSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private int _loginAttempts;

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginStatus.Text = "Signing in...";
            var username = Username.Text.Trim();
            var password = Password.Password.Trim();
            try
            {
                var credential = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri("https://www.arcgis.com/sharing/rest"), username, password);
                AuthenticationManager.Current.AddCredential(credential);
                LoginStatus.Text = "Success! Signed in as: " + credential.UserName;
                Frame.Navigate(typeof(MainPage));
            }
            catch (Exception ex)
            {
                LoginStatus.Text = "Error: " + ex.Message;
                _loginAttempts++;
                if (_loginAttempts >= 3)
                    Application.Current.Exit();
            }
        }
    }
}
