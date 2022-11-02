using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RoutingSample.Desktop
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const int MaxLoginAttempts = 3;
        private int _loginAttempts = 0;

        public LoginWindow()
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
                new MainWindow().Show();
                Close();
            }
            catch (Exception ex)
            {
                LoginStatus.Text = "Error: " + ex.Message;
                _loginAttempts++;
                if (_loginAttempts >= MaxLoginAttempts)
                    Close();
            }
        }
    }
}
