using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using LocalNetworkSample.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
#if NETFX_CORE
using Windows.Storage;
#else
#endif
namespace LocalNetworkSample
{
	public partial class MainPageVM : BaseViewModel
	{
		public MainPageVM()
		{
			LocationDisplay = new LocationDisplay();
			Editor = new Editor();
			AddPointBarrierCommand = new DelegateCommand(() => { DrawBarrier(DrawShape.Point); });
			AddPolylineBarrierCommand = new DelegateCommand(() => { DrawBarrier(DrawShape.Polyline); });
			AddPolygonBarrierCommand = new DelegateCommand(() => { DrawBarrier(DrawShape.Polygon); });
			ClearBarriersCommand = new DelegateCommand(() => { ClearBarriers(); });
		}

		private bool m_UseOnlineService;

		public bool UseOnlineService
		{
			get { return m_UseOnlineService; }
			set
			{
				m_UseOnlineService = value; base.OnPropertyChanged();
				locator = null; route = null;
				SelectedCostAttribute = null;
				m_NetworkDescription = null;
				OnPropertyChanged("NetworkDescription");
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
			else if(CurrentTabItem == 1)
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
			var layer = GraphicsOverlays["geocode"];
			if (layer.Graphics.Count == 0)
				layer.Graphics.Add(new Graphic());
			layer.Graphics[0].Geometry = location;
			if (locator == null)
				locator = GetLocator();
			var l = locator.IsCompleted ? locator.Result : await locator;
			LocatorReverseGeocodeResult result = null;
			try
			{
				var p =  GeometryEngine.Project(location, (await l.GetInfoAsync()).SpatialReference) as MapPoint;
				result = await l.ReverseGeocodeAsync(p, 500, CancellationToken.None);
			}
			catch { }
			if (result != null)
			{
				ReverseGeocodeResult = string.Join("\n", result.AddressFields.Values.ToArray());
			}
			else
				ReverseGeocodeResult = "No result";
		}
		private Task<Esri.ArcGISRuntime.Tasks.Geocoding.LocatorTask> locator;
		private string m_reverseGeocode;

		public string ReverseGeocodeResult
		{
			get { return m_reverseGeocode; }
			set { m_reverseGeocode = value; OnPropertyChanged(); }
		}

		private Task<Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteTask> route; //cached version of task to avoid creating it over and over again
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
			var layer = GraphicsOverlays["route"];
			try
			{
				var from = new MapPoint(-13042273.8388969, 3856316.68541748, SpatialReferences.WebMercator); //Route from San Diego convention center
				//If you want to route from the current location:
				//if (LocationDisplay.CurrentLocation != null)
				//	from = LocationDisplay.CurrentLocation.Location;
				if (route == null)
					route = GetRouter();
				var l = route.IsCompleted ? route.Result : await route;
				var param = await l.GetDefaultParametersAsync();

				param.SetStops(new MapPoint[] { from, location });
				if (SelectedCostAttribute != null) //Set what to optimize for
					param.ImpedanceAttributeName = SelectedCostAttribute.AttributeName;
				param.ReturnDirections = true;
				
				//Get barriers
				param.SetPointBarriers(((GraphicsLayer)Map.Layers["PointBarriers"]).Graphics);
				param.SetPolylineBarriers(((GraphicsLayer)Map.Layers["PolylineBarriers"]).Graphics);
				param.SetPolygonBarriers(((GraphicsLayer)Map.Layers["PolygonBarriers"]).Graphics);


				var result = await l.SolveAsync(param);
				if (result != null && result.Routes.Any())
				{
					layer.Graphics.Clear();
					var r = result.Routes[0];
					layer.Graphics.Add((Graphic)r.RouteFeature);
					RouteDirections = r.RouteDirections;
					OnPropertyChanged("RouteDirections");
				}
				m_lastRouteLocation = location;
			}
			catch {
				RouteDirections = null;
				OnPropertyChanged("RouteDirections");
				layer.Graphics.Clear();
			}
		}
		Esri.ArcGISRuntime.Tasks.NetworkAnalyst.NetworkDescription m_NetworkDescription;
		public Esri.ArcGISRuntime.Tasks.NetworkAnalyst.NetworkDescription NetworkDescription
		{
			get
			{
				if(m_NetworkDescription == null)
				{
					LoadNetworkAttributes();
				}
				return m_NetworkDescription;
				
			}
		}

