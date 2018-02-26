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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeoEventServerSample.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public GeoEventServerSample.MapViewModel VM { get; } = new MapViewModel();

        private async void mapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            var r = await mapView.IdentifyGraphicsOverlayAsync(mapView.GraphicsOverlays[0], e.Position, 1, false);
            r.GraphicsOverlay.ClearSelection();
            var g = r.Graphics?.FirstOrDefault();
            attributeView.ItemsSource = g?.Attributes;
            if (g != null)
                g.IsSelected = true;
        }
    }
}
