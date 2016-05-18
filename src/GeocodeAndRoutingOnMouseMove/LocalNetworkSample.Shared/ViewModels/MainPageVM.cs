using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using Esri.ArcGISRuntime.UI;
using LocalNetworkSample.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
#if NETFX_CORE
using Windows.Storage;
#endif

namespace LocalNetworkSample
{
    public partial class MainPageVM : BaseViewModel
    {
        public MainPageVM()
        {
            AddPointBarrierCommand = new DelegateCommand(() => DrawBarrier<MapPoint>());
            AddPolylineBarrierCommand = new DelegateCommand(() => DrawBarrier<Polyline>());
            AddPolygonBarrierCommand = new DelegateCommand(() => DrawBarrier<Polygon>());
            ClearBarriersCommand = new DelegateCommand(() => ClearBarriers());
        }

        private bool m_UseOnlineService;
        public bool UseOnlineService
        {
            get { return m_UseOnlineService; }
            set
            {
                m_UseOnlineService = value; base.OnPropertyChanged();
                m_locatorTask = null; route = null;
                SelectedCostAttributeName = null;
                m_RouteTaskInfo = null;
                OnPropertyChanged("RouteTaskInfo");
            }
        }

        public bool CalculateShortest { get; set; }

        public void UpdateMouseLocation(MapPoint location)
        {
#if NETFX_CORE
            if(!IsSidePanelOpen || location == null)
                return;
#endif
            if (CurrentTabItem == 0) //geocode
            {
                UpdateGeocode(location);
            }
            else if (CurrentTabItem == 1)
            {
                UpdateRoute(location);
            }
        }

        MapPoint pendingGeocodeLocation;
        private async void UpdateGeocode(MapPoint location)
        {
            //Don't run geocode if one is already running. Instead queue up to increase performance
            bool iscalculating = (pendingGeocodeLocation != null);
            pendingGeocodeLocation = location;
            if (iscalculating)
                return;

            await UpdateGeocodeAsync(location);
            var loc = pendingGeocodeLocation;
            pendingGeocodeLocation = null;
            if (loc != location)
            {
                UpdateGeocode(loc);
            }
        }

        private async Task UpdateGeocodeAsync(MapPoint location)
        {
            var layer = FindGraphicsOverlay("geocode");
            if (layer.Graphics.Count == 0)
                layer.Graphics.Add(new Graphic());
            layer.Graphics[0].Geometry = location;

            if (m_locatorTask == null)
                m_locatorTask = await GetLocator();

            GeocodeResult result = null;
            try
            {
                var p = GeometryEngine.Project(location, m_locatorTask.LocatorInfo.SpatialReference) as MapPoint;
                var param = new ReverseGeocodeParameters() { MaxDistance = 500 };
                param.ResultAttributeNames.Add("*");
                var results = await m_locatorTask.ReverseGeocodeAsync(p, param);
                result = results?.FirstOrDefault();
            }
            catch { }

            if (result != null)
            {
                ReverseGeocodeResult = string.Join("\n", result.Attributes.Values.ToArray());
            }
            else
                ReverseGeocodeResult = "No result";
        }

        private LocatorTask m_locatorTask;

        private string m_reverseGeocode;
        public string ReverseGeocodeResult
        {
            get { return m_reverseGeocode; }
            set { m_reverseGeocode = value; OnPropertyChanged(); }
        }

        private Task<RouteTask> route; //cached version of task to avoid creating it over and over again

        private MapPoint pendingRouteLocation;
        private async void UpdateRoute(MapPoint location)
        {
            // Don't get route if a calculation is already running. Instead queue up to increase performance.
            // This is important because mouse move can happen faster than the route calculation,
            // causing a huge chunk of work to build up. The following approach finishes the current
            // running task, and then calculates the very latest, thus skipping any interim locations
            // that are no longer needed.
            bool iscalculating = (pendingRouteLocation != null);
            pendingRouteLocation = location;
            if (iscalculating)
                return;

            await UpdateRouteAsync(location);
            var loc = pendingRouteLocation;
            pendingRouteLocation = null;
            if (loc != location)
            {
                UpdateRoute(loc);
            }
        }

        private MapPoint m_lastRouteLocation;

