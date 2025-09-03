using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ArcGISMapViewer.Controls
{
    /// <summary>
    /// Creates a selectable list of all content in the map/scene
    /// </summary>
    public sealed partial class MapContentsView : Page
    {
        public MapContentsView()
        {
            this.InitializeComponent();
        }

        private void TablesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedItem = e.AddedItems.FirstOrDefault();
                OperationalLayersTreeView.SelectedItem = null;
                ReferenceLayersTreeView.SelectedItem = null;
                BaseLayersTreeView.SelectedItem = null;
            }
        }

        private void TreeViewSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0)
            {
                SelectedItem = args.AddedItems.FirstOrDefault();
                TablesListView.SelectedItem = null;

                if (sender != OperationalLayersTreeView) OperationalLayersTreeView.SelectedItem = null;
                if (sender != ReferenceLayersTreeView) ReferenceLayersTreeView.SelectedItem = null;
                if (sender != BaseLayersTreeView) BaseLayersTreeView.SelectedItem = null;
            }
        }

        public GeoModel GeoModel
        {
            get { return (GeoModel)GetValue(GeoModelProperty); }
            set { SetValue(GeoModelProperty, value); }
        }

        public static readonly DependencyProperty GeoModelProperty =
            DependencyProperty.Register(nameof(GeoModel), typeof(GeoModel), typeof(MapContentsView), new PropertyMetadata(null, (s, e) => ((MapContentsView)s).OnGeoModelPropertyChanged(e.NewValue as GeoModel)));

        private async void OnGeoModelPropertyChanged(GeoModel? newValue)
        {
            if(newValue != null && newValue.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                try
                {
                    await newValue.LoadAsync();
                }
                catch { }
            }
            if (SelectedItem is not null)
                OnSelectedItemPropertyChanged(SelectedItem);
            else
            {
                if (newValue?.OperationalLayers.LastOrDefault() is Layer layer)
                    SelectedItem = layer;
                else if (newValue?.Tables.FirstOrDefault() is FeatureTable table)
                    SelectedItem = table;
                else if (newValue?.Basemap?.ReferenceLayers.FirstOrDefault() is Layer reflayer)
                    SelectedItem = reflayer;
                else if (newValue?.Basemap?.BaseLayers.FirstOrDefault() is Layer baselayer)
                    SelectedItem = baselayer;
                else
                    SelectedItem = null;
            }
        }

        public object? SelectedItem
        {
            get { return (object?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(MapContentsView), new PropertyMetadata(null, (s, e) => ((MapContentsView)s).OnSelectedItemPropertyChanged(e.NewValue)));

        private void OnSelectedItemPropertyChanged(object? newValue)
        {
            SelectedItemChanged?.Invoke(this, newValue);
            if (GeoModel is null)
                return;
            if (newValue is Layer layer)
            {
                if (GeoModel.OperationalLayers.Contains(layer))
                {
                    OperationalLayersTreeView.SelectedItem = layer;
                }
                else if (GeoModel.Basemap?.ReferenceLayers.Contains(layer) == true)
                {
                    ReferenceLayersTreeView.SelectedItem = layer;
                }
                else if (GeoModel.Basemap?.BaseLayers.Contains(layer) == true)
                {
                    BaseLayersTreeView.SelectedItem = layer;
                }
                SelectedItemText = layer.Name;
            }
            else if (newValue is FeatureTable table)
            {
                TablesListView.SelectedItem = table;
                SelectedItemText = table.DisplayName;
            }
            else if(newValue is ILayerContent ilayer)
            {
                SelectedItemText = ilayer.Name;
            }
            else
            {
                SelectedItemText = string.Empty;
            }
        }



        public string SelectedItemText
        {
            get { return (string)GetValue(SelectedItemTextProperty); }
            private set { SetValue(SelectedItemTextProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemTextProperty =
            DependencyProperty.Register(nameof(SelectedItemText), typeof(string), typeof(MapContentsView), new PropertyMetadata(string.Empty));


        public event EventHandler<object?>? SelectedItemChanged;

    }
}
