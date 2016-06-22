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

            var viewModel = (MainPageVM)MyMapView.DataContext;
            viewModel.LocationDisplay = MyMapView.LocationDisplay;
        }
        
        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
