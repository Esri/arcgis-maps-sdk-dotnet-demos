#define USE_DATA_MANAGER
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoutingSample.Services
{
    public struct SolveRouteResult
    {
        /// <summary>
        /// The route result.
        /// </summary>
        public RouteResult Route;

        /// <summary>
        /// The route task.
        /// </summary>
        public RouteTask Task;

        /// <summary>
        /// The route parameters.
        /// </summary>
        public RouteParameters Parameters;
    }

    public class NavigationService
    {
#if USE_DATA_MANAGER
        private const string Database = "sandiego.geodatabase";
        private const string NetworkName = "Streets_ND";
#else
        private const string LocatorService = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";
		private const string RouteService = "http://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World";

        private static readonly Uri s_locatorService = new Uri(LocatorService);
        private static readonly Uri s_routeService = new Uri(RouteService);
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        public NavigationService()
        {

        }

#if !USE_DATA_MANAGER
        /// <summary>
        /// Returns the location of the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MapPoint> Geocode(string address, CancellationToken cancellationToken)
        {
            var locator = await LocatorTask.CreateAsync(s_locatorService);
            var result = await locator.GeocodeAsync(address, cancellationToken).ConfigureAwait(false);
            return result?.FirstOrDefault()?.RouteLocation;
        }
#endif

        /// <summary>
        /// Returns a route between the specified stops.
        /// </summary>
        /// <param name="stops">The stops on the route.</param>
        /// <returns></returns>
        public async Task<SolveRouteResult> SolveRouteAsync(MapPoint from, MapPoint to)
        {
            RouteTask routeTask = null;
            RouteParameters routeParameters = null;
            RouteResult routeResult = null;

            try
            {
                // Ensure the sample data is ready to go
                await DataManager.EnsureDataPresent();

#if USE_DATA_MANAGER
                // Create a new route using the offline database
                routeTask = await RouteTask.CreateAsync(DataManager.GetDataFolder(Database), NetworkName);
#else
                // Create a new route using the onling routing service
                routeTask = await RouteTask.CreateAsync(s_routeService);
#endif

                // Configure the route and set the stops
                routeParameters = await routeTask.CreateDefaultParametersAsync();
                routeParameters.SetStops(new[] { new Stop(from), new Stop(to) });
                routeParameters.ReturnStops = true;
                routeParameters.ReturnDirections = true;
                routeParameters.RouteShapeType = RouteShapeType.TrueShapeWithMeasures;
                routeParameters.OutputSpatialReference = SpatialReferences.Wgs84;
                routeParameters.DirectionsDistanceUnits = UnitSystem.Metric;
                routeParameters.StartTime = DateTime.UtcNow;

                // Solve the route
                routeResult = await routeTask.SolveRouteAsync(routeParameters);
            }
            catch (Exception ex)
            {
                if (!(ex is ArcGISWebException web && web.Details.FirstOrDefault()?.Contains("Unlocated") == true))
                {
                    // There was some other error.
                    throw;
                }
            }

            return new SolveRouteResult { Route = routeResult, Task = routeTask, Parameters = routeParameters };
        }
    }
}
