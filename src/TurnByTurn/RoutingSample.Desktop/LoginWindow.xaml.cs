using Esri.ArcGISRuntime.Security;
using System;
using System.Windows;

namespace RoutingSample.Desktop
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            MessageBox.Show("This sample requires ArcGIS Online Subscription in order to use Global Routing Service.", this.Title.Text, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        int loginAttempts = 0;
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
                new MainWindow().Show();
                this.Close();
            }
            catch (Exception ex)
            {
                LoginStatus.Text = string.Format("Sign-in failed: {0}", ex.Message);
                loginAttempts++;
                if (loginAttempts >= 3)
                    this.Close();
            }
        }
    }
}