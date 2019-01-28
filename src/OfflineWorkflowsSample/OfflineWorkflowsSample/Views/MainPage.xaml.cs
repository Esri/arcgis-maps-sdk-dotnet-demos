﻿using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowSample.ViewModels;
using OfflineWorkflowSample.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace OfflineWorkflowsSample
{
    /// <summary>
    /// A map page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IWindowService
	{
		public MainPage()
		{            
            this.InitializeComponent();
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

        private MainViewModel ViewModel => (MainViewModel)Resources["ViewModel"];

        public void ShowMapItem(Map map)
        {
            ViewModel.SelectMap(map);
            Frame.Navigate(typeof(MapPage), ViewModel);
        }

        private void OfflineMapSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
            {
                Item selectedItem = e.AddedItems.First() as Item;
                // Creation of maps from local items isn't supported,
                // so the maps and their items are stored in a dictionary for easy lookup
                Map selectedMap = ViewModel.OfflineMapsViewModel.MapItems[selectedItem];
                ShowMapItem(selectedMap);
            }
        }

        private void FeaturedMapSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
            {
                Item selectedItem = e.AddedItems.First() as Item;
                Map selectedMap = new Map(selectedItem);
                ShowMapItem(selectedMap);
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
    }
}