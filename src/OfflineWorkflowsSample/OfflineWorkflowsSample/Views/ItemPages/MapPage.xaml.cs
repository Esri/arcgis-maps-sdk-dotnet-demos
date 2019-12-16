using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels.ItemPages;

namespace OfflineWorkflowSample.Views.ItemPages
{
    public sealed partial class MapPage : Page
    {
        private readonly MainViewModel _mainVM = (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        public MapPage()
        {
            InitializeComponent();
        }

        private MapPageViewModel ViewModel => (MapPageViewModel) Resources[nameof(ViewModel)];

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Item item = _mainVM.SelectedItem.Item;
            try
            {
                Map map;
                if (item is LocalItem localItem)
                {
                    // This logic is quite brittle and only valid for MMPKs created as a result of 
                    //   taking a map offline with this app. 
                    string mmpkPath = localItem.Path;

                    var mmpk = await MobileMapPackage.OpenAsync(mmpkPath);

                    // Get the first map.
                    map = mmpk.Maps.First();
                }
                else
                {
                    map = new Map(_mainVM.SelectedItem.Item as PortalItem);
                }

                await map.LoadAsync();

                if (map.LoadStatus != LoadStatus.Loaded)
                {
                    throw new Exception("Map couldn't be loaded.");
                }

                ViewModel.Initialize(map, _mainVM.SelectedItem);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                this.Frame.GoBack();
                await new MessageDialog("Couldn't load map.", "Error").ShowAsync();
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // Reset the view model to avoid object already owned exceptions.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ViewModel.Reset);
        }
    }
}