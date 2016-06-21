using Esri.ArcGISRuntime.Security;
using RoutingSample.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RoutingSample.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IChallengeHandler
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var viewModel = (MainPageVM)MyMapView.DataContext;
            viewModel.LocationDisplay = MyMapView.LocationDisplay;

            AuthenticationManager.Current.ChallengeHandler = this;
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region IChallendHandler 

        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo requestInfo)
        {
            if (Dispatcher == null)
                return await ChallengeUI(requestInfo);
            return await Dispatcher.Invoke(() => ChallengeUI(requestInfo));
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
            catch(Exception ex)
            {
                LoginError.Text = ex.Message;
                loginAttempt++;
                if(loginAttempt >= 3)
                    loginTask.TrySetException(ex);
            }
        }
        #endregion IChallendHandler
    }
}
