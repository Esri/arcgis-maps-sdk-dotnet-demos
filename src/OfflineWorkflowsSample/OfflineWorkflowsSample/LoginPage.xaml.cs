using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using OfflineWorkflowsSample;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page, IDialogService
    {
        public LoginPage()
        {
            this.InitializeComponent();
            ExtendAcrylicIntoTitleBar();

            // Using view service approach with the dialog service to abstract messages from ViewModels
            ViewModel = new MainViewModel(this as IDialogService);
            DataContext = ViewModel;
        }

        public MainViewModel ViewModel { get; }

        private void ExtendAcrylicIntoTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        public async Task ShowMessageAsync(string message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }

        private void Login_Clicked(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private async void Login()
        {
            // Set up the view model (including authentication)
            await ViewModel.ConfigurePortal();

            // Navigate to the main page, passing the view model as an argument.
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), ViewModel);
        }

        private void Entry_Keydown(object sender, KeyRoutedEventArgs e)
        {
            // Allow log in with enter key.
            if (e.Key == VirtualKey.Enter)
            {
                Login();
            }
        }
    }

    
}