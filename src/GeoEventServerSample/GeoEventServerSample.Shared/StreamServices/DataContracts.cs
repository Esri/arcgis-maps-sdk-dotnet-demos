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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace GeoEventServerSample.StreamServices
{
    [DataContract]
    public class FeatureMessage
    {
        private static DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(FeatureMessage), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
#if __IOS__
        public static FeatureMessage FromJson(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<FeatureMessage>(json);
        }
#else
        public static FeatureMessage FromJson(Stream stream)
        {
            return s.ReadObject(stream) as FeatureMessage;
        }
#endif

        [DataMember(Name = "geometry")]
        public JsonGeometry Geometry { get; private set; }

        [DataMember(Name = "attributes")]
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        [DataMember(Name = "spatialReference")]
        public SpatialReferenceInfo SpatialReference { get; private set; }
    }

    [DataContract]
    public class JsonGeometry
    {
        [DataMember(Name = "type")]
        public string Type { get; private set; }

        [DataMember(Name = "x")]
        public double X { get; private set; }

        [DataMember(Name = "y")]
        public double Y { get; private set; }

        [DataMember(Name = "z")]
        public double? Z { get; private set; }
    }

    [DataContract]
    public class SpatialReferenceInfo
    {
        [DataMember(Name = "wkid")]
        public int Wkid { get; private set; }

        public Esri.ArcGISRuntime.Geometry.SpatialReference AsSpatialReference()
        {
            return Esri.ArcGISRuntime.Geometry.SpatialReference.Create(Wkid);
        }
    }

    [DataContract]
    public class StreamServerInfo
    {
        private static DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(StreamServerInfo), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
        public static StreamServerInfo FromJson(Stream stream)
        {
            return s.ReadObject(stream) as StreamServerInfo;
        }

        [DataMember(Name = "objectIdField")]
        public string ObjectIdField { get; private set; }

        [DataMember(Name = "displayField")]
        public string DisplayField { get; private set; }

        [DataMember(Name = "timeInfo")]
        public TimeInfo TimeInfo { get; private set; }

        [DataMember(Name ="GeoemtryType")]
        public string GeometryType { get; private set; }

        [DataMember(Name = "spatialReference")]
        public SpatialReferenceInfo SpatialReference { get; private set; }

        [DataMember(Name = "fields")]
        public FieldInfo[] Fields { get; private set; }

        [DataMember(Name = "streamUrls")]
        public StreamUrlInfo[] StreamUrls { get; private set; }

        [DataMember(Name = "currentVersion")]
        public string CurrentVersion { get; private set; }

        [DataMember(Name = "capabilities")]
        public string Capabilities { get; private set; }
    }

    [DataContract]
    public class StreamUrlInfo
    {
        [DataMember(Name = "transport")]
        public string Transport { get; private set; }

        [DataMember(Name = "token")]
        public string Token { get; private set; }

        [DataMember(Name = "urls")]
        public string[] Urls { get; private set; }
    }

    [DataContract]
    public class TimeInfo
    {
        [DataMember(Name = "trackIdField")]
        public string TrackIdField { get; private set; }

        [DataMember(Name = "startTimeField")]
        public string StartTimeField { get; private set; }

        [DataMember(Name = "endTimeField")]
        public string EndTimeField { get; private set; }
    }

    [DataContract]
    public class FieldInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; private set; }

        [DataMember(Name = "type")]
        public string Type { get; private set; }
    }
}
