using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Data;
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
            mapView.DismissCallout();
            if (e.Location is null) return;
            try
            {
                var result = await mapView.IdentifyLayersAsync(e.Position, 2, false, 10);
                if (result.Any())
                {
                    var calloutview = new Controls.IdentifyResultView() { IdentifyResult = result, GeoViewController = PageVM.ViewController };
                    mapView.ShowCalloutAt(e.Location, calloutview);
                    calloutview.EditRequested += (s, e) =>
                    {
                        PageVM.CurrentFeature = e as Feature;
                        RightPanel.IsOpen = true;
                    };
                    calloutview.CloseRequested += (s, e) => mapView.DismissCallout();
                }
            }
            catch
            {
            }
        }

        private void OnFeatureEditingEnded(object sender, EventArgs e)
        {
            RightPanel.IsOpen = false;
            PageVM.CurrentFeature = null;
        }


        private async void AddBookmark_Click(object sender, RoutedEventArgs e)
        {
            var vp = mapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);
            var name = $"{CoordinateFormatter.ToLatitudeLongitude((MapPoint)vp.TargetGeometry, LatitudeLongitudeFormat.DecimalDegrees, 6)} - 1:{Math.Round(vp.TargetScale)}";
            ContentDialog cd = new ContentDialog()
            {
                Title = "Add Bookmark",
                Content = new TextBox() { Text = name, Header = "Enter bookmark name", PlaceholderText = "Bookmark Name" },
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            if (await cd.ShowAsync() == ContentDialogResult.Primary)
            {
                if (AppVM.Map != null)
                    AppVM.Map.Bookmarks.Add(new Bookmark() { Name = ((TextBox)cd.Content).Text, Viewpoint = vp });
            }
        }
    }
}
