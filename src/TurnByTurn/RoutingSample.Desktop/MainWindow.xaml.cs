using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using RoutingSample.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RoutingSample.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;

        public MainWindow()
        {
            // Create an API key at https://developers.arcgis.com/api-keys/, configure it with routing and geocoding services, then paste below.
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "";
            InitializeComponent();

            CheckKey();

            _mainViewModel = new MainViewModel();
            _mainViewModel.LocationDisplay = MapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            DataContext = _mainViewModel;
        }
        private void CheckKey()
        {
            if (string.IsNullOrEmpty(Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey))
            {
                MessageBox.Show("See MainWindow.xaml.cs for info about setting API key", "Error - No API Key provided", MessageBoxButton.OK, MessageBoxImage.Warning);
                System.Environment.Exit(0);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Turn-by-Turn Sample App",
                "About Turn-by-Turn Sample App", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