        private async Task UpdateRouteAsync(MapPoint location)
        {
            var routeGraphicsOverlay = FindGraphicsOverlay("route");
            try
            {
                var from = new MapPoint(-13042273.8388969, 3856316.68541748, SpatialReferences.WebMercator); //Route from San Diego convention center
                //If you want to route from the current location:
                //if (LocationDisplay.CurrentLocation != null)
                //	from = LocationDisplay.CurrentLocation.Location;
                if (route == null)
                    route = GetRouter();
                var l = route.IsCompleted ? route.Result : await route;
                var param = await l.GenerateDefaultParametersAsync();

                param.SetStops(new Stop[] { new Stop(from), new Stop(location) });
                if (SelectedCostAttributeName != null) //Set what to optimize for
                    param.TravelMode.ImpedanceAttributeName = SelectedCostAttributeName;
                param.ReturnDirections = true;

                //Get barriers
                param.SetPointBarriers(FindGraphicsOverlay("PointBarriers").Graphics.Select(g => new PointBarrier(g.Geometry as MapPoint)));
                param.SetPolylineBarriers(FindGraphicsOverlay("PolylineBarriers").Graphics.Select(g => new PolylineBarrier(g.Geometry as Polyline)));
                param.SetPolygonBarriers(FindGraphicsOverlay("PolygonBarriers").Graphics.Select(g => new PolygonBarrier(g.Geometry as Polygon)));

                var result = await l.SolveRouteAsync(param);
                if (result != null && result.Routes.Any())
                {
                    routeGraphicsOverlay.Graphics.Clear();
                    var r = result.Routes[0];
                    routeGraphicsOverlay.Graphics.Add(new Graphic(r.RouteGeometry));
                    RouteDirections = r.DirectionManeuvers;
                    OnPropertyChanged("RouteDirections");
                }
                m_lastRouteLocation = location;
            }
            catch
            {
                RouteDirections = null;
                OnPropertyChanged("RouteDirections");
                routeGraphicsOverlay.Graphics.Clear();
            }
        }

        RouteTaskInfo m_RouteTaskInfo;
        public RouteTaskInfo RouteTaskInfo
        {
            get
            {
                if (m_RouteTaskInfo == null)
                {
                    LoadNetworkAttributes();
                }
                return m_RouteTaskInfo;
            }
        }

        private async void LoadNetworkAttributes()
        {
            if (route == null)
                route = GetRouter();
            try
            {
                var l = route.IsCompleted ? route.Result : await route;

                m_RouteTaskInfo = l.RouteTaskInfo;
                var param = await l.GenerateDefaultParametersAsync();
                OnPropertyChanged("RouteTaskInfo");
                SelectedCostAttributeName = m_RouteTaskInfo.CostAttributes
                    .Where(ca => ca.Key == param.TravelMode.ImpedanceAttributeName)
                    .Select(ca => ca.Key)
                    .FirstOrDefault();
            }
            catch { /* */ }
        }

        private string m_SelectedCostAttributeName;
        public string SelectedCostAttributeName
        {
            get { return m_SelectedCostAttributeName; }
            set
            {
                m_SelectedCostAttributeName = value; OnPropertyChanged();
                if (m_lastRouteLocation != null)
                    UpdateRoute(m_lastRouteLocation);
            }
        }

        public IReadOnlyList<DirectionManeuver> RouteDirections { get; private set; }

