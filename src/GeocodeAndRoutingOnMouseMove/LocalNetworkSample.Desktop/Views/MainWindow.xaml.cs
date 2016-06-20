using System.Windows;
using System.Windows.Input;

namespace LocalNetworkSample.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = Resources["vm"] as MainPageVM;
            vm.GeoView = mapview;
            vm.LocationDisplay = mapview.LocationDisplay;
            mapview.GraphicsOverlays = vm.GraphicsOverlays;
        }

        private void mapview_PointerMoved(object sender, MouseEventArgs e)
        {
            var mapview = (Esri.ArcGISRuntime.UI.MapView)sender;
            var vm = (MainPageVM)mapview.DataContext;
            vm.UpdateMouseLocation(mapview.ScreenToLocation(e.GetPosition(mapview)));
        }
    }
}
