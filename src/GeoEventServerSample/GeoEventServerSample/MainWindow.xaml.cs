// /*******************************************************************************
//  * Copyright 2012-2018 Esri
//  *
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************/

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using GeoEventServerSample.StreamServices;
using System;
using System.Linq;
using System.Windows;

namespace GeoEventServerSample
{
    public partial class MainWindow : Window
    {
        private const string StreamServiceUri = "https://geoeventsample1.esri.com:6443/arcgis/rest/services/LABus/StreamServer";

        public MainWindow()
        {
            InitializeComponent();
            mapView.Map = new Map(Basemap.CreateLightGrayCanvasVector())
            {
                InitialViewpoint = new Viewpoint(new Envelope(-13234206.5948101, 3983021, -13093018, 4075286, SpatialReferences.WebMercator))
            };
            LoadService(StreamServiceUri);
        }

        private async void LoadService(string uri)
        {
            var client = await StreamServiceClient.CreateAsync(new Uri(uri));
            client.FeatureTimeout = TimeSpan.FromMinutes(5); // Removes any features that hasn't reported back in over 5 minutes
            client.OnUpdate += Client_OnUpdate;

            // Create overlay for rendering updates
            var si = typeof(LocationDisplay).Assembly.GetManifestResourceStream("Esri.ArcGISRuntime.Esri.ArcGISRuntime.LocationDisplayCourse.scale-200.png");
            var ri = await RuntimeImage.FromStreamAsync(si);
            PictureMarkerSymbol vehicleSymbol = new PictureMarkerSymbol(ri) { Width = 25, Height = 25 };
            var overlay = new Esri.ArcGISRuntime.UI.GraphicsOverlay()
            {
                Renderer = new SimpleRenderer(vehicleSymbol),
                SceneProperties = new LayerSceneProperties(SurfacePlacement.Absolute) //In case we use it in 3D and have Z values
            };
            var headingField = client.ServiceInfo.Fields.Where(f => f.Name.ToLower() == "heading").Select(f => f.Name).FirstOrDefault();
            if (!string.IsNullOrEmpty(headingField))
            {
                overlay.Renderer.RotationExpression = $"[{headingField}]";
                overlay.Renderer.SceneProperties.HeadingExpression = $"[{headingField}]";
            }
            mapView.GraphicsOverlays.Add(overlay);
            client.Overlay = overlay;

            // Connect
            await client.ConnectAsync();
        }

        private void Client_OnUpdate(object sender, string e)
        {
            if(e == "Add" || e == "Remove")
            {
                Dispatcher.Invoke(() => StatusText.Text = "Vehicle count: " + (sender as StreamServiceClient).VehicleCount.ToString());
            }
        }

        private async void mapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            var r = await mapView.IdentifyGraphicsOverlayAsync(mapView.GraphicsOverlays[0], e.Position, 1, false);
            r.GraphicsOverlay.ClearSelection();
            var g = r.Graphics?.FirstOrDefault();
            attributeView.ItemsSource = g?.Attributes;
            if(g != null)
                g.IsSelected = true;
        }
    }
}
