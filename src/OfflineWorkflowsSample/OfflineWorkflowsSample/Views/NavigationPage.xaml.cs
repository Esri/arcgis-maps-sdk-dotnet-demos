﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowSample.Views.ItemPages;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationPage : Page, IWindowService
    {
        private MainViewModel ViewModel => (MainViewModel)Application.Current.Resources[nameof(MainViewModel)];

        public NavigationPage()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(DraggablePart);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!ViewModel.IsInitialized)
            {
                LoginViewModel vm = (LoginViewModel)e.Parameter;
                await ViewModel.Initialize(vm.Portal, vm.UserProfile, this);
            }
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
            ViewModel.IsBusy = isBusy;
        }

        public void SetBusyMessage(string message)
        {
            ViewModel.IsBusyText = message;
        }

        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItemContainer.Tag.ToString())
            {
                case "Local":
                    ContentFrame.Navigate(typeof(OfflineMapsView), new SuppressNavigationTransitionInfo());
                    break;
                case "Folders":
                    ContentFrame.Navigate(typeof(PortalBrowserView), new SuppressNavigationTransitionInfo());
                    break;
                case "Groups":
                    ContentFrame.Navigate(typeof(PortalGroupView), new SuppressNavigationTransitionInfo());
                    break;
                case "Search":
                    break;
            }
        }

        public void NavigateToPageForItem(Item item)
        {
            if (item is LocalItem localItem)
            {
                switch (localItem.Type)
                {
                    case LocalItemType.MobileMap:
                        ContentFrame.Navigate(typeof(MapPage));
                        break;
                }
            }
            else if (item is PortalItem portalItem)
            {
                switch (portalItem.Type)
                {
                    case PortalItemType.WebMap:
                        if (portalItem.TypeKeywords.Contains("Offline"))
                            ContentFrame.Navigate(typeof(OfflineMapPage));
                        else
                            ContentFrame.Navigate(typeof(MapPage));
                        break;
                    case PortalItemType.WebScene:
                        ContentFrame.Navigate(typeof(ScenePage));
                        break;
                }
            }
        }

        private void NavigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack(new SuppressNavigationTransitionInfo());
            }
        }
    }
}