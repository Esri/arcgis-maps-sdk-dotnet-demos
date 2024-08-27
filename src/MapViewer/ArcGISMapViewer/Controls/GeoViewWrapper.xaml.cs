using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Data;

namespace ArcGISMapViewer.Controls
{
    /// <summary>
    /// Handles automatic switching between 2D and 3D
    /// </summary>
    public sealed partial class GeoViewWrapper : UserControl
    {
        public GeoViewWrapper()
        {
            this.InitializeComponent();
        }

        private void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            GeoViewTapped?.Invoke(sender, e);
        }

        public GeoView? GeoView
        {
            get { return (GeoView?)GetValue(GeoViewProperty); }
            private set { SetValue(GeoViewProperty, value); }
        }

        public static readonly DependencyProperty GeoViewProperty =
            DependencyProperty.Register("GeoView", typeof(GeoView), typeof(GeoViewWrapper), new PropertyMetadata(null));

        public MapView MapView => mapView;

        public GeoModel GeoModel
        {
            get { return (GeoModel)GetValue(GeoModelProperty); }
            set { SetValue(GeoModelProperty, value); }
        }

        public static readonly DependencyProperty GeoModelProperty =
            DependencyProperty.Register("GeoModel", typeof(GeoModel), typeof(GeoViewWrapper), new PropertyMetadata(null, (s,e)=>((GeoViewWrapper)s).OnGeoModelPropertyChanged(e.OldValue as GeoModel, e.NewValue as GeoModel)));

        private void OnGeoModelPropertyChanged(GeoModel? oldModel, GeoModel? newModel)
        {
            sceneView.Scene = null;
            mapView.Map = null;
            sceneView.Visibility = Visibility.Collapsed;
            mapView.Visibility = Visibility.Collapsed;
            if (newModel is Map map)
            {
                GeoView = mapView;
                mapView.Visibility = Visibility.Visible;
                mapView.Map = map;
            }
            else if (newModel is Scene scene)
            {
                GeoView = sceneView;
                sceneView.Visibility = Visibility.Visible;
                sceneView.Scene = scene;
            }
            else
                GeoView = null;

        }

        internal Task<IReadOnlyList<IdentifyLayerResult>> IdentifyLayersAsync(Point screenPoint, double tolerance, bool returnPopupsOnly, long maximumResultsPerLayer)
        {
            return (GeoView ?? mapView).IdentifyLayersAsync(screenPoint, tolerance, returnPopupsOnly, maximumResultsPerLayer);
        }

        internal void ShowCalloutAt(MapPoint location, IdentifyResultView calloutview)
        {
            (GeoView ?? mapView).ShowCalloutAt(location, calloutview);
        }

        public Esri.ArcGISRuntime.Toolkit.UI.GeoViewController GeoViewController
        {
            get { return (Esri.ArcGISRuntime.Toolkit.UI.GeoViewController)GetValue(GeoViewControllerProperty); }
            set { SetValue(GeoViewControllerProperty, value); }
        }

        public static readonly DependencyProperty GeoViewControllerProperty =
            DependencyProperty.Register("GeoViewController", typeof(Esri.ArcGISRuntime.Toolkit.UI.GeoViewController), typeof(GeoViewWrapper), new PropertyMetadata(null));

        public event EventHandler<GeoViewInputEventArgs>? GeoViewTapped;
    }
}
