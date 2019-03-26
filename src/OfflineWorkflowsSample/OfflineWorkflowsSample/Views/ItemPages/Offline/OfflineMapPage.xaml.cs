using System;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels;

namespace OfflineWorkflowSample.Views
{
    public sealed partial class OfflineMapPage : Page
    {
        private MainViewModel _mainVM = (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        public OfflineMapPage()
        {
            InitializeComponent();
        }

        private OfflineMapViewModel ViewModel => (OfflineMapViewModel) Resources[nameof(ViewModel)];

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                Map map = new Map(_mainVM.SelectedItem.Item);

                await map.LoadAsync();

                if (map.LoadStatus != LoadStatus.Loaded)
                {
                    throw new Exception("Map couldn't be loaded.");
                }

                ViewModel.MapViewService = _mainVM.MapViewService;
                ViewModel.Initialize(new Map(_mainVM.SelectedItem.Item), _mainVM.SelectedItem, _mainVM.PortalViewModel.Portal, _mainVM.WindowService);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                this.Frame.GoBack();
                await new MessageDialog("Couldn't load map.", "Error").ShowAsync();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // Reset the view model to avoid object already owned exceptions.
            ViewModel.Reset();
        }
    }
}