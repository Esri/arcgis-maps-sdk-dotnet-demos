﻿using OfflineWorkflowSample.ViewModels;
using OfflineWorkflowsSample;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Esri.ArcGISRuntime.Security;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page, IWindowService
    {
        public LoginPage()
        {
            this.InitializeComponent();
            ExtendAcrylicIntoTitleBar();
            ViewModel.WindowService = this;
            ViewModel.CompletedLogin += sender => Login();
        }

        private LoginViewModel ViewModel => (LoginViewModel)Resources["ViewModel"];

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
            rootFrame.Navigate(typeof(MainPage), ViewModel);
        }

        public async Task ShowAlertAsync(string message)
        {
            await ShowAlertAsync(message, "");
        }

        public async Task ShowAlertAsync(string message, string title)
        {
            
            var messageDialog = new MessageDialog(message, title);
            await messageDialog.ShowAsync();
        }

        public void SetBusy(bool isBusy)
        {
            throw new NotImplementedException();
        }

        public void SetBusyMessage(string message)
        {
            throw new NotImplementedException();
        }

        
    }
}