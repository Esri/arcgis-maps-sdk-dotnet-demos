using Esri.ArcGISRuntime.UI;
using RoutingSample.ViewModels;
using System.Windows;

namespace RoutingSample.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

		private void Exit_Clicked(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

        private void OnMapViewLoaded(object sender, RoutedEventArgs e)
        {
            var mapView = (MapView)sender;
            var viewModel = (MainPageVM)mapView.DataContext;
            viewModel.LocationDisplay = mapView.LocationDisplay;
        }
    }
}
