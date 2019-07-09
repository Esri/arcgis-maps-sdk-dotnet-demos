using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfClusterer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MapViewModel viewModel; 
        public MainWindow()
        {
            InitializeComponent();
            viewModel = Resources["MapViewModel"] as MapViewModel;
            PointClusterer.SetClusterer(MainMapView, viewModel.Clusterer);
        }

        private void MapView_NavigationCompleted(object sender, EventArgs e)
        {
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}
