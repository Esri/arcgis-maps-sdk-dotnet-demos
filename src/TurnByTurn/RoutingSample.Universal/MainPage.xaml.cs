using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using RoutingSample.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RoutingSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IChallengeHandler
    {
        public MainPage()
        {
            this.InitializeComponent();
            
            var viewModel = (MainPageVM)MyMapView.DataContext;
            viewModel.LocationDisplay = MyMapView.LocationDisplay;

            AuthenticationManager.Current.ChallengeHandler = this;
        }


        #region IChallendHandler 

        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo requestInfo)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            if (dispatcher == null)
                return await ChallengeUI(requestInfo);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => await ChallengeUI(requestInfo));
            return await loginTask.Task;
        }

        TaskCompletionSource<Credential> loginTask = null;
        int loginAttempt = 0;
        private async Task<Credential> ChallengeUI(CredentialRequestInfo cri)
        {
            try
            {
                loginTask = new TaskCompletionSource<Credential>();
                LoginUI.Visibility = Visibility.Visible;
                return await loginTask.Task;
            }
            finally
            {
                LoginUI.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnSignIn_Click(object sender, RoutedEventArgs e)
        {
            var username = Username.Text.Trim();
            var password = Password.Password.Trim();
            try
            {
                var credential = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri("http://www.arcgis.com/sharing/rest"), username, password);
                loginTask.TrySetResult(credential);
            }
            catch (Exception ex)
            {
                LoginError.Text = ex.Message;
                loginAttempt++;
                if (loginAttempt >= 3)
                    loginTask.TrySetException(ex);
            }
        }
        #endregion IChallendHandler
    }
}
