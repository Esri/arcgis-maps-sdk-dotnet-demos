using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Messaging;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Toolkit.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Controls
{
    public sealed partial class LayerListView : UserControl
    {
        public LayerListView()
        {
            this.InitializeComponent();
        }

        public GeoModel? GeoModel
        {
            get { return (GeoModel?)GetValue(GeoModelProperty); }
            set { SetValue(GeoModelProperty, value); }
        }

        public static readonly DependencyProperty GeoModelProperty =
            DependencyProperty.Register("GeoModel", typeof(GeoModel), typeof(LayerListView), new PropertyMetadata(null));

        public GeoViewController GeoViewController
        {
            get { return (GeoViewController)GetValue(GeoViewControllerProperty); }
            set { SetValue(GeoViewControllerProperty, value); }
        }

        public static readonly DependencyProperty GeoViewControllerProperty =
            DependencyProperty.Register("GeoViewController", typeof(GeoViewController), typeof(LayerListView), new PropertyMetadata(null));

        private void ZoomToItem_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement)?.DataContext as Layer;
            if (layer?.FullExtent is not null)
                GeoViewController.SetViewpointAsync(new Viewpoint(layer.FullExtent));
        }

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement)?.DataContext as Layer;
            if (layer is not null)
            {
                TextBox tb = new TextBox() { Text = layer.Name };
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Rename Layer",
                    Content = tb,
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    layer.Name = tb.Text;
                }
            }
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement)?.DataContext as Layer;
            if (layer is not null)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Remove layer?",
                    PrimaryButtonText = "OK",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (GeoModel?.OperationalLayers.Contains(layer) == true)
                        GeoModel.OperationalLayers.Remove(layer);
                }
            }
        }

        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement)?.DataContext as Layer;
            if (layer is not null)
            {
                WeakReferenceMessenger.Default.Send(new Views.MapPage.ShowRightPanelMessage(Views.MapPage.ShowRightPanelMessage.PanelId.ContentProperties, layer));
            }
        }

        private void ShowTable_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement)?.DataContext as FeatureLayer;
            if (layer?.FeatureTable is not null)
            {
                WeakReferenceMessenger.Default.Send(new Controls.TableView.ShowFeatureTable(layer.FeatureTable));
            }
        }

        public static IconSource LayerToIconSource(Layer layer)
        {
            var icon = layer switch
            {
                FeatureLayer => Esri.Calcite.WinUI.CalciteIcon.FeatureLayer,
                RasterLayer => Esri.Calcite.WinUI.CalciteIcon.Image,
                ArcGISSceneLayer => Esri.Calcite.WinUI.CalciteIcon.Layer3D,
                BuildingSceneLayer => Esri.Calcite.WinUI.CalciteIcon._3DBuilding,
                ArcGISMapImageLayer => Esri.Calcite.WinUI.CalciteIcon.LayerMapService,
                ArcGISVectorTiledLayer => Esri.Calcite.WinUI.CalciteIcon.LayerService,
                KmlLayer => Esri.Calcite.WinUI.CalciteIcon.LayerKml,
                WmsLayer => Esri.Calcite.WinUI.CalciteIcon.LayerService,
                WmtsLayer => Esri.Calcite.WinUI.CalciteIcon.TileLayer,
                ImageTiledLayer => Esri.Calcite.WinUI.CalciteIcon.TileLayer,
                GroupLayer => Esri.Calcite.WinUI.CalciteIcon.GroupLayers,
                PointCloudLayer => Esri.Calcite.WinUI.CalciteIcon.LayerPoints,
                _ => Esri.Calcite.WinUI.CalciteIcon.Layer,
            };
            if (layer.GetType().Name == "UnknownLayer")
                icon = Esri.Calcite.WinUI.CalciteIcon.Question;
            return new Esri.Calcite.WinUI.CalciteFontIconSource() { Icon = icon, Scale = Esri.Calcite.WinUI.CalciteIconScale.Medium };
        }
        public static string LayerToDescription(Layer layer)
        {
            var desc = layer.Item?.Title ?? layer.Name;
            if(!string.IsNullOrEmpty(layer.Item?.Description))
            {
                desc += Environment.NewLine + layer.Item.Description;
            }
            
            var layerType = layer switch
            {
                FeatureLayer => "Feature Layer",
                RasterLayer => "Raster Layer",
                ArcGISSceneLayer => "Scene Layer",
                ArcGISMapImageLayer => "ArcGIS Map Service",
                ArcGISVectorTiledLayer => "Vector Tiled Layer",
                KmlLayer => "KML",
                WmsLayer => "WMS",
                WmtsLayer => "WMTS",
                ImageTiledLayer => "Tile Layer",
                GroupLayer => "Group Layer",
                PointCloudLayer => "Point Cloud",
                BuildingSceneLayer => "Building Scene Layer",
                _ => null,
            };
            if (layer.GetType().Name == "UnknownLayer")
                layerType = "Unsupported Layer Type";
            if (!string.IsNullOrEmpty(layerType))
            {
                desc += Environment.NewLine + layerType;
            }

            return desc;
        }
    }
}
