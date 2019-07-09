using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using System.Collections.ObjectModel;

namespace WpfClusterer
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MapViewModel : INotifyPropertyChanged
    {
        static Random randomizer = new Random();

        public MapViewModel()
        {
            Clusterer.Graphics = GenerateRandomPoints(1000000);
            GraphicsOverlayCollection.Add(Clusterer.GraphicsOverlay);
        }

        private static List<Graphic> GenerateRandomPoints(uint count)
        {
            List<Graphic> collection = new List<Graphic>();
            var clusterCenter = new MapPoint(randomizer.NextDouble() * 40000000 - 20000000, randomizer.NextDouble() * 40000000 - 20000000, SpatialReferences.WebMercator);
            double clusterRadius = randomizer.NextDouble() * 100000;
            for (uint i = 0; i < count; i++)
            {
                MapPoint p = new MapPoint(randomizer.NextDouble() * clusterRadius + clusterCenter.X * (randomizer.Next(1) == 0 ? -1 : 1),
                    randomizer.NextDouble() * clusterRadius + clusterCenter.Y * (randomizer.Next(1) == 0 ? -1 : 1),
                    SpatialReferences.WebMercator);
                collection.Add(new Graphic(p));
                if (randomizer.Next(((int)count) / 10000) == 0) //switch cluster center
                {
                    clusterCenter = new MapPoint(randomizer.NextDouble() * 40000000 - 20000000, randomizer.NextDouble() * 40000000 - 20000000, SpatialReferences.WebMercator);
                    clusterRadius = randomizer.NextDouble() * 100000;
                }
            }
            return collection;
        }

        private GraphicsOverlayCollection _graphicsOverlayCollection = new GraphicsOverlayCollection();

        /// <summary>
        /// Gets or sets the GraphicsOverlayCollection
        /// </summary>
        public GraphicsOverlayCollection GraphicsOverlayCollection
        {
            get { return _graphicsOverlayCollection; }
            set { _graphicsOverlayCollection = value; OnPropertyChanged(); }
        }

        private Map _map = new Map(Basemap.CreateStreets());

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
        }

        PointClusterer _clusterer = new PointClusterer();
        public PointClusterer Clusterer
        {
            get => _clusterer;
            private set
            {
                if (_clusterer != value)
                {
                    _clusterer = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = PropertyChanged;
            if (propertyChangedHandler != null)
                propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
