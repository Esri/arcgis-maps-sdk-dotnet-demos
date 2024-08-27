using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Messaging;
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
            WeakReferenceMessenger.Default.Register<Controls.MapPropertiesView.ShowMapPropertiesMessage>(this, (r, m) =>
            {
                var panel = RightPanel.Items.Where(p => p.Tag as string == "Properties").First();
                RightPanel.SelectedItem = panel;
                RightPanel.IsOpen = true;
            });

        }

        public Controls.GeoViewWrapper GeoViewWrapper => geoViewWrapper;

        public ApplicationViewModel AppVM => ApplicationViewModel.Instance;

        public MapPageViewModel PageVM = new MapPageViewModel();

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // TODO: Rewrite to operate in VM
            geoViewWrapper.GeoViewController.DismissCallout();
            if (e.Location is null) return;
            try
            {
                var result = await geoViewWrapper.IdentifyLayersAsync(e.Position, 2, false, 10);
                if (result.Any())
                {
                    var calloutview = new Controls.IdentifyResultView() { IdentifyResult = result, GeoViewController = PageVM.ViewController };
                    geoViewWrapper.ShowCalloutAt(e.Location, calloutview);
                    calloutview.EditRequested += (s, e) =>
                    {
                        PageVM.CurrentFeature = e as Feature;
                        // TODO: Rewrite as an MVVM message
                        var panel = RightPanel.Items.Where(p => p.Tag as string == "Edit").First();
                        panel.IsEnabled = true;
                        RightPanel.SelectedItem = panel;
                        RightPanel.IsOpen = true;
                    };
                    calloutview.CloseRequested += (s, e) => geoViewWrapper.GeoViewController.DismissCallout();
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
            RightPanel.Items.Where(p => p.Tag as string == "Edit").First().IsEnabled = false;
        }


        private async void AddBookmark_Click(object sender, RoutedEventArgs e)
        {
            var vp = geoViewWrapper.GeoViewController.GetCurrentViewpoint(ViewpointType.CenterAndScale);
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
                if (AppVM.GeoModel != null)
                    AppVM.GeoModel.Bookmarks.Add(new Bookmark() { Name = ((TextBox)cd.Content).Text, Viewpoint = vp });
            }
        }
    }
}
