using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace HydrographicsSample
{
    /// <summary>
    /// Main Window
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Set up the "SENC" folder one folder up as the ENC cache. If not set, will use the application's temp folder
            var sencDir = @"..\SENC\";
            if(!Directory.Exists(sencDir))
                Directory.CreateDirectory(sencDir);
            EncEnvironmentSettings.Default.SencDataPath = sencDir;
            InitializeComponent();
            LoadingProgressPanel.Visibility = Visibility.Collapsed;
            Load();

            LoadSampleData();
        }

        protected override void OnClosed(EventArgs e)
        {
            SettingsSaver.SaveSettings(EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings, "EncMarinerSettings.config");
            SettingsSaver.SaveSettings(EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings, "EncTextGroupVisibilitySettings.config");
            SettingsSaver.SaveSettings(EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings, "ViewingGroupSettings.config");
            base.OnClosed(e);
        }

        private async void LoadSampleData()
        {
            foreach (var file in new DirectoryInfo("SampleData").GetFiles("*.000"))
            {
                LoadCell(file.FullName);
            }
            await Task.WhenAll(mapView.Map.OperationalLayers.Select(l => l.LoadAsync()));
            ZoomToAllLayers();
        }

        private void Load()
        {
            // Change some of the defaults
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.TwoDepthShades = false;
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.ShallowContour = 2;
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.SafetyContour = 5;
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.DeepContour = 8;

            SettingsSaver.LoadSettings(EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings, "EncMarinerSettings.config");
            SettingsSaver.LoadSettings(EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings, "EncTextGroupVisibilitySettings.config");
            SettingsSaver.LoadSettings(EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings, "ViewingGroupSettings.config");


            // Bind the view properties to the property grid controls.
            DisplayPropertiesPanel.Instance = EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings;
            S52TextGroupPropertiesPanel.Instance = EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings;
            S52ViewGroupPropertiesPanel.Instance = EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings;


            // Create an overlay for display identify results
            mapView.GraphicsOverlays.Add(new GraphicsOverlay() { Id = "IdentifyResults" });
        }

        private void mapView_MouseMove(object sender, MouseEventArgs e)
        {
            // Show the current location of the mouse cursor
            var loc = mapView.ScreenToLocation(e.GetPosition(mapView));
            if(loc != null) // This is null until a map is loaded
            {
                var p = loc; 
                if(p != null)
                {
                    status.Text = CoordinateFormatter.ToLatitudeLongitude(p, LatitudeLongitudeFormat.DegreesMinutesSeconds, 0);
                }
            }
        }

        private void OpenCell_MenuItemClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog() { Filter = "ENC Cells (*.000)|*.000", Multiselect = true };
            if(dlg.ShowDialog() == true)
            {
                foreach(var filename in dlg.FileNames)
                    LoadCell(filename);
            }
        }

        private void OpenExchangeSet_MenuItemClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog() { Filter = "ENC Exchange Set(CATALOG.*)|CATALOG.*|All files (*.*)|*.*", Multiselect = false };
            if (dlg.ShowDialog() == true)
            {
                var dlg2 = new OpenExchangeSetWindow(dlg.FileName);
                if (dlg2.ShowDialog() == true)
                {
                    foreach (var dataset in dlg2.SelectedDatasets.OrderBy(d=>d.Name))
                    {
                        try
                        {
                            var cell = new EncCell(dataset);
                            var layer = new EncLayer(cell) { Name = dataset.Name };
                            if (mapView.Map == null)
                            {
                                CreateMap(null);
                            }
                            var idx = dataset.Name[2];
                            int insertIndex = 0;
                            foreach (var l in mapView.Map.OperationalLayers)
                            {
                                var name = l.Name[2];
                                if(name > idx)
                                {
                                    break;
                                }
                                insertIndex++;
                            }
                            mapView.Map.OperationalLayers.Insert(insertIndex, layer);
                            LoadingProgressPanel.Visibility = Visibility.Visible;
                            progress.Maximum = mapView.Map.OperationalLayers.Count;
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                }
            }
        }

        private void LoadCell(string filename)
        {
            try
            {
                var cell = new EncCell(filename);
                var layer = new EncLayer(cell) { Name = new FileInfo(filename).Name };
                if (mapView.Map == null)
                {
                    CreateMap(null);
                }
                
                LoadingProgressPanel.Visibility = Visibility.Visible;

                var idx = layer.Name[2];
                int insertIndex = 0;
                foreach (var l in mapView.Map.OperationalLayers)
                {
                    var name = l.Name[2];
                    if (name > idx)
                    {
                        break;
                    }
                    insertIndex++;
                }
                mapView.Map.OperationalLayers.Insert(insertIndex, layer);
                progress.Maximum = mapView.Map.OperationalLayers.Count;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CreateMap(Viewpoint vp)
        {
            Map map = new Map(SpatialReferences.WebMercator); // Use WebMercator. If we don't specify a spatial reference, it'll default to the SR of the first ENC Layer - typically GCS_WGS84
            map.Basemap = new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=979c6cc89af9449cbeb5342a439c6a76")); // Lightgray background map, just for reference in this demo. Not necessarily needed
            map.InitialViewpoint = vp;
            mapView.Map = map;
        }

        private void ClearMap_MenuItemClick(object sender, RoutedEventArgs e)
        {
            mapView.Map = null;
        }

        private void mapView_LayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            // Update the progress bar as the layers load
            if(e.LayerViewState.Status == LayerViewStatus.Active)
            {
                Dispatcher.Invoke(() =>
                {
                    progress.Value = mapView.Map.OperationalLayers.Where(l => l.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded || l.LoadStatus == Esri.ArcGISRuntime.LoadStatus.FailedToLoad).Count();
                    progressText.Text = $"{progress.Value} / {mapView.Map.OperationalLayers.Count}";
                    if (progress.Value == mapView.Map.OperationalLayers.Count)
                    {
                        if (LoadingProgressPanel.Visibility == Visibility.Visible)
                        {
                            LoadingProgressPanel.Visibility = Visibility.Collapsed;
                            ZoomToAllLayers();
                        }
                    }
                });
            }
        }

        private void Zoom_MenuItemClick(object sender, RoutedEventArgs e)
        {
            ZoomToAllLayers();
        }

        private void ZoomToAllLayers()
        { 
            if (mapView.Map == null) return;

            // Calculate the full extent of all layers, then zoom to it.
            Envelope extent = null;
            foreach(var layer in mapView.Map?.OperationalLayers.OfType<EncLayer>().Where(l=>l.FullExtent != null))
            {
                if (extent == null)
                    extent = layer.FullExtent;
                else
                    extent = GeometryEngine.Union(extent, layer.FullExtent).Extent;
            }

            if(extent != null)
            {
                mapView.SetViewpointGeometryAsync(extent);
            }
        }

        private async void mapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Identify the map when the view is tapped
            var go = mapView.GraphicsOverlays["IdentifyResults"];
            go.Graphics.Clear();
            var results = await mapView.IdentifyLayersAsync(e.Position, 5, false);
            sidePanel.Show(results, go);
        }

        private void sidePanel_Opened(object sender, EventArgs e)
        {
            // Change the right inserts when the side-panel is visible and overlaid on top of the mapview
            mapView.ViewInsets = new Thickness(0, 0, sidePanel.ActualWidth + sidePanel.Margin.Right, 0);
        }

        private void sidePanel_Closed(object sender, EventArgs e)
        {
            // Change the right inserts when the side-panel is collapsed and no longer overlaid on top of the mapview
            mapView.ViewInsets = new Thickness(0);
        }

        private void sidePanel_ZoomButtonClicked(object sender, Esri.ArcGISRuntime.Data.GeoElement e)
        {
            // When a zoom button in the identify panel is clicked, zoom/pan to the associated feature
            if (e.Geometry is MapPoint)
                mapView.SetViewpointCenterAsync(e.Geometry as MapPoint);
            else
                mapView.SetViewpointGeometryAsync(e.Geometry, 50);
        }

        private void LoadingPanelCloseButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingProgressPanel.Visibility = Visibility.Collapsed;
        }

        private void ZoomToLayer_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as EncLayer;
            if (layer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                if (layer.FullExtent == null)
                    MessageBox.Show("Cell has no extent to zoom to");
                else
                    mapView.SetViewpointGeometryAsync(layer.FullExtent);
            }
            else if(layer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.FailedToLoad)
            {
                MessageBox.Show(layer.LoadError?.Message, "Layer Load Error");
            }
            else
            {
                MessageBox.Show("Layer not yet loaded");
            }
        }

        private void RemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as EncLayer;
            mapView.Map.OperationalLayers.Remove(layer);
        }

        private void BasemapSelector_Checked(object sender, RoutedEventArgs e)
        {
            if (basemapSelector_LightGray == null || basemapSelector_Imagery == null || mapView.Map == null)
                return;
            basemapSelector_LightGray.IsChecked = (sender == basemapSelector_LightGray);
            basemapSelector_Imagery.IsChecked = (sender == basemapSelector_Imagery);
            if (basemapSelector_LightGray.IsChecked)
                mapView.Map.Basemap = new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=979c6cc89af9449cbeb5342a439c6a76"));
            else if (basemapSelector_Imagery.IsChecked)
                mapView.Map.Basemap = new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=39858979a6ba4cfd96005bbe9bd4cf82"));
        }
    }
}
