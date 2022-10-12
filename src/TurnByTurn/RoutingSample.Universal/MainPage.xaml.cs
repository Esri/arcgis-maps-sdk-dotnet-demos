using RoutingSample.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
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

#warning Supply an API key in MainPage.xaml.cs, then delete this line
            // Create an API key at https://developers.arcgis.com/api-keys/, configure it with routing and geocoding services, then use it to replace YOUR_API_KEY below.
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "YOUR_API_KEY";

            _mainViewModel = new MainViewModel();
            _mainViewModel.LocationDisplay = MapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            DataContext = _mainViewModel;
            _ = CheckKey();
        }

        private async Task CheckKey()
        {
            if (Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey == "YOUR_API_KEY")
            {
                await new MessageDialog("See MainPage.xaml.cs for info about setting API key", "Error - No API Key provided").ShowAsync();
                System.Environment.Exit(0);
            }
        }
    }
}
