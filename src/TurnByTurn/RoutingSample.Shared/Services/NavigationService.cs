using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Security;
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
        private const string LocatorService = "https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer";
		private const string RouteService = "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World";

        private static readonly Uri s_locatorService = new Uri(LocatorService);
        private static readonly Uri s_routeService = new Uri(RouteService);

        /// <summary>
        /// Returns the location of the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static async Task<MapPoint> GeocodeAsync(string address)
        {
            var locator = await LocatorTask.CreateAsync(s_locatorService);
            var result = await locator.GeocodeAsync(address).ConfigureAwait(false);
            return result?.FirstOrDefault()?.RouteLocation;
        }

        /// <summary>
        /// Returns a route between the specified stops.
        /// </summary>
        /// <returns></returns>
        public static async Task<SolveRouteResult> SolveRouteAsync(MapPoint from, MapPoint to)
        {
            RouteTask routeTask = null;
            RouteParameters routeParameters = null;
            RouteResult routeResult = null;

            try
            {
                // Create a new route using the onling routing service
                routeTask = await RouteTask.CreateAsync(s_routeService);

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
            catch (ArcGISWebException ex)
            {
                // Any error other than failure to locate is rethrown
                if (ex.Details.FirstOrDefault()?.Contains("Unlocated") != true)
                {
                    throw ex;
                }
            }

            return new SolveRouteResult { Route = routeResult, Task = routeTask, Parameters = routeParameters };
        }
    }
}
