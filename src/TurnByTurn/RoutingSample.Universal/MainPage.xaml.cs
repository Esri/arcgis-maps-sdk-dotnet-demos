using RoutingSample.ViewModels;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoutingSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainViewModel _mainViewModel;

        public MainPage()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel();
            _mainViewModel.LocationDisplay = MapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            DataContext = _mainViewModel;

#if !OFFLINE
            InitializeAuth();
#endif
        }

#if !OFFLINE
        private async void InitializeAuth()
        {
            if (!await OAuth.AuthorizeAsync())
            {
                await new ContentDialog
                {
                    Content = "This sample requires an ArcGIS Online subscription " +
                    "in order to use the Global Routing service.",
                    Title = "ArcGIS Online Required",
                    CloseButtonText = "OK"
                }.ShowAsync();

                Application.Current.Exit();
            }
        }
#endif

        private void MapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            _mainViewModel.Destination = MapView.ScreenToLocation(e.Position);
        }
    }
}
