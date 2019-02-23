using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace KmlViewer
{
    [Windows.UI.Xaml.Data.Bindable]
    public class KmlSample
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public Uri Thumbnail { get; set; }
        public Viewpoint InitialViewpoint { get; set; }
    }

    [Windows.UI.Xaml.Data.Bindable]
    public class MainPageVM : INotifyPropertyChanged
    {
        public MainPageVM()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                m_Scene = new Scene();
                m_Scene.OperationalLayers.Add(new KmlLayer(new Uri("http://server.com/data1.kml")));
                m_Scene.OperationalLayers.Add(new KmlLayer(new Uri("http://server.com/data2.kml")));
            }
            else
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("Is3D"))
                    Is3D = (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values["Is3D"];
            }

            SampleData = new List<KmlSample>();
            SampleData.Add(new KmlSample()
            {
                Title = "Recent Earthquakes",
                Path = "http://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/1.0_week_age_link.kml",
                Thumbnail = new Uri("ms-appx:///SampleData/EarthquakesPreview.png"),
                InitialViewpoint = new Viewpoint(new MapPoint(-144.496, 21.68, SpatialReferences.Wgs84), 75555135, new Camera(-144.496, 21.68, 75555135, 21.714728, -144.5060788, 0))
            });
            SampleData.Add(new KmlSample()
            {
                Title = "Live Plane Tracker",
                Path = "http://radar.vlieghinder.nl/",
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
                //m_Scene.OperationalLayers.OfType<FadeGroupLayer>().First().PropertyChanged += MainPageVM_PropertyChanged;
                OnPropertyChanged();
                if (Is3D)
                {
                    OnPropertyChanged("Contents");
                    OnPropertyChanged("Layers");
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
            Map.OperationalLayers.Add(new KmlLayer(new Uri(path)));
            Scene.OperationalLayers.Add(new KmlLayer(new Uri(path)));
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
                //m_Map.OperationalLayers.OfType<FadeGroupLayer>().First().PropertyChanged += MainPageVM_PropertyChanged;
                OnPropertyChanged();
                if (!Is3D)
                {
                    OnPropertyChanged("Contents");
                    OnPropertyChanged("Layers");
                }
            }
        }

        private bool m_Is3D = true;

        public bool Is3D
        {
            get { return m_Is3D; }
            set
            {
                m_Is3D = value;
                OnPropertyChanged(nameof(Contents));
                OnPropertyChanged(nameof(Layers));
                OnPropertyChanged();
                StoreAppSetting(value);
            }
        }

        private bool m_IsShadowsEnabled;
        public bool IsShadowsEnabled
        {
            get => m_IsShadowsEnabled;
            set
            {
                m_IsShadowsEnabled = value;
                OnPropertyChanged();
            }
        }

        private void StoreAppSetting(object value, [CallerMemberName] string propertyName = null)
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[propertyName] = value;
        }

        private void MainPageVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO
            //var layer = (FadeGroupLayer)sender;
            //if (Is3D)
            //	Map.OperationalLayers.OfType<FadeGroupLayer>().First().SelectedIndex = layer.SelectedIndex;
            //else
            //	Scene.OperationalLayers.OfType<FadeGroupLayer>().First().SelectedIndex = layer.SelectedIndex;
            //if(e.PropertyName == "SelectedLayer")
            //{
            //	OnPropertyChanged("CopyrightText");
            //}
        }

        public string CopyrightText
        {
            get
            {
                //if (Layers != null)
                //{
                //	var layer = Layers.OfType<FadeGroupLayer>().First().SelectedLayer;
                //	if (layer is ICopyright)
                //	{
                //		return (layer as ICopyright).CopyrightText;
                //	}
                //}
                return "";

            }
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.OfType<KmlLayer>().Any() ||
                e.OldItems != null && e.OldItems.OfType<KmlLayer>().Any())
                OnPropertyChanged("Contents");
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