using System;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowSample.ViewModels;
using OfflineWorkflowSample.Views;
using OfflineWorkflowsSample;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample
{
    public sealed partial class LoginPage : Page, IWindowService
    {
        public LoginPage()
        {
            InitializeComponent();
            ExtendAcrylicIntoTitleBar();
            ViewModel.WindowService = this;
            ViewModel.CompletedLogin += sender => Login();
            
            // Configure the title bar.
            Window.Current.SetTitleBar(DraggablePart);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
        }

        private LoginViewModel ViewModel => (LoginViewModel) Resources["ViewModel"];

        public void LaunchItem(Item item)
        {
            throw new InvalidOperationException("Can't launch item - not logged in.");
        }

        public void NavigateToLoginPage(){}

        public void NavigateToPageForItem(PortalItemViewModel item)
        {
            throw new InvalidOperationException("Can't navigate to item - not logged in.");
        }

        public void SetBusy(bool isBusy){}

        public void SetBusyMessage(string message){}

        public async Task ShowAlertAsync(string message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }

        public async Task ShowAlertAsync(string message, string title)
        {
            var messageDialog = new MessageDialog(message, title);
            await messageDialog.ShowAsync();
        }

        private void ExtendAcrylicIntoTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void Login()
        {
            // Navigate to the main page, passing the view model as an argument.
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(NavigationPage), ViewModel);
        }
    }
}