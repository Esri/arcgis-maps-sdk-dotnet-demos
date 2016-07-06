using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Security;

namespace RoutingSample.Models
{
	/// <summary>
	/// Helper service that handles geocoding and routing
	/// </summary>
	public class RouteService
	{
		private const string locatorService = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";
		private const string routeService = "http://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World";
        
        private readonly Credential credential;
		public RouteService()
        {
            credential = AuthenticationManager.Current.FindCredential(new Uri("http://www.arcgis.com/sharing/rest"));
        }

		public async Task<RouteResult> GetRoute(string address, MapPoint from, CancellationToken cancellationToken)
		{
			var to = await Geocode(address, cancellationToken).ConfigureAwait(false);
			if (to == null)
				throw new ArgumentException("Address not found");
			return await GetRoute(from, to, cancellationToken);
		}

		public async Task<MapPoint> Geocode(string address, CancellationToken cancellationToken)
		{
            LocatorTask locator = await LocatorTask.CreateAsync(new Uri(locatorService));
            var result = await locator.GeocodeAsync(address).ConfigureAwait(false);
            if (result != null && result.Count > 0)
				return result.First().RouteLocation as MapPoint;
			return null;
		}

		public Task<RouteResult> GetRoute(MapPoint from, MapPoint to, CancellationToken cancellationToken)
		{
			return GetRoute(new MapPoint[] { from, to }, cancellationToken);
		}

		public async Task<RouteResult> GetRoute(IEnumerable<MapPoint> stops, CancellationToken cancellationToken)
		{
			if (stops == null)
				throw new ArgumentNullException("stops");

			List<Stop> stopList = new List<Stop>();
			foreach (var stop in stops)
			{
				stopList.Add(new Stop(stop));
			}
			if (stopList.Count < 2)
				throw new ArgumentException("Not enough stops");
            
			string svc = routeService;

			//Calculate route
			RouteTask task = await RouteTask.CreateAsync(new Uri(svc), credential).ConfigureAwait(false);
            
			var parameters = await task.GenerateDefaultParametersAsync().ConfigureAwait(false);
            parameters.SetStops(stopList);
			parameters.ReturnStops = true;
            parameters.ReturnDirections = true; 
            parameters.RouteShapeType = RouteShapeType.TrueShapeWithMeasures;
			parameters.OutputSpatialReference = SpatialReferences.Wgs84;
			parameters.DirectionsDistanceUnits = DirectionsDistanceTextUnits.Metric;
            parameters.LocalStartTime = DateTime.Now;
            return await task.SolveRouteAsync(parameters);
		}
    }
}
