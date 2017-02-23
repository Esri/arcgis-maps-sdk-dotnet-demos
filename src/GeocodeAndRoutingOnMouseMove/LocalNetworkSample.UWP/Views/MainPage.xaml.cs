using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LocalNetworkSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            var vm = Resources["vm"] as MainPageVM;
            vm.GeoView = mapview;
            vm.LocationDisplay = mapview.LocationDisplay;
            mapview.GraphicsOverlays = vm.GraphicsOverlays;
        }

        private void mapview_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			var mapview = (Esri.ArcGISRuntime.UI.MapView)sender;
			var vm = (MainPageVM)mapview.DataContext;
			vm.UpdateMouseLocation(mapview.ScreenToLocation(e.GetCurrentPoint(mapview).Position));
		}
    }
}
