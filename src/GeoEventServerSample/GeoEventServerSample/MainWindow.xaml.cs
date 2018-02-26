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
            DataContext = VM = new MapViewModel();
        }

        public MapViewModel VM { get; }
        
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