        private async Task<RouteTask> GetRouter()
        {
            if (UseOnlineService)
            {
                return await RouteTask.CreateAsync(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route"));
            }
            try
            {
                BusyMessage = "Loading network...";
#if NETFX_CORE
                string path = "Data\\Networks";
                var foldername = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\" + path;
                var datafolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(foldername);
                var folders = await datafolder.GetFoldersAsync();
                foreach (var folder in folders)
                {
                    var files = await folder.GetFilesAsync();
                    var file = files.Where(f => f.Name.EndsWith(".db")).FirstOrDefault();
                    if (file != null) //Locator found
                    {
                        return await RouteTask.CreateAsync(file.Path, "Streets_ND");
                    }
                }
#else
                string path = "..\\..\\..\\Data\\Networks";
                foreach (var folder in new System.IO.DirectoryInfo(path).GetDirectories())
                {
                    var file = folder.GetFiles("*.db").FirstOrDefault();
                    if (file != null) //Locator found
                    {
                        return await RouteTask.CreateAsync(file.FullName, "Streets_ND");
                    }
                }
#endif
            }
            catch
            {
                throw;
            }
            finally
            {
                BusyMessage = "";
            }
            return null;
        }

        // Searches for a locator in the app directory under \data\locators
        private async Task<LocatorTask> GetLocator()
        {
            if (UseOnlineService)
            {
                return await LocatorTask.CreateAsync(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
            }
            try
            {
#if NETFX_CORE
                BusyMessage = "Loading geocoder...";
                string path = "Data\\Locators";
                var foldername = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\" + path;
                var datafolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(foldername);
                var folders = await datafolder.GetFoldersAsync();
                foreach (var folder in folders)
                {
                    var files = await folder.GetFilesAsync();
                    var file = files.Where(f => f.Name.EndsWith(".loc")).FirstOrDefault();
                    if (file != null)
                    {
                        return await LocatorTask.CreateAsync(new Uri(file.Path));
                    }
                }
#else
                string path = "..\\..\\..\\Data\\Locators";
                foreach (var folder in new System.IO.DirectoryInfo(path).GetDirectories())
                {
                    var file = folder.GetFiles("*.loc").FirstOrDefault();
                    if (file != null) //Locator found
                    {
                        return await LocatorTask.CreateAsync(new Uri(file.FullName));
                    }
                }
#endif
            }
            catch { }
            finally
            {
                BusyMessage = "";
            }
            return null;
        }

        #region MapView Properties
        private GeoView m_geoView;
        public GeoView GeoView
        {
            get { return m_geoView; }
            set
            {
                m_geoView = value;
                OnPropertyChanged();
            }
        }

        private Map m_Map;
        public Map Map
        {
            get
            {
                if (m_Map == null)
                {
                    var extent = new Envelope(-13044883, 3853913, -13039791, 3857887, SpatialReferences.WebMercator);
                    m_Map = new Map(Basemap.CreateImagery()) { InitialViewpoint = new Viewpoint(extent) };
                }
                return m_Map;
            }
            private set
            {
                m_Map = value;
                OnPropertyChanged();
            }
        }

        private GraphicsOverlayCollection m_GraphicsOverlays;
        public GraphicsOverlayCollection GraphicsOverlays
        {
            get { return m_GraphicsOverlays; }
            set { m_GraphicsOverlays = value; OnPropertyChanged(); }
        }

        private LocationDisplay m_LocationDisplay;
        public LocationDisplay LocationDisplay
        {
            get { return m_LocationDisplay; }
            set { m_LocationDisplay = value; OnPropertyChanged(); }
        }
        #endregion MapView Properties

        private int m_CurrentTabItem;
        public int CurrentTabItem
        {
            get { return m_CurrentTabItem; }
            set { m_CurrentTabItem = value; OnPropertyChanged(); }
        }

        #region Barriers
        public ICommand AddPointBarrierCommand { get; private set; }
        public ICommand AddPolylineBarrierCommand { get; private set; }
        public ICommand AddPolygonBarrierCommand { get; private set; }
        public ICommand ClearBarriersCommand { get; private set; }

        private async void DrawBarrier<T>()
        {
            try
            {
                GraphicsOverlay barriers = null;
                Geometry geometry = null;

                if (typeof(T) == typeof(MapPoint))
                {
                    barriers = FindGraphicsOverlay("PointBarriers");
                    geometry = await GeoViewDrawHelper.DrawPointAsync(GeoView, CancellationToken.None);
                }
                else if (typeof(T) == typeof(Polyline))
                {
                    barriers = FindGraphicsOverlay("PolylineBarriers");
                    geometry = await GeoViewDrawHelper.DrawPolylineAsync(GeoView, CancellationToken.None);
                }
                else if (typeof(T) == typeof(Polygon))
                {
                    barriers = FindGraphicsOverlay("PolygonBarriers");
                    geometry = await GeoViewDrawHelper.DrawPolygonAsync(GeoView, CancellationToken.None);
                }

                if (geometry != null)
                    barriers.Graphics.Add(new Graphic(geometry));

                if (m_lastRouteLocation != null)
                    UpdateRoute(m_lastRouteLocation);
            }
            catch (OperationCanceledException) { } //ignore
        }

        private void ClearBarriers()
        {
            var barrierOverlays = new GraphicsOverlay[]
            {
                FindGraphicsOverlay("PointBarriers"),
                FindGraphicsOverlay("PolylineBarriers"),
                FindGraphicsOverlay("PolygonBarriers")
            };
            foreach (var barrierOverlay in barrierOverlays)
                barrierOverlay.Graphics.Clear();
            if (m_lastRouteLocation != null)
                UpdateRoute(m_lastRouteLocation);
        }
        #endregion

        private string m_BusyMessage;
        public string BusyMessage
        {
            get { return m_BusyMessage; }
            private set
            {
                System.Diagnostics.Debug.WriteLine("BusyMessage: " + value);
                m_BusyMessage = value; OnPropertyChanged();
            }
        }

        private GraphicsOverlay FindGraphicsOverlay(string label)
        {
            return GraphicsOverlays.FirstOrDefault(go =>
            {
                var rendererLabel = (go.Renderer as SimpleRenderer)?.Label;
                return string.Equals(rendererLabel, label, StringComparison.CurrentCultureIgnoreCase);
            });
        }
    }
}
