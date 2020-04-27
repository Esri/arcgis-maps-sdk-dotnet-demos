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

        private void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            //// Set the destination
            //_mainViewModel.Destination = MapView.ScreenToLocation(e.Position);
        }
    }
}
