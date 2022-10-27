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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace GeoEventServerSample
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private const string StreamServiceUri = "https://geoeventsample1.esri.com:6443/arcgis/rest/services/LABus/StreamServer";

        public MapViewModel()
        {
            Map = new Map(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=979c6cc89af9449cbeb5342a439c6a76")))
            {
                InitialViewpoint = new Viewpoint(new Envelope(-13234206.5948101, 3983021, -13093018, 4075286, SpatialReferences.WebMercator))
            };
            Overlays = new Esri.ArcGISRuntime.UI.GraphicsOverlayCollection();
            LoadService(StreamServiceUri);
        }
        public Map Map { get; }

        public Esri.ArcGISRuntime.UI.GraphicsOverlayCollection Overlays { get; }

        private void Client_OnUpdate(object sender, string e)
        {
            if (e == "Add" || e == "Remove")
            {
                VehicleCount = (sender as StreamServiceClient).VehicleCount;
                RunOnUIThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VehicleCount))));
            }
            else if(e == "MessagesPerSecond")
            {
                StreamInfo = $"{(sender as StreamServiceClient).MessagesPerSecond.ToString("0.0")} messages/second";
                RunOnUIThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StreamInfo))));
            }
        }

        private void RunOnUIThread(Action action)
        {
#if MauiWindows || __IOS__ || __Android__
            MainThread.BeginInvokeOnMainThread(action);
#elif NETFX_CORE
            if (Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                action();
            else
            {
                var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => action());
            }
#elif WPF
            var dispatcher = System.Windows.Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                return; // Occurs during shutdown
            if (dispatcher.CheckAccess())
                action();
            else
            {
                dispatcher.Invoke(action);
            }
#endif
        }

        public int VehicleCount { get; private set; }
        
        public string StreamInfo { get; private set; }
        
        private async void LoadService(string uri)
        {
            StreamInfo = "Initializing stream...";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StreamInfo)));
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
            Overlays.Add(overlay);
            client.Overlay = overlay;

            // Connect
            await client.ConnectAsync();
            StreamInfo = "Connected";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StreamInfo)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
