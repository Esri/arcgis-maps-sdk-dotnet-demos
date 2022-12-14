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
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeoEventServerSample.StreamServices
{
    /// <summary>
    /// Streaming client for ArcGIS GeoEvent Server
    /// </summary>
    public class StreamServiceClient
    {
        private Uri _serviceUri;
        private Dictionary<string, Graphic> _vehicles = new Dictionary<string, Graphic>();
        private Task _disconnectTask;
        private ClientWebSocket _socket;

        private StreamServiceClient(Uri serviceUri)
        {
            _serviceUri = serviceUri;
        }

        /// <summary>
        /// Moves features smoothly to a new location
        /// </summary>
        /// <seealso cref="AnimationSpeed"/>
        public bool AnimateUpdates { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether the socket is currently connected to the service
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Duration for smoothly updated point updates
        /// </summary>
        /// <seealso cref="AnimateUpdates"/>
        public TimeSpan AnimationSpeed { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Gets or sets a value for how old updates should be before being considered stale and should be removed
        /// </summary>

        public TimeSpan FeatureTimeout = TimeSpan.MaxValue;

        /// <summary>
        /// Number of features currently tracked
        /// </summary>
        public int VehicleCount => _vehicles.Count;

        /// <summary>
        /// Gets an estimate of messages generated pr second.
        /// </summary>
        public double MessagesPerSecond { get; private set; }

        /// <summary>
        /// Gets or sets the overlay to render the features in
        /// </summary>
        public GraphicsOverlay Overlay { get; set; }

        /// <summary>
        /// Raised each time a feature is updated
        /// </summary>

        public event EventHandler<string> OnUpdate;

        /// <summary>
        /// Gets the service info
        /// </summary>
        public StreamServerInfo ServiceInfo { get; private set; }

        /// <summary>
        /// Creates a new stream service client
        /// </summary>
        /// <param name="serviceUri"></param>
        /// <returns></returns>
        public static async Task<StreamServiceClient> CreateAsync(Uri serviceUri)
        {
            StreamServiceClient client = new StreamServiceClient(serviceUri);
            await client.LoadInfoAsync().ConfigureAwait(false);
            return client;
        }

        /// <summary>
        /// Starts streaming data
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;
            try
            {
                IsConnected = true;
                string socketUrl = $"{ServiceInfo.StreamUrls[0].Urls[0]}/subscribe";
                if (!string.IsNullOrEmpty(ServiceInfo.StreamUrls[0].Token))
                    socketUrl += "?token=" + ServiceInfo.StreamUrls[0].Token;
                _socket = new ClientWebSocket();
                _socket.Options.SetBuffer(65535, 16);
                await _socket.ConnectAsync(new Uri(socketUrl), CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                IsConnected = false;
                _socket = null;
                throw;
            }
            _disconnectTask = Task.Run(() => ProcessMessages(_socket));
        }

        /// <summary>
        /// Closes the connection to the socket
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            if (_disconnectTask != null)
            {
                await _disconnectTask;
                _disconnectTask = null;
            }
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        private async Task LoadInfoAsync()
        {
            HttpClient client = new HttpClient(new Esri.ArcGISRuntime.Http.ArcGISHttpMessageHandler(), true);
            try
            {
                using (var serviceJson = await client.GetStreamAsync(new Uri(_serviceUri + "?f=json")))
                {
                    ServiceInfo = StreamServerInfo.FromJson(serviceJson);
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private async void ProcessMessages(ClientWebSocket socket)
        {
            string idAttributeField = ServiceInfo.TimeInfo?.TrackIdField ?? ServiceInfo.ObjectIdField;
            var buff = new byte[65535];
            ArraySegment<byte> buffer = new ArraySegment<byte>(buff);
            var sr = ServiceInfo.SpatialReference.AsSpatialReference();
            var fields = new Dictionary<string, FieldInfo>();
            foreach (var f in from p in ServiceInfo.Fields select new KeyValuePair<string, FieldInfo>(p.Name, p))
                fields[f.Key] = f.Value;
            DateTimeOffset lastStaleCheck = DateTimeOffset.Now;
            int messageCount = 0;
            DateTimeOffset messageCountReset = DateTimeOffset.Now;
            while (IsConnected)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                if (!IsConnected) break;
                messageCount++;
                using (var ms = new MemoryStream(buff, 0, result.Count))
                {
                    FeatureMessage p;
                    try
                    {
                        p = FeatureMessage.FromJson(ms);
                    }
                    catch (System.Exception)
                    {
                        Debug.WriteLine("Failed to parse message: " + Encoding.UTF8.GetString(buff, 0, result.Count));
                        continue;
                    }
                    if (p.Geometry == null || p.Attributes == null)
                        continue;
                    
                    foreach (var pair in p.Attributes.ToArray())
                    {
                        var type = pair.Value?.GetType();
                        if (type == typeof(Decimal))
                        {
                            p.Attributes[pair.Key] = Convert.ToDouble(pair.Value);
                        }
                        if (fields.ContainsKey(pair.Key) && fields[pair.Key].Type == "esriFieldTypeDate" && pair.Value != null)
                            p.Attributes[pair.Key] = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(pair.Value));
                    }
                    if (p.Attributes.ContainsKey(idAttributeField))
                    {
                        string id = p.Attributes[idAttributeField].ToString();
                        MapPoint location;
                        if (p.Geometry.Z.HasValue)
                            location = new MapPoint(p.Geometry.X, p.Geometry.Y, p.Geometry.Z.Value, sr);
                        else
                            location = new MapPoint(p.Geometry.X, p.Geometry.Y, sr);
                        if (_vehicles.ContainsKey(id)) // update
                        {
                            var graphic = _vehicles[id];
                            foreach (var att in p.Attributes)
                                graphic.Attributes[att.Key] = att.Value;
                            if (AnimateUpdates && AnimationSpeed.TotalMilliseconds > 16)
                                Animations.GeometryAnimatorHelper.AnimatePointTo(graphic, location, AnimationSpeed, null);
                            else
                                graphic.Geometry = location;
                            OnUpdate?.Invoke(this, "Update");
                        }
                        else
                        {
                            var graphic = new Esri.ArcGISRuntime.UI.Graphic(location, p.Attributes);
                            _vehicles[id] = graphic;
                            if (Overlay != null)
                                Overlay.Graphics.Add(graphic);
                            OnUpdate?.Invoke(this, "Add");
                        }
                    }
                }
                DateTimeOffset now = DateTimeOffset.Now;
                double timeSinceCountReset = (now - messageCountReset).TotalSeconds;
                if (timeSinceCountReset > 10 && messageCount > 10)
                {
                    MessagesPerSecond = messageCount / timeSinceCountReset;
                    OnUpdate?.Invoke(this, "MessagesPerSecond");
                    messageCount = 0;
                    messageCountReset = now;
                }
                //Check if any features has gone stale and remove them
                if (FeatureTimeout < TimeSpan.MaxValue)
                {
                    DateTimeOffset timeoutDate = now - FeatureTimeout;
                    if (DateTimeOffset.Now - lastStaleCheck > TimeSpan.FromSeconds(10))
                    {
                        var timeField = ServiceInfo.TimeInfo?.StartTimeField;
                        if (!string.IsNullOrEmpty(timeField))
                        {
                            var outdated = (from a in _vehicles where a.Value.Attributes.ContainsKey(timeField) && ((DateTimeOffset)a.Value.Attributes[timeField]) < timeoutDate select a.Key).ToArray();
                            foreach (var key in outdated)
                            {
                                var graphic = _vehicles[key];
                                _vehicles.Remove(key);
                                if (Overlay != null)
                                    Overlay.Graphics.Remove(graphic);
                            }
                            if (outdated.Length > 0)
                                OnUpdate?.Invoke(this, "Remove");
                        }
                        lastStaleCheck = DateTimeOffset.Now;
                    }
                }
            }
            _vehicles.Clear();
            Overlay.Graphics.Clear();
        }
    }
}
