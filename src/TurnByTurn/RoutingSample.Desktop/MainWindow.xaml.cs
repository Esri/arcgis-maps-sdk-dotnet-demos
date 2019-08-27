using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using RoutingSample.ViewModels;
using System;
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
            InitializeComponent();

            _mainViewModel = new MainViewModel();
            _mainViewModel.LocationDisplay = MapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            DataContext = _mainViewModel;

#if OFFLINE

#else
            //_mainViewModel.Destination = "277 N Avenida Caballeros, Palm Springs, CA";

            InitializeAuth();
#endif
        }

#if !OFFLINE
        private async void InitializeAuth()
        {

            if (!await OAuth.AuthorizeAsync())
            {
                MessageBox.Show("This sample requires an ArcGIS Online subscription " +
                    "in order to use the Global Routing Service.",
                    "ArcGIS Online Required", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }
#endif

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Turn-by-Turn Sample App",
                "About Turn-by-Turn Sample App", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            //_mainViewModel.FindRoute();
        }

        private void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Set the destination ;)
            _mainViewModel.Destination = MapView.ScreenToLocation(e.Position);
        }

        private void MapView_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