		private async void LoadNetworkAttributes()
		{
			if (route == null)
				route = GetRouter();
			try
			{
				var l = route.IsCompleted ? route.Result : await route;

				m_NetworkDescription = await l.GetNetworkDescriptionAsync();
				var param = await l.GetDefaultParametersAsync();
				OnPropertyChanged("NetworkDescription");
				SelectedCostAttribute = m_NetworkDescription.CostAttributes.Where(c => c.AttributeName == param.ImpedanceAttributeName).FirstOrDefault();
			}
			catch { /* */ }
		}
		private Esri.ArcGISRuntime.Tasks.NetworkAnalyst.CostAttribute m_SelectedCostAttribute;

		public Esri.ArcGISRuntime.Tasks.NetworkAnalyst.CostAttribute SelectedCostAttribute
		{
			get { return m_SelectedCostAttribute; }
			set
			{
				m_SelectedCostAttribute = value; OnPropertyChanged();
				if (m_lastRouteLocation != null)
					UpdateRoute(m_lastRouteLocation);
			}
		}

		public IReadOnlyList<Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteDirection> RouteDirections { get; private set; }
		

		private async Task<Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteTask> GetRouter()
		{
			if(UseOnlineService)
			{
				return new Esri.ArcGISRuntime.Tasks.NetworkAnalyst.OnlineRouteTask(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route"));
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
						return new Esri.ArcGISRuntime.Tasks.NetworkAnalyst.LocalRouteTask(file.Path, "");
					}
				}
#else
				string path = "..\\..\\..\\Data\\Networks";
				foreach(var folder in new System.IO.DirectoryInfo(path).GetDirectories())
				{
					var file = folder.GetFiles("*.db").FirstOrDefault();
					if (file != null) //Locator found
					{
						return new Esri.ArcGISRuntime.Tasks.NetworkAnalyst.LocalRouteTask(file.FullName, "");
					}
				}				
#endif
			}
			catch {
				throw;
			}
			finally
			{
				BusyMessage = "";
			}
			return null;
		}


		// Searches for a locator in the app directory under \data\locators
		private async Task<Esri.ArcGISRuntime.Tasks.Geocoding.LocatorTask> GetLocator()
		{
			if(UseOnlineService)
			{
				return new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
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
						return new Esri.ArcGISRuntime.Tasks.Geocoding.LocalLocatorTask(file.Path);
					}
				}
#else
				string path = "..\\..\\..\\Data\\Locators";
				foreach(var folder in new System.IO.DirectoryInfo(path).GetDirectories())
				{
					var file = folder.GetFiles("*.loc").FirstOrDefault();
					if (file != null) //Locator found
					{
						return new Esri.ArcGISRuntime.Tasks.Geocoding.LocalLocatorTask(file.FullName);
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

		private Map m_Map;

		public Map Map
		{
			get { return m_Map; }
			set { m_Map = value; OnPropertyChanged(); }
		}

		private GraphicsOverlayCollection m_GraphicsOverlays;

		public GraphicsOverlayCollection GraphicsOverlays
		{
			get { return m_GraphicsOverlays; }
			set { m_GraphicsOverlays = value; OnPropertyChanged(); }
		}
		

		private Editor m_Editor;

		public Editor Editor
		{
			get { return m_Editor; }
			set { m_Editor = value; }
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


		private async void DrawBarrier(DrawShape drawShape)
		{
			try
			{
				var geometry = await Editor.RequestShapeAsync(drawShape);
				if(geometry is MapPoint)
				{
					((GraphicsLayer)Map.Layers["PointBarriers"]).Graphics.Add(new Graphic(geometry));
				}
				else if (geometry is Polyline)
				{
					((GraphicsLayer)Map.Layers["PolylineBarriers"]).Graphics.Add(new Graphic(geometry));
				}
				else if (geometry is Polygon)
				{
					((GraphicsLayer)Map.Layers["PolygonBarriers"]).Graphics.Add(new Graphic(geometry));
				}
				if (m_lastRouteLocation != null)
					UpdateRoute(m_lastRouteLocation);
				
			}
			catch (OperationCanceledException) { } //ignore
		}

		private void ClearBarriers()
		{
			foreach (var layer in ((GroupLayer)Map.Layers["Barriers"]).ChildLayers.OfType<GraphicsLayer>())
				layer.Graphics.Clear();
			if (m_lastRouteLocation != null)
				UpdateRoute(m_lastRouteLocation);
		}

		#endregion

		private string m_BusyMessage;

		public string BusyMessage
		{
			get { return m_BusyMessage; }
			private set {
				System.Diagnostics.Debug.WriteLine("BusyMessage: " +value);
				m_BusyMessage = value; OnPropertyChanged(); }
		}		
	}
}
