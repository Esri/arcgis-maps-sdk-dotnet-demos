using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

            WeakReferenceMessenger.Default.Register<ShowRightPanelMessage>(this, (r, m) =>
            {
                var panel = RightPanel.Items.Where(p => p.Tag as string == m.Panel.ToString()).First();
                RightPanel.SelectedItem = panel;
                RightPanel.IsOpen = true;
                switch (m.Panel)
                {
                    case ShowRightPanelMessage.PanelId.EditFeature:
                        PageVM.CurrentFeature = m.Parameter as Feature;
                        break;
                    case ShowRightPanelMessage.PanelId.ContentProperties:
                        ContentProperties.SelectedItem = m.Parameter;
                        break;
                }
            });
        }

        public class ShowRightPanelMessage
        {
            public ShowRightPanelMessage(PanelId panel, object? parameter = null)
            {
                Panel = panel;
                Parameter = parameter;
            }

            public PanelId Panel { get; }
            public object? Parameter { get; }
            public enum PanelId
            {
                ContentProperties,
                EditFeature
            }
        }

        public Controls.GeoViewWrapper GeoViewWrapper => geoViewWrapper;

        public ApplicationViewModel AppVM => ApplicationViewModel.Instance;

        public MapPageViewModel PageVM = new MapPageViewModel();


        private CancellationTokenSource? identifyToken;

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // TODO: Rewrite to operate in VM
            geoViewWrapper.GeoViewController.DismissCallout();
            if (e.Location is null) return;
            try
            {
                identifyToken?.Cancel();
                identifyToken = new CancellationTokenSource();
                var result = await geoViewWrapper.IdentifyLayersAsync(e.Position, 2, false, 10, identifyToken.Token);
                identifyToken = null;
                if (result.SelectMany(r=>r.GeoElements).Any())
                {
                    var calloutview = new Controls.IdentifyResultView() { IdentifyResult = result, GeoViewController = PageVM.ViewController };
                    geoViewWrapper.ShowCalloutAt(e.Location, calloutview);
                    calloutview.EditRequested += (s, e) =>
                    {
                        WeakReferenceMessenger.Default.Send(new ShowRightPanelMessage(ShowRightPanelMessage.PanelId.EditFeature, e as Feature));
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
        }


        private async void AddBookmark_Click(object sender, RoutedEventArgs e)
        {
            var vp = geoViewWrapper.GeoViewController.GetCurrentViewpoint(ViewpointType.CenterAndScale);
            if (vp is null)
                return;
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

        private async void AddFromFile_CLick(object sender, RoutedEventArgs e)
        {
            var hwnd = this.XamlRoot?.ContentIslandEnvironment?.AppWindowId.Value ?? 0;
            if (hwnd == 0)
                return; // Can't show dialog without a root window

            var filePicker = new global::Windows.Storage.Pickers.FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, (nint)hwnd);
            var licenseLevel = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.GetLicense().LicenseLevel;
            filePicker.FileTypeFilter.Add(".mmpk");
            filePicker.FileTypeFilter.Add(".mspk");
            if (AppVM.GeoModel is Scene)
            {
                filePicker.FileTypeFilter.Add(".slpk");
            }
            // These datatypes requires at least standard level license for local file access
            if (licenseLevel >= Esri.ArcGISRuntime.LicenseLevel.Standard || licenseLevel == Esri.ArcGISRuntime.LicenseLevel.Developer)
            {
                filePicker.FileTypeFilter.Add(".kml");
                filePicker.FileTypeFilter.Add(".kmz");
                filePicker.FileTypeFilter.Add(".shp");
            }
            filePicker.FileTypeFilter.Add(".tpk");
            filePicker.FileTypeFilter.Add(".tif");
            var file = await filePicker.PickSingleFileAsync();

            if (file is null)
                return;
            try
            {
                await ApplicationViewModel.Instance.AddDataFromFileAsync(file.Path);
            }
            catch(System.Exception ex)
            {
                _ = new ContentDialog()
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Error adding layer",
                    Content = ex.Message,
                    PrimaryButtonText = "OK"
                }.ShowAsync();
            }
           
        }
    }
}

