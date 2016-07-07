using System;
using Esri.ArcGISRuntime.Mapping;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
#if NETFX_CORE
using Windows.UI;
#endif
#if __ANDROID__ || __IOS__
using System.Drawing;
using Colors = System.Drawing.Color;
#endif
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Geometry;
using System.Runtime.CompilerServices;

namespace OfficeLocator
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public MapViewModel()
		{
			Overlays.Add(new GraphicsOverlay() { Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.CornflowerBlue, 5)) });
            Overlays[0].SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
			Overlays.Add(new GraphicsOverlay() { Renderer = new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Red, 15)) });
            Overlays[1].SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
		}

        /// <summary>
        /// Initiates offline data provisioning on first run
        /// </summary>
        /// <returns></returns>
        private async Task ProvisionDataAsync()
        {
            await ProvisionDataHelper.GetData(GetDataFolder(), (status) =>
            {
                UpdateLoadStatus(status);
            });
            UpdateLoadStatus("");
        }

        /// <summary>
        /// Initializes and if necessary provisions the data needed for the application
        /// </summary>
        /// <returns></returns>
        public async Task LoadAsync()
        {
            await ProvisionDataAsync();
            UpdateLoadStatus("Initializing map...");
            GeocodeHelper.Initialize();
#if NETFX_CORE
			var a = await PictureMarkerSymbol.CreateAsync(new Uri("ms-appx:///MarkerA.png"));
			var b = await PictureMarkerSymbol.CreateAsync(new Uri("ms-appx:///MarkerB.png"));
			a.Width = a.Height = b.Width = b.Height = 20;
#else
            var a = new SimpleMarkerSymbol() { Color = Colors.Red, Size = 20 };
            var b = new SimpleMarkerSymbol() { Color = Colors.Green, Size = 20 };
#endif
            Overlays[1].Graphics.Add(new Graphic() { Symbol = a }); //From
            Overlays[1].Graphics.Add(new Graphic() { Symbol = b }); //To
            try
            {
                await InitializeMap();
                InitializeScene();                
                IsLoaded = true;
                UpdateLoadStatus("");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoaded)));
                DoStartupZoomIn();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An unexpected error occured while loading the map: {ex.Message}");
                UpdateLoadStatus(ex.Message);
            }
        }

        /// <summary>
        /// Initializes a 2D Map of the campus using the locally stored data
        /// This map works offline, but if online, is also supplemented by a world map for better reference
        /// </summary>
        /// <returns></returns>
        private async Task InitializeMap()
        {
            Basemap basemap = Basemap.CreateTopographic();
            var layer = new ArcGISVectorTiledLayer(new Uri(Path.Combine(GetDataFolder(), "Basemap/CampusBasemap.vtpk"), UriKind.RelativeOrAbsolute));
            await layer.LoadAsync();
            basemap.BaseLayers.Add(layer);

            Map = new Map(SpatialReferences.WebMercator)
            {
                Basemap = basemap,
                InitialViewpoint = new Viewpoint(new MapPoint(-13046209, 4036456, SpatialReferences.WebMercator), 30000),
                MaxScale = 160,
                MinScale = 30000
            };

#if __ANDROID__ //workaround for current maxscale bug 
            Map.MaxScale = 300;
#endif
            await Map.LoadAsync();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Map)));
        }

        /// <summary>
        /// Creates a 3D Scene if the application is using the 3D Scene View instead
        /// In the view try changing "MapView" to "SceneView" to get a 3D view.
        /// The 3D Scene requires an online connection.
        /// </summary>
        private void InitializeScene()
        {
            //Create scene for use in a 3D environment
            Scene = new Scene()
            {
                Basemap = Basemap.CreateTopographic(),
                BaseSurface = new Surface(),
                InitialViewpoint = new Viewpoint(new MapPoint(-13046209, 4036456, SpatialReferences.WebMercator), 30000),
            };
            Scene.OperationalLayers.Add(new ArcGISSceneLayer(new Uri("http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Building_Wireframes/SceneServer/layers/0"))); //Building outlines
            Scene.OperationalLayers.Add(new ArcGISSceneLayer(new Uri("http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/BuildingInteriorSpace_ET_1/SceneServer/layers/0"))); //1st floor
            Scene.OperationalLayers.Add(new ArcGISSceneLayer(new Uri("http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/BuildingInteriorSpace_ET_2/SceneServer/layers/0")) { Opacity = .5 }); //2nd floor
            Scene.OperationalLayers.Add(new ArcGISSceneLayer(new Uri("http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/BuildingInteriorSpace_ET_3/SceneServer/layers/0")) { Opacity = .5 }); //3rd floor
            Scene.OperationalLayers.Add(new ArcGISSceneLayer(new Uri("http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Building_Textured/SceneServer/layers/0")) { Opacity = .35 }); //Facades                                                                                                                                                                                                       //Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scene)));
        }

        private void UpdateLoadStatus(string status)
        {
            LoadStatus = status;
            OnPropertyChanged(nameof(LoadStatus));
        }

        private async void DoStartupZoomIn()
        {
            await Task.Delay(1000);
            RequestViewpoint?.Invoke(this, new Viewpoint(new MapPoint(-13046209, 4036456, SpatialReferences.WebMercator), 3000));
        }

        /// <summary>
        /// Gets a value indicating whether the view model has fully loaded
        /// </summary>
		public bool IsLoaded { get; private set; }

        /// <summary>
        /// A 2D Map of the campus
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        /// A 3D Scene of the campus
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// A collection of overlays for displaying route and from/to locations
        /// </summary>
		public GraphicsOverlayCollection Overlays { get; } = new GraphicsOverlayCollection();
		
        /// <summary>
        /// Gets the current data loading status while provisioning data
        /// </summary>
        public string LoadStatus { get; private set; }
        
        public async void GeocodeFromLocation(string text)
        {
            if (IsBusy) return;

            IsBusy = true;
            Overlays[1].Graphics[0].Geometry = await GeocodeHelper.GeocodeAsync(text);
            CalculateRoute();
            IsBusy = false;
            ZoomToData();
        }

        public async void GeocodeToLocation(string text)
        {
            if (IsBusy) return;

            IsBusy = true;
            Overlays[1].Graphics[1].Geometry = await GeocodeHelper.GeocodeAsync(text);
            CalculateRoute();
            IsBusy = false;
            ZoomToData();
        }

        private async void CalculateRoute()
		{
			Overlays[0].Graphics.Clear();
			HasRoute = false;
			OnPropertyChanged(nameof(HasRoute));

			if (Overlays[1].Graphics[0].Geometry == null ||
				Overlays[1].Graphics[1].Geometry == null)
			{
				return; //We don't have both to and from destination, so return
			}

			var t1 = RouteHelper.RouteAsync(Overlays[1].Graphics[0].Geometry as MapPoint,
								   Overlays[1].Graphics[1].Geometry as MapPoint, false);
			var t2 = RouteHelper.RouteAsync(Overlays[1].Graphics[0].Geometry as MapPoint,
								   Overlays[1].Graphics[1].Geometry as MapPoint, true);
			var route1 = await t1;
			if (route1 != null)
			{
				Overlays[0].Graphics.Add(new Graphic(route1.RouteGeometry));
				WalkTime = route1.TotalTime.ToString("m\\:ss");
				WalkDistance = route1.TotalLength.ToString("0") + " m";
				OnPropertyChanged(nameof(WalkTime));
				OnPropertyChanged(nameof(WalkDistance));
			}
			var route2 = await t2;
			if (route2 != null)
			{
				Overlays[0].Graphics.Insert(0, new Graphic(route2.RouteGeometry) { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Black, 2) });
                WalkTimeAlt = route2.TotalTime.ToString("m\\:ss");
				WalkDistanceAlt = route1.TotalLength.ToString("0") + " m";
				OnPropertyChanged(nameof(WalkTimeAlt));
				OnPropertyChanged(nameof(WalkDistanceAlt));
			}

			HasRoute = route1 != null;
			OnPropertyChanged(nameof(HasRoute));
			OnPropertyChanged(nameof(NoRoute));
            if (HasRoute)
            {
                ZoomToData();
            }
            RouteUpdated?.Invoke(this, EventArgs.Empty);
		}

        private void ZoomToData()
        {
            if (HasRoute)
            {
                //Zoom to the route with a bit of a buffer around it
                RequestViewpoint?.Invoke(this, new Viewpoint(GeometryEngine.Buffer(Overlays[0].Extent, Math.Max(Overlays[0].Extent.Width, Overlays[0].Extent.Height) * .5)));
            }
            else if (Overlays[1].Graphics[0].Geometry != null && Overlays[1].Graphics[1].Geometry != null)
            {
                //Zoom to the two geocoded locations
                RequestViewpoint?.Invoke(this, new Viewpoint(GeometryEngine.Buffer(Overlays[1].Extent, Math.Max(Overlays[1].Extent.Width, Overlays[1].Extent.Height) * .5)));
            }
            else if (Overlays[1].Graphics[0].Geometry != null)
            {
                //Zoom to from-location
                RequestViewpoint?.Invoke(this, new Viewpoint(Overlays[1].Graphics[0].Geometry as MapPoint, 400));
            }
            else if (Overlays[1].Graphics[1].Geometry != null)
            {
                //Zoom to to-location
                RequestViewpoint?.Invoke(this, new Viewpoint(Overlays[1].Graphics[1].Geometry as MapPoint, 400));
            }
        }

        /// <summary>
        /// Formatted walk time for shortest route
        /// </summary>
		public string WalkTime { get; private set; }

        /// <summary>
        /// Formatted walk distance for shortest route
        /// </summary>
        public string WalkDistance { get; private set; }

        /// <summary>
        /// Formatted walk time for route that favors indoors over outdoors
        /// </summary>
        public string WalkTimeAlt { get; private set; }

        /// <summary>
        /// Formatted walk distance for route that favors indoors over outdoors
        /// </summary>
        public string WalkDistanceAlt { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a route is available
        /// </summary>
		public bool HasRoute { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a route is not available
        /// </summary>
		public bool NoRoute { get { return !HasRoute; } }

		private bool _isBusy;
        /// <summary>
        /// Gets a value indicating whether geocode or route calculation is currently processing
        /// </summary>
		public bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				_isBusy = value;
				OnPropertyChanged();
			}
		}

        /// <summary>
        /// Event fired when a new route calculation has completed
        /// </summary>
		public event EventHandler RouteUpdated;

        /// <summary>
        /// Raises an event that instructs the geoview to change its viewpoint
        /// </summary>
		public event EventHandler<Viewpoint> RequestViewpoint;

        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
#if NETFX_CORE
            if (!Dispatcher.HasThreadAccess)
            {
                var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => OnPropertyChanged(propertyName));
            }
            else
#endif
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets the data folder where locally provisioned data is stored
        /// </summary>
        /// <returns></returns>
        internal static string GetDataFolder()
        {
            var appDataFolder =
#if NETFX_CORE
                Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif __ANDROID__
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
#elif __IOS__
                "Library/";
#endif
            return Path.Combine(appDataFolder, "CampusData");
        }


#if NETFX_CORE
        private Windows.UI.Core.CoreDispatcher Dispatcher = Windows.UI.Xaml.Application.Current.Resources.Dispatcher;
#endif
    }
}
