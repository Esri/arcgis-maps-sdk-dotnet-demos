using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RoutingSample.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var username = Username.Text.Trim();
            var password = Password.Text.Trim();
            try
            {
                var credential = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri("https://www.arcgis.com/sharing/rest"), username, password);
                AuthenticationManager.Current.AddCredential(credential);
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex);
            }
        }
    }
}