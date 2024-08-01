using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Storage;
using Esri.ArcGISRuntime.UI;

namespace KmlViewer
{
    public class KmlSample
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public Uri Thumbnail { get; set; }
        public Viewpoint InitialViewpoint { get; set; }
    }

    public class MainPageVM : INotifyPropertyChanged
    {
        public MainPageVM()
        {
            SampleData = new List<KmlSample>();
            SampleData.Add(new KmlSample()
            {
                Title = "Recent Earthquakes",
                Path = "https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/1.0_week_age_link.kml",
                Thumbnail = new Uri("ms-appx:///SampleData/EarthquakesPreview.png"),
                InitialViewpoint = new Viewpoint(new MapPoint(-144.496, 21.68, SpatialReferences.Wgs84), 75555135, new Camera(-144.496, 21.68, 75555135, 21.714728, -144.5060788, 0))
            });
            SampleData.Add(new KmlSample()
            {
                Title = "Live Plane Tracker",
                Path = "https://radar.vlieghinder.nl/",
                Thumbnail = new Uri("ms-appx:///SampleData/RadarPreview.png"),
                InitialViewpoint = new Viewpoint(new MapPoint(52.0867, 3.919440, SpatialReferences.Wgs84), 134430, new Camera(52.0867, 3.919440, 10062, 69, 76.6, 0))
            });
            SampleData.Add(new KmlSample()
            {
                Title = "London Eye",
                Path = "ms-appx:///SampleData/london_eye.kmz",
                Thumbnail = new Uri("ms-appx:///SampleData/LondonEyePreview.png"),
            });
            SampleData.Add(new KmlSample()
            {
                Title = "Flight Maps",
                Path = "ms-appx:///SampleData/Flight_Maps_UK_Ireland.kmz",
                Thumbnail = new Uri("ms-appx:///SampleData/FlightMapsUKPreview.png"),
            });
            var scene = new Scene() { Basemap = CreateDefaultBasemap() };
            scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));
            Scene = scene;
            Map = new Map() { Basemap = CreateDefaultBasemap() };
        }
        private static FadeGroupLayer CreateDefaultBasemap()
        {
            FadeGroupLayer baselayers = new FadeGroupLayer();
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer")) { Name = "Aerial" });
            baselayers.BaseLayers.Add(new OpenStreetMapLayer() { Name = "Open Street Map" });
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer")) { Name = "Topographic" });
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("http://services.arcgisonline.com/arcgis/rest/services/Canvas/World_Dark_Gray_Base/MapServer")) { Name = "Dark Gray Reference" });
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("http://services.arcgisonline.com/arcgis/rest/services/Canvas/World_Light_Gray_Base/MapServer")) { Name = "Light Gray Reference" });
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("http://services.arcgisonline.com/arcgis/rest/services/NatGeo_World_Map/MapServer")) { Name = "National Geographic" });
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("http://services.arcgisonline.com/arcgis/rest/services/USA_Topo_Maps/MapServer")) { Name = "USGS Topographic (US)" });
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Terrain_Base/MapServer")) { Name = "World Terrain" });
            baselayers.BaseLayers.Add(new ArcGISTiledLayer(new Uri("http://services.arcgisonline.com/arcgis/rest/services/Specialty/World_Navigation_Charts/MapServer")) { Name = "World Navigation Charts" });
            return baselayers;
        }

        public List<KmlSample> SampleData { get; private set; }

        private Scene m_Scene;

        public Scene Scene
        {
            get { return m_Scene; }
            set
            {

                m_Scene = value;
                m_Scene.OperationalLayers.CollectionChanged += Layers_CollectionChanged;
                m_Scene.Basemap.PropertyChanged += MainPageVM_PropertyChanged;
                OnPropertyChanged();
                if (Is3D)
                {
                    OnPropertyChanged(nameof(Contents));
                    OnPropertyChanged(nameof(Layers));
                }
            }
        }

        public KmlLayer LoadKmlLayer(string path)
        {
            var kmllayers = Map.OperationalLayers.OfType<KmlLayer>().ToList();
            foreach (var l in kmllayers)
                Map.OperationalLayers.Remove(l);
            kmllayers = Scene.OperationalLayers.OfType<KmlLayer>().ToList();
            foreach (var l in kmllayers)
                Scene.OperationalLayers.Remove(l);
            var source = new Uri(path);
            string name = "";
            if(source.Scheme == "ms-appx")
            {
                //StorageFile.GetFileFromApplicationUriAsync(source)
            }
            if (source.IsFile)
                name = new System.IO.FileInfo(source.LocalPath).Name;
            else
            {
                name = source.Segments.Where(s=>s!="/").LastOrDefault();
                if (name == null)
                    name = source.Host;
            }
            Map.OperationalLayers.Add(new KmlLayer(source) { Name = name });
            Scene.OperationalLayers.Add(new KmlLayer(source) { Name = name });
            return Layers.Last() as KmlLayer;
        }

        private Map m_Map;

        public Map Map
        {
            get { return m_Map; }
            set
            {
                //value.SpatialReference = Esri.ArcGISRuntime.Geometry.SpatialReferences.WebMercator;
                m_Map = value;
                m_Map.OperationalLayers.CollectionChanged += Layers_CollectionChanged;
                m_Map.Basemap.PropertyChanged += MainPageVM_PropertyChanged;
                OnPropertyChanged();
                if (!Is3D)
                {
                    OnPropertyChanged(nameof(Contents));
                    OnPropertyChanged(nameof(Layers));
                }
            }
        }

        public bool Is3D
        {
            get => GetAppSetting(true);
            set
            {
                StoreAppSetting(value);
                OnPropertyChanged(nameof(Contents));
                OnPropertyChanged(nameof(Layers));
                OnPropertyChanged(nameof(Basemap));
                OnPropertyChanged();
            }
        }

        public bool IsShadowsEnabled
        {
            get => GetAppSetting<bool>(false);
            set
            {
                StoreAppSetting(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SunLighting));
            }
        }
        public LightingMode SunLighting => IsShadowsEnabled ? LightingMode.LightAndShadows : LightingMode.NoLight;

        public bool IsAtmosphereEnabled
        {
            get => GetAppSetting<bool>(false);
            set
            {
                StoreAppSetting(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(AtmosphereEffect));
            }
        }

        public AtmosphereEffect AtmosphereEffect => IsAtmosphereEnabled ? AtmosphereEffect.Realistic : AtmosphereEffect.HorizonOnly;

        private void StoreAppSetting(object value, [CallerMemberName] string propertyName = null)
        {
            ApplicationData.Current.LocalSettings.Values[propertyName] = value;
        }

        private T GetAppSetting<T>(T defaultValue, [CallerMemberName] string propertyName = null)
        {
            if(ApplicationData.Current.LocalSettings.Values.ContainsKey(propertyName))
                return (T)ApplicationData.Current.LocalSettings.Values[propertyName];
            return defaultValue;
        }

        private void MainPageVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var layer = (FadeGroupLayer)sender;
            if (Is3D)
            	(Map.Basemap as FadeGroupLayer).SelectedIndex = layer.SelectedIndex;
            else
                (Scene.Basemap as FadeGroupLayer).SelectedIndex = layer.SelectedIndex;
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.OfType<KmlLayer>().Any() ||
                e.OldItems != null && e.OldItems.OfType<KmlLayer>().Any())
                OnPropertyChanged(nameof(Contents));
        }


        public IEnumerable<Esri.ArcGISRuntime.Mapping.KmlLayer> Contents
        {
            get
            {
                if (Layers != null)
                    foreach (var item in Layers.OfType<KmlLayer>())
                        yield return item;
            }
        }
        public FadeGroupLayer Basemap
        {
            get
            {
                if (Is3D)
                {
                    if (Scene != null)
                    {
                        return Scene.Basemap as FadeGroupLayer;
                    }
                }
                else
                {
                    if (Map != null)
                    {
                        return Map.Basemap as FadeGroupLayer;
                    }
                }
                return null;
            }
        }
        public LayerCollection Layers
        {
            get
            {
                if (Is3D)
                {
                    if (Scene != null)
                    {
                        return Scene.OperationalLayers;
                    }
                }
                else
                {
                    if (Map != null)
                    {
                        return Map.OperationalLayers;
                    }
                }
                return null;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}