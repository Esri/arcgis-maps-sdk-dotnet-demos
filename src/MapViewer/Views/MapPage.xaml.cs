using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ArcGISMapViewer.Views
{
    /// <summary>
    /// Contains the main MapView and it's supporting controls to work with the map
    /// </summary>
    public sealed partial class MapPage : Page
    {
        public MapPage()
        {
            this.InitializeComponent();
        }

        public ApplicationViewModel AppVM => ApplicationViewModel.Instance;

        public MapPageViewModel PageVM = new MapPageViewModel();

        public MapView MapView => mapView;

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (e.Location is null) return;
            mapView.DismissCallout();
            try
            {
                var result = await mapView.IdentifyLayersAsync(e.Position, 2, false);
                if (result.Any())
                {
                    mapView.ShowCalloutAt(e.Location, new Controls.IdentifyResultView() { IdentifyResult = result });
                }
            }
            catch
            {
            }
        }
    }
}